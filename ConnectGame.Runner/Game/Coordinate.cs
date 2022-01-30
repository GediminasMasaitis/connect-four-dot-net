namespace ConnectGame.Runner.Game
{
    public record Coordinate(int Column, int Row)
    {
        public static Coordinate operator +(Coordinate lhs, Coordinate rhs)
        {
            return new Coordinate(lhs.Column + rhs.Column, lhs.Row + rhs.Row);
        }
        //public static Coordinate operator *(Coordinate lhs, int multiplier)
        //{

        //}
    }
}