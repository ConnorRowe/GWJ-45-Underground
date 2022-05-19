using Godot;

namespace Underground
{
    public class Character : KinematicBody2D
    {
        private const float JumpForce = 320f;
        private const float GravityAccel = 98f * 8;
        private const float TerminalVel = 400f;
        private const float MaxSpeed = 1600f;
        private const float Acceleration = 1200f;
        private const float MovementDamping = 10f;
        private const float ExternalDamping = 4f;
        private const float HookForce = 1000f;
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
        private bool isHooked = false;
        private Vector2 hookPosition = Vector2.Zero;
        private Label debug;
        private Sprite hook;
        private Sprite chain;
        private Vector2 chainStartPos;
        private bool isHookTraveling = false;
        private const float HookTravelSpeed = 500f;
        private Sprite hookDirDisplay;

        [Export]
        public bool InputLocked { get; set; } = false;
        [Export]
        public bool AnimLocked { get; set; } = false;
        [Export]
        public float Drive { get; set; } = 0;
        public Camera2D Camera2D { get; set; }
        [Export]
        public bool HasHook { get; set; } = true;

        public override void _Ready()
        {
            base._Ready();

            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            bodyParts = GetNode<Node2D>("BodyParts");
            torsoBone = GetNode<Node2D>("BodyParts/TorsoBone");
            footstepPlayer = GetNode<AudioStreamPlayer>("FootstepPlayer");
            fallhitPlayer = GetNode<AudioStreamPlayer>("FallHitPlayer");
            hook = GetNode<Sprite>("Hook");
            chain = GetNode<Sprite>("Chain");
            debug = GetNode<Label>("debug");
            chainStartPos = chain.Position;
            hookDirDisplay = GetNode<Sprite>("HookDirDisplay");

            var c = GetNodeOrNull<Camera2D>("Camera2D");
            if (c != null)
                Camera2D = c;
        }

        public override void _Process(float delta)
        {
            bodyParts.Rotation = Mathf.LerpAngle(bodyParts.Rotation, targetAngle, delta * 10f);
            torsoBone.Rotation = bodyParts.Scale.x > 0 ? -bodyParts.Rotation : bodyParts.Rotation;
            hookDirDisplay.Visible = false;

            if (isHooked || isHookTraveling)
            {
                Vector2 gPos = chain.GlobalPosition;
                float angleToHook = gPos.AngleToPoint(hookPosition);
                chain.Rotation = angleToHook - (Mathf.Pi / 2);
                float connectDist = gPos.DistanceTo(hookPosition);
                float chainLength = connectDist;
                if (isHookTraveling)
                {
                    chainLength = chain.RegionRect.Size.y + (HookTravelSpeed * delta);
                    if (chainLength >= connectDist)
                    {
                        chainLength = connectDist;
                        isHookTraveling = false;
                        isHooked = true;

                        // Autojump when hooked
                        if (IsOnFloor())
                            Jump();
                    }
                }
                chain.RegionRect = new Rect2(0, 0, 1, chainLength);
                chain.Offset = new Vector2(0, chainLength * -.5f);
                hook.GlobalPosition = ToGlobal(chain.Position + (new Vector2(Mathf.Cos(angleToHook), Mathf.Sin(angleToHook)) * -chainLength));
            }
            else if (HasHook)
            {
                hookDirDisplay.Offset = GetLocalMousePosition().Normalized() * new Vector2(15, 25);
                hookDirDisplay.Visible = true;
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            float inputX = GetInputX();

            bool isOnFloor = IsOnFloor();
            bool isTouchingSomething = isOnFloor || IsOnWall() || IsOnCeiling();

            if (!InputLocked && HasHook && Input.IsActionJustPressed("shoot_hook"))
            {
                var spaceState = GetWorld2d().DirectSpaceState;
                var trace = spaceState.IntersectRay(hookDirDisplay.GlobalPosition, hookDirDisplay.GlobalPosition + (GetLocalMousePosition().Normalized() * 400f), new Godot.Collections.Array() { this }, 1);
                if (trace.Count > 0)
                {
                    isHookTraveling = hook.Visible = chain.Visible = true;
                    hookPosition = (Vector2)trace["position"];
                    chain.RegionRect = new Rect2();
                }
            }

            if ((isHooked || isHookTraveling) && Input.IsActionJustReleased("shoot_hook"))
            {
                isHooked = isHookTraveling = hook.Visible = chain.Visible = false;
            }

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

                if (IsOnCeiling())
                {
                    externalVelocity.y = 0;
                }
            }

            velocity -= (velocity * MovementDamping * delta);
            velocity += new Vector2(inputDir, 0) * Acceleration * delta;
            externalVelocity -= (externalVelocity * (isTouchingSomething ? MovementDamping : ExternalDamping) * delta);

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

            if (((Input.IsActionPressed("jump") && !InputLocked)) && (isOnFloor || coyoteJump))
            {
                // Jump
                Jump();
            }

            // Hook velocity
            if (isHooked)
            {
                Vector2 hookDir = GlobalPosition.DirectionTo(hookPosition);

                externalVelocity += hookDir * new Vector2(Mathf.Sign(inputDir) != hookDir.x ? HookForce * .6f : HookForce * .1f, hookDir.y > 0 ? HookForce * .5f : HookForce * 1.25f) * delta;
                externalVelocity += new Vector2(hookDir.x, 0) * HookForce * 1.25f * delta;
            }

            MoveAndSlideWithSnap(velocity + externalVelocity + new Vector2(0, gravity), Vector2.Down, Vector2.Up, infiniteInertia: false);

            isOnFloor = IsOnFloor();

            if (velocity.x > 0 && bodyParts.Scale.x < 0)
                bodyParts.Scale = new Vector2(1, 1);
            else if (velocity.x < 0 && bodyParts.Scale.x > 0)
                bodyParts.Scale = new Vector2(-1, 1);

            if (!AnimLocked)
            {
                if (!isOnFloor && (gravity > 0 || (velocity + externalVelocity).y > 0))
                {
                    if (animationPlayer.CurrentAnimation != "Fall")
                        animationPlayer.Play("Fall");
                }
                else if ((velocity + externalVelocity).LengthSquared() > 8f)
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
                inAirLastFrame = false;
                jumpMaxHeight = float.MaxValue;

                if (fallDistance >= 15 && fallDistance < 100)
                {
                    footstepPlayer.Play();
                }
                else if (fallDistance >= 100)
                {
                    GlobalNodes.CameraShake(Camera2D, fallDistance / 300f);
                    fallhitPlayer.Play();
                }
            }

            // Check for button to press
            var lastSlide = GetLastSlideCollision();
            if (lastSlide != null)
            {
                if (lastSlide.Collider is PressureButton pb && Position.y < pb.GlobalPosition.y)
                    pb.Press();
                if (lastSlide.Collider is CrumblingPlatform cr)
                    cr.Crumble();
            }
        }

        private void Jump()
        {
            gravity = -JumpForce;

            bodyParts.Rotation = targetAngle = 0;
            coyoteJump = false;

            JumpEffect jumpEffect = jumpEffectScene.Instance<JumpEffect>();
            jumpEffect.Position = Position;
            jumpEffect.Modulate = Modulate;
            GetParent().AddChild(jumpEffect);
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