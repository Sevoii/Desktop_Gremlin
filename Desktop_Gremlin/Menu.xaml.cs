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

namespace Desktop_Gremlin
{
    /// <summary>
    /// Interaction logic for Menu.xaml
    /// </summary>
    public partial class Menu : Window
    {
        public class SpriteConfig
        {
            public double Width { get; set; } = 80;
            public double Height { get; set; } = 80;
            public double MarginLeft { get; set; } = 15;
            public double MarginTop { get; set; } = 50;
        }


        public double SpriteWidth => double.TryParse(WidthBox.Text, out var w) ? w : 80;
        public double SpriteHeight => double.TryParse(HeightBox.Text, out var h) ? h : 80;
        public double MarginLeft => double.TryParse(MarginLeftBox.Text, out var ml) ? ml : 15;
        public double MarginTop => double.TryParse(MarginTopBox.Text, out var mt) ? mt : 50;

        public Menu()
        {
            InitializeComponent();
            LoadConfig();

            var config = LoadConfig();

            WidthBox.Text = config.Width.ToString();
            HeightBox.Text = config.Height.ToString();
            MarginLeftBox.Text = config.MarginLeft.ToString();
            MarginTopBox.Text = config.MarginTop.ToString();
        }

        public string ConfigPath = "config.txt";

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var config = new SpriteConfig
            {
                Width = SpriteWidth,
                Height = SpriteHeight,
                MarginLeft = MarginLeft,
                MarginTop = MarginTop
            };
            // You can pass this to MainWindow via event, shared service, or settings
            MessageBox.Show($"Width: {SpriteWidth}, Height: {SpriteHeight}, Margin: ({MarginLeft},{MarginTop})");
            // Optionally: close or hide menu
            SaveConfig(config);
        }
        public void SaveConfig(SpriteConfig config)
        {
            string[] lines =
            {
                $"Width={config.Width}",
                $"Height={config.Height}",
                $"MarginLeft={config.MarginLeft}",
                $"MarginTop={config.MarginTop}"
            };

            File.WriteAllLines(ConfigPath, lines);
        }

        public SpriteConfig LoadConfig()
        {
            var config = new SpriteConfig();

            if (!File.Exists(ConfigPath))
                return config;

            string[] lines = File.ReadAllLines(ConfigPath);

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

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

        }
    }
}
