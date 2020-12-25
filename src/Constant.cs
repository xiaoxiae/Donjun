using System.Collections.Generic;

namespace Donjun
{
    /// <summary>
    /// A class for various algorithm-related constants that aren't meant to be tweaked by the user.
    /// </summary>
    public class Constant
    {
        // GENERAL
        public static readonly (int, int)[] ManhattanDeltas = {(0, 1), (1, 0), (-1, 0), (0, -1)};

        public static readonly (int, int)[] DiagonalDeltas =
            {(0, 1), (1, 0), (-1, 0), (0, -1), (-1, -1), (1, 1), (-1, 1), (1, -1)};

        public const int BorderThickness = 1; // thickness of the wall around the maze

        // ROOM COLLECTION GENERATION
        public const double RoomSplitChance = 0.9; // the chance for a room to not split in half if of valid size
        public const double RoomSplitPortion = 0.3; // the minimum portion the smallest room can be after split

        // ROOM LAYOUT GENERATION
        // the chances of various room layouts to appear
        // the value means the chance of the layout happening is value/(sum of all values)
        public static readonly SortedDictionary<RoomLayoutGenerator.Layout, int> RoomLayoutChance =
            new SortedDictionary<RoomLayoutGenerator.Layout, int>
            {
                {RoomLayoutGenerator.Layout.Regular, 15},
                {RoomLayoutGenerator.Layout.Columns, 5},
                {RoomLayoutGenerator.Layout.Lake, 2},
                {RoomLayoutGenerator.Layout.Filled, 1},
            };
        
        // offset of lake/columns/fill from the corner of a room
        public const int ColumnOffset = 2;
        public const int LakeOffset = 2; 
        public const int FillOffset = 1;
        
        // a chance to not add one of the columns (so it looks more realistic)
        public const double OmitColumnChance = 0.2;
        
        // how large of an area to 'excavate' after entrances in filled rooms
        public const double OmitDirectionStepChance = 0.1;  // the chance to not expand the BFS in the given direction
        public const int MinFillSteps = 2;
        public const int MaxFillSteps = 5;
    }
}