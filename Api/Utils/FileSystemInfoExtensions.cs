using Api.DTO;
using Api.Services;

namespace Api.Utils
{
    public static class FileSystemInfoExtensions
    {
        public static bool IsDirectory(this FileSystemInfo info)
        {
            return info.Attributes.HasFlag(FileAttributes.Directory);
        }

        public static Services.FileInfo ToFileItem(this FileSystemInfo fileSystemInfo)
        {
            return new Services.FileInfo()
            {
                Path = fileSystemInfo.FullName,
                Name = fileSystemInfo.Name,
            };
        }

        public static FolderInfo ToFolderItem(this FileSystemInfo fileSystemInfo)
        {
            return new FolderInfo()
            {
                Path = fileSystemInfo.FullName,
                Name = fileSystemInfo.Name,
            };
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