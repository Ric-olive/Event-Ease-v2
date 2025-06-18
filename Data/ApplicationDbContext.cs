using Event_Ease.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Event_Ease.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<EventType> EventTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the Event -> Venue relationship
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Venue)
                .WithMany(v => v.Events)
                .HasForeignKey(e => e.VenueID)
                .OnDelete(DeleteBehavior.SetNull); // Set VenueID to null if Venue is deleted
                
            // Configure the Event -> EventType relationship
            modelBuilder.Entity<Event>()
                .HasOne(e => e.EventType)
                .WithMany(et => et.Events)
                .HasForeignKey(e => e.EventTypeID)
                .OnDelete(DeleteBehavior.SetNull); // Set EventTypeID to null if EventType is deleted
                                                   // Event -> Booking relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventID)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of Event if Bookings exist

            // Venue -> Booking relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Venue)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VenueID)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of Venue if Bookings exist
        }


    }
}
