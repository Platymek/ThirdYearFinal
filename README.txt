# Pick'n'Punch

## File Structure

Scenes: Contains all scenes
- Game: Contains all scenes related the round
  - SubScenes: Contains all supporting scenes and classes
    - Actors: Contains all actors and the Actor class (the Player and Opponent scenes and classes and all supporting scenes and classes)
- Global: Contains the Global class and all supporting classes
- menus: Contains all menus scenes and classes


## How to Build

The source code can be built and run by opening the project in Godot Mono 4.2.1 and importing the project.godot file. The code can also be opened by opening the .sln file in .NET IDE but cannot be built and ran from here, but inspecting code is easier this way.

NOTE: As of my last check, Godot Mono 4.2.1 does not appear to be installed on any of the school's lab computers.

You MUST use Godot MONO 4.2.1 which is different to the normal Godot 4.2.1 as it supports .NET:

https://godotengine.org/download/archive/4.2.1-stable/