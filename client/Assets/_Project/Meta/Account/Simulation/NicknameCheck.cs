namespace PlowParty.Meta.Account.Simulation
{
    public readonly struct NicknameCheck
    {
        private NicknameCheck(string nickname, string error)
        {
            Nickname = nickname;
            Error = error;
        }

        public string Nickname { get; }

        public string Error { get; }

        public bool IsValid => string.IsNullOrEmpty(Error);

        public static NicknameCheck Valid(string nickname)
        {
            return new NicknameCheck(nickname, null);
        }

        public static NicknameCheck Invalid(string error)
        {
            return new NicknameCheck(null, error);
        }
    }
}
