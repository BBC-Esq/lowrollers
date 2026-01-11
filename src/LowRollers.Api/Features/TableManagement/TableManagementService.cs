using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LowRollers.Api.Data;
using LowRollers.Api.Data.Entities;
using LowRollers.Api.Features.TableManagement.Invites;
using LowRollers.Api.Features.TableManagement.Models;
using LowRollers.Api.Features.TableManagement.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LowRollers.Api.Features.TableManagement;

/// <summary>
/// Implementation of table management operations
/// </summary>
public sealed partial class TableManagementService : ITableManagementService
{
    private const byte TexasHoldemGameId = 1;

    private readonly LowRollersDbContext _dbContext;
    private readonly IInviteCodeGenerator _inviteCodeGenerator;
    private readonly ISessionTokenService _sessionTokenService;
    private readonly ILogger<TableManagementService> _logger;

    public TableManagementService(
        LowRollersDbContext dbContext,
        IInviteCodeGenerator inviteCodeGenerator,
        ISessionTokenService sessionTokenService,
        ILogger<TableManagementService> logger)
    {
        _dbContext = dbContext;
        _inviteCodeGenerator = inviteCodeGenerator;
        _sessionTokenService = sessionTokenService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TableManagementResult<CreateTableResponse>> CreateTableAsync(
        CreateTableRequest request,
        string baseUrl,
        CancellationToken cancellationToken = default)
    {
        // Validate display name
        var displayNameValidation = ValidateDisplayName(request.HostDisplayName);
        if (!displayNameValidation.IsValid)
        {
            return TableManagementResult.Failure<CreateTableResponse>(
                displayNameValidation.ErrorMessage!,
                TableManagementErrorCode.InvalidDisplayName);
        }

        try
        {
            // Create table config and validate
            var config = request.Config ?? TableConfig.CreateDefault();
            var (isValid, configErrors) = config.Validate();
            if (!isValid)
            {
                return TableManagementResult.Failure<CreateTableResponse>(
                    string.Join("; ", configErrors),
                    TableManagementErrorCode.InvalidRequest);
            }

            // Create or find player in global registry
            var player = await GetOrCreatePlayerAsync(request.HostDisplayName, cancellationToken);

            var configJson = JsonSerializer.Serialize(config);

            // Create GameTable record
            var gameTable = new GameTable
            {
                TableName = request.TableName,
                GameId = TexasHoldemGameId,
                TableOwner = player.PlayerId,
                CreatedBy = player.PlayerId,
                ModifiedBy = player.PlayerId,
                TableConfig = configJson
            };

            _dbContext.GameTables.Add(gameTable);

            // Generate invite code
            var (inviteCode, inviteCodeHash) = _inviteCodeGenerator.GenerateInviteCode();

            // Hash password if provided
            string? passwordHash = null;
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                passwordHash = HashPassword(request.Password);
            }

            // Create GameSession record
            var gameSession = new GameSession
            {
                TableId = gameTable.TableId,
                InviteCodeHash = inviteCodeHash,
                PasswordHash = passwordHash
            };

            _dbContext.GameSessions.Add(gameSession);

            // Create GameSessionPlayer record for host
            var sessionPlayer = new GameSessionPlayer
            {
                SessionId = gameSession.SessionId,
                PlayerId = player.PlayerId,
                SeatNumber = 0, // Standing/lobby initially
                ChipStack = 0,
                IsHost = true,
                TimeBankSeconds = config.TimeBankEnabled ? config.TimeBankSeconds : 0
            };

            _dbContext.GameSessionPlayers.Add(sessionPlayer);

            await _dbContext.SaveChangesAsync(cancellationToken);

            LogTableCreated(gameTable.TableId, gameSession.SessionId, player.PlayerId);

            // Generate session token for host
            var sessionToken = _sessionTokenService.GenerateToken(
                gameSession.SessionId,
                player.PlayerId,
                request.HostDisplayName);

            // Build invite URL
            var inviteUrl = $"{baseUrl.TrimEnd('/')}/join/{inviteCode}";

            return TableManagementResult.Success(new CreateTableResponse
            {
                TableId = gameTable.TableId,
                SessionId = gameSession.SessionId,
                InviteCode = inviteCode,
                InviteUrl = inviteUrl,
                SessionToken = sessionToken,
                PlayerId = player.PlayerId
            });
        }
        catch (Exception ex)
        {
            LogTableCreationError(ex.Message);
            return TableManagementResult.Failure<CreateTableResponse>(
                "Failed to create table",
                TableManagementErrorCode.InternalError);
        }
    }

    /// <inheritdoc />
    public async Task<TableManagementResult<TableStateResponse>> GetTableStateAsync(
        Guid sessionId,
        Guid playerId,
        CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.GameSessions
            .Include(s => s.GameTable)
            .Include(s => s.GameSessionPlayers)
                .ThenInclude(sp => sp.Player)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);

