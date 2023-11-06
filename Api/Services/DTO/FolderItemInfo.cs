namespace Api.Services.DTO
{
    public class FolderItemInfo : IItemInfo
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

        public bool IsFolder => true;
    }
}