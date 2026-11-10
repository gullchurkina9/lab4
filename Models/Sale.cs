namespace SalesStatisticsWpf.Models;

public sealed class Sale
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
    public DateTime Date { get; init; }
}
