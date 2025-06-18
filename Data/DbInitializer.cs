using Event_Ease.Models.Entities;

namespace Event_Ease.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context)
        {
            // Create default event types if none exist
            if (!context.EventTypes.Any())
            {
                var eventTypes = new List<EventType>
                {
                    new EventType
                    {
                        EventTypeID = Guid.NewGuid(),
                        Name = "Conference",
                        Description = "Professional gathering for discussion, learning, and networking",
                        IsActive = true
                    },
                    new EventType
                    {
                        EventTypeID = Guid.NewGuid(),
                        Name = "Wedding",
                        Description = "Ceremony where two people are united in marriage",
                        IsActive = true
                    },
                    new EventType
                    {
                        EventTypeID = Guid.NewGuid(),
                        Name = "Corporate Meeting",
                        Description = "Business gathering for employees, stakeholders, or clients",
                        IsActive = true
                    },
                    new EventType
                    {
                        EventTypeID = Guid.NewGuid(),
                        Name = "Birthday Party",
                        Description = "Celebration of a person's birth anniversary",
                        IsActive = true
                    },
                    new EventType
                    {
                        EventTypeID = Guid.NewGuid(),
                        Name = "Exhibition",
                        Description = "Public display of items or information",
                        IsActive = true
                    },
                    new EventType
                    {
                        EventTypeID = Guid.NewGuid(),
                        Name = "Workshop",
                        Description = "Interactive session focused on skill development",
                        IsActive = true
                    },
                    new EventType
                    {
                        EventTypeID = Guid.NewGuid(),
                        Name = "Concert",
                        Description = "Musical performance before an audience",
                        IsActive = true
                    }
                };

                await context.EventTypes.AddRangeAsync(eventTypes);
                await context.SaveChangesAsync();
            }
        }
    }
}
