using Godot;

namespace Underground
{
    public class MainMenu : Control
    {
        private static PackedScene buttonScn = GD.Load<PackedScene>("res://scenes/menus/ButtonA.tscn");
        private bool waitingForInput = false;
        private Button rebindButton = null;
        private string rebindAction = "";
        private InputEvent rebindEvent = null;
        private System.Collections.Generic.Dictionary<string, Godot.Collections.Array> actionInputs = new System.Collections.Generic.Dictionary<string, Godot.Collections.Array>();

        public override void _Ready()
        {
            GlobalNodes.PlayMusicTrack(GD.Load<AudioStreamOGGVorbis>("res://audio/music/menumenu_track.ogg"));

            // Generate keybind stuff
            var actionNames = GetNode<VBoxContainer>("Section_Keybinds/VBoxContainer/ScrollContainer/HBoxContainer/ActionNames");
            var primaryInputs = GetNode<VBoxContainer>("Section_Keybinds/VBoxContainer/ScrollContainer/HBoxContainer/PrimaryInputs");
            var altInputs = GetNode<VBoxContainer>("Section_Keybinds/VBoxContainer/ScrollContainer/HBoxContainer/AltInputs");

            foreach (string action in InputMap.GetActions())
            {
                if (action.Substr(0, 2) != "ui")
                {
                    GD.Print($"{action}¬");

                    Godot.Collections.Array actionEvents = InputMap.GetActionList(action);
                    actionInputs.Add(action, actionEvents);

                    Label actionName = new Label() { Text = action.Capitalize(), RectMinSize = new Vector2(0, 25), Align = Label.AlignEnum.Center };
                    actionNames.AddChild(actionName);
                    actionName.Modulate = new Color("df529e");

                    primaryInputs.AddChild(MakeKeybindButton(action, (InputEvent)actionEvents[0]));
                    altInputs.AddChild(MakeKeybindButton(action, actionEvents.Count > 1 ? (InputEvent)actionEvents[1] : null));
                }
            }

            GetNode("Section_Keybinds/VBoxContainer/ReturnToDefaults").Connect("pressed", this, nameof(ReturnToDefaults));

            GetNode("Section_Title/VBoxContainer/Play").Connect("pressed", this, nameof(Play));
            GetNode("Section_Title/VBoxContainer/Play").Connect("mouse_entered", GlobalNodes.INSTANCE, nameof(GlobalNodes.PlayUIClick));

            GetNode("Section_Title/VBoxContainer/Settings").Connect("pressed", this, nameof(ShowSettings));
            GetNode("Section_Title/VBoxContainer/Settings").Connect("mouse_entered", GlobalNodes.INSTANCE, nameof(GlobalNodes.PlayUIClick));

            GetNode("Section_Title/VBoxContainer/Controls").Connect("pressed", this, nameof(ShowControls));
            GetNode("Section_Title/VBoxContainer/Controls").Connect("mouse_entered", GlobalNodes.INSTANCE, nameof(GlobalNodes.PlayUIClick));

            GetNode("Section_Settings/VBoxContainer/HBoxContainer/BackButton").Connect("pressed", this, nameof(ShowTitle));
            GetNode("Section_Settings/VBoxContainer/HBoxContainer/BackButton").Connect("mouse_entered", GlobalNodes.INSTANCE, nameof(GlobalNodes.PlayUIClick));
            GetNode("Section_Keybinds/VBoxContainer/HBoxContainer/BackButton").Connect("pressed", this, nameof(CloseControls));
            GetNode("Section_Keybinds/VBoxContainer/HBoxContainer/BackButton").Connect("mouse_entered", GlobalNodes.INSTANCE, nameof(GlobalNodes.PlayUIClick));
        }

        public override void _Input(InputEvent evt)
        {
            if (waitingForInput && (evt is InputEventKey || evt is InputEventMouseButton || evt is InputEventJoypadButton || evt is InputEventJoypadMotion))
            {
                if (!(evt is InputEventKey ek && ek.Pressed && ek.Scancode == (int)KeyList.Escape))
                {
                    if (rebindEvent != null)
                        InputMap.ActionEraseEvent(rebindAction, rebindEvent);

                    if (evt != null)
                        InputMap.ActionAddEvent(rebindAction, evt);

                    rebindButton.Text = GetInputEventString(evt);
                }

                rebindButton.Modulate = new Color("df529e");
                waitingForInput = false;
                rebindAction = "";
                rebindButton = null;
                rebindEvent = null;
                GetTree().SetInputAsHandled();
            }
        }

