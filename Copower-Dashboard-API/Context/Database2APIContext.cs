using Copower_API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Copower_API.Context
{
    /// <summary>
    /// Database 2 API context
    /// </summary>
    /// <param name="Configuration">Configuration</param>
    public class Database2APIContext(IConfiguration Configuration) : DbContext
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
            options.UseNpgsql(_Configuration.GetConnectionString("Database2API"));
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
