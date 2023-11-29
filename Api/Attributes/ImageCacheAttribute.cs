namespace Api.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ImageCacheAttribute : Attribute
    {
        public required int DurationMinutes
        {
            get; set;
        }
    }
}