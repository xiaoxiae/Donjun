using System;
using System.IO;

namespace Donjun
{
    /// <summary>
    /// An item(-ASCII) enum for visualizing things in the dungeon.
    /// </summary>
    public enum Item
    {
        Empty = ' ',
        Wall = '#'
    } 
    
    /// <summary>
    /// An interface for a maze.
    /// </summary>
    public interface IMaze
    {
        /// <summary>
        /// Return the item at the given position of the maze.
        /// </summary>
        public Item At(int x, int y);

        /// <summary>
        /// Return the width of the maze.
        /// </summary>
        public int Width();

        /// <summary>
        /// Return the height of the maze.
        /// </summary>
        public int Height();
    }

    
    /// <summary>
    /// A parametrized random maze generator.
    /// </summary>
    public class RandomMazeGenerator
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int MinimumRoomWidth { get; set; }
        public int MinimumRoomHeight { get; set; }
        public int MaximumRoomWidth { get; set; }
        public int MaximumRoomHeight { get; set; }

        private const int RoomSpacing = 3; // minimum spacing between each adjacent rooms

        /// <summary>
        /// Generate the maze.
        ///
        /// THe algorithm works as follows:
        /// (1) generate rectangular areas where the rooms are going to be
        /// (2) connect these areas via paths and let them know where the entrances are (for generating the rooms)
        /// (3) generate unique rooms in each of the rectangular areas
        /// </summary>
        public void GenerateMaze()
        {
            // (1)
            RoomCollection rooms = GenerateRoomsRecursively();

            // (2)
            // TODO: connect the rooms, marking entrances

            // (3)
            // TODO: generate the respective rooms

            // TODO: concrete maze class implementation
        }

        /// <summary>
        /// Generate a collection of rooms, recursively.
        /// </summary>
        private RoomCollection GenerateRoomsRecursively()
        {
            var rooms = new RoomCollection(Width, Height, RoomSpacing);

            // create one big room
            var starting = new Room(0, 0, Width, Height);
            rooms.AddRoom(starting);

            // split it recursively
            RecursiveRoomSplit(starting, rooms, new Random());

            // TODO: temporary, just for debug
            var writer = new StreamWriter("tmp.out");
            writer.WriteLine(rooms.ToString());
            writer.Flush();

            return rooms;
        }

        /// <summary>
        /// Split a room in half, possibly removing it and adding the new rooms (and splitting further...).
        /// </summary>
        /// <param name="current">The room to split.</param>
        /// <param name="rooms">The collection of rooms to add to.</param>
        /// <param name="random">A random number generator.</param>
        private void RecursiveRoomSplit(Room current, RoomCollection rooms, Random random)
        {
            const double recursionChance = 0.1; // the chance for the room to not split in two if of valid size
            const double minimumSplitPortion = 0.3; // the minimum portion the smallest room can be after split

            // return only when the room is small enough and we don't want to make it any smaller
            if (current.Width < MaximumRoomWidth && current.Height < MaximumRoomHeight &&
                random.NextDouble() < recursionChance)
                return;

            while (true)
            {
                // number between minimumSplitPortion and (1 - minimumSplitPortion)
                double splitPortion = random.NextDouble() * (1 - minimumSplitPortion * 2) + minimumSplitPortion;

                (Room, Room)? pair = random.NextDouble() < 0.5
                    ? current.SplitHorizontally(splitPortion, RoomSpacing)
                    : current.SplitVertically(splitPortion, RoomSpacing);

                if (!pair.HasValue)
                {
                    // if we really have to split the room, continue
                    if (current.Width > MaximumRoomWidth || current.Height > MaximumRoomHeight) continue;
                    return;
                }

                (Room a, Room b) = pair.Value;

                // if the rooms that got split are too small, return
                if (a.Width < MinimumRoomWidth
                    || a.Height < MinimumRoomHeight
                    || b.Width < MinimumRoomWidth
                    || b.Height < MinimumRoomHeight)
                {
                    // if we really have to split the room, continue
                    if (current.Width > MaximumRoomWidth || current.Height > MaximumRoomHeight) continue;
                    return;
                }

                rooms.RemoveRoom(current);
                rooms.AddRoom(a);
                rooms.AddRoom(b);

                RecursiveRoomSplit(a, rooms, random);
                RecursiveRoomSplit(b, rooms, random);

                return;
            }
        }
    }
}