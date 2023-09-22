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
                var batchSize = 100;
                var counter = 0;

                var fileSystemInfos = new DirectoryInfo(path).EnumerateFileSystemInfos();

                var batch = fileSystemInfos.Skip(counter * batchSize).Take(batchSize).ToArray();

                while (batch.Length > 0)
                {
                    var buffer = new List<IFileSystemItem>();
                    for (int i = 0; i < batch.Length; i++)
                    {
                        var fileSystemInfo = batch[i];
                        var isDirectory = fileSystemInfo.Attributes.HasFlag(FileAttributes.Directory);
                        var existsInDb = DbContext
                            .FileSystemItems
                            .Any(x => string.Equals(x.Path, fileSystemInfo.FullName) && x.IsFolder == isDirectory);

                        if (!existsInDb)
                        {
                            if (isDirectory)
                            {
                                var folderItem = ToFolderItem(fileSystemInfo);
                                buffer.Add(folderItem);

                                var dbFolder = ToFileSystemItem(fileSystemInfo);
                                DbContext.FileSystemItems.Add(dbFolder);
                            }
                            else
                            {
                                var fileItem = ToFileItem(fileSystemInfo);
                                buffer.Add(fileItem);

                                var dbFile = ToFileSystemItem(fileSystemInfo);
                                DbContext.FileSystemItems.Add(dbFile);
                            }
                        }
                        else
                        {
                            if (isDirectory)
                            {
                                var folderItem = ToFolderItem(fileSystemInfo);
                                buffer.Add(folderItem);
                            }
                            else
                            {
                                var fileItem = ToFileItem(fileSystemInfo);
                                buffer.Add(fileItem);
                            }
                        }
                    }

                    DbContext.SaveChanges();
                    counter++;

                    batch = fileSystemInfos.Skip(counter * batchSize).Take(batchSize).ToArray();

                    foreach (var x in buffer)
                        yield return x;
                }
            }

            // return Enumerable.Empty<IFileSystemItem>();
        }

        private static FileItem ToFileItem(FileSystemInfo fileSystemInfo)
        {
            return new FileItem()
            {
                Path = fileSystemInfo.FullName,
                FullName = fileSystemInfo.Name,
            };
        }

        private static FileSystemItem ToFileSystemItem(FileSystemInfo fileSystemInfo)
        {
            return new FileSystemItem
            {
                Path = fileSystemInfo.FullName,
                Name = fileSystemInfo.Name,
                IsFolder = fileSystemInfo.Attributes.HasFlag(FileAttributes.Directory),
                Extension = fileSystemInfo.Extension,
            };
        }

        private static FolderItem ToFolderItem(FileSystemInfo fileSystemInfo)
        {
            return new FolderItem()
            {
                Path = fileSystemInfo.FullName,
                FullName = fileSystemInfo.Name
            };
        }
    }
}