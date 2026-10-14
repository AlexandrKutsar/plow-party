using System.Collections.Generic;

namespace PlowParty.Meta.Party.Simulation
{
    public static class PartyJoinOrder
    {
        public static List<string> Merge(IReadOnlyList<string> remembered, IReadOnlyList<string> current)
        {
            var order = new List<string>(current);
            for (var i = 0; i < remembered.Count; i++)
            {
                var key = remembered[i];
                if (!string.IsNullOrEmpty(key) && !order.Contains(key))
                {
                    order.Insert(PlaceAfterPredecessor(remembered, i, order), key);
                }
            }

            return order;
        }

        private static int PlaceAfterPredecessor(IReadOnlyList<string> remembered, int index, List<string> order)
        {
            for (var i = index - 1; i >= 0; i--)
            {
                var place = order.IndexOf(remembered[i]);
                if (place >= 0)
                {
                    return place + 1;
                }
            }

            return order.Count > 0 ? 1 : 0;
        }
    }
}
