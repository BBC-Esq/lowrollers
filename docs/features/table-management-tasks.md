# Implementation Tasks: Table Management

**Feature:** Table Management
**Spec:** [table-management-spec.md](./table-management-spec.md)
**Phase:** 1 (MVP)

---

## Database Setup

- [x] **Set up SQL Server Database Project**
  Task ID: `table-mgmt-00`
  > **Implementation**: Created `src/LowRollers.Database` project
  > **Details**:
  > - Added SQL Server Database Project to solution
  > - Created all tables matching spec:
  >   - `Games` - Lookup table (Texas Holdem)
  >   - `Players` - Global player registry
  >   - `GameTables` - Reusable table templates
  >   - `GameSessions` - Active game instances (with InviteCodeHash, PasswordHash)
  >   - `GameSessionPlayers` - Player-session junction (with TimeBankSeconds, ChipStack, IsHost)
  >   - `SessionTransactions` - Buy-in/cash-out tracking (with SessionId FK)
  >   - `SessionHands` - Hand containers
  >   - `SessionHandEvents` - Event sourcing (NULL PlayerId for system events)
  >   - `TableTemplates` - Saved configurations
  > - Added Entity Framework Core with SQL Server provider (Aspire integration)
  > - Created DbContext with all entity configurations
  > - Configured Aspire AppHost to use SQL Server

---

## API Endpoints

- [x] **Create table management API endpoints**
  Task ID: `table-mgmt-01`
  > **Implementation**: Created `src/LowRollers.Api/Features/TableManagement/`
  > **Details**:
  > - Created models: `TableConfig.cs`, `CreateTableRequest.cs`, `CreateTableResponse.cs`, `TableStateResponse.cs`
  > - Created `InviteCodeGenerator.cs` - Crypto-secure 8-char codes with SHA256 hashing
  > - Created `SessionTokenService.cs` - HMAC-SHA256 signed tokens (temporary, replaced by Firebase Auth)
  > - Created `TableManagementService.cs` with BCrypt password hashing (workFactor: 12)
  > - Created `TableManagementEndpoints.cs` - Minimal API endpoints at `/api/tables`
  > - Added `builder.Services.AddValidation()` for automatic DataAnnotation validation
  > - Added `TableStatus` enum for compile-time safety (Lobby, Active, Paused, Closed)
  > - TableConfig includes validation rules per HOST-TABLE-002 through HOST-TABLE-005

- [ ] **Implement join table flow**
  Task ID: `table-mgmt-02`
  > **Implementation**: Extend `src/LowRollers.Api/Features/TableManagement/`
  > **Details**:
  > - `ValidateInviteQuery.cs` - Check invite code hash exists, session active
  > - `JoinTableCommand.cs` + `JoinTableHandler.cs`
  >   - Validate display name (2-20 chars, unique at session, not banned)
  >   - Create/find player in global `Players` registry
  >   - Create `GameSessionPlayers` record linking player to session
  >   - Create guest session in Redis
  >   - Generate JWT token (SessionId, PlayerId, DisplayName)
  >   - Broadcast player joined via SignalR
  > - Return session token

- [ ] **Implement leave table flow**
  Task ID: `table-mgmt-03`
  > **Implementation**: Extend `src/LowRollers.Api/Features/TableManagement/`
  > **Details**:
  > - `LeaveTableCommand.cs`
  >   - Remove player from table
  >   - If host leaving, transfer to longest-seated
  >   - If last player, close table
  >   - Clean up Redis session

- [ ] **Implement seat management**
  Task ID: `table-mgmt-04`
  > **Implementation**: Create `src/LowRollers.Api/Features/TableManagement/Seating/`
  > **Details**:
  > - `TakeSeatCommand.cs` - Assign seat position (1-10)
  > - `StandUpCommand.cs` - Release seat, keep in lobby
  > - Validate seat not already taken
  > - Broadcast seat changes via SignalR

---

## Invite System

- [ ] **Implement invite link system**
  Task ID: `table-mgmt-05`
  > **Implementation**: Create `src/LowRollers.Api/Features/TableManagement/Invites/`
  > **Details**:
  > - `InviteCodeGenerator.cs`
  >   - Use `RandomNumberGenerator` for crypto-secure codes
  >   - 8-12 alphanumeric characters
  >   - Store hash, not plain text
  > - `RegenerateInviteCommand.cs`
  >   - Generate new code
  >   - Invalidate old code immediately
  >   - Broadcast to connected players

