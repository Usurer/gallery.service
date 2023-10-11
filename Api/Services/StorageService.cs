using Api.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Api.Services
{
    public interface IStorageService
    {
        public IList<IFileOrFolderInfo> GetFileSystemItems(string? root, int take);
    }

    public class StorageService : IStorageService
    {
        private readonly GalleryContext DbContext;

        private readonly FileSystemOptions FileSystemOptions;

        public StorageService(GalleryContext dbContext, IOptions<FileSystemOptions> fileSystemOptions)
        {
            DbContext = dbContext;
            FileSystemOptions = fileSystemOptions.Value;
        }

        public IList<IFileOrFolderInfo> GetFileSystemItems(string? root, int take)
        {
            IQueryable<FileSystemItem> items;

            if (string.IsNullOrWhiteSpace(root))
            {
                root = FileSystemOptions.DefaultFolder;
            }

            // Mind that we can't use StringComparison.Ordinal here and rely on Db collation
            long parentId =
                DbContext.FileSystemItems
                    .Where(x => string.Equals(x.Path, root))
                    .SingleOrDefault()
                    ?.Id
                ?? throw new ApplicationException($"There is no record for a folder with a {root} path");

            items = DbContext.FileSystemItems.Where(x => x.ParentId == parentId).Take(take).AsNoTracking();

            var result = new List<IFileOrFolderInfo>();
            foreach (var item in items)
            {
                if (item.IsFolder)
                {
                    result.Add(new FolderInfo { Name = item.Name, Id = item.Id });
                }
                else
                {
                    result.Add(new FileInfo { Name = item.Name, Id = item.Id });
                }
            }

            return result;
        }
    }
}