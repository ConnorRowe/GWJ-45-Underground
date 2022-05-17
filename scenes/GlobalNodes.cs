using Godot;

namespace Underground
{
    public class GlobalNodes : Node
    {
        public static GlobalNodes INSTANCE { get; private set; }

        private Shaker cameraShaker;
        private static AudioStreamPlayer uIClickPlayer;

        public override void _Ready()
        {
            INSTANCE = this;

            cameraShaker = GetNode<Shaker>("Shaker");
            uIClickPlayer = GetNode<AudioStreamPlayer>("UIClick");
        }

        public void CameraShake(Camera2D camera2D, float power)
        {
            cameraShaker.SetTarget(camera2D);
            cameraShaker.Shake(power);
        }

        public static void UIClick()
        {
            uIClickPlayer.Play();
        }
    }
}