# CapsuleWars2 — Playable Race Roster (canonical catalog)

> **Pipeline role:** This is the single source of truth for what races exist. Generation runs
> draw **one category at a time** (see the batch file for the active category). Most races are
> NOT separate from-scratch generations — they cluster into **base-families** (one shared
> grayscale geometry) with **variants** riding on top. Per `GENERATION_CONTRACT.md`, a variant is:
> - **Tint variant** — same geometry, color only → a `TintPreset` (primary/secondary/accent slots).
> - **Pattern variant** — same geometry, surface markings → preset + a grayscale region mask
>   (the marking is the secondary region).
> - **Geometry-delta variant** — reuses base, one part differs in form → regenerate only that part.
>
> This is what makes 213 races tractable: e.g. all 9 Big Cats ≈ 1 felid base + variants.

**Total: 213 races** across 18 categories. Every D&D playable race is represented; Wizards of the Coast "Product Identity" names are swapped for trademark-safe equivalents (the D&D source is noted so the theming is still covered).

### Naming approach
- **Free to use:** generic folklore/mythology terms (elf, dwarf, halfling, gnome, orc, goblin, centaur, minotaur, satyr, harpy, etc.) and descriptive animal compounds (anything ending in *-folk* / *-kin*). These are public-domain or purely descriptive.
- **Renamed:** WotC Product Identity terms (tiefling, aasimar, genasi, drow, githyanki, warforged, beholder, mind flayer, etc.). The safe name is primary; the D&D source is in the Notes column.
- **Avoided entirely:** anything Tolkien-trademarked (hobbit, ent, balrog). Note that "halfling" is the generic D&D substitute for "hobbit" precisely because it's safe.
- ⚠️ *Not legal advice — for a few borderline terms (e.g. "Drakkin", "Onikin") you may want a quick clearance pass before commercial release, but the vast majority below are squarely safe.*

---

## 1. D&D Playable Races

These cover PHB, Volo's, Mordenkainen's, Tasha's, Eberron, Theros, Ravnica, and Spelljammer playable options.

| # | Safe Name | D&D Source / Notes |
|---|-----------|--------------------|
| 1 | Human | Human |
| 2 | High Elf | Elf (High) |
| 3 | Wood Elf | Elf (Wood) |
| 4 | Duskelf | Dark Elf / *Drow* (renamed) |
| 5 | Seasonelf | *Eladrin* (renamed) |
| 6 | Tideelf | Sea Elf |
| 7 | Shadowelf | *Shadar-kai* (renamed) |
| 8 | Mountain Dwarf | Dwarf (Mountain) |
| 9 | Hill Dwarf | Dwarf (Hill) |
| 10 | Graydwarf | *Duergar* (renamed) |
| 11 | Lightfoot Halfling | Halfling (Lightfoot) |
| 12 | Stout Halfling | Halfling (Stout) |
| 13 | Forest Gnome | Gnome (Forest) |
| 14 | Rock Gnome | Gnome (Rock) |
| 15 | Deepgnome | *Svirfneblin* (renamed) |
| 16 | Half-Elf | Half-Elf |
| 17 | Half-Orc | Half-Orc |
| 18 | Orc | Orc |
| 19 | Goblin | Goblin |
| 20 | Hobgoblin | Hobgoblin |
| 21 | Bugbear | Bugbear |
| 22 | Kobold | Kobold |
| 23 | Hellborn | *Tiefling* (renamed) |
| 24 | Drakkin | *Dragonborn* (renamed) |
| 25 | Celestine | *Aasimar* (renamed) |
| 26 | Emberkin | *Genasi (Fire)* (renamed) |
| 27 | Tidekin | *Genasi (Water)* (renamed) |
| 28 | Galekin | *Genasi (Air)* (renamed) |
| 29 | Stonekin | *Genasi (Earth)* (renamed) |
| 30 | Goliath | Goliath |
| 31 | Firbolg | Firbolg (Irish myth — safe) |
| 32 | Catfolk | *Tabaxi* (renamed) |
| 33 | Lionfolk | *Leonin* (renamed) |
| 34 | Shellback | *Tortle* (renamed) |
| 35 | Crowkin | *Kenku* (renamed) |
| 36 | Lizardfolk | Lizardfolk |
| 37 | Serpentfolk | *Yuan-ti* (renamed) |
| 38 | Triton | Triton (Greek myth — safe) |
| 39 | Skyfolk | *Aarakocra* (renamed) |
| 40 | Owlfolk | *Owlin* (renamed) |
| 41 | Automaton | *Warforged* (renamed) |
| 42 | Changeling | Changeling |
| 43 | Shifter | Shifter |
| 44 | Dreamkin | *Kalashtar* (renamed) |
| 45 | Centaur | Centaur |
| 46 | Minotaur | Minotaur |
| 47 | Satyr | Satyr |
| 48 | Elephantkin | *Loxodon* (renamed) |
| 49 | Bluefolk | *Vedalken* (renamed) |
| 50 | Splicer | *Simic Hybrid* (renamed) |
| 51 | Astralborn | *Githyanki* (renamed) |
| 52 | Voidmonk | *Githzerai* (renamed) |
| 53 | Rabbitfolk | *Harengon* (renamed) |
| 54 | Fairy | Fairy |
| 55 | Oozefolk | *Plasmoid* (renamed) |
| 56 | Mantisfolk | *Thri-kreen* (renamed) |
| 57 | Glidefolk | *Hadozee* (renamed) |
| 58 | Clockwork | *Autognome* (renamed) |
| 59 | Hippofolk | *Giff* (renamed) |
| 60 | Hyenafolk | *Gnoll* (renamed) |
| 61 | Poisonfrog Folk | *Grung* (renamed) |
| 62 | Fishfolk | *Locathah* (renamed) |
| 63 | Frogfolk | *Bullywug* (renamed) |

