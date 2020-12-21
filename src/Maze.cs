namespace Donjun
{
    /// <summary>
    /// An item(-ASCII) enum for visualizing things in the dungeon.
    /// </summary>
    enum Item
    {
        Air = ' ',
        Wall = '#',
        Nothing = 'X',
    }

    class Maze : IMaze
    {
        private Path _path;
        private RoomCollection _roomCollection;

        public Maze(RoomCollection roomCollection, Path path)
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

        public int Width => _roomCollection.Width;

        public int Height => _roomCollection.Height;
    }
}