using Api.Database;

namespace Api.Utils
{
    public static class FileSystemInfoExtensions
    {
        public static bool IsDirectory(this FileSystemInfo info)
        {
            return info.Attributes.HasFlag(FileAttributes.Directory);
        }

        public static FileSystemItem ToFileSystemItem(this FileSystemInfo fileSystemInfo, long? parentid)
        {
            return new FileSystemItem
            {
                Path = fileSystemInfo.FullName,
                Name = fileSystemInfo.Name,
                IsFolder = fileSystemInfo.IsDirectory(),
                Extension = fileSystemInfo.Extension,
                ParentId = parentid,
            };
        }
    }
}