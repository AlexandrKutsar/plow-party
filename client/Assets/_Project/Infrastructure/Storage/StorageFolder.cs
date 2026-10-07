using System.IO;

namespace PlowParty.Infrastructure.Storage
{
    public static class StorageFolder
    {
        private const string FolderName = "PlowParty";

        public static string For(string dataPath, string persistentDataPath, bool isEditor)
        {
            if (!isEditor)
            {
                return Path.Combine(persistentDataPath, FolderName);
            }

            var projectRoot = Path.GetDirectoryName(dataPath.Replace('\\', '/').TrimEnd('/')) ?? dataPath;
            return Path.Combine(projectRoot, "Library", FolderName).Replace('\\', '/');
        }
    }
}
