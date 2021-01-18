# Programmer's Documentation
This document serves to shed light onto the inner workings and decisions made in creating this project, resources used, and possible future developments.

## Algorithm overview

The dungeon generating algorithm works as follows:
1. generate rectangular areas where the rooms are going to be
	- done recursively by splitting bigger room into smaller ones, horizontally/vertically
2. connect these areas via paths and let them know where the entrances are (for generating the rooms)
	1. mark tiles that are equidistant from multiple rooms as "on the path"
	2. cut of "dead ends" by orienting the graph and removing vertices with deg_out = 0 (under some conditions)
3. generate unique rooms in each of the rectangular areas
	- for each room, choose from one of the pre-defined layouts and possibly make it a loot/enemies room

## Project Structure
Here is the purpose of each of the files of the project:

- `Constant.cs` - contains constants used throughout the project that aren't meant to be user-tweaked
- `Donjun.cs` - glues the CLI and the logic together - parses options, calls the generator and produces the output
- `Generator.cs` - contains all of the generators for various components of the dungeon
- `Interface.cs` - contains interfaces that are used throughout the project
	- `IBoundable` - inherited by things that have a width and a height
	- `IAttable` - inherited by things for which it makes sense to ask "what item is at these coordinates"
	- `IDungeon` - a combination of `IBoundable` and `IAttable`, since that's all we need for a dungeon object
- `Dungeon.cs` - contains the implementation of the Dungeon class
- `Options.cs` - contains the options that the CLI uses
- `Path.cs` - contains the implementation of the Path class
- `Room.cs` - contains the implementation of the Room class

## Future development
I'm pretty happy with the current state of the project, so unless there is interest in further development from the users, it will stay this way (modulo bugfixes).

## Resources
- [CommandLine](https://github.com/commandlineparser/commandline) - the command line parser I used in the project.

