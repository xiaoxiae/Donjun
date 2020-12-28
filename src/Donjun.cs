using System;
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
                        RoomSpacing = options.RoomSpacing,
                        MinRoomEntrances = options.MinRoomEntrances,
                        MaxRoomEntrances = options.MaxRoomEntrances,
                        EnemyChance = options.EnemyChance,
                        LootChance = options.LootChance,
                    };
                    
                    Random random = options.Seed.HasValue ? new Random(options.Seed.Value) : new Random();

                    // generate the maze
                    var maze = rmg.Generate(random);

                    // print out the dungeon, with some border offset
                    for (int y = -Constant.BorderThickness; y < maze.Height + Constant.BorderThickness; y++)
                    {
                        for (int x = -Constant.BorderThickness; x < maze.Width + Constant.BorderThickness; x++)
                            Console.Write((char) maze.At(x, y));
                        Console.WriteLine();
                    }
                });
        }
    }
}