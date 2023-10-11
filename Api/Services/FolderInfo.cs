namespace Api.Services
{
    public class FolderInfo : IFileOrFolderInfo
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

        public bool IsFolder => true;
    }
}