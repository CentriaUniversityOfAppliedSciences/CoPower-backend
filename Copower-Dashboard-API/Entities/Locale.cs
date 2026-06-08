using System.ComponentModel.DataAnnotations;

namespace Copower_API.Entities
{
    /// <summary>
    /// Email locale object
    /// </summary>
    public class Locale
    {
        /// <summary>
        /// Id number
        /// </summary>
        [Key]
        public int? Id { get; set; }
        /// <summary>
        /// Language key
        /// </summary>
        public required string Key { get; set; }
        /// <summary>
        /// Language id
        /// </summary>
        public required string Language { get; set; }
        /// <summary>
        /// Message
        /// </summary>
        public required string Message { get; set; }
        /// <summary>
        /// Topic
        /// </summary>
        public required string Topic { get; set; }
        /// <summary>
        /// Type
        /// </summary>
        public required string Type { get; set; }
    }
}
