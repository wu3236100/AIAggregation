using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            this.Width = Properties.Settings.Default.LastWindowWidth;
            this.Height = Properties.Settings.Default.LastWindowHeight;
        }

        var appSettings = LoadAppSettings();
        this.Title = appSettings.Title;
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
                this.Close();
            }
        }
        else if (btn == BtnMaximize)
        {
            // Maximize or restore the window
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
        else if (btn == BtnMinimize)
        {
            this.WindowState = WindowState.Minimized;
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (this.WindowState != WindowState.Minimized)
        {
            Properties.Settings.Default.LastWindowWidth = (int)this.Width;
            Properties.Settings.Default.LastWindowHeight = (int)this.Height;
            Properties.Settings.Default.Save();
        }

        base.OnClosing(e);
    }

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            this.DragMove();
            //Window.DragMove();
        }
    }

    private void Window_StateChanged(object sender, EventArgs e)
    {
        ImgMaximize.Source = this.WindowState == WindowState.Maximized ? _restoreImage : _maxImage;
    }
}

public record AppSettings(string Title, List<WebSites> WebSites);

public record WebSites(string Title, string Url);