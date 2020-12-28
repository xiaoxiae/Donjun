namespace Donjun
{
    /// <summary>
    /// An item(-ASCII) enum for visualizing things in the dungeon.
    /// </summary>
    public enum Item
    {
        Air = ' ',
        Wall = '█',
        LUWallCorner = '▘',
        RUWallCorner = '▝',
        LDWallCorner = '▖',
        RDWallCorner = '▗',
        Enemies = 'E',
        Loot = 'L',
        Water = '~',
        Column = '●',
        Nothing = '?',
    }

    class Dungeon : IDungeon
    {
        private Path _path;
        private RoomCollection _roomCollection;

        public Dungeon(RoomCollection roomCollection, Path path)
        {
            _roomCollection = roomCollection;
            _path = path;
        }

        /// <summary>
        /// Check, whether it's a path first. If not, check rooms. Default to a wall;
        /// </summary>
        public Item At(int x, int y)
        {
            if (_path.At(x, y) != Item.Nothing) return _path.At(x, y);
            if (_roomCollection.At(x, y) != Item.Nothing) return _roomCollection.At(x, y);

            return Item.Wall;
        }

        /// <summary>
        /// Return the width of the dungeon.
        /// </summary>
        public int Width => _roomCollection.Width;

        /// <summary>
        /// Return the height of the dungeon.
        /// </summary>
        public int Height => _roomCollection.Height;
    }
}