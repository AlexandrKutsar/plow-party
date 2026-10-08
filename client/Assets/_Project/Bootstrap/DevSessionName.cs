using System;

namespace PlowParty.Bootstrap
{
    public static class DevSessionName
    {
        public const string OverrideVariable = "PLOW_PARTY_DEV_SESSION";

        private const string Prefix = "plow-party-dev";
        private const string VirtualPlayerFolder = "/Library/VP/";
        private const string AgentWorktreeFolder = "/.claude/worktrees/";
        private const uint FnvOffset = 2166136261;
        private const uint FnvPrime = 16777619;

        public static string For(string dataPath, bool isEditor, string overrideName)
        {
            if (!string.IsNullOrEmpty(overrideName))
            {
                return overrideName;
            }

            if (!isEditor)
            {
                return Prefix;
            }

            var root = ProjectRoot(dataPath);
            return root.Contains(AgentWorktreeFolder) ? $"{Prefix}-{Hash(root):x8}" : Prefix;
        }

        private static string ProjectRoot(string dataPath)
        {
            var path = dataPath.Replace('\\', '/').ToLowerInvariant();
            var virtualPlayer = path.IndexOf(VirtualPlayerFolder.ToLowerInvariant(), StringComparison.Ordinal);
            return virtualPlayer >= 0 ? path.Substring(0, virtualPlayer) : path.Substring(0, Math.Max(0, path.LastIndexOf('/')));
        }

        private static uint Hash(string text)
        {
            var hash = FnvOffset;
            for (var i = 0; i < text.Length; i++)
            {
                hash = (hash ^ text[i]) * FnvPrime;
            }

            return hash;
        }
    }
}
