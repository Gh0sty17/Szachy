using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Szachy
{
    public partial class MainWindow : Window
    {
        // Reprezentacja planszy: wielkie litery = Białe, małe = Czarne, puste = " "
        private string[,] board = new string[8, 8]
        {
            { "r", "n", "b", "q", "k", "b", "n", "r" },
            { "p", "p", "p", "p", "p", "p", "p", "p" },
            { " ", " ", " ", " ", " ", " ", " ", " " },
            { " ", " ", " ", " ", " ", " ", " ", " " },
            { " ", " ", " ", " ", " ", " ", " ", " " },
            { " ", " ", " ", " ", " ", " ", " ", " " },
            { "P", "P", "P", "P", "P", "P", "P", "P" },
            { "R", "N", "B", "Q", "K", "B", "N", "R" }
        };

        private Button[,] buttons = new Button[8, 8];
        private bool isWhiteTurn = true;
        private int selectedRow = -1;
        private int selectedCol = -1;

        public MainWindow()
        {
            InitializeComponent();
            CreateBoard();
        }

        private void CreateBoard()
        {
            ChessBoardGrid.Children.Clear();
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Button btn = new Button
                    {
                        FontSize = 36,
                        FontWeight = FontWeights.Bold,
                        Focusable = false
                    };
                    btn.Tag = new Tuple<int, int>(r, c);
                    btn.Click += BoardButton_Click;
                    buttons[r, c] = btn;
                    ChessBoardGrid.Children.Add(btn);
                }
            }
            RefreshBoardDisplay();
        }

        // ZMODYFIKOWANA FUNKCJA: Teraz podświetla także możliwe ruchy
        private void RefreshBoardDisplay()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    // 1. Domyślne kolorowanie szachownicy
                    if ((r + c) % 2 == 0)
                        buttons[r, c].Background = new SolidColorBrush(Color.FromRgb(240, 217, 181)); // Jasne
                    else
                        buttons[r, c].Background = new SolidColorBrush(Color.FromRgb(181, 136, 99));  // Ciemne

                    // 2. Podświetlenie MOŻLIWYCH RUCHÓW (na żółto-pomarańczowy)
                    // Jeśli jakaś figura jest zaznaczona, sprawdzamy czy może stanąć na polu [r, c]
                    if (selectedRow != -1 && selectedCol != -1)
                    {
                        if (IsValidMove(selectedRow, selectedCol, r, c))
                        {
                            buttons[r, c].Background = new SolidColorBrush(Color.FromRgb(255, 235, 156)); // Jasnożółty dla wskazania drogi
                        }
                    }

                    // 3. Podświetlenie AKTUALNIE ZAZNACZONEJ figury (na zielono)
                    if (r == selectedRow && c == selectedCol)
                    {
                        buttons[r, c].Background = Brushes.LightGreen;
                    }

                    // 4. Przypisanie figur i kolorów tekstu
                    string piece = board[r, c];
                    buttons[r, c].Content = GetPieceSymbol(piece);
                    buttons[r, c].Foreground = char.IsUpper(piece, 0) ? Brushes.White : Brushes.Black;
                }
            }
            StatusText.Text = isWhiteTurn ? "Ruch: Białe" : "Ruch: Czarne";
        }

        private string GetPieceSymbol(string piece)
        {
            return piece.ToUpper() switch
            {
                "P" => "♙",
                "R" => "♖",
                "N" => "♘",
                "B" => "♗",
                "Q" => "♕",
                "K" => "♔",
                _ => ""
            };
        }

        private void BoardButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            var position = (Tuple<int, int>)clickedButton.Tag;
            int row = position.Item1;
            int col = position.Item2;

            string clickedPiece = board[row, col];
            bool isPieceWhite = char.IsUpper(clickedPiece, 0);

            // 1. Wybór lub zmiana zaznaczenia własnej figury
            if (clickedPiece != " " && isPieceWhite == isWhiteTurn)
            {
                selectedRow = row;
                selectedCol = col;
                RefreshBoardDisplay(); // Odświeży i pokaże żółte pola dla nowej figury
            }
            // 2. Próba wykonania ruchu na zaznaczone pole
            else if (selectedRow != -1 && selectedCol != -1)
            {
                if (IsValidMove(selectedRow, selectedCol, row, col))
                {
                    ExecuteMove(selectedRow, selectedCol, row, col);
                    selectedRow = -1;
                    selectedCol = -1;
                    isWhiteTurn = !isWhiteTurn;
                    RefreshBoardDisplay();
                }
                else
                {
                    // Kliknięcie w niedozwolone miejsce resetuje zaznaczenie (czystsze UI niż wyskakujący komunikat)
                    selectedRow = -1;
                    selectedCol = -1;
                    RefreshBoardDisplay();
                }
            }
        }

        private void ExecuteMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            board[toRow, toCol] = board[fromRow, fromCol];
            board[fromRow, fromCol] = " ";
        }

        private bool IsValidMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            if (fromRow == toRow && fromCol == toCol) return false;

            string piece = board[fromRow, fromCol];
            string target = board[toRow, toCol];

            if (target != " " && char.IsUpper(piece, 0) == char.IsUpper(target, 0)) return false;

            int deltaRow = toRow - fromRow;
            int deltaCol = toCol - fromCol;
            int absDeltaRow = Math.Abs(deltaRow);
            int absDeltaCol = Math.Abs(deltaCol);

            switch (piece.ToUpper())
            {
                case "P":
                    int direction = char.IsUpper(piece, 0) ? -1 : 1;
                    int startRow = char.IsUpper(piece, 0) ? 6 : 1;

                    if (deltaCol == 0 && deltaRow == direction && target == " ")
                        return true;
                    if (deltaCol == 0 && fromRow == startRow && deltaRow == 2 * direction && target == " " && board[fromRow + direction, fromCol] == " ")
                        return true;
                    if (absDeltaCol == 1 && deltaRow == direction && target != " ")
                        return true;

                    return false;

                case "R":
                    if (fromRow == toRow || fromCol == toCol)
                        return IsPathClear(fromRow, fromCol, toRow, toCol);
                    return false;

                case "N":
                    return (absDeltaRow == 2 && absDeltaCol == 1) || (absDeltaRow == 1 && absDeltaCol == 2);

                case "B":
                    if (absDeltaRow == absDeltaCol)
                        return IsPathClear(fromRow, fromCol, toRow, toCol);
                    return false;

                case "Q":
                    if (fromRow == toRow || fromCol == toCol || absDeltaRow == absDeltaCol)
                        return IsPathClear(fromRow, fromCol, toRow, toCol);
                    return false;

                case "K":
                    return absDeltaRow <= 1 && absDeltaCol <= 1;

                default:
                    return false;
            }
        }

        private bool IsPathClear(int fromRow, int fromCol, int toRow, int toCol)
        {
            int stepRow = Math.Sign(toRow - fromRow);
            int stepCol = Math.Sign(toCol - fromCol);

            int currentRow = fromRow + stepRow;
            int currentCol = fromCol + stepCol;

            while (currentRow != toRow || currentCol != toCol)
            {
                if (board[currentRow, currentCol] != " ")
                    return false;

                currentRow += stepRow;
                currentCol += stepCol;
            }
            return true;
        }
    }
}