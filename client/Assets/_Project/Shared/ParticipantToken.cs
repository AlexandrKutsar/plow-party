using System;
using System.Text;

namespace PlowParty.Shared
{
    public sealed class ParticipantToken
    {
        public const int MaxBytes = 128;
        private const char Separator = '\n';

        public ParticipantToken(string accountId, string nickname)
        {
            AccountId = accountId ?? string.Empty;
            Nickname = nickname ?? string.Empty;
            if (AccountId.IndexOf(Separator) >= 0 || Nickname.IndexOf(Separator) >= 0)
            {
                throw new ArgumentException("Account Id and Nickname must not contain a line break");
            }
        }

        public string AccountId { get; }
        public string Nickname { get; }

        public byte[] ToBytes()
        {
            var bytes = Encoding.UTF8.GetBytes(AccountId + Separator + Nickname);
            if (bytes.Length > MaxBytes)
            {
                throw new InvalidOperationException($"Participant Token is {bytes.Length} bytes, the limit is {MaxBytes}");
            }

            return bytes;
        }

        public static bool TryFromBytes(byte[] bytes, out ParticipantToken token)
        {
            token = null;
            if (bytes == null || bytes.Length == 0 || bytes.Length > MaxBytes)
            {
                return false;
            }

            var text = Encoding.UTF8.GetString(bytes);
            var separator = text.IndexOf(Separator);
            if (separator < 0)
            {
                return false;
            }

            token = new ParticipantToken(text.Substring(0, separator), text.Substring(separator + 1));
            return true;
        }
    }
}
