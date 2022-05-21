using Godot;

namespace Underground
{
    public class PressureButton : StaticBody2D
    {
        [Signal]
        public delegate void IsPressedChanged();

        private CollisionPolygon2D upCollider;
        private CollisionPolygon2D pressedCollider;
        private Node2D textHolder;
        private Label label;
        private Shaker shaker;
        private PressureButton resetButton;

        public bool IsPressed { get; private set; } = false;

        [Export]
        private NodePath resetButtonPath = "";

        public override void _Ready()
        {
            upCollider = GetNode<CollisionPolygon2D>("UpCollider");
            pressedCollider = GetNode<CollisionPolygon2D>("PressedCollider");
            textHolder = GetNode<Node2D>("TextHolder");
            label = GetNode<Label>("TextHolder/Label");
            shaker = GetNode<Shaker>("Shaker");

            if (!resetButtonPath.IsEmpty())
                resetButton = GetNode<PressureButton>(resetButtonPath);
        }

        public void Press()
        {
            if (IsPressed)
                return;

            IsPressed = true;
            upCollider.Disabled = true;
            pressedCollider.Disabled = false;
            label.Text = "|)";
            GlobalNodes.SwitchClick();
            shaker.Shake(.5f);

            resetButton?.Reset();

            EmitSignal(nameof(IsPressedChanged));
        }

        private void Reset()
        {
            if (!IsPressed)
                return;

            IsPressed = false;
            upCollider.Disabled = false;
            pressedCollider.Disabled = true;
            label.Text = "|D";
            shaker.Shake(1f);

            EmitSignal(nameof(IsPressedChanged));
        }
    }
}