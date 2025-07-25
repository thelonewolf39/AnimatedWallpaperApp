using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

class AnimatedWallpaper : IDisposable
{
    // Define constants for wallpaper management
    const int SPI_SETDESKWALLPAPER = 0x0014;
    const int SPIF_UPDATEINIFILE = 0x01;
    const int SPIF_SENDCHANGE = 0x02;

    // External method to set wallpaper
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    private string _gifFilePath;
    private int _currentFrame = 0;
    private Timer _timer;
    private Image _gifImage;
    private int _totalFrames;

    public bool IsValid { get; private set; }

    public AnimatedWallpaper(string gifFilePath)
    {
        _gifFilePath = gifFilePath;

        // Validate the file path and load the GIF
        if (File.Exists(_gifFilePath) && Path.GetExtension(_gifFilePath).Equals(".gif", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                _gifImage = Image.FromFile(_gifFilePath);
                _totalFrames = _gifImage.GetFrameCount(FrameDimension.Time); // Get total frames from the GIF
                IsValid = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading GIF: {ex.Message}");
                IsValid = false;
            }
        }
        else
        {
            IsValid = false;
        }
    }

    public void Start()
    {
        if (!IsValid) return;

        // Set the wallpaper to the first frame of the GIF
        SetWallpaperToGifFrame(_currentFrame);

        // Start a timer to update the wallpaper every frame (10 FPS)
        _timer = new Timer(UpdateWallpaper, null, 0, 100); // 100ms interval -> 10 frames per second

        Console.WriteLine("The wallpaper will now be set to the GIF you provided and updated every frame.");
        Console.WriteLine("Press 'q' and hit Enter to quit the application at any time.");
    }

    private void UpdateWallpaper(object state)
    {
        try
        {
            // Move to the next frame in the GIF
            _currentFrame = (_currentFrame + 1) % _totalFrames;

            // Set the wallpaper to the current frame
            SetWallpaperToGifFrame(_currentFrame);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating wallpaper: {ex.Message}");
        }
    }

    private void SetWallpaperToGifFrame(int frameIndex)
    {
        try
        {
            // Select the specific frame from the GIF
            _gifImage.SelectActiveFrame(FrameDimension.Time, frameIndex);

            // Get the current screen resolution
            var screenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            var screenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

            // Get the GIF's original dimensions
            int gifWidth = _gifImage.Width;
            int gifHeight = _gifImage.Height;

            // Calculate the scale factor to preserve the aspect ratio
            float scaleX = (float)screenWidth / gifWidth;
            float scaleY = (float)screenHeight / gifHeight;
            float scale = Math.Min(scaleX, scaleY); // Scale to fit within screen dimensions

            // Calculate the new dimensions based on the scale factor
            int newWidth = (int)(gifWidth * scale);
            int newHeight = (int)(gifHeight * scale);

            // Create a new Bitmap to hold the resized image
            Bitmap resizedGif = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(resizedGif))
            {
                // Set the graphics quality for better image resizing
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                // Draw the GIF onto the new bitmap, resizing it while preserving aspect ratio
                g.DrawImage(_gifImage, 0, 0, newWidth, newHeight);
            }

            // Save the resized image as a temporary BMP file
            string tempBmpPath = Path.Combine(Path.GetTempPath(), $"temp_frame_{frameIndex}.bmp");
            resizedGif.Save(tempBmpPath, ImageFormat.Bmp);

            // Set the wallpaper to the resized BMP file
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempBmpPath, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);

            // Optionally delete the temporary file after applying it
            File.Delete(tempBmpPath);

            // Log the current frame being applied
            Console.WriteLine($"Set wallpaper to frame {frameIndex + 1} of {_totalFrames} with aspect ratio preserved.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting wallpaper to frame {frameIndex + 1}: {ex.Message}");
        }
    }


    // Dispose of resources
    public void Dispose()
    {
        _timer?.Dispose();
        _gifImage?.Dispose();
    }
}
