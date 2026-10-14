namespace PlowParty.Meta.Lobby.Simulation
{
    public static class PodiumLayout
    {
        public static float Offset(int position, int count, float spacing)
        {
            return (position - (count - 1) * 0.5f) * spacing;
        }

        public static float Scale(int count, float spacing, float stageWidth)
        {
            var rowWidth = count * spacing;
            return rowWidth <= stageWidth ? 1f : stageWidth / rowWidth;
        }
    }
}