---

## Authentication

- [ ] **Implement Firebase Anonymous Authentication**
  Task ID: `table-mgmt-05a`
  > **Implementation**: Integrate Firebase Auth for anonymous user identity
  > **Details**:
  > - **Firebase Setup**:
  >   - Create Firebase project (or use existing)
  >   - Enable Anonymous Authentication provider
  >   - Add Firebase config to Angular environment files
  > - **Angular Integration** (`src/LowRollers.Web/`):
  >   - Install `@angular/fire` and `firebase` packages
  >   - Configure Firebase in `app.config.ts`
  >   - Create `AuthService` with `signInAnonymously()` on app init
  >   - Store Firebase ID token, send with API requests
  >   - Handle token refresh automatically via Firebase SDK
  > - **API Integration** (`src/LowRollers.Api/`):
  >   - Install `FirebaseAdmin` NuGet package
  >   - Create `FirebaseAuthService.cs` - Initialize Firebase Admin SDK
  >   - Create `FirebaseAuthHandler.cs` - Validate ID tokens on requests
  >   - Update `Players` table: Add `FirebaseUid VARCHAR(128) UNIQUE`
  >   - Update `GetOrCreatePlayerAsync()` to use FirebaseUid instead of display name
  > - **Database Migration**:
  >   - Add `FirebaseUid` column to `Players` table
  >   - Create index on `FirebaseUid` for fast lookups
  > - **Remove**:
  >   - Remove custom `SessionTokenService` (replaced by Firebase)
  >   - Update endpoints to use Firebase token validation

---

## Guest Session Management

- [ ] **Implement guest session service**
  Task ID: `table-mgmt-06`
  > **Implementation**: Create `src/LowRollers.Api/Features/Sessions/`
  > **Depends on**: `table-mgmt-05a` (Firebase Auth)
  > **Details**:
  > - `GuestSession.cs` - Redis model (references PlayerId from global Players)
  > - `GuestSessionService.cs`
  >   - Create session in Redis with PlayerId reference
  >   - 5-minute reconnection window
  >   - Use Firebase ID token for authentication (no custom JWT)
  > - `PlayerService.cs` - Create/find players by FirebaseUid

- [ ] **Implement reconnection logic**
  Task ID: `table-mgmt-07`
  > **Implementation**: Extend `src/LowRollers.Api/Features/Sessions/`
  > **Details**:
  > - `ReconnectCommand.cs`
  >   - Validate session token still in Redis
  >   - Restore player to same seat
  >   - Restore chip stack
  >   - Rejoin SignalR group
  > - Handle edge case: hand completed during disconnect

---

## Host Controls

- [ ] **Implement host privileges**
  Task ID: `table-mgmt-08`
  > **Implementation**: Create `src/LowRollers.Api/Features/TableManagement/HostControls/`
  > **Details**:
  > - `KickPlayerCommand.cs` - Remove player immediately
  > - `BanPlayerCommand.cs` - Add display name to ban list
  > - `TransferHostCommand.cs` - Assign new host
  > - `StartGameCommand.cs` - Begin dealing (min 2 players)
  > - `PauseGameCommand.cs` - Pause game (between hands)
  > - `StopGameCommand.cs` - End game session

- [ ] **Implement auto host transfer**
  Task ID: `table-mgmt-09`
  > **Implementation**: Extend `src/LowRollers.Api/Features/TableManagement/`
  > **Details**:
  > - On host disconnect (5-min timeout), auto-transfer
  > - Transfer to longest-seated player
  > - Broadcast host change to all players
  > - Log host transfers for audit

---

## Chip Management

- [ ] **Implement buy-in system**
  Task ID: `table-mgmt-10`
  > **Implementation**: Create `src/LowRollers.Api/Features/ChipManagement/`
  > **Details**:
  > - `BuyInCommand.cs`
  >   - Validate amount within table limits (min/max from TableConfig)
  >   - Create `SessionTransactions` record (IsCredit=1)
  >   - Update `GameSessionPlayers.ChipStack`
  >   - Allowed before sitting or between hands
  > - `RebuyCommand.cs`
  >   - Add chips during session
  >   - Same limit validation
  >   - Create `SessionTransactions` record
  > - `CashOutCommand.cs`
  >   - Record cash-out in `SessionTransactions` (IsCredit=0)
  > - Cache current chip balance in Redis for real-time access

---

## SignalR Integration

