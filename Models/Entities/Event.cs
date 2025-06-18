namespace Event_Ease.Models.Entities
{
    public class Event
    {
        public Guid EventID { get; set; } // Primary key
        public string EventName { get; set; } // Event name
        public DateTime EventStartDate { get; set; } // Event date
        public DateTime EventEndDate { get; set; } // Event date
        public string Description { get; set; }//Description of Event

        // Nullable foreign key for Venue
        // Navigation property (optional)
        public Guid? VenueID { get; set; }
        public Venue? Venue { get; set; }
        
        // Foreign key for EventType
        public Guid? EventTypeID { get; set; }
        public EventType? EventType { get; set; }

        // Navigation property for related bookings
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}

