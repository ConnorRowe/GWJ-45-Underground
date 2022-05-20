using Godot;

namespace Underground
{
    public class StartLevel : Node2D
    {
        private AnimationPlayer animationPlayer;
        public override void _Ready()
        {
            animationPlayer = GetNode<AnimationPlayer>("BedroomScene/AnimationPlayer");
            GetNode("BedroomScene/BedroomDoor").Connect("body_entered", this, nameof(BodyEnteredSoPlayAnim), new Godot.Collections.Array() { "BedroomEnd" });
            GetNode("DownstairsScene/CrumblingFloor/CrumbleLabel/Area2D").Connect("body_entered", this, nameof(BodyEnteredSoPlayAnim), new Godot.Collections.Array() { "CrumbleFloor" });
            GetNode<Character>("Character").Camera2D = GetNode<Camera2D>("Camera2D");

            GlobalNodes.PlayMusicTrack(GD.Load<AudioStreamOGGVorbis>("res://audio/music/track_2.ogg"));
        }

        private void BodyEnteredSoPlayAnim(Node body, string animName)
        {
            if (body is Character)
                animationPlayer.Play(animName);
        }

        private void SwitchSceneTo(string packedScenePath)
        {
            GetTree().ChangeScene(packedScenePath);
            QueueFree();
        }

        private void FadeMusic()
        {
            GlobalNodes.FadeMusic(5f);
        }
    }
}