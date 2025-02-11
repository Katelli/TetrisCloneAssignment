using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TetrisClone
{
    public partial class Form1 : Form
    {
        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (Control control in this.Controls)
            {
                control.PreviewKeyDown += new PreviewKeyDownEventHandler(control_PreviewKeyDown);
            }
        }

        private void control_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Up || e.KeyCode == Keys.Right || e.KeyCode == Keys.Down)
            {
                e.IsInputKey = true;
            }
        }

        private void Volume_CheckedChanged(object sender, EventArgs e)
        {
            if (Volume.Checked == false)
            {
                Volume.Image = imageList1.Images[0];
                player.settings.mute = false;
                speedUpChime.settings.mute = false;
            }
            else
            {
                Volume.Image = imageList1.Images[1];
                player.settings.mute = true;
                speedUpChime.settings.mute = true;
            }

            grid.Focus();
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
            foreach (Control box in currentPiece)
            {
                if (grid.GetRow(box) > currentTopRow)
                {
                    currentTopRow = grid.GetRow(box);
                }
                if (grid.GetRow(box) < currentBottomRow)
                {
                    currentBottomRow = grid.GetRow(box);
                }
                if (grid.GetColumn(box) > currentLeftCol)
                {
                    currentLeftCol = grid.GetColumn(box);
                }
                if (grid.GetColumn(box) < currentRightCol)
                {
                    currentRightCol = grid.GetColumn(box);
                }
            }

            // Test if the move would put the piece out of bounds
            foreach (Control box in currentPiece)
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
                        else if (boxCol == 0)
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
                        else if (boxCol == 15)
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
                        else if (boxRow == 24)
                        {
                            // Out of bounds
                            return false;
                        }
                        break;
                }

                if ((newBox.BackColor != Color.DarkSlateGray & newBox.BackColor != Color.SlateGray)
                    & currentPiece.Contains(newBox) == false & nextBox >= 0)
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
            foreach (PictureBox box in currentPiece)
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
            foreach (PictureBox box in movingPieceHolder)
            {
                box.BackColor = colorArray[currentPieceInt];
                x++;
            }

            if (ghostPiece != null)
            {
                foreach (PictureBox box in ghostPiece)
                {
                    while (box.BackColor == Color.SlateGray)
                    {
                        box.BackColor = Color.DarkSlateGray;
                    }
                }
            }
            DrawGhostPiece();

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

            // Prevents infinite swaps, only once per piece!
            if (saveCounter != 0)
            {
                Console.WriteLine("It's the same piece!");
                return;
            }
            else if (TestMove("down") == false)
            {
                return;
            }
            else
            {
                rotations = 0;

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
                        movingPieceHolder[0] = grid.GetControlFromPosition(box1Col - 1, box1Row); // P2 -> O2
                        movingPieceHolder[1] = grid.GetControlFromPosition(box2Col, box2Row + 1); // O2 -> O3
                        movingPieceHolder[2] = grid.GetControlFromPosition(box3Col, box3Row - 1); // P3 -> P2
                        movingPieceHolder[3] = grid.GetControlFromPosition(box4Col - 1, box4Row + 2); // O1 -> N3

                        if (TestOverlap() == true)
                        {
                            rotations = 0;
                        }
                        else
                        {
                            return;
                        }
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
            else if (currentPieceInt == 3)
            {
                if (rotations == 0)
                {
                    if (box3Row == 24)
                    {
                        movingPieceHolder[0] = grid.GetControlFromPosition(box1Col, box1Row + 1); // M4 -> M5
                        movingPieceHolder[1] = grid.GetControlFromPosition(box2Col - 1, box2Row); // N4 -> M4
                        movingPieceHolder[2] = grid.GetControlFromPosition(box3Col, box3Row - 1); // N5 -> N4
                        movingPieceHolder[3] = grid.GetControlFromPosition(box4Col - 1, box4Row - 2); // O5 -> N3

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

                else if (rotations == 1)
                {
                    if (box1Col == 0)
                    {
                        movingPieceHolder[0] = grid.GetControlFromPosition(box1Col, box1Row - 1);
                        movingPieceHolder[1] = grid.GetControlFromPosition(box2Col + 1, box2Row);
                        movingPieceHolder[2] = grid.GetControlFromPosition(box3Col, box3Row + 1);
                        movingPieceHolder[3] = grid.GetControlFromPosition(box4Col + 1, box4Row + 2);

                        if (TestOverlap() == true)
                        {
                            rotations = 0;
                        }
                        else
                        {
                            return;
                        }
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
            else if (currentPieceInt == 4)
            {
                if (rotations == 0)
                {
                    if (box4Col == 15)
                    {
                        movingPieceHolder[0] = grid.GetControlFromPosition(box1Col + 1, box1Row + 1);
                        movingPieceHolder[1] = grid.GetControlFromPosition(box2Col, box2Row);
                        movingPieceHolder[2] = grid.GetControlFromPosition(box3Col - 1, box3Row - 1);
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

                else if (rotations == 1)
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

                else if (rotations == 2)
                {
                    if (box4Col == 0)
                    {
                        movingPieceHolder[0] = grid.GetControlFromPosition(box1Col - 1, box1Row - 1);
                        movingPieceHolder[1] = grid.GetControlFromPosition(box2Col, box2Row);
                        movingPieceHolder[2] = grid.GetControlFromPosition(box3Col + 1, box3Row + 1);
                        movingPieceHolder[3] = grid.GetControlFromPosition(box4Col + 2, box4Row);

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
            else if (currentPieceInt == 5)
            {
                if (rotations == 0)
                {
                    if (box4Col == 14)
                    {
                        movingPieceHolder[0] = grid.GetControlFromPosition(box1Col, box1Row + 2);
                        movingPieceHolder[1] = grid.GetControlFromPosition(box2Col - 1, box2Row + 1);
                        movingPieceHolder[2] = grid.GetControlFromPosition(box3Col - 2, box3Row);
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

                else if (rotations == 1)
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

                else if (rotations == 2)
                {
                    if (box1Col == 0)
                    {
                        movingPieceHolder[0] = grid.GetControlFromPosition(box1Col, box1Row - 2);
                        movingPieceHolder[1] = grid.GetControlFromPosition(box2Col + 1, box2Row - 1);
                        movingPieceHolder[2] = grid.GetControlFromPosition(box3Col + 2, box3Row);
                        movingPieceHolder[3] = grid.GetControlFromPosition(box4Col + 1, box4Row + 1);

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
            else if (currentPieceInt == 6)
            {
                if (rotations == 0)
                {
                    if (box4Col == 15)
                    {
                        movingPieceHolder[0] = grid.GetControlFromPosition(box1Col + 1, box1Row + 1);
                        movingPieceHolder[1] = grid.GetControlFromPosition(box2Col, box2Row);
                        movingPieceHolder[2] = grid.GetControlFromPosition(box3Col - 1, box3Row - 1);
                        movingPieceHolder[3] = grid.GetControlFromPosition(box4Col - 1, box4Row + 1);

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

                else if (rotations == 1)
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

                else if (rotations == 2)
                {
                    if (box4Col == 0)
                    {
                        movingPieceHolder[0] = grid.GetControlFromPosition(box1Col - 1, box1Row - 1);
                        movingPieceHolder[1] = grid.GetControlFromPosition(box2Col, box2Row);
                        movingPieceHolder[2] = grid.GetControlFromPosition(box3Col + 1, box3Row + 1);
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

            foreach (PictureBox box in currentPiece)
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
            foreach (PictureBox box in ghostPiece)
            {
                if (box.BackColor != Color.DarkSlateGray & currentPiece.Contains(box) == false)
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
            // I should redo this so it starts the check at the piece's position, and go down from there
            // instead of from the bottom since it can show impossible positions as possible
            // Maybe make a while loop that adds +1 to the row as long as it's true? 
            int rowDifference;

            int box1Col = grid.GetColumn(currentPiece[0]);
            int box1Row = grid.GetRow(currentPiece[0]);

            int box2Col = grid.GetColumn(currentPiece[1]);
            int box2Row = grid.GetRow(currentPiece[1]);

            int box3Col = grid.GetColumn(currentPiece[2]);
            int box3Row = grid.GetRow(currentPiece[2]);

            int box4Col = grid.GetColumn(currentPiece[3]);
            int box4Row = grid.GetRow(currentPiece[3]);

            if ((box1Row >= box2Row) & (box1Row >= box3Row) & (box1Row >= box4Row))
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

            if (TestMove("down") == false)
            {
                return;
            }

            for (int i = 0; i < rowDifference + 1; i++)
            {
                if (TestMove("down") == true)
                {
                    ghostPiece[0] = grid.GetControlFromPosition(box1Col, box1Row + i);
                    ghostPiece[1] = grid.GetControlFromPosition(box2Col, box2Row + i);
                    ghostPiece[2] = grid.GetControlFromPosition(box3Col, box3Row + i);
                    ghostPiece[3] = grid.GetControlFromPosition(box4Col, box4Row + i);

                    if (TestGhostPiece() == false)
                    {
                        bottom--;
                        DrawGhostPiece();
                        return;
                    }
                }
            }

            foreach (PictureBox box in ghostPiece)
            {
                while (box.BackColor == Color.DarkSlateGray)
                {
                    box.BackColor = Color.SlateGray;
                }
            }
            
            bottom = 24;
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
                    ControllersImage.Visible = false;
                    gameTimer.Start();
                    DropPiece();
                    pieceTimer.Start();
                    grid.Focus();
                }
                else
                {
                    // Prevents any of the other keys from working until the game has started.
                    return;
                }
            }

            if(e.KeyCode == Keys.M)
            {
                if(Volume.Checked == true)
                {
                    Volume.Checked = false;
                }
                else
                {
                    Volume.Checked = true;
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
            else if (e.KeyCode == Keys.Up | e.KeyCode == Keys.W | e.KeyCode == Keys.Z)
            {
                Rotate();
                // Redraw Ghost piece
                foreach (PictureBox box in ghostPiece)
                {
                    while (box.BackColor == Color.SlateGray)
                    {
                        box.BackColor = Color.DarkSlateGray;
                    }
                }
                DrawGhostPiece();
            }
            // Hard drop
            else if (CheckGameOver() == false & e.KeyCode == Keys.Space & TestMove("down") == true)
            {
                HardDrop();
            }
            else if (CheckGameOver() == false & e.KeyCode == Keys.ShiftKey)
            {
                SaveAPiece();
            }

        }
    }
}
