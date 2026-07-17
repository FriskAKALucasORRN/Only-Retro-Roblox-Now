using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Imaging;
using OnlyRetroRobloxHere.Launcher.Models.Attributes;

namespace OnlyRetroRobloxHere.Launcher.UI.ViewModels.Pages;

internal class AboutViewModel : ViewModelBase
{

    public static string Version => Utils.Version;

    public static BuildMetadataAttribute BuildMetadata => Utils.BuildMetadata;

    public static string BuildTime => BuildMetadata.Timestamp.ToString("dddd, d MMMM yyyy 'at' h:mm:ss tt", CultureInfo.InvariantCulture);

    public Visibility PrivateVersionWarningVisibility { get; set; }

    public static Visibility DebugVersionWarningVisibility =>
    Utils.IsDebug ? Visibility.Visible : Visibility.Collapsed;

    public BitmapImage BannerSource => new BitmapImage(new Uri("pack://application:,,,/OnlyRetroRobloxHere;component/Resources/Launcher/BannerLong" + (DateEvents.Fall ? "Fall" : "") + (DateEvents.Spring ? "Spring" : "") + (DateEvents.Summer ? "Summer" : "") + (DateEvents.Pride ? "Pride" : "") + (DateEvents.Winter ? "Winter" : "") + ".png"));

    public Visibility SnowfallVisibility
    {
        get
        {
            if (!DateEvents.Winter)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }
    }
}