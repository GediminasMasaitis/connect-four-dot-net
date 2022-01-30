namespace ConnectGame
{
    //public record Coordinate(int Column, int Row)
    //{
    //    public static Coordinate operator +(Coordinate lhs, Coordinate rhs)
    //    {
    //        return new Coordinate(lhs.Column + rhs.Column, lhs.Row + rhs.Row);
    //    }
    //    //public static Coordinate operator *(Coordinate lhs, int multiplier)
    //    //{

    //    //}
    //}

    public struct Coordinate
    {
        public readonly int Column;
        public readonly int Row;

        public static Coordinate operator +(Coordinate lhs, Coordinate rhs)
        {
            return new Coordinate(lhs.Column + rhs.Column, lhs.Row + rhs.Row);
        }

        public Coordinate(int column, int row)
        {
            Column = column;
            Row = row;
        }

        public override string ToString()
        {
            return $"{Column}; {Row}";
        }

        public (int, int) ToTuple()
        {
            return (Column, Row);
        }

        public int ToCell(int width)
        {
            return Column + Row * width;
        }
    }
}