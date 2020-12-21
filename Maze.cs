using System;

namespace Donjun
{
    /// <summary>
    /// An item(-ASCII) enum for visualizing things in the dungeon.
    /// </summary>
    enum Item
    {
        Empty = ' ',
        Wall = '#'
    }

    /// <summary>
    /// A parametrized random maze generator.
    /// </summary>
    class MazeGenerator
    {
        public int MazeWidth { get; set; }
        public int MazeHeight { get; set; }
        public int MinRoomSide { get; set; }
        public int MaxRoomSide { get; set; }
        public int RoomSpacing { get; set; }
        public int MinRoomEntrances { get; set; }
        public int MaxRoomEntrances { get; set; }

        /// <summary>
        /// Generate the maze.
        ///
        /// The algorithm works as follows:
        /// (1) generate rectangular areas where the rooms are going to be
        /// (2) connect these areas via paths and let them know where the entrances are (for generating the rooms)
        /// (3) generate unique rooms in each of the rectangular areas
        /// TODO: returns IMaze
        /// </summary>
        public void Generate()
        {
            // (1) generate rooms
            RoomCollection rooms = new RoomCollectionGenerator
            {
                MazeWidth = MazeWidth,
                MazeHeight = MazeHeight,
                MinRoomSide = MinRoomSide,
                MaxRoomSide = MaxRoomSide,
                RoomSpacing = RoomSpacing
            }.Generate();

            // (2) generate the path, modifying the rooms in the process (adding entrances)
            Path path = new PathGenerator
            {
                Rooms = rooms,
                MinRoomEntrances = MinRoomEntrances,
                MaxRoomEntrances = MaxRoomEntrances
            }.Generate();

            // (3)  generate the respective rooms
            // TODO: generate the respective rooms

            // TODO: concrete maze class implementation
        }
    }
}