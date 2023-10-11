namespace Api.Services.DTO
{
    public class FolderItemInfo : IItemInfo
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