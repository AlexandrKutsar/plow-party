using System.Collections.Generic;
using Fusion;
using PlowParty.Meta.Party.Config;
using PlowParty.Meta.Party.Simulation;
using PlowParty.Shared;
using UnityEngine;
using VContainer;

namespace PlowParty.Meta.Party.Network
{
    public sealed class PartyLink : NetworkBehaviour, IPlayerJoined, IPlayerLeft
    {
        public const int MaxMembers = 6;

        private readonly List<PartyMember> _readMembers = new List<PartyMember>(MaxMembers);
        private readonly Dictionary<PlayerRef, TickTimer> _pendingRemovals = new Dictionary<PlayerRef, TickTimer>();
        private readonly List<PlayerRef> _expiredRemovals = new List<PlayerRef>();
        private PartyLinks _links;
        private PartyConfig _config;
        private PartyMemory _memory;
        private PartyState _hostState;

        [Networked, Capacity(MaxMembers)] private NetworkArray<PartySeat> Seats { get; }

        [Networked] private int MemberCount { get; set; }

        [Networked] public PartyMode Mode { get; private set; }

        [Networked] public NetworkBool IsSearching { get; private set; }

        [Networked] public int Revision { get; private set; }

        [Inject]
        public void Construct(PartyLinks links, PartyConfig config, PartyMemory memory)
        {
            _links = links;
            _config = config;
            _memory = memory;
        }

        public override void Spawned()
        {
            _links.Attach(this);
            if (!HasStateAuthority)
            {
                return;
            }

            _hostState = new PartyState(Mathf.Min(_config.Capacity, MaxMembers), _memory.HasParty ? _memory.Mode : PartyMode.QuickPlay);
            Admit(Runner.LocalPlayer);
            foreach (var player in Runner.ActivePlayers)
            {
                Admit(player);
            }

            Publish();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _links.Detach(this);
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || _pendingRemovals.Count == 0)
            {
                return;
            }

            _expiredRemovals.Clear();
            foreach (var removal in _pendingRemovals)
            {
                if (removal.Value.Expired(Runner))
                {
                    _expiredRemovals.Add(removal.Key);
                }
            }

            foreach (var player in _expiredRemovals)
            {
                _pendingRemovals.Remove(player);
                Runner.Disconnect(player);
            }
        }

        public void PlayerJoined(PlayerRef player)
        {
            if (!HasStateAuthority || _hostState == null)
            {
                return;
            }

            switch (Admit(player))
            {
                case PartyJoinOutcome.Full:
                    Debug.LogWarning($"[Party] Player {player} refused: the Party is full");
                    Runner.Disconnect(player);
                    return;
                case PartyJoinOutcome.Joined:
                    Publish();
                    return;
            }
        }

        public void PlayerLeft(PlayerRef player)
        {
            if (HasStateAuthority && _hostState != null && _hostState.Leave(player.RawEncoded))
            {
                _pendingRemovals.Remove(player);
                Publish();
            }
        }

        public PartyState Read(int capacity)
        {
            _readMembers.Clear();
            var count = Mathf.Min(MemberCount, MaxMembers);
            for (var i = 0; i < count; i++)
            {
                var seat = Seats[i];
                _readMembers.Add(new PartyMember(seat.Player.RawEncoded, seat.Nickname.ToString(), seat.IsReady));
            }

            return PartyState.Restore(capacity, Mode, _readMembers, IsSearching);
        }

        public void RequestReady(bool isReady)
        {
            RPC_SetReady(isReady);
        }

        public void RequestStartSearch()
        {
            RPC_StartSearch();
        }

        public void RequestStopSearch()
        {
            RPC_StopSearch();
        }

        public void RequestMoveTo(string sessionName)
        {
            if (HasStateAuthority)
            {
                RPC_MoveTo(sessionName);
            }
        }

        public void RequestRemove(int memberId)
        {
            RPC_Remove(PlayerRef.FromEncoded(memberId));
        }

        public void RequestMode(PartyMode mode)
        {
            RPC_SetMode(mode);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_SetReady(NetworkBool isReady, RpcInfo info = default)
        {
            if (_hostState != null && _hostState.SetReady(info.Source.RawEncoded, isReady))
            {
                Publish();
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_StopSearch(RpcInfo info = default)
        {
            if (_hostState == null)
            {
                return;
            }

            _hostState.StopSearch(info.Source.RawEncoded);
            Publish();
            DisconnectGuests();
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_StartSearch(RpcInfo info = default)
        {
            if (_hostState != null && _hostState.StartSearch(info.Source.RawEncoded))
            {
                Publish();
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_MoveTo(NetworkString<_64> sessionName)
        {
            _links.NotifyMoveRequested(sessionName.ToString());
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_Remove(PlayerRef member, RpcInfo info = default)
        {
            if (_hostState == null || !_hostState.Remove(info.Source.RawEncoded, member.RawEncoded))
            {
                return;
            }

            Debug.Log($"[Party] Leader removed Player {member}");
            Publish();
            _pendingRemovals[member] = TickTimer.CreateFromSeconds(Runner, _config.RemovalGraceSeconds);
            RPC_Removed(member);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_SetMode(PartyMode mode, RpcInfo info = default)
        {
            if (_hostState != null && _hostState.SetMode(info.Source.RawEncoded, mode))
            {
                Publish();
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_Removed([RpcTarget] PlayerRef member)
        {
            _links.NotifyRemoved();
        }

        private void DisconnectGuests()
        {
            foreach (var player in Runner.ActivePlayers)
            {
                if (player != Runner.LocalPlayer && !_hostState.Contains(player.RawEncoded))
                {
                    Runner.Disconnect(player);
                }
            }
        }

        private PartyJoinOutcome Admit(PlayerRef player)
        {
            var token = ParticipantToken.TryFromBytes(Runner.GetPlayerConnectionToken(player), out var decoded) ? decoded : null;
            return _hostState.Join(player.RawEncoded, token?.Nickname);
        }

        private void Publish()
        {
            var members = _hostState.Members;
            for (var i = 0; i < MaxMembers; i++)
            {
                Seats.Set(i, i < members.Count ? SeatOf(members[i]) : default);
            }

            MemberCount = members.Count;
            Mode = _hostState.Mode;
            IsSearching = _hostState.IsSearching;
            Revision++;
        }

        private static PartySeat SeatOf(PartyMember member)
        {
            return new PartySeat
            {
                Player = PlayerRef.FromEncoded(member.Id),
                Nickname = member.Nickname,
                IsReady = member.IsReady,
            };
        }
    }
}
