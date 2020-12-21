using System;
using System.Collections.Generic;
using System.IO;

namespace Donjun
{
    /// <summary>
    /// A class representing a discrete path.
    /// </summary>
    class Path : IAttable
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
            if (rooms.IsRoom(x, y)) return false;

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

                    if (!rooms.IsRoom(xn, yn))
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
        /// Connect the specified room to one or more closest points on the path, returning the connection points.
        /// </summary>
        private List<(int, int)> ConnectRoomToPath(Room room, int minConnections = 1, int maxConnections = 4)
        {
            // the number of connections to the path
            Random random = new Random();
            int connections = random.Next(minConnections, maxConnections);
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
                    if (_pathPoints.Contains((x, y)))
                    {
                        int xc = x;
                        int yc = y;

                        connectionPoints.Add((xc, yc));

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

            return connectionPoints;
        }

        /// <summary>
        /// Return true if the specified coordinates point to the path.
        /// </summary>
        public bool PointsToPath(int x, int y) => _pathPoints.Contains((x, y));

        /// <summary>
        /// Remove dead ends using a BFS, starting from the provided coordinates.
        /// </summary>
        private void ClearDeadEnds(int xs, int ys, RoomCollection rooms)
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
                    if (!_pathPoints.Contains((xn, yn)))
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
                        if (rooms.IsRoom(x + xd, y + yd))
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
                _pathPoints.Remove((x, y));

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
        public Path(RoomCollection rooms)
        {
            _pathPoints = new HashSet<(int, int)>();

            // add equidistant points to rooms
            for (int x = 0; x < rooms.Width; x++)
                for (int y = 0; y < rooms.Height; y++)
                    if (IsEquidistantToMultipleRooms(x, y, rooms))
                        _pathPoints.Add((x, y));

            // connect rooms to the path
            // also save some point on the path that we definitely don't want to remove (is not a dead end)
            int xp = -1, yp = -1;
            foreach (var room in rooms.Rooms)
            {
                var points = ConnectRoomToPath(room);

                if (xp == -1) (xp, yp) = points[0];
            }

            // clear dead ends
            ClearDeadEnds(xp, yp, rooms);

            // TODO: remove this, just for debug
            var writer = new StreamWriter("tmp.out");
            for (int x = 0; x < rooms.Width; x++)
            {
                for (int y = 0; y < rooms.Height; y++)
                {
                    if (_pathPoints.Contains((x, y))) writer.Write(".");
                    else writer.Write(rooms.IsRoom(x, y) ? "#" : " ");
                }

                writer.WriteLine();
            }

            writer.Flush();
        }

        /// <summary>
        /// Return empty if the given coordinate is on a path, else return a wall.
        /// </summary>
        public Item At(int x, int y) => _pathPoints.Contains((x, y)) ? Item.Empty : Item.Wall;
    }
}