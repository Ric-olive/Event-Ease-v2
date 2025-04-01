using Event_Ease.Data;
using Event_Ease.Models.Entities;
using Event_Ease.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Event_Ease.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        public VenuesController(ApplicationDbContext dbContext)
        {
          
            this._dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddVenueViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                // Return the view with the validation errors
                return View(viewModel);
            }

            var venue = new Venue
            {
                VenueName = viewModel.VenueName,
                Capacity = viewModel.Capacity,
                Location = viewModel.Location,
                ImageUrl = viewModel.ImageUrl,
                Description = viewModel.Description,
                IsActive = viewModel.IsActive,
            };

            await _dbContext.Venues.AddAsync(venue);
            await _dbContext.SaveChangesAsync();
            
            return RedirectToAction("List", "Venues");
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
          var venues =  await _dbContext.Venues.ToListAsync();
           
          return View(venues);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
           var venue = await _dbContext.Venues.FindAsync(id);

            return View(venue);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Venue viewModel)
        {
           var venue= await _dbContext.Venues.FindAsync(viewModel.VenueID);

            if(venue is not null)
            {
                venue.VenueName = viewModel.VenueName;
                venue.Location = viewModel.Location;
                venue.ImageUrl = viewModel.ImageUrl;
                venue.Description = viewModel.Description;
                venue.IsActive = viewModel.IsActive;
                venue.Capacity = viewModel.Capacity;

                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("List","Venues");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var venue = await _dbContext.Venues
        .Include(v => v.Bookings) // Ensure Bookings are included in the query
        .FirstOrDefaultAsync(v => v.VenueID == id);

            // Check if venue is null
            if (venue == null)
            {
                TempData["ErrorMessage"] = "Venue not found.";
                return RedirectToAction("List", "Venues"); // Redirect back to the list view
            }

            // Check if venue has active bookings
            if (venue.Bookings.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete a venue linked to active bookings.";
                return RedirectToAction("List", "Venues"); // Redirect back to the list view
            }

            // Proceed with deletion
            _dbContext.Venues.Remove(venue);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Venue successfully deleted.";
            return RedirectToAction("List", "Venues"); // Redirect back to the list view
        }

    }
}
