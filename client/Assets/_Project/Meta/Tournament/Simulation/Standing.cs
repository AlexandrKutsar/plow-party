namespace PlowParty.Meta.Tournament.Simulation
{
    public readonly struct Standing
    {
        public Standing(int rank, string nickname, int score, bool isMe)
        {
            Rank = rank;
            Nickname = nickname;
            Score = score;
            IsMe = isMe;
        }

        public int Rank { get; }

        public string Nickname { get; }

        public int Score { get; }

        public bool IsMe { get; }

        public bool IsGap => Rank == 0;

        public static Standing Gap => new Standing(0, string.Empty, 0, false);
    }
}
