using Event_Ease.Data;
using Event_Ease.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Event_Ease.Controllers
{
    public class EventTypesController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public EventTypesController(ApplicationDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var eventTypes = await _dbContext.EventTypes.ToListAsync();
                return View(eventTypes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error retrieving event types: {ex.Message}";
                return View(new List<EventType>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(EventType eventType)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    eventType.EventTypeID = Guid.NewGuid();
                    await _dbContext.EventTypes.AddAsync(eventType);
                    await _dbContext.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Event type created successfully.";
                    return RedirectToAction("Index");
                }
                return View(eventType);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating event type: {ex.Message}";
                return View(eventType);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var eventType = await _dbContext.EventTypes.FindAsync(id);
                if (eventType == null)
                {
                    TempData["ErrorMessage"] = "Event type not found.";
                    return RedirectToAction("Index");
                }
                return View(eventType);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error retrieving event type: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EventType eventType)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _dbContext.EventTypes.Update(eventType);
                    await _dbContext.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Event type updated successfully.";
                    return RedirectToAction("Index");
                }
                return View(eventType);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating event type: {ex.Message}";
                return View(eventType);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var eventType = await _dbContext.EventTypes.FindAsync(id);
                if (eventType == null)
                {
                    TempData["ErrorMessage"] = "Event type not found.";
                    return RedirectToAction("Index");
                }

                // Check if there are any events using this event type
                var hasEvents = await _dbContext.Events.AnyAsync(e => e.EventTypeID == id);
                if (hasEvents)
                {
                    TempData["ErrorMessage"] = "Cannot delete event type because it is being used by one or more events.";
                    return RedirectToAction("Index");
                }

                _dbContext.EventTypes.Remove(eventType);
                await _dbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Event type deleted successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting event type: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
