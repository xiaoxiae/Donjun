using System;
using System.Collections.Generic;

namespace Donjun
{
    class RoomCollectionGenerator
    {
        public int MazeWidth { get; set; }
        public int MazeHeight { get; set; }
        public int MinRoomSide { get; set; }
        public int MaxRoomSide { get; set; }
        public int RoomSpacing { get; set; }

        /// <summary>
        /// Generate a collection of rooms, recursively.
        /// </summary>
        public RoomCollection Generate()
        {
            var rooms = new RoomCollection(MazeWidth, MazeHeight);

            // create one big room
            var starting = new Room(0, 0, MazeWidth, MazeHeight);
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
            if (current.Width < MaxRoomSide && current.Height < MaxRoomSide &&
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
                    if (current.Width > MaxRoomSide || current.Height > MaxRoomSide) continue;
                    return;
                }

                (Room a, Room b) = pair.Value;

                // if the rooms that got split are too small, return
                if (a.Width < MinRoomSide || a.Height < MinRoomSide || b.Width < MinRoomSide || b.Height < MinRoomSide)
                {
                    // if we really have to split the room, continue
                    if (current.Width > MaxRoomSide || current.Height > MaxRoomSide) continue;
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

    class PathGenerator
    {
        public RoomCollection Rooms { get; set; }
        public int MinRoomEntrances { get; set; }
        public int MaxRoomEntrances { get; set; }

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
        private bool IsEquidistantToMultipleRooms(int x, int y)
        {
            if (Rooms.IsRoom(x, y)) return false;

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

                    if (!Rooms.IsRoom(xn, yn))
                        queue.Enqueue((d + 1, xn, yn));
                    else
                    {
                        // first non-zero distance
                        if (currentDistance == 0) currentDistance = d + 1;

                        if (d + 1 == currentDistance) equidistantRooms.Add(Rooms.RoomAt(xn, yn));
                        else return equidistantRooms.Count > 1;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Connect the specified room to one or more closest points on the path, returning the connection points.
        /// </summary>
        private List<(int, int)> ConnectRoomToPath(Path path, Room room)
        {
            // the number of connections to the path
            Random random = new Random();
            int connections = random.Next(MinRoomEntrances, MaxRoomEntrances);
            var connectionPoints = new List<(int, int)>();

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

                    // backtrack when the connection is found
                    if (path.Contains(x, y))
                    {
                        int xc = x;
                        int yc = y;

                        connectionPoints.Add((xc, yc));

                        while (!room.Contains(xc, yc))
                        {
                            path.Add(xc, yc);
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

            return connectionPoints;
        }

        /// <summary>
        /// Remove dead ends using a BFS, starting from the provided coordinates.
        /// </summary>
        private void ClearDeadEnds(Path path, int xs, int ys)
        {
            var queue = new Queue<(int, int)>();
            queue.Enqueue((xs, ys));

            // TODO: some of the paths that cause cycles aren't generated properly
            // not a program-breaking bug but a little annoying

            var explored = new Dictionary<(int, int), List<(int, int)>>();
            var dangling = new Queue<(int, int)>();

            // create a DAG using BFS
            while (queue.Count != 0)
            {
                (int x, int y) = queue.Dequeue();
                explored[(x, y)] = new List<(int, int)>();

                // add neighbours
                foreach ((int xd, int yd) in fourDirections)
                {
                    int xn = x + xd, yn = y + yd;

                    // skip non-path points and those that were explored
                    if (!path.Contains(xn, yn))
                        continue;

                    // possibly connect to already explored path, if it isn't the previous one
                    if (!explored.ContainsKey((xn, yn)) || !explored[(xn, yn)].Contains((x, y)))
                        explored[(x, y)].Add((xn, yn));

                    // if it hasn't been explored yet, explore
                    if (!explored.ContainsKey((xn, yn)))
                        queue.Enqueue((xn, yn));
                }

                // if it is dangling (isn't connected to a room and no other path), remove it
                if (explored[(x, y)].Count == 0)
                {
                    bool found = false;
                    foreach ((int xd, int yd) in fourDirections)
                    {
                        if (Rooms.IsRoom(x + xd, y + yd))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        dangling.Enqueue((x, y));
                }
            }

            // remove the dangling bits by stripping them, removing them as sons
            // from other vertices and possibly adding those
            while (dangling.Count != 0)
            {
                (int x, int y) = dangling.Dequeue();
                path.Remove(x, y);

                foreach ((int xd, int yd) in fourDirections)
                {
                    int xn = x + xd, yn = y + yd;

                    // find all predecessors that contain (x, y)
                    if (explored.ContainsKey((xn, yn)))
                    {
                        // remove it
                        if (explored[(xn, yn)].Contains((x, y)))
                            explored[(xn, yn)].Remove((x, y));

                        // if this reduced the successors to 0, enqueue it
                        if (explored[(xn, yn)].Count == 0)
                        {
                            explored.Remove((xn, yn));
                            dangling.Enqueue((xn, yn));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generate the path between the rooms in the maze.
        ///
        /// The algorithm works as follows:
        /// (1) mark all points that are the same distance to more than one room to be on the path
        /// (2) connect all rooms to the path using one or more connections
        /// (3) cut off dead ends
        /// </summary>
        public Path Generate()
        {
            Path path = new Path();

            // add equidistant points to rooms
            for (int x = 0; x < Rooms.Width; x++)
                for (int y = 0; y < Rooms.Height; y++)
                    if (IsEquidistantToMultipleRooms(x, y))
                        path.Add(x, y);

            // connect rooms to the path
            // also save some point on the path that we definitely don't want to remove (is not a dead end)
            int xp = -1, yp = -1;
            foreach (var room in Rooms.Rooms)
            {
                var points = ConnectRoomToPath(path, room);

                if (xp == -1) (xp, yp) = points[0];
            }

            // clear dead ends
            ClearDeadEnds(path, xp, yp);

            return path;
        }
    }

    class RoomLayoutGenerator
    {
        public Room Room { get; set; }

        public List<List<Item>> Generate()
        {
            // TODO: actual implementation, this just create a container of the appropriate size, filled with air
            var layout = new List<List<Item>>();

            for (int i = 0; i < Room.Height; i++)
            {
                var list = new List<Item>();
                for (int j = 0; j < Room.Width; j++)
                    list.Add(Item.Air);

                layout.Add(list);
            }

            return layout;
        }
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
        /// </summary>
        public IMaze Generate()
        {
            // (1) generate rooms
            RoomCollection roomCollection = new RoomCollectionGenerator
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
                Rooms = roomCollection,
                MinRoomEntrances = MinRoomEntrances,
                MaxRoomEntrances = MaxRoomEntrances
            }.Generate();

            // (3)  generate the respective room layouts
            foreach (var room in roomCollection.Rooms)
            {
                room.SetLayout(new RoomLayoutGenerator {Room = room}.Generate());
            }

            return new Maze(roomCollection, path);
        }
    }
}