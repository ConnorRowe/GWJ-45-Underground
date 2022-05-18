using Godot;

namespace Underground
{
    public class Door : Node2D
    {
        [Export]
        private Godot.Collections.Array<NodePath> triggerButtonsPaths = new Godot.Collections.Array<NodePath>();
        private System.Collections.Generic.List<PressureButton> triggerButtons = new System.Collections.Generic.List<PressureButton>();
        private bool isOpen = false;
        private AnimationPlayer animationPlayer;

        public override void _Ready()
        {
            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

            foreach (var path in triggerButtonsPaths)
            {
                var btn = GetNode<PressureButton>(path);
                triggerButtons.Add(btn);
                btn.Connect(nameof(PressureButton.IsPressedChanged), this, nameof(TriggerButtonChanged));
            }
        }

        private void TriggerButtonChanged()
        {
            bool allPressed = triggerButtons.TrueForAll(btn => btn.IsPressed);

            if (!isOpen && allPressed)
            {
                Open();
            }
            else if (isOpen && !allPressed)
            {
                Close();
            }
        }

        private void Open()
        {
            isOpen = true;
            animationPlayer.Play("Open");
        }

        private void Close()
        {
            isOpen = false;
            animationPlayer.Play("Close");
        }
    }
}