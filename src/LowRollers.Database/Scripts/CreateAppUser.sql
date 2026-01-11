-- Create application login and user with least-privilege permissions
-- This script should be run after database creation

USE [lowrollers];
GO

-- Create login if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'lowrollers_app')
BEGIN
    CREATE LOGIN [lowrollers_app] WITH PASSWORD = '$(AppUserPassword)';
END
GO

-- Create user if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'lowrollers_app')
BEGIN
    CREATE USER [lowrollers_app] FOR LOGIN [lowrollers_app];
END
GO

-- Grant only necessary permissions
ALTER ROLE db_datareader ADD MEMBER [lowrollers_app];
ALTER ROLE db_datawriter ADD MEMBER [lowrollers_app];

-- If using stored procedures, grant execute
-- GRANT EXECUTE TO [lowrollers_app];

-- Explicitly deny dangerous permissions
DENY ALTER TO [lowrollers_app];
DENY CONTROL TO [lowrollers_app];

PRINT 'Application user [lowrollers_app] configured successfully';
GO
