namespace Api.Services
{
    public class FolderInfo : IFileOrFolderInfo
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

        public bool IsFolder => true;
    }
}