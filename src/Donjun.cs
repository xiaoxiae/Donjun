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
                    var rmg = new DungeonGenerator
                    {
                        Width = options.Width,
                        Height = options.Height,
                        MinRoomSide = options.MinRoomSide,
                        MaxRoomSide = options.MaxRoomSide,
                        RoomSpacing = options.RoomSpacing,
                        MinRoomEntrances = options.MinRoomEntrances,
                        MaxRoomEntrances = options.MaxRoomEntrances,
                        EnemyChance = options.EnemyChance,
                        LootChance = options.LootChance,
                    };
                    
                    Random random = options.Seed.HasValue ? new Random(options.Seed.Value) : new Random();

                    // generate the dungeon
                    var dungeon = rmg.Generate(random);

                    // print out the dungeon, with some border offset
                    for (int y = -Constant.BorderThickness; y < dungeon.Height + Constant.BorderThickness; y++)
                    {
                        for (int x = -Constant.BorderThickness; x < dungeon.Width + Constant.BorderThickness; x++)
                            Console.Write((char) dungeon.At(x, y));
                        Console.WriteLine();
                    }
                });
        }
    }
}