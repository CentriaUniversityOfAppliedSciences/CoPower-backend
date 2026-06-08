using Copower_API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Copower_API.Context
{
    /// <summary>
    /// Common context
    /// </summary>
    /// <remarks>
    /// Common context constructor
    /// </remarks>
    /// <param name="Configuration">Configuration</param>
    public class CommonContext(IConfiguration Configuration) : DbContext
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
            options.UseNpgsql(_Configuration.GetConnectionString("CommonDB"),
                npgsqlOptions => npgsqlOptions
                    .ConfigureDataSource(dataSourceBuilder =>
                        dataSourceBuilder.EnableDynamicJson()
                    )
            );
        }

        /// <summary>
        /// API table
        /// </summary>
        public DbSet<API> API { get; set; }

        /// <summary>
        /// Default dashboard table
        /// </summary>
        public DbSet<DashboardDefault> DashboardDefault { get; set; }

        /// <summary>
        /// Database name table
        /// </summary>
        public DbSet<DB> DB { get; set; }

        /// <summary>
        /// Locale table
        /// </summary>
        public DbSet<Locale> Locale { get; set; }

        /// <summary>
        /// Sensor settings table
        /// </summary>
        public DbSet<Organisation> Organisation { get; set; }

        /// <summary>
        /// Sensor settings table
        /// </summary>
        public DbSet<SensorSettings> SensorSettings { get; set; }

        /// <summary>
        /// User table
        /// </summary>
        public DbSet<User> User { get; set; }

        /// <summary>
        /// User table
        /// </summary>
        public DbSet<ResetTokens> ResetTokens { get; set; }


        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // API
            modelBuilder.Entity<API>()
                .Property(e => e.Active)
                .HasColumnType("boolean")
                .HasDefaultValue(false)
                .IsRequired(false);

            modelBuilder.Entity<API>()
                .Property(e => e.Created)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired(false);

            modelBuilder.Entity<API>()
                .Property(e => e.Creator)
                .HasColumnType("uuid")
                .IsRequired(true);

            modelBuilder.Entity<API>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("character varying(50)");
            });

            modelBuilder.Entity<API>()
                .Property(e => e.LastUsed)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            modelBuilder.Entity<API>()
                .Property(e => e.Organisation)
                .HasColumnType("uuid")
                .IsRequired(false);

            // DB
            modelBuilder.Entity<DB>()
                .Property(e => e.DBId)
                .HasColumnType("text")
                .IsRequired(true);

            modelBuilder.Entity<DB>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .HasDefaultValueSql("gen_random_uuid()");
            });

            modelBuilder.Entity<DB>()
                .Property(e => e.IdNumber)
                .HasColumnType("integer")
                .IsRequired(false);

            modelBuilder.Entity<DB>()
                .Property(e => e.Name)
                .HasColumnType("text")
                .IsRequired(true);

            // Dashboard
            modelBuilder.Entity<DashboardDefault>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("character varying(36)")
                    .HasDefaultValueSql("gen_random_uuid()::text");
            });

            modelBuilder.Entity<DashboardDefault>()
                .Property(e => e.Dashboard)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb")
                .IsRequired(false);

            // Locale
            modelBuilder.Entity<Locale>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .UseIdentityColumn()
                    .ValueGeneratedOnAdd();

            });

            modelBuilder.Entity<Locale>()
                .Property(e => e.Key)
                .HasColumnType("text")
                .IsRequired(true);

            modelBuilder.Entity<Locale>()
                .Property(e => e.Language)
                .HasColumnType("text")
                .IsRequired(true);

            modelBuilder.Entity<Locale>()
                .Property(e => e.Message)
                .HasColumnType("text")
                .IsRequired(true);

            modelBuilder.Entity<Locale>()
                .Property(e => e.Topic)
                .HasColumnType("text")
                .IsRequired(true);

            modelBuilder.Entity<Locale>()
                .Property(e => e.Type)
                .HasColumnType("text")
                .IsRequired(true);

            // Organisation
            modelBuilder.Entity<Organisation>()
                .Property(e => e.Created)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired(false);

            modelBuilder.Entity<Organisation>()
                .Property(e => e.Deleted)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            modelBuilder.Entity<Organisation>()
                .Property(e => e.DeletedItems)
                .HasColumnType("text[]")
                .IsRequired(false);

            modelBuilder.Entity<Organisation>()
                .Property(e => e.Disabled)
                .HasColumnType("boolean")
                .HasDefaultValue(false)
                .IsRequired(false);

            modelBuilder.Entity<Organisation>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .HasDefaultValueSql("gen_random_uuid()");
            });

            modelBuilder.Entity<Organisation>()
                .Property(e => e.Name)
                .HasColumnType("text")
                .IsRequired(true);

            modelBuilder.Entity<Organisation>()
                .Property(e => e.Type)
                .HasColumnType("integer")
                .IsRequired(false);

            modelBuilder.Entity<Organisation>()
                .Property(e => e.Updated)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            // ResetTokens
            modelBuilder.Entity<ResetTokens>()
                .Property(e => e.Created)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired(false);

            modelBuilder.Entity<ResetTokens>()
                .Property(e => e.Expiry)
                .HasColumnType("timestamp with time zone")
                .IsRequired(true);

            modelBuilder.Entity<ResetTokens>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .HasDefaultValueSql("gen_random_uuid()");
            });

            modelBuilder.Entity<ResetTokens>()
                .Property(e => e.Token)
                .HasColumnType("text")
                .IsRequired(true);

            modelBuilder.Entity<ResetTokens>()
                .Property(e => e.Used)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            modelBuilder.Entity<ResetTokens>()
                .Property(e => e.UserId)
                .HasColumnType("uuid")
                .IsRequired(true);

            // SensorSettings
            modelBuilder.Entity<SensorSettings>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .HasDefaultValueSql("gen_random_uuid()");
            });

            modelBuilder.Entity<SensorSettings>()
                .Property(e => e.Unit)
                .HasColumnType("text")
                .HasDefaultValueSql("''::text")
                .IsRequired(false);

            modelBuilder.Entity<SensorSettings>()
                .Property(e => e.Created)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired(false);

            modelBuilder.Entity<SensorSettings>()
                .Property(e => e.DBID)
                .HasColumnType("text")
                .IsRequired(true);

            modelBuilder.Entity<SensorSettings>()
                .Property(e => e.Deleted)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            modelBuilder.Entity<SensorSettings>()
                .Property(e => e.Disabled)
                .HasColumnType("boolean")
                .HasDefaultValue(false)
                .IsRequired(false);

            modelBuilder.Entity<SensorSettings>()
                .Property(e => e.DisplayDashboard)
                .HasColumnType("boolean")
                .HasDefaultValue(false)
                .IsRequired(false);

            modelBuilder.Entity<SensorSettings>()
                .Property(e => e.DBVALUE)
                .HasColumnType("text")
                .IsRequired(true);

            modelBuilder.Entity<SensorSettings>()
                .Property(e => e.DeviceSource)
                .HasColumnType("text")
                .HasDefaultValueSql("''::text")
                .IsRequired(false);

            modelBuilder.Entity<SensorSettings>()
                .Property(e => e.Name)
                .HasColumnType("text")
                .IsRequired(true);

            modelBuilder.Entity<SensorSettings>()
                .Property(e => e.Organisation)
                .HasColumnType("uuid")
                .IsRequired(true);

            modelBuilder.Entity<SensorSettings>()
                .Property(e => e.Shared)
                .HasColumnType("integer")
                .HasDefaultValue(0)
                .IsRequired(true);

            modelBuilder.Entity<SensorSettings>()
                .Property(e => e.Updated)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            modelBuilder.Entity<SensorSettings>()
                .Property(e => e.ValueChange)
                .HasColumnType("double precision")
                .HasDefaultValue(1.0)
                .IsRequired(false);

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .HasDefaultValueSql("gen_random_uuid()");
            });

            modelBuilder.Entity<User>()
                .Property(e => e.Access)
                .HasColumnType("text")
                .HasDefaultValue("'user'::text")
                .IsRequired(false);

            modelBuilder.Entity<User>()
                .Property(e => e.Created)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired(false);

            modelBuilder.Entity<User>()
                .Property(e => e.Deleted)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            modelBuilder.Entity<User>()
                .Property(e => e.Disabled)
                .HasColumnType("boolean")
                .HasDefaultValue(false)
                .IsRequired(false);

            modelBuilder.Entity<User>()
                .Property(e => e.Email)
                .HasColumnType("text")
                .IsRequired(true);

            modelBuilder.Entity<User>()
                .Property(e => e.FailedLogins)
                .HasColumnType("integer")
                .HasDefaultValue(0)
                .IsRequired(false);

            modelBuilder.Entity<User>()
                .Property(e => e.LastLogin)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            modelBuilder.Entity<User>()
                .Property(e => e.Name)
                .HasColumnType("text")
                .IsRequired(true);

            modelBuilder.Entity<User>()
                .Property(e => e.Organisation)
                .HasColumnType("uuid")
                .IsRequired(false);

            modelBuilder.Entity<User>()
                .Property(e => e.Password)
                .HasColumnType("text")
                .IsRequired(false);

            modelBuilder.Entity<User>()
                .Property(e => e.Registered)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            modelBuilder.Entity<User>()
                .Property(e => e.Updated)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);
        }
    }
}
