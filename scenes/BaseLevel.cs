using Godot;

namespace Underground
{
    public class BaseLevel : Node2D
    {
        private Tween tween;
        private RichTextLabel speechBox;
        private int speechMaxChars = 0;
        private const float CharTime = .06f;
        private float currentCharTime = 0f;

        [Export]
        public Color InitialColour { get; set; } = new Color(1f,1f,1f);

        public override void _Ready()
        {
            tween = new Tween();
            AddChild(tween);
            speechBox = GetNode<RichTextLabel>("SpeechBox");

            foreach (Node child in GetNode("SpeechTriggers").GetChildren())
            {
                if (child is SpeechArea2D speechArea2D)
                {
                    speechArea2D.Connect("body_entered", this, nameof(SpeechAreaBodyEntered), new Godot.Collections.Array() { speechArea2D.Speech });
                }
            }

            SetColour(InitialColour);
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
                    GlobalNodes.UIClick();
                }
            }
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

        public void SetColour(Color colour)
        {
            GetNode<Character>("Character").Modulate = colour;
            GetNode<TileMap>("TileMap").Modulate = colour;
            speechBox.Modulate = colour;
        }
    }
}