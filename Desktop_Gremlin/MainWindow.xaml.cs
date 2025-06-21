using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Media.Animation;
namespace Desktop_Gremlin
{
    public partial class MainWindow : Window
    {
        private List<BitmapImage> IDLE_FRAMES = new List<BitmapImage>();
        private List<BitmapImage> DRAG_FRAMES = new List<BitmapImage>();
        private List<BitmapImage> WALK_LEFT_FRAMES = new List<BitmapImage>();
        private List<BitmapImage> WALK_RIGHT_FRAMES = new List<BitmapImage>();
        private List<BitmapImage> WALK_UP_FRAMES = new List<BitmapImage>();
        private List<BitmapImage> WALK_DOWN_FRAMES = new List<BitmapImage>();
        private List<BitmapImage> EMOTE1_FRAMES = new List<BitmapImage>();  
        private List<BitmapImage> INTRO_FRAMES = new List<BitmapImage>();   

        private DispatcherTimer INTRO_TIMER;
        private DispatcherTimer IDLE_TIMER;
        private DispatcherTimer WALK_TIMER;
        private DispatcherTimer DRAG_TIMER;
        private DispatcherTimer TYPING_TIMER;
        private DispatcherTimer EMOTE1_TIMER;

        private int CURRENT_IDLE_FRAME = 0;
        private int CURRENT_DRAG_FRAME = 0;
        private int CURRENT_WALK_FRAME = 0;
        private int CURRENT_EMOTE1_FRAME = 0;
        private int CURRENT_INTRO_FRAME = 0;    


        private bool IS_INTRO = true;
        private bool IS_WALKING = false;
        private bool IS_DRAGGING = false;
        private bool LAST_DRAG_STATE = false;
        private bool LAST_STATE_DRAG_OR_WALK = false;
        private bool IS_EMOTING1 = false;
        public enum Direction
        {
            Left,
            Right,
            Up,
            Down
        }
        private Direction CURRENT_DIRECTION;
        private double SPEED = 10.0;

        //Strictly for mouse movement
        //Currently working on how to adjust the sprite to move with mouse
        //V.2
        private bool MOVE_LEFT = false;
        private bool MOVE_RIGHT = false;
        private bool MOVE_UP = false;
        private bool MOVE_DOWN = false;
        private bool WALK_TO_CURSOR = false;
        private bool ALLOW_WALK_TO_CURSOR = false;
        private double MOUSE_DELTAX = 0;
        private double MOUSE_DELTAY = 0;
        private Point TARGET_POSITION;
        private Point LAST_CURSOR_POSITION;
        private DateTime LAST_CURSOR_MOVE_TIME;



        private DispatcherTimer TYPEWRITER_TIMER;
        private string FULL_SPEECH_TEXT = "";
        private int TYPING_INDEX = 0;


        private bool IS_SPEAKING = false;

