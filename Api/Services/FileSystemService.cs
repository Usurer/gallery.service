using Api.DTO;
using Microsoft.Extensions.Options;

namespace Api.Services
{
    public interface IFileSystemService
    {
        public IEnumerable<IFileSystemItem> ScanFolder(string? fullPath, bool recurse = false);
    }

    public class FileSystemService : IFileSystemService
    {
        private readonly FileSystemOptions FileSystemOptions;

        private readonly GalleryContext DbContext;

        public FileSystemService(IOptions<FileSystemOptions> options, GalleryContext context)
        {
            FileSystemOptions = options.Value;
            DbContext = context;
        }

        public IEnumerable<IFileSystemItem> ScanFolder(string? fullPath, bool recurse = false)
        {
            var path = string.IsNullOrEmpty(fullPath) ? FileSystemOptions.DefaultFolder : fullPath;

            if (Directory.Exists(path))
            {
                return new DirectoryInfo(path)
                    .EnumerateFileSystemInfos()
                    .Select<FileSystemInfo, IFileSystemItem>(x =>
                        {
                            var isDirectory = x.Attributes.HasFlag(FileAttributes.Directory);
                            if (isDirectory)
                            {
                                var folderItem = new FolderItem() { Path = x.FullName, FullName = x.Name };
                                var dbFolder = new FileSystemItem { Path = x.FullName, Name = x.Name, IsFolder = true };
                                DbContext.FileSystemItems.Add(dbFolder);
                                //DbContext.SaveChanges();
                                return folderItem;
                            }

                            var fileItem = new FileItem() { Path = x.FullName, FullName = x.Name };
                            var dbFile = new FileSystemItem { Path = x.FullName, Name = x.Name, IsFolder = false };
                            DbContext.FileSystemItems.Add(dbFile);
                            return fileItem;
                        }
                    );
            }

            return Enumerable.Empty<IFileSystemItem>();
        }
    }
}