        private Button MakeKeybindButton(string action, InputEvent evt)
        {
            GD.Print($"\t{evt}");
            var btn = buttonScn.Instance<Button>();
            btn.Modulate = new Color("df529e");
            btn.Connect("pressed", this, nameof(KeybindButtonPressed), new Godot.Collections.Array(btn, action, evt));
            btn.Connect("mouse_entered", GlobalNodes.INSTANCE, nameof(GlobalNodes.PlayUIClick));
            btn.Text = GetInputEventString(evt);
            btn.RectMinSize = new Vector2(0, 25);

            return btn;
        }

        private void KeybindButtonPressed(Button button, string action, InputEvent inputEvent)
        {
            GD.Print($"KeybindButtonPressed({button}, {action}, {inputEvent})");
            rebindButton = button;
            rebindAction = action;
            rebindEvent = inputEvent;
            waitingForInput = true;
            button.Modulate = new Color("e64e4b");
        }

        private string GetInputEventString(InputEvent evt)
        {
            if (evt != null)
            {
                if (evt is InputEventKey ek)
                    return OS.GetScancodeString(ek.Scancode);
                else if (evt is InputEventMouseButton emb)
                {
                    switch (emb.ButtonIndex)
                    {
                        case (int)ButtonList.Left: return "Left click";
                        case (int)ButtonList.Right: return "Right click";
                        case (int)ButtonList.Middle: return "Middle click";
                        case (int)ButtonList.WheelUp: return "Scroll up";
                        case (int)ButtonList.WheelDown: return "Scroll down";
                    }
                }
                else if (evt is InputEventJoypadButton ejb)
                    return Input.GetJoyButtonString(ejb.ButtonIndex);
                else if (evt is InputEventJoypadMotion ejm)
                    return Input.GetJoyAxisString(ejm.Axis);
                else
                    return evt.ToString();
            }

            return "...";
        }

        private void ReturnToDefaults()
        {
            foreach (string action in actionInputs.Keys)
            {
                InputMap.ActionEraseEvents(action);
            }

            InputMap.LoadFromGlobals();

            GetTree().ReloadCurrentScene();
        }

        private void Play()
        {
            GetTree().ChangeScene("res://scenes/levels/StartLevel.tscn");
            QueueFree();
        }

        private void ShowTitle()
        {
            GetNode<MarginContainer>("Section_Title").Visible = true;
            GetNode<MarginContainer>("Section_Settings").Visible = false;
            GetNode<MarginContainer>("Section_Keybinds").Visible = false;
            GetNode<MarginContainer>("Section_LevelSelect").Visible = false;
        }

        private void ShowControls()
        {
            GetNode<MarginContainer>("Section_Title").Visible = false;
            GetNode<MarginContainer>("Section_Settings").Visible = false;
            GetNode<MarginContainer>("Section_Keybinds").Visible = true;
            GetNode<MarginContainer>("Section_LevelSelect").Visible = false;
        }

        private void ShowSettings()
        {
            GetNode<MarginContainer>("Section_Title").Visible = false;
            GetNode<MarginContainer>("Section_Settings").Visible = true;
            GetNode<MarginContainer>("Section_Keybinds").Visible = false;
            GetNode<MarginContainer>("Section_LevelSelect").Visible = false;
        }

        private void ShowLevelSelect()
        {
            GetNode<MarginContainer>("Section_Title").Visible = false;
            GetNode<MarginContainer>("Section_Settings").Visible = false;
            GetNode<MarginContainer>("Section_Keybinds").Visible = false;
            GetNode<MarginContainer>("Section_LevelSelect").Visible = true;
        }

        private void CloseControls()
        {
            ShowTitle();
            SaveData.SaveKeybinds();
            SaveData.SaveToDisk();
        }
    }
}