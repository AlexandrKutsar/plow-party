namespace PlowParty.Meta.Lobby.Simulation
{
    public readonly struct SetupSlotRow
    {
        public SetupSlotRow(int slot, string label, int color, bool canAddBot, bool canChangeBot)
        {
            Slot = slot;
            Label = label;
            Color = color;
            CanAddBot = canAddBot;
            CanChangeBot = canChangeBot;
        }

        public int Slot { get; }

        public string Label { get; }

        public int Color { get; }

        public bool HasColor => Color >= 0;

        public bool CanAddBot { get; }

        public bool CanChangeBot { get; }
    }
}
