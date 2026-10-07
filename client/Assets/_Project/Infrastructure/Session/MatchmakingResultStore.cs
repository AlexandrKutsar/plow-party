using PlowParty.Shared;

namespace PlowParty.Infrastructure.Session
{
    public sealed class MatchmakingResultStore
    {
        private MatchmakingResult? _matchmakingResult;

        public void Set(MatchmakingResult matchmakingResult)
        {
            _matchmakingResult = matchmakingResult;
        }

        public bool TryGet(out MatchmakingResult matchmakingResult)
        {
            matchmakingResult = _matchmakingResult.GetValueOrDefault();
            return _matchmakingResult.HasValue;
        }

        public void Clear()
        {
            _matchmakingResult = null;
        }
    }
}
