using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebsiteAggregation;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly BitmapImage _maxImage;
    private readonly BitmapImage _restoreImage;

    public MainWindow()
    {
        //SetWindowIcon();
        InitializeComponent();
        if (Properties.Settings.Default.LastWindowWidth > 0 && Properties.Settings.Default.LastWindowHeight > 0)
        {
            Width = Properties.Settings.Default.LastWindowWidth;
            Height = Properties.Settings.Default.LastWindowHeight;
        }

        var appSettings = LoadAppSettings();
        Title = appSettings.Title;
        AddTabsToControl(appSettings.WebSites);

        _maxImage = new BitmapImage(new Uri("pack://application:,,,/Images/最大化.png"));
        _restoreImage = new BitmapImage(new Uri("pack://application:,,,/Images/还原.png"));
    }

    private void AddTabsToControl(List<WebSites> webSites)
    {
        foreach (var site in webSites)
        {
            TabControlMain.Items.Add(new TabItem
            {
                Header = site.Title,
                Content = new WebView2()
                {
                    Source = new Uri(site.Url)
                }
            });
        }
    }

    private static AppSettings LoadAppSettings()
    {
        // 获取配置文件路径
        var configFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        var fileContents = File.ReadAllText(configFilePath);

        return JsonConvert.DeserializeObject<AppSettings>(fileContents)!;
    }

    private void WindowStatusChange(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn)
            return;
        if (btn == BtnClose)
        {
            //弹出确认框
            var result = MessageBox.Show("是否关闭？", "提示", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Close();
            }
        }
        else if (btn == BtnMaximize)
        {
            // Maximize or restore the window
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
        else if (btn == BtnMinimize)
        {
            WindowState = WindowState.Minimized;
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (WindowState != WindowState.Minimized)
        {
            Properties.Settings.Default.LastWindowWidth = (int)Width;
            Properties.Settings.Default.LastWindowHeight = (int)Height;
            Properties.Settings.Default.Save();
        }

        base.OnClosing(e);
    }

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && WindowState == WindowState.Normal)
        {
            DragMove();
        }
    }

    private void Window_OnStateChanged(object sender, EventArgs e)
    {
        ImgMaximize.Source = WindowState == WindowState.Maximized ? _restoreImage : _maxImage;
    }

    private DateTime _lastClickTime = DateTime.Now;

    private void Canvas_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var now = DateTime.Now;
        if (now - _lastClickTime < TimeSpan.FromMilliseconds(500))
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            _lastClickTime = now.AddSeconds(-1);
        }
        else
        {
            _lastClickTime = now;
        }
    }

    private void TabControlMain_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Home || e.Key == Key.End)
        {
            e.Handled = true;
        }
    }
}

public record AppSettings(string Title, List<WebSites> WebSites);

public record WebSites(string Title, string Url);