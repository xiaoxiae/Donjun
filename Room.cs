using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Donjun
{
    /// <summary>
    /// An enum for doing things with directions.
    /// </summary>
    enum Direction
    {
        All = -1,
        Right = 0,
        Up = 1,
        Left = 2,
        Down = 3
    }

    /// <summary>
    /// A single room in the maze.
    /// </summary>
    class Room
    {
        private Rectangle _room; // internally stored as a rectangle
        private List<(int, int)> entrances; // entrances to the room

        public Room(int x, int y, int width, int height)
        {
            _room = new Rectangle();
            entrances = new List<(int, int)>();

            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Add an entrance to the room.
        /// Note that the point has to be right on the boundary of the room.
        /// TODO: add a check for that
        /// </summary>
        public void AddEntrance(int x, int y)
        {
            entrances.Add((x, y));
        }

        /// <summary>
        /// Returns the rectangle encapsulating the entire room.
        /// </summary>
        public Rectangle Boundary => _room;

        /// <summary>
        /// Return True if the room contains the given point.
        /// </summary>
        public bool Contains(int x, int y) => Boundary.Contains(x, y);

        public int X
        {
            get => _room.X;
            set => _room.X = value;
        }

        public int Y
        {
            get => _room.Y;
            set => _room.Y = value;
        }

        public int Width
        {
            get => _room.Width;
            set
            {
                if (value <= 0) throw new ArgumentException();
                _room.Width = value;
            }
        }

        public int Height
        {
            get => _room.Height;
            set
            {
                if (value <= 0) throw new ArgumentException();
                _room.Height = value;
            }
        }

        /// <summary>
        /// Return the area of the room.
        /// TODO: maybe unused?
        /// </summary>
        public int Area => Width * Height;

        /// <summary>
        /// Expand the room's width/height.
        /// TODO: maybe unused?
        /// </summary>
        /// <param name="direction">The direction in which to shrink.
        /// If no direction is specified, shrink in all directions.</param>
        /// <param name="by">The amount of space by which to shrink.</param>
        public void Expand(Direction direction = Direction.All, int by = 1)
        {
            ResizeInDirection(direction, by);
        }

        /// <summary>
        /// Shrink the room's width/height.
        /// TODO: maybe unused?
        /// </summary>
        /// <param name="direction">The direction in which to shrink.
        /// If no direction is specified, shrink in all directions.</param>
        /// <param name="by">The amount of space by which to shrink.</param>
        public void Shrink(Direction direction = Direction.All, int by = 1)
        {
            ResizeInDirection(direction, -by);
        }

        /// <summary>
        /// Resize the room in the given direction.
        /// TODO: maybe unused?
        /// </summary>
        private void ResizeInDirection(Direction direction, int delta)
        {
            switch (direction)
            {
                case Direction.All:
                    Width += delta * 2;
                    Height += delta * 2;
                    Y -= delta;
                    X -= delta;
                    break;
                case Direction.Right:
                    Width += delta;
                    break;
                case Direction.Up:
                    Height += delta;
                    break;
                case Direction.Left:
                    Width += delta;
                    X -= delta;
                    break;
                case Direction.Down:
                    Height += delta;
                    Y -= delta;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        /// <summary>
        /// Return true if this room intersects another.
        /// </summary>
        /// TODO: maybe unused?
        /// <param name="other">The room to check against.</param>
        /// <param name="spacing">Additional number of spaces that have to separate the rooms. Defaults to 0.</param>
        public bool Intersects(Room other, int spacing = 0)
            => other.Boundary.IntersectsWith(Rectangle.Inflate(Boundary, spacing, spacing));

        /// <summary>
        /// Split the room into two, horizontally.
        /// </summary>
        /// <param name="percentage">The percentage of the new room the left one should occupy. Should be between 0 and 1.</param>
        /// <param name="spacing">Additional spacing between the rooms.</param>
        public (Room left, Room right)? SplitHorizontally(double percentage, int spacing = 0)
        {
            int leftWidth = (int) (Width * percentage);
            int rightWidth = Width - leftWidth;

            leftWidth -= spacing / 2;
            rightWidth -= spacing - spacing / 2;

            if (leftWidth <= 0 || rightWidth <= 0) return null;

            var left = new Room(X, Y, leftWidth, Height);
            var right = new Room(X + leftWidth + spacing, Y, rightWidth, Height);

            return (left, right);
        }

        /// <summary>
        /// Split the room into two, vertically.
        /// </summary>
        /// <param name="percentage">The percentage of the new room the bottom one should occupy. Should be between 0 and 1.</param>
        /// <param name="spacing">Additional spacing between the rooms.</param>
        public (Room left, Room right)? SplitVertically(double percentage, int spacing = 0)
        {
            int bottomHeight = (int) (Height * percentage);
            int topHeight = Height - bottomHeight;

            bottomHeight -= spacing / 2;
            topHeight -= spacing - spacing / 2;

            if (bottomHeight <= 0 || topHeight <= 0) return null;

            var bottom = new Room(X, Y, Width, bottomHeight);
            var top = new Room(X, Y + bottomHeight + spacing, Width, topHeight);

            return (bottom, top);
        }
    }

    /// <summary>
    /// A collection of rooms.
    /// </summary>
    class RoomCollection
    {
        public readonly List<Room> Rooms = new List<Room>();

        // the boundary in which all rooms have to reside
        public Rectangle Boundary { get; set; }

        // the minimum number of empty spaces between each of the rooms
        private readonly int _roomSpacing;

        public RoomCollection(int width, int height, int roomSpacing)
        {
            Boundary = new Rectangle(0, 0, width, height);
            _roomSpacing = roomSpacing;
        }

        /// <summary>
        /// Add a room to the room collection.
        /// </summary>
        public void AddRoom(Room room)
        {
            Rooms.Add(room);
        }

        /// <summary>
        /// Remove a room from the room collection.
        /// </summary>
        public void RemoveRoom(Room room)
        {
            Rooms.Remove(room);
        }

        /// <summary>
        /// The width of the room collection.
        /// </summary>
        public int Width => Boundary.Width;

        /// <summary>
        /// The height of the room collection.
        /// </summary>
        public int Height => Boundary.Width;

        /// <summary>
        /// Return True if the specified coordinate does not intersect a room.
        /// </summary>
        public bool Free(int x, int y) => RoomAt(x, y) == null;

        /// <summary>
        /// Return the room at the given coordinates. If no such room exists, return null.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Room? RoomAt(int x, int y)
        {
            var room = new Room(x, y, 1, 1);

            foreach (var other in Rooms)
                if (room.Intersects(other))
                    return other;

            return null;
        }

        /// <summary>
        /// Return true if a room can be placed in this room collection without being outside bounds or intersecting
        /// other rooms, else false.
        /// TODO: maybe unused?
        /// </summary>
        public bool CanContain(Room room, bool accountForSpacing = false)
        {
            // the room must be contained within the room boundary
            if (!Boundary.Contains(room.Boundary)) return false;

            // check intersection with other rooms
            foreach (var other in Rooms)
                if (room.Intersects(other, accountForSpacing ? _roomSpacing : 0))
                    return false;

            return true;
        }

        /// <summary>
        /// Prints the collection of rooms as a string. Mostly for debug purposes.
        /// TODO: just for debug, remove this after!
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    char c = (char) Item.Empty;
                    foreach (var room in Rooms)
                    {
                        if (room.Boundary.Contains(x, y)) c = (char) Item.Wall;
                    }

                    sb.Append(c);
                }

                sb.Append("\n");
            }

            return sb.ToString();
        }
    }
}