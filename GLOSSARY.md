# Plow Party

Domain language of the game. The bold term is the code name; the Russian word in parentheses is the GDD term it maps to.

## Match

**Match** (матч):
One 180-second free-for-all round between 4–6 Participants, from Countdown to Results.
_Avoid_: Game, round, level

**Participant** (участник):
A seat in a Match, held by either a Player or a Bot.
_Avoid_: Racer, competitor

**Player** (игрок):
A Participant controlled by a human on a device.
_Avoid_: User, client

**Bot** (бот):
A Participant controlled by host-side AI, indistinguishable from a Player in the HUD.
_Avoid_: AI player, NPC

**Host** (хост):
The Player whose device runs the authoritative Fusion simulation for the Match.
_Avoid_: Server, master

**Session** (сессия):
The Fusion network session a Match runs in; created by Quick Play or a Room Code.
_Avoid_: Room, lobby (as the network concept)

**Room Code** (код комнаты):
A 5-character code a Host shares so friends join the same Session.

**Match Result** (итоговая таблица):
The final placement and Score of every Participant, submitted to the backend.
_Avoid_: Scoreboard, leaderboard (that is the Tournament's)

## Snow

**Vehicle** (машина, снегоуборщик):
The snowplow a Participant drives; every Participant's Vehicle has identical stats.
_Avoid_: Car, plow, truck

**Critter** (зверюшка):
The cosmetic animal riding a Vehicle; no gameplay effect.
_Avoid_: Hero, character, pet

**Snow Grid** (сетка снега):
The map-wide grid of cells tracking how much Snow lies on the ground.

**Bucket** (ковш):
The Vehicle's snow container; its Load fills while driving over Snow and empties at the Drop-Off Zone.
_Avoid_: Scoop, container, inventory

**Load** (груз):
The amount of Snow currently in a Bucket, 0 to capacity.
_Avoid_: Cargo, fill

**Drop-Off Zone** (зона сдачи, снегоплавилка):
The area where a Vehicle unloads its Bucket to earn Score.
_Avoid_: Base, melter, deposit zone

**Multiplier** (множитель):
The Score factor fixed by the Load at the moment a Vehicle enters the Drop-Off Zone.

**Score** (очки):
Points a Participant earns only by unloading at the Drop-Off Zone.
_Avoid_: Points (as a type name)

**Spill** (высыпание):
Losing a share of Load when rammed or hit, which leaves a Snow Pile.

**Snow Pile** (кучка):
Spilled Snow lying on the ground that any Participant can collect.
_Avoid_: Heap (that is the Trap Pile)

**Regrowth** (восстановление):
The slow continuous return of Snow onto cleared cells.

**Blizzard** (метель):
A scheduled wave that rapidly re-covers the whole map with Snow.
_Avoid_: Storm, snowfall

## Collisions

**Ram** (таран):
A collision into another Vehicle's side or rear that causes a Spill.
_Avoid_: Hit, bump, crash

## Loot and Gadgets

**Drift** (сугроб):
A large breakable snow mound that drops Loot when a Vehicle drives into it.
_Avoid_: Snowbank, crate, chest

**Loot** (лут):
An item dropped from a Drift, owned by the Participant who broke it and collectable only by them.
_Avoid_: Pickup, drop, reward

**Gadget** (гаджет):
A single-use ability held in the Vehicle's one Gadget slot.
_Avoid_: Power-up, item, weapon

**Immunity** (иммунитет):
A short window after any enemy effect during which a Participant ignores new enemy effects.

**Trap Pile** (куча-ловушка):
A Gadget that leaves a disguised pile behind the Vehicle, visible as a trap only to its owner.

## Meta

**Tournament** (турнир дня):
The daily competition ranking each account's best Match Score, reset at 00:00 UTC.
_Avoid_: League (reserved for the post-MVP weekly league)

**Medal** (медаль):
A cosmetic portrait frame awarded at Tournament reset.
