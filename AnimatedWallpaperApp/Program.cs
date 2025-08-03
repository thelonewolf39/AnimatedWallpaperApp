using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AnimatedWallpaperApp
{
    internal class Program
    {
        private static readonly CancellationTokenSource _cts = new();

        private static async Task<int> Main()
        {
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;            // prevent hard kill
                _cts.Cancel();
            };

            try
            {
                await UpdateChecker.CheckForUpdatesAsync(_cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update check failed: {ex.Message}");
            }

            Console.WriteLine("Welcome to the Animated Wallpaper Setter!");
            var gifPath = RequestGifPath();
            if (gifPath is null) return 1;

            using var wallpaper = new AnimatedWallpaper(gifPath);
            if (!wallpaper.IsValid)
            {
                Console.WriteLine("Invalid file or not a GIF.");
                return 1;
            }

            try
            {
                wallpaper.Start();
                Console.WriteLine("Press 'q' then Enter (or Ctrl+C) to quit.");

                while (!_cts.IsCancellationRequested)
                {
                    var line = Console.ReadLine();
                    if (string.Equals(line, "q", StringComparison.OrdinalIgnoreCase))
                        break;
                }
            }
            finally
            {
                // AnimatedWallpaper.Dispose() called by await using
            }

            Console.WriteLine("Application closed.");
            return 0;
        }

        private static string RequestGifPath()
        {
            Console.WriteLine("Enter full path of the GIF to use:");
            var input = Console.ReadLine()?.Trim('"', ' ');

            if (string.IsNullOrWhiteSpace(input) ||
                !File.Exists(input) ||
                !string.Equals(Path.GetExtension(input), ".gif", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("❌ File not found or not a .gif");
                return null;
            }

            return Path.GetFullPath(input);
        }
    }

    public static class UpdateChecker
    {
        private const string CurrentVersion = "1.1.0";

        private static readonly HttpClient _http =
            new() { Timeout = TimeSpan.FromSeconds(10) };

        private const string VersionUrl =
            "https://raw.githubusercontent.com/thelonewolf39/AnimatedWallpaperApp/master/version.txt";

        private const string DownloadUrl =
            "https://github.com/thelonewolf39/AnimatedWallpaperApp/releases/latest/download/AnimatedWallpaperInstaller.msi";

        public static async Task CheckForUpdatesAsync(CancellationToken ct)
        {
            string latestText;
            try
            {
                latestText = (await _http.GetStringAsync(VersionUrl, ct)).Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Version check failed: {ex.Message}");
                return;
            }

            if (!Version.TryParse(latestText, out var latest) ||
                !Version.TryParse(CurrentVersion, out var current))
            {
                Console.WriteLine("⚠️  Could not parse version numbers.");
                return;
            }

            if (latest <= current)
            {
                Console.WriteLine("Already on latest version.");
                return;
            }

            Console.WriteLine($"New version {latest} available – downloading…");

            var installerBytes = await _http.GetByteArrayAsync(DownloadUrl, ct);
            var msiPath = Path.Combine(Path.GetTempPath(), "AnimatedWallpaperInstaller.msi");
            await File.WriteAllBytesAsync(msiPath, installerBytes, ct);

            Console.WriteLine("Launching installer…");
            Process.Start(new ProcessStartInfo(msiPath)
            {
                UseShellExecute = true,
                Verb = "runas"
            });

            // Exit current process so installer can overwrite files
            Environment.Exit(0);
        }
    }
}
