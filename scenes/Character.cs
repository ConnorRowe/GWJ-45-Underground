using Godot;

namespace Underground
{
    public class Character : KinematicBody2D
    {
        private const float JumpForce = 250f;
        private const float GravityAccel = 98f * 8;
        private const float TerminalVel = 400f;
        private const float MaxSpeed = 1600f;
        private const float Acceleration = 1200f;
        private const float MovementDamping = 10f;
        private static PackedScene jumpEffectScene = GD.Load<PackedScene>("res://scenes/JumpEffect.tscn");

        private AnimationPlayer animationPlayer;
        private AudioStreamPlayer footstepPlayer;
        private AudioStreamPlayer fallhitPlayer;
        private Node2D bodyParts;
        private Node2D torsoBone;
        private float inputDir = 0f;
        private float targetAngle = 0f;
        private Vector2 velocity = Vector2.Zero;
        private Vector2 externalVelocity = Vector2.Zero;
        private float gravity = 0;
        private bool coyoteJump = true;
        private float coyoteJumpTime = 0f;
        private bool inAirLastFrame = false;
        private float jumpMaxHeight = float.MaxValue;

        [Export]
        public bool InputLocked { get; set; } = false;
        [Export]
        public bool AnimLocked { get; set; } = false;
        [Export]
        public float Drive { get; set; } = 0;
        public Camera2D Camera2D { get; set; }

        public override void _Ready()
        {
            base._Ready();

            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            bodyParts = GetNode<Node2D>("BodyParts");
            torsoBone = GetNode<Node2D>("BodyParts/TorsoBone");
            footstepPlayer = GetNode<AudioStreamPlayer>("FootstepPlayer");
            fallhitPlayer = GetNode<AudioStreamPlayer>("FallHitPlayer");
        }

        public override void _Process(float delta)
        {
            bodyParts.Rotation = Mathf.LerpAngle(bodyParts.Rotation, targetAngle, delta * 10f);
            torsoBone.Rotation = bodyParts.Scale.x > 0 ? -bodyParts.Rotation : bodyParts.Rotation;
        }

        public override void _PhysicsProcess(float delta)
        {
            float inputX = GetInputX();

            bool isOnFloor = IsOnFloor();

            if (isOnFloor)
            {
                inputDir = inputX;

                Vector2 floorNormal = GetFloorNormal();
                targetAngle = Mathf.Atan2(floorNormal.y, floorNormal.x) + (Mathf.Pi / 2f);
            }
            else
            {
                if (Input.IsActionPressed("move_left") || Input.IsActionPressed("move_right"))
                {
                    inputDir = Mathf.Lerp(inputDir, inputX, delta * 5f);
                    bodyParts.Rotation = 0f;
                }

                jumpMaxHeight = Mathf.Min(jumpMaxHeight, Position.y);
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

            if (((Input.IsActionJustPressed("jump") && !InputLocked)) && (isOnFloor || coyoteJump))
            {
                // Jump
                gravity = -JumpForce;

                bodyParts.Rotation = targetAngle = 0;
                coyoteJump = false;

                JumpEffect jumpEffect = jumpEffectScene.Instance<JumpEffect>();
                jumpEffect.Position = Position;
                jumpEffect.Modulate = Modulate;
                GetParent().AddChild(jumpEffect);
            }

            MoveAndSlideWithSnap(velocity + externalVelocity + new Vector2(0, gravity), Vector2.Down, Vector2.Up, infiniteInertia: false);
            isOnFloor = IsOnFloor();

            if (velocity.x > 0 && bodyParts.Scale.x < 0)
                bodyParts.Scale = new Vector2(1, 1);
            else if (velocity.x < 0 && bodyParts.Scale.x > 0)
                bodyParts.Scale = new Vector2(-1, 1);

            if (!AnimLocked)
            {
                if ((velocity + externalVelocity).LengthSquared() > 8f)
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

            if (!isOnFloor && coyoteJumpTime > 0f)
            {
                coyoteJumpTime -= delta;

                if (coyoteJumpTime < 0f)
                {
                    coyoteJumpTime = 0f;
                    coyoteJump = false;
                }
            }

            if (isOnFloor && !coyoteJump)
            {
                coyoteJump = true;
                coyoteJumpTime = .1f;
            }

            if (!isOnFloor)
            {
                inAirLastFrame = true;
            }
            else if (inAirLastFrame)
            {
                // Just landed
                float fallDistance = Position.y - jumpMaxHeight;
                GD.Print($"fallDistance={fallDistance}");
                inAirLastFrame = false;
                jumpMaxHeight = float.MaxValue;

                if (fallDistance >= 15 && fallDistance < 60)
                {
                    footstepPlayer.Play();
                }
                else if (fallDistance >= 60 && fallDistance < 120)
                {
                    GlobalNodes.INSTANCE.CameraShake(Camera2D, 1);
                    fallhitPlayer.Play();
                }
                else if (fallDistance >= 120)
                {
                    GlobalNodes.INSTANCE.CameraShake(Camera2D, 5);
                    fallhitPlayer.Play();
                }
            }
        }

        private void PlayFootstep()
        {
            if (IsOnFloor())
            {
                footstepPlayer.Play();
            }
        }

        protected virtual float GetInputX()
        {
            if (InputLocked)
            {
                return Drive;
            }
            else
            {
                int inputX = 0;
                if (Input.IsActionPressed("move_left"))
                    inputX -= 1;
                if (Input.IsActionPressed("move_right"))
                    inputX += 1;

                return inputX;
            }
        }

        public void PlayAnimation(string animName)
        {
            animationPlayer.Play(animName);
        }

        public void AddExternalVelocity(Vector2 externalV)
        {
            externalVelocity += externalV;
        }
    }
}