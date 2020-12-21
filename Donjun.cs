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
                    var rmg = new RandomMazeGenerator
                    {
                        Width = options.Width,
                        Height = options.Height,
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