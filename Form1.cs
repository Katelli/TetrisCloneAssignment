using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TetrisClone
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.tableLayoutPanel1.CellPaint += tableLayoutPanel1_CellPaint;
            DropPiece();
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
            // I piece
            //BoxH1.BackColor = Color.CornflowerBlue;
            //BoxH2.BackColor = Color.CornflowerBlue;
            //BoxH3.BackColor = Color.CornflowerBlue;
            //BoxH4.BackColor = Color.CornflowerBlue;
            BoxA1.BackColor = Color.CornflowerBlue;
            BoxA2.BackColor = Color.CornflowerBlue;
            BoxA3.BackColor = Color.CornflowerBlue;
            BoxA4.BackColor = Color.CornflowerBlue;

            // O piece
            //BoxH1.BackColor = Color.Yellow;
            //BoxH2.BackColor = Color.Yellow;
            //BoxI1.BackColor = Color.Yellow;
            //BoxI2.BackColor = Color.Yellow;
            BoxC1.BackColor = Color.Yellow;
            BoxC2.BackColor = Color.Yellow;
            BoxD1.BackColor = Color.Yellow;
            BoxD2.BackColor = Color.Yellow;

            // S piece
            //BoxH1.BackColor = Color.Crimson;
            //BoxH2.BackColor = Color.Crimson;
            //BoxI1.BackColor = Color.Crimson;
            //BoxG2.BackColor = Color.Crimson;
            BoxG1.BackColor = Color.Crimson;
            BoxG2.BackColor = Color.Crimson;
            BoxH1.BackColor = Color.Crimson;
            BoxF2.BackColor = Color.Crimson;


            // Z piece
            //BoxH1.BackColor = Color.Magenta;
            //BoxI1.BackColor = Color.Magenta;
            //BoxI2.BackColor = Color.Magenta;
            //BoxJ2.BackColor = Color.Magenta;
            BoxJ1.BackColor = Color.Magenta;
            BoxK1.BackColor = Color.Magenta;
            BoxK2.BackColor = Color.Magenta;
            BoxL2.BackColor = Color.Magenta;

            // L piece
            //BoxH1.BackColor = Color.Cyan;
            //BoxH2.BackColor = Color.Cyan;
            //BoxH3.BackColor = Color.Cyan;
            //BoxI3.BackColor = Color.Cyan;
            BoxN1.BackColor = Color.Cyan;
            BoxN2.BackColor = Color.Cyan;
            BoxN3.BackColor = Color.Cyan;
            BoxO3.BackColor = Color.Cyan;

            // J piece
            //BoxI1.BackColor = Color.SpringGreen;
            //BoxI2.BackColor = Color.SpringGreen;
            //BoxI3.BackColor = Color.SpringGreen;
            //BoxH3.BackColor = Color.SpringGreen;
            BoxB6.BackColor = Color.SpringGreen;
            BoxB7.BackColor = Color.SpringGreen;
            BoxB8.BackColor = Color.SpringGreen;
            BoxA8.BackColor = Color.SpringGreen;

            // T piece
            //BoxH1.BackColor = Color.BlueViolet;
            //BoxH2.BackColor = Color.BlueViolet;
            //BoxH3.BackColor = Color.BlueViolet;
            //BoxI2.BackColor = Color.BlueViolet;
            BoxD6.BackColor = Color.BlueViolet;
            BoxD7.BackColor = Color.BlueViolet;
            BoxD8.BackColor = Color.BlueViolet;
            BoxE7.BackColor = Color.BlueViolet;
        }
    }
}
