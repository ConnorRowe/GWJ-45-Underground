using Godot;

namespace Underground
{
    public class EndScreen : Node2D
    {
        private Tween tween;

        public override void _Ready()
        {
            tween = new Tween();
            AddChild(tween);

            GetNode("TitleHolder/Title/Label").Connect("mouse_entered", GetNode("TitleHolder/Title/Shaker"), nameof(Shaker.Shake), new Godot.Collections.Array() { 3f });
            GetNode("StarHolder/Star/Label").Connect("mouse_entered", GetNode("StarHolder/Star/Shaker"), nameof(Shaker.Shake), new Godot.Collections.Array() { 3f });
            GetNode("TitleHolder/Title/Label").Connect("mouse_entered", GlobalNodes.INSTANCE, nameof(GlobalNodes.Boing));
            GetNode("StarHolder/Star/Label").Connect("mouse_entered", GlobalNodes.INSTANCE, nameof(GlobalNodes.Boing));
            GetNode("ReturnButton").Connect("pressed", this, nameof(ReturnPressed));
        }

        private void ReturnPressed()
        {
            GetTree().CreateTimer(2f).Connect("timeout", this, nameof(GotoMainMenu));
            GlobalNodes.FadeMusic(2f);
            GetNode("ReturnButton").Disconnect("pressed", this, nameof(ReturnPressed));
            tween.InterpolateProperty(GetNode("Overlay"), "modulate", Colors.Transparent, Colors.White, 2);
            tween.Start();
        }

        private void GotoMainMenu()
        {
            GetTree().ChangeScene("res://scenes/menus/MainMenu.tscn");
            QueueFree();
        }
    }
}