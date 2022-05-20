using Godot;

namespace Underground
{
    public class GlobalNodes : Node
    {
        public static GlobalNodes INSTANCE { get; private set; }
        public static bool DisableCameraShake { get; set; } = false;

        private static Shaker cameraShaker;
        private static AudioStreamPlayer uiClick;
        private static AudioStreamPlayer boing;
        private static AudioStreamPlayer switchClick;
        private static AudioStreamPlayer pop;
        private static AudioStreamPlayer music;

        private static Tween tween;

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

            GD.Print($"{SaveData.MaxLevel}");
            SaveData.ApplyLoadedSettings();
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
    }
}