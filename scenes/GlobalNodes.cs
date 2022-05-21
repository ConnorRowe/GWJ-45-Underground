using Godot;

namespace Underground
{
    public class GlobalNodes : Node
    {
        public struct LevelData
        {
            public PackedScene Scene { get; }
            public Color BaseColour { get; }
            public int Number { get; }

            public LevelData(PackedScene scene, Color baseColour, int number)
            {
                Scene = scene;
                BaseColour = baseColour;
                Number = number;
            }
        }

        public static GlobalNodes INSTANCE { get; private set; }
        public static bool DisableCameraShake { get; set; } = false;

        private static Shaker cameraShaker;
        private static AudioStreamPlayer uiClick;
        private static AudioStreamPlayer boing;
        private static AudioStreamPlayer switchClick;
        private static AudioStreamPlayer pop;
        private static AudioStreamPlayer music;

        private static Tween tween;

        public static LevelData[] Levels { get; private set; } = new LevelData[9]
        {
            LoadLevelData("res://scenes/levels/StartLevel.tscn"),
            LoadLevelData("res://scenes/levels/Level1.tscn"),
            LoadLevelData("res://scenes/levels/Level2.tscn"),
            LoadLevelData("res://scenes/levels/Level3.tscn"),
            LoadLevelData("res://scenes/levels/Level4.tscn"),
            LoadLevelData("res://scenes/levels/Level5.tscn"),
            LoadLevelData("res://scenes/levels/Level6.tscn"),
            LoadLevelData("res://scenes/levels/Level7.tscn"),
            LoadLevelData("res://scenes/levels/Level8.tscn"),
        };

        public override void _Ready()
        {
            INSTANCE = this;

            cameraShaker = GetNode<Shaker>("Shaker");
            uiClick = GetNode<AudioStreamPlayer>("UIClick");
            boing = GetNode<AudioStreamPlayer>("Boing");
            switchClick = GetNode<AudioStreamPlayer>("SwitchClick");
            pop = GetNode<AudioStreamPlayer>("Pop");
            music = GetNode<AudioStreamPlayer>("Music");

            tween = new Tween();
            AddChild(tween);

            SaveData.ApplyLoadedSettings();
        }

        public override void _Input(InputEvent evt)
        {
            if (evt is InputEventKey ek && !ek.Pressed && ek.Scancode == (int)KeyList.F11)
            {
                OS.WindowFullscreen = !OS.WindowFullscreen;
                SaveData.SetValue("fullscreen", OS.WindowFullscreen);
                SaveData.SaveToDisk();
            }
        }

        public static void CameraShake(Camera2D camera2D, float power)
        {
            if (DisableCameraShake)
                return;

            cameraShaker.SetTarget(camera2D);
            cameraShaker.Shake(power);
        }

        public static void UIClick() => uiClick.Play();
        public void PlayUIClick() => uiClick.Play();
        public static void Boing() => boing.Play();
        public static void SwitchClick() => switchClick.Play();
        public static void Pop() => pop.Play();
        public static void PlayMusicTrack(AudioStreamOGGVorbis track)
        {
            if (music.Stream != track)
            {
                music.Stream = track;
                music.Play();
            }
        }
        public static void FadeMusic(float duration)
        {
            tween.InterpolateProperty(music, "volume_db", 0, -80, duration);
            tween.InterpolateCallback(music, duration + float.Epsilon, "set", "volume_db", 0);
            tween.Start();
        }
        public static void ResetMusicVol() => music.VolumeDb = 0;

        private static LevelData LoadLevelData(string path)
        {
            File file = new File();

            PackedScene scene = GD.Load<PackedScene>(path);
            Color baseCol = new Color(0, 0, 0, 0);
            int lvlNum = -1;

            if (file.Open(path, File.ModeFlags.Read) == Error.Ok)
            {
                while (!file.EofReached())
                {
                    string line = file.GetLine();

                    if (lvlNum < 0 && line.Find("levelNum = ") > -1)
                        lvlNum = int.Parse(line.Split("= ")[1]);
                    else if (baseCol.a == 0 && line.Find("InitialColour =") > -1)
                        baseCol = ColourParse(line.Split("= ")[1]);

                    if (lvlNum >= 0 && baseCol.a > 0)
                    {
                        file.Close();
                        return new LevelData(scene, baseCol, lvlNum);
                    }
                }
            }

            file.Close();
            return new LevelData();
        }

        private static Color ColourParse(string str)
        {
            var s = str.LStrip("Color( ");
            s = s.RStrip(" )");
            var nums = s.Split(", ");
            return new Color(float.Parse(nums[0]), float.Parse(nums[1]), float.Parse(nums[2]), float.Parse(nums[3]));
        }
    }
}