namespace PlowParty.Meta.Account
{
    public readonly struct RenameResult
    {
        private RenameResult(bool renamed, string error)
        {
            Renamed = renamed;
            Error = error;
        }

        public bool Renamed { get; }

        public string Error { get; }

        public static RenameResult Success()
        {
            return new RenameResult(true, null);
        }

        public static RenameResult Failure(string error)
        {
            return new RenameResult(false, error);
        }
    }
}
