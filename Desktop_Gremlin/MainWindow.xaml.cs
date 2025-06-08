using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Interop;
using System.IO;

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
        private bool LAST_STATE_DRAG_OR_WALK = false; // for animation reset
        public enum Direction
        {
            Left,
            Right,
            Up,
            Down
        }
        private Direction CURRENT_DIRECTION = Direction.Right;
        private double SPEED = 10.0; // pixels per tick

        private bool MOVE_LEFT = false;
        private bool MOVE_RIGHT = false;
        private bool MOVE_UP = false;
        private bool MOVE_DOWN = false;
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

        private void InitializeAnimationTimers()
        {
            // Idle Animation
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

            // Walk Animation
            WALK_TIMER = new DispatcherTimer();
            WALK_TIMER.Interval = TimeSpan.FromMilliseconds(60);
            WALK_TIMER.Tick += (s, e) =>
            {
                if (IS_WALKING)
                {
                    switch (CURRENT_DIRECTION)
                    {
                        case 
                        Direction.Left: this.Left = Math.Max(0, this.Left - SPEED); 
                            break;
                        case Direction.Right: this.Left = Math.Min(SystemParameters.PrimaryScreenWidth - this.Width, this.Left + SPEED); break;
                        case Direction.Up: this.Top = Math.Max(0, this.Top - SPEED); break;
                        case Direction.Down: this.Top = Math.Min(SystemParameters.PrimaryScreenHeight - this.Height, this.Top + SPEED); break;
                    }

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
                            frames = WALK_UP_FRAMES.Count > 0 ? WALK_UP_FRAMES : WALK_RIGHT_FRAMES;
                            break;
                        case Direction.Down:
                            frames = WALK_DOWN_FRAMES.Count > 0 ? WALK_DOWN_FRAMES : WALK_LEFT_FRAMES;
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
            };

            // Drag Animation
            DRAG_TIMER = new DispatcherTimer();
            DRAG_TIMER.Interval = TimeSpan.FromMilliseconds(60);
            DRAG_TIMER.Tick += (s, e) =>
            {
                if (IS_DRAGGING && DRAG_FRAMES.Count > 0)
                {
                    SpriteImage.Source = DRAG_FRAMES[CURRENT_DRAG_FRAME];
                    CURRENT_DRAG_FRAME = (CURRENT_DRAG_FRAME + 1) % DRAG_FRAMES.Count;
                }
            };

            // Start timers (they will be conditionally used)
            IDLE_TIMER.Start();
            WALK_TIMER.Start();
            DRAG_TIMER.Start();
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
