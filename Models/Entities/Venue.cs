using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Ease.Models.Entities
{
    public class Venue
    {
        public Guid VenueID { get; set; }
        public string VenueName { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsAvailable { get; set; } = true;
        
        [NotMapped]
        public IFormFile ImageFile { get; set; }



        // Navigation property for related events
        public ICollection<Event> Events { get; set; } = new List<Event>();

        // Navigation property for related bookings
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}

