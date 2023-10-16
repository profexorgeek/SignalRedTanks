# SignalRed Tanks

**NOTE: This game is still in a very crude state. It also requires building FlatRedBall from source and either having the FlatRedBall editor installed or also building it from source. The game has a large amount of generated code, created by FlatRedBall tools, that is not checked into the repository. I will be posting a released version on Itch.io at some point in the future so it's easier to test!**

[SignalRed is an open source library designed to make it easier to build networked multiplayer games](https://github.com/profexorgeek/SignalRed) that use SignalR as the transport layer.

TankMp is a game that only exists to test the library. It is not intended to have a rich set of features or game design that pushes the envelope - instead it has just enough features to make sure SignalRed is really capable of powering a networked game. As you can see below, there are a lot of goals that are incomplete.


## Goals

- [x] Get damage, death and respawn working
- [x] Tank respawning, counters, etc should be managed by the controller
- [x] Add scoring to game
- [x] Add endgame screen
- [ ] Add respawn zone entity to map
- [ ] Add ability to clear all history from the server
- [ ] Add ability to host server as Azure instance
- [ ] Tank does not need to implement IVisible
- [x] Bullets do not need misleading ApplyUpdate logic
- [ ] Add Host/Guest concept to either core library or game
- [ ] Add tank customization
- [ ] Add level selection
