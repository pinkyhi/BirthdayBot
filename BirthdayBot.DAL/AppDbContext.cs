using BirthdayBot.Core.Enums;
using BirthdayBot.Core.Static;
using BirthdayBot.Core.Types;
using BirthdayBot.DAL.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Linq;

namespace BirthdayBot.DAL
{
    public class AppDbContext : DbContext
    {
        private readonly ClientSettings clientSettings;
        private readonly ILogger<AppDbContext> logger;

        public DbSet<TUser> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options, ClientSettings clientSettings, ILogger<AppDbContext> logger, IConfiguration configuration, IHostingEnvironment environment)
            : base(options)
        {
            this.clientSettings = clientSettings;
            this.logger = logger;
            this.Database.Migrate();

            if (!StaticFlags.IsQrtzChecked)
            {
                string connectionString;
                if (environment.IsDevelopment())
                {
                    connectionString = configuration.GetConnectionString("DefaultConnection");
                }
                else
                {
                    connectionString = configuration.GetConnectionString("DefaultConnectionProd");
                }
                FillQuartzTables(connectionString);
                StaticFlags.IsQrtzChecked = true;
            }

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Fluent API
            modelBuilder.Entity<Subscription>()
                .HasKey(x => new { x.SubscriberId, x.TargetId });
            modelBuilder.Entity<Entities.ChatMember>()
                .HasKey(x => new { x.UserId, x.ChatId });
            modelBuilder.Entity<Address_ComponentConnector>()
                .HasKey(x => new { x.AddressComponentId, x.AddressId });

            modelBuilder.Entity<Address>()
                .HasMany(x => x.AddressComponentConnectors)
                .WithOne(x => x.Address)
                .HasForeignKey(x => x.AddressId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Address_Component>()
                .HasMany(x => x.AddressComponentConnectors)
                .WithOne(x => x.AddressComponent)
                .HasForeignKey(x => x.AddressComponentId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<Entities.ChatMember>()
                .Property(x => x.IsSubscribedOnCalendar).HasDefaultValue(true);

            modelBuilder.Entity<TUser>()
                .HasMany(x => x.Addresses)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TUser>()
                .HasMany(x => x.ChatMembers)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TUser>()
                .HasMany(x => x.Notes)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TUser>()
                .HasMany(x => x.Subscriptions)
                .WithOne(x => x.Subscriber)
                .HasForeignKey(x => x.SubscriberId)
                .OnDelete(DeleteBehavior.ClientCascade);
            modelBuilder.Entity<TUser>()
                .HasMany(x => x.Subscribers)
                .WithOne(x => x.Target)
                .HasForeignKey(x => x.TargetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TUser>().OwnsOne(c => c.Settings, a => {
                a.Property(x => x.BirthDateConfidentiality)
                .HasConversion<int>()
                .HasDefaultValue(ConfidentialType.Public);
                a.Property(x => x.BirthYearConfidentiality)
                .HasConversion<int>()
                .HasDefaultValue(ConfidentialType.Public);                
                a.Property(x => x.CommonNotification_0)
                .HasDefaultValue(0);
                a.Property(x => x.StrongNotification_0)
                .HasDefaultValue(7);
                a.Property(x => x.StrongNotification_1)
                .HasDefaultValue(3);
                a.Property(x => x.StrongNotification_2)
                .HasDefaultValue(0);
            });

            modelBuilder.Entity<TUser>().OwnsOne(c => c.Limitations, a => {
                a.Property(x => x.StartLocationInputAttempts)
                .HasDefaultValue(this.clientSettings.StartLocationInputAttempts);
                a.Property(x => x.ChangeLocationInputAttempts)
                .HasDefaultValue(this.clientSettings.ChangeLocationInputAttempts);
            });


            modelBuilder.Entity<Entities.Chat>()
                .HasMany(x => x.ChatMembers)
                .WithOne(x => x.Chat)
                .HasForeignKey(x => x.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Entities.Chat>()
                .Property(c => c.Type)
                .HasConversion<int>();

            var valueComparer = new ValueComparer<string[]>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToArray());

            modelBuilder.Entity<Address>()
                .Property(x => x.Types)
                .HasConversion(
                    x => string.Join(',', x),
                    x => x.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Metadata
                .SetValueComparer(valueComparer);
            modelBuilder.Entity<Address_Component>()
                .Property(x => x.Types)
                .HasConversion(
                    x => string.Join(',', x),
                    x => x.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Metadata
                .SetValueComparer(valueComparer);
        }

        private void FillQuartzTables(string connectionString)
        {
            var commands = @$"USE [{this.Database.GetDbConnection().Database}];
                GO

                IF OBJECT_ID(N'[dbo].[FK_QRTZ_TRIGGERS_QRTZ_JOB_DETAILS]', N'F') IS NOT NULL
                ALTER TABLE [dbo].[QRTZ_TRIGGERS] DROP CONSTRAINT [FK_QRTZ_TRIGGERS_QRTZ_JOB_DETAILS];
                GO

                IF OBJECT_ID(N'[dbo].[FK_QRTZ_CRON_TRIGGERS_QRTZ_TRIGGERS]', N'F') IS NOT NULL
                ALTER TABLE [dbo].[QRTZ_CRON_TRIGGERS] DROP CONSTRAINT [FK_QRTZ_CRON_TRIGGERS_QRTZ_TRIGGERS];
                GO

                IF OBJECT_ID(N'[dbo].[FK_QRTZ_SIMPLE_TRIGGERS_QRTZ_TRIGGERS]', N'F') IS NOT NULL
                ALTER TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS] DROP CONSTRAINT [FK_QRTZ_SIMPLE_TRIGGERS_QRTZ_TRIGGERS];
                GO

                IF OBJECT_ID(N'[dbo].[FK_QRTZ_SIMPROP_TRIGGERS_QRTZ_TRIGGERS]', N'F') IS NOT NULL
                ALTER TABLE [dbo].[QRTZ_SIMPROP_TRIGGERS] DROP CONSTRAINT [FK_QRTZ_SIMPROP_TRIGGERS_QRTZ_TRIGGERS];
                GO

                IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_QRTZ_JOB_LISTENERS_QRTZ_JOB_DETAILS]') AND parent_object_id = OBJECT_ID(N'[dbo].[QRTZ_JOB_LISTENERS]'))
                ALTER TABLE [dbo].[QRTZ_JOB_LISTENERS] DROP CONSTRAINT [FK_QRTZ_JOB_LISTENERS_QRTZ_JOB_DETAILS];

                IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_QRTZ_TRIGGER_LISTENERS_QRTZ_TRIGGERS]') AND parent_object_id = OBJECT_ID(N'[dbo].[QRTZ_TRIGGER_LISTENERS]'))
                ALTER TABLE [dbo].[QRTZ_TRIGGER_LISTENERS] DROP CONSTRAINT [FK_QRTZ_TRIGGER_LISTENERS_QRTZ_TRIGGERS];


                IF OBJECT_ID(N'[dbo].[QRTZ_CALENDARS]', N'U') IS NOT NULL
                DROP TABLE [dbo].[QRTZ_CALENDARS];
                GO

                IF OBJECT_ID(N'[dbo].[QRTZ_CRON_TRIGGERS]', N'U') IS NOT NULL
                DROP TABLE [dbo].[QRTZ_CRON_TRIGGERS];
                GO

                IF OBJECT_ID(N'[dbo].[QRTZ_BLOB_TRIGGERS]', N'U') IS NOT NULL
                DROP TABLE [dbo].[QRTZ_BLOB_TRIGGERS];
                GO

                IF OBJECT_ID(N'[dbo].[QRTZ_FIRED_TRIGGERS]', N'U') IS NOT NULL
                DROP TABLE [dbo].[QRTZ_FIRED_TRIGGERS];
                GO

                IF OBJECT_ID(N'[dbo].[QRTZ_PAUSED_TRIGGER_GRPS]', N'U') IS NOT NULL
                DROP TABLE [dbo].[QRTZ_PAUSED_TRIGGER_GRPS];
                GO

                IF  OBJECT_ID(N'[dbo].[QRTZ_JOB_LISTENERS]', N'U') IS NOT NULL
                DROP TABLE [dbo].[QRTZ_JOB_LISTENERS];

                IF OBJECT_ID(N'[dbo].[QRTZ_SCHEDULER_STATE]', N'U') IS NOT NULL
                DROP TABLE [dbo].[QRTZ_SCHEDULER_STATE];
                GO

                IF OBJECT_ID(N'[dbo].[QRTZ_LOCKS]', N'U') IS NOT NULL
                DROP TABLE [dbo].[QRTZ_LOCKS];
                GO
                IF OBJECT_ID(N'[dbo].[QRTZ_TRIGGER_LISTENERS]', N'U') IS NOT NULL
                DROP TABLE [dbo].[QRTZ_TRIGGER_LISTENERS];


                IF OBJECT_ID(N'[dbo].[QRTZ_JOB_DETAILS]', N'U') IS NOT NULL
                DROP TABLE [dbo].[QRTZ_JOB_DETAILS];
                GO

                IF OBJECT_ID(N'[dbo].[QRTZ_SIMPLE_TRIGGERS]', N'U') IS NOT NULL
                DROP TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS];
                GO

                IF OBJECT_ID(N'[dbo].[QRTZ_SIMPROP_TRIGGERS]', N'U') IS NOT NULL
                DROP TABLE [dbo].[QRTZ_SIMPROP_TRIGGERS];
                GO

                IF OBJECT_ID(N'[dbo].[QRTZ_TRIGGERS]', N'U') IS NOT NULL
                DROP TABLE [dbo].[QRTZ_TRIGGERS];
                GO

                CREATE TABLE [dbo].[QRTZ_CALENDARS] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [CALENDAR_NAME] nvarchar(200) NOT NULL,
                  [CALENDAR] varbinary(max) NOT NULL
                );
                GO

                CREATE TABLE [dbo].[QRTZ_CRON_TRIGGERS] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [TRIGGER_NAME] nvarchar(150) NOT NULL,
                  [TRIGGER_GROUP] nvarchar(150) NOT NULL,
                  [CRON_EXPRESSION] nvarchar(120) NOT NULL,
                  [TIME_ZONE_ID] nvarchar(80) 
                );
                GO

