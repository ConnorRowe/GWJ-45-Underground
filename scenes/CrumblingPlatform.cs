using Godot;

namespace Underground
{
    [Tool]
    public class CrumblingPlatform : StaticBody2D
    {
        private AnimationPlayer animationPlayer;

        public override void _Ready()
        {
            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

            foreach (var child in GetChildren())
            {
                if (child is CollisionShape2D collisionShape2D && collisionShape2D.Shape is RectangleShape2D r)
                {
                    collisionShape2D.OneWayCollision = true;
                    GetNode<NinePatchRect>("Display/NinePatchRect").RectSize = new Vector2(r.Extents.x * 2, 2);
                    var particles = GetNode<Particles2D>("Display/FloorCrumble");
                    particles.Position = new Vector2(r.Extents.x, 0);
                    (particles.ProcessMaterial as ParticlesMaterial).EmissionBoxExtents = new Vector3(r.Extents.x, 0, 0);
                    break;
                }
            }
        }

        public void Crumble()
        {
            if (!animationPlayer.IsPlaying())
                animationPlayer.Play("Crumble");
        }
    }
}