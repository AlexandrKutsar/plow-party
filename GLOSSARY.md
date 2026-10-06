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

**Roster** (состав участников):
The Slots of a registered Match, each held by an Account (a Player) or by a Bot, fixed by the Host at registration.
_Avoid_: Lineup, party

**Confirmed Player** (подтвердивший участник):
A Player in the Roster who confirmed with their own Auth Token before Countdown ended; only Confirmed Players vote and earn Credited Score.

**Vote** (голос):
One Confirmed Player's submitted Match Result; the backend accepts the version a strict majority of Votes agree on.
_Avoid_: Submission (as a type name), report

**Verdict** (вердикт):
The backend's final decision on a Match: accepted with its Scores, or rejected with a reason.

**Interrupted Match** (прерванный матч):
A Match that stopped before 180 s because the Host left; it counts with a lower Weight.

**Weight** (вес):
The factor a Match's Scores are multiplied by for the Tournament: 1 for a full Match, lower for an Interrupted Match.

**Credited Score** (зачётные очки):
A Confirmed Player's Score times the Match Weight, rounded down; what the Tournament ranks.

## Snow

**Vehicle** (машина, снегоуборщик):
The snowplow a Participant drives; every Participant's Vehicle has identical stats.
_Avoid_: Car, plow, truck

**Critter** (зверюшка):
The cosmetic animal riding a Vehicle; no gameplay effect.
_Avoid_: Hero, character, pet

**Critter Species** (вид зверюшки):
The animal a Critter depicts: Fox, Bear, Rabbit, Raccoon, Penguin, or Beaver; each has its own model `SM_Critter_<Species>`.
_Avoid_: Skin, character type

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

**Delivery** (сдача):
One continuous unloading of a Bucket in the Drop-Off Zone, from the tick a Vehicle with Load is inside until the Bucket is empty, the Vehicle leaves, or a Spill interrupts it; its Multiplier is locked when it starts.
_Avoid_: Unload (as a type name), deposit

**Snow-Free Area** (зона без снега):
The ground around the Drop-Off Zone where Snow never falls: no initial cover, no Regrowth, no Blizzard refill; Snow Piles may still land there.
_Avoid_: Clearing, mask (the Arena mask is Obstacles)

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

**Plowing a Pile** (пробивание кучки):
A tick in which a Vehicle's Blade takes Snow from a Snow Pile; capped per tick and slowing the Vehicle, so a Pile feels heavy.
_Avoid_: Digging, mining

**Cell** (клетка):
One square of the Snow Grid; its Depth is the unit of Snow tracking.

**Depth** (глубина снега):
How much Snow lies on a Cell, in whole steps from cleared to fully covered, with Snow Piles stacking above full.
_Avoid_: Height, amount, level (as a type name)

**Blade** (отвал ковша):
The strip in front of a Vehicle that scrapes Cells clear and feeds their Snow into the Bucket.
_Avoid_: Footprint, collector

**Regrowth** (восстановление):
The slow return of Snow onto a lowered Cell: after a delay it gains one Depth step at a fixed pace up to full, so the oldest part of a track refills first.

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

**Speed Factor** (множитель скорости):
One feature's share of a Vehicle's speed multiplier, keyed by its source (Load, Snow Pile); the multiplier is the product of all factors, so no feature overwrites another.
_Avoid_: Speed penalty (as a type name), slowdown

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

**Snowball** (снежок):
A throwable Gadget; its projectile knocks the hit Vehicle back and causes a Spill.
_Avoid_: Projectile (as the Gadget's name), ball

**Trap Pile** (куча-ловушка):
A Gadget that leaves a disguised pile behind the Vehicle, visible as a trap only to its owner.

## Meta

**Tournament** (турнир дня):
The daily competition ranking each account's best Match Score, reset at 00:00 UTC.
_Avoid_: League (reserved for the post-MVP weekly league)

**Medal** (медаль):
A cosmetic portrait frame awarded at Tournament reset.

**Tournament Day** (турнирные сутки):
One UTC calendar day of the Tournament; a Match counts for the day its result settles on (registration plus the submission window), so a day is final at midnight.
_Avoid_: Season, round

**Daily Best** (лучший результат дня):
An Account's highest Credited Score among the Matches of one Tournament Day; what the Leaderboard ranks.
_Avoid_: High score, record

**Leaderboard** (таблица турнира):
The Standings of one Tournament Day, shown as the top and "around me".
_Avoid_: Scoreboard, Match Result

**Standing** (строка таблицы):
One Account's Rank, Nickname and Daily Best on the Leaderboard.
_Avoid_: Entry, row (as a type name)

**Rank** (место):
An Account's unique position on the Leaderboard; equal Daily Bests go to whoever reached the score first.
_Avoid_: Placement (that is a Match's)

**Medal Day** (день медалей):
Yesterday's Tournament Day, final since midnight; its top three Ranks hold gold, silver and bronze Medals until the next midnight.

**Account** (аккаунт):
A person's identity on the backend, created on first guest login and owning their Nickname and Tournament results.
_Avoid_: User, profile, Player (that is a Participant in a Match)

**Nickname** (ник):
The display name of an Account shown in the HUD and the Tournament; not unique.
_Avoid_: Name, username

**Device Id** (идентификатор устройства):
The identifier a device generates once and presents to log in to its Account as a guest.
_Avoid_: Hardware id, install id

**Auth Token** (токен):
The bearer credential issued at login that a client presents to act as its Account.
_Avoid_: Session (that is the Fusion Session), API key
