# Guest accounts: client-generated Device Id, opaque rotating Auth Token

A guest logs in with a Device Id that the client generates once as a UUID and keeps in local storage, not with Unity's `SystemInfo.deviceUniqueIdentifier`: that value is identical for every virtual Player in Multiplayer Play Mode and its format differs per platform. Login answers with a random opaque Auth Token, of which the backend stores only the SHA-256 hash; every login issues a new Auth Token and invalidates the previous one, and tokens do not expire. Clients send it as `Authorization: Bearer`, log in on every app start, and log in again on a 401.

## Considered Options

- JWT: verifiable without a database read, but needs a signing secret in configuration and cannot be revoked; one indexed lookup per request costs nothing at this scale.
- Non-rotating token: simpler, but leaves no way to cut off a leaked token short of a database edit.

## Consequences

The Device Id is the real credential: whoever reads it from the device owns the Account, and reinstalling the app starts a new Account. The Auth Token only spares sending the Device Id on every request and makes revocation possible. There is no rate limiting, so a script can create Accounts at will; Tournament results stay guarded by the Match checks (GDD 9.3), not by Account creation. Other features authenticate through the accounts service's current-Account dependency and never read the token table.
