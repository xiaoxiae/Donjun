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

        [Option("min-room-side", Default = 5, HelpText = "The minimum width/height a room can have.")]
        public int MinRoomSide { get; set; }

        [Option("max-room-side", Default = 20, HelpText = "The maximum width/height a room can have.")]
        public int MaxRoomSide { get; set; }

        [Option("room-spacing", Default = 3, HelpText = "The distances between the rooms. Must be an odd number.")]
        public int RoomSpacing { get; set; }

        [Option("min-room-entrances", Default = 1, HelpText = "The minimum number of entrances a room can have.")]
        public int MinRoomEntrances { get; set; }

        [Option("max-room-entrances", Default = 3, HelpText = "The maximum number of entrances a room can have.")]
        public int MaxRoomEntrances { get; set; }
    }
}