## 2. Beastfolk — Big Cats   ← active category (felid base)

| # | Name | Notes |
|---|------|-------|
| 64 | Tigerfolk | striped apex predator |
| 65 | Leopardfolk | spotted |
| 66 | Jaguarfolk | heavy-jawed |
| 67 | Pantherfolk | melanistic / black |
| 68 | Cheetahfolk | speed archetype |
| 69 | Pumafolk | a.k.a. cougar/mountain lion |
| 70 | Lynxfolk | tufted ears |
| 71 | Snowcatfolk | snow leopard |
| 72 | Sabertoothfolk | prehistoric |

## 3. Beastfolk — Bears

| # | Name | Notes |
|---|------|-------|
| 73 | Grizzlyfolk | grizzly/brown |
| 74 | Polarbearfolk | arctic |
| 75 | Blackbearfolk | smaller, agile |
| 76 | Pandafolk | bamboo / monk flavor |
| 77 | Sunbearfolk | small, climber |
| 78 | Cavebearfolk | prehistoric tank |

## 4. Beastfolk — Canines & Hyenas

| # | Name | Notes |
|---|------|-------|
| 79 | Wolffolk | pack archetype |
| 80 | Direwolffolk | larger prehistoric variant |
| 81 | Foxfolk | trickster/agile |
| 82 | Jackalfolk | scavenger |
| 83 | Coyotefolk | lean opportunist |
| 84 | Houndfolk | loyal/tracker |
| 85 | Wolverinefolk | small but ferocious (mustelid, grouped here) |

## 5. Beastfolk — Primates

| # | Name | Notes |
|---|------|-------|
| 86 | Gorillafolk | heavy bruiser |
| 87 | Chimpfolk | clever/aggressive |
| 88 | Orangutanfolk | strong, deliberate |
| 89 | Baboonfolk | troop fighter |
| 90 | Gibbonfolk | agile swinger |
| 91 | Lemurfolk | nimble, big-eyed |

## 6. Beastfolk — Hoofed & Large Herbivores

