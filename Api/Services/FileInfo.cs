namespace Api.Services
{
    public class FileInfo : IFileOrFolderInfo
    {
        public string Name
        {
            get;
            set;
        }

        public string Path
        {
            get;
            set;
        }

        public bool IsFolder => false;
    }
}