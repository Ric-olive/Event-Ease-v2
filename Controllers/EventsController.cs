using Event_Ease.Data;
using Event_Ease.Models.Entities;
using Event_Ease.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Event_Ease.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public EventsController(ApplicationDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Add()
        {
            var viewModel = new AddEventViewModel
            {
                Venues = _dbContext.Venues
                .Select(v => new SelectListItem
                {
                    Value = v.VenueID.ToString(),
                    Text = v.VenueName
                }).ToList()
             };


            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddEventViewModel viewModel)
        {
   
            // Save the event if validation passes
            var Userevent = new Event
            {
                EventName = viewModel.EventName,
                EventStartDate = viewModel.EventStartDate,
                EventEndDate = viewModel.EventEndDate,
                Description = viewModel.Description,
                VenueID = viewModel.VenueID,
            };

            await _dbContext.Events.AddAsync(Userevent);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("List", "Events");

        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var UserEvents = await _dbContext.Events.Include(e=>e.Venue).ToListAsync();
            return View(UserEvents);
        }

        [HttpGet]
        public async Task<IActionResult>Edit(Guid id)
        {
            var UserEvent = await _dbContext.Events.FindAsync(id);
            // Pass the list of venues to the ViewBag
            ViewBag.Venues = _dbContext.Venues.Select(v => new SelectListItem
            {
                Value = v.VenueID.ToString(),
                Text = v.VenueName
            }).ToList();
            return View(UserEvent);
        }

        public async Task<IActionResult> Edit(Event viewModel)
        {
            var UserEvent = await _dbContext.Events.FindAsync(viewModel.EventID);
            if (UserEvent is not null)
            {
                UserEvent.EventName = viewModel.EventName;
                UserEvent.EventStartDate = viewModel.EventStartDate;
                UserEvent.EventEndDate = viewModel.EventEndDate;
                UserEvent.Description = viewModel.Description;
                UserEvent.VenueID = viewModel.VenueID; // Update the venue

                // Save changes to the database
                await _dbContext.SaveChangesAsync();

            }
            return RedirectToAction("List", "Events");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Find the event and include related bookings
            var eventItem = await _dbContext.Events
                .Include(e => e.Bookings) // Include bookings for validation
                .FirstOrDefaultAsync(e => e.EventID == id);

            // Check if the event exists
            if (eventItem == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction("List", "Events");
            }

            // Check if the event has active bookings
            if (eventItem.Bookings.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete an event linked to active bookings.";
                return RedirectToAction("List", "Events");
            }

            // Proceed with deletion if no bookings are linked
            _dbContext.Events.Remove(eventItem);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event successfully deleted.";
            return RedirectToAction("List", "Events");
        }

    }
}
