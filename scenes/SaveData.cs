using Godot;

namespace Underground
{
    public static class SaveData
    {
        private const string filePath = "user://save.ini";
        private static ConfigFile Save { get; } = new ConfigFile();

        public static float MasterVol => (float)Save.GetValue("Settings", "master_vol");
        public static float MusicVol => (float)Save.GetValue("Settings", "music_vol");
        public static float SFXVol => (float)Save.GetValue("Settings", "sfx_vol");
        public static bool Fullscreen => (bool)Save.GetValue("Settings", "fullscreen");
        public static bool DisableCamShake => (bool)Save.GetValue("Settings", "disable_cam_shake");
        public static int MaxLevel => (int)Save.GetValue("Settings", "max_level");
        public static Godot.Collections.Dictionary KeyBinds => (Godot.Collections.Dictionary)Save.GetValue("Settings", "keybinds");

        static SaveData()
        {
            var e = Save.Load(filePath);

            if (e == Error.Ok && Save.HasSection("Settings"))
            {
                GD.Print("Loaded save data.");
            }
            else
            {
                GD.Print("No save file found. Re-creating.", e);
                Save.SetValue("Settings", "master_vol", .8f);
                Save.SetValue("Settings", "music_vol", .5f);
                Save.SetValue("Settings", "sfx_vol", .5f);
                Save.SetValue("Settings", "fullscreen", false);
                Save.SetValue("Settings", "disable_cam_shake", false);
                Save.SetValue("Settings", "max_level", 0);
                SaveKeybinds();
                Save.Save(filePath);
                Save.Load(filePath);
            }
        }

        public static void SetValue(string key, object value) => Save.SetValue("Settings", key, value);
        public static void SaveToDisk() => Save.Save(filePath);
        public static void SaveKeybinds()
        {
            Godot.Collections.Dictionary keybinds = new Godot.Collections.Dictionary();
            foreach (string action in InputMap.GetActions())
            {
                if (action.Substr(0, 2) != "ui")
                    keybinds.Add(action, InputMap.GetActionList(action));
            }
            Save.SetValue("Settings", "keybinds", keybinds);
        }

        public static void ApplyLoadedSettings()
        {
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), GD.Linear2Db(MasterVol));
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), GD.Linear2Db(MusicVol));
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), GD.Linear2Db(SFXVol));
            OS.WindowFullscreen = Fullscreen;
            GlobalNodes.DisableCameraShake = DisableCamShake;

            var keybinds = KeyBinds;

            foreach (string action in keybinds.Keys)
            {
                InputMap.ActionEraseEvents(action);
                foreach (InputEvent evt in (Godot.Collections.Array)keybinds[action])
                {
                    InputMap.ActionAddEvent(action, evt);
                }
            }
        }
    }
}