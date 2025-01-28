using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace TetrisClone
{
    // --- New ToDo ---
    /*
    - Change it up so the piece starts dropping from above the table instead of creating it in the first
        few rows
    - Make rotations more adaptable, if it's too close to the edge, maybe i should just move each box in one column?
    - Find a way to save the user's high score
    - Add a label or something similar that explains the controls of the game, should disappear when the game starts.
    - Make a combo countdown timer to let the user get more combos (within a limited time)
    - Add music
        - Increase music speed in conjuction with the pieceTimer interval?
     */
    public partial class Form1 : Form
    {
        // Global variables
        TimeSpan timeLeft = new TimeSpan(0, 5, 0);
        Color[] colorArray = { Color.CornflowerBlue, Color.Yellow, Color.Crimson,
            Color.Magenta, Color.Cyan, Color.SpringGreen, Color.BlueViolet };
        Control[] currentPiece = { null, null, null, null };
        Control[] nextPiece = { null, null, null, null };
        Control[] movingPieceHolder = { null, null, null, null };
        Control[] ghostPiece = {null, null, null, null };
        Control[] savedPiece = { null, null, null, null };
        int currentPieceInt;
        int nextPieceInt;
        int savedPieceInt;
        int rotations = 0;
        int combo = 0;
        int score = 0;
        int bottom = 24;
        int pieceIterationCount = 0;
        int saveCounter = 0;
        bool gameOver = false;
        Random random = new Random();
        List<int> nextPieceList = new List<int>();


        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.grid.CellPaint += tableLayoutPanel1_CellPaint;
            startLabelFlash.Start();

            // Randomly select a int for the pieces through random.Next()
            // and add it to the nextPieceList.
            while (nextPieceList.Count < 5)
            {
                int n = random.Next(7);
                nextPieceList.Add(n);
            }

            // Select the first piece
            nextPieceInt = nextPieceList[0];
            pieceIterationCount++;
        }

        /// <summary>
        /// Draws a black border between the cells in the table. That's it.
        /// I couldn't find an easier way to set the color of the borders, so this is what I ended up doing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tableLayoutPanel1_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {
            e.Graphics.DrawRectangle(
                Pens.Black,
                e.CellBounds.Location.X-1,
                e.CellBounds.Location.Y-1,
                e.CellBounds.Width+1,
                e.CellBounds.Height+1);
        }

        /// <summary>
        /// Randomly generates and colors a tetris piece
        /// </summary>
        public void DropPiece()
        {
            // Resets the saveCounter
            saveCounter = 0;

            // Reset the rotations
            rotations = 0;

            // Layout option for each active pieace
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

            // Layout option for the nextPiece grid
            Control[,] nextPieceArray =
            {
                { boxNP31, boxNP32, boxNP33, boxNP34 }, // I piece
                { boxNP32, boxNP33, boxNP42, boxNP43 }, // O piece
                { boxNP32, boxNP33, boxNP42, boxNP23 }, // S piece
                { boxNP22, boxNP32, boxNP33, boxNP43 }, // Z piece
                { boxNP31, boxNP32, boxNP33, boxNP43 }, // L piece
                { boxNP41, boxNP42, boxNP43, boxNP33 }, // J piece
                { boxNP31, boxNP32, boxNP33, boxNP42 } // T piece
            };

            // Move the nextPiece to the current piece
            currentPieceInt = nextPieceInt;

            // If all pieces in the nextPieceList has been used
            if(pieceIterationCount == 5)
            {
                // Reset the counter
                pieceIterationCount = 0;
                // Clear the list
                nextPieceList.Clear();

                while (nextPieceList.Count < 5)
                {
                    int n = random.Next(7);
                    if (!nextPieceList.Contains(n))
                    {
                        nextPieceList.Add(n);
                    }
                }
            }
            
            // Select the next piece from the list
            nextPieceInt = nextPieceList[pieceIterationCount];
            pieceIterationCount++;

            // Clear the nextPieceGrid if it's not the first move
            if (nextPiece.Contains(null) == false)
            {
                foreach (Control box in nextPiece)
                {
                    box.BackColor = Color.LightSlateGray;
                }
            }

            // Get layout for the current piece
            for(int i = 0; i < 4; i++)
            {
                currentPiece[i] = pieceArray[currentPieceInt, i];
            }

            // Get layout for the next piece
            for (int i = 0; i < 4; i++)
            {
                nextPiece[i] = nextPieceArray[nextPieceInt, i];
            }

            // Check for game over
            foreach (Control box in currentPiece)
            {
                if(box.BackColor != Color.DarkSlateGray & box.BackColor != Color.SlateGray)
                {
                    pieceTimer.Stop();
                    gameTimer.Stop();
                    gameOver = true;
                    MessageBox.Show("No more space", "Game Over!");
                    Close();
                }
            }

            // Color each box in the currentPiece Array
            foreach(Control box in currentPiece)
            {
                box.BackColor = colorArray[currentPieceInt];
            }

            // Color each box in the nextPiece Array
            foreach (Control box in nextPiece)
            {
                box.BackColor = colorArray[nextPieceInt];
            }

            DrawGhostPiece();

            if (currentPieceInt == 0)
            {
                Console.WriteLine(rotations.ToString());
            }
        }

        /// <summary>
        /// Determines wheter the move (right/left/down) would put the piece outside the grid,
        /// or if it would collide with another piece
        /// </summary>
        /// <param name="direction"></param>
        /// <returns> true/ false </returns>
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

                if((newBox.BackColor != Color.DarkSlateGray & newBox.BackColor != Color.SlateGray) & currentPiece.Contains(newBox) == false & nextBox > 0)
                {
                    // Collides with another piece
                    return false;
                }
            }

            // All tests passed
            return true;
        }

        /// <summary>
        /// Erase the old position of the piece, and determine the new position based on the inputs(received as direction)
        /// This code will only run after TestMove() returns true
        /// </summary>
        /// <param name="direction"></param>
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

            if (ghostPiece != null)
            {
                foreach (PictureBox box in ghostPiece)
                {
                    box.BackColor = Color.DarkSlateGray;
                }
            }
            DrawGhostPiece();

            foreach(PictureBox box in ghostPiece)
            {
                if(currentPiece.Contains(box))
                {
                    box.BackColor = colorArray[currentPieceInt];
                }
            }

        }

        /// <summary>
        /// Checks if there are any complete rows
        /// </summary>
        /// <returns> null / the rowNumber of the full row</returns>
        public int CheckForCompleteRows()
        {
            // For each row (there are 25 rows)
            for (int y = 24; y >= 0; y--)
            {
                // For each column in a row (there are 16 columns)
                for (int x = 0; x <= 15; x++)
                {
                    Control checkedRow = grid.GetControlFromPosition(x, y);
                    if (checkedRow.BackColor == Color.DarkSlateGray)
                    {
                        break;
                    }
                    if (x == 15)
                    {
                        // Return the number of the full row
                        return y;
                    }
                }
            }
            // Return null
            return -1;
        }


        /// <summary>
        /// Clears full rows by turning the whole row of the rowNumber it gets from CheckForCompleteRows() into DarkSlateGray
        /// Then it moves all rows above the cleared row down one
        /// After that it checks if there are more than one full row, if there is it will add to the combo count,
        /// if not it will reset the combo to 0.
        /// Lastly, it updates the score
        /// </summary>
        public void ClearFullRows()
        {
            int completedRow = CheckForCompleteRows();

            // Turn the completed row back into DarkSlateGray
            for (int x = 0; x <= 15; x++)
            {
                Control checkedRow = grid.GetControlFromPosition(x, completedRow);
                checkedRow.BackColor = Color.DarkSlateGray;
            }

            // Move the inactive pieces above the completed row down
            for (int y = completedRow - 1; y >= 0; y--)
            {
                for (int x = 0; x <= 15; x++)
                {
                    // The current position of the box
                    Control currentBox = grid.GetControlFromPosition(x, y);
                    // The box below the current position
                    Control nextBox = grid.GetControlFromPosition(x, y + 1);

                    // "Move" it down by erasing and redrawing the backColor
                    nextBox.BackColor = currentBox.BackColor;
                    currentBox.BackColor = Color.DarkSlateGray;
                }
            }

            // Check if there are more than one full row
            if (CheckForCompleteRows() > -1)
            {
                ClearFullRows();
                combo++;
            }
            else
            {
                combo = 0;
            }

            // Update Score
            UpdateScore();
        }



        /// <summary>
        /// Updates the ScoreLabel
        /// The user gets a different amount of points depending on how many rows they clear at once
        /// Decreases the interval of the timer based on the user's score
        /// </summary>
        public void UpdateScore()
        {
            // No combo = 100 points
            if(combo == 0)
            {
                score += 100;
            }
            // One combo = 100 + extra 200 points
            else if(combo == 1)
            {
                score += 200;
            }
            // Two combos = 100 + extra 300 points
            else if(combo == 2)
            {
                score += 300;
            }
            // Three combos = 100 + extra 400 points
            else if(combo == 3)
            {
                score += 400;
            }
            // Four combos = 100 + extra 500 points
            else if( combo == 4)
            {
                score += 500;
            }
            // Five or more combos = 100 + extra 1000 points
            else if(combo >= 5)
            {
                score += 1000;
            }

            // Update score label
            scoreLabel.Text = score.ToString();

            // Change the interval of the piecetimer according to the score
            if(score >= 4000)
            {
                pieceTimer.Interval = 200;
            }
            else if(score >= 3000)
            {
                pieceTimer.Interval = 400;
            }
            else if(score >= 2000)
            {
                pieceTimer.Interval = 600;
            }
            else if(score >= 1000)
            {
                pieceTimer.Interval = 800;
            }
            else
            {
                pieceTimer.Interval = 1000;
            }
            
        }
        
        /// <summary>
        /// Checks for game over by checking if there are any colored boxes(that's not in the current piece) in the top row
        /// </summary>
        /// <returns> true/ false </returns>
        public bool CheckGameOver()
        {
            Control[] topRow = { BoxA1, BoxB1, BoxC1, BoxD1, BoxE1, BoxF1, BoxG1, BoxH1, 
                BoxI1 , BoxJ1, BoxK1, BoxL1, BoxM1, BoxN1, BoxO1, BoxP1 }; 
            foreach (Control box in topRow)
            {
                if((box.BackColor != Color.DarkSlateGray & box.BackColor != Color.SlateGray) & !currentPiece.Contains(box))
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

        /// <summary>
        /// Checks if the rotation would make the piece collide with another
        /// </summary>
        /// <returns> true / false </returns>
        public bool TestOverlap()
        {
            foreach (PictureBox box in movingPieceHolder)
            {
                if ((box.BackColor != Color.DarkSlateGray & box.BackColor != Color.SlateGray) & currentPiece.Contains(box) == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Rotates the piece
        /// It will check the position of the piece's boxes, then put the potential new location
        /// of the boxes in a seperate array which it uses to check if the move would be valid through TestOverlap().
        /// If it's valid, then it will replace the location of the boxes in the current piece's location with the tested ones
        /// </summary>
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
                if (rotations == 0)
                {
                    if (box1Col == 13)
                    {
                        movingPieceHolder[0] = grid.GetControlFromPosition(box1Col + 2, box1Row + 3);
                        movingPieceHolder[1] = grid.GetControlFromPosition(box2Col + 1, box2Row + 2);
                        movingPieceHolder[2] = grid.GetControlFromPosition(box3Col, box3Row + 1); 
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
                    else if (box1Col == 14)
                    {
                        movingPieceHolder[0] = grid.GetControlFromPosition(box1Col + 1, box1Row + 3);
                        movingPieceHolder[1] = grid.GetControlFromPosition(box2Col, box2Row + 2);
                        movingPieceHolder[2] = grid.GetControlFromPosition(box3Col - 1, box3Row + 1);
                        movingPieceHolder[3] = grid.GetControlFromPosition(box4Col - 2, box4Row);

                        if (TestOverlap() == true)
                        {
                            rotations++;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if (box1Col == 15)
                    {
                        movingPieceHolder[0] = grid.GetControlFromPosition(box1Col, box1Row + 3); 
                        movingPieceHolder[1] = grid.GetControlFromPosition(box2Col - 1, box2Row + 2); 
                        movingPieceHolder[2] = grid.GetControlFromPosition(box3Col - 2, box3Row + 1); 
                        movingPieceHolder[3] = grid.GetControlFromPosition(box4Col - 3, box4Row);

                        if (TestOverlap() == true)
                        {
                            rotations++;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
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
                }

                else if (rotations == 1)
                {
                    if (box1Row == 24)
                    {
                        return;
                    }
                    else
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
                if (rotations == 0)
                {
                    if (box3Row == 24)
                    {
                        return;
                    }
                    else
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
                }

                else if (rotations == 1)
                {
                    if (box3Col == 15)
                    {
                        return;
                    }
                    else
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
            }
            // Z piece
            else if(currentPieceInt == 3)
            {
                if(rotations == 0)
                {
                    if (box1Row == 23)
                    {
                        return;
                    }
                    else
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
                }
                
                else if(rotations == 1)
                {
                    if (box1Col == 0)
                    {
                        return;
                    }
                    else
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
            }
            // L piece
            else if(currentPieceInt == 4)
            {
                if(rotations == 0)
                {
                    if (box4Col == 15)
                    {
                        return;
                    }
                    else
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
                }

                else if(rotations == 1)
                {
                    if (box4Row == 24)
                    {
                        return;
                    }
                    else
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
                }

                else if(rotations == 2)
                {
                    if (box4Col == 0)
                    {
                        return;
                    }
                    else
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
                if(rotations == 0)
                {
                    if (box4Col == 14)
                    {
                        return;
                    }
                    else
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
                }

                else if(rotations == 1)
                {
                    if (box1Row == 24)
                    {
                        return;
                    }
                    else
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
                }

                else if(rotations == 2)
                {
                    if (box1Col == 0)
                    {
                        return;
                    }
                    else
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
                if(rotations == 0)
                {
                    if (box4Col == 15)
                    {
                        return;
                    }
                    else
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
                }
                
                else if(rotations == 1)
                {
                    if (box4Row == 24)
                    {
                        return;
                    }
                    else
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
                }

                else if(rotations == 2)
                {
                    if (box4Col == 0)
                    {
                        return;
                    }
                    else
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

        public bool TestGhostPiece()
        {
            foreach(PictureBox box in ghostPiece)
            {
                if(box.BackColor != Color.DarkSlateGray & currentPiece.Contains(box) == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Draws a ghost of the current piece at the lowest row it can go.
        /// It starts by checking the bottom row, and if that overlaps with another piece,
        /// then it checks the row above. It will draw a ghostPiece at the lowest valid row.
        /// </summary>
        public void DrawGhostPiece()
        {
            int rowDifference;

            int box1Col = grid.GetColumn(currentPiece[0]);
            int box1Row = grid.GetRow(currentPiece[0]);

            int box2Col = grid.GetColumn(currentPiece[1]);
            int box2Row = grid.GetRow(currentPiece[1]);

            int box3Col = grid.GetColumn(currentPiece[2]);
            int box3Row = grid.GetRow(currentPiece[2]);

            int box4Col = grid.GetColumn(currentPiece[3]);
            int box4Row = grid.GetRow(currentPiece[3]);

            if((box1Row >= box2Row) & (box1Row >= box3Row) & ( box1Row >= box4Row))
            {
                rowDifference = bottom - box1Row;
            }
            else if ((box2Row >= box1Row) & (box2Row >= box3Row) & (box2Row >= box4Row))
            {
                rowDifference = bottom - box2Row;
            }
            else if ((box3Row >= box1Row) & (box3Row >= box2Row) & (box3Row >= box4Row))
            {
                rowDifference = bottom - box3Row;
            }
            else
            {
                rowDifference = bottom - box4Row;
            }
            
            ghostPiece[0] = grid.GetControlFromPosition(box1Col, box1Row + rowDifference);
            ghostPiece[1] = grid.GetControlFromPosition(box2Col, box2Row + rowDifference);
            ghostPiece[2] = grid.GetControlFromPosition(box3Col, box3Row + rowDifference);
            ghostPiece[3] = grid.GetControlFromPosition(box4Col, box4Row + rowDifference);

            while(TestGhostPiece() == false)
            {
                bottom--;
                DrawGhostPiece();
                return;
            }

            if(TestGhostPiece() == true)
            {
                foreach(PictureBox box in ghostPiece)
                {
                    box.BackColor = Color.SlateGray;
                }
            }
            bottom = 24;
        }

        /// <summary>
        /// Drops the piece as far down as it can go.
        /// Since I've added a ghost piece, this function will simply "erase" the current piece,
        /// and "redraw it" by adding color to the ghost piece
        /// </summary>
        public void HardDrop()
        {
            foreach (PictureBox box in currentPiece)
            {
                box.BackColor = Color.DarkSlateGray;
            }

            int x = 0;
            foreach (PictureBox box in ghostPiece)
            {
                box.BackColor = colorArray[currentPieceInt];
                currentPiece[x] = box;
                x++;
            }
        }

        /// <summary>
        /// Saves the current piece and displays it in the savedPieceGrid.
        /// If there is no piece added, it will add the current piece to the display and erase it from the board.
        /// If there is a piece that's been saved, then it will switch them.
        /// </summary>
        public void SaveAPiece()
        {
            // Needs to save the current piece, erase it, and drop a new one if there is no piece saved
            // If there is a piece saved, then it should switch the saved piece and current piece

            // Layout option for each saved piece
            Control[,] savedPieceArray =
            {
                    { boxSP31, boxSP32, boxSP33, boxSP34 }, // I piece
                    { boxSP32, boxSP33, boxSP42, boxSP43 }, // O piece
                    { boxSP32, boxSP33, boxSP42, boxSP23 }, // S piece
                    { boxSP22, boxSP32, boxSP33, boxSP43 }, // Z piece
                    { boxSP31, boxSP32, boxSP33, boxSP43 }, // L piece
                    { boxSP41, boxSP42, boxSP43, boxSP33 }, // J piece
                    { boxSP31, boxSP32, boxSP33, boxSP42 } // T piece
                };

            // Layout option for each active pieace
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

            rotations = 0;

            // Prevents infinite swaps, only once per piece!
            if (saveCounter != 0)
            {
                Console.WriteLine("It's the same piece!");
                return;
            }
            else
            { 
                // Erase the current piece
                foreach (Control box in currentPiece)
                {
                    box.BackColor = Color.DarkSlateGray;
                }

                // Erase the ghost piece
                foreach (Control box in ghostPiece)
                {
                    box.BackColor = Color.DarkSlateGray;
                }

                // If no piece has been saved yet
                if (savedPiece.FirstOrDefault() == null)
                {
                    // Set the int to the currentPieceInt
                    savedPieceInt = currentPieceInt;
                    // Get the layout for the saved piece
                    for (int i = 0; i < 4; i++)
                    {
                        savedPiece[i] = savedPieceArray[savedPieceInt, i];
                    }

                    // Color each box in the savedPiece Array
                    foreach (Control box in savedPiece)
                    {
                        box.BackColor = colorArray[savedPieceInt];
                    }

                    Console.WriteLine($"first saved piece int {savedPieceInt}, and the counter is at {saveCounter}");

                    // Drop a new piece
                    DropPiece();

                    // Needs to go here since DropPiece() resets it
                    saveCounter++;
                }
                //If a piece has been saved
                else
                {
                    // Temporary int
                    int transferInt = currentPieceInt;

                    // Erase the saved piece
                    foreach (Control box in savedPiece)
                    {
                        box.BackColor = Color.LightSlateGray;
                    }

                    // Swap the pieces through swapping ints with the help of a temporary int for the transfer
                    currentPieceInt = savedPieceInt;
                    savedPieceInt = transferInt;

                    for (int i = 0; i < 4; i++)
                    {
                        savedPiece[i] = savedPieceArray[savedPieceInt, i];
                        currentPiece[i] = pieceArray[currentPieceInt, i];
                    }

                    // Color each box in the currentPiece Array
                    foreach (Control box in currentPiece)
                    {
                        box.BackColor = colorArray[currentPieceInt];
                    }

                    // Color each box in the savedPiece Array
                    foreach (Control box in savedPiece)
                    {
                        box.BackColor = colorArray[savedPieceInt];
                    }

                    saveCounter++;
                    Console.WriteLine($"save counter {saveCounter}");
                    Console.WriteLine("\n");

                    DrawGhostPiece();

                }
            }
        }

        /// <summary>
        /// A tick timer that moves the current piece down one row each tick, and drops a new piece
        /// once the current piece has gone as far down as it can go.
        /// It also checks for game over and completed rows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                    if(CheckForCompleteRows() > -1)
                    {
                        ClearFullRows();
                    }

                    // If the piece can't move down anymore, then it stops the timer and restarts the DropPiece function.
                    rotations = 0;
                    DropPiece();
                }
            }
        }

        /// <summary>
        /// The 5 minute timer I made for the game
        /// Decreases by one second at an interval of 1000 ticks (1 second)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                MessageBox.Show($"You scored: {score.ToString()}", "Time's up!");
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

        /// <summary>
        /// The event handler for keys that are pressed on the keyboard.
        /// Most keys do nothing until the game timer starts, at which point the enter key will be disabled
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    pieceTimer.Start();
                }
                else
                {
                    // Prevents any of the other keys from working until the game has started.
                    return;
                }
            }
            
            // Left
            if (CheckGameOver() == false & (e.KeyCode == Keys.Left || e.KeyCode == Keys.A) & (TestMove("left") == true))
            {
                MovePiece("left");
            }
            // Right
            else if (CheckGameOver() == false & (e.KeyCode == Keys.Right | e.KeyCode == Keys.D) & (TestMove("right") == true))
            {
                MovePiece("right");
            }
            // Down
            else if (CheckGameOver() == false & (e.KeyCode == Keys.Down | e.KeyCode == Keys.S) & (TestMove("down") == true))
            {
                MovePiece("down");
            }
            // Up
            else if(e.KeyCode == Keys.Up | e.KeyCode == Keys.W)
            {
                Rotate();
                if (ghostPiece != null)
                {
                    foreach (PictureBox box in ghostPiece)
                    {
                        box.BackColor = Color.DarkSlateGray;
                    }
                }
                DrawGhostPiece();
            }
            // Hard drop
            else if(CheckGameOver() == false & e.KeyCode == Keys.Space)
            {
                HardDrop();
            }
            else if(CheckGameOver() == false & e.KeyCode == Keys.ShiftKey)
            {
                SaveAPiece();
            }
            
        }

    }
}