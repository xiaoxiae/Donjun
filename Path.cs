using System.Collections.Generic;

namespace Donjun
{
    /// <summary>
    /// A class representing a discrete path.
    /// </summary>
    class Path : IAttable
    {
        private HashSet<(int, int)> _pathPoints = new HashSet<(int, int)>();

        /// <summary>
        /// Return empty if the given coordinate is on a path, else return a wall.
        /// </summary>
        public Item At(int x, int y) => _pathPoints.Contains((x, y)) ? Item.Air : Item.Nothing;

        /// <summary>
        /// Add a point to path.
        /// </summary>
        public void Add(int x, int y)
        {
            _pathPoints.Add((x, y));
        }

        /// <summary>
        /// Remove a point from the path.
        /// </summary>
        public void Remove(int x, int y)
        {
            _pathPoints.Remove((x, y));
        }

        /// <summary>
        /// Return true if the path contains a point.
        /// </summary>
        public bool Contains(int x, int y) => _pathPoints.Contains((x, y));
    }
}