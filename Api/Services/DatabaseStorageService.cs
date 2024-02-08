using Api.Database;
using Api.Exceptions;
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

        private readonly ILogger<DatabaseStorageService> Logger;

        public DatabaseStorageService(
            GalleryContext dbContext,
            IOptions<FileSystemOptions> fileSystemOptions,
            ILogger<DatabaseStorageService> logger)
        {
            DbContext = dbContext;
            FileSystemOptions = fileSystemOptions.Value;
            Logger = logger;
        }

        public ItemInfo? GetItem(long id)
        {
            var item = DbContext
                .FileSystemItems
                .SingleOrDefault(x => x.Id == id);

            if (item == null)
            {
                Logger.LogWarning("Item with id {ItemId} was not found", id);
                return null;
            }

            if (item.IsFolder)
            {
                return new FolderItemInfo
                {
                    Id = item.Id,
                    Name = item.Name,
                    CreationDate = DateTimeUtils.FromUnixTimestamp(item.CreationDate),
                    UpdatedAtDate = item.UpdatedAtDate,
                };
            }

            return new FileItemInfo
            {
                Id = item.Id,
                Name = item.Name,
                CreationDate = DateTimeUtils.FromUnixTimestamp(item.CreationDate),
                UpdatedAtDate = item.UpdatedAtDate,
                Width = item.Width.Value,
                Height = item.Height.Value,
                Extension = item.Extension,
            };
        }

        public IEnumerable<FileItemInfo> GetFileItems(long? folderId, int skip, int take, string[]? extensions)
        {
            IQueryable<FileSystemItem> items;

            items = DbContext
                .FileSystemItems;

            if (folderId.HasValue)
            {
                items = items.Where(x => x.ParentId == folderId);
            }

            // TODO: Should we check whether the folder itself exist?
            items = items
                .Where(x => !x.IsFolder)
                .OrderBy(x => x.ParentId)
                .ThenBy(x => x.CreationDate)
                .Skip(skip)
                .Take(take);

            var result = new List<FileItemInfo>();
            foreach (var item in items)
            {
                if (extensions?.Length > 0)
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
                    UpdatedAtDate = item.UpdatedAtDate,
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

            items = DbContext.FileSystemItems;

            if (!folderId.HasValue)
            {
                items = items.Where(x => x.ParentId == null);
            }
            else
            {
                items = items.Where(x => x.ParentId == folderId);
            }

            // TODO: Should we query for the folder first and return error if there's no folder with given Id?
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
                    UpdatedAtDate = item.UpdatedAtDate,
                });
            }

            return result;
        }

        public IEnumerable<FolderItemInfo>? GetFolderAncestors(long folderId)
        {
            var ansectors = new List<FileSystemItem>();
            var currentFolder = DbContext
                .FileSystemItems
                .SingleOrDefault(x => x.Id == folderId && x.IsFolder);

            if (currentFolder == null)
            {
                Logger.LogWarning("Folder with id {FolderId} was not found", folderId);
                return null;
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
                    UpdatedAtDate = x.UpdatedAtDate,
                };
            });
        }

        public CollectionMetadata GetCollectionMetadata(long? rootId)
        {
            var result = new CollectionMetadata()
            {
                RootId = rootId,
                ItemsPerMonth = new Dictionary<DateTime, int>()
            };

            using var connection = DbContext.Database.GetDbConnection();
            connection.Open();

            // TODO: Refactor this, or at least add params properly
            var command = connection.CreateCommand();
            command.CommandText = $"" +
                $"SELECT count(id) as num, dateTime(CreationDate, 'unixepoch', 'start of day') as d " +
                $"FROM FileSystemItems " +
                $"WHERE isFolder = 0 ";

            if (rootId.HasValue)
            {
                command.CommandText += $"AND parentId = {rootId} ";
            }
            command.CommandText += $"GROUP BY d";

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

        public FileItemData? GetImage(long id)
        {
            var fileItem = DbContext
                .FileSystemItems
                .SingleOrDefault(x => x.Id == id && x.IsFolder == false);

            if (fileItem == null)
            {
                Logger.LogWarning("Image with id {ImageId} was not found", id);
                return null;
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
                    UpdatedAtDate = fileItem.UpdatedAtDate,
                    Extension = fileItem.Extension,
                    Width = fileItem.Width.Value,
                    Height = fileItem.Height.Value,
                },
                Data = stream
            };

            return info;
        }
    }
}