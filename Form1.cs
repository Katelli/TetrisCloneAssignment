using System;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace TetrisClone
{
    public partial class Form1 : Form
    {
        // Global variables
        TimeSpan timeLeft = new TimeSpan(0, 5, 0);
        Color[] colorArray = { Color.CornflowerBlue, Color.Yellow, Color.Crimson,
            Color.Magenta, Color.Cyan, Color.SpringGreen, Color.BlueViolet };
        Control[] currentPiece = { null, null, null, null };
        Control[] movingPieceHolder = { null, null, null, null };
        int currentPieceInt;
        int rotations = 0;
        bool gameOver = false;
        Random random = new Random();

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.grid.CellPaint += tableLayoutPanel1_CellPaint;
            startLabelFlash.Start();
        }

        // Draws the black border between the cells.
        // I didn't like the default color, and there wasn't an easier way (that I found) to change it
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

            // Check for game over
            foreach(Control box in currentPiece)
            {
                if(box.BackColor != Color.DarkSlateGray)
                {
                    pieceTimer.Stop();
                    gameTimer.Stop();
                    gameOver = true;
                    MessageBox.Show("No more space", "Game Over!");
                    return;
                }
            }

            // Color each box in the currentPiece Array
            foreach(Control box in currentPiece)
            {
                box.BackColor = colorArray[currentPieceInt];
            }

            pieceTimer.Start();
        }

        // Determines whether the move(right/left/down)/rotation(up) would put the piece outside the grid
        // or collide with another piece
        public bool TestMove(string direction)
        {
            // The outer bounds of the grid
            int currentTopRow = 0;
            int currentBottomRow = 24;
            int currentLeftCol = 0;
            int currentRightCol = 15;

            int nextBox = 0;
            Control newBox = new Control();

            // Determine how high, low, left or right the piece can potentially move
            foreach(Control box in currentPiece)
            {
                if(grid.GetRow(box) > currentTopRow)
                {
                    currentTopRow = grid.GetRow(box);
                }
                if(grid.GetRow(box) < currentBottomRow)
                {
                    currentBottomRow = grid.GetRow(box);
                }
                if(grid.GetColumn(box) > currentLeftCol)
                {
                    currentLeftCol = grid.GetColumn(box);
                }
                if(grid.GetColumn(box) < currentRightCol)
                {
                    currentRightCol = grid.GetColumn(box);
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
                            nextBox = currentLeftCol;
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
                            nextBox = currentRightCol;
                        }
                        else if(boxCol == 15)
                        {
                            // Out of bounds
                            return false;
                        }
                        break;

                    case "down":
                        if (boxRow < 24)
                        {
                            newBox = grid.GetControlFromPosition(boxCol, boxRow + 1);
                            nextBox = currentBottomRow;
                        }
                        else if(boxRow == 24)
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
                // Erases the piece by replacing the background color with the grid's color
                box.BackColor = Color.DarkSlateGray;

                // Gets the current row and column of each box in the currentPiece
                int boxRow = grid.GetRow(box);
                int boxCol = grid.GetColumn(box);

                // Holds the new position of the boxes in the piece
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

        // ---------- ToDos -------------
        // - Check for complete rows and update them
        // - Update Score
        // - Drop piece as far as it can go

        // Check for game over
        public bool CheckGameOver()
        {
            Control[] topRow = { BoxA1, BoxB1, BoxC1, BoxD1, BoxE1, BoxF1, BoxG1, BoxH1, 
                BoxI1 , BoxJ1, BoxK1, BoxL1, BoxM1, BoxN1, BoxO1, BoxP1 }; 
            foreach (Control box in topRow)
            {
                if(box.BackColor != Color.DarkSlateGray & !currentPiece.Contains(box))
                {
                    // GameOver
                    return true;
                }
            }

            if(gameOver == true)
            {
                return true;
            }

            return false;
        }



        // Check if the rotation would make the currentPiece collide with another piece
        public bool TestOverlap()
        {
            foreach (PictureBox box in movingPieceHolder)
            {
                if (box.BackColor != Color.DarkSlateGray & currentPiece.Contains(box) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public void Rotate()
        {
            // Each piece consists of four boxes, this gets all four boxes' columns and rows
            int box1Col = grid.GetColumn(currentPiece[0]);
            int box1Row = grid.GetRow(currentPiece[0]);

            int box2Col = grid.GetColumn(currentPiece[1]);
            int box2Row = grid.GetRow(currentPiece[1]);

            int box3Col = grid.GetColumn(currentPiece[2]);
            int box3Row = grid.GetRow(currentPiece[2]);

            int box4Col = grid.GetColumn(currentPiece[3]);
            int box4Row = grid.GetRow(currentPiece[3]);

            // I piece
            if (currentPieceInt == 0)
            {
                // Test if the piece is too close to the edge of the board
                if(rotations == 0 & (box1Col == 13 | box1Col >= 13))
                {
                    return;
                }
                else if(rotations == 1 & box1Row == 24)
                {
                    return;
                }

                if(rotations == 0)
                {
                    movingPieceHolder[0] = grid.GetControlFromPosition(box1Col + 3, box1Row + 3);
                    movingPieceHolder[1] = grid.GetControlFromPosition(box2Col + 2, box2Row + 2);
                    movingPieceHolder[2] = grid.GetControlFromPosition(box3Col + 1, box3Row + 1);
                    movingPieceHolder[3] = grid.GetControlFromPosition(box4Col, box4Row);

                    if (TestOverlap() == true)
                    {
                        rotations++;
                    }
                    else
                    {
                        return;
                    }
                }
                else if(rotations == 1)
                {
                    movingPieceHolder[0] = grid.GetControlFromPosition(box1Col - 3, box1Row - 2); 
                    movingPieceHolder[1] = grid.GetControlFromPosition(box2Col - 2, box2Row - 1); 
                    movingPieceHolder[2] = grid.GetControlFromPosition(box3Col - 1, box3Row); 
                    movingPieceHolder[3] = grid.GetControlFromPosition(box4Col, box4Row + 1); 

                    if (TestOverlap() == true)
                    {
                        rotations = 0;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            // O piece
            else if (currentPieceInt == 1)
            {
                // Can't rotate
                return;
            }
            // S piece 
            else if (currentPieceInt == 2)
            {
                // Test if the piece is too close to the edge of the board
                if (rotations == 0 & box3Row == 24)
                {
                    return;
                }
                else if (rotations == 1 & box3Col == 15)
                {
                    return;
                }

                if (rotations == 0)
                {
                    movingPieceHolder[0] = grid.GetControlFromPosition(box1Col + 1, box1Row + 1);
                    movingPieceHolder[1] = grid.GetControlFromPosition(box2Col, box2Row);
                    movingPieceHolder[2] = grid.GetControlFromPosition(box3Col, box3Row + 2);
                    movingPieceHolder[3] = grid.GetControlFromPosition(box4Col + 1, box4Row - 1);

                    if (TestOverlap() == true)
                    {
                        rotations++;
                    }
                    else
                    {
                        return;
                    }
                }
                else if (rotations == 1)
                {
                    movingPieceHolder[0] = grid.GetControlFromPosition(box1Col, box1Row);
                    movingPieceHolder[1] = grid.GetControlFromPosition(box2Col + 1, box2Row + 1);
                    movingPieceHolder[2] = grid.GetControlFromPosition(box3Col + 1, box3Row - 1);
                    movingPieceHolder[3] = grid.GetControlFromPosition(box4Col, box4Row + 2);

                    if (TestOverlap() == true)
                    {
                        rotations = 0;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            // Z piece
            else if(currentPieceInt == 3)
            {
                // Test if the piece is too close to the edge of the board
                if (rotations == 0 & box1Row == 23)
                {
                    return;
                }
                else if (rotations == 1 & box1Col == 0)
                {
                    return;
                }

                if (rotations == 0)
                {
                    movingPieceHolder[0] = grid.GetControlFromPosition(box1Col, box1Row + 2); 
                    movingPieceHolder[1] = grid.GetControlFromPosition(box2Col - 1, box2Row + 1);
                    movingPieceHolder[2] = grid.GetControlFromPosition(box3Col, box3Row);
                    movingPieceHolder[3] = grid.GetControlFromPosition(box4Col - 1, box4Row - 1);

                    if (TestOverlap() == true)
                    {
                        rotations++;
                    }
                    else
                    {
                        return;
                    }
                }
                else if (rotations == 1)
                {
                    movingPieceHolder[0] = grid.GetControlFromPosition(box1Col - 1, box1Row - 1); 
                    movingPieceHolder[1] = grid.GetControlFromPosition(box2Col, box2Row); 
                    movingPieceHolder[2] = grid.GetControlFromPosition(box3Col - 1, box3Row + 1); 
                    movingPieceHolder[3] = grid.GetControlFromPosition(box4Col, box4Row + 2);

                    if (TestOverlap() == true)
                    {
                        rotations = 0;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            // L piece
            else if(currentPieceInt == 4)
            {
                // Test if the piece is too close to the edge of the board
                if (rotations == 0 & box4Col == 15)
                {
                    return;
                }
                else if (rotations == 1 & box4Row == 24)
                {
                    return;
                }
                else if (rotations == 2 & box4Col == 0)
                {
                    return;
                }

                if (rotations == 0)
                {
                    movingPieceHolder[0] = grid.GetControlFromPosition(box1Col + 2, box1Row + 1);
                    movingPieceHolder[1] = grid.GetControlFromPosition(box2Col + 1, box2Row); 
                    movingPieceHolder[2] = grid.GetControlFromPosition(box3Col, box3Row - 1); 
                    movingPieceHolder[3] = grid.GetControlFromPosition(box4Col - 1, box4Row); 

                    if (TestOverlap() == true)
                    {
                        rotations++;
                    }
                    else
                    {
                        return;
                    }
                }
                else if (rotations == 1)
                {
                    movingPieceHolder[0] = grid.GetControlFromPosition(box1Col - 1, box1Row + 2);
                    movingPieceHolder[1] = grid.GetControlFromPosition(box2Col, box2Row + 1); 
                    movingPieceHolder[2] = grid.GetControlFromPosition(box3Col + 1, box3Row);
                    movingPieceHolder[3] = grid.GetControlFromPosition(box4Col, box4Row - 1);

                    if (TestOverlap() == true)
                    {
                        rotations++;
                    }
                    else
                    {
                        return;
                    }
                }
                else if (rotations == 2)
                {
                    movingPieceHolder[0] = grid.GetControlFromPosition(box1Col - 2, box1Row - 1); 
                    movingPieceHolder[1] = grid.GetControlFromPosition(box2Col - 1, box2Row); 
                    movingPieceHolder[2] = grid.GetControlFromPosition(box3Col, box3Row + 1); 
                    movingPieceHolder[3] = grid.GetControlFromPosition(box4Col + 1, box4Row); 

                    if (TestOverlap() == true)
                    {
                        rotations++;
                    }
                    else
                    {
                        return;
                    }
                }
                else if (rotations == 3)
                {
                    movingPieceHolder[0] = grid.GetControlFromPosition(box1Col + 1, box1Row - 1); 
                    movingPieceHolder[1] = grid.GetControlFromPosition(box2Col, box2Row); 
                    movingPieceHolder[2] = grid.GetControlFromPosition(box3Col - 1, box3Row + 1); 
                    movingPieceHolder[3] = grid.GetControlFromPosition(box4Col, box4Row + 2); 

                    if (TestOverlap() == true)
                    {
                        rotations = 0;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            // J piece
            else if(currentPieceInt == 5)
            {
                // Test if the piece is too close to the edge of the board
                if (rotations == 0 & box4Col == 14)
                {
                    return;
                }
                else if (rotations == 1 & box1Row == 24)
                {
                    return;
                }
                else if (rotations == 2 & box1Col == 0)
                {
                    return;
                }

                if (rotations == 0)
                {
                    movingPieceHolder[0] = grid.GetControlFromPosition(box1Col + 1, box1Row + 2); 
                    movingPieceHolder[1] = grid.GetControlFromPosition(box2Col, box2Row + 1); 
                    movingPieceHolder[2] = grid.GetControlFromPosition(box3Col - 1, box3Row); 
                    movingPieceHolder[3] = grid.GetControlFromPosition(box4Col, box4Row - 1); 

                    if (TestOverlap() == true)
                    {
                        rotations++;
                    }
                    else
                    {
                        return;
                    }
                }
                else if (rotations == 1)
                {
                    movingPieceHolder[0] = grid.GetControlFromPosition(box1Col - 2, box1Row + 1); 
                    movingPieceHolder[1] = grid.GetControlFromPosition(box2Col - 1, box2Row); 
                    movingPieceHolder[2] = grid.GetControlFromPosition(box3Col, box3Row - 1);
                    movingPieceHolder[3] = grid.GetControlFromPosition(box4Col + 1, box4Row); 

                    if (TestOverlap() == true)
                    {
                        rotations++;
                    }
                    else
                    {
                        return;
                    }
                }
                else if (rotations == 2)
                {
                    movingPieceHolder[0] = grid.GetControlFromPosition(box1Col - 1, box1Row - 2); 
                    movingPieceHolder[1] = grid.GetControlFromPosition(box2Col, box2Row - 1); 
                    movingPieceHolder[2] = grid.GetControlFromPosition(box3Col + 1, box3Row); 
                    movingPieceHolder[3] = grid.GetControlFromPosition(box4Col, box4Row + 1); 

                    if (TestOverlap() == true)
                    {
                        rotations++;
                    }
                    else
                    {
                        return;
                    }
                }
                else if (rotations == 3)
                {
                    movingPieceHolder[0] = grid.GetControlFromPosition(box1Col + 2, box1Row); 
                    movingPieceHolder[1] = grid.GetControlFromPosition(box2Col + 1, box2Row + 1); 
                    movingPieceHolder[2] = grid.GetControlFromPosition(box3Col, box3Row + 2); 
                    movingPieceHolder[3] = grid.GetControlFromPosition(box4Col - 1, box4Row + 1); 

                    if (TestOverlap() == true)
                    {
                        rotations = 0;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            // T piece
            else if(currentPieceInt == 6)
            {
                // Test if the piece is too close to the edge of the board
                if (rotations == 0 & box4Col == 15)
                {
                    return;
                }
                else if (rotations == 1 & box4Row == 24)
                {
                    return;
                }
                else if (rotations == 2 & box4Col == 0)
                {
                    return;
                }

                if (rotations == 0)
                {
                    movingPieceHolder[0] = grid.GetControlFromPosition(box1Col + 2, box1Row + 1); 
                    movingPieceHolder[1] = grid.GetControlFromPosition(box2Col + 1, box2Row); 
                    movingPieceHolder[2] = grid.GetControlFromPosition(box3Col, box3Row - 1); 
                    movingPieceHolder[3] = grid.GetControlFromPosition(box4Col, box4Row + 1); 

                    if (TestOverlap() == true)
                    {
                        rotations++;
                    }
                    else
                    {
                        return;
                    }
                }
                else if (rotations == 1)
                {
                    movingPieceHolder[0] = grid.GetControlFromPosition(box1Col - 1, box1Row + 2); 
                    movingPieceHolder[1] = grid.GetControlFromPosition(box2Col, box2Row + 1); 
                    movingPieceHolder[2] = grid.GetControlFromPosition(box3Col + 1, box3Row); 
                    movingPieceHolder[3] = grid.GetControlFromPosition(box4Col - 1, box4Row); 

                    if (TestOverlap() == true)
                    {
                        rotations++;
                    }
                    else
                    {
                        return;
                    }
                }
                else if (rotations == 2)
                {
                    movingPieceHolder[0] = grid.GetControlFromPosition(box1Col - 2, box1Row - 1); 
                    movingPieceHolder[1] = grid.GetControlFromPosition(box2Col - 1, box2Row);
                    movingPieceHolder[2] = grid.GetControlFromPosition(box3Col, box3Row + 1); 
                    movingPieceHolder[3] = grid.GetControlFromPosition(box4Col, box4Row - 1); 

                    if (TestOverlap() == true)
                    {
                        rotations++;
                    }
                    else
                    {
                        return;
                    }
                }
                else if (rotations == 3)
                {
                    movingPieceHolder[0] = grid.GetControlFromPosition(box1Col + 1, box1Row - 1); 
                    movingPieceHolder[1] = grid.GetControlFromPosition(box2Col, box2Row); 
                    movingPieceHolder[2] = grid.GetControlFromPosition(box3Col - 1, box3Row + 1); 
                    movingPieceHolder[3] = grid.GetControlFromPosition(box4Col + 1, box4Row + 1); 

                    if (TestOverlap() == true)
                    {
                        rotations = 0;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            foreach(PictureBox box in currentPiece)
            {
                box.BackColor = Color.DarkSlateGray;
            }

            // Replace current piece's position with movingPieceHolder's
            int x = 0;
            foreach (PictureBox box in movingPieceHolder)
            {
                box.BackColor = colorArray[currentPieceInt];
                currentPiece[x] = box;
                x++;
            }
        }

        // Moves the current piece down one row every second
        private void pieceTimer_Tick(object sender, EventArgs e)
        {
            // Check for game over
            if (gameOver == true)
            {
                pieceTimer.Stop();
                gameTimer.Stop();
                MessageBox.Show("No more space", "Game Over!");
                Close();
            }
            else
            {
                // Check if the current piece can move down
                if (TestMove("down") == true)
                {
                    MovePiece("down");
                }
                else
                {
                    if (gameOver == true)
                    {
                        pieceTimer.Stop();
                        gameTimer.Stop();
                        MessageBox.Show("No more space", "Game Over!");
                        Close();
                    }

                    // If the piece can't move down anymore, then it stops the timer and restarts the DropPiece function.
                    pieceTimer.Stop();
                    rotations = 0;
                    DropPiece();
                }
            }
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {            
            // Decrease the time left by 1 second and update the displayed countdown 
            timeLeft = timeLeft - new TimeSpan(0, 0, 1);
            countdownLabel.Text = timeLeft.ToString("mm\\:ss");
            
            // Stops the game if there is zero seconds left
            if(timeLeft == TimeSpan.Zero)
            {
                gameTimer.Stop();
                pieceTimer.Stop();
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

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Prevents DropPiece() from starting again if the timer is already running
            if (gameTimer.Enabled == false)
            {
                // Starts the game
                // Hides and disables the flashing startLabel
                if (e.KeyCode == Keys.Enter)
                {
                    startLabelFlash.Stop();
                    startGameLabel.Visible = false;
                    gameTimer.Start();
                    DropPiece();
                }
            }
            
            // Left
            if ((e.KeyCode == Keys.Left || e.KeyCode == Keys.A) && (TestMove("left") == true))
            {
                MovePiece("left");
            }
            // Right
            else if ((e.KeyCode == Keys.Right | e.KeyCode == Keys.D) & (TestMove("right") == true))
            {
                MovePiece("right");
            }
            // Up
            else if(e.KeyCode == Keys.Up | e.KeyCode == Keys.W)
            {
                Rotate();
            }
            // Down
            else if ((e.KeyCode == Keys.Down | e.KeyCode == Keys.S) & (TestMove("down") == true))
            {
                MovePiece("down");
            }
        }

    }
}