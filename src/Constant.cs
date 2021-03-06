using System.Collections.Generic;

namespace Donjun
{
    /// <summary>
    /// A class for various algorithm-related constants that aren't meant to be tweaked by the user.
    /// </summary>
    public static class Constant
    {
        public static readonly (int, int)[] ManhattanDeltas = {(0, 1), (1, 0), (-1, 0), (0, -1)};

        public static readonly (int, int)[] DiagonalDeltas =
            {(0, 1), (1, 0), (-1, 0), (0, -1), (-1, -1), (1, 1), (-1, 1), (1, -1)};

        public const int BorderThickness = 1; // thickness of the wall around the dungeon

        // ROOM COLLECTION GENERATION
        public const double RoomSplitChance = 0.9; // the chance for a room to not split in half if of valid size
        public const double RoomSplitPortion = 0.3; // the minimum portion the smallest room can be after split

        // ROOM LAYOUT GENERATION
        // the chances of various room layouts to appear
        // the value means the chance of the layout happening is value/(sum of all values)
        public static readonly SortedDictionary<RoomLayoutGenerator.LayoutType, int> RoomLayoutChance =
            new SortedDictionary<RoomLayoutGenerator.LayoutType, int>
            {
                {RoomLayoutGenerator.LayoutType.Regular, 15},
                {RoomLayoutGenerator.LayoutType.Columns, 5},
                {RoomLayoutGenerator.LayoutType.Lake, 2},
                {RoomLayoutGenerator.LayoutType.Filled, 2},
            };
        
        // offset of lake/columns/fill from the corner of a room (walls included)
        public const int ColumnOffset = 2;
        public const int LakeOffset = 2; 
        public const int FillOffset = 1;
        
        // a chance to not add one of the columns (so it looks more realistic)
        public const double OmitColumnChance = 0.2;
        
        // the chance to not expand the BFS in the given direction (raised to the power of the number of steps)
        public const double DirectionStepChance = 0.85;
        
        // the chance to add corners to all possible places in the room
        public const double RoomCornersChance = 0.8;
        
    }
}