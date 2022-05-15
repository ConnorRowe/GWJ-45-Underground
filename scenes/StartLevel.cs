using Godot;

namespace Underground
{
    public class StartLevel : Node2D
    {
        public override void _Ready()
        {
            var character = GetNode<Character>("Character");
            character.Locked = true;
			
        }
    }
}