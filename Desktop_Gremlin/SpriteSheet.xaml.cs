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

namespace Desktop_Gremlin
{
    /// <summary>
    /// Interaction logic for SpriteSheet.xaml
    /// </summary>
    public partial class SpriteSheet : Window
    {

        private const int IDLE_FRAME_WIDTH = 300;
        private const int IDLE_FRAME_HEIGHT = 300;
        private const int IDLE_FRAME_COUNT = 41;
        private const int GRAB_FRAME_COUNT = 58;
        private int CURRENT_IDLE_FRAME = 0;
        private int CURRENT_GRAB_FRAME = 0;

        private BitmapImage IDLE_SHEET;
        private BitmapImage GRAB_SHEET;
        private DispatcherTimer IDLE_TIMER;
        private DispatcherTimer GRAB_TIMER;

        private bool IS_DRAGGING = false;  
        public SpriteSheet()
        {
            InitializeComponent();
            LoadSpritesSheet();
            InitializeAnimations();
        }

        private void LoadSpritesSheet()
        {
            IDLE_SHEET = new BitmapImage();
            IDLE_SHEET.BeginInit();
            IDLE_SHEET.UriSource = new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sprites", "SpriteSheet", "Idle.png"));
            IDLE_SHEET.CacheOption = BitmapCacheOption.OnLoad;

            IDLE_SHEET.EndInit();
            IDLE_SHEET.Freeze(); 



            GRAB_SHEET = new BitmapImage();
            GRAB_SHEET.BeginInit();
            GRAB_SHEET.UriSource = new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sprites", "SpriteSheet", "grab.png"));
            GRAB_SHEET.CacheOption = BitmapCacheOption.OnLoad;

            GRAB_SHEET.EndInit();
            GRAB_SHEET.Freeze(); 
        }

        private void InitializeAnimations()
        {
            IDLE_TIMER = new DispatcherTimer();
            IDLE_TIMER.Interval = TimeSpan.FromMilliseconds(33); // Adjust speed
            IDLE_TIMER.Tick += (s, e) =>
            {
                if (IDLE_SHEET == null || IS_DRAGGING)
                    return;

                int columns = 5;
                int x = (CURRENT_IDLE_FRAME % columns) * IDLE_FRAME_WIDTH;
                int y = (CURRENT_IDLE_FRAME / columns) * IDLE_FRAME_HEIGHT;


                if (x + IDLE_FRAME_WIDTH > IDLE_SHEET.PixelWidth || y + IDLE_FRAME_HEIGHT > IDLE_SHEET.PixelHeight)
                    return;

                var cropped = new CroppedBitmap(
                    IDLE_SHEET,
                    new Int32Rect(x, y, IDLE_FRAME_WIDTH, IDLE_FRAME_HEIGHT)
                );

                SpriteImage.Source = cropped;

                CURRENT_IDLE_FRAME = (CURRENT_IDLE_FRAME + 1) % IDLE_FRAME_COUNT;
            };
           
            GRAB_TIMER = new DispatcherTimer();
            GRAB_TIMER.Interval = TimeSpan.FromMilliseconds(33);
            GRAB_TIMER.Tick += (s, e) =>
            {
                if (GRAB_SHEET == null || !IS_DRAGGING)
                {
                    return;
                }

                int columns = 5;
                int x = (CURRENT_GRAB_FRAME % columns) * IDLE_FRAME_WIDTH;
                int y = (CURRENT_GRAB_FRAME / columns) * IDLE_FRAME_HEIGHT;


                if (x + IDLE_FRAME_WIDTH > GRAB_SHEET.PixelWidth || y + IDLE_FRAME_HEIGHT > GRAB_SHEET.PixelHeight)
                    return;

                var cropped = new CroppedBitmap(
                    GRAB_SHEET,
                    new Int32Rect(x, y, IDLE_FRAME_WIDTH, IDLE_FRAME_HEIGHT)
                );

                SpriteImage.Source = cropped;

                CURRENT_GRAB_FRAME = (CURRENT_GRAB_FRAME + 1) % GRAB_FRAME_COUNT;
            };
            



            GRAB_TIMER.Start(); 
            IDLE_TIMER.Start();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IS_DRAGGING = true;
            DragMove();
            IS_DRAGGING = false;

        }

    }
}
