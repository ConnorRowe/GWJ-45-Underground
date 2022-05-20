using Godot;

namespace Underground
{
    public class BaseLevel : Node2D
    {
        private RichTextLabel speechBox;
        private int speechMaxChars = 0;
        private const float CharTime = .035f;
        private float currentCharTime = 0f;
        private ShaderMaterial tileMaterial;
        private Camera2D camera2D;
        protected Character character;
        private PopupPanel pausePopup;

        [Export]
        public Color InitialColour { get; set; } = new Color(1f, 1f, 1f);
        [Export]
        public Color HighlightColour { get; set; } = new Color(1f, 1f, 1f);
        [Export(PropertyHint.File, "*.tscn")]
        private string nextLevelScene = "";
        [Export]
        private AudioStreamOGGVorbis track;

        public override void _Ready()
        {
            character = GetNode<Character>("Character");
            speechBox = GetNode<RichTextLabel>("Overlay/SpeechBox");
            camera2D = GetNode<Camera2D>("Character/Camera2D");
            tileMaterial = GetNode<TileMap>("TileMap").TileSet.TileGetMaterial(0);
            pausePopup = GetNode<PopupPanel>("Overlay/PausePopup");

            foreach (Node child in GetNode("SpeechTriggers").GetChildren())
            {
                if (child is SpeechArea2D speechArea2D)
                {
                    speechArea2D.Connect("body_entered", this, nameof(SpeechAreaBodyEntered), new Godot.Collections.Array() { speechArea2D.Speech });
                }
            }

            SetColour(InitialColour, HighlightColour);

            GetNode("OtherStuff/Highlighted/ExitArea").Connect("body_entered", this, nameof(ExitAreaBodyEntered));

            GlobalNodes.PlayMusicTrack(track);
        }

        public override void _Input(InputEvent evt)
        {
            if (evt.IsActionReleased("reload_level"))
                GetTree().ReloadCurrentScene();
        }

        public override void _Process(float delta)
        {
            if (speechBox.VisibleCharacters < speechMaxChars)
            {
                currentCharTime += delta;

                if (currentCharTime > CharTime)
                {
                    currentCharTime -= CharTime;

                    speechBox.VisibleCharacters++;
                    GlobalNodes.Pop();
                }
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            tileMaterial.SetShaderParam("offset", camera2D.GetCameraScreenCenter() + camera2D.Offset);
        }

        public void AddSpeech(string speech)
        {
            speechBox.BbcodeText = speech;
            speechBox.VisibleCharacters = 0;

            CallDeferred(nameof(UpdateSpeechMaxChars));
        }

        public void SpeechAreaBodyEntered(Node body, string speech)
        {
            if (body is Character && speechBox.BbcodeText != speech)
            {
                AddSpeech(speech);
            }
        }

        private void UpdateSpeechMaxChars()
        {
            speechMaxChars = speechBox.GetTotalCharacterCount();
        }

        public void SetColour(Color normal, Color highlight)
        {
            GetNode<Character>("Character").Modulate = highlight;
            GetNode<TileMap>("TileMap").Modulate = normal;
            speechBox.Modulate = normal;
            GetNode<Node2D>("SpeechTriggers").Modulate = highlight;
            GetNode<Node2D>("OtherStuff/Normal").Modulate = normal;
            GetNode<Node2D>("OtherStuff/Highlighted").Modulate = highlight;
        }

        private void ExitAreaBodyEntered(Node body)
        {
            if (body is Character)
            {
                GetNode<AnimationPlayer>("AnimationPlayer").Play("FadeOut");
            }
        }

        private void NextLevel()
        {
            GetTree().ChangeScene(nextLevelScene);
            QueueFree();
        }
    }
}