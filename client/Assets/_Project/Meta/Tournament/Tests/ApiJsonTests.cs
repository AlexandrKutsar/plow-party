using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using PlowParty.Meta.Tournament.Api;

namespace PlowParty.Meta.Tournament.Tests
{
    public sealed class ApiJsonTests
    {
        [Test]
        public void LeaderboardResponse_BackendJsonWithoutMe_ReadsNullMe()
        {
            const string json = "{\"day\":\"2026-10-07\",\"ends_at\":\"2026-10-08T00:00:00Z\",\"final\":false,\"players\":1,\"me\":null,"
                + "\"entries\":[{\"rank\":1,\"account_id\":\"3a5392be-0263-4447-9b05-bd21cbe7c786\",\"nickname\":\"Plower7676\",\"score\":420}]}";

            var response = JsonConvert.DeserializeObject<LeaderboardResponse>(json);

            Assert.That(response.Me, Is.Null);
            Assert.That(response.Players, Is.EqualTo(1));
            Assert.That(response.Entries[0].Nickname, Is.EqualTo("Plower7676"));
            Assert.That(response.Entries[0].Score, Is.EqualTo(420));
        }

        [Test]
        public void MatchResponse_RejectedJson_ReadsReason()
        {
            const string json = "{\"match_id\":\"0fabef97-a5cc-46f1-98e9-48d46deed48d\",\"status\":\"rejected\",\"rejection_reason\":\"Votes have no majority\","
                + "\"registered_at\":\"2026-10-07T13:50:17Z\",\"interrupted\":null,\"played_seconds\":null,\"weight\":null,\"participants\":[]}";

            var response = JsonConvert.DeserializeObject<MatchResponse>(json);

            Assert.That(response.MatchId, Is.EqualTo("0fabef97-a5cc-46f1-98e9-48d46deed48d"));
            Assert.That(response.Status, Is.EqualTo("rejected"));
            Assert.That(response.RejectionReason, Is.EqualTo("Votes have no majority"));
        }

        [Test]
        public void RegisterMatchRequest_BotSlot_WritesNullAccount()
        {
            var request = new RegisterMatchRequest
            {
                Roster = new List<RosterSlotRequest>
                {
                    new RosterSlotRequest { Slot = 0, AccountId = "3a5392be-0263-4447-9b05-bd21cbe7c786" },
                    new RosterSlotRequest { Slot = 1, AccountId = null },
                },
            };

            Assert.That(
                JsonConvert.SerializeObject(request),
                Is.EqualTo("{\"roster\":[{\"slot\":0,\"account_id\":\"3a5392be-0263-4447-9b05-bd21cbe7c786\"},{\"slot\":1,\"account_id\":null}]}"));
        }

        [Test]
        public void VoteRequest_FullMatch_WritesNullInterruption()
        {
            var request = new VoteRequest { Scores = new List<SlotScore> { new SlotScore { Slot = 0, Score = 120 } } };

            Assert.That(JsonConvert.SerializeObject(request), Is.EqualTo("{\"scores\":[{\"slot\":0,\"score\":120}],\"interrupted_at_seconds\":null}"));
        }
    }
}
