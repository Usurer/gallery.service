namespace Api.Middleware
{
    public class CachedResponse
    {
        public long? ContentLength
        {
            get; set;
        }

        public string ContentType
        {
            get; set;
        }

        public byte[] Body
        {
            get; set;
        }
    }
}