        if (session == null)
        {
            return TableManagementResult.Failure<TableStateResponse>(
                "Session not found",
                TableManagementErrorCode.SessionNotFound);
        }

        // Check if player is part of this session
        var isPlayerInSession = session.GameSessionPlayers.Any(sp => sp.PlayerId == playerId);
        if (!isPlayerInSession)
        {
            return TableManagementResult.Failure<TableStateResponse>(
                "Not authorized to view this table",
                TableManagementErrorCode.NotAuthorized);
        }

        var config = JsonSerializer.Deserialize<TableConfig>(session.GameTable.TableConfig) ?? new TableConfig();
        var hostPlayer = session.GameSessionPlayers.FirstOrDefault(sp => sp.IsHost);

        var response = new TableStateResponse
        {
            TableId = session.TableId,
            SessionId = session.SessionId,
            TableName = session.GameTable.TableName,
            Status = session.EndedOn.HasValue ? TableStatus.Closed : TableStatus.Lobby,
            Config = config,
            HostPlayerId = hostPlayer?.PlayerId ?? Guid.Empty,
            GameInProgress = false, // TODO: Check in-memory table state
            Players = session.GameSessionPlayers
                .Where(sp => sp.DepartedOn == null)
                .Select(sp => new TablePlayerInfo
                {
                    PlayerId = sp.PlayerId,
                    DisplayName = sp.Player.PlayerName,
                    SeatNumber = sp.SeatNumber,
                    ChipStack = sp.ChipStack,
                    IsHost = sp.IsHost,
                    IsConnected = true // TODO: Check connection manager
                })
                .ToList()
        };

        return TableManagementResult.Success(response);
    }

    /// <inheritdoc />
    public async Task<TableManagementResult<InviteValidationResponse>> ValidateInviteCodeAsync(
        string inviteCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(inviteCode))
        {
            return TableManagementResult.Failure<InviteValidationResponse>(
                "Invite code is required",
                TableManagementErrorCode.InvalidInviteCode);
        }

        // Hash the invite code to search
        var inviteCodeHash = _inviteCodeGenerator.HashInviteCode(inviteCode);

        var session = await _dbContext.GameSessions
            .Include(s => s.GameTable)
            .Include(s => s.GameSessionPlayers)
            .FirstOrDefaultAsync(s => s.InviteCodeHash == inviteCodeHash && s.EndedOn == null, cancellationToken);

        if (session == null)
        {
            LogInvalidInviteCode(inviteCode[..Math.Min(4, inviteCode.Length)]);
            return TableManagementResult.Failure<InviteValidationResponse>(
                "Invalid or expired invite code",
                TableManagementErrorCode.InvalidInviteCode);
        }

        var config = JsonSerializer.Deserialize<TableConfig>(session.GameTable.TableConfig) ?? new TableConfig();
        var activePlayers = session.GameSessionPlayers.Count(sp => sp.DepartedOn == null);

        return TableManagementResult.Success(new InviteValidationResponse
        {
            SessionId = session.SessionId,
            TableName = session.GameTable.TableName,
            CurrentPlayers = activePlayers,
            MaxPlayers = config.MaxSeats,
            RequiresPassword = !string.IsNullOrEmpty(session.PasswordHash),
            GameInProgress = false // TODO: Check in-memory table state
        });
    }

    private async Task<Player> GetOrCreatePlayerAsync(string displayName, CancellationToken cancellationToken)
    {
        // Try to find existing player with same name
        var existingPlayer = await _dbContext.Players
            .FirstOrDefaultAsync(p => p.PlayerName == displayName, cancellationToken);

        if (existingPlayer != null)
        {
            return existingPlayer;
        }

        // Create new player
        var newPlayer = new Player
        {
            PlayerName = displayName
        };

        _dbContext.Players.Add(newPlayer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogPlayerCreated(newPlayer.PlayerId, displayName);
        return newPlayer;
    }

    private static (bool IsValid, string? ErrorMessage) ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return (false, "Display name is required");
        }

        if (displayName.Length < 2)
        {
            return (false, "Display name must be at least 2 characters");
        }

        if (displayName.Length > 20)
        {
            return (false, "Display name must be at most 20 characters");
        }

        return (true, null);
    }

    // Use BCrypt.Net-Next package for secure password hashing
    private static string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

    internal static bool VerifyPassword(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);

    [LoggerMessage(Level = LogLevel.Information, Message = "Table created: TableId={TableId}, SessionId={SessionId}, HostPlayerId={HostPlayerId}")]
    private partial void LogTableCreated(Guid tableId, Guid sessionId, Guid hostPlayerId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create table: {Error}")]
    private partial void LogTableCreationError(string error);

    [LoggerMessage(Level = LogLevel.Information, Message = "Player created: PlayerId={PlayerId}, Name={DisplayName}")]
    private partial void LogPlayerCreated(Guid playerId, string displayName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid invite code attempted: {CodePrefix}****")]
    private partial void LogInvalidInviteCode(string codePrefix);
}
