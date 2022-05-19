using Godot;

namespace Underground
{
    public class Level2 : BaseLevel
    {
        private float timer;
        private Sprite grappleSprite;

        public override void _Ready()
        {
            base._Ready();

            grappleSprite = GetNode<Sprite>("OtherStuff/GrapplingHookItem/Sprite");
            GetNode("OtherStuff/GrapplingHookItem").Connect("body_entered", this, nameof(GrappleItemBodyEntered));
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            timer += delta * 2;
            if (timer > Mathf.Tau)
                timer -= Mathf.Tau;

            if (!character.HasHook)
                grappleSprite.Offset = new Vector2(.5f, .5f + (Mathf.Cos(timer) * 6));
        }

        private void GrappleItemBodyEntered(Node node)
        {
            if (node is Character c && !c.HasHook)
            {
                GlobalNodes.CameraShake(c.Camera2D, 1f);
                c.HasHook = true;
                GetNode<Particles2D>("OtherStuff/ExclaimExplosion").Emitting = true;
                GetNode("OtherStuff/GrapplingHookItem").QueueFree();
                AddSpeech("[wave amp=30 freq=3]Cool!! Some kind of grappling hook!");
            }
        }
    }
}