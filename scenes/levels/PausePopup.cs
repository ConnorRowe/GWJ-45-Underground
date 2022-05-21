using Godot;

namespace Underground
{
    public class PausePopup : PopupPanel
    {
        public override void _Ready()
        {
            GetNode("Panel/VBoxContainer/MarginContainer/HBoxContainer/Exit").Connect("pressed", this, nameof(Quit));
            GetNode("Panel/VBoxContainer/MarginContainer/HBoxContainer/Return").Connect("pressed", this, nameof(TogglePause));
        }

        public override void _Input(InputEvent evt)
        {
            if (evt.IsActionReleased("pause"))
            {
                TogglePause();
            }
        }

        private void TogglePause()
        {
            if (GetTree().Paused)
            {
                // Resume
                Hide();
                GetTree().Paused = false;
                Input.SetMouseMode(Input.MouseMode.Confined);
            }
            else
            {
                // Pause
                PopupCentered(new Vector2(306, 250));
                GetTree().Paused = true;
                Input.SetMouseMode(Input.MouseMode.Visible);
            }
        }

        private void Quit()
        {
            GetTree().Paused = false;
            GetTree().CurrentScene.QueueFree();
            GetTree().ChangeScene("res://scenes/menus/MainMenu.tscn");
        }

        public void SetColours(Color normal, Color highlight)
        {
            GetNode<Panel>("Panel").SelfModulate = normal;
            GetNode("Panel/VBoxContainer/Label").Set("custom_colors/font_color", highlight);
            GetNode<Settings>("Panel/VBoxContainer/Settings").SetColours(normal, highlight);
        }
    }
}