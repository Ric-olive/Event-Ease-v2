using Event_Ease.Data;
using Event_Ease.Models.Entities;
using Event_Ease.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            try
            {
                var viewModel = new AddEventViewModel
                {
                    EventStartDate = DateTime.Today,
                    EventEndDate = DateTime.Today.AddDays(1),
                    Venues = _dbContext.Venues
                        .Where(v => v.IsActive) // Only active venues
                        .Select(v => new SelectListItem
                        {
                            Value = v.VenueID.ToString(),
                            Text = v.VenueName
                        }).ToList(),
                    EventTypes = _dbContext.EventTypes
                        .Where(et => et.IsActive)
                        .Select(et => new SelectListItem
                        {
                            Value = et.EventTypeID.ToString(),
                            Text = et.Name
                        })
                        .ToList()
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error loading Add Event page: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading the page. Please try again.";
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddEventViewModel viewModel)
        {
            try
            {
                // Reload venues for dropdown in case we need to return the view
                viewModel.Venues = await _dbContext.Venues
                    .Where(v => v.IsActive)
                    .Select(v => new SelectListItem
                    {
                        Value = v.VenueID.ToString(),
                        Text = v.VenueName
                    }).ToListAsync();
                
                viewModel.EventTypes = await _dbContext.EventTypes
                    .Where(et => et.IsActive)
                    .Select(et => new SelectListItem
                    {
                        Value = et.EventTypeID.ToString(),
                        Text = et.Name
                    })
                    .ToListAsync();
                
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please correct the validation errors below.";
                    return View(viewModel);
                }
                
                // Validate dates
                if (viewModel.EventStartDate > viewModel.EventEndDate)
                {
                    ModelState.AddModelError("EventEndDate", "End date must be after start date.");
                    TempData["ErrorMessage"] = "Event end date must be after start date.";
                    return View(viewModel);
                }
                
                // Validate venue is available for the selected dates
                if (viewModel.VenueID.HasValue)
                {
                    bool isVenueBooked = await _dbContext.Bookings
                        .Include(b => b.Event)
                        .AnyAsync(b =>
                            b.VenueID == viewModel.VenueID &&
                            b.Event.EventStartDate <= viewModel.EventEndDate &&
                            b.Event.EventEndDate >= viewModel.EventStartDate);

                    if (isVenueBooked)
                    {
                        ModelState.AddModelError("VenueID", "Venue is already booked during this time period.");
                        TempData["ErrorMessage"] = "The selected venue is already booked during this time period.";
                        return View(viewModel);
                    }
                }
                
                // Save the event
                var userEvent = new Event
                {
                    EventID = Guid.NewGuid(),
                    EventName = viewModel.EventName,
                    EventStartDate = viewModel.EventStartDate,
                    EventEndDate = viewModel.EventEndDate,
                    Description = viewModel.Description,
                    VenueID = viewModel.VenueID,
                    EventTypeID = viewModel.EventTypeID
                };
                
                await _dbContext.Events.AddAsync(userEvent);
                await _dbContext.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction("List", "Events");
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error creating event: {ex.Message}");
                
                // Reload venues dropdown
                viewModel.Venues = await _dbContext.Venues
                    .Where(v => v.IsActive)
                    .Select(v => new SelectListItem
                    {
                        Value = v.VenueID.ToString(),
                        Text = v.VenueName
                    }).ToListAsync();
                
                viewModel.EventTypes = await _dbContext.EventTypes
                    .Where(et => et.IsActive)
                    .Select(et => new SelectListItem
                    {
                        Value = et.EventTypeID.ToString(),
                        Text = et.Name
                    })
                    .ToListAsync();
                
                TempData["ErrorMessage"] = "An error occurred while processing your request. Please try again.";
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                var userEvents = await _dbContext.Events
                    .Include(e => e.Venue)
                    .ToListAsync();
                
                return View(userEvents);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error retrieving events: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while retrieving events. Please try again.";
                return View(new List<Event>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var userEvent = await _dbContext.Events.FindAsync(id);
                
                if (userEvent == null)
                {
                    TempData["ErrorMessage"] = "Event not found.";
                    return RedirectToAction("List");
                }
                
                // Pass the list of venues to the ViewBag
                ViewBag.Venues = await _dbContext.Venues
                    .Where(v => v.IsActive)
                    .Select(v => new SelectListItem
                    {
                        Value = v.VenueID.ToString(),
                        Text = v.VenueName,
                        Selected = v.VenueID == userEvent.VenueID
                    }).ToListAsync();
                
                ViewBag.EventTypes = await _dbContext.EventTypes
                    .Where(et => et.IsActive)
                    .Select(et => new SelectListItem
                    {
                        Value = et.EventTypeID.ToString(),
                        Text = et.Name,
                        Selected = et.EventTypeID == userEvent.EventTypeID
                    })
                    .ToListAsync();
                
                return View(userEvent);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error retrieving event for edit: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while retrieving the event. Please try again.";
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Event viewModel)
        {
            try
            {
                // Reload venues for the form in case validation fails
                ViewBag.Venues = await _dbContext.Venues
                    .Where(v => v.IsActive)
                    .Select(v => new SelectListItem
                    {
                        Value = v.VenueID.ToString(),
                        Text = v.VenueName,
                        Selected = v.VenueID == viewModel.VenueID
                    }).ToListAsync();
                
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please correct the validation errors below.";
                    return View(viewModel);
                }
                
                // Validate dates
                if (viewModel.EventStartDate > viewModel.EventEndDate)
                {
                    ModelState.AddModelError("EventEndDate", "End date must be after start date.");
                    TempData["ErrorMessage"] = "Event end date must be after start date.";
                    return View(viewModel);
                }
                
                var userEvent = await _dbContext.Events
                    .Include(e => e.Bookings)
                    .FirstOrDefaultAsync(e => e.EventID == viewModel.EventID);
                
                if (userEvent == null)
                {
                    TempData["ErrorMessage"] = "Event not found.";
                    return RedirectToAction("List");
                }
                
                // If venue is being changed, check for booking conflicts
                if (viewModel.VenueID.HasValue && viewModel.VenueID != userEvent.VenueID)
                {
                    bool isVenueBooked = await _dbContext.Bookings
                        .Include(b => b.Event)
                        .Where(b => b.EventID != userEvent.EventID) // Exclude current event's bookings
                        .AnyAsync(b =>
                            b.VenueID == viewModel.VenueID &&
                            b.Event.EventStartDate <= viewModel.EventEndDate &&
                            b.Event.EventEndDate >= viewModel.EventStartDate);

                    if (isVenueBooked)
                    {
                        ModelState.AddModelError("VenueID", "Venue is already booked during this time period.");
                        TempData["ErrorMessage"] = "The selected venue is already booked during this time period.";
                        return View(viewModel);
                    }
                }
                
                // Update the event
                userEvent.EventName = viewModel.EventName;
                userEvent.EventStartDate = viewModel.EventStartDate;
                userEvent.EventEndDate = viewModel.EventEndDate;
                userEvent.Description = viewModel.Description;
                userEvent.VenueID = viewModel.VenueID;
                userEvent.EventTypeID = viewModel.EventTypeID;
                
                await _dbContext.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Event updated successfully!";
                return RedirectToAction("List", "Events");
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error updating event: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while updating the event. Please try again.";
                return View(viewModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // Find the event and include related bookings
                var eventItem = await _dbContext.Events
                    .Include(e => e.Bookings)
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
                
                // Proceed with deletion
                _dbContext.Events.Remove(eventItem);
                await _dbContext.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Event successfully deleted.";
                return RedirectToAction("List", "Events");
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error deleting event: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while deleting the event. Please try again.";
                return RedirectToAction("List", "Events");
            }
        }
    }
}