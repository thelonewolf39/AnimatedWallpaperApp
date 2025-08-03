using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

class AnimatedWallpaper : IDisposable
{
    private const int SPI_SETDESKWALLPAPER = 0x0014;
    private const int SPIF_UPDATEINIFILE = 0x01;
    private const int SPIF_SENDCHANGE = 0x02;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    private readonly Image _gifImage;
    private readonly FrameDimension _frameDimension = FrameDimension.Time;
    private readonly int _totalFrames;
    private int _currentFrame = 0;
    private System.Threading.Timer _timer;

    public bool IsValid { get; }

    public AnimatedWallpaper(string gifPath)
    {
        try
        {
            using var stream = new FileStream(gifPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _gifImage = Image.FromStream(stream); // avoids file lock
            _totalFrames = _gifImage.GetFrameCount(_frameDimension);
            IsValid = _totalFrames > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading GIF: {ex.Message}");
            IsValid = false;
        }
    }

    public void Start()
    {
        if (!IsValid) return;

        _timer = new System.Threading.Timer(UpdateWallpaper, null, 0, 100); // fixed 100ms update
    }

    private void UpdateWallpaper(object _)
    {
        try
        {
            _gifImage.SelectActiveFrame(_frameDimension, _currentFrame);
            _currentFrame = (_currentFrame + 1) % _totalFrames;

            using Bitmap resizedFrame = GetScaledFrame(_gifImage);
            string tempPath = Path.Combine(Path.GetTempPath(), "animated_wallpaper.bmp");
            resizedFrame.Save(tempPath, ImageFormat.Bmp);

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Failed to update wallpaper: {ex.Message}");
        }
    }

    private static Bitmap GetScaledFrame(Image frame)
    {
        var screen = Screen.PrimaryScreen;
        float dpiScale;
        using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
        {
            dpiScale = graphics.DpiX / 96f;
        }

        int screenWidth = (int)(screen.Bounds.Width * dpiScale);
        int screenHeight = (int)(screen.Bounds.Height * dpiScale);

        int gifWidth = frame.Width;
        int gifHeight = frame.Height;

        float scaleX = (float)screenWidth / gifWidth;
        float scaleY = (float)screenHeight / gifHeight;
        float scale = Math.Min(scaleX, scaleY);

        int newWidth = (int)(gifWidth * scale);
        int newHeight = (int)(gifHeight * scale);

        Bitmap bmp = new Bitmap(newWidth, newHeight);
        using Graphics g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.DrawImage(frame, 0, 0, newWidth, newHeight);
        return bmp;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _gifImage?.Dispose();
    }
}
