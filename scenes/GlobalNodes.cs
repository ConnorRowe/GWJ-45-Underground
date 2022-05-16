using Godot;

namespace Underground
{
    public class GlobalNodes : Node
    {
        public static GlobalNodes INSTANCE { get; private set; }

        private Shaker cameraShaker;

        public override void _Ready()
        {
            INSTANCE = this;

            cameraShaker = GetNode<Shaker>("Shaker");
        }

        public void CameraShake(Camera2D camera2D, float power)
        {
            cameraShaker.SetTarget(camera2D);
            cameraShaker.Shake(power);
        }
    }
}