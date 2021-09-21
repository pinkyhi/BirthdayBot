﻿// <auto-generated />
using System;
using BirthdayBot.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BirthdayBot.DAL.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20210921094927_UserStatuses")]
    partial class UserStatuses
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.17")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("BirthdayBot.DAL.Entities.Address", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Formatted_Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Types")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Address");
                });

            modelBuilder.Entity("BirthdayBot.DAL.Entities.Address_Component", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Long_Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Short_Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Types")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Address_Component");
                });

            modelBuilder.Entity("BirthdayBot.DAL.Entities.Address_ComponentConnector", b =>
                {
                    b.Property<long>("AddressComponentId")
                        .HasColumnType("bigint");

                    b.Property<long>("AddressId")
                        .HasColumnType("bigint");

                    b.HasKey("AddressComponentId", "AddressId");

                    b.HasIndex("AddressId");

                    b.ToTable("Address_ComponentConnector");
                });

            modelBuilder.Entity("BirthdayBot.DAL.Entities.Chat", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("AddingDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("BigFileUniqueId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("NotificationsCount")
                        .HasColumnType("int");

                    b.Property<string>("SmallFileUniqueId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Chat");
                });

            modelBuilder.Entity("BirthdayBot.DAL.Entities.ChatMember", b =>
                {
                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<long>("ChatId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("AddingDate")
                        .HasColumnType("datetime2");

                    b.HasKey("UserId", "ChatId");

                    b.HasIndex("ChatId");

                    b.ToTable("ChatMember");
                });

            modelBuilder.Entity("BirthdayBot.DAL.Entities.Note", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsStrong")
                        .HasColumnType("bit");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Note");
                });

            modelBuilder.Entity("BirthdayBot.DAL.Entities.Subscription", b =>
                {
                    b.Property<long>("SubscriberId")
                        .HasColumnType("bigint");

                    b.Property<long>("TargetId")
                        .HasColumnType("bigint");

                    b.Property<bool>("IsStrong")
                        .HasColumnType("bit");

                    b.HasKey("SubscriberId", "TargetId");

                    b.HasIndex("TargetId");

                    b.ToTable("Subscription");
                });

            modelBuilder.Entity("BirthdayBot.DAL.Entities.TUser", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("BirthDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("CurrentStatus")
                        .HasColumnType("int");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsBot")
                        .HasColumnType("bit");

                    b.Property<string>("LanguageCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MiddlewareData")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RegistrationDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Status")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<string>("Username")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("BirthdayBot.DAL.Entities.Address", b =>
                {
                    b.HasOne("BirthdayBot.DAL.Entities.TUser", "User")
                        .WithMany("Addresses")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BirthdayBot.DAL.Entities.Address_ComponentConnector", b =>
                {
                    b.HasOne("BirthdayBot.DAL.Entities.Address_Component", "AddressComponent")
                        .WithMany("AddressComponentConnectors")
                        .HasForeignKey("AddressComponentId")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();

                    b.HasOne("BirthdayBot.DAL.Entities.Address", "Address")
                        .WithMany("AddressComponentConnectors")
                        .HasForeignKey("AddressId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BirthdayBot.DAL.Entities.ChatMember", b =>
                {
                    b.HasOne("BirthdayBot.DAL.Entities.Chat", "Chat")
                        .WithMany("ChatMembers")
                        .HasForeignKey("ChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BirthdayBot.DAL.Entities.TUser", "User")
                        .WithMany("ChatMembers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BirthdayBot.DAL.Entities.Note", b =>
                {
                    b.HasOne("BirthdayBot.DAL.Entities.TUser", "User")
                        .WithMany("Notes")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BirthdayBot.DAL.Entities.Subscription", b =>
                {
                    b.HasOne("BirthdayBot.DAL.Entities.TUser", "Subscriber")
                        .WithMany("Subscriptions")
                        .HasForeignKey("SubscriberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BirthdayBot.DAL.Entities.TUser", "Target")
                        .WithMany("Subscribers")
                        .HasForeignKey("TargetId")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BirthdayBot.DAL.Entities.TUser", b =>
                {
                    b.OwnsOne("BirthdayBot.DAL.Entities.GoogleTimeZone.UserTimezone", "Timezone", b1 =>
                        {
                            b1.Property<long>("TUserId")
                                .HasColumnType("bigint");

                            b1.Property<long>("DstOffset")
                                .HasColumnType("bigint");

                            b1.Property<long>("RawOffset")
                                .HasColumnType("bigint");

                            b1.Property<string>("TimeZoneId")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("TimeZoneName")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("TUserId");

                            b1.ToTable("Users");

                            b1.WithOwner()
                                .HasForeignKey("TUserId");
                        });

                    b.OwnsOne("BirthdayBot.DAL.Entities.UserLimitations", "Limitations", b1 =>
                        {
                            b1.Property<long>("TUserId")
                                .HasColumnType("bigint");

                            b1.Property<int>("ChangeLocationInputAttempts")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .HasDefaultValue(3);

                            b1.Property<DateTime?>("ChangeLocationInputBlockDate")
                                .HasColumnType("datetime2");

                            b1.Property<int>("StartLocationInputAttempts")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .HasDefaultValue(5);

                            b1.Property<DateTime?>("StartLocationInputBlockDate")
                                .HasColumnType("datetime2");

                            b1.HasKey("TUserId");

                            b1.ToTable("Users");

                            b1.WithOwner()
                                .HasForeignKey("TUserId");
                        });

                    b.OwnsOne("BirthdayBot.DAL.Entities.UserSettings", "Settings", b1 =>
                        {
                            b1.Property<long>("TUserId")
                                .HasColumnType("bigint");

                            b1.Property<int>("BirthDateConfidentiality")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .HasDefaultValue(0);

                            b1.Property<int>("BirthYearConfidentiality")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .HasDefaultValue(1);

                            b1.Property<int>("CommonNotification_0")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .HasDefaultValue(0);

                            b1.Property<int>("StrongNotification_0")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .HasDefaultValue(7);

                            b1.Property<int>("StrongNotification_1")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .HasDefaultValue(3);

                            b1.Property<int>("StrongNotification_2")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .HasDefaultValue(0);

                            b1.HasKey("TUserId");

                            b1.ToTable("Users");

                            b1.WithOwner()
                                .HasForeignKey("TUserId");
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
