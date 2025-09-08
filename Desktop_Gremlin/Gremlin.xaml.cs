using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;


namespace Desktop_Gremlin
{
    /// <summary>
    /// Interaction logic for Jukebox.xaml
    /// </summary>
    public partial class Gremlin : Window
    {
        private NotifyIcon TRAY_ICON;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);



        public class AppSettings
        {
            public int FrameRate { get; set; } = 31;
            public string StartingChar { get; set; } = "Mambo";
            public double FollowRadius { get; set; } = 150.0;
            public int FrameWidthJukebox { get; set; } = 300;
            public int FrameHeightJukebox { get; set; } = 450;
            public int FrameWidth { get; set; } = 300;
            public int FrameHeight { get; set; } = 300;

        }

        public AppSettings Settings = new AppSettings();

        public class AnimationFrameCounts
        {
            public int Intro { get; set; } = 0;
            public int Idle { get; set; } = 0;
            public int Left { get; set; } = 0;
            public int Right { get; set; } = 0;
            public int Up { get; set; } = 0;
            public int Down { get; set; } = 0;
            public int Outro { get; set; } = 0;
            public int Grab { get; set; } = 0;
            public int WalkIdle { get; set; } = 0;
            public int Click { get; set; } = 0;
            public int Dance { get; set; } = 0;
            public int Hover { get; set; } = 0;
            public int Jukebox { get; set; } = 22;
            public int JukeboxFunny { get; set; } = 17;
            public int MusicNote { get; set; } = 123;
        }
        private AnimationFrameCounts FrameCounts = new AnimationFrameCounts();


        private MediaPlayer GRAB_PLAYER = new MediaPlayer();

        private const int SPRITE_COLUMN = 5;

        public class AnimationState
        {
            public int JukeboxFunny { get; set; } = 0;
            public int Jukebox { get; set; } = 0;
            public int MusicNote { get; set; } = 0;
            public int MascotIndex { get; set; } = 0;
            public int Intro { get; set; } = 0;
            public int Idle { get; set; } = 0;
            public int Outro { get; set; } = 0;
            public int WalkDown { get; set; } = 0;
            public int WalkUp { get; set; } = 0;
            public int WalkRight { get; set; } = 0;
            public int WalkLeft { get; set; } = 0;
            public int Grab { get; set; } = 0;
            public int WalkIdle { get; set; } = 0;
            public int Click { get; set; } = 0;
            public int Dance { get; set; } = 0;
            public int Hover { get; set; } = 0;
        }
        private AnimationState CurrentFrames = new AnimationState();


        public class AnimationSheets
        {
            public BitmapImage Intro { get; set; }
            public BitmapImage Idle { get; set; }
            public BitmapImage WalkDown { get; set; }
            public BitmapImage WalkUp { get; set; }
            public BitmapImage WalkLeft { get; set; }
            public BitmapImage WalkRight { get; set; }
            public BitmapImage Outro { get; set; }
            public BitmapImage WalkIdle { get; set; }
            public BitmapImage Click { get; set; }
            public BitmapImage Dance { get; set; }
            public BitmapImage Grab { get; set; }
            public BitmapImage Jukebox { get; set; }
            public BitmapImage JukeboxFunny { get; set; }
            public BitmapImage Hover { get; set; }
        }
        private AnimationSheets Sheets = new AnimationSheets();



        public class State
        {
            public bool PlayIntroOnNewSong { get; set; } = true;
            public bool IsIntro { get; set; } = true;
            public bool IsIntroJukebox { get; set; } = true;
            public bool ForwardAnimation { get; set; } = true;
            public bool AllowRandomMascot { get; set; } = true;
            public bool AllowMusicNotes { get; set; } = true;
            public bool IsRandom { get; set; } = false;
            public bool IsHover { get; set; } = false;
            public bool IsIdle { get; set; } = true;
            public bool IsWalking { get; set; } = false;
            public bool IsDragging { get; set; } = false;
            public bool LastDragState { get; set; } = false;
            public bool LastStateDragOrWalk { get; set; } = false;
            public bool IsWalkIdle { get; set; } = false;
            public bool IsClick { get; set; } = false;
            public bool IsEmoting3 { get; set; } = false;
        }
        private State States = new State();

