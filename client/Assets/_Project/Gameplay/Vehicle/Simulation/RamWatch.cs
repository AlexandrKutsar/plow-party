namespace PlowParty.Gameplay.Vehicle.Simulation
{
    public sealed class RamWatch
    {
        private bool _hasBaseline;
        private int _seenTimesRammed;
        private int _seenRamsDealt;

        public RamRole Observe(int timesRammed, int ramsDealt)
        {
            if (!_hasBaseline)
            {
                _hasBaseline = true;
                _seenTimesRammed = timesRammed;
                _seenRamsDealt = ramsDealt;
                return RamRole.None;
            }

            var wasRammed = timesRammed > _seenTimesRammed;
            var dealtRam = ramsDealt > _seenRamsDealt;
            if (wasRammed)
            {
                _seenTimesRammed = timesRammed;
            }

            if (dealtRam)
            {
                _seenRamsDealt = ramsDealt;
            }

            if (wasRammed)
            {
                return RamRole.Victim;
            }

            return dealtRam ? RamRole.Rammer : RamRole.None;
        }

        public void Reset()
        {
            _hasBaseline = false;
        }
    }
}
