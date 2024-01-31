namespace Api.Services.DTO
{
    public abstract class ItemInfo
    {
        public long Id
        {
            get; init;
        }

        public required string Name
        {
            get; init;
        }

        public DateTime CreationDate
        {
            get; init;
        }

        public abstract bool IsFolder
        {
            get;
        }

        public long UpdatedAtDate
        {
            get; init;
        }
    }
}