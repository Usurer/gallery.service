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

        public static FileItem ToFileItem(this FileSystemInfo fileSystemInfo)
        {
            return new FileItem()
            {
                Path = fileSystemInfo.FullName,
                FullName = fileSystemInfo.Name,
            };
        }

        public static FolderItem ToFolderItem(this FileSystemInfo fileSystemInfo)
        {
            return new FolderItem()
            {
                Path = fileSystemInfo.FullName,
                FullName = fileSystemInfo.Name,
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