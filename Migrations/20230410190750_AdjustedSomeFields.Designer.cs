﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ComfyBot.Migrations
{
    [DbContext(typeof(ComfyContext))]
    [Migration("20230410190750_AdjustedSomeFields")]
    partial class AdjustedSomeFields
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.4")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true);

            modelBuilder.Entity("GuildSettings", b =>
                {
                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong?>("ReminderChannel")
                        .HasColumnType("INTEGER");

                    b.HasKey("GuildId");

                    b.ToTable("GuildSettings");
                });

            modelBuilder.Entity("ScheduledEvent", b =>
                {
                    b.Property<long>("ScheduledEventId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("End")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("Recurrence")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RecurrenceValue")
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan?>("Reminder")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("Start")
                        .HasColumnType("TEXT");

                    b.HasKey("ScheduledEventId");

                    b.HasIndex("GuildId");

                    b.ToTable("ScheduledEvents");
                });

            modelBuilder.Entity("ScheduledEvent", b =>
                {
                    b.HasOne("GuildSettings", "GuildSettings")
                        .WithMany()
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GuildSettings");
                });
#pragma warning restore 612, 618
        }
    }
}