        public class MouseSettings
        {
            public bool FollowCursor { get; set; } = false;
            public System.Drawing.Point LastMousePosition { get; set; }
            public double FollowSpeed { get; set; } = 5.0;
            public DateTime LastCursorMove { get; set; } 
            public double MouseX { get; set; } 
            public double MouseY { get; set; }
            public double Speed { get; set; } = 10.0;

        }
       private MouseSettings Mouse = new MouseSettings();


        private DispatcherTimer CLOSE_TIMER;
        private DispatcherTimer MASTER_TIMER;
        private DispatcherTimer SCROLL_TIMER;


        private double SCROLL_POS;

        private List<string> MASCOTS = new List<string>();
        private List<string> MUSIC_FILES = new List<string>();

        private MediaPlayer PLAYER;

        public struct POINT
        {
            public int X;
            public int Y;
        }

        public Gremlin()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
            LoadMasterConfig();
            LoadConfigChar();
            SetupTrayIcon();
            LoadSpritesSheet();
            InitializeAnimations();

        }
        private BitmapImage LoadSprite(string filefolder, string fileName, string rootFolder = "Gremlins")
        {
            
            string path = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "SpriteSheet", rootFolder, filefolder, fileName);

            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(path);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error] Failed to load sprite {fileName}: {ex.Message}");
                return null;
            }
        }

        private void LoadSpritesSheet()
        {
            Sheets.Intro = LoadSprite(Settings.StartingChar, "intro.png");
            Sheets.Idle = LoadSprite(Settings.StartingChar, "idle.png");
            Sheets.WalkLeft = LoadSprite(Settings.StartingChar, "left.png");
            Sheets.WalkRight = LoadSprite(Settings.StartingChar, "right.png");
            Sheets.WalkDown = LoadSprite(Settings.StartingChar, "forward.png");
            Sheets.WalkUp = LoadSprite(Settings.StartingChar, "backward.png");
            Sheets.Outro = LoadSprite(Settings.StartingChar, "outro.png");
            Sheets.Grab = LoadSprite(Settings.StartingChar, "grab.png");
            Sheets.WalkIdle = LoadSprite(Settings.StartingChar, "wIdle.png");
            Sheets.Click = LoadSprite(Settings.StartingChar, "click.png");
            Sheets.Hover = LoadSprite(Settings.StartingChar, "hover.png");

            // special non-character ones
            Sheets.Jukebox = LoadSprite("Jukebox", "jukebox.png");
            Sheets.JukeboxFunny = LoadSprite("Jukebox", "music_box_goofy.png");
        }


        //Mainly for the user, This will be updated or debugging purposes
        //To many edge cases for the program to crash.
        private void FatalError(string message, string title = "Error")
        {
            System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            TRAY_ICON?.Dispose();
            System.Windows.Application.Current.Shutdown();
        }
        private int PlayAnimation(BitmapImage sheet, int currentFrame, int frameCount, int frameWidth, int frameHeight, System.Windows.Controls.Image targetImage, bool reverse = false)
        {
            if (sheet == null)
                return currentFrame;

            int x = (currentFrame % SPRITE_COLUMN) * frameWidth;
            int y = (currentFrame / SPRITE_COLUMN) * frameHeight;

            if (x + frameWidth > sheet.PixelWidth || y + frameHeight > sheet.PixelHeight)
                return currentFrame;

            targetImage.Source = new CroppedBitmap(sheet, new Int32Rect(x, y, frameWidth, frameHeight));

            if (!reverse)
            {
                return (currentFrame + 1) % frameCount;
            }
            else
            {
                if (States.ForwardAnimation)
                {
                    currentFrame++;
                    if (currentFrame >= frameCount - 1) States.ForwardAnimation = false;
                }
                else
                {
                    currentFrame--;
                    if (currentFrame <= 0) States.ForwardAnimation = true;
                }
                return currentFrame;
            }
        }

        private void InitializeAnimations()
        {
            if (Sheets.Intro == null)
            {
                States.IsIntro = false;
            }

            MASTER_TIMER = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Settings.FrameRate) };
            MASTER_TIMER.Tick += (s, e) =>
            {
                if (States.IsIntro)
                {
                    CurrentFrames.Intro = PlayAnimation(
                        Sheets.Intro,
                        CurrentFrames.Intro,
                        FrameCounts.Intro,
                        Settings.FrameWidth,
                        Settings.FrameHeight,
                        SpriteImage);

                    if (CurrentFrames.Intro == 0)
                    {
                        States.IsIntroJukebox = false;
                        States.IsIntro = false;
                    }
                }

                if (States.IsIntroJukebox)
                {
                    CurrentFrames.Jukebox = PlayAnimation(
                        Sheets.Jukebox,
                        CurrentFrames.Jukebox,
                        FrameCounts.Jukebox,
                        Settings.FrameWidthJukebox,
                        Settings.FrameHeightJukebox,
                        JukeBoxSprite);
                }

                if (States.IsDragging)
                {
                    CurrentFrames.Grab = PlayAnimation(
                        Sheets.Grab,
                        CurrentFrames.Grab,
                        FrameCounts.Grab,
                        Settings.FrameWidth,
                        Settings.FrameHeight,
                        SpriteImage);

                    States.IsIntro = false;
                    States.IsClick = false;
                }

                if (States.IsHover && !States.IsDragging)
                {
                    CurrentFrames.Hover = PlayAnimation(
                        Sheets.Hover,
                        CurrentFrames.Hover,
                        FrameCounts.Hover,
                        Settings.FrameWidth,
                        Settings.FrameHeight,
                        SpriteImage);
                }

                if (States.IsClick)
                {
                    CurrentFrames.Click = PlayAnimation(
                        Sheets.Click,
                        CurrentFrames.Click,
                        FrameCounts.Click,
                        Settings.FrameWidth,
                        Settings.FrameHeight,
                        SpriteImage);

                    States.IsIntro = false;

                    if (CurrentFrames.Click == 0)
                    {
                        States.IsClick = false;
                    }
                }

                if (!States.IsIntro && !States.IsDragging && !States.IsWalkIdle && !States.IsClick && !States.IsHover)
                {
                    CurrentFrames.Idle = PlayAnimation(
                        Sheets.Idle,
                        CurrentFrames.Idle,
                        FrameCounts.Idle,
                        Settings.FrameWidth,
                        Settings.FrameHeight,
                        SpriteImage);
                }

                if (Mouse.FollowCursor && !States.IsDragging && !States.IsClick)
                {
                    POINT cursorPos;
                    GetCursorPos(out cursorPos);
                    var cursorScreen = new System.Windows.Point(cursorPos.X, cursorPos.Y);

                    double halfW = SpriteImage.ActualWidth > 0 ? SpriteImage.ActualWidth / 2.0 : Settings.FrameWidth / 2.0;
                    double halfH = SpriteImage.ActualHeight > 0 ? SpriteImage.ActualHeight / 2.0 : Settings.FrameHeight / 2.0;
                    var spriteCenterScreen = SpriteImage.PointToScreen(new System.Windows.Point(halfW, halfH));

                    var source = PresentationSource.FromVisual(this);
                    System.Windows.Media.Matrix transformFromDevice = System.Windows.Media.Matrix.Identity;
                    if (source?.CompositionTarget != null)
                    {
                        transformFromDevice = source.CompositionTarget.TransformFromDevice;
                    }

                    var spriteCenterWpf = transformFromDevice.Transform(spriteCenterScreen);
                    var cursorWpf = transformFromDevice.Transform(cursorScreen);

                    double dx = cursorWpf.X - spriteCenterWpf.X;
                    double dy = cursorWpf.Y - spriteCenterWpf.Y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    if (distance > Settings.FollowRadius)
                    {
                        double step = Math.Min(Mouse.Speed, distance - Settings.FollowRadius);
                        double nx = dx / distance;
                        double ny = dy / distance;
                        double moveX = nx * step;
                        double moveY = ny * step;

                        this.Left += moveX;
                        this.Top += moveY;

                        if (Math.Abs(moveX) > Math.Abs(moveY))
                        {
                            if (moveX < 0)
                            {
                                CurrentFrames.WalkLeft = PlayAnimation(
                                    Sheets.WalkLeft,
                                    CurrentFrames.WalkLeft,
                                    FrameCounts.Left,
                                    Settings.FrameWidth,
                                    Settings.FrameHeight,
                                    SpriteImage);
                            }
                            else
                            {
                                CurrentFrames.WalkRight = PlayAnimation(
                                    Sheets.WalkRight,
                                    CurrentFrames.WalkRight,
                                    FrameCounts.Right,
                                    Settings.FrameWidth,
                                    Settings.FrameHeight,
                                    SpriteImage);
                            }
                        }
                        else
                        {
                            if (moveY > 0)
                            {
                                CurrentFrames.WalkDown = PlayAnimation(
                                    Sheets.WalkDown,
                                    CurrentFrames.WalkDown,
                                    FrameCounts.Down,
                                    Settings.FrameWidth,
                                    Settings.FrameHeight,
                                    SpriteImage);
                            }
                            else
                            {
                                CurrentFrames.WalkUp = PlayAnimation(
                                    Sheets.WalkUp,
                                    CurrentFrames.WalkUp,
                                    FrameCounts.Up,
                                    Settings.FrameWidth,
                                    Settings.FrameHeight,
                                    SpriteImage);
                            }
                        }

                        SpriteLabel.Content =
                            $"dist={distance:F1}\nmoveX={moveX:F1}, moveY={moveY:F1}\nsprCenter=({spriteCenterWpf.X:F1},{spriteCenterWpf.Y:F1})";
                    }
                    else
                    {
                        CurrentFrames.WalkIdle = PlayAnimation(
                            Sheets.WalkIdle,
                            CurrentFrames.WalkIdle,
                            FrameCounts.WalkIdle,
                            Settings.FrameWidth,
                            Settings.FrameHeight,
                            SpriteImage);
                    }
                }
            };

            MASTER_TIMER.Start();
        }




        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            States.IsDragging = true;
            //PlaySound("grab.mp3", restart: true, playerOverride: GRAB_PLAYER);      
            DragMove(); // only if you actually move it
            States.IsDragging = false;
            Mouse.FollowCursor = !Mouse.FollowCursor;
        }

        private void ResetApp()
        {
            TRAY_ICON.Visible = false;
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(exePath);
            System.Windows.Application.Current.Shutdown();
        }

        private void CloseApp()
        {
            MASTER_TIMER.Stop();

            CLOSE_TIMER = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Settings.FrameRate) };
            CLOSE_TIMER.Tick += (s, e) =>
            {
                CurrentFrames.Outro = PlayAnimation(
                    Sheets.Outro,
                    CurrentFrames.Outro,
                    FrameCounts.Outro,
                    Settings.FrameWidth,
                    Settings.FrameHeight,
                    SpriteImage);

                if (CurrentFrames.Outro == 0)
                {
                    TRAY_ICON.Visible = false;
                    TRAY_ICON?.Dispose();
                    System.Windows.Application.Current.Shutdown();
                }
            };

            CLOSE_TIMER.Start();
        }

        private void SetupTrayIcon()
        {
            TRAY_ICON = new NotifyIcon();
            TRAY_ICON.Icon = new Icon("icon.ico");
            TRAY_ICON.Visible = true;
            TRAY_ICON.Text = "Jukebox";

            var menu = new ContextMenuStrip();

            var randomChar = new ToolStripMenuItem("Random Characters") { CheckOnClick = true };
            randomChar.CheckedChanged += (s, e) =>
            {
                States.AllowRandomMascot = randomChar.Checked;
            };


            menu.Items.Add(randomChar);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Reappear", null, (s, e) => ResetApp());
            menu.Items.Add("Close", null, (s, e) => CloseApp());

            TRAY_ICON.ContextMenuStrip = menu;
        }

        
        private void PlaySound(string fileName, bool restart = false, MediaPlayer playerOverride = null)
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds", fileName);

            if (!File.Exists(path))
            {
                Debug.WriteLine($"[Warning] Missing sound: {path}");
                return;
            }

            try
            {
                MediaPlayer player = playerOverride ?? new MediaPlayer();

                if (restart)
                {
                    player.Stop();
                    player.Open(new Uri(path));
                    player.Volume = 0.8;
                    player.Play();
                }
                else
                {
                    player.Open(new Uri(path));
                    player.Volume = 0.8;
                    player.Play();

                    if (playerOverride == null)
                    {
                        player.MediaEnded += (s, e) => player.Close();
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void SwitchToRandomCharacter()
        {
            if (MASCOTS == null || MASCOTS.Count == 0)
            {
                return;
            }

            // clear current sheets and image
            Sheets.Intro = null;
            Sheets.Idle = null;
            SpriteImage.Source = null;

            GC.Collect();

            var rand = new Random();
            string randomChar = MASCOTS[rand.Next(MASCOTS.Count)];

            Settings.StartingChar = randomChar;
            LoadConfigChar();

            // load new sheets
            Sheets.Intro = LoadSprite(randomChar, "intro.png");
            Sheets.Idle = LoadSprite(randomChar, "dance.png");

            // reset animation states
            CurrentFrames.Intro = 0;
            CurrentFrames.Idle = 0;

            States.IsIntro = true;
        }



        private void LoadMasterConfig()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.txt");
            if (!File.Exists(path))
            {
                FatalError("Cannot find the config file for the main directory", "Missing Config File");
            }

            foreach (var line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line) || !line.Contains("="))
                {
                    continue;
                }

                var parts = line.Split('=');
                if (parts.Length != 2)
                {
                    continue;
                }

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                switch (key.ToUpper())
                {
                    case "START_CHAR":
                        {
                            Settings.StartingChar = value;
                            break;
                        }
                    case "ALLOW_RANDOM_MASCOT":
                        {
                            if (bool.TryParse(value, out bool boolValue))
                            {
                                States.AllowRandomMascot = boolValue;
                            }
                            break;
                        }
                    case "ALLOW_MUSIC_NOTES":
                        {
                            if (bool.TryParse(value, out bool boolValue2))
                            {
                                States.AllowMusicNotes = boolValue2;
                            }
                            break;
                        }
                    case "SPRITE_SPEED":
                        {
                            if (int.TryParse(value, out int intValue))
                            {
                                Settings.FrameRate = intValue;
                            }
                            break;
                        }
                    case "FOLLOW_RADIUS":
                        {
                            if (double.TryParse(value, out double intValue))
                            {
                                Settings.FollowRadius = intValue;
                            }
                            break;
                        }
                }
            }
        }
        private void LoadConfigChar()
        {
            string path = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "SpriteSheet", "Gremlins", Settings.StartingChar, "config.txt");

            if (!File.Exists(path))
            {
                FatalError("Cannot find character name from config/folder. Please check config file if filename matches",
                    "Missing Character File");
            }

            foreach (var line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line) || !line.Contains("="))
                {
                    continue;
                }

                var parts = line.Split('=');
                if (parts.Length != 2)
                {
                    continue;
                }

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                if (!int.TryParse(value, out int intValue))
                {
                    continue;
                }

                switch (key.ToUpper())
                {
                    case "FRAME_HEIGHT":
                        Settings.FrameHeight = intValue;
                        break;
                    case "FRAME_WIDTH":
                        Settings.FrameWidth = intValue;
                        break;
                    case "FRAME_RATE":
                        Settings.FrameRate = intValue;
                        break;
                    case "INTRO_FRAME_COUNT":
                        FrameCounts.Intro = intValue;
                        break;
                    case "IDLE_FRAME_COUNT":
                        FrameCounts.Idle = intValue;
                        break;
                    case "UP_FRAME_COUNT":
                        FrameCounts.Up = intValue;
                        break;
                    case "DOWN_FRAME_COUNT":
                        FrameCounts.Down = intValue;
                        break;
                    case "LEFT_FRAME_COUNT":
                        FrameCounts.Left = intValue;
                        break;
                    case "RIGHT_FRAME_COUNT":
                        FrameCounts.Right = intValue;
                        break;
                    case "OUTRO_FRAME_COUNT":
                        FrameCounts.Outro = intValue;
                        break;
                    case "GRAB_FRAME_COUNT":
                        FrameCounts.Grab = intValue;
                        break;
                    case "WALK_IDLE_FRAME_COUNT":
                        FrameCounts.WalkIdle = intValue;
                        break;
                    case "CLICK_FRAME_COUNT":
                        FrameCounts.Click = intValue;
                        break;
                    case "HOVER_FRAME_COUNT":
                        FrameCounts.Hover = intValue;
                        break;
                }
            }
        }
        private void SpriteImage_RightClick(object sender, MouseButtonEventArgs e)
        {
            States.IsClick = !States.IsClick;
            CurrentFrames.Hover = 0;
        }

        private void SpriteImage_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            States.IsHover = true;
            States.IsIntro = false;
        }

        private void SpriteImage_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            States.IsHover = false;
            CurrentFrames.Hover = 0;
        }

    }



}
