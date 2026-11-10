namespace SalesStatisticsWpf.Models;

public sealed class CategoryStatistic
{
    public string Category { get; init; } = "";
    public decimal Revenue { get; init; }
    public int SalesCount { get; init; }
    public int UnitsSold { get; init; }
}
