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

        public IList<IItemInfo> GetItems(long? rootId, int skip, int take)
        {
            IQueryable<FileSystemItem> items;

            if (!rootId.HasValue)
            {
                rootId = GetDefaultRoot(rootId);
            }

            items = DbContext
                .FileSystemItems
                .Where(x => x.ParentId == rootId)
                .OrderBy(x => x.CreationDate)
                .Skip(skip)
                .Take(take);

            var result = new List<IItemInfo>();
            foreach (var item in items)
            {
                if (item.IsFolder)
                {
                    result.Add(new FolderItemInfo
                    {
                        Id = item.Id,
                        Name = item.Name,
                        CreationDate = DateTimeUtils.FromUnixTimestamp(item.CreationDate),
                    });
                }
                else
                {
                    result.Add(new FileItemInfo
                    {
                        Id = item.Id,
                        Name = item.Name,
                        CreationDate = DateTimeUtils.FromUnixTimestamp(item.CreationDate)
                    });
                }
            }

            return result;
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

            // TODO: Refactor this
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
                    Extension = fileItem.Extension!
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