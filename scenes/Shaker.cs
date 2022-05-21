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
        private Control targetControl;
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
            if (targetNode2DPath != null)
                SetTarget(GetNode(targetNode2DPath));
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            if (target != null || targetCamera != null || targetControl != null)
            {
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

                    float r = maxRotation * amount * noise.GetNoise2d(noise.Seed, noiseY);
                    Vector2 offset = new Vector2();
                    offset.x = maxOffset.x * amount * noise.GetNoise2d(noise.Seed * 2, noiseY);
                    offset.y = maxOffset.y * amount * noise.GetNoise2d(noise.Seed * 3, noiseY);

                    if (target != null)
                        target.Rotation = r;

                    if (targetCamera != null)
                        targetCamera.Offset = offset;
                    else if (targetControl != null)
                        targetControl.RectRotation = r;
                    else if (target != null)
                        target.Position = offset;
                }
            }
        }

        public void Shake(float power)
        {
            trauma = Mathf.Min(trauma + power, 1f);
        }

        public void SetTarget(Node target)
        {
            if (target is Camera2D camera2D)
            {
                this.target = targetCamera = camera2D;
            }
            else if (target is Node2D node2D)
            {
                this.target = node2D;
            }
            else if (target is Control control)
            {
                this.targetControl = control;
            }
        }
    }
}