- [ ] **Create table SignalR hub methods**
  Task ID: `table-mgmt-11`
  > **Implementation**: Extend `src/LowRollers.Api/Hubs/GameHub.cs`
  > **Details**:
  > - `JoinTableGroup(tableId)` - Add connection to SignalR group
  > - `LeaveTableGroup(tableId)` - Remove from group
  > - Broadcasts:
  >   - `PlayerJoined(PlayerInfo)`
  >   - `PlayerLeft(Guid playerId)`
  >   - `PlayerSeated(Guid playerId, int position)`
  >   - `HostChanged(Guid newHostId)`
  >   - `TableSettingsUpdated(TableSettings)`

---

## Angular Components

> **DESIGN APPROVAL REQUIRED**: Before implementing any UI component below, either:
> 1. Agent presents a generated mockup for user approval, OR
> 2. User provides a screenshot/design reference to emulate
>
> Approved designs are stored in `docs/designs/` for implementation reference.

- [ ] **Create table creation page**
  Task ID: `table-mgmt-12`
  > **Implementation**: Create `src/LowRollers.Web/src/app/features/table/create-table/`
  > **Details**:
  > - `create-table.component.ts`
  > - Form fields:
  >   - Table name
  >   - Host display name
  >   - Basic settings (blinds, buy-in limits)
  > - Generate invite link on submit
  > - Copy link button
  > - Use PrimeNG InputText, Button, Card

- [ ] **Create join table page**
  Task ID: `table-mgmt-13`
  > **Implementation**: Create `src/LowRollers.Web/src/app/features/table/join-table/`
  > **Details**:
  > - `join-table.component.ts`
  > - Route: `/join/:inviteCode`
  > - Validate invite code on load
  > - Display name input (prefill from localStorage)
  > - Error handling for invalid/expired codes
  > - Use PrimeNG InputText, Button, Message

- [ ] **Create table lobby component**
  Task ID: `table-mgmt-14`
  > **Implementation**: Create `src/LowRollers.Web/src/app/features/table/table-lobby/`
  > **Details**:
  > - `table-lobby.component.ts`
  > - Display current players
  > - Seat selection grid
  > - Buy-in dialog trigger
  > - Host controls (if host): Start Game button
  > - Invite link display/copy
  > - Use PrimeNG DataView, Button, Dialog

- [ ] **Create buy-in dialog**
  Task ID: `table-mgmt-15`
  > **Implementation**: Create `src/LowRollers.Web/src/app/features/table/buy-in-dialog/`
  > **Details**:
  > - `buy-in-dialog.component.ts`
  > - Slider for buy-in amount
  > - Show min/max limits
  > - Quick buttons (min, default, max)
  > - Use PrimeNG Dialog, Slider, InputNumber

- [ ] **Create host controls panel**
  Task ID: `table-mgmt-16`
  > **Implementation**: Create `src/LowRollers.Web/src/app/features/table/host-controls/`
  > **Details**:
  > - `host-controls.component.ts`
  > - Only visible to host
  > - Buttons: Start Game, Pause, Stop
  > - Player list with kick/ban options
  > - Regenerate invite link
  > - Use PrimeNG Button, ConfirmDialog, Menu

---

## Testing

- [ ] **Create table management tests**
  Task ID: `table-mgmt-17`
  > **Implementation**: Create `tests/LowRollers.Api.Tests/Features/TableManagement/`
  > **Details**:
  > - Create table generates valid invite code
  > - Join with valid code succeeds
  > - Join with invalid code fails
  > - Display name validation (length, uniqueness, banned)
  > - Kick/ban functionality
  > - Host transfer logic

- [ ] **Create session management tests**
  Task ID: `table-mgmt-18`
  > **Implementation**: Create `tests/LowRollers.Api.Tests/Features/Sessions/`
  > **Details**:
  > - Session creation stores in Redis
  > - Session expiry after timeout
  > - Reconnection within window succeeds
  > - Reconnection after window fails
  > - JWT validation

---

## Completion Checklist

- [ ] Create table with invite link working
- [ ] Join via invite link working
- [ ] Display name validation (2-20 chars, unique, not banned)
- [ ] Guest sessions persist correctly
- [ ] Reconnection within 5 minutes works
- [ ] Host can kick/ban players
- [ ] Host transfer on disconnect works
- [ ] Buy-in within limits enforced
- [ ] All API endpoints secured with session auth

---

*Generated by Clavix /clavix:plan*
