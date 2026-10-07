namespace PlowParty.Infrastructure.Backend
{
    public sealed class BackendRequest
    {
        private BackendRequest(string method, string path, object body, string authToken)
        {
            Method = method;
            Path = path;
            Body = body;
            AuthToken = authToken;
        }

        public string Method { get; }

        public string Path { get; }

        public object Body { get; }

        public string AuthToken { get; }

        public static BackendRequest Get(string path)
        {
            return new BackendRequest("GET", path, null, null);
        }

        public static BackendRequest Post(string path, object body = null)
        {
            return new BackendRequest("POST", path, body, null);
        }

        public static BackendRequest Patch(string path, object body)
        {
            return new BackendRequest("PATCH", path, body, null);
        }

        public BackendRequest WithAuthToken(string authToken)
        {
            return new BackendRequest(Method, Path, Body, authToken);
        }
    }
}
