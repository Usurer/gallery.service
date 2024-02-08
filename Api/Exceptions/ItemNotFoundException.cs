namespace Api.Exceptions
{
    [Serializable]
    public class ItemNotFoundException : Exception
    {
        private const string DefaultMessage = "Item not found";

        public ItemNotFoundException() : base(DefaultMessage)
        {
        }

        public ItemNotFoundException(long id) : base($"Item with ID ${id} not found")
        {
        }
    }
}