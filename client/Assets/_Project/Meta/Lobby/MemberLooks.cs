namespace PlowParty.Meta.Lobby
{
    public sealed class MemberLooks
    {
        public const int SoloId = -1;

        public MemberLook LookOf(int memberId)
        {
            return new MemberLook(UnityEngine.Color.white, memberId - SoloId);
        }
    }
}
