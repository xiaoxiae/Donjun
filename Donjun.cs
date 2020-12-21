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
                    };

                    var maze = rmg.Generate();

                    // print out the dungeon
                    int offset = 1;
                    for (int y = -offset; y < maze.Height + offset; y++)
                    {
                        for (int x = -offset; x < maze.Width + offset; x++)
                            Console.Write((char) maze.At(x, y));
                        Console.WriteLine();
                    }
                });
        }
    }
}