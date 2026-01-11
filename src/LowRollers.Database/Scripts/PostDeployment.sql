/*
Post-Deployment Script
----------------------
This script runs after the database schema is deployed.
It seeds required lookup data and system records.
*/

-- Seed Games lookup table
IF NOT EXISTS (SELECT 1 FROM dbo.Games WHERE GameId = 1)
BEGIN
    INSERT INTO dbo.Games (GameId, GameName)
    VALUES (1, 'Texas Holdem');
END

-- Seed Dealer system player (used for system events in SessionHandEvents)
IF NOT EXISTS (SELECT 1 FROM dbo.Players WHERE PlayerId = '00000000-0000-0000-0000-000000000000')
BEGIN
    SET IDENTITY_INSERT dbo.Players OFF;
    INSERT INTO dbo.Players (PlayerId, PlayerName)
    VALUES ('00000000-0000-0000-0000-000000000000', 'Dealer');
END

GO
