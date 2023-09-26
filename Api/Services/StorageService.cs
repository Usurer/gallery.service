using Api.DTO;
using Microsoft.Extensions.Options;

namespace Api.Services
{
    public interface IStorageService
    {
        public IList<IFileSystemItem> GetFileSystemItems(string? root);
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

        public IList<IFileSystemItem> GetFileSystemItems(string? root)
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

            items = DbContext.FileSystemItems.Where(x => x.ParentId == parentId);

            var result = new List<IFileSystemItem>();
            foreach (var item in items)
            {
                if (item.IsFolder)
                {
                    result.Add(new FolderItem { FullName = item.Name, Path = item.Path });
                }
                else
                {
                    result.Add(new FileItem { FullName = item.Name, Path = item.Path });
                }
            }

            return result;
        }
    }
}