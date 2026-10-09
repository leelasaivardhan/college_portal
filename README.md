# Multi College Portal - Updated Version

This project is the updated ASP.NET Core MVC + SQL Server implementation based on the requested change document.

## Technology
- Visual Studio 2022
- .NET 8
- ASP.NET Core MVC
- Entity Framework Core 8
- SQL Server / SSMS 2016 compatible setup
- Cookie authentication
- ASP.NET Core PasswordHasher for password storage
- Responsive mobile-first UI

## What was changed

### Login
- Centered login card over a full-page college background image.
- Student, Faculty and Admin role selection on the same login screen.
- The selected role is checked against the database role before login succeeds.
- Demo credentials are NOT displayed on the login page.
- New-user message says to contact college administration.

### Student
- Today's timetable is moved into the highlighted hero area.
- The previous timetable area now contains a college image.
- Every sidebar/quick-action item has a working route.
- Academics, Examinations, Results, Attendance, Notices, Placements, Library, Hostel, Fee Payments, Academic Calendar and Downloads are implemented.
- Placement content is shown only when the student's semester is 7 or above.
- Library displays books issued against the student's college roll number.
- Hostel displays room information when the student is a hosteller.
- Fee Payments displays unpaid and paid fees.
- Academic Calendar displays two six-month semester periods.
- Downloads displays downloaded documents.

### Faculty
- Pending Actions is clickable and opens the pending-work page.
- Add Marks has Student, Subject and Assessment dropdowns.
- Assessments: Mid1, Mid2, Mid3, Mid4, Lab1, Lab2.
- Marks are restricted to a maximum of 30 both in the UI and server-side validation.
- Successful mark save displays a success popup.
- Attendance has Student, Date, Subject and Present/Absent controls.
- Successful attendance save displays a success popup.
- My Periods, Student List, Notices, Examination, Library and Dashboard all have working routes.

### Admin
- Multi-college switcher works through the admin dashboard.
- Five colleges are seeded for demonstration.
- Manage Content / Notices works.
- Add Period works and validates the selected faculty against the active college.
- Timetable Management works.
- College Management works.
- User Management and Add User work.
- Examination Management works.
- Reports works.
- The previous foreign-key problem caused by CollegeId = 0 is fixed by giving Admin an active college claim at login and validating the active college before writes.

### Background image
All authenticated pages and the login page use the same image:

`wwwroot/images/college-background.svg`

You can replace this file with the college image you want to use. If you replace it with JPG/PNG, update the CSS URL in `wwwroot/css/site.css` accordingly.

## Database

The application uses this database name:

`MultiCollegePortalDbV2`

Connection string is in `appsettings.json`:

`Server=localhost;Database=MultiCollegePortalDbV2;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true`

On first startup, Entity Framework creates the database and the application seeds the initial colleges and test users.

If your SQL Server is a named instance, for example SQL Express, change `Server=localhost` to `Server=.\\SQLEXPRESS`.

## Initial login credentials

These are intentionally documented here and NOT shown on the website login page.

Admin:
- Email: `admin@portal.local`
- Password: `Admin@123`

Student:
- Email: `student@portal.local`
- Password: `Student@123`

Faculty:
- Email: `faculty@portal.local`
- Password: `Faculty@123`

Passwords are stored as hashes in the database, not as plain text.

## Run in Visual Studio 2022

1. Extract the ZIP.
2. Open `MultiCollegePortal.sln`.
3. Confirm SQL Server is running.
4. If your SQL Server is not `localhost`, update the connection string in `appsettings.json`.
5. Build the solution.
6. Press F5 / Ctrl+F5.
7. The database `MultiCollegePortalDbV2` will be created automatically on first startup.
8. Login using one of the credentials above.

## If the database already exists

For a clean first run, delete `MultiCollegePortalDbV2` from SSMS and run the application again. The application uses a fresh V2 database name so it does not conflict with the older project database/schema.

## Important production note

The seeded accounts are only for initial development/testing. Before production deployment, create real accounts through Admin > User Management, change the initial admin password, use HTTPS, and configure a production SQL Server connection string.
