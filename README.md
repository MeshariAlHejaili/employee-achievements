# Employee Achievements System

A web application for managing and tracking employee achievements within an organization. Built with ASP.NET Core MVC and SQL Server LocalDB.

## Quick Setup

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)

### Database Setup

1. **Install SQL Server LocalDB** (if not already installed)
2. **Create Database**: Open SQL Server Management Studio or Azure Data Studio
3. **Connect to**: `(localdb)\mssqllocaldb`
4. **Create Database**: `EmployeeAchievementsDB`
5. **Run Script**: Execute the `current_seed.sql` file against the database

### Run Application

```bash
dotnet restore
dotnet run
```

The application will be available at `https://localhost:7000` or `http://localhost:5000`

## Sample Users

- **mashari@amana.com** (password: 123456) - Software Development
- **sara@amana.com** (password: 123456) - Product Management  
- **ahmed@amana.com** (password: 123456) - Software Development
- **manager1@amana.com** (password: test123) - Software Development Manager
- **manager2@amana.com** (password: test123) - Product Management Manager