                CREATE TABLE [dbo].[QRTZ_FIRED_TRIGGERS] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [ENTRY_ID] nvarchar(140) NOT NULL,
                  [TRIGGER_NAME] nvarchar(150) NOT NULL,
                  [TRIGGER_GROUP] nvarchar(150) NOT NULL,
                  [INSTANCE_NAME] nvarchar(200) NOT NULL,
                  [FIRED_TIME] bigint NOT NULL,
                  [SCHED_TIME] bigint NOT NULL,
                  [PRIORITY] int NOT NULL,
                  [STATE] nvarchar(16) NOT NULL,
                  [JOB_NAME] nvarchar(150) NULL,
                  [JOB_GROUP] nvarchar(150) NULL,
                  [IS_NONCONCURRENT] bit NULL,
                  [REQUESTS_RECOVERY] bit NULL 
                );
                GO

                CREATE TABLE [dbo].[QRTZ_PAUSED_TRIGGER_GRPS] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [TRIGGER_GROUP] nvarchar(150) NOT NULL 
                );
                GO

                CREATE TABLE [dbo].[QRTZ_SCHEDULER_STATE] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [INSTANCE_NAME] nvarchar(200) NOT NULL,
                  [LAST_CHECKIN_TIME] bigint NOT NULL,
                  [CHECKIN_INTERVAL] bigint NOT NULL
                );
                GO

                CREATE TABLE [dbo].[QRTZ_LOCKS] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [LOCK_NAME] nvarchar(40) NOT NULL 
                );
                GO

                CREATE TABLE [dbo].[QRTZ_JOB_DETAILS] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [JOB_NAME] nvarchar(150) NOT NULL,
                  [JOB_GROUP] nvarchar(150) NOT NULL,
                  [DESCRIPTION] nvarchar(250) NULL,
                  [JOB_CLASS_NAME] nvarchar(250) NOT NULL,
                  [IS_DURABLE] bit NOT NULL,
                  [IS_NONCONCURRENT] bit NOT NULL,
                  [IS_UPDATE_DATA] bit NOT NULL,
                  [REQUESTS_RECOVERY] bit NOT NULL,
                  [JOB_DATA] varbinary(max) NULL
                );
                GO

                CREATE TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [TRIGGER_NAME] nvarchar(150) NOT NULL,
                  [TRIGGER_GROUP] nvarchar(150) NOT NULL,
                  [REPEAT_COUNT] int NOT NULL,
                  [REPEAT_INTERVAL] bigint NOT NULL,
                  [TIMES_TRIGGERED] int NOT NULL
                );
                GO

                CREATE TABLE [dbo].[QRTZ_SIMPROP_TRIGGERS] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [TRIGGER_NAME] nvarchar(150) NOT NULL,
                  [TRIGGER_GROUP] nvarchar(150) NOT NULL,
                  [STR_PROP_1] nvarchar(512) NULL,
                  [STR_PROP_2] nvarchar(512) NULL,
                  [STR_PROP_3] nvarchar(512) NULL,
                  [INT_PROP_1] int NULL,
                  [INT_PROP_2] int NULL,
                  [LONG_PROP_1] bigint NULL,
                  [LONG_PROP_2] bigint NULL,
                  [DEC_PROP_1] numeric(13,4) NULL,
                  [DEC_PROP_2] numeric(13,4) NULL,
                  [BOOL_PROP_1] bit NULL,
                  [BOOL_PROP_2] bit NULL,
                  [TIME_ZONE_ID] nvarchar(80) NULL 
                );
                GO

                CREATE TABLE [dbo].[QRTZ_BLOB_TRIGGERS] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [TRIGGER_NAME] nvarchar(150) NOT NULL,
                  [TRIGGER_GROUP] nvarchar(150) NOT NULL,
                  [BLOB_DATA] varbinary(max) NULL
                );
                GO

                CREATE TABLE [dbo].[QRTZ_TRIGGERS] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [TRIGGER_NAME] nvarchar(150) NOT NULL,
                  [TRIGGER_GROUP] nvarchar(150) NOT NULL,
                  [JOB_NAME] nvarchar(150) NOT NULL,
                  [JOB_GROUP] nvarchar(150) NOT NULL,
                  [DESCRIPTION] nvarchar(250) NULL,
                  [NEXT_FIRE_TIME] bigint NULL,
                  [PREV_FIRE_TIME] bigint NULL,
                  [PRIORITY] int NULL,
                  [TRIGGER_STATE] nvarchar(16) NOT NULL,
                  [TRIGGER_TYPE] nvarchar(8) NOT NULL,
                  [START_TIME] bigint NOT NULL,
                  [END_TIME] bigint NULL,
                  [CALENDAR_NAME] nvarchar(200) NULL,
                  [MISFIRE_INSTR] int NULL,
                  [JOB_DATA] varbinary(max) NULL
                );
                GO

                ALTER TABLE [dbo].[QRTZ_CALENDARS] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_CALENDARS] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [CALENDAR_NAME]
                  );
                GO

                ALTER TABLE [dbo].[QRTZ_CRON_TRIGGERS] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_CRON_TRIGGERS] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  );
                GO

                ALTER TABLE [dbo].[QRTZ_FIRED_TRIGGERS] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_FIRED_TRIGGERS] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [ENTRY_ID]
                  );
                GO

                ALTER TABLE [dbo].[QRTZ_PAUSED_TRIGGER_GRPS] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_PAUSED_TRIGGER_GRPS] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [TRIGGER_GROUP]
                  );
                GO

                ALTER TABLE [dbo].[QRTZ_SCHEDULER_STATE] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_SCHEDULER_STATE] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [INSTANCE_NAME]
                  );
                GO

                ALTER TABLE [dbo].[QRTZ_LOCKS] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_LOCKS] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [LOCK_NAME]
                  );
                GO

                ALTER TABLE [dbo].[QRTZ_JOB_DETAILS] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_JOB_DETAILS] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [JOB_NAME],
                    [JOB_GROUP]
                  );
                GO

                ALTER TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_SIMPLE_TRIGGERS] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  );
                GO

                ALTER TABLE [dbo].[QRTZ_SIMPROP_TRIGGERS] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_SIMPROP_TRIGGERS] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  );
                GO

                ALTER TABLE [dbo].[QRTZ_TRIGGERS] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_TRIGGERS] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  );
                GO

                ALTER TABLE [dbo].[QRTZ_BLOB_TRIGGERS] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_BLOB_TRIGGERS] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  );
                GO

                ALTER TABLE [dbo].[QRTZ_CRON_TRIGGERS] ADD
                  CONSTRAINT [FK_QRTZ_CRON_TRIGGERS_QRTZ_TRIGGERS] FOREIGN KEY
                  (
                    [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  ) REFERENCES [dbo].[QRTZ_TRIGGERS] (
                    [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  ) ON DELETE CASCADE;
                GO

                ALTER TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS] ADD
                  CONSTRAINT [FK_QRTZ_SIMPLE_TRIGGERS_QRTZ_TRIGGERS] FOREIGN KEY
                  (
                    [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  ) REFERENCES [dbo].[QRTZ_TRIGGERS] (
                    [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  ) ON DELETE CASCADE;
                GO

                ALTER TABLE [dbo].[QRTZ_SIMPROP_TRIGGERS] ADD
                  CONSTRAINT [FK_QRTZ_SIMPROP_TRIGGERS_QRTZ_TRIGGERS] FOREIGN KEY
                  (
	                [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  ) REFERENCES [dbo].[QRTZ_TRIGGERS] (
                    [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  ) ON DELETE CASCADE;
                GO

                ALTER TABLE [dbo].[QRTZ_TRIGGERS] ADD
                  CONSTRAINT [FK_QRTZ_TRIGGERS_QRTZ_JOB_DETAILS] FOREIGN KEY
                  (
                    [SCHED_NAME],
                    [JOB_NAME],
                    [JOB_GROUP]
                  ) REFERENCES [dbo].[QRTZ_JOB_DETAILS] (
                    [SCHED_NAME],
                    [JOB_NAME],
                    [JOB_GROUP]
                  );
                GO

                -- drop indexe if they exist and rebuild if current ones
                DROP INDEX IF EXISTS [IDX_QRTZ_T_J] ON [dbo].[QRTZ_TRIGGERS];
                DROP INDEX IF EXISTS [IDX_QRTZ_T_JG] ON [dbo].[QRTZ_TRIGGERS];
                DROP INDEX IF EXISTS [IDX_QRTZ_T_C] ON [dbo].[QRTZ_TRIGGERS];
                DROP INDEX IF EXISTS [IDX_QRTZ_T_G] ON [dbo].[QRTZ_TRIGGERS];
                DROP INDEX IF EXISTS [IDX_QRTZ_T_G_J] ON [dbo].[QRTZ_TRIGGERS];
                DROP INDEX IF EXISTS [IDX_QRTZ_T_STATE] ON [dbo].[QRTZ_TRIGGERS];
                DROP INDEX IF EXISTS [IDX_QRTZ_T_N_STATE] ON [dbo].[QRTZ_TRIGGERS];
                DROP INDEX IF EXISTS [IDX_QRTZ_T_N_G_STATE] ON [dbo].[QRTZ_TRIGGERS];
                DROP INDEX IF EXISTS [IDX_QRTZ_T_NEXT_FIRE_TIME] ON [dbo].[QRTZ_TRIGGERS];
                DROP INDEX IF EXISTS [IDX_QRTZ_T_NFT_ST] ON [dbo].[QRTZ_TRIGGERS];
                DROP INDEX IF EXISTS [IDX_QRTZ_T_NFT_MISFIRE] ON [dbo].[QRTZ_TRIGGERS];
                DROP INDEX IF EXISTS [IDX_QRTZ_T_NFT_ST_MISFIRE] ON [dbo].[QRTZ_TRIGGERS];
                DROP INDEX IF EXISTS [IDX_QRTZ_T_NFT_ST_MISFIRE_GRP] ON [dbo].[QRTZ_TRIGGERS];
                DROP INDEX IF EXISTS [IDX_QRTZ_FT_TRIG_INST_NAME] ON [dbo].[QRTZ_FIRED_TRIGGERS];
                DROP INDEX IF EXISTS [IDX_QRTZ_FT_INST_JOB_REQ_RCVRY] ON [dbo].[QRTZ_FIRED_TRIGGERS];
                DROP INDEX IF EXISTS [IDX_QRTZ_FT_J_G] ON [dbo].[QRTZ_FIRED_TRIGGERS];
                DROP INDEX IF EXISTS [IDX_QRTZ_FT_JG] ON [dbo].[QRTZ_FIRED_TRIGGERS];
                DROP INDEX IF EXISTS [IDX_QRTZ_FT_T_G] ON [dbo].[QRTZ_FIRED_TRIGGERS];
                DROP INDEX IF EXISTS [IDX_QRTZ_FT_TG] ON [dbo].[QRTZ_FIRED_TRIGGERS];
                DROP INDEX IF EXISTS [IDX_QRTZ_FT_G_J] ON [dbo].[QRTZ_FIRED_TRIGGERS];
                DROP INDEX IF EXISTS [IDX_QRTZ_FT_G_T] ON [dbo].[QRTZ_FIRED_TRIGGERS];
                GO


                CREATE INDEX [IDX_QRTZ_T_G_J]                 ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, JOB_GROUP, JOB_NAME);
                CREATE INDEX [IDX_QRTZ_T_C]                   ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, CALENDAR_NAME);

                CREATE INDEX [IDX_QRTZ_T_N_G_STATE]           ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_GROUP, TRIGGER_STATE);
                CREATE INDEX [IDX_QRTZ_T_STATE]               ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_STATE);
                CREATE INDEX [IDX_QRTZ_T_N_STATE]             ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP, TRIGGER_STATE);
                CREATE INDEX [IDX_QRTZ_T_NEXT_FIRE_TIME]      ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, NEXT_FIRE_TIME);
                CREATE INDEX [IDX_QRTZ_T_NFT_ST]              ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_STATE, NEXT_FIRE_TIME);
                CREATE INDEX [IDX_QRTZ_T_NFT_ST_MISFIRE]      ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, MISFIRE_INSTR, NEXT_FIRE_TIME, TRIGGER_STATE);
                CREATE INDEX [IDX_QRTZ_T_NFT_ST_MISFIRE_GRP]  ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, MISFIRE_INSTR, NEXT_FIRE_TIME, TRIGGER_GROUP, TRIGGER_STATE);

                CREATE INDEX [IDX_QRTZ_FT_INST_JOB_REQ_RCVRY] ON [dbo].[QRTZ_FIRED_TRIGGERS](SCHED_NAME, INSTANCE_NAME, REQUESTS_RECOVERY);
                CREATE INDEX [IDX_QRTZ_FT_G_J]                ON [dbo].[QRTZ_FIRED_TRIGGERS](SCHED_NAME, JOB_GROUP, JOB_NAME);
                CREATE INDEX [IDX_QRTZ_FT_G_T]                ON [dbo].[QRTZ_FIRED_TRIGGERS](SCHED_NAME, TRIGGER_GROUP, TRIGGER_NAME);
                GO
                ".Split("GO");

            bool tableExists = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    var checkCommand = "SELECT CASE WHEN OBJECT_ID('dbo.QRTZ_TRIGGERS', 'U') IS NOT NULL THEN 1 ELSE 0 END";

                    SqlCommand command = new SqlCommand(checkCommand, connection);
                    command.CommandType = CommandType.Text;
                    connection.Open();

                    tableExists = Convert.ToBoolean(command.ExecuteScalar());
                }
            }
            catch(Exception ex)
            {
                logger.LogError($"Error during QRTZ db check: {ex.Message}\n{ex.StackTrace}");
                return;
            }

            if (!tableExists)
            {
                foreach (var command in commands)
                {
                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        this.Database.ExecuteSqlRaw(command);
                    }
                }
            }
        }
    }
}
