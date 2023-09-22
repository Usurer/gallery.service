namespace Api.Services
{
    public interface IFolderItem : IFileSystemItem
    {
    }

    public class FolderItem : IFolderItem
    {
        public string FullName
        {
            get;
            set;
        }

        public string Path
        {
            get;
            set;
        }
    }
}