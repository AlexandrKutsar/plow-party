using NUnit.Framework;
using PlowParty.Infrastructure.Backend;

namespace PlowParty.Infrastructure.Tests
{
    public sealed class BackendResponseTests
    {
        [TestCase(false, 0, BackendOutcome.Offline)]
        [TestCase(true, 0, BackendOutcome.Offline)]
        [TestCase(true, 200, BackendOutcome.Ok)]
        [TestCase(true, 204, BackendOutcome.Ok)]
        [TestCase(true, 401, BackendOutcome.Unauthorized)]
        [TestCase(true, 409, BackendOutcome.Rejected)]
        [TestCase(true, 422, BackendOutcome.Rejected)]
        [TestCase(true, 500, BackendOutcome.Rejected)]
        public void OutcomeFor_StatusCode_MapsToOutcome(bool reachedServer, long statusCode, BackendOutcome expected)
        {
            Assert.That(BackendResponse.OutcomeFor(reachedServer, statusCode), Is.EqualTo(expected));
        }

        [Test]
        public void ValidationMessage_PydanticDetail_ReturnsFirstMessageWithoutPrefix()
        {
            var response = new BackendResponse(
                BackendOutcome.Rejected,
                422,
                "{\"detail\":[{\"type\":\"value_error\",\"loc\":[\"body\",\"nickname\"],\"msg\":\"Value error, Nickname must be at least 3 characters\"}]}");

            Assert.That(response.ValidationMessage(), Is.EqualTo("Nickname must be at least 3 characters"));
        }

        [Test]
        public void ValidationMessage_PlainDetail_ReturnsIt()
        {
            var response = new BackendResponse(BackendOutcome.Rejected, 409, "{\"detail\":\"Confirmation closed\"}");

            Assert.That(response.ValidationMessage(), Is.EqualTo("Confirmation closed"));
        }

        [Test]
        public void ValidationMessage_NotJson_ReturnsEmpty()
        {
            var response = new BackendResponse(BackendOutcome.Rejected, 500, "Internal Server Error");

            Assert.That(response.ValidationMessage(), Is.Empty);
        }
    }
}
