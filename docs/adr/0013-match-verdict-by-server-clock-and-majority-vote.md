# Match Verdict by server clock and majority Vote, finalized lazily

The Host registers a Match with its full Roster at the start of Countdown; every other Player confirms with their own Auth Token within 15 s, and an unconfirmed seat counts as a Bot. Each Confirmed Player then submits their Match Result as a Vote. The backend accepts the version that a strict majority of the Votes agree on (Scores per Slot and whether the Match was interrupted) and rejects the whole Match when there is no majority or the winning version fails a plausibility check. Time comes only from the server clock: a Vote is refused if it arrives sooner after registration than Countdown plus the claimed play time allow. The Verdict is reached the moment the last Confirmed Player votes, or lazily on the first read after the submission window closes; there is no background job.

## Considered Options

- Trusting client-reported timestamps or durations: free, but the Host writes them.
- Rejecting only the implausible rows instead of the whole Match: a cheating Host forges the whole table, so partial acceptance still credits forged rows of honest-looking Players.
- A scheduler that finalizes Matches when the window closes: exact timing, but a second process to run and test; the Tournament can finalize overdue Matches on read just as well.
- Plurality instead of strict majority: breaks 1-vs-1 and 2-vs-2 disagreements in favour of whoever the tie-break picks, which is always someone the cheater can influence.

## Consequences

A Match with a single Vote ("me + bots", or everyone else left) is guarded only by the plausibility checks, as GDD 9.3 admits. Two colluding Players outvote one honest Player in a three-Player Match. Rows lock with `SELECT … FOR UPDATE` so that two last Votes arriving together reach one Verdict. Plausibility limits (`MAX_SCORE_PER_SECOND`, the Interrupted Match minimum) live as constants in the matches rules and must follow Bucket balance changes.
