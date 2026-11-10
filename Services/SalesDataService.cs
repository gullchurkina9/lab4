using SalesStatisticsWpf.Models;

namespace SalesStatisticsWpf.Services;

/// <summary>
/// Демонстрирует асинхронную загрузку исходных данных.
/// В реальном проекте задержку можно заменить чтением файла или запросом к API/БД.
/// </summary>
public sealed class SalesDataService
{
    public async Task<(List<Product> Products, List<Sale> Sales)> LoadAsync(
        bool includeInvalidSale, CancellationToken cancellationToken)
    {
        // Имитация длительной операции ввода-вывода без блокировки UI-потока.
        await Task.Delay(1200, cancellationToken);

        var products = new List<Product>
        {
            new() { Id = 1, Name = "Ноутбук", Category = "Электроника", Price = 85000m },
            new() { Id = 2, Name = "Наушники", Category = "Электроника", Price = 7500m },
            new() { Id = 3, Name = "Кофемашина", Category = "Бытовая техника", Price = 24000m },
            new() { Id = 4, Name = "Пылесос", Category = "Бытовая техника", Price = 18000m },
            new() { Id = 5, Name = "Рюкзак", Category = "Аксессуары", Price = 4200m },
            new() { Id = 6, Name = "Мышь", Category = "Аксессуары", Price = 1900m }
        };

        var today = DateTime.Today;
        var sales = new List<Sale>
        {
            new() { ProductId = 1, Quantity = 2, Date = today.AddDays(-4) },
            new() { ProductId = 2, Quantity = 5, Date = today.AddDays(-3) },
            new() { ProductId = 3, Quantity = 1, Date = today.AddDays(-3) },
            new() { ProductId = 4, Quantity = 3, Date = today.AddDays(-2) },
            new() { ProductId = 5, Quantity = 4, Date = today.AddDays(-1) },
            new() { ProductId = 6, Quantity = 10, Date = today },
            new() { ProductId = 1, Quantity = 1, Date = today },
            new() { ProductId = 2, Quantity = 2, Date = today }
        };

        if (includeInvalidSale)
            sales.Add(new Sale { ProductId = 999, Quantity = 1, Date = today });

        cancellationToken.ThrowIfCancellationRequested();
        return (products, sales);
    }
}
