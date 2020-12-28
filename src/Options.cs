using CommandLine;

namespace Donjun
{
    // TODO: convert to pairs (w,h; min,max...)
    // TODO: export option to file instead of stdout?
    // TODO: parametrize borders

    class Options
    {
        [Option('w', "width", Default = 50, HelpText = "The width of the generated maze.")]
        public int Width { get; set; }

        [Option('h', "height", Default = 50, HelpText = "The height of the generated maze.")]
        public int Height { get; set; }

        [Option("min-room-side", Default = 6, HelpText = "The minimum width/height a room can have.")]
        public int MinRoomSide { get; set; }

        [Option("max-room-side", Default = 20, HelpText = "The maximum width/height a room can have.")]
        public int MaxRoomSide { get; set; }

        [Option("room-spacing", Default = 1, HelpText = "The distances between the rooms. Must be an odd number.")]
        public int RoomSpacing { get; set; }

        [Option("min-room-entrances", Default = 1, HelpText = "The minimum number of entrances a room can have.")]
        public int MinRoomEntrances { get; set; }

        [Option("max-room-entrances", Default = 2, HelpText = "The maximum number of entrances a room can have.")]
        public int MaxRoomEntrances { get; set; }
        
        [Option("seed", HelpText = "A specific random number generator seed.")]
        public int? Seed { get; set; }
        
        [Option("loot-chance", Default = 0.33, HelpText = "The chance for a regular room to contain loot.")]
        public double LootChance { get; set; }
        
        [Option("enemy-chance", Default  = 0.2, HelpText = "The chance for a regular room to contain enemies.")]
        public double EnemyChance { get; set; }
    }
}