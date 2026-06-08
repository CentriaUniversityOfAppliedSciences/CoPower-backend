namespace Copower_API.Models.API
{
    /// <summary>
    /// API initialisation model
    /// </summary>
    public class APIInitModel
    {
        /// <summary>
        /// Organisation identifier
        /// </summary>
        public required Guid Id { get; set; }

        /// <summary>
        /// Organisation name
        /// </summary>
        public required String Name { get; set; }
    }
}
