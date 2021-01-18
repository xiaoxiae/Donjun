using System;
using System.Collections.Generic;
using System.Drawing;

namespace Donjun
{
    /// <summary>
    /// A single room in the dungeon.
    /// </summary>
    public class Room : IBoundable, IAttable
    {
        private Rectangle _room; // internally stored as a rectangle
        public List<(int, int)> Entrances; // entrances to the room
        private List<List<Item>> _layout;

        public Room(int x, int y, int width, int height)
        {
            _room = new Rectangle();
            Entrances = new List<(int, int)>();

            X = x;
            Y = y;
            Width = width;
            Height = height;
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
            private set => _room.X = value;
        }

        public int Y
        {
            get => _room.Y;
            private set => _room.Y = value;
        }

        public int Width
        {
            get => _room.Width;
            private set
            {
                if (value <= 0) throw new ArgumentException();
                _room.Width = value;
            }
        }

        public int Height
        {
            get => _room.Height;
            private set
            {
                if (value <= 0) throw new ArgumentException();
                _room.Height = value;
            }
        }

        /// <summary>
        /// Return true if the given room intersects another.
        /// </summary>
        /// <param name="other">The room to check against.</param>
        public bool Intersects(Room other) => other.Boundary.IntersectsWith(Boundary);

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

        /// <summary>
        /// Return the item in the room.
        /// Note that x and y are relative to the position of the room.
        /// </summary>
        public Item At(int x, int y)
        {
            return _layout[y][x];
        }

        /// <summary>
        /// Set the layout of the room.
        /// </summary>
        public void SetLayout(List<List<Item>> layout)
        {
            _layout = layout;
        }
    }

    /// <summary>
    /// A collection of rooms.
    /// </summary>
    class RoomCollection : IBoundable, IAttable
    {
        /// <summary>
        /// The list of rooms that make up this collection.
        /// </summary>
        public readonly List<Room> Rooms = new List<Room>();

        /// <summary>
        /// The boundary in which all rooms have to reside.
        /// </summary>
        public Rectangle Boundary { get; }

        public RoomCollection(int width, int height)
        {
            Boundary = new Rectangle(0, 0, width, height);
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
        /// Throws an exception if the room isn't in the collection.
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
        /// Return True if the specified coordinate is an entrance to some room.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool IsEntrance(int x, int y) => IsRoom(x, y) && RoomAt(x, y).Entrances.Contains((x, y));

        /// <summary>
        /// Return True if the specified coordinate does not intersect a room.
        /// </summary>
        public bool IsRoom(int x, int y) => RoomAt(x, y) != null;

        /// <summary>
        /// Return the room at the given coordinates. If no such room exists, return null.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Room RoomAt(int x, int y)
        {
            var room = new Room(x, y, 1, 1);

            foreach (var other in Rooms)
                if (room.Intersects(other))
                    return other;

            return null;
        }

        /// <summary>
        /// If it intersects a room, return the value at it, else Nothing.
        /// </summary>
        public Item At(int x, int y)
        {
            var room = RoomAt(x, y);
            return room?.At(x - room.X, y - room.Y) ?? Item.Nothing;
        }
    }
}