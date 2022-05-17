using Godot;

namespace Underground
{
    public class SpeechArea2D : Area2D
    {
        [Export]
        public string Speech { get; set; } = "";
    }
}