using Api.Database;
using Api.Services.DTO;
using Api.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Api.Services
{
    public class DatabaseStorageService : IStorageService
    {
        private readonly GalleryContext DbContext;

        private readonly FileSystemOptions FileSystemOptions;

        public DatabaseStorageService(GalleryContext dbContext, IOptions<FileSystemOptions> fileSystemOptions)
        {
            DbContext = dbContext;
            FileSystemOptions = fileSystemOptions.Value;
        }

        public ItemInfo GetItem(long id)
        {
            // TODO: Handle exception, return error
            var item = DbContext
                .FileSystemItems
                .SingleOrDefault(x => x.Id == id);

            if (item.IsFolder)
            {
                return new FolderItemInfo
                {
                    Id = item.Id,
                    Name = item.Name,
                    CreationDate = DateTimeUtils.FromUnixTimestamp(item.CreationDate),
                };
            }

            return new FileItemInfo
            {
                Id = item.Id,
                Name = item.Name,
                CreationDate = DateTimeUtils.FromUnixTimestamp(item.CreationDate),
                Width = item.Width.Value,
                Height = item.Height.Value,
                Extension = item.Extension,
            };
        }

        public IEnumerable<FileItemInfo> GetFileItems(long? folderId, int skip, int take, string[]? extensions)
        {
            IQueryable<FileSystemItem> items;

            if (!folderId.HasValue)
            {
                folderId = GetDefaultRoot(folderId);
            }

            items = DbContext
                .FileSystemItems
                .Where(x => x.ParentId == folderId)
                .Where(x => !x.IsFolder)
                .OrderBy(x => x.CreationDate)
                .Skip(skip)
                .Take(take);

            var result = new List<FileItemInfo>();
            foreach (var item in items)
            {
                if (extensions != null)
                {
                    if (!extensions.Contains(item.Extension, StringComparer.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                }

                if (item.Name.EndsWith(".MP4", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                result.Add(new FileItemInfo
                {
                    Id = item.Id,
                    Name = item.Name,
                    CreationDate = DateTimeUtils.FromUnixTimestamp(item.CreationDate),
                    Width = item.Width.Value,
                    Height = item.Height.Value,
                    Extension = item.Extension,
                });
            }

            return result;
        }

        public IEnumerable<FolderItemInfo> GetFolderItems(long? folderId, int skip, int take)
        {
            IQueryable<FileSystemItem> items;

            if (!folderId.HasValue)
            {
                folderId = GetDefaultRoot(folderId);
            }

            items = DbContext.FileSystemItems;

            if (!folderId.HasValue)
            {
                items = items.Where(x => x.ParentId == null);
            }
            else
            {
                items = items.Where(x => x.ParentId == folderId);
            }

            items = items
                .Where(x => x.IsFolder)
                .OrderBy(x => x.CreationDate)
                .Skip(skip)
                .Take(take);

            var result = new List<FolderItemInfo>();
            foreach (var item in items)
            {
                result.Add(new FolderItemInfo
                {
                    Id = item.Id,
                    Name = item.Name,
                    CreationDate = DateTimeUtils.FromUnixTimestamp(item.CreationDate),
                });
            }

            return result;
        }

        public IEnumerable<FolderItemInfo> GetFolderAncestors(long folderId)
        {
            var ansectors = new List<FileSystemItem>();
            var currentFolder = DbContext
                .FileSystemItems
                .SingleOrDefault(x => x.Id == folderId && x.IsFolder);

            if (currentFolder == null)
            {
                // TODO: Log?
                return Enumerable.Empty<FolderItemInfo>();
            }

            ansectors.Add(currentFolder);

            var parent = DbContext
                .FileSystemItems
                .SingleOrDefault(x => x.Id == currentFolder.ParentId && x.IsFolder);

            while (parent != null)
            {
                ansectors.Add(parent);
                parent = DbContext
                    .FileSystemItems
                    .SingleOrDefault(x => x.Id == parent.ParentId && x.IsFolder);
            }

            return ansectors.Select(x =>
            {
                return new FolderItemInfo
                {
                    Id = x.Id,
                    Name = x.Name,
                    CreationDate = DateTimeUtils.FromUnixTimestamp(x.CreationDate),
                };
            });
        }

        public CollectionMetadata GetCollectionMetadata(long? rootId)
        {
            if (!rootId.HasValue)
            {
                rootId = GetDefaultRoot(rootId);
            }

            var result = new CollectionMetadata()
            {
                RootId = rootId.Value,
                ItemsPerMonth = new Dictionary<DateTime, int>()
            };

            using var connection = DbContext.Database.GetDbConnection();
            connection.Open();

            // TODO: Refactor this, or at least add params properly
            var command = connection.CreateCommand();
            command.CommandText = $"" +
                $"SELECT count(id) as num, dateTime(CreationDate, 'unixepoch', 'start of day') as d " +
                $"FROM FileSystemItems " +
                $"WHERE isFolder = 0 " +
                $"AND parentId = {rootId} " +
                $"Group By d";

            using var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var count = reader.GetInt32(0);
                    var dateTimeString = reader.GetString(1);
                    // TODO: String format
                    var dateTime = DateTime.Parse(dateTimeString);
                    result.ItemsCount += count;
                    result.ItemsPerMonth.Add(dateTime, count);
                }
            }

            reader.Close();
            connection.Close();

            return result;
        }

        public FileItemData GetImage(long id)
        {
            var fileItem = DbContext
                .FileSystemItems
                .SingleOrDefault(x => x.Id == id && x.IsFolder == false);

            if (fileItem == null)
            {
                throw new ArgumentException("Id not found", nameof(id));
            }

            /* From time to time I keep getting the System.IO.IOException:
             * The process cannot access the file because it is being used by another process.
             * Maybe I should just copy FileStream to a MemoryStream and release the file handler.
             * TODO: Fix this
             */
            var stream = new FileStream(fileItem.Path, FileMode.Open);
            var info = new FileItemData
            {
                Info = new FileItemInfo
                {
                    Id = fileItem.Id,
                    Name = fileItem.Name,
                    CreationDate = new DateTime(fileItem.CreationDate),
                    Extension = fileItem.Extension,
                    Width = fileItem.Width.Value,
                    Height = fileItem.Height.Value,
                },
                Data = stream
            };

            return info;
        }

        /// <exception cref="ArgumentException"></exception>
        private long GetDefaultRoot(long? rootId)
        {
            // Mind that we can't use StringComparison.Ordinal here and rely on Db collation
            rootId = DbContext.FileSystemItems
                .Where(x => string.Equals(x.Path, FileSystemOptions.DefaultFolder))
                .SingleOrDefault()
                ?.Id
            ?? throw new ArgumentException($"There is no record for the defaul folder");
            return rootId.Value;
        }
    }
}