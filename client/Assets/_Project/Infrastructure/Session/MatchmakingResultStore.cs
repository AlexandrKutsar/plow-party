using PlowParty.Shared;

namespace PlowParty.Infrastructure.Session
{
    public sealed class MatchLineupStore
    {
        private MatchLineup? _lineup;

        public void Set(MatchLineup lineup)
        {
            _lineup = lineup;
        }

        public bool TryGet(out MatchLineup lineup)
        {
            lineup = _lineup.GetValueOrDefault();
            return _lineup.HasValue;
        }

        public void Clear()
        {
            _lineup = null;
        }
    }
}