        private double BUBBLE_WIDTH = 125; 


        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);
        public MainWindow()
        {
            InitializeComponent();
            PositionBottomLeft();
            LoadIdleFrames();
            LoadDragFrames();
            LoadWalkFrames();
            LoadEmote1();
            LoadIntroFrames();
            InitializeAnimationTimers();
            ApplySpriteSettings();
            ShowSpeech("Wazzup my skididsadsalda;dldsakdjsalkdksadsa");
        }
        public struct POINT
        {
            public int X;
            public int Y;
        }
        public class SpriteConfig
        {
            public double Width { get; set; } = 80;
            public double Height { get; set; } = 80;
            public double MarginLeft { get; set; } = 15;
            public double MarginTop { get; set; } = 50;
        }
        public SpriteConfig LoadConfig()
        {
            string configPath = "Config.txt";
            var config = new SpriteConfig();

            if (!File.Exists(configPath))
                return config;

            string[] lines = File.ReadAllLines(configPath);

            foreach (var line in lines)
            {
                var parts = line.Split('=');
                if (parts.Length != 2) continue;

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                switch (key)
                {
                    case "Width":
                        if (double.TryParse(value, out double w)) config.Width = w;
                        break;
                    case "Height":
                        if (double.TryParse(value, out double h)) config.Height = h;
                        break;
                    case "MarginLeft":
                        if (double.TryParse(value, out double ml)) config.MarginLeft = ml;
                        break;
                    case "MarginTop":
                        if (double.TryParse(value, out double mt)) config.MarginTop = mt;
                        break;
                }
            }

            return config;
        }
        private void ApplySpriteSettings()
        {
            SpriteConfig config = LoadConfig();

            SpriteImage.Width = config.Width;
            SpriteImage.Height = config.Height;
            SpriteImage.Margin = new Thickness(config.MarginLeft, config.MarginTop, 0, 0);
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                MOVE_LEFT = true;
                IS_WALKING = true;
                CURRENT_DIRECTION = Direction.Left;
            }
            else if (e.Key == Key.Right)
            {
                MOVE_RIGHT = true;
                IS_WALKING = true;
                CURRENT_DIRECTION = Direction.Right;
            }
            else if (e.Key == Key.Up)
            {
                MOVE_UP = true;
                IS_WALKING = true;
                CURRENT_DIRECTION = Direction.Up;
            }
            else if (e.Key == Key.Down)
            {
                MOVE_DOWN = true;
                IS_WALKING = true;
                CURRENT_DIRECTION = Direction.Down;
            }
            else if (e.Key == Key.Space)
            {
                if (!ALLOW_WALK_TO_CURSOR)
                {
                    ALLOW_WALK_TO_CURSOR = true;
                }
                else
                {
                    ALLOW_WALK_TO_CURSOR = false;
                    IS_WALKING = false;
                    WALK_TO_CURSOR = false;
                }
            }
            else if(e.Key == Key.Z)
            {
                if (!IS_EMOTING1)
                {
                    IS_EMOTING1 = true; 
                }
                else
                {
                    IS_EMOTING1 = false;
                }
            }
            else if(e.Key == Key.Tab)
            {
                if (IS_SPEAKING == false)
                {
                    IS_SPEAKING = true; 
                    ShowSpeech("Hello! I'm your desktop gremlin!");
                }                  
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left) MOVE_LEFT = false;
            if (e.Key == Key.Right) MOVE_RIGHT = false;
            if (e.Key == Key.Up) MOVE_UP = false;
            if (e.Key == Key.Down) MOVE_DOWN = false;
            IS_WALKING = MOVE_LEFT || MOVE_RIGHT || MOVE_DOWN || MOVE_UP;
        }
        private void LoadIntroFrames()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sprites", "Intro");
            INTRO_FRAMES = LoadFramesFromFolder(path, "Frame Folder");
        }
        private void LoadIdleFrames()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sprites", "Idle");
            IDLE_FRAMES = LoadFramesFromFolder(path, "Idle folder");
        }

        private void LoadDragFrames()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sprites", "Drag");
            DRAG_FRAMES = LoadFramesFromFolder(path, "Drag folder");
        }
        private void LoadEmote1()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sprites", "Emotes", "Emotes1");
            EMOTE1_FRAMES = LoadFramesFromFolder(path, "Emote folder");
        }

        private void LoadWalkFrames()
        {
            string basePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sprites", "Walk");

            WALK_LEFT_FRAMES = LoadFramesFromFolder(System.IO.Path.Combine(basePath, "Left"), "Walk Left folder");
            WALK_RIGHT_FRAMES = LoadFramesFromFolder(System.IO.Path.Combine(basePath, "Right"), "Walk Right folder");
            WALK_UP_FRAMES = LoadFramesFromFolder(System.IO.Path.Combine(basePath, "Up"), "Walk Up folder");
            WALK_DOWN_FRAMES = LoadFramesFromFolder(System.IO.Path.Combine(basePath, "Down"), "Walk Down folder");

            // Fallback if Up/Down are missing
            if (WALK_UP_FRAMES.Count == 0)
                WALK_UP_FRAMES = new List<BitmapImage>(WALK_RIGHT_FRAMES);
            if (WALK_DOWN_FRAMES.Count == 0)
                WALK_DOWN_FRAMES = new List<BitmapImage>(WALK_LEFT_FRAMES);
        }
        private List<BitmapImage> LoadDirectionFrames(string path)
        {
            var list = new List<BitmapImage>();

            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path, "*.png");
                Array.Sort(files, (a, b) =>
                {
                    int numA = int.Parse(System.IO.Path.GetFileNameWithoutExtension(a));
                    int numB = int.Parse(System.IO.Path.GetFileNameWithoutExtension(b));
                    return numA.CompareTo(numB);
                });

                foreach (var file in files)
                {
                    list.Add(new BitmapImage(new Uri(file, UriKind.Absolute)));
                }
            }

            return list;
        }
        private List<BitmapImage> LoadFramesFromFolder(string folderPath, string folderDescription = "Folder")
        {
            var frames = new List<BitmapImage>();

            if (!Directory.Exists(folderPath))
            {
                //MessageBox.Show($"{folderDescription} not found: {folderPath}");
                return frames;
            }

            string[] frameFiles = Directory.GetFiles(folderPath, "*.png");

            Array.Sort(frameFiles, (a, b) =>
            {
                int numA = int.Parse(System.IO.Path.GetFileNameWithoutExtension(a));
                int numB = int.Parse(System.IO.Path.GetFileNameWithoutExtension(b));
                return numA.CompareTo(numB);
            });

            foreach (var filePath in frameFiles)
            {
                frames.Add(new BitmapImage(new Uri(filePath, UriKind.Absolute)));
            }

            return frames;
        }
        public static double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
        private void InitializeAnimationTimers()
        {
            INTRO_TIMER = new DispatcherTimer();
            INTRO_TIMER.Interval = TimeSpan.FromMilliseconds(300);
            INTRO_TIMER.Tick += (s, e) =>
            {
                if (IS_INTRO)
                {
                    SpriteImage.Source = INTRO_FRAMES[CURRENT_INTRO_FRAME];
                    CURRENT_INTRO_FRAME = (CURRENT_INTRO_FRAME + 1) % INTRO_FRAMES.Count;
                    if (CURRENT_INTRO_FRAME == INTRO_FRAMES.Count - 1)
                    {
                        IS_INTRO = false;
                    }
                    SpriteImage.RenderTransform = new ScaleTransform(1, 1);
                }
            };


            IDLE_TIMER = new DispatcherTimer();
            IDLE_TIMER.Interval = TimeSpan.FromMilliseconds(300);
            IDLE_TIMER.Tick += (s, e) =>
            {

                if (!IS_INTRO && !IS_WALKING && !IS_DRAGGING && !WALK_TO_CURSOR && !IS_EMOTING1 && IDLE_FRAMES.Count > 0)
                {
                    SpriteImage.Source = IDLE_FRAMES[CURRENT_IDLE_FRAME];
                    CURRENT_IDLE_FRAME = (CURRENT_IDLE_FRAME + 1) % IDLE_FRAMES.Count;
                    SpriteImage.RenderTransform = new ScaleTransform(1, 1);
                }
            };

            EMOTE1_TIMER = new DispatcherTimer();
            EMOTE1_TIMER.Interval = TimeSpan.FromMilliseconds(200);
            EMOTE1_TIMER.Tick += (s, e) =>
            {
                if (!WALK_TO_CURSOR && !IS_DRAGGING && !IS_WALKING &&IS_EMOTING1 && EMOTE1_FRAMES.Count > 0)
                {
                    SpriteImage.Source = EMOTE1_FRAMES[CURRENT_EMOTE1_FRAME];
                    CURRENT_EMOTE1_FRAME = (CURRENT_EMOTE1_FRAME + 1) % EMOTE1_FRAMES.Count;
                    SpriteImage.RenderTransform = new ScaleTransform(1, 1);
                }
            };

            DRAG_TIMER = new DispatcherTimer();
            DRAG_TIMER.Interval = TimeSpan.FromMilliseconds(120);
            DRAG_TIMER.Tick += (s, e) =>
            {
                if (IS_DRAGGING && DRAG_FRAMES.Count > 0)
                {
                    SpriteImage.Source = DRAG_FRAMES[CURRENT_DRAG_FRAME];
                    CURRENT_DRAG_FRAME = (CURRENT_DRAG_FRAME + 1) % DRAG_FRAMES.Count;
                    ALLOW_WALK_TO_CURSOR = false;
                    WALK_TO_CURSOR = false;
                    IS_INTRO = false;   
                }
            };

            WALK_TIMER = new DispatcherTimer();
            WALK_TIMER.Interval = TimeSpan.FromMilliseconds(60);
            WALK_TIMER.Tick += (s, e) =>
            {
                if (IS_WALKING)
                {
                    MOUSE_DELTAX = 0;
                    MOUSE_DELTAY = 0;


                    if (MOVE_LEFT) MOUSE_DELTAX -= SPEED;
                    if (MOVE_RIGHT) MOUSE_DELTAX += SPEED;
                    if (MOVE_UP) MOUSE_DELTAY -= SPEED;
                    if (MOVE_DOWN) MOUSE_DELTAY += SPEED;

                    if (MOUSE_DELTAX != 0 && MOUSE_DELTAY != 0)
                    {
                        double length = Math.Sqrt(MOUSE_DELTAX * MOUSE_DELTAX + MOUSE_DELTAY * MOUSE_DELTAY);
                        MOUSE_DELTAX = (MOUSE_DELTAX / length) * SPEED;
                        MOUSE_DELTAY = (MOUSE_DELTAY / length) * SPEED;
                    }

                    this.Left = Clamp(this.Left + MOUSE_DELTAX, 0, SystemParameters.PrimaryScreenWidth - SpriteImage.Width);
                    this.Top = Clamp(this.Top + MOUSE_DELTAY, 0, SystemParameters.PrimaryScreenHeight - SpriteImage.Height);

                    List<BitmapImage> frames;
                    if (Math.Abs(MOUSE_DELTAX) > Math.Abs(MOUSE_DELTAY))
                    {
                        frames = MOUSE_DELTAX > 0 ? WALK_RIGHT_FRAMES : WALK_LEFT_FRAMES;
                        if (MOUSE_DELTAX > 0)
                        {
                            CURRENT_DIRECTION = Direction.Right;
                        }
                        else
                        {
                            CURRENT_DIRECTION = Direction.Left;
                        }
                        //CURRENT_DIRECTION = deltaX > 0 ? Direction.Right : Direction.Left;
                    }
                    else
                    {
                        frames = MOUSE_DELTAY > 0 ? WALK_DOWN_FRAMES : WALK_UP_FRAMES;
                        CURRENT_DIRECTION = MOUSE_DELTAY > 0 ? Direction.Down : Direction.Up;
                    }
                    if (frames.Count > 0)
                    {
                        SpriteImage.Source = frames[CURRENT_WALK_FRAME];
                        CURRENT_WALK_FRAME = (CURRENT_WALK_FRAME + 1) % frames.Count;
                    }
                }
                if (ALLOW_WALK_TO_CURSOR)
                {
                    GetCursorPos(out POINT cursorPos);
                    Point currentCursorPos = new Point(cursorPos.X - this.Width / 2, cursorPos.Y - this.Height / 2);

                    if ((currentCursorPos - LAST_CURSOR_POSITION).Length > 1)
                    {             
                        LAST_CURSOR_POSITION = currentCursorPos;
                        LAST_CURSOR_MOVE_TIME = DateTime.Now;
                        TARGET_POSITION = currentCursorPos;
                        WALK_TO_CURSOR = true;
                    }
                 
                    SetTargetToCursor();
                    Vector toTarget = TARGET_POSITION - new Point(this.Left, this.Top);
                    double distance = toTarget.Length;
                    if (WALK_TO_CURSOR)
                    {

                        if (distance < SPEED)
                        {
                            WALK_TO_CURSOR = false;
                            IS_WALKING = false;
                        }
                        else
                        {
                            toTarget.Normalize();
                            MOUSE_DELTAX = toTarget.X * SPEED;
                            MOUSE_DELTAY = toTarget.Y * SPEED;

                            this.Left += MOUSE_DELTAX;
                            this.Top += MOUSE_DELTAY;

                            if (Math.Abs(MOUSE_DELTAX) > Math.Abs(MOUSE_DELTAY))
                                CURRENT_DIRECTION = MOUSE_DELTAX > 0 ? Direction.Right : Direction.Left;
                            else
                                CURRENT_DIRECTION = MOUSE_DELTAY > 0 ? Direction.Down : Direction.Up;

                            List<BitmapImage> frames;

                            switch (CURRENT_DIRECTION)
                            {
                                case Direction.Left:
                                    frames = WALK_LEFT_FRAMES;
                                    break;
                                case Direction.Right:
                                    frames = WALK_RIGHT_FRAMES;
                                    break;
                                case Direction.Up:
                                    frames = WALK_UP_FRAMES;
                                    break;
                                case Direction.Down:
                                    frames = WALK_DOWN_FRAMES;
                                    break;
                                default:
                                    frames = WALK_RIGHT_FRAMES;
                                    break;
                            }

                            if (frames.Count > 0)
                            {
                                SpriteImage.Source = frames[CURRENT_WALK_FRAME];
                                CURRENT_WALK_FRAME = (CURRENT_WALK_FRAME + 1) % frames.Count;
                            }
                        }
                    }
                }       

            };
            INTRO_TIMER.Start();
            EMOTE1_TIMER.Start();   
            IDLE_TIMER.Start();
            WALK_TIMER.Start();
            DRAG_TIMER.Start();
        }
      
        public void HideSpeech()
        {
            var shrinkAnimation = new DoubleAnimation
            {
                From = BUBBLE_WIDTH,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            shrinkAnimation.Completed += (s, e) =>
            {
                SpeechBubbleBorder.Visibility = Visibility.Collapsed;
                SpriteSpeech.Text = "";
                 IS_SPEAKING = false;
            };

            SpeechBubbleBorder.BeginAnimation(Border.WidthProperty, shrinkAnimation);
        }

        public void ShowSpeech(string message)
        {
            FULL_SPEECH_TEXT = message;
            TYPING_INDEX = 0;
            SpriteSpeech.Text = "";

            SpeechBubbleBorder.Visibility = Visibility.Visible;

            int durationPerCharacter = 30;
            int durationMs = message.Length * durationPerCharacter;

            if (durationMs < 200)
            {
                durationMs = 200;
            }

            var expandAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(durationMs),
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
            };

            expandAnimation.Completed += (s, e) => StartTyping();

            SpeechBubbleBorder.BeginAnimation(Border.WidthProperty, expandAnimation);
        }
        private void StartTyping()
        {
            TYPING_TIMER = new DispatcherTimer();
            TYPING_TIMER.Interval = TimeSpan.FromMilliseconds(40);
            TYPING_TIMER.Tick += (s, e) =>
            {
                if (TYPING_INDEX < FULL_SPEECH_TEXT.Length)
                {
                    SpriteSpeech.Text += FULL_SPEECH_TEXT[TYPING_INDEX];
                    TYPING_INDEX++;
                }
                else
                {
                    TYPING_TIMER.Stop();
                }
            };
            TYPING_TIMER.Start();

            DispatcherTimer autoClose = new DispatcherTimer();
            autoClose.Interval = TimeSpan.FromSeconds(5);
            autoClose.Tick += (s, e) =>
            {
                SpriteSpeech.Text = "";
                HideSpeech();
                autoClose.Stop();
            };
            autoClose.Start();
        }

        private void SetTargetToCursor()
        {
            GetCursorPos(out POINT pos);


            double targetX = Clamp(pos.X - this.Width, 0, SystemParameters.PrimaryScreenWidth - SpriteImage.Width)             ;
            double targetY = Clamp(pos.Y - this.Height, 0, SystemParameters.PrimaryScreenHeight - SpriteImage.Height);

            TARGET_POSITION = new Point(targetX, targetY);
            WALK_TO_CURSOR = true;
        }

        private void PositionBottomLeft()
        {
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            this.Left = 0;
            this.Top = screenHeight - this.Height + 20;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IS_DRAGGING = true;
            DragMove();
            IS_DRAGGING = false;
        }
    }

}
