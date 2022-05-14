using Godot;

namespace Underground
{
    public class Character : KinematicBody2D
    {
        private const float JumpForce = 200f;
        private const float GravityAccel = 98f * 8;
        private const float TerminalVel = 245f;
        private const float MaxSpeed = 1000f;
        private const float Acceleration = 600f;
        private const float MovementDamping = 10f;

        private AnimationPlayer animationPlayer;
        private Node2D bodyParts;
        private float inputDir = 0f;
        private float targetAngle = 0f;
        private Vector2 velocity = Vector2.Zero;
        private Vector2 externalVelocity = Vector2.Zero;
        private float gravity = 0;

        public override void _Ready()
        {
            base._Ready();

            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            bodyParts = GetNode<Node2D>("BodyParts");
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            bodyParts.Rotation = Mathf.LerpAngle(bodyParts.Rotation, targetAngle, delta * 10f);
        }

        public override void _PhysicsProcess(float delta)
        {
            int inputX = 0;
            if (Input.IsActionPressed("move_left"))
                inputX -= 1;
            if (Input.IsActionPressed("move_right"))
                inputX += 1;

            bool isOnFloor = IsOnFloor();

            if (isOnFloor)
            {
                inputDir = inputX;

                Vector2 floorNormal = GetFloorNormal();
                targetAngle = Mathf.Atan2(floorNormal.y, floorNormal.x) + (Mathf.Pi / 2f);
            }
            else if (Input.IsActionPressed("move_left") || Input.IsActionPressed("move_right"))
            {
                inputDir = Mathf.Lerp(inputDir, inputX, delta * 5f);
                bodyParts.Rotation = 0f;
            }

            velocity -= (velocity * MovementDamping * delta);
            velocity += new Vector2(inputDir, 0) * Acceleration * delta;
            externalVelocity -= (externalVelocity * MovementDamping * delta);

            if (velocity.Length() > MaxSpeed)
                velocity = velocity.Normalized() * MaxSpeed;

            if (gravity < 0 && IsOnCeiling())
                gravity = 0;

            if (!isOnFloor)
            {
                if (gravity < TerminalVel)
                    gravity += GravityAccel * delta;
                else if (gravity > TerminalVel)
                    gravity = TerminalVel;
            }
            else if (gravity > 0)
                gravity = 0;

            MoveAndSlideWithSnap(velocity + externalVelocity + new Vector2(0, gravity), Vector2.Down, Vector2.Up, infiniteInertia: false);

            if (velocity.x > 0 && bodyParts.Scale.x < 0)
                bodyParts.Scale = new Vector2(1, 1);
            else if (velocity.x < 0 && bodyParts.Scale.x > 0)
                bodyParts.Scale = new Vector2(-1, 1);

            if (velocity.LengthSquared() > .1f)
            {
                if (animationPlayer.CurrentAnimation != "Walk")
                    animationPlayer.Play("Walk");
            }
            else
            {
                if (animationPlayer.CurrentAnimation != "Idle")
                    animationPlayer.Play("Idle");
            }
        }
    }
}