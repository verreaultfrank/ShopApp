using JobHunterDashboard.ViewModels;
using Syncfusion.Maui.Charts;

namespace JobHunterDashboard.Views;

public partial class AnalyticsPage : ContentPage
{
    private readonly AnalyticsViewModel _viewModel;

    public AnalyticsPage(AnalyticsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // Listen for chart type changes to swap series dynamically
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        // Style active tab on load
        UpdateTabStyles();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AnalyticsViewModel.SelectedChartType))
        {
            UpdateCartesianSeries();
        }
        else if (e.PropertyName == nameof(AnalyticsViewModel.SelectedTabIndex))
        {
            UpdateTabStyles();
        }
    }

    private void UpdateTabStyles()
    {
        var activeColor = Application.Current?.Resources["AppPrimaryColor"] as Color ?? Colors.Blue;
        var inactiveColor = Application.Current?.Resources["AppBorderColor"] as Color ?? Colors.Gray;
        var activeTextColor = Colors.White;
        var inactiveTextColor = Application.Current?.Resources["AppTextColor"] as Color ?? Colors.Black;

        TabStatistics.BackgroundColor = _viewModel.SelectedTabIndex == 0 ? activeColor : inactiveColor;
        TabStatistics.TextColor = _viewModel.SelectedTabIndex == 0 ? activeTextColor : inactiveTextColor;
        TabStatistics.FontAttributes = _viewModel.SelectedTabIndex == 0 ? FontAttributes.Bold : FontAttributes.None;

        TabRegression.BackgroundColor = _viewModel.SelectedTabIndex == 1 ? activeColor : inactiveColor;
        TabRegression.TextColor = _viewModel.SelectedTabIndex == 1 ? activeTextColor : inactiveTextColor;
        TabRegression.FontAttributes = _viewModel.SelectedTabIndex == 1 ? FontAttributes.Bold : FontAttributes.None;

        TabML.BackgroundColor = _viewModel.SelectedTabIndex == 2 ? activeColor : inactiveColor;
        TabML.TextColor = _viewModel.SelectedTabIndex == 2 ? activeTextColor : inactiveTextColor;
        TabML.FontAttributes = _viewModel.SelectedTabIndex == 2 ? FontAttributes.Bold : FontAttributes.None;
    }

    private void UpdateCartesianSeries()
    {
        if (CartesianChart == null) return;

        // Remove existing series
        CartesianChart.Series.Clear();

        // For "Bar" chart, transpose the axes to make columns horizontal
        CartesianChart.IsTransposed = (_viewModel.SelectedChartType == "Bar");

        ChartSeries newSeries = _viewModel.SelectedChartType switch
        {
            "Bar" => new ColumnSeries
            {
                ItemsSource = _viewModel.ChartData,
                XBindingPath = "XValue",
                YBindingPath = "YValue",
                Fill = new SolidColorBrush(Application.Current?.Resources["AppPrimaryColor"] as Color ?? Colors.Blue)
            },
            "Line" => new LineSeries
            {
                ItemsSource = _viewModel.ChartData,
                XBindingPath = "XValue",
                YBindingPath = "YValue",
                Fill = new SolidColorBrush(Application.Current?.Resources["AppPrimaryColor"] as Color ?? Colors.Blue),
                StrokeWidth = 2
            },
            "Scatter" => new ScatterSeries
            {
                ItemsSource = _viewModel.ChartData,
                XBindingPath = "XValue",
                YBindingPath = "YValue",
                Fill = new SolidColorBrush(Application.Current?.Resources["AppPrimaryColor"] as Color ?? Colors.Blue),
                PointHeight = 10,
                PointWidth = 10
            },
            _ => new ColumnSeries // Default "Column"
            {
                ItemsSource = _viewModel.ChartData,
                XBindingPath = "XValue",
                YBindingPath = "YValue",
                Fill = new SolidColorBrush(Application.Current?.Resources["AppPrimaryColor"] as Color ?? Colors.Blue)
            }
        };

        CartesianChart.Series.Add(newSeries);

        // Also update circular chart series for Pie/Doughnut
        if (CircularChart != null)
        {
            CircularChart.Series.Clear();

            if (_viewModel.SelectedChartType == "Doughnut")
            {
                CircularChart.Series.Add(new DoughnutSeries
                {
                    ItemsSource = _viewModel.ChartData,
                    XBindingPath = "XValue",
                    YBindingPath = "YValue"
                });
            }
            else
            {
                CircularChart.Series.Add(new PieSeries
                {
                    ItemsSource = _viewModel.ChartData,
                    XBindingPath = "XValue",
                    YBindingPath = "YValue"
                });
            }
        }
    }
}
