# Labyrinth_BaldelliAndrea

## Build
<a href="https://github.com/Baldi00/Labyrinth_BaldelliAndrea/releases/download/v1.0.0/Labyrinth_BaldelliAndrea_v1.0.0_build202307061916.zip">Download Windows Build</a>

## How to play
- Player can move one tile at a time using WASD or shoot an arrow in the four directions using IJKL
- Tunnels are traversed automatically by both player and arrows
- If player enters a teleport area it gets teleported
- If player enters a well, meets the monster, finishes the arrows or gets hit by an arrow it loses
- If player hits the monster with an arrow it wins
- You can play in a precomputed maze or generate a new one each time. You can also customize the maze parameters
- You can play with other players on the same screen by pressing the "Add Player" button (you can add as many player as you like)

## Requirements

|#| Requirement | State |
|---------------|-----|:-----:|
|1.0| Grid|🟢|
|1.1| Movement|🟢|
|2.a| Fog of war|🟢|
|2.b| Monster tile|🟢|
|2.c| Teleport tile|🟢|
|2.d| Well tile|🟢|
|3.a| Blood UI|🟢|
|3.b| Mold UI|🟢|
|3.c| Wind UI|🟢|
|4.0| Arrows|🟢|
|5.0| Tunnels|🟢|
|5.a| Tunnel fog reveal and crossing|🟢|
|5.b| Arrow crossing tunnel|🟢|
|5.c| No special tiles on tunnels|🟢|
|Extra.1| Procedural map generation (without tunnels)|🟢|
|Extra.2| Procedural map generation (with tunnels)|🟢|
|Extra.3| Online turn based pvp|🟡[^1]|
|Extra.3x| Local turn based pvp (limitless number of players)|🟢[^1]|

[^1]: Since we haven't studied how to make a multiplayer game yet, instead of making a bad implementation I preferred to make just a local shared screen multiplayer.
The architecture of the code is designed for multiplayer so it can be easily converted into an online multiplayer game.
