using Api.DTO;
using Api.Utils;
using Microsoft.Extensions.Options;

namespace Api.Services
{
    public interface IFileSystemService
    {
        public Task<ScanFolderResult> ScanFolderAsync(string? fullPath, IProgress<int>? progress);

        public Task<IList<ScanFolderResult>> ScanTreeAsync(string? root);
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

        public async Task<IList<ScanFolderResult>> ScanTreeAsync(string? root)
        {
            if (!Directory.Exists(root))
            {
                return Array.Empty<ScanFolderResult>();
            }

            var rootDirectoryInfo = new DirectoryInfo(root);
            var directories = rootDirectoryInfo.GetDirectories();

            var results = new List<ScanFolderResult>();

            var folderResult = await ScanFolderAsync(root);
            results.Add(folderResult);

            foreach (var directory in directories)
            {
                var subtreeResults = await ScanTreeAsync(directory.FullName);
                results.AddRange(subtreeResults);
            }

            return results;
        }

        public async Task<ScanFolderResult> ScanFolderAsync(string? fullPath, IProgress<int>? progress = null)
        {
            var path = string.IsNullOrEmpty(fullPath) ? FileSystemOptions.DefaultFolder : fullPath;
            var result = new ScanFolderResult(path);

            if (Directory.Exists(path))
            {
                var batchSize = 100;
                var batchCounter = 0;

                var rootDirectoryInfo = new DirectoryInfo(path);
                var rootDbRecord = DbContext.FileSystemItems.Where(x => string.Equals(x.Path, path)).FirstOrDefault();
                if (rootDbRecord == null)
                {
                    rootDbRecord = rootDirectoryInfo.ToFileSystemItem(null);
                    DbContext.Add(rootDbRecord);
                    await DbContext.SaveChangesAsync();
                }

                var fileSystemInfos = rootDirectoryInfo.EnumerateFileSystemInfos();

                var batch = fileSystemInfos.Skip(batchCounter * batchSize).Take(batchSize).ToArray();

                while (batch.Length > 0)
                {
                    for (int i = 0; i < batch.Length; i++)
                    {
                        var fileSystemInfo = batch[i];
                        var isDirectory = fileSystemInfo.IsDirectory();

                        var existsInDb = DbContext
                            .FileSystemItems
                            .Any(x =>
                                string.Equals(x.Path, fileSystemInfo.FullName)
                            );

                        // TODO: Even if existsInDb we can update missing ParentId if it's possible. Not sure about it
                        if (!existsInDb)
                        {
                            FileSystemItem newItem = fileSystemInfo.ToFileSystemItem(rootDbRecord.Id);

                            DbContext.FileSystemItems.Add(newItem);

                            // Yeah, these are not saved yet, just added to the Context, but okay
                            result.Saved++;
                        }
                        result.Total++;
                    }

                    await DbContext.SaveChangesAsync();

                    if (progress != null)
                    {
                        progress.Report(batchSize * batchCounter + batch.Length);
                    }

                    batchCounter++;

                    batch = fileSystemInfos.Skip(batchCounter * batchSize).Take(batchSize).ToArray();
                }
            }

            return result;
        }
    }
}