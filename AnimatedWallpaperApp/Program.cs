using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Check for updates
        await UpdateChecker.CheckForUpdatesAsync();
        // Introduction to the user
        Console.WriteLine("Welcome to the Animated Wallpaper Setter!");
        Console.WriteLine("This application will help you set an animated GIF as your wallpaper.");

        // Ask for the GIF file path
        Console.WriteLine("Please enter the full path of the GIF file you want to use as the wallpaper:");

        // Get the GIF path from user input
        string gifFilePath = Console.ReadLine();

        // Validate the file path and handle setting the wallpaper
        AnimatedWallpaper wallpaper = new AnimatedWallpaper(gifFilePath);
        if (wallpaper.IsValid)
        {
            wallpaper.Start();
        }
        else
        {
            Console.WriteLine("Invalid file path or unsupported file type. Please provide a valid GIF.");
        }

        // Instructions to the user
        Console.WriteLine("The wallpaper will now be set to the GIF you provided and updated every frame.");
        Console.WriteLine("Press 'q' and hit Enter to quit the application at any time.");

        // Use Console.ReadLine to wait for user input and safely handle exit
        while (true)
        {
            string userInput = Console.ReadLine();
            if (userInput?.ToLower() == "q")
            {
                Console.WriteLine("Exiting the wallpaper setter.");
                break; // Exit the loop and close the app
            }
        }

        // Clean up resources when the user exits
        wallpaper.Dispose();
        Console.WriteLine("Application closed.");
    }
}

public class UpdateChecker
{
    private const string CurrentVersion = "1.0.0";  // The current version of the app
    private const string VersionUrl = "https://github.com/thelonewolf39/AnimatedWallpaperApp/blob/master/version.txt";  // URL where version.txt is hosted
    private const string DownloadUrl = "https://github.com/thelonewolf39/AnimatedWallpaperApp/blob/master/AnimatedWallpaperInstaller.msi";  // URL for the installer

    public static async Task CheckForUpdatesAsync()
    {
        try
        {
            // Step 1: Check for the latest version from the remote file
            var latestVersion = await GetLatestVersionAsync();

            // Step 2: Compare the latest version with the current version
            if (latestVersion != CurrentVersion)
            {
                Console.WriteLine($"New version {latestVersion} available! Updating...");
                await DownloadAndInstallUpdateAsync();
            }
            else
            {
                Console.WriteLine("You already have the latest version.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking for updates: {ex.Message}");
        }
    }

    private static async Task<string> GetLatestVersionAsync()
    {
        using (var client = new HttpClient())
        {
            // Step 1: Download the version file from the server
            var versionText = await client.GetStringAsync(VersionUrl);
            return versionText.Trim();  // Remove any extra whitespace
        }
    }

    private static async Task DownloadAndInstallUpdateAsync()
    {
        using (var client = new HttpClient())
        {
            // Step 2: Download the new installer
            var installerFile = await client.GetByteArrayAsync(DownloadUrl);

            // Save the installer to a temporary location
            string tempFilePath = Path.Combine(Path.GetTempPath(), "AnimatedWallpaperInstaller.msi");
            await File.WriteAllBytesAsync(tempFilePath, installerFile);

            // Step 3: Run the installer to update the app
            InstallUpdate(tempFilePath);
        }
    }

    private static void InstallUpdate(string installerPath)
    {
        try
        {
            Console.WriteLine("Installing the new version...");
            Process.Start(new ProcessStartInfo(installerPath)
            {
                UseShellExecute = true,
                Verb = "runas"  // Run with elevated privileges
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error installing update: {ex.Message}");
        }
    }

}
