namespace Api.Services.DTO
{
    public class FileItemInfo : IItemInfo
    {
        public required long Id
        {
            get;
            set;
        }

        public required string Name
        {
            get;
            set;
        }

        public required DateTime CreationDate
        {
            get; set;
        }

        public string Extension
        {
            get;
            set;
        }

        public bool IsFolder => false;
    }
}