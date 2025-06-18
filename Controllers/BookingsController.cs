using Event_Ease.Data;
using Event_Ease.Models.Entities;
using Event_Ease.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Ease.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public BookingsController(ApplicationDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Add()
        {
            var events = _dbContext.Events
                .Include(e => e.Venue)
                .Where(e => e.Venue != null)
                .Select(e => new SelectListItem
                {
                    Value = e.EventID.ToString(),
                    Text = e.EventName
                }).ToList();

            var viewModel = new AddBookingViewModel
            {
                BookingDate = DateTime.Today,
                Events = events
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddBookingViewModel viewModel)
        {
            try
            {
                // Reload events for dropdown in case we need to return the view
                var events = await _dbContext.Events
                    .Include(e => e.Venue)
                    .Where(e => e.Venue != null)
                    .Select(e => new SelectListItem
                    {
                        Value = e.EventID.ToString(),
                        Text = e.EventName
                    }).ToListAsync();

                viewModel.Events = events;

                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please correct the validation errors below.";
                    return View(viewModel);
                }

                // Fetch the event to retrieve the linked VenueID
                var eventEntity = await _dbContext.Events
                    .Include(e => e.Venue)
                    .FirstOrDefaultAsync(e => e.EventID == viewModel.EventID);

                if (eventEntity == null || eventEntity.Venue == null)
                {
                    TempData["ErrorMessage"] = "Invalid event or venue selection.";
                    return View(viewModel);
                }

                // Get the venue ID from the event
                var venueId = eventEntity.Venue.VenueID;

                // Check if the venue is already booked for the event's date range
                var isVenueBooked = await IsVenueBookedDuringEventAsync(
                    venueId, 
                    eventEntity.EventStartDate, 
                    eventEntity.EventEndDate, 
                    null // No booking ID since this is a new booking
                );

                if (isVenueBooked)
                {
                    ModelState.AddModelError("", 
                        "This venue is already booked for another event during this time period.");
                    TempData["ErrorMessage"] = "Venue booking conflict detected. Please choose another venue or date.";
                    return View(viewModel);
                }

                // Map data to Booking entity
                var booking = new Booking
                {
                    BookingID = Guid.NewGuid(),
                    BookingDate = viewModel.BookingDate,
                    EventID = viewModel.EventID,
                    VenueID = venueId
                };

                await _dbContext.Bookings.AddAsync(booking);
                await _dbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Booking created successfully!";
                return RedirectToAction("List", "Bookings");
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error creating booking: {ex.Message}");
                
                // Reload events dropdown
                viewModel.Events = await _dbContext.Events
                    .Include(e => e.Venue)
                    .Where(e => e.Venue != null)
                    .Select(e => new SelectListItem
                    {
                        Value = e.EventID.ToString(),
                        Text = e.EventName
                    }).ToListAsync();
                
                TempData["ErrorMessage"] = "An error occurred while processing your request. Please try again.";
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> List(string searchTerm = "", string venueFilter = "", string statusFilter = "", string eventTypeFilter = "", DateTime? startDate = null, DateTime? endDate = null, bool? venueAvailabilityFilter = null)
        {
            try
            {
                // Get all bookings with related data 
                var bookingsQuery = _dbContext.Bookings
                    .Include(b => b.Event)
                    .ThenInclude(e => e.EventType)
                    .Include(b => b.Venue);

                // Build view models
                var bookingViewModels = await bookingsQuery
                    .Select(b => new BookingViewModel
                    {
                        // Booking Info
                        BookingID = b.BookingID,
                        BookingDate = b.BookingDate,
                        
                        // Event Info
                        EventID = b.EventID,
                        EventName = b.Event.EventName,
                        EventStartDate = b.Event.EventStartDate,
                        EventEndDate = b.Event.EventEndDate,
                        EventDescription = b.Event.Description,
                        
                        // Event Type Info
                        EventTypeID = b.Event.EventTypeID,
                        EventTypeName = b.Event.EventType != null ? b.Event.EventType.Name : "Not Specified",
                        
                        // Venue Info
                        VenueID = b.VenueID,
                        VenueName = b.Venue.VenueName,
                        VenueLocation = b.Venue.Location,
                        VenueCapacity = b.Venue.Capacity,
                        VenueImageUrl = b.Venue.ImageUrl,
                        VenueIsAvailable = b.Venue.IsAvailable,
                        
                        // Status
                        BookingStatus = b.Event.EventStartDate > DateTime.Now 
                            ? "Upcoming" 
                            : (b.Event.EventEndDate < DateTime.Now ? "Completed" : "In Progress")
                    })
                    .ToListAsync();

                // Apply search filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    bookingViewModels = bookingViewModels.Where(b => 
                        b.BookingID.ToString().Contains(searchTerm) || 
                        b.EventName.ToLower().Contains(searchTerm) ||
                        b.VenueName.ToLower().Contains(searchTerm)
                    ).ToList();
                }
                
                // Apply venue filter
                if (!string.IsNullOrEmpty(venueFilter) && venueFilter != "all")
                {
                    bookingViewModels = bookingViewModels.Where(b => 
                        b.VenueName.Equals(venueFilter, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                // Apply status filter
                if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
                {
                    bookingViewModels = bookingViewModels.Where(b => 
                        b.BookingStatus.Equals(statusFilter, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }
                
                // Apply event type filter
                if (!string.IsNullOrEmpty(eventTypeFilter) && eventTypeFilter != "all")
                {
                    bookingViewModels = bookingViewModels.Where(b => 
                        b.EventTypeName != null && b.EventTypeName.Equals(eventTypeFilter, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }
                
                // Apply date range filter
                if (startDate.HasValue)
                {
                    bookingViewModels = bookingViewModels.Where(b => 
                        b.EventStartDate >= startDate.Value
                    ).ToList();
                }
                
                if (endDate.HasValue)
                {
                    bookingViewModels = bookingViewModels.Where(b => 
                        b.EventEndDate <= endDate.Value
                    ).ToList();
                }
                
                // Apply venue availability filter
                if (venueAvailabilityFilter.HasValue)
                {
                    bookingViewModels = bookingViewModels.Where(b => 
                        b.VenueIsAvailable == venueAvailabilityFilter.Value
                    ).ToList();
                }

                // Get unique venues for filter dropdown
                ViewBag.Venues = bookingViewModels
                    .Select(b => b.VenueName)
                    .Distinct()
                    .OrderBy(v => v)
                    .ToList();
                    
                // Get all event types from the database for filter dropdown
                ViewBag.EventTypes = await _dbContext.EventTypes
                    .Where(et => et.IsActive)
                    .Select(et => et.Name)
                    .OrderBy(name => name)
                    .ToListAsync();

                // Store the search parameters for the view
                ViewBag.SearchTerm = searchTerm;
                ViewBag.VenueFilter = venueFilter;
                ViewBag.StatusFilter = statusFilter;
                ViewBag.EventTypeFilter = eventTypeFilter;
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                ViewBag.VenueAvailabilityFilter = venueAvailabilityFilter;
                
                return View(bookingViewModels);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error retrieving bookings: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while retrieving bookings. Please try again.";
                return View(new List<BookingViewModel>());
            }
        }

        // The rest of your controller methods remain the same...

        // Helper method to check if venue is already booked during the specified time period
        private async Task<bool> IsVenueBookedDuringEventAsync(Guid venueId, DateTime startDate, DateTime endDate, Guid? excludeBookingId)
        {
            var query = _dbContext.Bookings
                .Include(b => b.Event)
                .Where(b => 
                    b.VenueID == venueId &&
                    b.Event.EventStartDate <= endDate && 
                    b.Event.EventEndDate >= startDate);

            // Exclude the current booking if we're editing
            if (excludeBookingId.HasValue)
            {
                query = query.Where(b => b.BookingID != excludeBookingId.Value);
            }

            return await query.AnyAsync();
        }
        
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                // Make sure we're loading booking with all related data
                var booking = await _dbContext.Bookings
                    .Include(b => b.Event)
                    .Include(b => b.Venue)
                    .FirstOrDefaultAsync(b => b.BookingID == id);
                
                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Booking not found.";
                    return RedirectToAction("List");
                }

                // Load events for dropdown
                ViewBag.Events = await _dbContext.Events
                    .Select(e => new SelectListItem
                    {
                        Value = e.EventID.ToString(),
                        Text = e.EventName,
                        Selected = e.EventID == booking.EventID
                    }).ToListAsync();

                return View(booking);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Edit GET: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while retrieving the booking.";
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Booking viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Reload dropdown data
                    ViewBag.Events = await _dbContext.Events
                        .Select(e => new SelectListItem
                        {
                            Value = e.EventID.ToString(),
                            Text = e.EventName,
                            Selected = e.EventID == viewModel.EventID
                        }).ToListAsync();

                    return View(viewModel);
                }

                var booking = await _dbContext.Bookings
                    .Include(b => b.Event)
                    .FirstOrDefaultAsync(b => b.BookingID == viewModel.BookingID);

                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Booking not found.";
                    return RedirectToAction("List");
                }

                // Get the event to obtain the venue ID
                var selectedEvent = await _dbContext.Events
                    .Include(e => e.Venue)
                    .FirstOrDefaultAsync(e => e.EventID == viewModel.EventID);

                if (selectedEvent == null || selectedEvent.Venue == null)
                {
                    TempData["ErrorMessage"] = "The selected event or venue is not valid.";
                    
                    // Reload dropdown data
                    ViewBag.Events = await _dbContext.Events
                        .Select(e => new SelectListItem
                        {
                            Value = e.EventID.ToString(),
                            Text = e.EventName,
                            Selected = e.EventID == viewModel.EventID
                        }).ToListAsync();
                    
                    return View(viewModel);
                }

                // Update properties
                booking.BookingDate = viewModel.BookingDate;
                booking.EventID = viewModel.EventID;
                booking.VenueID = selectedEvent.Venue.VenueID; // Get venue from selected event

                await _dbContext.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Booking updated successfully.";
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Edit POST: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while updating the booking.";
                
                // Reload dropdown data
                ViewBag.Events = await _dbContext.Events
                    .Select(e => new SelectListItem
                    {
                        Value = e.EventID.ToString(),
                        Text = e.EventName,
                        Selected = e.EventID == viewModel.EventID
                    }).ToListAsync();
                
                return View(viewModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var booking = await _dbContext.Bookings
                    .Include(b => b.Event)
                    .FirstOrDefaultAsync(b => b.BookingID == id);
                
                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Booking not found or already deleted.";
                    return RedirectToAction("List");
                }

                // Check if event is in progress
                var now = DateTime.Now;
                if (booking.Event != null && 
                    booking.Event.EventStartDate <= now && 
                    booking.Event.EventEndDate >= now)
                {
                    TempData["ErrorMessage"] = "Cannot delete a booking for an event that is currently in progress.";
                    return RedirectToAction("List");
                }

                _dbContext.Bookings.Remove(booking);
                await _dbContext.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Booking deleted successfully.";
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Delete: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while deleting the booking.";
                return RedirectToAction("List");
            }
        }
    }
}