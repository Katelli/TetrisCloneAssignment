using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;


namespace TetrisClone
{
    // --- New ToDo ---
    /*
    - Change it up so the piece starts dropping from above the table instead of creating it in the first
        few rows
    - Find a way to save the user's high score
     */
    public partial class Form1 : Form
    {
        // Global variables
        PlayerScoreList playerScoreList = new PlayerScoreList();
        string scorePath = "playerScores.json";
        string hiScPath = "top10Score.json";
        Dictionary<string, List<Player>> top10 = new Dictionary<string, List<Player>>();
        Dictionary<string, List<Player>> newTop10 = new Dictionary<string, List<Player>>();
        int scoreBoardPlace = 1;

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
        int bottom = 21;
        int pieceIterationCount = 0;
        int saveCounter = 0;
        bool gameOver = false;
        Random random = new Random();
        List<int> nextPieceList = new List<int>();
        WindowsMediaPlayer player = new WindowsMediaPlayer();
        WindowsMediaPlayer speedUpChime = new WindowsMediaPlayer();
        double previousN = 0;

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.grid.CellPaint += tableLayoutPanel1_CellPaint;
            startLabelFlash.Start();

            /* Credit for the music
            Yay by Sakura Girl | https://soundcloud.com/sakuragirl_official
            Music promoted by https://www.chosic.com/free-music/all/
            Creative Commons CC BY 3.0
            https://creativecommons.org/licenses/by/3.0/
             */
            player.URL = "Sakura-Girl-Yay.mp3";
            player.settings.setMode("Loop", true);
            player.settings.volume = 10;

            Volume.Image = imageList1.Images[0];

            
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

            // Hide top four rows
            for (int y = 0; y <= 3; y++)
            {
                for (int x = 0; x <= 11; x++)
                {
                    Control topRows = grid.GetControlFromPosition(x, y);
                    topRows.Hide();
                }
            }

            // Get playerScores from json file
            string existingScoreJson = File.ReadAllText(scorePath);
            if (!string.IsNullOrWhiteSpace(existingScoreJson))
            {
                playerScoreList.PlayerScore = JsonSerializer.Deserialize<List<Player>>(existingScoreJson);
            }

            // Get top10 from json file
            string existingTopJson = File.ReadAllText(hiScPath);
            if (!string.IsNullOrWhiteSpace(existingTopJson))
            {
                top10 = JsonSerializer.Deserialize<Dictionary<string, List<Player>>>(existingTopJson);
            }

            // Populate scoreBoard dataGrid with top10
            var scoreBoardList = top10.Select(b => new
            {
                Place = b.Key,
                PlayerName = string.Join(", ", b.Value.Select(a => a.PlayerName)),
                Score = string.Join(", ", b.Value.Select(a => a.Score.ToString()))
            }).ToList();

            scoreBoard.DataSource = scoreBoardList;

            //Display topScore
            var top1 = top10["1. Place"].Select(x => x.Score.ToString());
            topScore.Text = top1.FirstOrDefault();
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

            // ---- Here ----
            // Layout option for each active pieace
            Control[,] pieceArray =
            {
                { BoxF1, BoxF2, BoxF3, BoxF4 }, // I piece
                { BoxF3, BoxF4, BoxG3, BoxG4 }, // O piece
                { BoxF3, BoxF4, BoxG3, BoxE4 }, // S piece
                { BoxF3, BoxG3, BoxG4, BoxH4 }, // Z piece
                { BoxF2, BoxF3, BoxF4, BoxG4 }, // L piece
                { BoxG2, BoxG3, BoxG4, BoxF4 }, // J piece
                { BoxF2, BoxF3, BoxF4, BoxG3 }  // T piece
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
                    gameOver = true;
                    pieceTimer.Stop();
                    gameTimer.Stop();
                    MessageBox.Show($"No more space \n You scored: {score.ToString()}", "Game Over!");

                    PlayerScores();

                    return;
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
        }

