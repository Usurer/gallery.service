namespace Api.Services.DTO
{
    public class FileItemInfo : ItemInfo
    {
        public override bool IsFolder => false;

        public required int Width
        {
            get; set;
        }

        public required int Height
        {
            get; set;
        }

        public required string Extension
        {
            get; init;
        }
    }
}