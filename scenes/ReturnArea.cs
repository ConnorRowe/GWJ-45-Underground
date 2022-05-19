using Godot;

namespace Underground
{
    [Tool]
    public class ReturnArea : Area2D
    {
        private Tween tween;
        private Position2D returnPosition2D;

        public override void _Ready()
        {
            foreach (var child in GetChildren())
            {
                if (child is CollisionShape2D collisionShape2D && collisionShape2D.Shape is RectangleShape2D r)
                {
                    var particles = GetNode<Particles2D>("Particles2D");
                    (particles.ProcessMaterial as ParticlesMaterial).EmissionBoxExtents = new Vector3(r.Extents.x, r.Extents.y, 0);
                    particles.Amount = (int)Mathf.Sqrt(r.Extents.x * r.Extents.y);
                }
                else if (child is Position2D position2D)
                {
                    returnPosition2D = position2D;
                }
            }

            Connect("body_entered", this, nameof(BodyEntered));

            if (!Engine.EditorHint)
            {
                tween = new Tween();
                AddChild(tween);

                if (returnPosition2D == null)
                    GD.PrintErr($"{Name} has no target Position2D. Add one to it!");
            }
        }

        private void BodyEntered(Node node)
        {
            if (node is Character c && !c.InputLocked)
            {
                GlobalNodes.CameraShake(c.Camera2D, 2);
                GlobalNodes.Boing();

                c.InputLocked = c.AnimLocked = true;

                tween.InterpolateProperty(c, "global_position", c.GlobalPosition, returnPosition2D.GlobalPosition, 2f, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                tween.InterpolateProperty(c, "rotation_degrees", 0f, -720, 2f, Tween.TransitionType.Sine, Tween.EaseType.InOut);
                tween.InterpolateCallback(c, 2f, "set", nameof(c.InputLocked), false);
                tween.InterpolateCallback(c, 2f, "set", nameof(c.AnimLocked), false);
                tween.InterpolateCallback(c, 2f, "set_physics_process", true);
                tween.InterpolateCallback(c, 2f, "set_process", true);
                tween.Start();
            }
        }
    }
}