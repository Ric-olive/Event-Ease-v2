using System.ComponentModel.DataAnnotations;

namespace Event_Ease.Models.ViewModels
{   //This model will be userd to bind the data to the Add venue Form
    public class AddVenueViewModel
    {
        [Required(ErrorMessage = "Venue name is required")]
        public string VenueName { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 10000, ErrorMessage = "Capacity must be between 1 and 10,000")]
        public int Capacity { get; set; }

        // Make ImageUrl optional - remove any [Required] attribute
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Please upload an image for the venue")]
        public IFormFile ImageFile { get; set; }
    }
}
