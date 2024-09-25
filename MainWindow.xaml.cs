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
    }

    private void AddTabsToControl(List<WebSites> webSites)
    {
        foreach (var Site in webSites)
        {
            tabControl.Items.Add(new TabItem
            {
                Header = Site.Title,
                Content = new WebView2()
                {
                    Source = new Uri(Site.Url)
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

    //private void SetWindowIcon()
    //{
    //    var iconFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.png");

    //    BitmapImage bitmap = new();

    //    bitmap.BeginInit();
    //    bitmap.UriSource = new Uri(iconFilePath, UriKind.Absolute);
    //    bitmap.CacheOption = BitmapCacheOption.OnLoad;
    //    bitmap.EndInit();

    //    this.Icon = bitmap;
    //}

    protected override void OnClosing(CancelEventArgs e)
    {
        Properties.Settings.Default.LastWindowWidth = (int)this.Width;
        Properties.Settings.Default.LastWindowHeight = (int)this.Height;
        Properties.Settings.Default.Save();

        base.OnClosing(e);
    }
}

public record AppSettings(string Title, List<WebSites> WebSites);

public record WebSites(string Title, string Url);