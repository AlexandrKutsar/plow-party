namespace PlowParty.Infrastructure.Backend
{
    public sealed class BackendResponse
    {
        public BackendResponse(BackendOutcome outcome, long statusCode, string body)
        {
            Outcome = outcome;
            StatusCode = statusCode;
            Body = body ?? string.Empty;
        }

        public BackendOutcome Outcome { get; }

        public long StatusCode { get; }

        public string Body { get; }

        public bool IsOk => Outcome == BackendOutcome.Ok;

        public static BackendOutcome OutcomeFor(bool reachedServer, long statusCode)
        {
            if (!reachedServer || statusCode == 0)
            {
                return BackendOutcome.Offline;
            }

            if (statusCode == 401)
            {
                return BackendOutcome.Unauthorized;
            }

            return statusCode >= 200 && statusCode < 300 ? BackendOutcome.Ok : BackendOutcome.Rejected;
        }

        public T Read<T>()
        {
            return BackendJson.Deserialize<T>(Body);
        }

        public string ValidationMessage()
        {
            return BackendJson.FirstValidationMessage(Body);
        }
    }
}
