# Event Ease Booking Web App

## 📖 Overview
The **Event Booking Web App** is a modern platform designed to streamline event management. Whether you're hosting a small gathering or a large conference, this app provides users with an intuitive interface to browse, book, and manage events effortlessly.

---

## 🚀 Features
- **Event Listings**: Display detailed event information, including descriptions, dates, types, and venues.
- **Venue Listings & Management**: View, add, edit, and delete venues with image, location, capacity, and status. Image uploads are stored in Azure Blob Storage.
- **Event Management**: Tools for organizers to create, edit, and delete events, assign venues and event types.
- **Advanced Booking Filters**: Filter bookings by venue, event type, booking status (upcoming, in progress, completed), date range, and venue availability.
- **Personalized Dashboard**: Users can view and manage their bookings with advanced filtering and detailed event/venue info.
- **Event Types**: Events can be categorized (e.g., Conference, Workshop) and filtered accordingly.
- **Modern UI/UX**: Responsive Bootstrap design, floating action buttons, SweetAlert2 confirmations, and improved card/list layouts.
- **Database Seeding**: Automatic initialization of the database with sample data on first run.

---

## 🛠️ Technologies Used

- **Frontend**:
  - Bootstrap 5 (responsive UI)
  - HTML5 & CSS3
  - JavaScript (dynamic UI, SweetAlert2 for confirmations)
- **Backend**:
  - ASP.NET Core MVC (server-side framework)
  - Entity Framework Core (ORM, migrations, seeding)
  - LINQ (data querying)
  - Dependency Injection (service management)
- **Database**:
  - SQL Server (relational database)
  - Entity Framework Core Migrations (schema/version management)
- **Cloud & Storage**:
  - Azure App Services (cloud hosting)
  - Azure Blob Storage (media/image storage for venues)
- **Tooling**:
  - .NET 8 SDK
  - Visual Studio / Visual Studio Code
  - NuGet package manager

---

## 📂 Installation and Setup
Ensure you have the following installed:
- [.NET 8](https://dotnet.microsoft.com/download)
- A text editor like [Visual Studio](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- Access to the database connection details(SQL Server)

1. Clone the repository:
   ```bash
   git clone https://github.com/Ric-olive/Event-Ease-v2/tree/development 

2. Ensure you have the following Nuget Packages Installed
   - Microsoft.EntityFrameworkCore.SqlServer   v9.0.3
   - Microsoft.EntityFrameworkCore.Tools       v9.0.3

3. Replace the connection string in the appsettings.json with your own connection string 

4. Run "Update-Database" in the package manager console