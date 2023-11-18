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

        // TODO: Actually Image width and Height cannot be nulls
        // but because it shares the interface with the Directory, and directory doesn't have dimensions
        // I have to set these to int? which sucks and should be refactored
        public required int? Width
        {
            get; set;
        }

        public required int? Height
        {
            get; set;
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