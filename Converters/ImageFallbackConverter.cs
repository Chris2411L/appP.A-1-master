using System;
using System.Globalization;
using System.IO;
using Microsoft.Maui.Controls;

namespace appP.A.Converters
{
    public class ImageFallbackConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var s = value as string;
                if (string.IsNullOrWhiteSpace(s))
                {
                    // Fallback to a bundled image included in the project (dotnet_bot.png exists in Resources/Images)
                    return ImageSource.FromFile("dotnet_bot.png");
                }

                if (s.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    // Use UriImageSource with caching enabled so MAUI downloads and caches the image automatically
                    try
                    {
                        var uri = new Uri(s);
                        return new UriImageSource
                        {
                            Uri = uri,
                            CachingEnabled = true,
                            CacheValidity = TimeSpan.FromDays(7)
                        };
                    }
                    catch
                    {
                        return ImageSource.FromFile("placeholder.jpg");
                    }
                }

                // Assume local resource filename
                // If file not found, return placeholder
                try
                {
                    return ImageSource.FromFile(s);
                }
                catch
                {
                    return ImageSource.FromFile("dotnet_bot.png");
                }
            }
            catch
            {
                return ImageSource.FromFile("placeholder.jpg");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
