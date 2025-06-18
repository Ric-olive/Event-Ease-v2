using Event_Ease.Data;
using Event_Ease.Models.Entities;
using Event_Ease.Models.ViewModels;
using Event_Ease.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Event_Ease.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly BlobStorageService _blobStorageService;

        public VenuesController(ApplicationDbContext dbContext, BlobStorageService blobStorageService)
        {
            this._dbContext = dbContext;
            this._blobStorageService = blobStorageService;
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddVenueViewModel viewModel)
        {
            // Debug information
            Console.WriteLine($"Form submitted: {viewModel.VenueName}, File: {viewModel.ImageFile?.FileName ?? "No file"}");
             try
    {
        // Check for model validation errors excluding ImageUrl
        if (!ModelState.IsValid)
        {
            // If the only error is for ImageUrl, we can ignore it
            var errors = ModelState.Where(x => x.Key != "ImageUrl" && x.Value.Errors.Any()).ToList();
            
            if (errors.Any())
            {
                return View(viewModel);
            }
        }

        // Check file
        if (viewModel.ImageFile == null || viewModel.ImageFile.Length == 0)
        {
            ModelState.AddModelError("ImageFile", "Please upload an image file");
            return View(viewModel);
        }

        // Upload file to blob storage
        string imageUrl;
        try
        {
            Console.WriteLine($"Uploading file: {viewModel.ImageFile.FileName}, Length: {viewModel.ImageFile.Length}");
            imageUrl = await _blobStorageService.UploadImageAsync(viewModel.ImageFile);
            Console.WriteLine($"File uploaded, URL: {imageUrl}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Upload failed: {ex.Message}");
            ModelState.AddModelError("ImageFile", $"Failed to upload image: {ex.Message}");
            return View(viewModel);
        }

        // Create venue entity
        var venue = new Venue
        {
            VenueID = Guid.NewGuid(),
            VenueName = viewModel.VenueName,
            Capacity = viewModel.Capacity,
            Location = viewModel.Location,
            ImageUrl = imageUrl,  // Set ImageUrl from the uploaded file
            Description = viewModel.Description,
            IsActive = viewModel.IsActive,
        };

        // Save to database
        Console.WriteLine($"Saving venue to database: {venue.VenueName}, ImageUrl: {venue.ImageUrl}");
        await _dbContext.Venues.AddAsync(venue);
        var saveResult = await _dbContext.SaveChangesAsync();
        Console.WriteLine($"Save result: {saveResult} records affected");

        if (saveResult > 0)
        {
            TempData["SuccessMessage"] = "Venue created successfully!";
            return RedirectToAction("List", "Venues");
        }
        else
        {
            ModelState.AddModelError("", "Failed to save venue to database");
            return View(viewModel);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception in Add Venue: {ex.Message}");
        ModelState.AddModelError("", $"An unexpected error occurred: {ex.Message}");
        return View(viewModel);
    }
           
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var venues = await _dbContext.Venues.ToListAsync();
            return View(venues);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var venue = await _dbContext.Venues.FindAsync(id);
            
            if (venue == null)
            {
                return View(null);
            }

            // Convert to view model
            var viewModel = new Venue
            {
                VenueID = venue.VenueID,
                VenueName = venue.VenueName,
                Location = venue.Location,
                Capacity = venue.Capacity,
                ImageUrl = venue.ImageUrl,
                Description = venue.Description,
                IsActive = venue.IsActive
            };
            
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Venue viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var venue = await _dbContext.Venues.FindAsync(viewModel.VenueID);
            
            if(venue is not null)
            {
                venue.VenueName = viewModel.VenueName;
                venue.Location = viewModel.Location;
                venue.Description = viewModel.Description;
                venue.IsActive = viewModel.IsActive;
                venue.Capacity = viewModel.Capacity;

                // Handle image update only if new image is uploaded
                if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
                {
                    // If there's an existing image, delete it first
                    if (!string.IsNullOrEmpty(venue.ImageUrl))
                    {
                        // Only delete if it's a blob storage URL (not an external URL)
                        if (venue.ImageUrl.Contains("blob.core.windows.net"))
                        {
                            await _blobStorageService.DeleteImageAsync(venue.ImageUrl);
                        }
                    }
                    
                    // Upload new image and update URL
                    venue.ImageUrl = await _blobStorageService.UploadImageAsync(viewModel.ImageFile);
                }

                await _dbContext.SaveChangesAsync();
            }
            
            return RedirectToAction("List", "Venues");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var venue = await _dbContext.Venues
                    .Include(v => v.Bookings) // Include bookings
                    .Include(v => v.Events)   // Include events
                    .FirstOrDefaultAsync(v => v.VenueID == id);
            
                // Check if venue is null
                if (venue == null)
                {
                    TempData["ErrorMessage"] = "Venue not found.";
                    return RedirectToAction("List", "Venues"); 
                }
        
                // Check if venue has active bookings
                if (venue.Bookings.Any())
                {
                    TempData["ErrorMessage"] = "Cannot delete a venue linked to active bookings.";
                    return RedirectToAction("List", "Venues"); 
                }
        
                // Check if venue has associated events
                if (venue.Events.Any())
                {
                    TempData["ErrorMessage"] = "Cannot delete a venue linked to events. Remove the events first.";
                    return RedirectToAction("List", "Venues");
                }

                // Delete image from blob storage if it exists
                if (!string.IsNullOrEmpty(venue.ImageUrl) && venue.ImageUrl.Contains("blob.core.windows.net"))
                {
                    await _blobStorageService.DeleteImageAsync(venue.ImageUrl);
                }

                // Proceed with deletion
                _dbContext.Venues.Remove(venue);
                await _dbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Venue successfully deleted.";
                return RedirectToAction("List", "Venues"); 
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error deleting venue: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while deleting the venue. Please try again.";
                return RedirectToAction("List", "Venues");
            }
        }
    }
}