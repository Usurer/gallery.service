﻿using Api.Controllers.Internal;
using Api.Database;
using Api.Utils;
using Imageflow.Bindings;
using Imageflow.Fluent;
using Microsoft.Extensions.Options;

namespace Api.Services
{
    public interface IFileSystemService
    {
        public Task<ScanFolderResult> ScanFolderAsync(string? fullPath, IProgress<int>? progress);

        public IAsyncEnumerable<ScanFolderResult> ScanFoldersFromRootAsync(string? root);
    }

    /*
     * TODO: Refactor it
     * I don't like how it's implemented right now - it's not clear that this class writes to DB
     * Maybe it should only read FS data and then use IStorageService implementation to write it
     * */

    public class FileSystemService : IFileSystemService
    {
        private readonly FileSystemOptions FileSystemOptions;

        private readonly GalleryContext DbContext;

        private readonly ILogger<FileSystemService> Logger;

        public FileSystemService(IOptions<FileSystemOptions> options, GalleryContext context, ILogger<FileSystemService> logger)
        {
            FileSystemOptions = options.Value;
            DbContext = context;
            Logger = logger;
        }

        public async IAsyncEnumerable<ScanFolderResult> ScanFoldersFromRootAsync(string? root)
        {
            root = string.IsNullOrEmpty(root) ? FileSystemOptions.DefaultFolder : root.Trim();

            if (!Directory.Exists(root))
            {
                yield break;
            }

            var rootDirectoryInfo = new DirectoryInfo(root);
            var directories = rootDirectoryInfo.GetDirectories();

            var folderResult = await ScanFolderAsync(root);
            //results.Add(folderResult);
            yield return folderResult;

            foreach (var directory in directories)
            {
                var subtreeResults = ScanFoldersFromRootAsync(directory.FullName);
                await foreach (var subtreeResult in subtreeResults)
                {
                    yield return subtreeResult;
                }
            }

            yield break;
        }

        public async Task<ScanFolderResult> ScanFolderAsync(string? fullPath, IProgress<int>? progress = null)
        {
            fullPath = string.IsNullOrEmpty(fullPath) ? FileSystemOptions.DefaultFolder : fullPath.Trim();
            var result = new ScanFolderResult { Path = fullPath };

            if (Directory.Exists(fullPath))
            {
                var batchSize = 100;
                var batchCounter = 0;

                var rootDirectoryInfo = new DirectoryInfo(fullPath);

                // Use path from the filesystem instead of user-provided value
                result.Path = rootDirectoryInfo.FullName;

                var rootDbRecord = DbContext.FileSystemItems.Where(x => string.Equals(x.Path, fullPath)).FirstOrDefault();
                if (rootDbRecord == null)
                {
                    rootDbRecord = rootDirectoryInfo.ToFileSystemItem(null, null, null);
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
                            ImageInfo? imageInfo = null;
                            if (!isDirectory)
                            {
                                var job = new ImageJob();
                                using var imageData = File.OpenRead(fileSystemInfo.FullName);
                                try
                                {
                                    imageInfo = await ImageJob.GetImageInfo(new StreamSource(imageData, false));
                                }
                                catch (Exception ex)
                                {
                                    Logger.Log(LogLevel.Error, ex, $"Error while getting Image Info for {fileSystemInfo.FullName}");
                                }
                            }

                            FileSystemItem newItem = fileSystemInfo.ToFileSystemItem(
                                rootDbRecord.Id,
                                imageInfo != null ? (int)imageInfo.ImageWidth : null,
                                imageInfo != null ? (int)imageInfo.ImageHeight : null
                            );

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