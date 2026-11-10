using System.Collections.Concurrent;
using SalesStatisticsWpf.Models;

namespace SalesStatisticsWpf.Services;

/// <summary>Проверяет данные и рассчитывает показатели категорий параллельно через TPL.</summary>
public sealed class SalesStatisticsService
{
    private sealed class Aggregate
    {
        public decimal Revenue;
        public int SalesCount;
        public int UnitsSold;
    }

    public async Task<List<CategoryStatistic>> CalculateAsync(
        IReadOnlyCollection<Product> products,
        IReadOnlyCollection<Sale> sales,
        IProgress<int>? progress,
        CancellationToken cancellationToken)
    {
        // Проверки целостности выполняются до параллельного этапа.
        var productById = products.ToDictionary(p => p.Id);
        foreach (var sale in sales)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!productById.ContainsKey(sale.ProductId))
                throw new InvalidOperationException(
                    $"Продажа ссылается на несуществующий продукт (ID={sale.ProductId}).");
            if (sale.Quantity <= 0)
                throw new InvalidOperationException("Количество в продаже должно быть положительным.");
        }

        // Имитация отдельного этапа подготовки данных; UI-поток не блокируется.
        await Task.Yield();

        var aggregates = new ConcurrentDictionary<string, Aggregate>();
        int completed = 0;
        var options = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };

        // Parallel.ForEach использует TPL и обрабатывает продажи на нескольких потоках.
        await Task.Run(() =>
        {
            Parallel.ForEach(sales, options, sale =>
            {
                options.CancellationToken.ThrowIfCancellationRequested();
                var product = productById[sale.ProductId];
                var aggregate = aggregates.GetOrAdd(product.Category, _ => new Aggregate());

                lock (aggregate)
                {
                    aggregate.Revenue += product.Price * sale.Quantity;
                    aggregate.SalesCount++;
                    aggregate.UnitsSold += sale.Quantity;
                }

                int current = Interlocked.Increment(ref completed);
                progress?.Report((int)(current * 100.0 / sales.Count));
            });
        }, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        return aggregates
            .Select(pair => new CategoryStatistic
            {
                Category = pair.Key,
                Revenue = pair.Value.Revenue,
                SalesCount = pair.Value.SalesCount,
                UnitsSold = pair.Value.UnitsSold
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();
    }
}
