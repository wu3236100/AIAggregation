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
    private bool _isDragging;
    private Point _clickPosition;
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
            tabControl.Items.Add(new TabItem
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
        if (sender is Button btn)
        {
            switch (btn.Name)
            {
                case "Btn_Close":
                    //弹出确认框
                    var result = MessageBox.Show("是否关闭？", "提示", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        this.Close();
                    }
                    break;
                case "Btn_Maximize":
                    this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                    break;
                case "Btn_Minimize":
                    this.WindowState = WindowState.Minimized;
                    break;
            }
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

    #region 拖动实现
    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isDragging = true;
        _clickPosition = e.GetPosition(this);
        Mouse.Capture(canvas);
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isDragging && this.WindowState != WindowState.Maximized)
        {
            Point currentPosition = e.GetPosition(this);
            this.Left += currentPosition.X - _clickPosition.X;
            this.Top += currentPosition.Y - _clickPosition.Y;
        }
    }

    private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
        Mouse.Capture(null);
    }
    #endregion

    private void Window_StateChanged(object sender, EventArgs e)
    {
        Img_Maximize.Source = this.WindowState == WindowState.Maximized ? _restoreImage : _maxImage;
    }
}

public record AppSettings(string Title, List<WebSites> WebSites);

public record WebSites(string Title, string Url);