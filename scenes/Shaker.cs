using Godot;

namespace Underground
{
    public class Shaker : Node
    {
        [Export]
        private NodePath targetNode2DPath;
        [Export]
        private float maxRotation = .1f;
        [Export]
        private Vector2 maxOffset = new Vector2(20f, 20f);
        private Node2D target;
        private Camera2D targetCamera;
        private static OpenSimplexNoise noise = new OpenSimplexNoise()
        {
            Seed = 42069,
            Period = 4,
            Octaves = 2
        };
        [Export]
        private float decay = .8f;
        private float trauma = 0f;
        private int noiseY = 0;

        public override void _Ready()
        {
            target = GetNode<Node2D>(targetNode2DPath);

            if (target is Camera2D c)
                targetCamera = c;
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            if (trauma > 0)
            {
                trauma = Mathf.Max(trauma - (decay * delta), 0);

                float amount = Mathf.Pow(trauma, 2);

                if (noiseY == int.MaxValue)
                {
                    noiseY = 0;
                }
                else
                    noiseY++;

                target.Rotation = maxRotation * amount * noise.GetNoise2d(noise.Seed, noiseY);
                Vector2 offset = new Vector2();
                offset.x = maxOffset.x * amount * noise.GetNoise2d(noise.Seed * 2, noiseY);
                offset.y = maxOffset.y * amount * noise.GetNoise2d(noise.Seed * 3, noiseY);

                if (targetCamera != null)
                    targetCamera.Offset = offset;
                else
                    target.Position = offset;
            }
        }

        public void Shake(float power)
        {
            trauma = Mathf.Min(trauma + power, 1f);
        }
    }
}