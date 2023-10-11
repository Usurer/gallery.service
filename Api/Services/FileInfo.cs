namespace Api.Services
{
    public class FileInfo : IFileOrFolderInfo
    {
        public string Name
        {
            get;
            set;
        }

        public long Id
        {
            get;
            set;
        }

        public bool IsFolder => false;
    }
}