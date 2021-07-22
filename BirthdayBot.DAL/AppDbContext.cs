using BirthdayBot.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types;

namespace BirthdayBot.DAL
{
    public class AppDbContext : DbContext
    {
        public DbSet<TUser> Users { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
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
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TUser>()
                .HasMany(x => x.Subscribers)
                .WithOne(x => x.Target)
                .HasForeignKey(x => x.TargetId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<TUser>().OwnsOne(c => c.Settings, a => {
                a.Property(x => x.BirthDateConfidentiality)
                .HasConversion<int>();
                a.Property(x => x.BirthYearConfidentiality)
                .HasConversion<int>();
            });


            modelBuilder.Entity<Entities.Chat>()
                .HasMany(x => x.ChatMembers)
                .WithOne(x => x.Chat)
                .HasForeignKey(x => x.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Entities.ChatMember>()
                .Property(c => c.Status)
                .HasConversion<int>();
            modelBuilder.Entity<Entities.Chat>()
                .Property(c => c.Type)
                .HasConversion<int>();

            modelBuilder.Entity<Address>()
                .Property(x => x.Types)
                .HasConversion(
                    x => string.Join(',', x),
                    x => x.Split(',', StringSplitOptions.RemoveEmptyEntries));
            modelBuilder.Entity<Address_Component>()
                .Property(x => x.Types)
                .HasConversion(
                    x => string.Join(',', x),
                    x => x.Split(',', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
