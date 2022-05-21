using Godot;

namespace Underground
{
    public class MyButton : Button
    {
        private Shaker shaker;

        public override void _Ready()
        {
            shaker = GetNode<Shaker>("Shaker");
            RectPivotOffset = RectSize / 2;

            Connect("mouse_entered", this, nameof(MouseEntered));
        }

        private void MouseEntered()
        {
            if (!Disabled)
            {
                shaker.Shake(5f);
                GlobalNodes.UIClick();
            }
        }
    }
}