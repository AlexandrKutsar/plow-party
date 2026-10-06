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
Extra Snow lying on the ground above full cover, left by a Spill or a Blizzard, that any Participant can collect; holds no Loot and has no owner.
_Avoid_: Heap (that is the Trap Pile), Drift (that holds Loot)

**Cell** (клетка):
One square of the Snow Grid; its Depth is the unit of Snow tracking.

**Depth** (глубина снега):
How much Snow lies on a Cell, in whole steps from cleared to fully covered, with Snow Piles stacking above full.
_Avoid_: Height, amount, level (as a type name)

**Blade** (отвал ковша):
The strip in front of a Vehicle that scrapes Cells clear and feeds their Snow into the Bucket.
_Avoid_: Footprint, collector

**Regrowth** (восстановление):
The slow continuous return of Snow onto cleared cells.

**Blizzard** (метель):
A scheduled wave that rapidly re-covers the whole map with Snow and leaves a few Snow Piles in its wake.
_Avoid_: Storm, snowfall

## Collisions

**Ram** (таран):
A collision into another Vehicle's side or rear that causes a Spill.
_Avoid_: Hit, bump, crash

**Rammer** (таранящий):
The Vehicle that delivers a Ram; it bounces back by the Recoil.

**Victim** (протараненный):
The Vehicle hit by a Ram; it is pushed away by the Knockback and Spills Load.
_Avoid_: Target (reserved for targeted Gadgets)

**Knockback** (отбрасывание):
The extra push a Victim receives along the Ram direction.

**Recoil** (отдача):
The push back a Rammer receives after a Ram.

**Arena** (арена):
The static layout a Match is played on, as the Vehicle simulation sees it: a set of Obstacles.
_Avoid_: Level, map (as a code type)

**Obstacle** (препятствие):
A static shape in the Arena that Vehicles bounce off: an axis-aligned box or a circle.
_Avoid_: Wall (one kind of Obstacle), collider

**Slot** (слот участника):
A Participant's seat number in a Match, 0 to 5; fixes their Spawn Point and Ram cooldown pairing.

**Spawn Point** (точка старта):
The fixed position and facing where the Vehicle of a given Slot appears.

## Vehicle control

**Modifier** (модификатор):
A per-Vehicle adjustment other features apply to driving: speed multiplier, Immobilised, Impulse, Ram strength multiplier.
_Avoid_: Buff, effect (as a type name)

**Immobilised** (обездвижен):
A Vehicle state where it ignores the stick and stays in place, used by Freeze and Countdown.
_Avoid_: Stunned, frozen (Freeze is the Gadget)

**Impulse** (импульс):
A one-tick velocity change applied to a Vehicle, such as a snowball knockback or a Turbo Rocket dash.

## Loot and Gadgets

**Drift** (сугроб):
A large breakable snow mound that drops Loot when a Vehicle drives into it.
_Avoid_: Snowbank, crate, chest, loot pile

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
