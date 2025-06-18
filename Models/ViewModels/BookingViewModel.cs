using System;

namespace Event_Ease.Models.ViewModels
{
    public class BookingViewModel
    {
        // Booking Information
        public Guid BookingID { get; set; }
        public DateTime BookingDate { get; set; }
        
        // Event Information
        public Guid EventID { get; set; }
        public string EventName { get; set; }
        public DateTime EventStartDate { get; set; }
        public DateTime EventEndDate { get; set; }
        public string EventDescription { get; set; }
        
        // Event Type Information
        public Guid? EventTypeID { get; set; }
        public string EventTypeName { get; set; }
        
        // Venue Information
        public Guid VenueID { get; set; }
        public string VenueName { get; set; }
        public string VenueLocation { get; set; }
        public int VenueCapacity { get; set; }
        public string VenueImageUrl { get; set; }
        public bool VenueIsAvailable { get; set; }
        
        // Status Information
        public string BookingStatus { get; set; }
        public bool IsEventActive => DateTime.Now >= EventStartDate && DateTime.Now <= EventEndDate;
        public bool IsUpcoming => DateTime.Now < EventStartDate;
        public bool IsPast => DateTime.Now > EventEndDate;
        
        // Duration Information
        public int EventDurationDays => (int)(EventEndDate - EventStartDate).TotalDays + 1;
        public string EventDurationDisplay => EventDurationDays == 1 
            ? "1 day" 
            : $"{EventDurationDays} days";
            
        // Calculated Fields
        public int DaysToEvent => IsUpcoming ? (int)(EventStartDate - DateTime.Now).TotalDays : 0;
        public string TimeRemainingDisplay {
            get {
                if (IsEventActive) return "In Progress";
                if (IsPast) return "Completed";
                if (DaysToEvent == 0) return "Today";
                if (DaysToEvent == 1) return "Tomorrow";
                return $"In {DaysToEvent} days";
            }
        }
    }
}