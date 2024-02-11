using System;
using Cuemon.Extensions.IO;
using Cuemon.Extensions.Text.Json.Formatters;
using Microsoft.EntityFrameworkCore;

namespace Wish.JournalAzureSqlServer
{
    public static class ModelBuilderExtensions
    {
        public static ModelBuilder AddJournal(this ModelBuilder mb)
        {
            mb.Entity<Owner>(entity =>
            {
                entity.ToTable(nameof(Owner))
                    .HasKey(e => e.Id);
                entity.Property(o => o.EmailAddress)
                    .HasColumnName("emailAddress")
                    .HasColumnType("varchar(512)")
                    .IsRequired();
                entity.Property(o => o.Created)
                    .HasColumnName("created")
                    .HasColumnType("datetime")
                    .IsRequired();
                entity.Property(o => o.Modified)
                    .HasColumnName("modified")
                    .HasColumnType("datetime")
                    .IsRequired(false);
                entity.HasMany(o => o.Journals)
                    .WithOne()
                    .HasForeignKey(j => j.OwnerId);
            });

            mb.Entity<Journal>(entity =>
            {
                entity.ToTable(nameof(Journal))
                    .HasKey(j => j.Id);
                entity.Property(j => j.OwnerId)
                    .HasColumnName("ownerId")
                    .HasColumnType("uniqueidentifier")
                    .IsRequired();
                entity.Property(j => j.Title)
                    .HasColumnName("title")
                    .HasColumnType("varchar(256)")
                    .IsRequired();
                entity.Property(j => j.Description)
                    .HasColumnName("description")
                    .HasColumnType("varchar(1024)")
                    .IsRequired(false);
                entity.Property(j => j.Created)
                    .HasColumnName("created")
                    .HasColumnType("datetime")
                    .IsRequired();
                entity.Property(j => j.Modified)
                    .HasColumnName("modified")
                    .HasColumnType("datetime")
                    .IsRequired(false);
                entity.HasMany(j => j.Entries)
                    .WithOne()
                    .HasForeignKey(je => je.JournalId);
            });

            var setup = new Action<JsonFormatterOptions>(o => o.Settings.WriteIndented = false);

            mb.Entity<JournalEntry>(entity =>
            {
                entity.ToTable(nameof(JournalEntry))
                    .HasKey(entry => entry.Id);
                entity.Property(entry => entry.JournalId)
                    .HasColumnName("journalId")
                    .HasColumnType("uniqueidentifier")
                    .IsRequired();
                entity.Property(entry => entry.TimeZone)
                    .HasColumnName("timezone")
                    .HasColumnType("varchar(256)")
                    .IsRequired();
                entity.Property(entry => entry.Coordinates)
                    .HasColumnName("coordinates")
                    .HasColumnType("varchar(128)")
                    .IsRequired()
                    .HasConversion(coordinates => JsonFormatter.SerializeObject(coordinates, typeof(Coordinates), setup).ToEncodedString(null), 
                        json => JsonFormatter.DeserializeObject<Coordinates>(json.ToStream(null)));
                entity.Property(entry => entry.Location)
                    .HasColumnName("location")
                    .HasColumnType("varchar(1024)")
                    .IsRequired()
                    .HasConversion(location => JsonFormatter.SerializeObject(location, typeof(Location), setup).ToEncodedString(null), 
                        json => JsonFormatter.DeserializeObject<Location>(json.ToStream(null)));
                entity.Property(entry => entry.Weather)
                    .HasColumnName("weather")
                    .HasColumnType("varchar(1024)")
                    .IsRequired()
                    .HasConversion(weather => JsonFormatter.SerializeObject(weather, typeof(Weather), setup).ToEncodedString(null), 
                        json => JsonFormatter.DeserializeObject<Weather>(json.ToStream(null)));
                entity.Property(entry => entry.Notes)
                    .HasColumnName("notes")
                    .HasColumnType("varchar(2048)")
                    .IsRequired(false);
                entity.Property(entry => entry.Created)
                    .HasColumnName("created")
                    .HasColumnType("datetimeoffset(7)")
                    .IsRequired();
                entity.Property(entry => entry.Modified)
                    .HasColumnName("modified")
                    .HasColumnType("datetimeoffset(7)")
                    .IsRequired(false);
            });

            return mb;
        }
    }
}
