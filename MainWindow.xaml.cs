using System.Windows;
using System.Windows.Media;
using SalesStatisticsWpf.Services;

namespace SalesStatisticsWpf;

public partial class MainWindow : Window
{
    private readonly SalesDataService _dataService = new();
    private readonly SalesStatisticsService _statisticsService = new();
    private CancellationTokenSource? _cts;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (_cts != null) return;

        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        SetBusy(true);
        ProgressBar.Value = 0;
        StatisticsGrid.ItemsSource = null;

        // UI-индикатор меняется сразу: окно остаётся интерактивным во время await.
        UiHeartbeatText.Text = "UI активен • операция выполняется";
        UiHeartbeatText.Foreground = Brushes.ForestGreen;
        StatusText.Text = "Асинхронная загрузка продуктов и продаж...";
        try
        {
            var data = await _dataService.LoadAsync(InvalidSaleCheckBox.IsChecked == true, token);
            ProductsGrid.ItemsSource = data.Products;
            SalesGrid.ItemsSource = data.Sales;
            StatusText.Text = $"Загружено продуктов: {data.Products.Count}; продаж: {data.Sales.Count}. Выполняется параллельный расчёт...";

            var progress = new Progress<int>(value =>
            {
                ProgressBar.Value = value;
                ProgressText.Text = $"Обработка продаж: {value}%";
            });

            var result = await _statisticsService.CalculateAsync(
                data.Products, data.Sales, progress, token);

            StatisticsGrid.ItemsSource = result;
            ProgressBar.Value = 100;
            ProgressText.Text = "Расчёт завершён: 100%.";
            StatusText.Text = $"Готово. Рассчитано категорий: {result.Count}. Выручка рассчитана по всем продажам.";
            StatusText.Foreground = Brushes.ForestGreen;
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "Операция отменена пользователем. Незавершённые вычисления остановлены.";
            StatusText.Foreground = Brushes.DarkOrange;
            ProgressText.Text = "Выполнение отменено.";
        }
        catch (InvalidOperationException ex)
        {
            StatusText.Text = $"Ошибка в продажах: {ex.Message}";
            StatusText.Foreground = Brushes.Firebrick;
            MessageBox.Show(StatusText.Text, "Ошибка данных", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Непредвиденная ошибка: {ex.Message}";
            StatusText.Foreground = Brushes.Firebrick;
            MessageBox.Show(ex.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
            SetBusy(false);
            UiHeartbeatText.Text = "Интерфейс отвечает";
            UiHeartbeatText.Foreground = Brushes.ForestGreen;
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        _cts?.Cancel();
        StatusText.Text = "Отправлен запрос на отмену...";
    }

    private void SetBusy(bool busy)
    {
        StartButton.IsEnabled = !busy;
        CancelButton.IsEnabled = busy;
        InvalidSaleCheckBox.IsEnabled = !busy;
    }

    protected override void OnClosed(EventArgs e)
    {
        _cts?.Cancel();
        base.OnClosed(e);
    }
}
