namespace Api.Services
{
    public interface IFileItem : IFileSystemItem
    {
    }

    public class FileItem : IFileItem
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