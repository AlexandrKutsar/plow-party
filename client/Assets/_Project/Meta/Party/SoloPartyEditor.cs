using System;
using PlowParty.Meta.Party.Simulation;
using PlowParty.Shared;

namespace PlowParty.Meta.Party
{
    internal sealed class SoloPartyEditor : IPartyEditor
    {
        private readonly PartyMemory _memory;
        private readonly PartyLooks _looks;
        private readonly int _capacity;
        private readonly Random _random;

        public SoloPartyEditor(PartyMemory memory, PartyLooks looks, int capacity, Random random)
        {
            _memory = memory;
            _looks = looks;
            _capacity = capacity;
            _random = random;
        }

        public event Action Changed;

        public PartyState State { get; private set; }

        public PartyState Reset(string nickname)
        {
            State = new PartyState(_capacity, _memory.SoloMode, _looks);
            State.Join(PartyState.SoloId, nickname, _random.Next(Math.Max(1, _looks.CritterCount)));
            if (_memory.HasLook)
            {
                State.PickColor(PartyState.SoloId, _memory.Color);
                State.PickCritter(PartyState.SoloId, _memory.Critter);
                foreach (var bot in _memory.Bots)
                {
                    State.SetBot(PartyState.SoloId, bot.Slot, bot.Difficulty);
                }
            }

            return State;
        }

        public void SetMode(PartyMode mode)
        {
            _memory.RememberSoloMode(mode);
            Edit(State.SetMode(PartyState.SoloId, mode));
        }

        public void SetBot(int slot, BotDifficulty difficulty)
        {
            Edit(State.SetBot(PartyState.SoloId, slot, difficulty));
        }

        public void RemoveBot(int slot)
        {
            Edit(State.RemoveBot(PartyState.SoloId, slot));
        }

        public void PickColor(int color)
        {
            Edit(State.PickColor(PartyState.SoloId, color));
        }

        public void PickCritter(int critter)
        {
            Edit(State.PickCritter(PartyState.SoloId, critter));
        }

        private void Edit(bool changed)
        {
            if (!changed || !State.TryGetMember(PartyState.SoloId, out var solo))
            {
                return;
            }

            _memory.RememberSetup(solo.Color, solo.Critter, State.Bots);
            Changed?.Invoke();
        }
    }
}
