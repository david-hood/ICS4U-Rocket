using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace FinalProjectDavid
{
    public partial class Form1 : Form
    {
        //Rotation Variables
        int px = 500, py = 400; //Set the position of the rocket
        int sx = 250, sy = 250; //Set the size of the rocket
        int angleVel;//Angular velocity
        float angle; //Angle of rocket
        double autoAngle;

        //Movememnt Variables
        double upVel, rightVel, comVel, autoUpVel, comAcc, altitude;
        //double CoD = 0.21; //Coefficient of drag
        double grav = 9.81; //Gravity
        double thrust = 700;
        double fuel = 500;
        double dryMass = 500;
        double isp = 320; //specific impulse (efficiency)
        //double twr; //thrust to weight ratio
        bool landed = true, autoRotate = true;

        //Control variables
        int rotate = 0, thrustSetting = 0;
        double throttle = 0;

        //Sprites
        Image spriteFlame = Properties.Resources.FlameSpriteSheet;
        Image spriteExplosion = Properties.Resources.explosion;
        Rectangle rSprite;
        Rectangle rSprite2;
        int count, row, col, totalRows, totalCols, spriteWidth, spriteHeight;
        int count2, row2, col2, totalRows2, totalCols2, spriteWidth2, spriteHeight2;
        bool explode = false;

        //Objects
        Rectangle rGround = new Rectangle(-492, 525, 1963, 492);
        Rectangle rThrust = new Rectangle(100, 100, 20, 100);
        Bitmap bShip = Properties.Resources.PlaceholderRocket;
        Pen ThrottlePen = new Pen(Brushes.White, 3);

        //Misc
        Random rnd = new Random();
        int SkyRed = 100;
        int SkyGreen = 180;
        int SkyBlue = 255, SkyBlue2 = 255;
        int colourAlt;
        int mapGroundHeight, mapGroundHeight2;
        int mapShipHeight;
        String SOI = "Earth";

        //Ship customization
        bool designer = true;
        int top, tank = 1, engine;


        //Designer Buttons
        Rectangle rTop1 = new Rectangle(43, 65, 74, 146);
        Rectangle rTop2 = new Rectangle(227, 57, 78, 154);

        Rectangle rTank1 = new Rectangle(93, 280, 77, 140);
        Rectangle rTank2 = new Rectangle(180, 280, 77, 140);
        Rectangle rTank3 = new Rectangle(0, 280, 83, 140);
        Rectangle rTank4 = new Rectangle(267, 280, 83, 140);

        Rectangle rEngine1 = new Rectangle(55, 480, 50, 23);
        Rectangle rEngine2 = new Rectangle(240, 480, 50, 23);
        Rectangle rEngine3 = new Rectangle(55, 620, 50, 13);
        Rectangle rEngine4 = new Rectangle(240, 615, 50, 23);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Define properties of sprite sheets
            totalCols = 23;
            totalRows = 1;
            totalCols2 = 9;
            totalRows2 = 9;

            //Define the size of each sprite
            spriteWidth = spriteFlame.Width / totalCols;
            spriteHeight = spriteFlame.Height / totalRows;
            spriteWidth2 = spriteExplosion.Width / totalCols2;
            spriteHeight2 = spriteExplosion.Height / totalRows2;

            //Set up rectangles
            rSprite = new Rectangle(-25, 133, spriteWidth, spriteHeight);
            rSprite2 = new Rectangle(0, 0, spriteWidth2, spriteHeight2);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (rotate == 1 && angleVel > -50)
                angleVel -= 3; //Rotate right
            else if (rotate == 2 && angleVel < 50)
                angleVel += 3; //Rotate left
            pbArena.Invalidate(); //Refresh
            float angleFloat = Convert.ToSingle(angleVel); //Keep everything in integer form until it is sent to the drawing code
            angle += angleFloat / 50; //ints are too big so divide by 50
            if (angle > 180)
                angle -= 360;
            if (angle < -180)
                angle += 360;
            if (angleVel == 0 && ((angle < 1 && angle > 0) || (angle > -1 && angle < 0)))
                angle = 0; //Set rocket straight if it is almost straight (fixes messy float math)
            if (angleVel > 0)
                angleVel -= 1; //Return angleVel to 0 over time
            if (angleVel < 0)
                angleVel += 1;
            //Console.WriteLine("Angle (float) = " + angle); //Debug
            //Console.WriteLine("AngleVel (double) = " + angleVel);
            //Console.WriteLine("AngleVel (float) = " + angleFloat);

            if (thrustSetting == 1 && throttle < 100)
                throttle += 3;
            if (thrustSetting == 2 && throttle > 0)
                throttle -= 3;
            if (throttle > 100)
                throttle = 100;
            if (throttle < 0)
                throttle = 0;
            rThrust.Height = Convert.ToInt32(throttle);
            rThrust.Y = 200 - Convert.ToInt32(throttle);
            //Console.WriteLine("Throttle: " + throttle);
            //Console.WriteLine("Thrust setting: " + thrustSetting);
            if (fuel > 0)
                comAcc = ((thrust * (throttle / 100)) / (fuel + dryMass)); //Current change in rocket's velocity
            else
            {
                comAcc = 0; //This code runs if the rocket runs out of fuel
                throttle = 0;
            }
            //Console.WriteLine("Velocity: " + upVel + "m/s");
            //Console.WriteLine("Altitude: " + altitude + "M");


            double angleRad = angle * (Math.PI / 180); //Convert angle of rocket from degrees to radians.

            if (landed == false)
            {
                rightVel += comAcc * Math.Sin(angleRad); //Since the rocket is vertical at 0 degrees, use sine to find the x component

                upVel += comAcc * Math.Cos(angleRad);//Use cosine to find the y component of the rocket's velocity

                upVel -= grav / 30; //Gravity

                altitude += upVel / 30; //There are about 30 ticks of the timer per second, so divide by 30 for m & m/s
            }
            fuel -= ((0.1 * thrust * (throttle / 1000)) / isp); //My formula to calculate fuel drain. Takes into account throttle %, engine thrust, & engine efficiency


            comVel = Math.Sqrt(upVel * upVel + rightVel * rightVel); //Use pythagorean theorm to find combined velocity using up/right vectors

            //Console.WriteLine("Fuel: " + fuel);

            double x = altitude / 1000;
            double y = 0;

            if (x > 0)
                y = Math.Sqrt(x) * 10; //Create square root function. Altitude in, y out.

            if (y > 100) //Keep y at or below 100
                y = 100;

            if (y < 1) //Keep y above 0
                y = 1;

            colourAlt = 100 - Convert.ToInt32(y); //convert y to colourAlt

            //reduce sky brightness as a function of colourAlt
            if (SOI == "Earth")
                pbArena.BackColor = Color.FromArgb(SkyRed, SkyGreen, SkyBlue2);
            if (colourAlt <= 100)
            {
                SkyRed = colourAlt;
                SkyGreen = colourAlt + colourAlt / 3;
                if (SkyGreen < 128)
                    SkyBlue = SkyGreen * 2;
                SkyBlue2 = SkyBlue;
            }

            //Ground contact and landing
            if (altitude < 0 && upVel < 0 && angle < 5 && angle > -5 && rightVel < 5 && rightVel > -5 && upVel > -50 && autoRotate == true) //&&upVel>-10
                landed = true;
            if (landed == true)
            {
                upVel = 0;
                rightVel = 0;
                altitude = 0;
                angle = 0;
                angleVel = 0;
            }
            else if (altitude < 0 && upVel < 0)
            {
                explode = true; //change to true when not debugging
                upVel = 0;
                rightVel = 0;
                grav = 0;
                throttle = 0;
                angleVel = 0;
            }

            if (landed == true && (comAcc * Math.Cos(angleRad)) > grav / 30) //If the thrust exceeds gravity, rocket is not landed
            {
                landed = false;
            }

            rGround.Y = Convert.ToInt32(altitude * 10) + 552;
            if ((rightVel > 0 && rightVel < 400) || (rightVel < 0 && rightVel > -400))
                rGround.X += Convert.ToInt32(rightVel * -1);
            else if (rightVel < 0)
                rGround.X += 400;
            else if (rightVel > 0) //if the rocket is going too fast, the ground cannot be reset fast enough. These two elseif statements fix that.
                rGround.X -= 400;

            //Make the ground go on forever
            if (rGround.X > -50 && rightVel < 0)
                rGround.X -= 492;
            if (rGround.X < -500 && rightVel > 0)
                rGround.X += 492;


            //Auto rotation near the ground
            if (autoRotate == true && explode == false)
            {
                double CubeRoot;
                double posRightVel;
                posRightVel = rightVel;
                if (rightVel < 0)
                    posRightVel *= -1; //Make sure rightVel value is always positive so we can take its cube root
                autoUpVel = ((altitude / 10) + 10) * -1;
                CubeRoot = (Math.Pow(posRightVel, (double)1 / 3)); //Take cube root of rightVel
                autoAngle = CubeRoot * -10; //use the function y = cbrt(x)*-10 to output the desired autoAngle
                if (autoAngle > 70)
                    autoAngle = 70;
                else if (autoAngle < -70)
                    autoAngle = -70;
                if (rightVel < 0)
                    autoAngle *= -1;
                if (rightVel < 1 && rightVel > -1)
                    autoAngle = 0; //Do not angle the rocket if it is not moving sideways (quickly)
                //Console.WriteLine("x: " + rightVel);
                //Console.WriteLine("y: " + autoAngle);
                //Console.WriteLine(rightVel);

                if (angle < autoAngle && autoAngle - angle > 1)
                    angleVel = 1;
                else if (angle > autoAngle && angle - autoAngle > 1)
                    angleVel = -1;
                angle += angleVel;
            }

            //Graphics for corner map/radar
            mapGroundHeight = (15 * Convert.ToInt32(altitude)) / 100 + 100; // y = 0.15x+100
            mapGroundHeight2 = (15 * Convert.ToInt32(altitude)) / 10 + 100; // y = 1.5x+100
            //Console.WriteLine("MapHeight: " + mapGroundHeight);
            if (altitude > 2000 && altitude < 3000)
                mapShipHeight = ((Convert.ToInt32(altitude)) / 10 - 110); //Slowly moves white ball to centre of radar between 2000m and 3000m
            else if (altitude < 2000)
                mapShipHeight = 90; //Make sure white ball is high enough up for landing configurations (gives more room for the ground to come up)
            else if (altitude > 3000)
                mapShipHeight = 190; //Make sure white ball is centered in the radar for flying configurations (makes the radar look better)
            //Console.WriteLine("Ship Height: " + mapShipHeight);


            //Sprites
            if (count >= (totalRows * totalCols))
                count = 0;
            row = count / totalCols; //returns the integer only (no remainder)
            col = count % totalCols; //returns the remainder only (no integer)
            count += 1;

            //Explosion sprite
            if (explode == true)
            {
                if (count2 >= (totalRows2 * totalCols2))
                {
                    count2 = 0;
                    explode = false;
                    restartProgram();
                }
                if (count2 > 30)
                {
                    top = 0;
                    tank = 0;
                    engine = 0;
                }
                row2 = count2 / totalCols2; //returns the integer only (no remainder)
                col2 = count2 % totalCols2; //returns the remainder only (no integer)
                count2 += 1;
            }

            //Change the size of the flame as a function of the throttle %
            rSprite.Height = Convert.ToInt32(1.31 * throttle);
            rSprite.Width = Convert.ToInt32(rSprite.Height * 0.38168);
            rSprite.X = Convert.ToInt32(throttle * -0.23) - 2;
            if (engine == 3)
                rSprite.Y = 125;
            else
                rSprite.Y = 133;

            //Don't display certain elements if the designer is active
            if (designer == true)
                pbMap.Visible = false;
            else
                pbMap.Visible = true;


            //Console.WriteLine(comAcc * Math.Cos(angleRad));
            //Console.WriteLine(grav / 30);


        }

        private void pbArena_Paint(object sender, PaintEventArgs e)
        {
            pbMap.Refresh();
            //e.Graphics.FillRectangle(Brushes.Green, rGround);
            if (SOI == "Earth")
                e.Graphics.DrawImage(Properties.Resources.grass, rGround);
            else if (SOI == "Moon")
                e.Graphics.DrawImage(Properties.Resources.moonGround, rGround);
            e.Graphics.FillRectangle(Brushes.White, rThrust);
            e.Graphics.DrawRectangle(ThrottlePen, rThrust.X, 100, rThrust.Width, 100);

            e.Graphics.DrawString("Speed: " + Math.Round(comVel, 1).ToString("N1") + "m/s", new System.Drawing.Font("Tahoma", 20), Brushes.White, this.Width - 250, 40);
            e.Graphics.DrawString("Altitude: " + Math.Round(altitude, 1).ToString("N1") + "m", new System.Drawing.Font("Tahoma", 20), Brushes.White, this.Width - 250, 70);
            e.Graphics.DrawString("Fuel: " + Math.Round(fuel, 1).ToString("N1") + "Kg", new System.Drawing.Font("Tahoma", 20), Brushes.White, this.Width - 250, 100);

            if (upVel > 0)
            {
                e.Graphics.DrawString(Math.Round(upVel, 1).ToString("N1") + " m/s", new System.Drawing.Font("Tahoma", 20), Brushes.White, this.Width / 2 + 50, 70);
                e.Graphics.DrawImage(Properties.Resources.ArrowU, this.Width / 2 - 30, 40, 59, 80);
            }
            else if (upVel < 0)
            {
                e.Graphics.DrawString(Math.Round(-upVel, 1).ToString("N1") + "m/s", new System.Drawing.Font("Tahoma", 20), Brushes.White, this.Width / 2 + 50, this.Height - 150);
                e.Graphics.DrawImage(Properties.Resources.ArrowD, this.Width / 2 - 30, this.Height - 170, 59, 80);
            }
            if (rightVel > 0)
            {
                e.Graphics.DrawString(Math.Round(rightVel, 1).ToString("N1") + "m/s", new System.Drawing.Font("Tahoma", 20), Brushes.White, this.Width - 230, this.Height / 2 + 30);
                e.Graphics.DrawImage(Properties.Resources.ArrowR, this.Width - 150, this.Height / 2 - 30, 80, 59);
            }
            else if (rightVel < 0)
            {
                e.Graphics.DrawString(Math.Round(-rightVel, 1).ToString("N1") + "m/s", new System.Drawing.Font("Tahoma", 20), Brushes.White, 350, this.Height / 2 + 10);
                e.Graphics.DrawImage(Properties.Resources.ArrowL, 270, this.Height / 2 - 30, 80, 59);
            }
            e.Graphics.FillRectangle(Brushes.White, 0, 273, 230, 420);


            //Anything past here is painted on top of the blueprint (designer elements only)
            if (designer == true)
            {
                //Graphical Elements
                e.Graphics.DrawImage(Properties.Resources.blueprint, 0, 0, 2560, 1440);
                e.Graphics.FillRectangle(Brushes.DarkSlateGray, 0, 0, 350, 1000);
                e.Graphics.FillRectangle(Brushes.White, 350, 0, 10, 1000);

                //Lines dividing part categories
                e.Graphics.FillRectangle(Brushes.White, 0, 270, 350, 10);
                e.Graphics.FillRectangle(Brushes.White, 0, 420, 350, 10);
                e.Graphics.FillRectangle(Brushes.White, 0, 684, 350, 10);

                //Lines dividing part options
                e.Graphics.FillRectangle(Brushes.White, 170, 0, 10, 900);
                e.Graphics.FillRectangle(Brushes.White, 170, 270, 10, 160);
                e.Graphics.FillRectangle(Brushes.White, 83, 270, 10, 160);
                e.Graphics.FillRectangle(Brushes.White, 257, 270, 10, 160);
                e.Graphics.FillRectangle(Brushes.White, 0, 552, 350, 10);

                //Parts (to chose)
                e.Graphics.DrawImage(Properties.Resources.VectorCapsule, rTop1);
                e.Graphics.DrawImage(Properties.Resources.VectorFaring, rTop2);
                e.Graphics.FillRectangle(Brushes.Gray, rTank1);
                e.Graphics.FillRectangle(Brushes.Red, rTank2);
                e.Graphics.FillRectangle(Brushes.Gainsboro, rTank3);
                e.Graphics.FillRectangle(Brushes.Blue, rTank4);
                e.Graphics.DrawImage(Properties.Resources.VectorEngineSilver, rEngine1);
                e.Graphics.DrawImage(Properties.Resources.VectorEngineBlack, rEngine2);
                e.Graphics.DrawImage(Properties.Resources.VectorEngineDual, rEngine3);
                e.Graphics.DrawImage(Properties.Resources.VectorEnginePurple, rEngine4);

                //Information
                e.Graphics.DrawString("Thrust: " + thrust + "Kn", new System.Drawing.Font("Tahoma", 20), Brushes.White, this.Width - 250, 30);
                e.Graphics.DrawString("Efficiency: " + isp + "s", new System.Drawing.Font("Tahoma", 20), Brushes.White, this.Width - 250, 60);
                e.Graphics.DrawString("Fuel: " + fuel + "Kg", new System.Drawing.Font("Tahoma", 20), Brushes.White, this.Width - 250, 90);
                e.Graphics.DrawString("Dry Mass: " + dryMass + "Kg", new System.Drawing.Font("Tahoma", 20), Brushes.White, this.Width - 250, 120);
                e.Graphics.DrawString("TWR: " + Math.Round((thrust / (fuel + dryMass) / 0.327), 2), new System.Drawing.Font("Tohoma", 20), Brushes.White, this.Width - 250, 150);
                e.Graphics.DrawString("Press Enter to begin flight!", new System.Drawing.Font("Tohoma", 20), Brushes.White, 400, 30);
                e.Graphics.DrawString("Press M to go to the moon!", new System.Drawing.Font("Tohoma", 20), Brushes.White, 400, 60);
            }

            //Anything past this point will be drawn attached to the ship
            Graphics g = e.Graphics;
            g.RotateTransform(angle, MatrixOrder.Append);
            g.TranslateTransform(px, py, MatrixOrder.Append);
            if (autoRotate == false && tank > 0)
            {
                g.DrawImage(Properties.Resources.RR, 30, 26, 27, 79);
                g.DrawImage(Properties.Resources.LR, -57, 26, 27, 79);
            }
            else if (autoRotate == true && tank > 0)
            {
                g.DrawImage(Properties.Resources.REB, 30, 76, 57, 76);
                g.DrawImage(Properties.Resources.LEB, -87, 76, 57, 76);
            }

            //Paint custom components
            switch (engine)
            {
                case 1:
                    g.DrawImage(Properties.Resources.VectorEngineSilver, -25, 108, 50, 23);
                    thrust = 700;
                    isp = 320;
                    break;
                case 2:
                    g.DrawImage(Properties.Resources.VectorEngineBlack, -25, 108, 50, 23);
                    thrust = 700;
                    isp = 320;
                    break;
                case 3:
                    g.DrawImage(Properties.Resources.VectorEngineDual, -25, 108, 50, 13);
                    thrust = 1200;
                    isp = 250;
                    break;
                case 4:
                    g.DrawImage(Properties.Resources.VectorEnginePurple, -25, 108, 50, 23);
                    thrust = 500;
                    isp = 375;
                    break;
            }
            switch (tank)
            {
                case 1:
                    g.DrawImage(Properties.Resources.VectorTankGrey, -31, -50, 63, 160);
                    if (designer == true)
                        fuel = 500;
                    dryMass = 500;
                    break;
                case 2:
                    g.DrawImage(Properties.Resources.VectorTankRed, -31, -50, 63, 160);
                    if (designer == true)
                        fuel = 200;
                    dryMass = 400;
                    break;
                case 3:
                    g.DrawImage(Properties.Resources.VectorTankWhite, -31, -50, 63, 160);
                    if (designer == true)
                        fuel = 500;
                    dryMass = 500;
                    break;
                case 4:
                    g.DrawImage(Properties.Resources.VectorTankBlue, -31, -50, 63, 160);
                    if (designer == true)
                        fuel = 1000;
                    dryMass = 600;
                    break;
            }
            switch (top)
            {
                case 1:
                    g.DrawImage(Properties.Resources.VectorCapsule, -37, -194, 74, 146);
                    break;
                case 2:
                    g.DrawImage(Properties.Resources.VectorFaring, -38, -200, 78, 154);
                    break;
            }


            //Paint the rocket's flame
            if (throttle > 0 && tank > 0 && fuel > 0)
                e.Graphics.DrawImage(spriteFlame, rSprite, new RectangleF(col * spriteWidth, row * spriteHeight, spriteWidth, spriteHeight), GraphicsUnit.Pixel);

            //Explosion
            if (explode == true)
            {
                rSprite2.Width = 500; //Change properties of explosion image (make it big enough to cover the rocket)
                rSprite2.Height = 500;
                rSprite2.X = -250;
                rSprite2.Y = -250;
                e.Graphics.DrawImage(spriteExplosion, rSprite2, new RectangleF(col2 * spriteWidth2, row2 * spriteHeight2, spriteWidth2, spriteHeight2), GraphicsUnit.Pixel);
            }


        }

        private void pbMap_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.Green, 0, mapGroundHeight, 100, 350);
            e.Graphics.FillRectangle(Brushes.Green, 110, mapGroundHeight2, 100, 350);
            e.Graphics.FillRectangle(Brushes.White, 45, mapShipHeight, 10, 10);
            e.Graphics.FillRectangle(Brushes.White, 155, mapShipHeight, 10, 10);
            e.Graphics.FillRectangle(Brushes.White, 100, 0, 10, 400);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyData == Keys.A || e.KeyData == Keys.Left) && landed == false && autoRotate == false && explode == false)
                rotate = 1;
            if ((e.KeyData == Keys.D || e.KeyData == Keys.Right) && landed == false && autoRotate == false && explode == false)
                rotate = 2;
            if (e.KeyData == Keys.W && designer == false && explode == false) //Increase throttle
                thrustSetting = 1;
            if (e.KeyData == Keys.S && designer == false && explode == false) //Decrease throttle
                thrustSetting = 2;
            if (e.KeyData == Keys.Z && designer == false) //Max throttle
                throttle = 100;
            if (e.KeyData == Keys.X && designer == false) //Cut throttle
                throttle = 0;
            if (e.KeyData == Keys.G && landed == false) //Landing gear
                autoRotate = !autoRotate;
            if (e.KeyData == Keys.H) //Cheat code
            {
                tank = 2;
                engine = 1;
                top = 1;
            }
            if (e.KeyData == Keys.I) //Cheat code
                thrust = 7000;
            if (e.KeyData == Keys.J) //Cheat code
                explode = true;
            if (e.KeyData == Keys.M) //Cheat code
            {
                grav = 2.00;
                SOI = "Moon";
                grav = 2;
                pbArena.BackColor = Color.Black;
            }
            if (e.KeyData == Keys.Enter && engine > 0 && top > 0 && tank > 0)
            {
                designer = false;
            }
            else if (e.KeyData == Keys.Enter)
                MessageBox.Show("You need to select parts first!");
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.A || e.KeyData == Keys.D || e.KeyData == Keys.Left || e.KeyData == Keys.Right)
                rotate = 0;
            if (e.KeyData == Keys.W || e.KeyData == Keys.S)
                thrustSetting = 0;
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            colourAlt = trackBar1.Value;
            textBox1.Text = ("Alt: " + (100 - colourAlt));
        }
        private void pbArena_MouseDown(object sender, MouseEventArgs e)
        {
            if (rTank1.Contains(e.Location))
                tank = 1;
            if (rTank2.Contains(e.Location))
                tank = 2;
            if (rTank3.Contains(e.Location))
                tank = 3;
            if (rTank4.Contains(e.Location))
                tank = 4;
            if (rEngine1.Contains(e.Location))
                engine = 1;
            if (rEngine2.Contains(e.Location))
                engine = 2;
            if (rEngine3.Contains(e.Location))
                engine = 3;
            if (rEngine4.Contains(e.Location))
                engine = 4;
            if (rTop1.Contains(e.Location))
                top = 1;
            if (rTop2.Contains(e.Location))
                top = 2;
        }
        private void restartProgram()
        {
            designer = true;

            angle = 0;
            autoAngle = 0;
            comAcc = 0;
            altitude = 0;
            grav = 9.91;
            landed = true;
            autoRotate = true;
            rotate = 0;
            thrustSetting = 0;
            throttle = 0;
            count = 0;
            count2 = 0;
            tank = 1;
            SOI = "Earth";
        }
    }
}
