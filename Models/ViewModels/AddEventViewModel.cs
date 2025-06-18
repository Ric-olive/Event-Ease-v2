using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Event_Ease.Models.ViewModels
{
    public class AddEventViewModel
    {
        [Required(ErrorMessage = "Event name is required")]
        [StringLength(100, ErrorMessage = "Event name must be between {2} and {1} characters", MinimumLength = 3)]
        [Display(Name = "Event Name")]
        public string EventName { get; set; }
        
        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime EventStartDate { get; set; }
        
        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EventEndDate { get; set; }
        
        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description must be between {2} and {1} characters", MinimumLength = 10)]
        public string Description { get; set; }
        
        [Display(Name = "Venue")]
        public Guid? VenueID { get; set; }
        
        [Display(Name = "Event Type")]
        public Guid? EventTypeID { get; set; }
        
        public List<SelectListItem> Venues { get; set; } = new List<SelectListItem>();
        
        public List<SelectListItem> EventTypes { get; set; } = new List<SelectListItem>();
    }
}