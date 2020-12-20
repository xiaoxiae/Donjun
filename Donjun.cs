using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using CommandLine;
using CommandLine.Text;


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
    /// An item-ASCII enum for visualizing things in the dungeon.
    /// </summary>
    enum Item
    {
        Empty = ' ',
        Wall = '#'
    }

    /// <summary>
    /// A class representing a single room in the maze.
    /// </summary>
    class Room
    {
        private Rectangle _room; // internally stored as a room

        public Room(int x, int y, int width, int height)
        {
            _room = new Rectangle();
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Returns the rectangle encapsulating the entire room.
        /// </summary>
        public Rectangle Boundary() => _room;

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
        /// </summary>
        public int Area => Width * Height;

        /// <summary>
        /// Expand the room's width/height.
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
        /// <param name="other">The room to check against.</param>
        /// <param name="spacing">Additional number of spaces that have to separate the rooms. Defaults to 0.</param>
        public bool Intersects(Room other, int spacing = 0)
            => other.Boundary().IntersectsWith(Rectangle.Inflate(Boundary(), spacing, spacing));

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
        private readonly List<Room> _rooms = new List<Room>();

        // the boundary in which all rooms have to reside
        private Rectangle Boundary { get; set; }

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
            _rooms.Add(room);
        }

        /// <summary>
        /// Remove a room from the room collection.
        /// </summary>
        public void RemoveRoom(Room room)
        {
            _rooms.Remove(room);
        }

        /// <summary>
        /// Return true if a room can be placed in this room collection without being outside bounds or intersecting
        /// other rooms, else false.
        /// </summary>
        public bool CanContain(Room room)
        {
            // the room must be contained within the room boundary
            if (!Boundary.Contains(room.Boundary())) return false;

            // check intersection with other rooms
            foreach (var other in _rooms)
                if (room.Intersects(other, _roomSpacing))
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

            for (int x = 0; x < Boundary.Width; x++)
            {
                for (int y = 0; y < Boundary.Height; y++)
                {
                    char c = (char) Item.Empty;
                    foreach (var room in _rooms)
                    {
                        if (room.Boundary().Contains(x, y)) c = (char) Item.Wall;
                    }

                    sb.Append(c);
                }

                sb.Append("\n");
            }

            return sb.ToString();
        }
    }

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

            // TODO: return the maze
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

    class Options
    {
        [Option('w', "width", Default = 50, HelpText = "The width of the generated maze.")]
        public int Width { get; set; }

        [Option('h', "height", Default = 50, HelpText = "The height of the generated maze.")]
        public int Height { get; set; }

        [Option("min-room-width", Default = 5, HelpText = "The minimum width a room can have.")]
        public int MinimumRoomWidth { get; set; }

        [Option("min-room-height", Default = 5, HelpText = "The minimum height a room can have.")]
        public int MinimumRoomHeight { get; set; }

        [Option("max-room-width", Default = 20, HelpText = "The maximum width a room can have.")]
        public int MaximumRoomWidth { get; set; }

        [Option("max-room-height", Default = 20, HelpText = "The maximum height a room can have.")]
        public int MaximumRoomHeight { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    var rmg = new RandomMazeGenerator
                    {
                        Width = options.Width,
                        Height = options.Height,
                        MinimumRoomWidth = options.MinimumRoomWidth,
                        MinimumRoomHeight = options.MinimumRoomHeight,
                        MaximumRoomWidth = options.MaximumRoomWidth,
                        MaximumRoomHeight = options.MaximumRoomHeight
                    };

                    rmg.GenerateMaze();
                });
        }
    }
}