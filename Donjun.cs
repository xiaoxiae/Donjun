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
                        MinimumRoomWidth = options.MinimumRoomWidth,
                        MinimumRoomHeight = options.MinimumRoomHeight,
                        MaximumRoomWidth = options.MaximumRoomWidth,
                        MaximumRoomHeight = options.MaximumRoomHeight
                    };

                    rmg.GenerateMaze();
                });
        }
    }
}