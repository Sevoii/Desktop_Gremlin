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

        private DispatcherTimer IDLE_TIMER;
        private DispatcherTimer WALK_TIMER;
        private DispatcherTimer DRAG_TIMER;
        private DispatcherTimer TYPING_TIMER;

        private int CURRENT_IDLE_FRAME = 0;
        private int CURRENT_DRAG_FRAME = 0;
        private int CURRENT_WALK_FRAME = 0;
        private bool IS_WALKING = false;
        private bool IS_DRAGGING = false;
        private bool LAST_DRAG_STATE = false;
        private bool LAST_STATE_DRAG_OR_WALK = false;
        public enum Direction
        {
            Left,
            Right,
            Up,
            Down
        }
        private Direction CURRENT_DIRECTION;
        private double SPEED = 10.0;

        private bool MOVE_LEFT = false;
        private bool MOVE_RIGHT = false;
        private bool MOVE_UP = false;
        private bool MOVE_DOWN = false;

        private DispatcherTimer TYPEWRITER_TIMER;
        private string FULL_SPEECH_TEXT = "";
        private int TYPING_INDEX = 0;

        private double deltaX = 0;
        private double deltaY = 0;
        private bool WALK_TO_CURSOR = false;
        private bool ALLOW_WALK_TO_CURSOR = false;
        private Point TARGET_POSITION;
        private Point LAST_CURSOR_POSITION;
        private DateTime LAST_CURSOR_MOVE_TIME;
        private bool IS_SPEAKING = false;

        private double BUBBLE_WIDTH = 125; // Your target width


        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);
        public MainWindow()
        {
            InitializeComponent();
            PositionBottomLeft();
            LoadIdleFrames();
            LoadDragFrames();
            LoadWalkFrames();
            InitializeAnimationTimers();
        }
        public struct POINT
        {
            public int X;
            public int Y;
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


            // IDLING
            IDLE_TIMER = new DispatcherTimer();
            IDLE_TIMER.Interval = TimeSpan.FromMilliseconds(300);
            IDLE_TIMER.Tick += (s, e) =>
            {

                if (!IS_WALKING && !IS_DRAGGING && !WALK_TO_CURSOR && IDLE_FRAMES.Count > 0)
                {
                    SpriteImage.Source = IDLE_FRAMES[CURRENT_IDLE_FRAME];
                    CURRENT_IDLE_FRAME = (CURRENT_IDLE_FRAME + 1) % IDLE_FRAMES.Count;
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
                }
            };

            // WALKING 
            WALK_TIMER = new DispatcherTimer();
            WALK_TIMER.Interval = TimeSpan.FromMilliseconds(60);
            WALK_TIMER.Tick += (s, e) =>
            {
                if (IS_WALKING)
                {
                    deltaX = 0;
                    deltaY = 0;


                    if (MOVE_LEFT) deltaX -= SPEED;
                    if (MOVE_RIGHT) deltaX += SPEED;
                    if (MOVE_UP) deltaY -= SPEED;
                    if (MOVE_DOWN) deltaY += SPEED;

                    if (deltaX != 0 && deltaY != 0)
                    {
                        double length = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                        deltaX = (deltaX / length) * SPEED;
                        deltaY = (deltaY / length) * SPEED;
                    }

                    //this.Left = Clamp(this.Left + deltaX, 0, SystemParameters.PrimaryScreenWidth - this.Width);
                    //this.Top = Clamp(this.Top + deltaY, 0, SystemParameters.PrimaryScreenHeight - this.Height);
                    this.Left = this.Left + deltaX;
                    this.Top = this.Top + deltaY;
                    List<BitmapImage> frames;
                    if (Math.Abs(deltaX) > Math.Abs(deltaY))
                    {
                        frames = deltaX > 0 ? WALK_RIGHT_FRAMES : WALK_LEFT_FRAMES;
                        if (deltaX > 0)
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
                        frames = deltaY > 0 ? WALK_DOWN_FRAMES : WALK_UP_FRAMES;
                        CURRENT_DIRECTION = deltaY > 0 ? Direction.Down : Direction.Up;
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
                        // Cursor moved
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
                            // Stop walking if close enough
                            WALK_TO_CURSOR = false;
                            IS_WALKING = false;
                        }
                        else
                        {
                            toTarget.Normalize();
                            deltaX = toTarget.X * SPEED;
                            deltaY = toTarget.Y * SPEED;

                            this.Left += deltaX;
                            this.Top += deltaY;

                            // Choose frame direction
                            if (Math.Abs(deltaX) > Math.Abs(deltaY))
                                CURRENT_DIRECTION = deltaX > 0 ? Direction.Right : Direction.Left;
                            else
                                CURRENT_DIRECTION = deltaY > 0 ? Direction.Down : Direction.Up;

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


            IDLE_TIMER.Start();
            WALK_TIMER.Start();
            DRAG_TIMER.Start();
        }
        private void StartTyping(string message, int speedMs = 50)
        {
            FULL_SPEECH_TEXT = message;
            TYPING_INDEX = 0;
            SpriteSpeech.Text = "";

            if (TYPEWRITER_TIMER != null)
                TYPEWRITER_TIMER.Stop();

            TYPEWRITER_TIMER = new DispatcherTimer();
            TYPEWRITER_TIMER.Interval = TimeSpan.FromMilliseconds(speedMs);
            TYPEWRITER_TIMER.Tick += (s, e) =>
            {
                if (TYPING_INDEX < FULL_SPEECH_TEXT.Length)
                {
                    SpriteSpeech.Text = (string)SpriteSpeech.Text + FULL_SPEECH_TEXT[TYPING_INDEX];
                    TYPING_INDEX++;
                }
                else
                { 
                    TYPEWRITER_TIMER.Stop();
                }
            };

            TYPEWRITER_TIMER.Start();
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

            var expandAnimation = new DoubleAnimation
            {
                From = 0,
                To = BUBBLE_WIDTH,
                Duration = TimeSpan.FromMilliseconds(300),
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
                HideSpeech();
                autoClose.Stop();
            };
            autoClose.Start();
        }

        private void SetTargetToCursor()
        {
            GetCursorPos(out POINT pos);

            // Offset to center the gremlin around cursor
            double targetX = pos.X - this.Width / 2;
            double targetY = pos.Y - this.Height / 2;

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
