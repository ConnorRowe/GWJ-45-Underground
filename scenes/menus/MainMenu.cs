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
        private Button pulseButton = null;
        private float timer;

        public override void _Ready()
        {
            Input.SetMouseMode(Input.MouseMode.Visible);

            GlobalNodes.PlayMusicTrack(GD.Load<AudioStreamOGGVorbis>("res://audio/music/menumenu_track.ogg"));

            // Generate keybind stuff
            var actionNames = GetNode<VBoxContainer>("Section_Keybinds/VBoxContainer/ScrollContainer/HBoxContainer/ActionNames");
            var primaryInputs = GetNode<VBoxContainer>("Section_Keybinds/VBoxContainer/ScrollContainer/HBoxContainer/PrimaryInputs");
            var altInputs = GetNode<VBoxContainer>("Section_Keybinds/VBoxContainer/ScrollContainer/HBoxContainer/AltInputs");

            foreach (string action in InputMap.GetActions())
            {
                if (action.Substr(0, 2) != "ui")
                {
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

            GetNode("Section_Title/VBoxContainer/Play").Connect("pressed", this, nameof(ShowLevelSelect));

            GetNode("Section_Title/VBoxContainer/Settings").Connect("pressed", this, nameof(ShowSettings));

            GetNode("Section_Title/VBoxContainer/Controls").Connect("pressed", this, nameof(ShowControls));

            GetNode("Section_LevelSelect/VBoxContainer/HBoxContainer/BackButton").Connect("pressed", this, nameof(ShowTitle));
            GetNode("Section_Settings/VBoxContainer/HBoxContainer/BackButton").Connect("pressed", this, nameof(ShowTitle));
            GetNode("Section_Keybinds/VBoxContainer/HBoxContainer/BackButton").Connect("pressed", this, nameof(CloseControls));

            var lvlGrid = GetNode<GridContainer>("Section_LevelSelect/VBoxContainer/MarginContainer/Panel/MarginContainer/GridContainer");
            // Populate level selection
            var boldFont = GD.Load<DynamicFont>("res://fonts/Bold.tres");
            int maxLevel = SaveData.MaxLevel;
            foreach (var levelData in GlobalNodes.Levels)
            {
                var btn = buttonScn.Instance<Button>();
                btn.RectMinSize = new Vector2(60, 60);
                btn.Text = $"{levelData.Number + 1}";

                if (levelData.Number > maxLevel)
                {
                    btn.Disabled = true;
                    float[] hsv = new float[3];
                    levelData.BaseColour.ToHsv(out hsv[0], out hsv[1], out hsv[2]);
                    btn.Modulate = Color.FromHsv(hsv[0], hsv[1] * .5f, hsv[2]);
                }
                else
                {
                    btn.Modulate = levelData.BaseColour;
                    btn.Connect("pressed", this, nameof(PlayLevel), new Godot.Collections.Array() { levelData.Scene });
                    btn.Set("custom_fonts/font", boldFont);
                }
                lvlGrid.AddChild(btn);

                if (levelData.Number == maxLevel)
                    pulseButton = btn;
            }

            GetNode("Section_Title/VBoxContainer/Node2D/TitleShaker/Title").Connect("mouse_entered", this, nameof(TitleHovered));

            GetNode("Section_Title/Control/Credit").Connect("meta_clicked", this, nameof(CreditMetaClicked));
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

        public override void _Process(float delta)
        {
            timer += delta * 3;
            if (timer > Mathf.Tau)
                timer -= Mathf.Tau;

            if (pulseButton != null)
            {
                float s = .95f + (Mathf.Cos(timer) * .1f);
                pulseButton.RectScale = new Vector2(s, s);
            }
        }

        private Button MakeKeybindButton(string action, InputEvent evt)
        {
            var btn = buttonScn.Instance<Button>();
            btn.Modulate = new Color("df529e");
            btn.Connect("pressed", this, nameof(KeybindButtonPressed), new Godot.Collections.Array(btn, action, evt));
            btn.Text = GetInputEventString(evt);
            btn.RectMinSize = new Vector2(0, 25);

            return btn;
        }

        private void KeybindButtonPressed(Button button, string action, InputEvent inputEvent)
        {
            rebindButton = button;
            rebindAction = action;
            rebindEvent = inputEvent;
            waitingForInput = true;
            button.Modulate = new Color("e64e4b");
        }

        public static string GetInputEventString(InputEvent evt)
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

        public static string ReplaceInputTags(string str)
        {
            int foundin = 0;
            string s = str;
            while ((foundin = s.Find("input={", foundin)) > -1)
            {
                int end = s.Find("}", foundin);
                string action = s.Substr(foundin + 7, end - foundin - 7);
                s = s.Remove(foundin, end - foundin + 1);
                string human = MainMenu.GetInputEventString((InputEvent)InputMap.GetActionList(action)[0]);
                s = s.Insert(foundin, human);
            }

            return s;
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

        private void PlayLevel(PackedScene level)
        {
            GetTree().ChangeSceneTo(level);
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

        private void StartLevel(PackedScene levelScene)
        {
            GetTree().ChangeSceneTo(levelScene);
            QueueFree();
        }

        private void MakeFakeInputAction(string action, bool pressed)
        {
            Input.ParseInputEvent(new InputEventAction()
            {
                Action = action,
                Pressed = pressed
            });
        }

        private void TitleHovered()
        {
            GlobalNodes.Boing();
            GetNode<Shaker>("Section_Title/VBoxContainer/Node2D/TitleShaker/Shaker").Shake(5);
        }

        private void CreditMetaClicked(object meta)
        {
            if (meta is string str)
            {
                OS.ShellOpen(str);
            }
        }
    }
}