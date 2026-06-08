using Copower_API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Copower_API.Context
{
    /// <summary>
    /// Commondata context
    /// </summary>
    /// <remarks>
    /// Commondata context constructor
    /// </remarks>
    /// <param name="Configuration">Configuration</param>
    public class CommondataContext(IConfiguration Configuration) : DbContext
    {
        /// <summary>
        /// Configuration
        /// </summary>
        protected readonly IConfiguration _Configuration = Configuration;

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="options"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(_Configuration.GetConnectionString("CommondataDB"));
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MeasurementData>().HasNoKey();
        }
    }
}
