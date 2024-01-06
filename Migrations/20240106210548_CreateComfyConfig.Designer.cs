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
    [Migration("20240106210548_CreateComfyConfig")]
    partial class CreateComfyConfig
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.4");

            modelBuilder.Entity("ComfyConfig", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.ToTable("ComfyConfigs");
                });

            modelBuilder.Entity("GuildSettings", b =>
                {
                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong?>("ReminderChannel")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Timezone")
                        .HasColumnType("TEXT");

                    b.HasKey("GuildId");

                    b.ToTable("GuildSettings");
                });

            modelBuilder.Entity("ScheduledEvent", b =>
                {
                    b.Property<ulong>("ScheduledEventId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<long?>("EndTime")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("Recurrence")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RecurrenceValue")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("ReminderDuration")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("StartTime")
                        .HasColumnType("INTEGER");

                    b.HasKey("ScheduledEventId");

                    b.HasIndex("GuildId");

                    b.ToTable("ScheduledEvents");
                });

            modelBuilder.Entity("ScheduledEventOccurrence", b =>
                {
                    b.Property<ulong>("ScheduledEventOccurrenceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsReminder")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ScheduledEventId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("When")
                        .HasColumnType("INTEGER");

                    b.HasKey("ScheduledEventOccurrenceId");

                    b.HasIndex("ScheduledEventId");

                    b.HasIndex("When");

                    b.ToTable("ScheduledEventOccurences");
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

            modelBuilder.Entity("ScheduledEventOccurrence", b =>
                {
                    b.HasOne("ScheduledEvent", "ScheduledEvent")
                        .WithMany()
                        .HasForeignKey("ScheduledEventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ScheduledEvent");
                });
#pragma warning restore 612, 618
        }
    }
}
