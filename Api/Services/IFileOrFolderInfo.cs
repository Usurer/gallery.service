namespace Api.Services
{
    public interface IFileOrFolderInfo
    {
        public string Name
        {
            get; set;
        }

        public long Id
        {
            get; set;
        }

        public bool IsFolder
        {
            get;
        }
    }
}