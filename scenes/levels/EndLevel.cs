using Godot;

namespace Underground
{
    public class EndLevel : StartLevel
    {
        public override void _Ready()
        {
            base._Ready();
            GetNode("DownstairsScene/StartEndBedroomArea").Connect("body_entered", this, nameof(EndStairsBodyEntered));
        }

        private void EndStairsBodyEntered(Node body)
        {
            if (body is Character)
            {
                animationPlayer.Play("EndDownstairsEnd");
            }
        }
    }
}