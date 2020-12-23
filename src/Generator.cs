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

                foreach ((int xd, int yd) in Constant.DiagonalDeltas)
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

                        room.Entrances.Add((xc, yc));
                        break;
                    }

                    foreach ((int xd, int yd) in Constant.ManhattanDeltas)
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
            // TODO: re-write, the algorithm was broken
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
        enum Layout
        {
            Regular,
            // ########
            // #      #
            // #      #
            // #      #
            // #      #
            // #      #
            // ########

            Lake,
            // ########
            // #      #
            // # ~~~~ #
            // # ~~~~ #
            // # ~~~~ #
            // #      #
            // ########

            Columns,
            // ########
            // #      #
            // # o  o #
            // #      #
            // # o  o #
            // #      #
            // ########

            Filled,
            // ##### ##
            // ####   #
            // ##### ##
            // ########
            // ######  
            // ########
            // ########
        }

        public Room Room { get; set; }

        public List<List<Item>> Generate()
        {
            var layout = new List<List<Item>>();
            Random random = new Random();

            for (int i = 0; i < Room.Height; i++)
            {
                var list = new List<Item>();
                for (int j = 0; j < Room.Width; j++)
                    list.Add(Item.Air);

                layout.Add(list);
            }

            var odds = new SortedDictionary<Layout, int>
            {
                {Layout.Regular, 15},
                {Layout.Columns, 5},
                {Layout.Lake, 2},
                {Layout.Filled, 1},
            };

            // get a random number from 0 to sum of values
            int total = 0;
            foreach ((var key, var value) in odds)
                total += value;
            int picked = random.Next(0, total);

            // get the layout that corresponds to this random number
            Layout l = Layout.Regular;
            total = 0;
            foreach ((var key, var value) in odds)
            {
                if (total <= picked)
                    l = key;
                total += value;
            }

            if (picked == 25)
                Console.WriteLine("wow");

            // add walls (same for all rooms)
            for (int x = 0; x < Room.Width; x++)
            {
                layout[0][x] = Item.Wall;
                layout[^1][x] = Item.Wall;
            }

            for (int y = 0; y < Room.Height; y++)
            {
                layout[y][0] = Item.Wall;
                layout[y][^1] = Item.Wall;
            }

            // remove walls for entrances
            foreach ((var xe, var ye) in Room.Entrances)
            {
                int x = xe - Room.X;
                int y = ye - Room.Y;

                layout[y][x] = Item.Air;

                // special case for corners (so the room is accessible)
                if (x == 0 && y == 0
                    || x == 0 && y == Room.Height - 1
                    || x == Room.Width - 1 && y == 0
                    || x == Room.Width - 1 && y == Room.Height - 1)
                {
                    foreach ((int xd, int yd) in Constant.ManhattanDeltas)
                    {
                        int xn = x + xd, yn = y + yd;

                        if (xn >= 0 && yn >= 0 && xn < Room.Width && yn < Room.Height)
                            layout[yn][xn] = Item.Air;
                    }
                }
            }

            switch (l)
            {
                case Layout.Regular: // do nothing extra
                    break;
                case Layout.Lake:
                    const int lakeOffset = 2; // TODO: change depending on size?

                    for (int x = lakeOffset; x < Room.Width - lakeOffset; x++)
                    for (int y = lakeOffset; y < Room.Height - lakeOffset; y++)
                        layout[y][x] = Item.Water;

                    break;
                case Layout.Columns:
                    const int columnOffset = 2 ; // TODO: change depending on size?

                    // a chance to not add one of the columns
                    const double dontAddColumnChance = 0.2;

                    // if the columns would be touching, only add the diagonal ones
                    if (!(Room.Width <= columnOffset * 2 + 2 || Room.Height <= columnOffset * 2 + 2))
                    {
                        if (random.NextDouble() > dontAddColumnChance) layout[columnOffset][columnOffset] = Item.Column;
                        if (random.NextDouble() > dontAddColumnChance)
                            layout[^(columnOffset + 1)][^(columnOffset + 1)] = Item.Column;
                    }

                    if (random.NextDouble() > dontAddColumnChance) layout[columnOffset][^(columnOffset + 1)] = Item.Column;
                    if (random.NextDouble() > dontAddColumnChance) layout[^(columnOffset + 1)][columnOffset] = Item.Column;
                    break;
                case Layout.Filled:
                    int fillOffset = 1;
                    for (int x = fillOffset; x < Room.Width - fillOffset; x++)
                    for (int y = fillOffset; y < Room.Height - fillOffset; y++)
                        layout[y][x] = Item.Wall;

                    // a BFS will be run for the given number of steps to make the effect of a "hole" into the room
                    foreach ((int xe, int ye) in Room.Entrances)
                    {
                        int fillSteps = random.Next(2, 5);
                        double ignoreDirectionChange = 0.1;

                        var explored = new HashSet<(int, int)>();
                        explored.Add((xe - Room.X, ye - Room.Y));

                        var queue = new Queue<(int, int, int)>();
                        queue.Enqueue((xe - Room.X, ye - Room.Y, 0));

                        while (queue.Count != 0)
                        {
                            (int x, int y, int d) = queue.Dequeue();

                            if (d > fillSteps)
                                break;

                            layout[y][x] = Item.Air;

                            foreach ((int xd, int yd) in Constant.ManhattanDeltas)
                            {
                                int xn = x + xd, yn = y + yd;

                                // skip explored
                                if (explored.Contains((xn, yn)))
                                    continue;

                                explored.Add((xn, yn));

                                if (xn >= fillOffset
                                    && yn >= fillOffset
                                    && xn < Room.Width - fillOffset
                                    && yn < Room.Height - fillOffset
                                    && random.NextDouble() > ignoreDirectionChange)
                                {
                                    queue.Enqueue((xn, yn, d + 1));
                                }
                            }
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // TODO: add items and enemies to the room

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