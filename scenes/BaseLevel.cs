using Godot;

namespace Underground
{
    public class BaseLevel : Node2D
    {
        [Export]
        public Color InitialColour { get; set; } = new Color(1f, 1f, 1f);
        [Export]
        public Color HighlightColour { get; set; } = new Color(1f, 1f, 1f);
        [Export(PropertyHint.File, "*.tscn")]
        private string nextLevelScene = "";
        [Export]
        private AudioStreamOGGVorbis track;
        [Export]
        protected int levelNum = -1;

        public override void _Ready()
        {
            GlobalNodes.PlayMusicTrack(track);

            GetNode<PausePopup>("Overlay/PausePopup").SetColours(InitialColour, HighlightColour);
        }

        private void NextLevel()
        {
            if (levelNum > SaveData.MaxLevel)
            {
                SaveData.SetValue("max_level", levelNum);
                SaveData.SaveToDisk();
            }

            GetTree().ChangeScene(nextLevelScene);
            QueueFree();
        }
    }
}