| # | Name | Notes |
|---|------|-------|
| 92 | Bullfolk | (distinct from Minotaur — fully upright) |
| 93 | Bisonfolk | shaggy charger |
| 94 | Goatfolk | (distinct from Satyr — full goat) |
| 95 | Ramfolk | horned charger |
| 96 | Stagfolk | antlered deer |
| 97 | Moosefolk | massive antlers |
| 98 | Boarfolk | tusked brawler |
| 99 | Rhinofolk | armored tank |
| 100 | Camelfolk | desert endurance |
| 101 | Giraffefolk | tall reach |
| 102 | Zebrafolk | striped runner |
| 103 | Horsefolk | (upright, distinct from Centaur) |

## 7. Beastfolk — Rodents & Small Mammals

| # | Name | Notes |
|---|------|-------|
| 104 | Ratfolk | swarm/plague flavor |
| 105 | Mousefolk | tiny, fast |
| 106 | Squirrelfolk | acrobatic |
| 107 | Beaverfolk | builder |
| 108 | Hedgehogfolk | defensive spines |
| 109 | Molefolk | burrower/blind sense |
| 110 | Badgerfolk | tenacious digger |
| 111 | Porcupinefolk | ranged quills |
| 112 | Skunkfolk | area-denial flavor |
| 113 | Raccoonfolk | thief/utility |
| 114 | Ferretfolk | sleek/agile |

## 8. Beastfolk — Other Mammals

| # | Name | Notes |
|---|------|-------|
| 115 | Batfolk | flight / echolocation |
| 116 | Otterfolk | aquatic-agile |
| 117 | Sealfolk | coastal |
| 118 | Walrusfolk | tusked tank |
| 119 | Pangolinfolk | scaled armor |
| 120 | Armadillofolk | roll/shell |
| 121 | Slothfolk | slow but durable |
| 122 | Mammothfolk | prehistoric heavy |
| 123 | Yetifolk | sasquatch/snow brute |

## 9. Reptiles & Amphibians

| # | Name | Notes |
|---|------|-------|
| 124 | Geckofolk | wall-climber |
| 125 | Chameleonfolk | stealth/color-shift |
| 126 | Komodofolk | venom-bite heavy |
| 127 | Cobrafolk | venom-spitter |
| 128 | Crocodilefolk | water ambush |
| 129 | Toadfolk | bulky, distinct from Frogfolk |
| 130 | Salamanderfolk | fire-tinged amphibian |
| 131 | Axolotlfolk | regeneration flavor |
| 132 | Raptorfolk | dino — agile pack |
| 133 | Tyrantfolk | dino — T-Rex bruiser |
| 134 | Hornfacefolk | dino — Triceratops tank |
| 135 | Drakefolk | lesser wingless dragon |
| 136 | Wyrmkin | serpentine dragon |

## 10. Birds

| # | Name | Notes |
|---|------|-------|
| 137 | Eaglefolk | aerial striker |
| 138 | Hawkfolk | falconer/precision |
| 139 | Ravenfolk | distinct from Crowkin — ominous |
| 140 | Vulturefolk | scavenger/death flavor |
| 141 | Penguinfolk | cold, aquatic |
| 142 | Roosterfolk | aggressive duelist |
| 143 | Peacockfolk | display/buff flavor |
| 144 | Phoenixkin | rebirth/fire (mythic) |

## 11. Aquatic & Sea Life

| # | Name | Notes |
|---|------|-------|
| 145 | Merfolk | classic |
| 146 | Sharkfolk | apex aggressor |
| 147 | Octopusfolk | multi-limb caster flavor |
| 148 | Crabfolk | armored pincers |
| 149 | Jellyfolk | floating, stinging |
| 150 | Anglerfolk | deep-sea lure |

## 12. Insects & Arthropods

| # | Name | Notes |
|---|------|-------|
| 151 | Antfolk | swarm/colony |
| 152 | Beefolk | sting/hive buffs |
| 153 | Spiderfolk | web/ambush |
| 154 | Scorpionfolk | venom-tail |
| 155 | Mothfolk | distinct from Mantisfolk — dusty/night |
| 156 | Beetlefolk | armored scarab |

