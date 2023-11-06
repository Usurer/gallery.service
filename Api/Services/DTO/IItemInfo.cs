namespace Api.Services.DTO
{
    public interface IItemInfo
    {
        public long Id
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public DateTime CreationDate
        {
            get; set;
        }

        public bool IsFolder
        {
            get;
        }
    }
}