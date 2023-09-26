using Api.DTO;
using Api.Utils;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Api.Services
{
    public interface IFileSystemService
    {
        public Task<ScanFolderResult> ScanFolderAsync(string? fullPath, bool recurse, IProgress<int>? progress);
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

        public async Task<ScanFolderResult> ScanFolderAsync(string? fullPath, bool recurse = false, IProgress<int>? progress = null)
        {
            var path = string.IsNullOrEmpty(fullPath) ? FileSystemOptions.DefaultFolder : fullPath;
            var result = new ScanFolderResult();

            if (Directory.Exists(path))
            {
                var batchSize = 100;
                var counter = 0;

                var rootDirectoryInfo = new DirectoryInfo(path);
                var rootDbRecord = DbContext.FileSystemItems.Where(x => string.Equals(x.Path, path)).FirstOrDefault();
                if (rootDbRecord == null)
                {
                    rootDbRecord = rootDirectoryInfo.ToFileSystemItem(null);
                    DbContext.Add(rootDbRecord);
                    await DbContext.SaveChangesAsync();
                }

                var fileSystemInfos = rootDirectoryInfo.EnumerateFileSystemInfos();

                var batch = fileSystemInfos.Skip(counter * batchSize).Take(batchSize).ToArray();

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

                        // TODO: Even if existsInDb we can update missing ParentId if it's possible
                        if (!existsInDb)
                        {
                            FileSystemItem newItem = fileSystemInfo.ToFileSystemItem(rootDbRecord.Id);

                            DbContext.FileSystemItems.Add(newItem);

                            // Yeah, these are not saved yet, just added to the Context, but okay
                            result.Saved++;
                        }
                    }

                    await DbContext.SaveChangesAsync();

                    if (progress != null)
                    {
                        progress.Report(batchSize * counter + batch.Length);
                    }

                    counter++;

                    batch = fileSystemInfos.Skip(counter * batchSize).Take(batchSize).ToArray();
                }

                // TODO: Incorect, fix this
                result.Total = batchSize * counter + batch.Length;
            }

            return result;
        }
    }
}