namespace Donjun
{
    /// <summary>
    /// Something that has bounds (width + height).
    /// </summary>
    interface IBoundable
    {
        public int Width { get; }

        public int Height { get; }
    }

    /// <summary>
    /// Something for which it makes sense to ask "what is at this position?"
    /// </summary>
    interface IAttable
    {
        public Item At(int x, int y);
    }

    /// <summary>
    /// An interface for a dungeon. It has to both have given bounds and return items at the given positions.
    /// </summary>
    interface IDungeon : IAttable, IBoundable { }
}