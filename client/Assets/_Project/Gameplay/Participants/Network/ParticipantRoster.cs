using Fusion;
using PlowParty.Gameplay.Participants.Simulation;
using PlowParty.Gameplay.Vehicle.Network;

namespace PlowParty.Gameplay.Participants.Network
{
    public sealed class ParticipantRoster : NetworkBehaviour
    {
        public const int MaxSlots = VehicleWorldDriver.MaxVehicles;

        private readonly string[] _accountIds = new string[MaxSlots];
        private readonly NetworkString<_16>[] _shownNicknames = new NetworkString<_16>[MaxSlots];
        private readonly string[] _nicknameTexts = new string[MaxSlots];
        private bool _isSpawned;

        [Networked, Capacity(MaxSlots)] private NetworkArray<ParticipantSeat> Seats { get; }

        [Networked] private int OpenSlots { get; set; }

        public bool IsReady => _isSpawned;

        public int SlotCount => _isSpawned ? OpenSlots : 0;

        public int SeatedCount => CountSeats(false) + CountSeats(true);

        public int BotCount => CountSeats(true);

        public int PlayerCount => CountSeats(false);

        public override void Spawned()
        {
            _isSpawned = true;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _isSpawned = false;
        }

        public bool IsSeated(int slot)
        {
            return HasSlot(slot) && Seats[slot].IsSeated;
        }

        public bool IsBot(int slot)
        {
            return IsSeated(slot) && Seats[slot].IsBot;
        }

        public bool TryGetProfile(int slot, out ParticipantProfile profile)
        {
            if (!IsSeated(slot))
            {
                profile = default;
                return false;
            }

            var seat = Seats[slot];
            profile = new ParticipantProfile(NicknameOf(slot), (CritterSpecies)seat.Species, seat.Color, seat.IsBot);
            return true;
        }

        public string NicknameOf(int slot)
        {
            if (!IsSeated(slot))
            {
                return ParticipantProfiles.FallbackNickname(slot);
            }

            var nickname = Seats[slot].Nickname;
            if (_nicknameTexts[slot] == null || !_shownNicknames[slot].Equals(nickname))
            {
                _shownNicknames[slot] = nickname;
                _nicknameTexts[slot] = nickname.ToString();
            }

            return _nicknameTexts[slot];
        }

        public int ColorOf(int slot)
        {
            return IsSeated(slot) ? Seats[slot].Color : ParticipantColors.NoPreference;
        }

        public bool TryGetAccountId(int slot, out string accountId)
        {
            accountId = slot >= 0 && slot < MaxSlots ? _accountIds[slot] : null;
            return !string.IsNullOrEmpty(accountId);
        }

        public void OpenSeats(int slotCount)
        {
            if (HasStateAuthority)
            {
                OpenSlots = slotCount;
            }
        }

        public void Seat(int slot, ParticipantProfile profile, string accountId)
        {
            if (!HasStateAuthority || slot < 0 || slot >= MaxSlots)
            {
                return;
            }

            _accountIds[slot] = profile.IsBot ? null : accountId;
            Seats.Set(slot, new ParticipantSeat
            {
                Nickname = profile.Nickname,
                Species = (byte)profile.Species,
                Color = (byte)profile.Color,
                IsBot = profile.IsBot,
                IsSeated = true,
            });
        }

        public void Vacate(int slot)
        {
            if (!HasStateAuthority || slot < 0 || slot >= MaxSlots)
            {
                return;
            }

            _accountIds[slot] = null;
            Seats.Set(slot, default);
        }

        private bool HasSlot(int slot)
        {
            return _isSpawned && slot >= 0 && slot < MaxSlots;
        }

        private int CountSeats(bool bots)
        {
            var count = 0;
            for (var slot = 0; slot < MaxSlots; slot++)
            {
                if (IsSeated(slot) && Seats[slot].IsBot == bots)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
