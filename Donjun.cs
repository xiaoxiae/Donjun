using CommandLine;


namespace Donjun
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    var rmg = new MazeGenerator
                    {
                        MazeWidth = options.Width,
                        MazeHeight = options.Height,
                        MinRoomSide = options.MinRoomSide,
                        MaxRoomSide = options.MaxRoomSide,
                        RoomSpacing = options.RoomSpacing
                    };

                    rmg.Generate();
                });
        }
    }
}