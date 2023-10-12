namespace Api.Services.DTO
{
    public class FileItemInfo : IItemInfo
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

        public string Extension
        {
            get;
            set;
        }

        public bool IsFolder => false;
    }
}