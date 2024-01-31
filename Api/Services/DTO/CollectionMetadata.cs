namespace Api.Services.DTO
{
    public class CollectionMetadata
    {
        public long? RootId
        {
            get; set;
        }

        public int ItemsCount
        {
            get; set;
        }

        public required Dictionary<DateTime, int> ItemsPerMonth
        {
            get; set;
        }
    }
}