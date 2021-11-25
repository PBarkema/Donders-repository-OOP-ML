using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WindowsFormsApplication4
{
    public partial class Form1 : Form
    {
        // Global var list
        int Game_Size = 6;
        int FiX = 6;
        int FiY = 1;
        int Square_Size = 50;
        // Red begins
        bool Turn_Bool = true;
        PictureBox ReversiBoard = new PictureBox();
        Bitmap Reversemap = new Bitmap(12 * 50, 12 * 50);
        int radius = 20;
        int X_Stone, Y_Stone;
        int[,] ActiveStones = new int[12, 12];
        Pen Play_Clr = new Pen(Color.White, 3);
        bool helper = false;
        // Make list of stones to color and empty after colouring 
        int[,] Take_Down = new int[12, 12];
        bool N,NE,E,SE,S,SW,W,NW;
        //int reach;


        public Form1()
        {
            int Mid = (Game_Size / 2)-1;
            // Initial four stones
            ActiveStones[Mid, Mid] = 1;
            ActiveStones[Mid + 1, Mid + 1] = 1;
            ActiveStones[Mid + 1, Mid] = -1;
            ActiveStones[Mid, Mid + 1] = -1;

            ReversiBoard.Paint += ReversiBoard_Paint;
            ReversiBoard.Click += ReversiBoard_Click;
            this.Controls.Add(ReversiBoard);
            InitializeComponent();
        }




        // Dropdown menu values (changing initial values)
        public void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (ComboGame.SelectedIndex == 0)
            {
                Game_Size = 6;
            }
            if (ComboGame.SelectedIndex == 1)
            {
                Game_Size = 8;
            }
            if (ComboGame.SelectedIndex == 2)
            {
                Game_Size = 12;
            }
            GameBox.Size = new Size(Game_Size * 50 + FiX, Game_Size * 50 + FiY);
        }

        public void ReversiBoard_Click(object sender, EventArgs e)
        { 
            int Ctr_X = 25 + FiX - radius, Ctr_Y = 25 - radius;
            Graphics g = Graphics.FromImage(Reversemap);
            Point MousePoint = this.PointToClient(new Point(MousePosition.X - ReversiBoard.Location.X, MousePosition.Y - ReversiBoard.Location.Y));
            ReversiBoard.Image = Reversemap;
            // Round down to determine square location (always divisible by 50)
            int DrawX = MousePoint.X - MousePoint.X % 50;
            int DrawY = MousePoint.Y - MousePoint.Y % 50;

            // Prevent DivideByZero and determine square
            if (DrawX == 0)
            {
                X_Stone = 0;
            }
            else {
                X_Stone = DrawX / 50;
            }

            if (DrawY == 0)
            {
                Y_Stone = 0;
            }
            else {
                Y_Stone = DrawY / 50;
            }

            //Draw color based on turn unlesss spot is taken
            if (ActiveStones[Y_Stone, X_Stone] == 0)
            {
                if (ValidateMove(X_Stone, Y_Stone) == true)
                {
                    if (Turn_Bool == true)
                    {
                        ActiveStones[Y_Stone, X_Stone] = -1;
                        Turn_Bool = false;
                        //foreach element in directiontruelist = -1;
                    }
                    else if (Turn_Bool == false)
                    {
                        ActiveStones[Y_Stone, X_Stone] = 1;
                        Turn_Bool = true;
                    }
                    helper = false;
                

                }
                else MessageBox.Show("This move is invalid.");
            }
            else MessageBox.Show("This place is already taken.");

            //ReversiBoard.Image = Reversemap;
            ReversiBoard.Invalidate();

        }
	
	# Validate the attempted move for each specific direction.
        public bool ValidateMove(int X_coord, int Y_coord)
        {

           //bool N= false, NE = false, E = false, SE = false, S = false, SW = false, W = false, NW = false;
            int My_Clr;
            Graphics g = Graphics.FromImage(Reversemap);
            if (Turn_Bool == false)
            {
                My_Clr = 1;
                Play_Clr = new Pen(Color.Blue, 3);
            }
            else {
                My_Clr = -1;
                Play_Clr = new Pen(Color.Red, 3);
            }
                int j;

            // For each direction, compute if the next stone is an enemy stone, and the stone after is friendly.
            for (j = 1; j < Game_Size - (Game_Size - Y_coord); j++)
            {
                if (ActiveStones[Y_coord - j, X_coord] * My_Clr == -1)
                {
                    if (ActiveStones[Y_coord - j - 1, X_coord] == My_Clr)
                    {
                        N = true;
                        return true;
                    }
                }
                else break;
            }

            // South Walker
            for (j=1; j < Game_Size - Y_coord; j++)
                {
                if (ActiveStones[Y_coord + j, X_coord] * My_Clr == -1)
                {
                    if (ActiveStones[Y_coord + j + 1, X_coord] == My_Clr)
                    {
                        S = true;
                        return true;
                    }

                }
                else break;
            }

            // West Walker
            for (j = 1; j < Game_Size - (Game_Size - X_coord); j++)
            {
                if (ActiveStones[Y_coord, X_coord - j] * My_Clr == -1)
                {
                    if (ActiveStones[Y_coord, X_coord - j - 1] == My_Clr)
                    {
                        W = true;
                        //ActiveStones[Y_coord, X_coord - j] = My_Clr;
                        //g.DrawEllipse(Play_Clr, 50*(X_coord- j) + 10, 50*Y_coord + 5, radius * 2, radius * 2);
                        return true;
                    }
                }
                else break;
            }

            // East Walker
            for (j = 1; j < Game_Size - X_coord; j++)
            {
                if (ActiveStones[Y_coord, X_coord + j] * My_Clr == -1)
                {
                    if (ActiveStones[Y_coord, X_coord + j + 1] == My_Clr)
                    {
                        E = true;
                        return true;
                    }
                }
                else break;
            }

            // North-West walker
            for (j = 1; j < X_coord && j < Y_coord; j++)
            {
                if (ActiveStones[Y_coord - j, X_coord - j] * My_Clr == -1)
                {
                    if (ActiveStones[Y_coord - j - 1, X_coord - j - 1] == My_Clr)
                    {
                        NW = true;
                        return true;
                    }
                }
                else break;
            }

            // South-East Walker
            for (j = 1; j < (Game_Size - X_coord) && j < (Game_Size - Y_coord); j++)
            {
                if (ActiveStones[Y_coord + j, X_coord + j] * My_Clr == -1)
                {
                    if (ActiveStones[Y_coord + j + 1, X_coord + j + 1] == My_Clr)
                    {
                        SE = true;
                        return true;
                    }

                }
                else break;
            }

            // South-West Walker
            for (j = 1; j < X_coord && j < (Game_Size - Y_coord); j++)
            {
                if (ActiveStones[Y_coord + j, X_coord - j] * My_Clr == -1)
                {
                    if (ActiveStones[Y_coord + j + 1, X_coord - j - 1] == My_Clr)
                    {
                        SW = true;
                        return true;
                    }

                }
                else break;
            }

            // North-East Walker
            for (j = 1; j < (Game_Size - X_coord) && j < Y_coord; j++)
            {
                if (ActiveStones[Y_coord - j, X_coord + j] * My_Clr == -1)
                {
                    if (ActiveStones[Y_coord - j - 1, X_coord + j + 1] == My_Clr)
                    {
                        NE = true;
                        return true;
                    }

                }
                else break;
            }
            if (N || W || E || S || NW || NE || SW || SE)
            {
                
            }
            return false;
        }

        // Help function that returns all valid stones.
        private void button1_Click(object sender, EventArgs e)
        {
            helper = !helper;
        }

        // Fill in a colour depending on the selected value
        private void ReversiBoard_Paint(object sender, PaintEventArgs e)
        {
            int Count_X, Count_Y, Rec_Width, Rec_Length;
            
            Count_Y = 0;
            Rec_Width = Rec_Length = 50;
            int Panel_Size = Game_Size * Square_Size;
            Graphics g = e.Graphics;


            Pen blackpen = new Pen(Color.Black, 1);
            ReversiBoard.Size = new Size(Panel_Size + FiX, Panel_Size + FiY);
            ReversiBoard.Location = new Point(86, 147);
            // Draw a line from left to right.

            int x=0, y=0;

            while (Count_Y < Panel_Size)
            {
                Count_X = 5;
                x = 0;
                while (Count_X < Panel_Size)
                {
                    g.DrawRectangle(blackpen, Count_X, Count_Y, Rec_Width, Rec_Length);
                    if (ActiveStones[y,x] == -1)
                    {
                        g.FillEllipse(Brushes.Red, Count_X, Count_Y, Rec_Width, Rec_Length);
                    }
                    else if (ActiveStones[y,x] == 1)
                    {
                        g.FillEllipse(Brushes.Blue, Count_X, Count_Y, Rec_Width, Rec_Length);
                    }
                    else if (ValidateMove(x, y) && helper)
                    {
                        g.FillEllipse(Brushes.LightGreen, Count_X, Count_Y, Rec_Width, Rec_Length);
                    }
                    // Circles tekenen/ kleur bepalen
                    Count_X = Count_X + Square_Size;
                    x++;
                }
                Count_Y = Count_Y + Square_Size;
                y++;
            }
            ReversiBoard.Image = Reversemap;
        }
        
    }
}
