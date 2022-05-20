using Godot;

namespace Underground
{
    public class PausePopup : PopupPanel
    {
        public override void _Input(InputEvent evt)
        {
            if (evt.IsActionReleased("pause"))
            {
                if (GetTree().Paused)
                {
                    // Resume
                    Hide();
                    GetTree().Paused = false;
                }
                else
                {
                    // Pause
                    PopupCentered(new Vector2(306, 250));
                    GetTree().Paused = true;
                }
            }
        }
    }
}