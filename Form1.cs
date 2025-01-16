using System;
using System.Drawing;
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

        public Form1()
        {
            InitializeComponent();
            this.tableLayoutPanel1.CellPaint += tableLayoutPanel1_CellPaint;
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
            gameTimer.Start();
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

        private void pieceTimer_Tick(object sender, EventArgs e)
        {

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
