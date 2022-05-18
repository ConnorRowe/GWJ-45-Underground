using Godot;

namespace Underground
{
    public class GlobalNodes : Node
    {
        public static GlobalNodes INSTANCE { get; private set; }

        private Shaker cameraShaker;
        private static AudioStreamPlayer uiClick;
        private static AudioStreamPlayer boing;
        private static AudioStreamPlayer switchClick;
        private static AudioStreamPlayer pop;

        public override void _Ready()
        {
            INSTANCE = this;

            cameraShaker = GetNode<Shaker>("Shaker");
            uiClick = GetNode<AudioStreamPlayer>("UIClick");
            boing = GetNode<AudioStreamPlayer>("Boing");
            switchClick = GetNode<AudioStreamPlayer>("SwitchClick");
            pop = GetNode<AudioStreamPlayer>("Pop");
        }

        public void CameraShake(Camera2D camera2D, float power)
        {
            cameraShaker.SetTarget(camera2D);
            cameraShaker.Shake(power);
        }

        public static void UIClick() => uiClick.Play();
        public static void Boing() => boing.Play();
        public static void SwitchClick() => switchClick.Play();
        public static void Pop() => pop.Play();
    }
}