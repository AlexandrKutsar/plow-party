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
        private const int MaxMembers = PartyState.MaxCapacity;
        private const byte NoBot = 0;

        private readonly List<PartyMember> _readMembers = new List<PartyMember>(MaxMembers);
        private readonly List<PartyBot> _readBots = new List<PartyBot>(MaxMembers);
        private readonly Dictionary<PlayerRef, TickTimer> _pendingRemovals = new Dictionary<PlayerRef, TickTimer>();
        private readonly List<PlayerRef> _expiredRemovals = new List<PlayerRef>();
        private readonly System.Random _random = new System.Random();
        private PartyLinks _links;
        private PartyConfig _config;
        private PartyMemory _memory;
        private PartyLooks _looks;
        private PartyState _hostState;

        [Networked, Capacity(MaxMembers)] private NetworkArray<PartySeat> Seats { get; }

        [Networked, Capacity(MaxMembers)] private NetworkArray<byte> BotSlots { get; }

        [Networked] private int MemberCount { get; set; }

        [Networked] public PartyMode Mode { get; private set; }

        [Networked] public NetworkBool IsSearching { get; private set; }

        [Networked] public int Revision { get; private set; }

        [Inject]
        public void Construct(PartyLinks links, PartyConfig config, PartyMemory memory, IParticipantPalette palette)
        {
            _links = links;
            _config = config;
            _memory = memory;
            _looks = new PartyLooks(palette.ColorCount, palette.CritterCount);
        }

        public override void Spawned()
        {
            _links.Attach(this);
            if (!HasStateAuthority)
            {
                return;
            }

            _hostState = new PartyState(_config.Capacity, _memory.HasParty ? _memory.Mode : PartyMode.QuickPlay, _looks, _memory.JoinOrder);
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

        public PartyState Read(int capacity, PartyLooks looks)
        {
            _readMembers.Clear();
            var count = Mathf.Min(MemberCount, MaxMembers);
            for (var i = 0; i < count; i++)
            {
                var seat = Seats[i];
                _readMembers.Add(new PartyMember(seat.Player.RawEncoded, seat.Nickname.ToString(), seat.IsReady, seat.Slot, seat.Color, seat.Critter, seat.Key.ToString()));
            }

            _readBots.Clear();
            for (var slot = 0; slot < MaxMembers; slot++)
            {
                var bot = BotSlots[slot];
                if (bot != NoBot)
                {
                    _readBots.Add(new PartyBot(slot, (BotDifficulty)(bot - 1)));
                }
            }

            return PartyState.Restore(capacity, Mode, looks, _readMembers, IsSearching, _readBots);
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

        public void RequestBot(int slot, BotDifficulty difficulty)
        {
            RPC_SetBot(slot, (int)difficulty);
        }

        public void RequestRemoveBot(int slot)
        {
            RPC_RemoveBot(slot);
        }

        public void RequestColor(int color)
        {
            RPC_PickColor(color);
        }

        public void RequestCritter(int critter)
        {
            RPC_PickCritter(critter);
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

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_SetBot(int slot, int difficulty, RpcInfo info = default)
        {
            if (_hostState != null && _hostState.SetBot(info.Source.RawEncoded, slot, (BotDifficulty)difficulty))
            {
                Publish();
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_RemoveBot(int slot, RpcInfo info = default)
        {
            if (_hostState != null && _hostState.RemoveBot(info.Source.RawEncoded, slot))
            {
                Publish();
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_PickColor(int color, RpcInfo info = default)
        {
            if (_hostState != null && _hostState.PickColor(info.Source.RawEncoded, color))
            {
                Publish();
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_PickCritter(int critter, RpcInfo info = default)
        {
            if (_hostState != null && _hostState.PickCritter(info.Source.RawEncoded, critter))
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
            return _hostState.Join(player.RawEncoded, token?.Nickname, _random.Next(_looks.CritterCount), MemberKey(token));
        }

        private void Publish()
        {
            var members = _hostState.Members;
            for (var i = 0; i < MaxMembers; i++)
            {
                Seats.Set(i, i < members.Count ? SeatOf(members[i]) : default);
                BotSlots.Set(i, _hostState.TryGetBot(i, out var bot) ? (byte)((int)bot.Difficulty + 1) : NoBot);
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
                Slot = (byte)member.Slot,
                Color = (byte)member.Color,
                Critter = (byte)member.Critter,
                Key = member.Key,
            };
        }

        private static string MemberKey(ParticipantToken token)
        {
            if (token == null)
            {
                return null;
            }

            return string.IsNullOrEmpty(token.AccountId) ? token.Nickname : token.AccountId;
        }
    }
}
