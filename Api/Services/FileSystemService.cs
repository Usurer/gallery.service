using Microsoft.Extensions.Options;

namespace Api.Services
{
    public interface IFileSystemService
    {
        public IEnumerable<IFileSystemItem> GetItems(string? fullPath, bool recurse = false);
    }

    public class FileSystemService : IFileSystemService
    {
        private readonly FileSystemOptions FileSystemOptions;

        public FileSystemService(IOptions<FileSystemOptions> options)
        {
            FileSystemOptions = options.Value;
        }

        public IEnumerable<IFileSystemItem> GetItems(string? fullPath, bool recurse = false)
        {
            var path = string.IsNullOrEmpty(fullPath) ? FileSystemOptions.DefaultFolder : fullPath;
            
            if (Directory.Exists(path))
            {
                return Directory.EnumerateFiles(path).Select(x =>
                {
                    var info = new FileInfo(x);
                    return new FileItem { Path = x, FullName = info.Name };
                });
            }

            return Enumerable.Empty<IFileSystemItem>();
        }
    }
}