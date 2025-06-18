using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Event_Ease.Models.ViewModels
{
    public class AddBookingViewModel
    {
        public Guid BookingID { get; set; }
        
        [Required(ErrorMessage = "Please select an event")]
        [Display(Name = "Event")]
        public Guid EventID { get; set; }
        
        public Guid VenueID { get; set; }
        
        [Required(ErrorMessage = "Booking date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; }
        
        // Dropdown lists
        public List<SelectListItem> Events { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Venues { get; set; } = new List<SelectListItem>();
    }
}