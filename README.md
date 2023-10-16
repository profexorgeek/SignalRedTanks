# SignalRed Tanks

**NOTE: This game is still in a very crude state. It also requires building [FlatRedBall](https://github.com/vchelaru/FlatRedBall) from source and either having the FlatRedBall editor installed or also building it from source. The game has a large amount of generated code, created by FlatRedBall tools, that is not checked into the repository. I will be posting a released version on Itch.io at some point in the future so it's easier to test!**

[SignalRed is an open source library designed to make it easier to build networked multiplayer games](https://github.com/profexorgeek/SignalRed) that use SignalR as the transport layer.

TankMp (the actual namespace for SignalRed Tanks) is a game that only exists to test the library. It is not intended to have a rich set of features or game design that pushes the envelope - instead it has just enough features to make sure SignalRed is really capable of powering a networked game. As you can see below, there are a lot of goals that are incomplete.

## Concepts

TankMp has a service class called `TankMp.Services.GameStateService`. This class holds models for the currently connect players, their state (connected, ready, etc), and their stats such as kills and deaths. These models are MVVM bound to UI and are the core place where state persists across game screens. The `PlayerStatusViewModel` is a "pseudo entity" in that it isn't directly represented in the game as a game engine object. Both the pseudo-entity/viewmodel and it's network state are in the **Models/PlayerStatusModels.cs** file.

TankMp has these major game screens in the **Screens** folder:

- `NetworkedScreenBase` - base class for networked screens, has retrieval and management of network messages
- `ConnectToServer` - allows player to input a username and server (including port) to connect to
- `ServerLobby` - hosts chat and shows ping and ready state for connected players
- `GameScreen` - base class for gameplay logic
  - `Level1` - derived `GameScreen` with a specific TMX level
 
TankMp has these networked entities in the **Entities** folder:

- `TankBase` - the base class for tanks, manages interpolation in tank state and reacts to user input
- `BulletBase` - the base class for bullets, manages interpolation in bullet state

For each entity, there is a network transfer state in the **Models** folder:

- `TankNetworkState` - the state of both the tank and its input controller
- `BulletNetworkState` - the state of the bullet

### ConnectToServer

This is pretty self-explanatory. It just forms a connection and then moves to the lobby.

### NetworkedScreenBase

This is a good place to look for basic networking code. This screen retrieves messages and passes them to `virtual` methods that the game screen can override.

### ServerLobby

The `ServerLobby` screen is an instance of `NetworkedScreenBase` and manages the joining of players. The only entity type it exchanges across the network is the `PlayerStatusNetworkState`, which keeps track of player join status, username, and score.

The `ServerLobby` screen uses `GenericMessage`s to exchange chats. In a real game, this should probably be another entity type but this was a simple way to show how the `GenericMessage` type can be used!

### GameScreen

This is where the heavy lifting happens. Ignoring engine-specific practices and game logic, the general pattern is that the screen manages a collection of `Tank` entities and `Bullet` entities. Each of these entities also has a network state object in the `Models` folder of the project, which represents its state in the game.

It's a _design choice_ in this game to have each client be the authority over anything that deals with its tank instance. This pattern makes cheating very easy and is not recommended but it's easier to understand than other authoritative patterns.

When the local client is ready to spawn a tank, it sends a tank spawn request. When that request bounces back, the game screen spawns a tank and, since the tank owner matches the local entity owner, assigns that to a `localTank` instance for tracking. Likewise, when a bullet is fired a request is made to the server and the bullet is created when the bounceback message arrives. In most cases, things that affect the game state make a server roundtrip before the local client takes action. Meaning, it first requests an event such as a bullet creation, and then responds when it receives the server message back. This is a simpler pattern to write and explain.

One major difference between `Tank`s and `Bullet`s is that the `Tank` entity sends regular update messages with its current state across the network. This allows all clients to interpolate their local state to the server state with some physics projection based on the time elasped since the message is sent. This is important because the tank position is constantly changing in reaction to user input and so regular updates must be sent. However, bullets do _not_ frequently change state. Once a bullet is fired, it typically travels in a deterministic way until it's life counter expires or it impacts a tank. This means that the instance can fully simulate bullets locally. If a bullet hits the locally-controlled tank, it is destroyed and the local tank's health is deducted. If a bullet hits a non-controlled tank, the bullet is destroyed but no change is made to the non-controlled tank's health. It is assumed that the client controlling that tank will broadcast the update to the tank's health in its own simulation since it is the authority over its own tank.

This means that the behavior of bullets can vary slightly across clients. This is generally okay as long as the game state is not significantly different across clients. It's possible that a bullet on client A may hit a tank and be destroyed, but on client B the bullet may miss the tank. This only matters to the local player if it affects their health and so the game design allows this discrepancy between clients in the interest of smooth gameplay. If bullets were forced to be in identical state across all clients, it would cause significant jitter and interpolation as bullets slightly shifted their state. This is especially true when bullets are bouncing in a corner.

To reiterate, this makes network programming much easier to demonstrate, and makes simulation look smoother across clients. However, it makes it extremely easy to cheat since things like taking damage, reload speed, respawn time, and other critical game concepts are not enforced by the server or by clients.


## Goals

- [x] Get damage, death and respawn working
- [x] Tank respawning, counters, etc should be managed by the controller
- [x] Add scoring to game
- [x] Add endgame screen
- [ ] Add respawn zone entity to map
- [ ] Add ability to clear all history from the server
- [ ] Add ability to host server as Azure instance
- [ ] When the server is in the cloud, what happens when people try to join when a game is in progress?
- [ ] Tank does not need to implement IVisible
- [x] Bullets do not need misleading ApplyUpdate logic
- [ ] Add Host/Guest concept to either core library or game
- [ ] Add tank customization
- [ ] Add level selection

