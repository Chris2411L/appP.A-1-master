using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using appP.A.Models;
using Microsoft.Maui.Storage;

namespace appP.A.Services
{
    public static class ImageHelper
    {
        // This helper will create simple placeholder PNG files in Resources/Images
        public static void EnsurePlaceholders(IEnumerable<string> filenames)
        {
            try
            {
                var root = Path.Combine(FileSystem.Current.AppDataDirectory, "..", "Resources", "Images");
                if (!Directory.Exists(root))
                {
                    var projectRoot = AppDomain.CurrentDomain.BaseDirectory;
                    root = Path.Combine(projectRoot, "Resources", "Images");
                }

                if (!Directory.Exists(root)) Directory.CreateDirectory(root);

                const string onePixelPngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR4nGNgYAAAAAMAASsJTYQAAAAASUVORK5CYII=";

                foreach (var f in filenames.Distinct())
                {
                    try
                    {
                        var path = Path.Combine(root, f);
                        if (!File.Exists(path))
                        {
                            var bytes = Convert.FromBase64String(onePixelPngBase64);
                            File.WriteAllBytes(path, bytes);
                        }
                    }
                    catch
                    {
                        // ignore individual errors
                    }
                }
            }
            catch { }
        }

        // Downloads remote images referenced in producto.Imagen (if they are HTTP URLs)
        // and replaces producto.Imagen with the local cached file path so MAUI can load it reliably.
        public static async Task DownloadAndCacheRemoteImagesAsync(IEnumerable<Producto> products)
        {
            try
            {
                using var http = new HttpClient();
                var cacheRoot = Path.Combine(FileSystem.CacheDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProductImages");
                if (!Directory.Exists(cacheRoot)) Directory.CreateDirectory(cacheRoot);

                foreach (var p in products)
                {
                    try
                    {
                        var img = p.Imagen;
                        if (string.IsNullOrWhiteSpace(img)) continue;
                        if (!img.StartsWith("http", StringComparison.OrdinalIgnoreCase)) continue;

                        var safeName = ($"{p.Id}-{p.Nombre}").ToLowerInvariant();
                        foreach (var c in Path.GetInvalidFileNameChars()) safeName = safeName.Replace(c, '-');
                        safeName = Regex.Replace(safeName, "\\s+", "-");
                        var ext = ".jpg";
                        var filePath = Path.Combine(cacheRoot, safeName + ext);

                        if (File.Exists(filePath))
                        {
                            p.Imagen = filePath;
                            continue;
                        }

                        var bytes = await http.GetByteArrayAsync(img);
                        await File.WriteAllBytesAsync(filePath, bytes);
                        p.Imagen = filePath; // notify via Producto.Imagen setter
                    }
                    catch { /* ignore per-image errors */ }
                }
            }
            catch { }
        }
    }
}
