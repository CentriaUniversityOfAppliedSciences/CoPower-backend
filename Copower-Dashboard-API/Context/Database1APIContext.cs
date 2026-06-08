using Copower_API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Copower_API.Context
{
    /// <summary>
    /// Database 1 API context
    /// </summary>
    /// <param name="Configuration"></param>
    public class Database1APIContext(IConfiguration Configuration) : DbContext
    {
        /// <summary>
        /// Configuration
        /// </summary>
        protected readonly IConfiguration _Configuration = Configuration;

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="options">Options</param>
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(_Configuration.GetConnectionString("Database1API"));
        }

        /// <summary>
        /// Model creation
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MeasurementData>().HasNoKey();
        }
    }
}
