/* Optional manual database creation for SSMS. The application can create it automatically. */
IF DB_ID(N'MultiCollegePortalDbV2') IS NULL
BEGIN
    CREATE DATABASE MultiCollegePortalDbV2;
END
GO
USE MultiCollegePortalDbV2;
GO
-- Do not manually create the tables here. The ASP.NET Core application uses EF Core EnsureCreatedAsync on first startup.
