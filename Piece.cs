// Piece.cs
namespace ChessGame
{
    public class Piece
    {
        public char Type { get; set; }
        public bool IsWhite { get; set; }
        public string Symbol { get; set; }

        public Piece(string symbol)
        {
            Symbol = symbol;
            Type = char.ToUpper(symbol[0]);
            IsWhite = char.IsUpper(symbol[0]);
        }
    }
}