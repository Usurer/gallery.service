using Api.Database;

namespace Api.Utils
{
    public static class FileSystemInfoExtensions
    {
        public static bool IsDirectory(this FileSystemInfo info)
        {
            return info.Attributes.HasFlag(FileAttributes.Directory);
        }

        public static FileSystemItem ToFileSystemItem(
            this FileSystemInfo fileSystemInfo,
            long? parentid,
            int? width,
            int? height
            )
        {
            // Some files have Created date which is later than Modified date
            // Probably due to file copying
            // OFC I should use date from EXIF, but right now let's just do like that
            var creationDate = fileSystemInfo.CreationTime > fileSystemInfo.LastWriteTime
                        ? fileSystemInfo.LastWriteTime
                        : fileSystemInfo.CreationTime;

            return new FileSystemItem
            {
                Path = fileSystemInfo.FullName,
                Name = fileSystemInfo.Name,
                IsFolder = fileSystemInfo.IsDirectory(),
                Extension = fileSystemInfo.Extension,
                CreationDate = DateTimeUtils.ToUnixTimestamp(creationDate),
                UpdatedAtDate = DateTimeUtils.ToUnixTimestamp(DateTime.Now),
                ParentId = parentid,
                Width = width,
                Height = height,
            };
        }
    }
}