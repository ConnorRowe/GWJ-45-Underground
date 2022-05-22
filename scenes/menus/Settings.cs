using Godot;

namespace Underground
{
    public class Settings : MarginContainer
    {
        private HSlider masterSlider;
        private HSlider musicSlider;
        private HSlider sfxSlider;
        private CheckButton fullscreen;
        private CheckButton camShake;
        private int masterBus;
        private int musicBus;
        private int sfxBus;

        public override void _Ready()
        {
            masterBus = AudioServer.GetBusIndex("Master");
            musicBus = AudioServer.GetBusIndex("Music");
            sfxBus = AudioServer.GetBusIndex("SFX");
            masterSlider = GetNode<HSlider>("VBoxContainer/MasterVol");
            musicSlider = GetNode<HSlider>("VBoxContainer/MusicVol");
            sfxSlider = GetNode<HSlider>("VBoxContainer/SFXVol");
            fullscreen = GetNode<CheckButton>("VBoxContainer/ToggleFullscreen");
            camShake = GetNode<CheckButton>("VBoxContainer/DisableCameraShake");

            GetSettings();

            masterSlider.Connect("value_changed", this, nameof(VolValueChanged), new Godot.Collections.Array() { masterBus });
            musicSlider.Connect("value_changed", this, nameof(VolValueChanged), new Godot.Collections.Array() { musicBus });
            sfxSlider.Connect("value_changed", this, nameof(VolValueChanged), new Godot.Collections.Array() { sfxBus });
            fullscreen.Connect("toggled", this, nameof(SetFullscreen));
            camShake.Connect("toggled", this, nameof(SetCamShake));

            Connect("visibility_changed", this, nameof(VisibilityChanged));

            foreach (Node node in GetNode("VBoxContainer").GetChildren())
            {
                if (node is Control && !(node is Label) && !(node is MyButton))
                    node.Connect("mouse_entered", GlobalNodes.INSTANCE, nameof(GlobalNodes.UIClick));
            }
        }

        private void VisibilityChanged()
        {
            if (IsVisibleInTree())
            {
                GetSettings();
            }
            else
            {
                SaveData.SetValue("master_vol", masterSlider.Value);
                SaveData.SetValue("music_vol", musicSlider.Value);
                SaveData.SetValue("sfx_vol", sfxSlider.Value);
                SaveData.SetValue("fullscreen", fullscreen.Pressed);
                SaveData.SetValue("disable_cam_shake", camShake.Pressed);
                SaveData.SaveToDisk();
            }
        }

        private void GetSettings()
        {
            masterSlider.Value = GD.Db2Linear(AudioServer.GetBusVolumeDb(masterBus));
            musicSlider.Value = GD.Db2Linear(AudioServer.GetBusVolumeDb(musicBus));
            sfxSlider.Value = GD.Db2Linear(AudioServer.GetBusVolumeDb(sfxBus));
            fullscreen.Pressed = OS.WindowFullscreen;
            camShake.Pressed = GlobalNodes.DisableCameraShake;
        }

        private void VolValueChanged(float vol, int busIdx)
        {
            AudioServer.SetBusVolumeDb(busIdx, GD.Linear2Db(vol));
        }

        private void SetFullscreen(bool fs) => OS.WindowFullscreen = fs;
        private void SetCamShake(bool cs) => GlobalNodes.DisableCameraShake = cs;

        public void SetColours(Color normal, Color highlight)
        {
            GetNode("VBoxContainer/Label").Set("custom_colors/font_color", highlight);
            GetNode("VBoxContainer/Label2").Set("custom_colors/font_color", highlight);
            GetNode("VBoxContainer/Label3").Set("custom_colors/font_color", highlight);
            fullscreen.Modulate = highlight;
            camShake.Modulate = highlight;
        }
    }
}