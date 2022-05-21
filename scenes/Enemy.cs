using Godot;

namespace Underground
{
    public class Enemy : Node2D
    {
        private enum State
        {
            Roaming,
            Alert,
            Charge,
            Cooldown
        }

        private const float WalkSpeed = 80f;
        private const float ChargeSpeed = WalkSpeed * 3f;
        private Position2D pointA;
        private Position2D pointB;
        private AnimationPlayer animationPlayer;
        private Tween tween;
        private bool dirLeft = false;
        private State state = State.Roaming;
        private Node2D bodyParts;

        [Export]
        private bool startLeft = false;

        public override void _Ready()
        {
            base._Ready();
            tween = new Tween();
            AddChild(tween);
            bodyParts = GetNode<Node2D>("BodyParts");
            pointA = GetNode<Position2D>("PointA");
            pointB = GetNode<Position2D>("PointB");
            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

            if (startLeft)
                SwitchDirection();
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            if (state == State.Roaming || state == State.Charge)
            {
                Vector2 rayEnd = Vector2.Zero;
                float speed = state == State.Charge ? ChargeSpeed : WalkSpeed;

                switch (state)
                {
                    case State.Roaming:
                        rayEnd = (dirLeft ? pointA : pointB).GlobalPosition + new Vector2((dirLeft ? pointA : pointB).GizmoExtents, 0);
                        break;
                    case State.Charge:
                        rayEnd = bodyParts.GlobalPosition + new Vector2(dirLeft ? -20 : 20, 0);
                        break;
                }

                var ray = GetWorld2d().DirectSpaceState.IntersectRay(bodyParts.GlobalPosition, rayEnd, collisionLayer: 2);

                if (ray.Count > 0)
                {
                    if (state == State.Roaming)
                        Alert();
                    else
                    {
                        if (ray["collider"] is Character c)
                        {
                            c.AddExternalVelocity(new Vector2(dirLeft ? -1000 : 1000, -600));
                            c.HitSound();
                        }

                        Cooldown();
                    }
                }

                bodyParts.Position += new Vector2(dirLeft ? -speed : speed, 0) * delta;

                if ((dirLeft && bodyParts.GlobalPosition.x <= pointA.GlobalPosition.x) || (!dirLeft && bodyParts.GlobalPosition.x >= pointB.GlobalPosition.x))
                {
                    SwitchDirection();

                    if (state == State.Charge)
                        Cooldown();
                }
            }
        }

        public void SwitchDirection()
        {
            dirLeft = !dirLeft;
            bodyParts.Scale *= new Vector2(-1, 1);
        }

        private void Roam()
        {
            animationPlayer.Play("Walk");
            state = State.Roaming;
        }

        private void Charge()
        {
            state = State.Charge;
            animationPlayer.Play("Walk");
        }

        private void Alert()
        {
            animationPlayer.Play("Alert");
            state = State.Alert;
            GlobalNodes.Boing();
        }

        private void Cooldown()
        {
            state = State.Cooldown;
            GetNode<Label>("BodyParts/ShakePoint/Eyebrows").Visible = false;
            GetTree().CreateTimer(2f, false).Connect("timeout", this, nameof(Roam));
        }
    }
}