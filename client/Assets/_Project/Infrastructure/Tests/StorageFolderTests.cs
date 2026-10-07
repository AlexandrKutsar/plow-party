using NUnit.Framework;
using PlowParty.Infrastructure.Storage;

namespace PlowParty.Infrastructure.Tests
{
    public sealed class StorageFolderTests
    {
        [Test]
        public void For_Editor_UsesProjectLibrary()
        {
            var folder = StorageFolder.For("C:/Work/client/Assets", "C:/Users/me/LocalLow/Plow Party", true);

            Assert.That(folder, Is.EqualTo("C:/Work/client/Library/PlowParty"));
        }

        [Test]
        public void For_EditorVirtualPlayer_UsesItsOwnLibrary()
        {
            var folder = StorageFolder.For("C:/Work/client/Library/VP/mppm1/Assets", "C:/Users/me/LocalLow/Plow Party", true);

            Assert.That(folder, Is.EqualTo("C:/Work/client/Library/VP/mppm1/Library/PlowParty"));
        }

        [Test]
        public void For_Player_UsesPersistentDataPath()
        {
            var folder = StorageFolder.For("/data/app/base.apk", "/storage/emulated/0/Android/data/plow/files", false);

            Assert.That(folder.Replace('\\', '/'), Is.EqualTo("/storage/emulated/0/Android/data/plow/files/PlowParty"));
        }
    }
}
