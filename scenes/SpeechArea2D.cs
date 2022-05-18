using Godot;

namespace Underground
{
    public class SpeechArea2D : Area2D
    {
        [Export]
        public string Speech { get; set; } = "";
        private float timer;

        private Sprite sprite;
        private Shaker shaker;

        public override void _Ready()
        {
            sprite = GetNode<Sprite>("Sprite");
            shaker = GetNode<Shaker>("Shaker");
            Connect("body_entered", this, nameof(BodyEntered));
        }

        private void BodyEntered(Node node)
        {
            if (node is Character)
            {
                shaker.Shake(1);
                GlobalNodes.Boing();
            }
        }

        public override void _Process(float delta)
        {
            timer += delta * 2;
            if (timer > Mathf.Tau)
                timer -= Mathf.Tau;

            sprite.Offset = new Vector2(.5f, .5f + (Mathf.Cos(timer) * 6));
        }
    }
}