using System.ComponentModel.DataAnnotations;

namespace LowRollers.Api.Features.TableManagement.Models;

/// <summary>
/// Request to create a new table
/// </summary>
public sealed class CreateTableRequest
{
    /// <summary>
    /// Display name of the host (2-20 characters)
    /// </summary>
    [Required]
    [StringLength(20, MinimumLength = 2)]
    public string HostDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Name of the table
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Optional password for the table
    /// </summary>
    [StringLength(100)]
    public string? Password { get; set; }

    /// <summary>
    /// Table configuration (uses defaults if not provided)
    /// </summary>
    public TableConfig? Config { get; set; }
}
