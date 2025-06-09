using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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
        private Direction CURRENT_DIRECTION = Direction.Right;
        private double SPEED = 10.0;

        private bool MOVE_LEFT = false;
        private bool MOVE_RIGHT = false;
        private bool MOVE_UP = false;
        private bool MOVE_DOWN = false;

        private double deltaX = 0;
        private double deltaY = 0;

        public MainWindow()
        {
            InitializeComponent();
            PositionBottomLeft();
            LoadIdleFrames();
            LoadDragFrames();
            LoadWalkFrames();
            InitializeAnimationTimers();
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
                if (!IS_WALKING && !IS_DRAGGING && IDLE_FRAMES.Count > 0)
                {
                    SpriteImage.Source = IDLE_FRAMES[CURRENT_IDLE_FRAME];
                    CURRENT_IDLE_FRAME = (CURRENT_IDLE_FRAME + 1) % IDLE_FRAMES.Count;
                    SpriteImage.RenderTransform = new ScaleTransform(1, 1);
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
                    UpdateLabelPosition();
                    if (frames.Count > 0)
                    {
                        SpriteImage.Source = frames[CURRENT_WALK_FRAME];
                        CURRENT_WALK_FRAME = (CURRENT_WALK_FRAME + 1) % frames.Count;
                    }
                }

            };

            // DRAGGGGG
            DRAG_TIMER = new DispatcherTimer();
            DRAG_TIMER.Interval = TimeSpan.FromMilliseconds(120);
            DRAG_TIMER.Tick += (s, e) =>
            {
                if (IS_DRAGGING && DRAG_FRAMES.Count > 0)
                {
                    SpriteImage.Source = DRAG_FRAMES[CURRENT_DRAG_FRAME];
                    CURRENT_DRAG_FRAME = (CURRENT_DRAG_FRAME + 1) % DRAG_FRAMES.Count;
                }
                UpdateLabelPosition();
            };

            // Start timers (they will be conditionally used)
            IDLE_TIMER.Start();
            WALK_TIMER.Start();
            DRAG_TIMER.Start();
        }
        private void UpdateLabelPosition()
        {
            double offsetX = this.Width / 2 - SpriteLabel.ActualWidth / 2;
            string debugText = "offset:" + offsetX.ToString() + "\n"
                + "Left: " + this.Left.ToString() + "\n"
                + "Up: " + this.Top.ToString() + "\n"
                + "DeltaX: " + deltaX.ToString() + "\n"
                + "DeltaY: " + deltaY.ToString() + "\n";
            SpriteLabel.Content = debugText;
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
