using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TetrisClone
{
    public partial class Form1 : Form
    {
        // Global variables
        TimeSpan timeLeft = new TimeSpan(0, 5, 0);
        Color[] colorArray = { Color.CornflowerBlue, Color.Yellow, Color.Crimson,
            Color.Magenta, Color.Cyan, Color.SpringGreen, Color.BlueViolet };
        Control[] pieceArray = {null, null, null, null };
        Control[] currentPiece = { null, null, null, null };
        Control[] movingPieceHolder = { null, null, null, null };
        int currentPieceInt;
        Random random = new Random();

        public Form1()
        {
            InitializeComponent();
            this.grid.CellPaint += tableLayoutPanel1_CellPaint;
            startLabelFlash.Start();
        }

        private void tableLayoutPanel1_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {
            e.Graphics.DrawRectangle(
                Pens.Black,
                e.CellBounds.Location.X-1,
                e.CellBounds.Location.Y-1,
                e.CellBounds.Width+1,
                e.CellBounds.Height+1);
        }

        public void DropPiece()
        {
            pieceTimer.Start();

            // Layout option for each pieace
            Control[,] pieceArray =
            {
                { BoxH1, BoxH2, BoxH3, BoxH4 }, // I piece
                { BoxH1, BoxH2, BoxI1, BoxI2 }, // O piece
                { BoxH1, BoxH2, BoxI1, BoxG2 }, // S piece
                { BoxH1, BoxI1, BoxI2, BoxJ2 }, // Z piece
                { BoxH1, BoxH2, BoxH3, BoxI3 }, // L piece
                { BoxI1, BoxI2, BoxI3, BoxH3 }, // J piece
                { BoxH1, BoxH2, BoxH3, BoxI2 }  // T piece
            };

            // Randomly select a piece from the pieceArray through random.Next()
            currentPieceInt = random.Next( 7 );
            for(int i = 0; i < 4; i++)
            {
                currentPiece[i] = pieceArray[currentPieceInt, i];
            }

            // Color each box in the currentPiece Array
            foreach(Control box in currentPiece)
            {
                box.BackColor = colorArray[currentPieceInt];
            }

            // I piece
            //BoxB2.BackColor = Color.CornflowerBlue;
            //BoxB3.BackColor = Color.CornflowerBlue;
            //BoxB4.BackColor = Color.CornflowerBlue;
            //BoxB5.BackColor = Color.CornflowerBlue;

            // O piece
            //BoxD2.BackColor = Color.Yellow;
            //BoxD3.BackColor = Color.Yellow;
            //BoxE2.BackColor = Color.Yellow;
            //BoxE3.BackColor = Color.Yellow;

            // S piece
            //BoxH2.BackColor = Color.Crimson;
            //BoxH3.BackColor = Color.Crimson;
            //BoxI2.BackColor = Color.Crimson;
            //BoxG3.BackColor = Color.Crimson;


            // Z piece
            //BoxK2.BackColor = Color.Magenta;
            //BoxL2.BackColor = Color.Magenta;
            //BoxL3.BackColor = Color.Magenta;
            //BoxM3.BackColor = Color.Magenta;

            // L piece
            //BoxB7.BackColor = Color.Cyan;
            //BoxB8.BackColor = Color.Cyan;
            //BoxB9.BackColor = Color.Cyan;
            //BoxC9.BackColor = Color.Cyan;

            // J piece
            //BoxF7.BackColor = Color.SpringGreen;
            //BoxF8.BackColor = Color.SpringGreen;
            //BoxF9.BackColor = Color.SpringGreen;
            //BoxE9.BackColor = Color.SpringGreen;

            // T piece
            //BoxH7.BackColor = Color.BlueViolet;
            //BoxH8.BackColor = Color.BlueViolet;
            //BoxH9.BackColor = Color.BlueViolet;
            //BoxI8.BackColor = Color.BlueViolet;
        }

        // Determines whether the move(right/left/down)/rotation(up) would put the piece outside the grid
        // or collide with another piece
        public bool TestMove(string direction)
        {
            int maxTopRow = 0;
            int maxBottomRow = 19;
            int maxLeftCol = 0;
            int maxRightCol = 15;

            int nextBox = 0;
            Control newBox = new Control();

            // Determine the outer bounds of the grid
            foreach(Control box in currentPiece)
            {
                if(grid.GetRow(box) > maxTopRow)
                {
                    maxTopRow = grid.GetRow(box);
                }
                if(grid.GetRow(box) < maxBottomRow)
                {
                    maxBottomRow = grid.GetRow(box);
                }
                if(grid.GetColumn(box) > maxLeftCol)
                {
                    maxLeftCol = grid.GetColumn(box);
                }
                if(grid.GetColumn(box) < maxRightCol)
                {
                    maxRightCol = grid.GetColumn(box);
                }
            }

            // Test if the move would put the piece out of bounds
            foreach(Control box in currentPiece)
            {
                int boxRow = grid.GetRow(box);
                int boxCol = grid.GetColumn(box);

                switch (direction)
                {
                    case "left":
                        if (boxCol > 0)
                        {
                            newBox = grid.GetControlFromPosition(boxCol - 1, boxRow);
                            nextBox = maxLeftCol;
                        }
                        else if(boxCol == 0)
                        {
                            // Out of bounds
                            return false;
                        }
                        break;

                    case "right":
                        if (boxCol < 15)
                        {
                            newBox = grid.GetControlFromPosition(boxCol + 1, boxRow);
                            nextBox = maxRightCol;
                        }
                        else if(boxCol == 15)
                        {
                            // Out of bounds
                            return false;
                        }
                        break;

                    // I think I might add a seperate function for rotating.
                    // I at least need to think som more on this one before I add it in.
                    //case "up":
                    //    if (nextSquare < maxLeftCol || nextSquare > maxRightCol || nextSquare < maxTopRow || nextSquare > maxBottomRow)
                    //    {
                    //        return false;
                    //    }
                    //    break;

                    case "down":
                        if (boxRow < 19)
                        {
                            newBox = grid.GetControlFromPosition(boxCol, boxRow + 1);
                            nextBox = maxBottomRow;
                        }
                        else if(boxRow == 19)
                        {
                            // Out of bounds
                            return false;
                        }
                        break;
                }

                if(newBox.BackColor != Color.DarkSlateGray & currentPiece.Contains(newBox) == false & nextBox > 0)
                {
                    // Collides with another piece
                    return false;
                }
            }

            // All tests passed
            return true;
        }

        // Erase the old position of the piece, and determine new position based on inputs 
        public void MovePiece(string direction)
        {
            int x = 0;
            foreach(PictureBox box in currentPiece)
            {
                box.BackColor = Color.DarkSlateGray;
                int boxRow = grid.GetRow(box);
                int boxCol = grid.GetColumn(box);
                int newBoxRow = 0;
                int newBoxCol = 0;

                switch (direction)
                {
                    case "left":
                        newBoxCol = boxCol - 1;
                        newBoxRow = boxRow;
                        break;

                    case "right":
                        newBoxCol = boxCol + 1;
                        newBoxRow = boxRow;
                        break;

                    //case "up":

                    //    break;

                    case "down":
                        newBoxCol = boxCol;
                        newBoxRow = boxRow + 1;
                        break;
                }

                movingPieceHolder[x] = grid.GetControlFromPosition(newBoxCol, newBoxRow);
                x++;
            }

            // Replace current piece's position with movingPieceHolder's
            x = 0;
            foreach (PictureBox box in movingPieceHolder)
            {
                currentPiece[x] = box;
                x++;
            }

            // Draw piece in new position
            x = 0;
            foreach(PictureBox box in movingPieceHolder)
            {
                box.BackColor = colorArray[currentPieceInt];
                x++;
            }

        }

        private void pieceTimer_Tick(object sender, EventArgs e)
        {
            if(TestMove("down") == true)
            {
                MovePiece("down");
            }
            else
            {
                pieceTimer.Stop();
                DropPiece();
            }
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {            
            timeLeft = timeLeft - new TimeSpan(0, 0, 1);
            countdownLabel.Text = timeLeft.ToString("mm\\:ss");
            
            if(timeLeft == TimeSpan.Zero)
            {
                gameTimer.Stop();
                MessageBox.Show("You scored: x", "Time's up!");
                Close();
            }
        }

        // Makes the startGameLabel visible/invisible every 0,4 second
        private void startLabelFlash_Tick(object sender, EventArgs e)
        {
            if(startGameLabel.ForeColor == Color.WhiteSmoke)
            {
                startGameLabel.ForeColor = Color.Black;
            }
            else
            {
                startGameLabel.ForeColor = Color.WhiteSmoke;
            }
        }

        // Starts the game
        // Hides and disables the button
        private void Form1_Enter(object sender, EventArgs e)
        {
            startLabelFlash.Stop();
            startGameLabel.Visible = false;
            gameTimer.Start();
            DropPiece();

        }
    }
}