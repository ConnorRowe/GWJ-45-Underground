using Godot;

namespace Underground
{
    public class JumpEffect : AnimatedSprite
    {
        public override void _Ready()
        {
            Playing = true;
            Connect("animation_finished", this, "queue_free");
        }
    }
}