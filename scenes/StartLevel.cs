using Godot;

namespace Underground
{
    public class StartLevel : BaseLevel
    {
        protected AnimationPlayer animationPlayer;
        public override void _Ready()
        {
            base._Ready();

            animationPlayer = GetNode<AnimationPlayer>("BedroomScene/AnimationPlayer");
            GetNode("BedroomScene/BedroomDoor").Connect("body_entered", this, nameof(BodyEnteredSoPlayAnim), new Godot.Collections.Array() { "BedroomEnd" });
            GetNode("DownstairsScene/CrumblingFloor/CrumbleLabel/Area2D").Connect("body_entered", this, nameof(BodyEnteredSoPlayAnim), new Godot.Collections.Array() { "CrumbleFloor" });
            GetNode<Character>("Character").Camera2D = GetNode<Camera2D>("Camera2D");
            var controlsInfo = GetNode<RichTextLabel>("BedroomScene/ControlsInfo");
            controlsInfo.BbcodeText = MainMenu.ReplaceInputTags(controlsInfo.BbcodeText);
        }

        private void BodyEnteredSoPlayAnim(Node body, string animName)
        {
            if (body is Character)
                animationPlayer.Play(animName);
        }

        private void FadeMusic()
        {
            GlobalNodes.FadeMusic(5f);
        }
    }
}