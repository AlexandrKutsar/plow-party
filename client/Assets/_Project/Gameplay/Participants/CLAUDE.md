# Participants

Who sits in each Slot of a Match: the Participant Profile (Nickname, Critter Species, Player or Bot), synced to every peer, and the Critter riding each Vehicle (GDD 3.1, 8). Match seats Participants; Hud and Bots read them. Spec: `.scratch/bots/spec.md`.

## Entry points

- `ParticipantProfiles` — pure naming and Critter rules, the test seam: `PlayerNickname(tokenNickname, slot)` (trimmed, cut to `MaxNicknameLength` 16 like the backend, else the fallback), `FallbackNickname(slot)` ("Игрок N"), `BotNickname(pool, taken, slot, random)` (an unused pool name, else a pool name with a number, else the fallback), `PickSpecies(taken, random)` (an unused Critter Species when one is left).
- `ParticipantProfile` — Nickname, `CritterSpecies`, `IsBot`.
- `ParticipantRoster` — scene `NetworkObject` (`Prefabs/ParticipantRoster.prefab`, placed in `Match.unity`), registered in `MatchScope` with `RegisterComponentInHierarchy`. `[Networked] NetworkArray<ParticipantSeat>` indexed by Slot plus the open Slot count. Every peer reads `SlotCount`, `SeatedCount`, `PlayerCount`, `BotCount`, `IsSeated`, `IsBot`, `TryGetProfile`, `NicknameOf(slot)` (fallback name for an empty Slot; the string is cached until the networked Nickname changes, so steady frames allocate nothing). The Host writes `OpenSeats`, `Seat(slot, profile, accountId)`, `Vacate(slot)`; it alone keeps each Player's Account Id (`TryGetAccountId`, never networked) for the backend Roster.
- `CritterSeatView` — on `Vehicle.prefab`; finds the `CritterSeat` anchor in the nested `V_Vehicle` model and instantiates the visual prefab of the Slot's Critter Species under it once the profile arrives (and again if it changes).
- `ParticipantsConfig` — Bot Nickname pool and the six `V_Critter_<Species>` prefabs in `CritterSpecies` order; asset `_Project/Configs/ParticipantsConfig.asset`, registered in `RootLifetimeScope`.

## Rules worth knowing

- The profile lives per Slot on a scene object, not on the Vehicle, so it outlives any Vehicle respawn and stays the Match's record of who sits where.
- A Bot looks like a Player everywhere on screen (GDD 8): pool Nicknames, random Critters, no marker. `IsBot` is networked only because the Host needs it after a restart and a client may need it later (portraits); views must not show it.
- Player Critters are random for now; a chosen Critter would come in the `ParticipantToken` from Meta.

## Depends on

Vehicle (`NetworkVehicle`, `VehicleWorldDriver.MaxVehicles`), Fusion, VContainer. Match, Bots and Hud depend on this module; it references none of them. The assembly is in Fusion's `AssembliesToWeave`.
