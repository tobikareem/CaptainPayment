using System.ComponentModel.DataAnnotations;

namespace CaptainPayment.Core.Models;

/// <summary>
/// Options for transforming the quantity before billing
/// </summary>
public class TransformQuantityOptions
{
    /// <summary>
    /// Divide usage by this number. Must be greater than 0.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "DivideBy must be greater than 0")]
    public int DivideBy { get; set; }

    /// <summary>
    /// After division, either round the result up or down.
    /// Valid values: "up" or "down"
    /// </summary>
    [RegularExpression("^(up|down)$", ErrorMessage = "Round must be either 'up' or 'down'")]
    public string Round { get; set; } = "up";
}