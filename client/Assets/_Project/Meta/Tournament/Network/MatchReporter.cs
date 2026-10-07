using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlowParty.Infrastructure.Network;
using PlowParty.Meta.Tournament.Config;
using PlowParty.Meta.Tournament.Simulation;
using VContainer.Unity;

namespace PlowParty.Meta.Tournament.Network
{
    public sealed class MatchReporter : ITickable, IDisposable
    {
        private readonly IMatchProgress _progress;
        private readonly IMatchRoster _roster;
        private readonly MatchReportLinks _links;
        private readonly MatchReportApi _api;
        private readonly NetworkSession _session;
        private readonly TournamentConfig _config;
        private readonly Dictionary<int, int> _scores = new Dictionary<int, int>();
        private int _joinedMatch;
        private int _reportedMatch;
        private int _votedMatch;
        private string _matchId;
        private List<int> _rosterSlots;
        private MatchProgressPhase _lastPhase = MatchProgressPhase.Waiting;
        private float _lastPlayingElapsed;

        public MatchReporter(IMatchProgress progress, IMatchRoster roster, MatchReportLinks links, MatchReportApi api, NetworkSession session, TournamentConfig config)
        {
            _progress = progress;
            _roster = roster;
            _links = links;
            _api = api;
            _session = session;
            _config = config;
            _session.Ended += OnSessionEnded;
        }

        public void Tick()
        {
            if (!_session.IsRunning || !_progress.IsRunning)
            {
                return;
            }

            var match = _progress.MatchNumber;
            var phase = _progress.Phase;
            if (_session.IsHost)
            {
                _links.SpawnOnce(_session.Runner, _config.ReportLinkPrefab);
                RegisterOnCountdown(match, phase);
            }
            else
            {
                ConfirmPublishedMatch(match);
            }

            TrackPlaying(match, phase);
            VoteOnResults(match, phase);
            _lastPhase = phase;
        }

        public void Dispose()
        {
            _session.Ended -= OnSessionEnded;
        }

        private void RegisterOnCountdown(int match, MatchProgressPhase phase)
        {
            if (phase != MatchProgressPhase.Countdown || _joinedMatch == match)
            {
                return;
            }

            _joinedMatch = match;
            RegisterAsync(match).Forget();
        }

        private async UniTask RegisterAsync(int match)
        {
            var roster = MatchReportRules.BuildRoster(_roster.ReadSeats());
            if (!MatchReportRules.CanRegister(roster, _api.AccountId))
            {
                return;
            }

            var matchId = await _api.RegisterAsync(roster);
            if (matchId == null)
            {
                return;
            }

            var slots = new List<int>();
            foreach (var slot in roster.Roster)
            {
                slots.Add(slot.Slot);
            }

            Report(match, matchId, slots);
            if (_links.Current != null)
            {
                _links.Current.Publish(match, matchId, MatchReportRules.SlotMask(slots));
            }
        }

        private void ConfirmPublishedMatch(int match)
        {
            var link = _links.Current;
            if (link == null || _joinedMatch == match || link.MatchNumber != match || string.IsNullOrEmpty(link.MatchId))
            {
                return;
            }

            _joinedMatch = match;
            ConfirmAsync(match, link.MatchId, MatchReportRules.SlotsOf(link.RosterSlotMask)).Forget();
        }

        private async UniTask ConfirmAsync(int match, string matchId, List<int> slots)
        {
            if (await _api.ConfirmAsync(matchId))
            {
                Report(match, matchId, slots);
            }
        }

        private void Report(int match, string matchId, List<int> slots)
        {
            _reportedMatch = match;
            _matchId = matchId;
            _rosterSlots = slots;
        }

        private void TrackPlaying(int match, MatchProgressPhase phase)
        {
            if (phase != MatchProgressPhase.Playing || _reportedMatch != match)
            {
                return;
            }

            _lastPlayingElapsed = _progress.PlayingElapsed;
            _progress.ReadLiveScores(_scores);
        }

        private void VoteOnResults(int match, MatchProgressPhase phase)
        {
            if (phase != MatchProgressPhase.Results || _reportedMatch != match || _votedMatch == match)
            {
                return;
            }

            _votedMatch = match;
            _progress.ReadFinalScores(_scores);
            _api.VoteAsync(_matchId, MatchReportRules.BuildVote(_rosterSlots, _scores, null)).Forget();
        }

        private void OnSessionEnded(SessionEnd end)
        {
            if (end != SessionEnd.Lost
                || _lastPhase != MatchProgressPhase.Playing
                || _reportedMatch == 0
                || _reportedMatch == _votedMatch
                || !MatchReportRules.TryGetInterruptedSecond(_lastPlayingElapsed, out var second))
            {
                return;
            }

            _votedMatch = _reportedMatch;
            _api.VoteAsync(_matchId, MatchReportRules.BuildVote(_rosterSlots, _scores, second)).Forget();
        }
    }
}
