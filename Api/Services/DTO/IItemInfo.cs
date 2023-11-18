namespace Api.Services.DTO
{
    public interface IItemInfo
    {
        public long Id
        {
            get;
        }

        public string Name
        {
            get;
        }

        public DateTime CreationDate
        {
            get;
        }

        public bool IsFolder
        {
            get;
        }

        public int? Width
        {
            get;
        }

        public int? Height
        {
            get;
        }
    }
}