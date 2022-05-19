using Godot;

namespace Underground
{
    public class RandomClutter : AnimatedSprite
    {
        public override void _Ready()
        {
            var rng = new RandomNumberGenerator()
            {
                Seed = (ulong)GD.Hash(GlobalPosition)
            };

            Frame = rng.RandiRange(0, Frames.GetFrameCount("default") - 1);
        }
    }
}