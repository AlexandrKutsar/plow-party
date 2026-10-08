# Participants

Who sits in each Slot of a Match: the Participant Profile (Nickname, Critter Species, Participant Color, Player or Bot), synced to every peer, the Critter riding each Vehicle and the colour of its body (GDD 3.1, 8). Match seats Participants; Hud and Bots read them. Specs: `.scratch/bots/spec.md`, `.scratch/party/spec.md` (Participant Color).

## Entry points

- `ParticipantProfiles` — pure naming and Critter rules, the test seam: `PlayerNickname(tokenNickname, slot)` (trimmed, cut to `MaxNicknameLength` 16 like the backend, else the fallback), `FallbackNickname(slot)` ("Игрок N"), `BotNickname(pool, taken, slot, random)` (an unused pool name, else a pool name with a number, else the fallback), `PickSpecies(taken, random)` (an unused Critter Species when one is left).
- `ParticipantColors` — pure Participant Color rule, the test seam: `Assign(preferred, taken, paletteSize, random)`. `NoPreference` (-1) gives a random free palette index; a free preference is kept; a taken one gives the nearest free index in palette order (next before previous, wrapping around); with every colour taken it still returns a palette index.
- `ParticipantProfile` — Nickname, `CritterSpecies`, `Color` (palette index), `IsBot`.
- `ParticipantRoster` — scene `NetworkObject` (`Prefabs/ParticipantRoster.prefab`, placed in `Match.unity`), registered in `MatchScope` with `RegisterComponentInHierarchy`. `[Networked] NetworkArray<ParticipantSeat>` indexed by Slot plus the open Slot count. Every peer reads `SlotCount`, `SeatedCount`, `PlayerCount`, `BotCount`, `IsSeated`, `IsBot`, `TryGetProfile`, `ColorOf(slot)` (palette index, `NoPreference` for an empty Slot), `NicknameOf(slot)` (fallback name for an empty Slot; the string is cached until the networked Nickname changes, so steady frames allocate nothing). The Host writes `OpenSeats`, `Seat(slot, profile, accountId)`, `Vacate(slot)`; it alone keeps each Player's Account Id (`TryGetAccountId`, never networked) for the backend Roster.
- `CritterSeatView` — on `Vehicle.prefab`; finds the `CritterSeat` anchor in the nested `V_Vehicle` model and instantiates the visual prefab of the Slot's Critter Species under it once the profile arrives (and again if it changes).
- `VehicleBodyColorView` — on `Vehicle.prefab`; finds the `Body` renderer in the nested `V_Vehicle` model (hood, rear body and fenders, painted white in the palette) and sets `_BaseColor` through a `MaterialPropertyBlock` whenever the Slot's colour changes, so `M_Palette` is never copied.
- `ParticipantsConfig` — Bot Nickname pool, the six `V_Critter_<Species>` prefabs in `CritterSpecies` order, and the Participant Color palette (`ColorCount`, `ParticipantColor(index)`, white for an unknown index); asset `_Project/Configs/ParticipantsConfig.asset`, registered in `RootLifetimeScope`.

## Rules worth knowing

- The profile lives per Slot on a scene object, not on the Vehicle, so it outlives any Vehicle respawn and stays the Match's record of who sits where.
- A Bot looks like a Player everywhere on screen (GDD 8): pool Nicknames, random Critters, no marker. `IsBot` is networked only because the Host needs it after a restart and a client may need it later (portraits); views must not show it.
- Participant Color: the Host assigns it at seating (`MatchSeating`), unique within the Match; Quick Play Players and every Bot get a random free colour. The palette is 8 colours in config, based on the Okabe-Ito colour-blind-safe set (vermillion, orange, yellow, bluish green, sky blue, blue, reddish purple, charcoal), the yellow darkened to read on snow; the order is a hue ring, because "nearest free" walks it. Every screen that shows a Participant (Nickname label over the Vehicle, Vehicle body, Score row, later the Podium) resolves the index through `ParticipantsConfig.ParticipantColor`.
- Custom Game picks and a future Garage preference go through the same `Assign`: pass the pick as `preferred`; to keep a not-yet-arrived Player's pick away from Bots, add the reserved picks to `taken` when seating Bots.
- Player Critters are random for now; a chosen Critter would come in the `ParticipantToken` from Meta.

## Depends on

Vehicle (`NetworkVehicle`, `VehicleWorldDriver.MaxVehicles`), Fusion, VContainer. Match, Bots and Hud depend on this module; it references none of them. The assembly is in Fusion's `AssembliesToWeave`.
