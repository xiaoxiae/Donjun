using System;
using System.Collections.Generic;
using System.IO;

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
    /// An interface for a maze.
    /// </summary>
    interface IMaze
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

    class MazePath
    {
        private HashSet<(int, int)> _pathPoints;

        private (int, int)[] fourDirections = {(0, 1), (1, 0), (-1, 0), (0, -1)};
        private (int, int)[] eightDirections = {(0, 1), (1, 0), (-1, 0), (0, -1), (-1, -1), (1, 1), (-1, 1), (1, -1)};

        /// <summary>
        /// Return True if a given point is equidistant to multiple rooms.
        ///
        /// Example: T marks points that return true, F is false, "|.," denote rooms
        /// .--.  T  .--.
        /// |  |  TF | F|
        /// '--'  T F'--'
        /// </summary>
        private bool IsEquidistantToMultipleRooms(int x, int y, RoomCollection rooms)
        {
            if (!rooms.Free(x, y)) return false;

            // use BFS to do so
            var queue = new Queue<(int, int, int)>();
            var explored = new HashSet<(int, int)>();
            queue.Enqueue((0, x, y));

            int currentDistance = 0;
            var equidistantRooms = new HashSet<Room>();
            while (queue.Count != 0)
            {
                (int d, int xc, int yc) = queue.Dequeue();
                explored.Add((xc, yc));

                foreach ((int xd, int yd) in eightDirections)
                {
                    int xn = xc + xd, yn = yc + yd;

                    if (explored.Contains((xn, yn)))
                        continue;

                    if (rooms.Free(xn, yn))
                        queue.Enqueue((d + 1, xn, yn));
                    else
                    {
                        // first non-zero distance
                        if (currentDistance == 0) currentDistance = d + 1;

                        if (d + 1 == currentDistance) equidistantRooms.Add(rooms.RoomAt(xn, yn));
                        else return equidistantRooms.Count > 1;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Connect the specified room to one or more closest points on the path.
        /// </summary>
        private void ConnectRoomToPath(Room room, int minConnections = 1, int maxConnections = 4)
        {
            // the number of connections to the path
            Random random = new Random();
            int connections = random.Next(minConnections, maxConnections);

            for (int _ = 0; _ < connections; _++)
            {
                // starting point
                int xs = random.Next(room.X, room.X + room.Width - 1);
                int ys = random.Next(room.Y, room.Y + room.Height - 1);

                var queue = new Queue<(int, int)>();
                queue.Enqueue((xs, ys));

                var explored = new Dictionary<(int, int), (int, int)> {[(xs, ys)] = (xs, ys)};

                while (queue.Count != 0)
                {
                    (int x, int y) = queue.Dequeue();

                    if (_pathPoints.Contains((x, y)))
                    {
                        int xc = x;
                        int yc = y;

                        while (!room.Contains(xc, yc))
                        {
                            _pathPoints.Add((xc, yc));
                            (xc, yc) = explored[(xc, yc)];
                        }

                        room.AddEntrance(xc, yc);
                        break;
                    }

                    foreach ((int xd, int yd) in fourDirections)
                    {
                        int xn = x + xd, yn = y + yd;

                        // skip explored ones
                        if (explored.ContainsKey((xn, yn)))
                            continue;

                        // mark backtracking
                        explored[(xn, yn)] = (x, y);
                        queue.Enqueue((xn, yn));
                    }
                }
            }
        }

        public MazePath(RoomCollection rooms)
        {
            _pathPoints = new HashSet<(int, int)>();

            // TODO: temporary, just for debug
            var writer = new StreamWriter("tmp.out");

            // add equidistant points to rooms
            const int offset = 0;
            for (int x = offset; x < rooms.Width - offset; x++)
                for (int y = offset; y < rooms.Height - offset; y++)
                    if (IsEquidistantToMultipleRooms(x, y, rooms))
                        _pathPoints.Add((x, y));

            // connect rooms to the path
            foreach (var room in rooms.Rooms) ConnectRoomToPath(room);

            for (int x = 0; x < rooms.Width; x++)
            {
                for (int y = 0; y < rooms.Height; y++)
                {
                    if (_pathPoints.Contains((x, y))) writer.Write(".");
                    else writer.Write(rooms.Free(x, y) ? " " : "#");
                }

                writer.WriteLine();
            }

            writer.Flush();
        }
    }

    /// <summary>
    /// A parametrized random maze generator.
    /// </summary>
    class RandomMazeGenerator
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
            // (1) generate rooms
            RoomCollection rooms = GenerateRoomsRecursively();

            // (2) generate the path, modifying the rooms in the process (adding entrances)
            MazePath path = new MazePath(rooms);

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