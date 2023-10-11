namespace Api.Services
{
    public class FileInfo : IFileOrFolderInfo
    {
        public long Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public bool IsFolder => false;
    }
}