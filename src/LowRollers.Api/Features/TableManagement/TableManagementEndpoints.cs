using LowRollers.Api.Features.TableManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace LowRollers.Api.Features.TableManagement;

/// <summary>
/// Minimal API endpoints for table management
/// </summary>
public static class TableManagementEndpoints
{
    /// <summary>
    /// Maps all table management endpoints
    /// </summary>
    public static IEndpointRouteBuilder MapTableManagementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tables")
            .WithTags("Tables");

        // POST /api/tables - Create a new table
        group.MapPost("/", CreateTableAsync)
            .WithName("CreateTable")
            .WithSummary("Create a new poker table")
            .WithDescription("Creates a new poker table with the specified configuration. Returns invite code and session token for the host.");

        // GET /api/tables/validate/{code} - Validate invite code
        group.MapGet("/validate/{code}", ValidateInviteCodeAsync)
            .WithName("ValidateInviteCode")
            .WithSummary("Validate an invite code")
            .WithDescription("Validates an invite code and returns basic table information if valid.");

        // GET /api/tables/{sessionId} - Get table state
        group.MapGet("/{sessionId:guid}", GetTableStateAsync)
            .WithName("GetTableState")
            .WithSummary("Get table state")
            .WithDescription("Returns the current state of a table for an authorized player.");

        return app;
    }

    private static async Task<IResult> CreateTableAsync(
        [FromBody] CreateTableRequest request,
        [FromServices] ITableManagementService tableService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        // Build base URL from request
        var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";

        var result = await tableService.CreateTableAsync(request, baseUrl, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                TableManagementErrorCode.InvalidDisplayName => Results.BadRequest(new { error = result.ErrorMessage }),
                TableManagementErrorCode.InvalidRequest => Results.BadRequest(new { error = result.ErrorMessage }),
                _ => Results.Problem(result.ErrorMessage, statusCode: 500)
            };
        }

        return Results.Created($"/api/tables/{result.Data!.SessionId}", result.Data);
    }

    private static async Task<IResult> ValidateInviteCodeAsync(
        string code,
        [FromServices] ITableManagementService tableService,
        CancellationToken cancellationToken)
    {
        var result = await tableService.ValidateInviteCodeAsync(code, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                TableManagementErrorCode.InvalidInviteCode => Results.NotFound(new { error = result.ErrorMessage }),
                _ => Results.Problem(result.ErrorMessage, statusCode: 500)
            };
        }

        return Results.Ok(result.Data);
    }

    private static async Task<IResult> GetTableStateAsync(
        Guid sessionId,
        [FromHeader(Name = "X-Player-Id")] Guid? playerId,
        [FromServices] ITableManagementService tableService,
        CancellationToken cancellationToken)
    {
        if (!playerId.HasValue)
        {
            return Results.Unauthorized();
        }

        var result = await tableService.GetTableStateAsync(sessionId, playerId.Value, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                TableManagementErrorCode.SessionNotFound => Results.NotFound(new { error = result.ErrorMessage }),
                TableManagementErrorCode.NotAuthorized => Results.Forbid(),
                _ => Results.Problem(result.ErrorMessage, statusCode: 500)
            };
        }

        return Results.Ok(result.Data);
    }
}
