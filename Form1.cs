using System;
using System.Drawing;
using System.Windows.Forms;

namespace MyWinFormsApp
{
    public partial class Form1 : Form
    {
        private const int TileSize = 60;
        private const int GridSize = 8;
        private bool isWhiteTurn = true;
        private Panel[,] tiles = new Panel[8, 8];
        private Label draggingPiece = null;
        private Point draggingFrom;

        private readonly string[,] initialSetup = new string[8, 8]
        {
            { "♜", "♞", "♝", "♛", "♚", "♝", "♞", "♜" },
            { "♟", "♟", "♟", "♟", "♟", "♟", "♟", "♟" },
            { "", "", "", "", "", "", "", "" },
            { "", "", "", "", "", "", "", "" },
            { "", "", "", "", "", "", "", "" },
            { "", "", "", "", "", "", "", "" },
            { "♙", "♙", "♙", "♙", "♙", "♙", "♙", "♙" },
            { "♖", "♘", "♗", "♕", "♔", "♗", "♘", "♖" }
        };

        public Form1()
        {
            InitializeComponent();
            this.Text = "Chess - White's Turn";
            this.ClientSize = new Size(TileSize * GridSize, TileSize * GridSize + 40);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            CreateChessBoard();
            AddRestartButton();
        }

        private void CreateChessBoard()
        {
            for (int row = 0; row < GridSize; row++)
            {
                for (int col = 0; col < GridSize; col++)
                {
                    Panel tile = new Panel
                    {
                        Size = new Size(TileSize, TileSize),
                        Location = new Point(col * TileSize, row * TileSize),
                        BackColor = (row + col) % 2 == 0 ? Color.Beige : Color.SaddleBrown,
                        BorderStyle = BorderStyle.FixedSingle,
                        Tag = new Point(row, col),
                        AllowDrop = true
                    };

                    tile.DragEnter += Tile_DragEnter;
                    tile.DragDrop += Tile_DragDrop;
                    tile.DragLeave += Tile_DragLeave;
                    this.Controls.Add(tile);
                    tiles[row, col] = tile;

                    string piece = initialSetup[row, col];
                    if (!string.IsNullOrEmpty(piece))
                    {
                        AddPieceToTile(tile, piece);
                    }
                }
            }
        }

        private void AddRestartButton()
        {
            Button btnRestart = new Button
            {
                Text = "Restart",
                Location = new Point(0, TileSize * GridSize + 5),
                Width = TileSize * GridSize
            };
            btnRestart.Click += (s, e) =>
            {
                this.Controls.Clear();
                tiles = new Panel[8, 8];
                isWhiteTurn = true;
                this.Text = "Chess - White's Turn";
                CreateChessBoard();
                AddRestartButton();
            };
            this.Controls.Add(btnRestart);
        }

        private void AddPieceToTile(Panel tile, string piece)
        {
            Label pieceLabel = new Label
            {
                Text = piece,
                Font = new Font("Segoe UI", 28),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(TileSize, TileSize),
                BackColor = Color.Transparent
            };

            pieceLabel.MouseDown += Piece_MouseDown;
            tile.Controls.Add(pieceLabel);
        }