        /// <summary>
        /// Checks if there are any complete rows
        /// </summary>
        /// <returns> null/the rowNumber of the full row </returns>
        public int CheckForCompleteRows()
        {
            // For each row (there are 25 rows)
            for (int y = 21; y >= 0; y--)
            {
                // For each column in a row (there are 16 columns)
                for (int x = 0; x <= 11; x++)
                {
                    Control checkedRow = grid.GetControlFromPosition(x, y);
                    if (checkedRow.BackColor == Color.DarkSlateGray)
                    {
                        break;
                    }
                    if (x == 11)
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
        /// Clears full rows by turning the whole row of the rowNumber it gets from CheckForCompleteRows() 
        /// into DarkSlateGray, then it moves all rows above the cleared row down
        /// After that it checks if there are more than one full row, if there is it will add to the combo count,
        /// if not it will reset the combo to 0.
        /// Lastly, it updates the score
        /// </summary>
        public void ClearFullRows()
        {
            int completedRow = CheckForCompleteRows();

            // Turn the completed row back into DarkSlateGray
            for (int x = 0; x <= 11; x++)
            {
                Control checkedRow = grid.GetControlFromPosition(x, completedRow);
                checkedRow.BackColor = Color.DarkSlateGray;
            }

            // Move the inactive pieces above the completed row down
            for (int y = completedRow - 1; y >= 0; y--)
            {
                for (int x = 0; x <= 11; x++)
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
            // If the comboTimer is running 
            else if(comboTimer.Enabled == true)
            {
                combo++;
            }
            // Reset the combo counter
            else
            {
                combo = 0;
            }

            // Starts the comboTimer
            comboTimer.Start();

            // Update Score
            UpdateScore();
        }

        /// <summary>
        /// Updates the ScoreLabel
        /// The user gets a different amount of points depending on the combo counter
        /// Decreases the interval of the pieceTimer based on the user's score
        /// </summary>
        public async void UpdateScore()
        {
            //  Five or more combos = 100 + extra 1000 points
            if(combo > 5)
            {
                score += 1000;
            }
            else if (combo == 5)
            {
                score += 500;
            }
            // Four combos = 500 points
            else if(combo == 4)
            {
                score += 400;
            }
            // Three combos = 400 points
            else if(combo == 3)
            {
                score += 300;
            }
            // Two combos = 300 points
            else if(combo == 2)
            {
                score += 200;
            }
            // One combo = 200 points
            else if( combo == 1)
            {
                score += 100;
            }
            // No combo = 100 points
            else if(combo == 0)
            {
                score += 50;
            }

            // Update score label
            scoreLabel.Text = score.ToString();

            double n;
            // Change the interval of the piecetimer according to the score
            if(score >= 4000)
            {
                pieceTimer.Interval = 200;
                n = 1.4; 
            }
            else if(score >= 3500)
            {
                pieceTimer.Interval = 300;
                n = 1.3;
            }
            else if(score >= 3000)
            {
                pieceTimer.Interval = 400;
                n = 1.3;
            }
            else if(score >= 2500)
            {
                pieceTimer.Interval = 500;
                n = 1.2;
            }
            else if(score >= 2000)
            {
                pieceTimer.Interval = 600;
                n = 1.2;
            }
            else if(score >= 1500)
            {
                pieceTimer.Interval = 700;
                n = 1.1;
            }
            else if(score >= 1000)
            {
                pieceTimer.Interval = 800;
                n = 1.1;
            }
            else if(score >= 500)
            {
                pieceTimer.Interval = 900;
                n = 1;
            }
            else
            {
                pieceTimer.Interval = 1000;
                n = 1;
            }

            await ChangeMusicSpeed(n);
            
        }
        
        /// <summary>
        /// Checks for game over by checking if there are any colored boxes(that's not in the current piece) in the top row
        /// </summary>
        /// <returns> true/false </returns>
        // ----- Here ------
        public bool CheckGameOver()
        {
            Control[] topRow = { BoxA4, BoxB4, BoxC4, BoxD4, BoxE4, BoxF4, BoxG4, BoxH4, 
                BoxI4 , BoxJ4, BoxK4, BoxL4}; 
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
        /// Changes the music speed through the variable it recieves for UpdateScore()
        /// Plays a soundeffect to signal the change
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private async Task ChangeMusicSpeed(double n)
        {
            if(previousN == 0)
            {
                previousN = n;
                return;
            }
            else if(previousN == n)
            {
                return;
            }
            else if (previousN != n)
            {
                previousN = n;
                player.controls.pause();

                /* Credit for the music
                Party-horn by PD music
                Music promoted by https://www.chosic.com/free-music/all/
                */
                speedUpChime.URL = "Party-horn.mp3";
                speedUpChime.settings.volume = 10;
                speedUpChime.controls.play();

                await Task.Delay(50);
                player.settings.rate = n;
                player.controls.play();
            }
        }

        private void PlayerScores()
        {
            if ((MessageBox.Show("Would you like to save your score?", "ScoreBoard", MessageBoxButtons.YesNo)) ==
                        DialogResult.Yes)
            {
                playerNamePopUp.Show();
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
                MessageBox.Show($"No more space \n You scored: {score.ToString()}", "Game Over!");

                PlayerScores();
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
                        MessageBox.Show($"No more space \n You scored: {score.ToString()}", "Game Over!");

                        PlayerScores();
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

                PlayerScores();
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

        private void comboTimer_Tick(object sender, EventArgs e)
        {
            /*
             Just writing down an idea I had if I want to somehow show how much time the user has until 
            the combo resets
            - First set the interval to 1000 ticks, as it's currently set to 5000 ticks
            int comboCountDown = 5
            comboCountDown --;
            if(comboCountDown == 0)
            {
                comboTimer.Stop()
            }
            else
            {
                comboCountDownLabel.Text = "combo resets in " + comboCountDown; 
            }

            - Maybe I should show how many combos the user has too if I do this?
             */
            comboTimer.Stop();
        }

        private void submitBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(playerNameBox.Text))
            {
                MessageBox.Show("Do you want to try again?", "No name was provided", MessageBoxButtons.YesNo);
                if (DialogResult == DialogResult.Yes)
                {
                    PlayerScores();
                }
                else if(DialogResult == DialogResult.No)
                {
                    if ((MessageBox.Show("Would you like to play again?", "Continue?", MessageBoxButtons.YesNo)) ==
                        DialogResult.Yes)
                    {
                        Application.Restart();
                    }
                    else
                    {
                        Close();
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(playerNameBox.Text))
            {
                string playerName = playerNameBox.Text;

                playerScoreList.CreatePSList(playerName, score);

                foreach (var playScore in playerScoreList.PlayerScore)
                {
                    playerScoreList.PlayerScore = playerScoreList.PlayerScore.OrderByDescending(i => i.Score).ToList();
                    var scoreJson = JsonSerializer.Serialize(playerScoreList.PlayerScore, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(scorePath, scoreJson);
                    Console.WriteLine(scoreJson);
                }

                for (int i = 0; i < 10; i++)
                {
                    newTop10.Add($"{scoreBoardPlace}. Place", playerScoreList.PlayerScore.GetRange(i, 1));
                    top10 = newTop10;
                    var topJson = JsonSerializer.Serialize(top10, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(hiScPath, topJson);
                    scoreBoardPlace++;
                }


                if ((MessageBox.Show("Would you like to play again?", "Continue?", MessageBoxButtons.YesNo)) ==
                            DialogResult.Yes)
                {
                    Application.Restart();
                }
                else
                {
                    Close();
                }
            }
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            if ((MessageBox.Show("Would you like to play again?", "Continue?", MessageBoxButtons.YesNo)) ==
                        DialogResult.Yes)
            {
                Application.Restart();
            }
            else
            {
                Close();
            }
        }

    }
}