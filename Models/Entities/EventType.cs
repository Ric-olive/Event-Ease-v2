using System.ComponentModel.DataAnnotations;

namespace Event_Ease.Models.Entities
{
    public class EventType
    {
        public Guid EventTypeID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation property for related events
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