        private void Piece_MouseDown(object sender, MouseEventArgs e)
        {
            Label piece = sender as Label;
            Panel parent = (Panel)piece.Parent;
            Point from = (Point)parent.Tag;

            if (!IsPieceTurnValid(piece.Text))
            {
                MessageBox.Show($"It's {(isWhiteTurn ? "White" : "Black")}'s turn!", "Invalid Move", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            draggingPiece = piece;
            draggingFrom = from;
            piece.DoDragDrop(piece, DragDropEffects.Move);
        }

        private void Tile_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Label)))
            {
                e.Effect = DragDropEffects.Move;
                Panel target = sender as Panel;
                target.BackColor = Color.LightGreen;
            }
        }

        private void Tile_DragLeave(object sender, EventArgs e)
        {
            Panel tile = sender as Panel;
            Point pos = (Point)tile.Tag;
            tile.BackColor = (pos.X + pos.Y) % 2 == 0 ? Color.Beige : Color.SaddleBrown;
        }

        private void Tile_DragDrop(object sender, DragEventArgs e)
        {
            Panel targetTile = (Panel)sender;
            Point to = (Point)targetTile.Tag;

            if (draggingPiece == null) return;

            if (!IsLegalMove(draggingFrom, to)) {
                MessageBox.Show("Illegal move!", "Invalid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                targetTile.BackColor = (to.X + to.Y) % 2 == 0 ? Color.Beige : Color.SaddleBrown;
                return;
            }

            Panel oldTile = (Panel)draggingPiece.Parent;
            oldTile.Controls.Clear();
            targetTile.Controls.Clear(); // Capture if piece exists
            AddPieceToTile(targetTile, draggingPiece.Text);

            isWhiteTurn = !isWhiteTurn;
            this.Text = $"Chess - {(isWhiteTurn ? "White" : "Black")}'s Turn";
            draggingPiece = null;

            targetTile.BackColor = (to.X + to.Y) % 2 == 0 ? Color.Beige : Color.SaddleBrown;
        }

        private bool IsPieceTurnValid(string piece)
        {
            return (isWhiteTurn && "♖♘♗♕♔♙".Contains(piece)) || (!isWhiteTurn && "♜♞♝♛♚♟".Contains(piece));
        }

        private bool IsLegalMove(Point from, Point to)
        {
            if (from == to) return false;

            string piece = GetPieceAt(from);
            string target = GetPieceAt(to);

            // Cannot capture same color
            if (!string.IsNullOrEmpty(target) &&
                ("♖♘♗♕♔♙".Contains(piece) && "♖♘♗♕♔♙".Contains(target)) ||
                ("♜♞♝♛♚♟".Contains(piece) && "♜♞♝♛♚♟".Contains(target)))
                return false;

            int dx = to.Y - from.Y;
            int dy = to.X - from.X;

            switch (piece)
            {
                case "♙": return dy == -1 && dx == 0 && string.IsNullOrEmpty(target)
                        || dy == -2 && dx == 0 && from.X == 6 && string.IsNullOrEmpty(GetPieceAt(new Point(5, from.Y))) && string.IsNullOrEmpty(target)
                        || dy == -1 && Math.Abs(dx) == 1 && !string.IsNullOrEmpty(target);
                case "♟": return dy == 1 && dx == 0 && string.IsNullOrEmpty(target)
                        || dy == 2 && dx == 0 && from.X == 1 && string.IsNullOrEmpty(GetPieceAt(new Point(2, from.Y))) && string.IsNullOrEmpty(target)
                        || dy == 1 && Math.Abs(dx) == 1 && !string.IsNullOrEmpty(target);
                case "♖": return IsClearPath(from, to) && (dx == 0 || dy == 0);
                case "♜": return IsClearPath(from, to) && (dx == 0 || dy == 0);
                case "♗": return IsClearPath(from, to) && Math.Abs(dx) == Math.Abs(dy);
                case "♝": return IsClearPath(from, to) && Math.Abs(dx) == Math.Abs(dy);
                case "♕":
                case "♛": return IsClearPath(from, to) && (dx == 0 || dy == 0 || Math.Abs(dx) == Math.Abs(dy));
                case "♘":
                case "♞": return Math.Abs(dx) == 1 && Math.Abs(dy) == 2 || Math.Abs(dx) == 2 && Math.Abs(dy) == 1;
                case "♔":
                case "♚": return Math.Abs(dx) <= 1 && Math.Abs(dy) <= 1;
                default: return false;
            }
        }

        private string GetPieceAt(Point pos)
        {
            Control ctrl = tiles[pos.X, pos.Y];
            return ctrl.Controls.Count > 0 ? ctrl.Controls[0].Text : "";
        }

        private bool IsClearPath(Point from, Point to)
        {
            int dx = Math.Sign(to.Y - from.Y);
            int dy = Math.Sign(to.X - from.X);
            int x = from.Y + dx;
            int y = from.X + dy;

            while (x != to.Y || y != to.X)
            {
                if (!string.IsNullOrEmpty(GetPieceAt(new Point(y, x))))
                    return false;

                x += dx;
                y += dy;
            }

            return true;
        }
    }
}
