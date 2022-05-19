using Godot;

namespace Underground
{
    public class MainMenu : Control
    {
        private static PackedScene buttonScn = GD.Load<PackedScene>("res://scenes/menus/ButtonA.tscn");

        public override void _Ready()
        {
            base._Ready();

            var keybindings = GetNode<Control>("Keybindings");

            foreach (string action in InputMap.GetActions())
            {
                if (action.Substr(0, 2) != "ui")
                {
                    GD.Print($"{action}¬");

                    Godot.Collections.Array actionEvents = InputMap.GetActionList(action);

                    HBoxContainer hBox = new HBoxContainer() { Name = $"HBox_{action}" };
                    Label label = new Label() { Text = action.Capitalize() };
                    hBox.AddChild(label);

                    hBox.AddChild(MakeKeybindButton((InputEvent)actionEvents[0]));
                    Button altBtn;
                    if (actionEvents.Count > 1)
                        altBtn = MakeKeybindButton((InputEvent)actionEvents[1]);
                    else
                    {
                        altBtn = buttonScn.Instance<Button>();
                        altBtn.Text = "...";
                    }
                    hBox.AddChild(altBtn);
                    keybindings.AddChild(hBox);
                }
            }
        }

        private Button MakeKeybindButton(InputEvent evt)
        {
            GD.Print($"\t{evt}");
            var btn = buttonScn.Instance<Button>();

            if (evt is InputEventKey ek)
                btn.Text = OS.GetScancodeString(ek.Scancode);
            else if (evt is InputEventMouseButton emb)
            {
                switch (emb.ButtonIndex)
                {
                    case (int)ButtonList.Left: btn.Text = "Left click"; break;
                    case (int)ButtonList.Right: btn.Text = "Right click"; break;
                    case (int)ButtonList.Middle: btn.Text = "Middle click"; break;
                    case (int)ButtonList.WheelUp: btn.Text = "Scroll up"; break;
                    case (int)ButtonList.WheelDown: btn.Text = "Scroll down"; break;
                }
            }
            else if (evt is InputEventJoypadButton ejb)
                btn.Text = Input.GetJoyButtonString(ejb.ButtonIndex);
            else if (evt is InputEventJoypadMotion ejm)
                btn.Text = Input.GetJoyAxisString(ejm.Axis);
            else
                btn.Text = evt.ToString();

            return btn;
        }
    }
}