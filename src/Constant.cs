namespace Donjun
{
    public class Constant
    {
        public static readonly (int, int)[] ManhattanDeltas = {(0, 1), (1, 0), (-1, 0), (0, -1)};
        public static readonly (int, int)[] DiagonalDeltas = {(0, 1), (1, 0), (-1, 0), (0, -1), (-1, -1), (1, 1), (-1, 1), (1, -1)};
    }
}