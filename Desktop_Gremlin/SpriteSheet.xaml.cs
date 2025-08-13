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
using System.Windows.Shapes;
using System.IO;
using System.Windows.Threading;
using static Desktop_Gremlin.MainWindow;

namespace Desktop_Gremlin
{
    /// <summary>
    /// Interaction logic for SpriteSheet.xaml
    /// </summary>
    public partial class SpriteSheet : Window
    {

        private const int FRAME_WIDTH = 300;
        private const int FRAME_HEIGHT = 300;
        private const int IDLE_FRAME_COUNT = 41;
        private const int GRAB_FRAME_COUNT = 105;
        private const int WALK_DOWN_FRAME = 17;
        private const int WALK_UP_FRAME = 17; 
        private const int WALK_LEFT_FRAME = 18;
        private const int WALK_RIGHT_FRAME = 18;
        private const int EMOTE_1 = 61;

        private int CURRENT_WALK_DOWN_FRAME = 0;
        private int CURRENT_WALK_UP_FRAME = 0;  
        private int CURRENT_WALK_RIGHT_FRAME = 0;
        private int CURRENT_WALK_UP_LEFT_FRAME = 0;
        private int CURRENT_IDLE_FRAME = 0;
        private int CURRENT_GRAB_FRAME = 0;
        private int CURRENT_WALK_LEFT_FRAME = 0;
        private int CURRENT_EMOTE_1 = 0;

        private int FRAME_RRATE = 30;

        private BitmapImage WALK_DOWN_SHEET;
        private BitmapImage IDLE_SHEET;
        private BitmapImage GRAB_SHEET;
        private BitmapImage WALK_UP_SHEET;  
        private BitmapImage WALK_LEFT_SHEET;
        private BitmapImage WALK_RIGHT_SHEET;   
        private BitmapImage EMOTE_SHEET;

        private DispatcherTimer WALK_TIMER;   
        private DispatcherTimer IDLE_TIMER;
        private DispatcherTimer GRAB_TIMER;
        private DispatcherTimer EMOTE1_TIMER;

        private bool IS_INTRO = true;
        private bool IS_WALKING = false;
        private bool IS_DRAGGING = false;
        private bool LAST_DRAG_STATE = false;
        private bool LAST_STATE_DRAG_OR_WALK = false;
        private bool IS_EMOTING1 = true;

        private bool WALK_TO_CURSOR = false;
        private bool ALLOW_WALK_TO_CURSOR = false;
        private double MOUSE_DELTAX = 0;
        private double MOUSE_DELTAY = 0;

        private Direction CURRENT_DIRECTION;
        private double SPEED = 10.0;
        public enum Direction
        {
            Left,
            Right,
            Up,
            Down
        }

        private bool MOVE_LEFT = false;
        private bool MOVE_RIGHT = false;
        private bool MOVE_UP = false;
        private bool MOVE_DOWN = false;
        public SpriteSheet()
        {
            InitializeComponent();
            LoadSpritesSheet();
            InitializeAnimations();
        }

        private BitmapImage LoadSprite(string fileName)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sprites", "SpriteSheet", fileName)
            );
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            return image;
        }
        private void LoadSpritesSheet()
        {
            IDLE_SHEET = LoadSprite("Idle.png");
            GRAB_SHEET = LoadSprite("grab.png");
            WALK_DOWN_SHEET = LoadSprite("run_down.png");
            WALK_UP_SHEET = LoadSprite("run_up.png");
            WALK_LEFT_SHEET = LoadSprite("run_left.png");
            WALK_RIGHT_SHEET = LoadSprite("run_right.png");
            EMOTE_SHEET = LoadSprite("emote1.png");
        }
        private int PlayAnimationFrame(BitmapImage sheet, int currentFrame, int frameCount, int columns = 5)
        {
            if (sheet == null)
                return currentFrame;

            int x = (currentFrame % columns) * FRAME_WIDTH;
            int y = (currentFrame / columns) * FRAME_HEIGHT;

            if (x + FRAME_WIDTH > sheet.PixelWidth || y + FRAME_HEIGHT > sheet.PixelHeight)
                return currentFrame;

            SpriteImage.Source = new CroppedBitmap(sheet, new Int32Rect(x, y, FRAME_WIDTH, FRAME_HEIGHT));

            return (currentFrame + 1) % frameCount;
        }

        private void InitializeAnimations()
        {

            EMOTE1_TIMER = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(FRAME_RRATE) };
            EMOTE1_TIMER.Tick += (s, e) =>
            {
                if(IS_WALKING || IS_DRAGGING) {
                    IS_EMOTING1 = false;
                    EMOTE1_TIMER.Stop();
                };
                if (IS_EMOTING1)
                    CURRENT_EMOTE_1 = PlayAnimationFrame(EMOTE_SHEET, CURRENT_EMOTE_1, EMOTE_1);

                if (CURRENT_EMOTE_1 == 0)
                {
                    IS_EMOTING1 = false;
                    EMOTE1_TIMER.Stop();
                }
            };

            IDLE_TIMER = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(FRAME_RRATE) };
            IDLE_TIMER.Tick += (s, e) =>
            {
                if (!IS_DRAGGING && !IS_WALKING && !IS_EMOTING1)
                    CURRENT_IDLE_FRAME = PlayAnimationFrame(IDLE_SHEET, CURRENT_IDLE_FRAME, IDLE_FRAME_COUNT);
            };

            GRAB_TIMER = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(FRAME_RRATE) };
            GRAB_TIMER.Tick += (s, e) =>
            {
                if (IS_DRAGGING)
                    CURRENT_GRAB_FRAME = PlayAnimationFrame(GRAB_SHEET, CURRENT_GRAB_FRAME, GRAB_FRAME_COUNT);
                    
            };
            WALK_TIMER = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(FRAME_RRATE) };
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
                    this.Left = this.Left + MOUSE_DELTAX;
                    this.Top = this.Top + MOUSE_DELTAY;
                    //SpriteLabel.Content = this.Left.ToString() + " > " + this.Top.ToString() ;


                    if (Math.Abs(MOUSE_DELTAX) > Math.Abs(MOUSE_DELTAY))
                    {
                        if (MOUSE_DELTAX < 0)
                            CURRENT_WALK_LEFT_FRAME = PlayAnimationFrame(WALK_LEFT_SHEET, CURRENT_WALK_LEFT_FRAME, WALK_LEFT_FRAME);
                        else
                            CURRENT_WALK_RIGHT_FRAME = PlayAnimationFrame(WALK_RIGHT_SHEET, CURRENT_WALK_RIGHT_FRAME, WALK_RIGHT_FRAME);
                    }
                    else
                    {
                        if (MOUSE_DELTAY > 0)
                            CURRENT_WALK_DOWN_FRAME = PlayAnimationFrame(WALK_DOWN_SHEET, CURRENT_WALK_DOWN_FRAME, WALK_DOWN_FRAME);
                        else
                            CURRENT_WALK_UP_FRAME = PlayAnimationFrame(WALK_UP_SHEET, CURRENT_WALK_UP_FRAME, WALK_UP_FRAME);
                    }
                } 
            };
            EMOTE1_TIMER.Start();
            WALK_TIMER.Start();
            GRAB_TIMER.Start(); 
            IDLE_TIMER.Start();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IS_DRAGGING = true;
            DragMove();
            IS_DRAGGING = false;

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

    }
}
