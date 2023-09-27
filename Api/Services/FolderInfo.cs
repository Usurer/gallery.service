namespace Api.Services
{
    public class FolderInfo : IFileOrFolderInfo
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

        public bool IsFolder => true;
    }
}