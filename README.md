# SignalRed Tanks

[SignalRed is an open source library designed to make it easier to build networked multiplayer games](https://github.com/profexorgeek/SignalRed) that use SignalR as the transport layer.

TankMp is a game that only exists to test the library. It is not intended to have a rich set of features or game design that pushes the envelope - instead it has just enough features to make sure SignalRed is really capable of powering a networked game!

## Goals

- [x] Get damage, death and respawn working
- [x] Tank respawning, counters, etc should be managed by the controller
- [ ] Add respawn zone entity to map
- [ ] Tank does not need to implement IVisible
- [ ] Bullets do not need misleading ApplyUpdate logic
- [ ] Add Host/Guest concept to either core library or game
- [ ] Add ability to clear all history from the server
- [ ] Add ability to host server as Azure instance
- [ ] Add scoring to game
- [ ] Add endgame screen
- [ ] Add tank customization
- [ ] Add level selection