## 13. Undead

| # | Name | Notes |
|---|------|-------|
| 157 | Skeleton | classic |
| 158 | Zombie | shambling |
| 159 | Ghoul | fast, hungering |
| 160 | Wight | armored undead warrior |
| 161 | Revenant | vengeance-driven, hard to kill |
| 162 | Wraith | incorporeal drainer |
| 163 | Phantom | ghostly, evasive |
| 164 | Bonelord | skeletal commander/summoner |
| 165 | Mummykin | curse/wrap flavor |
| 166 | Banshee | wail / sonic |
| 167 | Vampirekin | lifesteal archetype |
| 168 | Lichborn | undead spellcaster |

## 14. Constructs & Mechanical

| # | Name | Notes |
|---|------|-------|
| 169 | Stonewalker | stone golem — slow heavy tank |
| 170 | Hollowplate | animated armor — empty suit |
| 171 | Strawman | scarecrow — flammable, fear flavor |
| 172 | Gargoyle | stone flyer, sleep-to-stone |
| 173 | Cogling | tiny clockwork — distinct from Clockwork (#58) |

## 15. Elementals & Planar

| # | Name | Notes |
|---|------|-------|
| 174 | Frostborn | ice/freeze |
| 175 | Stormkin | air/lightning |
| 176 | Lavakin | magma/burn |
| 177 | Crystalkin | gem/reflective armor |
| 178 | Sandkin | desert/erosion |
| 179 | Shadowborn | umbral/stealth |
| 180 | Lightborn | radiant/blind |
| 181 | Voidkin | null/anti-magic |
| 182 | Djinnkin | wish/wind genie (*Genie* renamed) |
| 183 | Demonkin | chaotic fiend |
| 184 | Devilkin | lawful fiend |
| 185 | Angelkin | celestial warrior |

## 16. Plants & Fungi

| # | Name | Notes |
|---|------|-------|
| 186 | Barkfolk | treant/ent-type (*Treant* renamed) |
| 187 | Fungalfolk | mushroom colony (*Myconid* renamed) |
| 188 | Cactusfolk | spines/desert |
| 189 | Bloomkin | flower/pollen buffs |
| 190 | Vinefolk | grapple/entangle |
| 191 | Mossfolk | regen/damp |
| 192 | Sproutling | tiny seedling, grows over time |
| 193 | Mandrakekin | scream/uproot flavor |

## 17. Mythic Monsters

| # | Name | Notes |
|---|------|-------|
| 194 | Ogre | brute melee |
| 195 | Troll | regenerating brute |
| 196 | Cyclops | single-eye heavy hitter |
| 197 | Gorgon | metal bull (D&D sense — distinct from Medusakin) |
| 198 | Harpy | winged, luring song |
| 199 | Medusakin | snake-hair, petrify gaze |
| 200 | Sphinxkin | riddle/arcane |
| 201 | Gryphonkin | eagle-lion hybrid flyer |
| 202 | Pegasuskin | winged horse |
| 203 | Unicornkin | horn/healing |
| 204 | Krakenkin | tentacled sea horror |
| 205 | Manticorekin | spike-tail flyer |
| 206 | Wendigokin | gaunt cold predator (distinct from Yetifolk) |
| 207 | Onikin | horned ogre-demon |
| 208 | Yokaikin | shapeshifting spirit |

## 18. Exotic Aberrations (PI-equivalent, renamed)

| # | Name | Notes |
|---|------|-------|
| 209 | Eyekin | floating eye-tyrant (*Beholder* renamed) |
| 210 | Cerebrid | psionic brain-eater (*Mind Flayer/Illithid* renamed) |
| 211 | Toadwarrior | fish/frog deep-dweller (*Kuo-toa* renamed) |
| 212 | Maulbeast | hulking ambush brute (*Bugbear-adjacent*, distinct from #21) |
| 213 | Hulkfolk | burrowing armored aberration (*Umber Hulk* renamed) |
