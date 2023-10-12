using Api.Database;
using Api.Services.DTO;
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

        public IList<IItemInfo> GetItems(long? rootId, int take)
        {
            IQueryable<FileSystemItem> items;

            if (!rootId.HasValue)
            {
                // Mind that we can't use StringComparison.Ordinal here and rely on Db collation
                rootId = DbContext.FileSystemItems
                    .Where(x => string.Equals(x.Path, FileSystemOptions.DefaultFolder))
                    .SingleOrDefault()
                    ?.Id
                ?? throw new ApplicationException($"There is no record for a folder with a {rootId} id, nor for the defaul folder");
            }

            items = DbContext
                .FileSystemItems
                .Where(x => x.ParentId == rootId)
                .Take(take)
                .AsNoTracking();

            var result = new List<IItemInfo>();
            foreach (var item in items)
            {
                if (item.IsFolder)
                {
                    result.Add(new FolderItemInfo { Name = item.Name, Id = item.Id });
                }
                else
                {
                    result.Add(new FileItemInfo { Name = item.Name, Id = item.Id });
                }
            }

            return result;
        }

        public FileItemInfo GetImage(long id)
        {
            var fileItem = DbContext
                .FileSystemItems
                .SingleOrDefault(x => x.Id == id && x.IsFolder == false);

            if (fileItem == null)
            {
                throw new ArgumentException("Id not found", nameof(id));
            }

            var stream = new FileStream(fileItem.Path, FileMode.Open);
            var info = new FileItemInfo
            {
                Id = fileItem.Id,
                Name = fileItem.Name,
                Extension = fileItem.Extension,
                Data = stream
            };

            return info;
        }
    }
}