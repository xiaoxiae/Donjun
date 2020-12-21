using CommandLine;

namespace Donjun
{
    class Options
    {
        [Option('w', "width", Default = 50, HelpText = "The width of the generated maze.")]
        public int Width { get; set; }

        [Option('h', "height", Default = 50, HelpText = "The height of the generated maze.")]
        public int Height { get; set; }

        [Option("min-room-width", Default = 5, HelpText = "The minimum width a room can have.")]
        public int MinimumRoomWidth { get; set; }

        [Option("min-room-height", Default = 5, HelpText = "The minimum height a room can have.")]
        public int MinimumRoomHeight { get; set; }

        [Option("max-room-width", Default = 20, HelpText = "The maximum width a room can have.")]
        public int MaximumRoomWidth { get; set; }

        [Option("max-room-height", Default = 20, HelpText = "The maximum height a room can have.")]
        public int MaximumRoomHeight { get; set; }
    }
}