using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Desktop_Gremlin
{
    internal class SpriteLoader
    {
        public List<BitmapImage> IDLE_FRAMES = new List<BitmapImage>();
        public List<BitmapImage> DRAG_FRAMES = new List<BitmapImage>();
        private List<BitmapImage> WALK_LEFT_FRAMES = new List<BitmapImage>();
        private List<BitmapImage> WALK_RIGHT_FRAMES = new List<BitmapImage>();
        private List<BitmapImage> WALK_UP_FRAMES = new List<BitmapImage>();
        private List<BitmapImage> WALK_DOWN_FRAMES = new List<BitmapImage>();
        private List<BitmapImage> EMOTE1_FRAMES = new List<BitmapImage>();
        public  List<BitmapImage> INTRO_FRAMES = new List<BitmapImage>();

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

        public void LoadAll()
        {
            string BASE_DIR = AppDomain.CurrentDomain.BaseDirectory;

            INTRO_FRAMES = LoadFramesFromFolder(Path.Combine(BASE_DIR, "Sprites", "Intro"));
            IDLE_FRAMES = LoadFramesFromFolder(Path.Combine(BASE_DIR, "Sprites", "Idle"));
            DRAG_FRAMES = LoadFramesFromFolder(Path.Combine(BASE_DIR, "Sprites", "Drag"));
            EMOTE1_FRAMES = LoadFramesFromFolder(Path.Combine(BASE_DIR, "Sprites", "Emotes", "Emotes1"));

            string WALK_BASE = Path.Combine(BASE_DIR, "Sprites", "Walk");
            WALK_LEFT_FRAMES = LoadFramesFromFolder(Path.Combine(WALK_BASE, "Left"));
            WALK_RIGHT_FRAMES = LoadFramesFromFolder(Path.Combine(WALK_BASE, "Right"));
            WALK_UP_FRAMES = LoadFramesFromFolder(Path.Combine(WALK_BASE, "Up"));
            WALK_DOWN_FRAMES = LoadFramesFromFolder(Path.Combine(WALK_BASE, "Down"));

            if (WALK_UP_FRAMES.Count == 0)
                WALK_UP_FRAMES = new List<BitmapImage>(WALK_RIGHT_FRAMES);

            if (WALK_DOWN_FRAMES.Count == 0)
                WALK_DOWN_FRAMES = new List<BitmapImage>(WALK_LEFT_FRAMES);
        }
    }

}
