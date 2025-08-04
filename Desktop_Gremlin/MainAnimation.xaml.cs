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
using System.Windows.Threading;

namespace Desktop_Gremlin
{
    /// <summary>
    /// Interaction logic for MainAnimation.xaml
    /// </summary>
    public partial class MainAnimation : Window
    {
        private SpriteLoader SPRITE_LOADER = new SpriteLoader();

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

        private bool IS_INTRO = false;
        private bool IS_WALKING = false;
        private bool IS_DRAGGING = false;
        private bool LAST_DRAG_STATE = false;
        private bool LAST_STATE_DRAG_OR_WALK = false;
        private bool IS_EMOTING1 = false;

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


        public MainAnimation()
        {
            InitializeComponent();
            SPRITE_LOADER.LoadAll();
            InitializeAnimationTimers();
        }

        private void InitializeAnimationTimers()
        {
            INTRO_TIMER = new DispatcherTimer();
            INTRO_TIMER.Interval = TimeSpan.FromMilliseconds(33); // Make this 300 if using sprite base - Kurt;
            INTRO_TIMER.Tick += (s, e) =>
            {
                if (IS_INTRO && !IS_WALKING)
                {
                    SpriteImage.Source = SPRITE_LOADER.INTRO_FRAMES[CURRENT_INTRO_FRAME];
                    CURRENT_INTRO_FRAME = (CURRENT_INTRO_FRAME + 1) % SPRITE_LOADER.INTRO_FRAMES.Count;
                    if (CURRENT_INTRO_FRAME == SPRITE_LOADER.INTRO_FRAMES.Count - 1)
                    {
                        IS_INTRO = false;
                    }
                    SpriteImage.RenderTransform = new ScaleTransform(1, 1);
                }
            };
            //DRAG_TIMER = new DispatcherTimer();
            //DRAG_TIMER.Interval = TimeSpan.FromMilliseconds(33);
            //DRAG_TIMER.Tick += (s, e) =>
            //{
            //    if (IS_DRAGGING && SPRITE_LOADER.Count > 0)
            //    {
            //        SpriteImage.Source = SPRITE_LOADER.DRAG_FRAMES[CURRENT_DRAG_FRAME];
            //        CURRENT_DRAG_FRAME = (CURRENT_DRAG_FRAME + 1) % SPRITE_LOADER.DRAG_FRAMES.Count;
            //        ALLOW_WALK_TO_CURSOR = false;
            //        WALK_TO_CURSOR = false;
            //        IS_INTRO = false;
            //    }
            //};
            IDLE_TIMER = new DispatcherTimer();
            IDLE_TIMER.Interval = TimeSpan.FromMilliseconds(33);
            IDLE_TIMER.Tick += (s, e) =>
            {

                if (!IS_WALKING && !IS_DRAGGING && !WALK_TO_CURSOR && !IS_INTRO && SPRITE_LOADER.IDLE_FRAMES.Count > 0)
                {
                    SpriteImage.Source = SPRITE_LOADER.IDLE_FRAMES[CURRENT_IDLE_FRAME];
                    CURRENT_IDLE_FRAME = (CURRENT_IDLE_FRAME + 1) % SPRITE_LOADER.IDLE_FRAMES.Count;
                    SpriteImage.RenderTransform = new ScaleTransform(1, 1);
                }
            };

            IDLE_TIMER.Start();
            //INTRO_TIMER.Start();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IS_DRAGGING = true;
            DragMove();
            IS_DRAGGING = false;
        }
    }
}
