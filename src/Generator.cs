using System;
using System.Collections.Generic;

namespace Donjun
{
    /// <summary>
    /// A generator for a collection of rooms.
    /// </summary>
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
        /// <param name="random"></param>
        public RoomCollection Generate(Random random)
        {
            var rooms = new RoomCollection(MazeWidth, MazeHeight);

            // create one big room
            var starting = new Room(0, 0, MazeWidth, MazeHeight);
            rooms.AddRoom(starting);

            // split it recursively
            RecursiveRoomSplit(starting, rooms, random);

            return rooms;
        }

        /// <summary>
        /// Split a room in half, possibly removing it and adding the new rooms (and splitting further...).
        /// </summary>
        /// <param name="current">The room to split.</param>
        /// <param name="rooms">The collection of rooms to add the smaller rooms to.</param>
        /// <param name="random">A random number generator.</param>
        private void RecursiveRoomSplit(Room current, RoomCollection rooms, Random random)
        {
            // return only when the room is small enough and we don't want to make it any smaller
            if (current.Width < MaxRoomSide && current.Height < MaxRoomSide &&
                random.NextDouble() > Constant.RoomSplitChance)
                return;

            while (true)
            {
                // number between minimumSplitPortion and (1 - minimumSplitPortion)
                double splitPortion = random.NextDouble() * (1 - Constant.RoomSplitPortion * 2) +
                                      Constant.RoomSplitPortion;

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

    /// <summary>
    /// A generator for a path that generates it using a collection of rooms.
    /// </summary>
    class PathGenerator
    {
        public RoomCollection Rooms { get; set; }
        public int MinRoomEntrances { get; set; }
        public int MaxRoomEntrances { get; set; }

        /// <summary>
        /// Return <value>true</value> if a given point is equidistant to more than one room, else <value>false</value>.
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
        private List<(int, int)> ConnectRoomToPath(Path path, Room room, Random random)
        {
            // the number of connections to the path
            int connections = random.Next(MinRoomEntrances, MaxRoomEntrances);
            var connectionPoints = new List<(int, int)>();

            for (int _ = 0; _ < connections; _++)
            {
                // starting point
                // don't include the corner points so the entrance is not in the corner
                int xs = random.Next(room.X + 1, room.X + room.Width - 2);
                int ys = random.Next(room.Y + 1, room.Y + room.Height - 2);

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
        /// Remove dead ends.
        /// 
        /// The algorithm works as follows:
        /// (1) create a DAG out of the path using BFS (orient by layers), starting at the specified coordinates
        /// (2) add vertices of deg_out 0 to be "dangling"
        /// (3) start removing the "dangling" vertices iteratively, not doing so when:
        ///     a) the vertex is adjacent to a room entrance
        ///     b) two or more vertices point at the vertex being removed
        /// </summary>
        private void ClearDeadEnds(Path path, int xs, int ys)
        {
            var queue = new Queue<(int, int)>();
            queue.Enqueue((xs, ys));

            // denote the neighbours of each vertex
            var explored = new Dictionary<(int, int), List<(int, int)>>();
            var dangling = new Queue<(int, int)>();

            // create a DAG using BFS
            while (queue.Count != 0)
            {
                (int x, int y) = queue.Dequeue();
                explored[(x, y)] = new List<(int, int)>();

                // add neighbours
                foreach ((int xd, int yd) in Constant.ManhattanDeltas)
                {
                    int xn = x + xd, yn = y + yd;

                    // skip non-path points
                    if (!path.Contains(xn, yn))
                        continue;

                    // possibly connect to either an unexplored path, or to not previous path
                    if (!explored.ContainsKey((xn, yn)) || !explored[(xn, yn)].Contains((x, y)))
                        explored[(x, y)].Add((xn, yn));

                    // if it hasn't been explored yet, explore it
                    if (!explored.ContainsKey((xn, yn)))
                        queue.Enqueue((xn, yn));
                }

                // if it is dangling (has deg_out == 0)
                if (explored[(x, y)].Count == 0)
                    dangling.Enqueue((x, y));
            }

            // remove the dangling bits by stripping them
            while (dangling.Count != 0)
            {
                (int x, int y) = dangling.Dequeue();

                // see if there is an adjacent wall
                bool found = false;
                foreach ((int xd, int yd) in Constant.ManhattanDeltas)
                {
                    int xn = x + xd, yn = y + yd;

                    if (Rooms.IsEntrance(xn, yn))
                        found = true;
                }

                if (found)
                    continue;

                Queue<(int, int)> contained = new Queue<(int, int)>();
                foreach ((int xd, int yd) in Constant.ManhattanDeltas)
                {
                    int xn = x + xd, yn = y + yd;

                    // find all predecessors that contain (x, y) and remove (x, y) from it
                    if (explored.ContainsKey((xn, yn)) && explored[(xn, yn)].Contains((x, y)))
                    {
                        explored[(xn, yn)].Remove((x, y));
                        contained.Enqueue((xn, yn));
                    }
                }

                if (contained.Count != 1)
                    continue;

                path.Remove(x, y);

                (int xc, int yc) = contained.Dequeue();

                if (explored[(xc, yc)].Count == 0)
                    dangling.Enqueue((xc, yc));
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
        /// <param name="random"></param>
        public Path Generate(Random random)
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
                var points = ConnectRoomToPath(path, room, random);

                if (xp == -1) (xp, yp) = points[0];
            }

            // clear dead ends
            ClearDeadEnds(path, xp, yp);

            return path;
        }
    }

    /// <summary>
    /// A generator for the layout of one specific room.
    /// </summary>
    public class RoomLayoutGenerator
    {
        public enum Layout
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

        public double EnemiesInRoomChance { get; set; }
        public double LootInRoomChance { get; set; }

        public List<List<Item>> Generate(Random random)
        {
            var layout = new List<List<Item>>();

            // empty room, no walls
            for (int i = 0; i < Room.Height; i++)
            {
                var list = new List<Item>();
                for (int j = 0; j < Room.Width; j++)
                    list.Add(Item.Air);

                layout.Add(list);
            }

            // get a random number from 0 to sum of values
            int total = 0;
            foreach ((var key, var value) in Constant.RoomLayoutChance)
                total += value;
            int picked = random.Next(0, total);

            // get the layout that corresponds to this random number
            Layout l = Layout.Regular;
            total = 0;
            foreach ((var key, var value) in Constant.RoomLayoutChance)
            {
                if (total <= picked)
                    l = key;
                total += value;
            }

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
            }

            // possibly add corners, if room entrances aren't in the corners
            if (random.NextDouble() < Constant.RoomCornersChance)
            {
                // for each corner
                foreach ((int x, int y) in new[]
                    {(0, 0), (0, Room.Height - 1), (Room.Width - 1, 0), (Room.Width - 1, Room.Height - 1)})
                {
                    // check if there are walls all around
                    bool allWalls = true;
                    foreach ((int xd, int yd) in Constant.ManhattanDeltas)
                    {
                        int xn = x + xd, yn = y + yd;

                        if (xn >= 0 && yn >= 0 && xn < Room.Width && yn < Room.Height && layout[yn][xn] != Item.Wall)
                            allWalls = false;
                    }

                    if (!allWalls) continue;

                    if ((x, y) == (0, 0)) layout[1][1] = Item.LUWallCorner;
                    else if ((x, y) == (0, Room.Height - 1)) layout[^2][1] = Item.LDWallCorner;
                    else if ((x, y) == (Room.Width - 1, 0)) layout[1][^2] = Item.RUWallCorner;
                    else if ((x, y) == (Room.Width - 1, Room.Height - 1)) layout[^2][^2] = Item.RDWallCorner;
                }
            }

            // generate the layout
            switch (l)
            {
                case Layout.Regular:
                    // do nothing extra
                    break;
                case Layout.Lake:
                    // fill the center with a lake
                    for (int x = Constant.LakeOffset; x < Room.Width - Constant.LakeOffset; x++)
                    for (int y = Constant.LakeOffset; y < Room.Height - Constant.LakeOffset; y++)
                        layout[y][x] = Item.Water;

                    break;
                case Layout.Columns:
                    // if the columns would be touching, only add the diagonal ones
                    if (!(Room.Width <= Constant.ColumnOffset * 2 + 2 || Room.Height <= Constant.ColumnOffset * 2 + 2))
                    {
                        if (random.NextDouble() > Constant.OmitColumnChance)
                            layout[Constant.ColumnOffset][Constant.ColumnOffset] = Item.Column;
                        if (random.NextDouble() > Constant.OmitColumnChance)
                            layout[^(Constant.ColumnOffset + 1)][^(Constant.ColumnOffset + 1)] = Item.Column;
                    }

                    if (random.NextDouble() > Constant.OmitColumnChance)
                        layout[Constant.ColumnOffset][^(Constant.ColumnOffset + 1)] = Item.Column;
                    if (random.NextDouble() > Constant.OmitColumnChance)
                        layout[^(Constant.ColumnOffset + 1)][Constant.ColumnOffset] = Item.Column;
                    break;
                case Layout.Filled:
                    for (int x = Constant.FillOffset; x < Room.Width - Constant.FillOffset; x++)
                    for (int y = Constant.FillOffset; y < Room.Height - Constant.FillOffset; y++)
                        layout[y][x] = Item.Wall;

                    // a BFS will be run for the given number of steps to make the effect of a "hole" into the room
                    foreach ((int xe, int ye) in Room.Entrances)
                    {
                        var explored = new HashSet<(int, int)>();
                        explored.Add((xe - Room.X, ye - Room.Y));

                        var queue = new Queue<(int, int, int)>();
                        queue.Enqueue((xe - Room.X, ye - Room.Y, 0));

                        while (queue.Count != 0)
                        {
                            (int x, int y, int d) = queue.Dequeue();

                            int fillSteps = random.Next(Constant.MinFillSteps, Constant.MaxFillSteps);
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

                                if (xn >= Constant.FillOffset
                                    && yn >= Constant.FillOffset
                                    && xn < Room.Width - Constant.FillOffset
                                    && yn < Room.Height - Constant.FillOffset
                                    && random.NextDouble() > Constant.OmitDirectionStepChance)
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

            var emptyTiles = new List<(int, int)>();

            for (int x = 1; x < Room.Width - 1; x++)
            for (int y = 1; y < Room.Height - 1; y++)
                if (layout[y][x] == Item.Air)
                    emptyTiles.Add((x, y));

            // possibly mark a room as containing enemies/loot
            bool enemies = random.NextDouble() < EnemiesInRoomChance && l != Layout.Filled && l != Layout.Lake;
            bool loot = random.NextDouble() < LootInRoomChance && l != Layout.Filled && l != Layout.Lake;

            int offset = 0;
            // +1s because left and top-align
            if (enemies) layout[(Room.Height - 1) / 2][(Room.Width - 1) / 2 + offset++] = Item.Enemies;
            if (loot) layout[(Room.Height - 1) / 2][(Room.Width - 1) / 2 + offset] = Item.Loot;

            return layout;
        }
    }

    /// <summary>
    /// A generator for a maze, combining multiple generators.
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
        public double EnemyChance { get; set; }
        public double LootChance { get; set; }

        /// <summary>
        /// Generate the maze.
        /// 
        /// The algorithm works as follows:
        /// (1) generate rectangular areas where the rooms are going to be
        /// (2) connect these areas via paths and let them know where the entrances are (for generating the rooms)
        /// (3) generate unique rooms in each of the rectangular areas
        /// </summary>
        /// <param name="random"></param>
        public IMaze Generate(Random random)
        {
            // (1) generate rooms
            RoomCollection roomCollection = new RoomCollectionGenerator
            {
                MazeWidth = MazeWidth,
                MazeHeight = MazeHeight,
                MinRoomSide = MinRoomSide,
                MaxRoomSide = MaxRoomSide,
                RoomSpacing = RoomSpacing
            }.Generate(random);

            // (2) generate the path, modifying the rooms in the process (adding entrances)
            Path path = new PathGenerator
            {
                Rooms = roomCollection,
                MinRoomEntrances = MinRoomEntrances,
                MaxRoomEntrances = MaxRoomEntrances
            }.Generate(random);

            // (3)  generate the respective room layouts
            foreach (var room in roomCollection.Rooms)
            {
                room.SetLayout(new RoomLayoutGenerator
                {
                    Room = room,
                    EnemiesInRoomChance = EnemyChance,
                    LootInRoomChance = LootChance,
                }.Generate(random));
            }

            return new Maze(roomCollection, path);
        }
    }
}