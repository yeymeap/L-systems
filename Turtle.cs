using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L_systems
{
    internal class Turtle // turtle graphics implementation
    {
        // turtle state
        private Graphics g;
        private Pen pen;
        private float x, y;
        private float angle;
        private bool penDown = false;
        private struct TurtleState // saving turtle state for push/pop
        {
            public float X, Y;
            public float Angle;
            public bool PenDown;
            public Color PenColor;
            public float PenWidth;
        }
        private readonly Stack<TurtleState> stateStack = new Stack<TurtleState>(); // stack for turtle states
        public Turtle(Graphics graphics, float startX, float startY) // constructor
        {
            g = graphics;
            x = startX;
            y = startY;
            angle = 0f;
            pen = new Pen(Color.Black, 1);
        }
        public void Forward(float distance) // move forward
        {
            float rad = angle * (float)Math.PI / 180f; // convert angle to radians
            float newX = x + (float)Math.Cos(rad) * distance; // horizontal movement
            float newY = y + (float)Math.Sin(rad) * distance; // vertical movement
            if (penDown) // draw line if pen is down
            {
                g.DrawLine(pen, x, y, newX, newY);
            }
            // update position
            x = newX;
            y = newY;
        }
        public void Backward(float distance) // move backward
        {
            Forward(-distance); // reuse forward method
        }
        public void Right(float degrees) // turn right
        {
            angle += degrees; // update angle
        }
        public void Left(float degrees) // turn left
        {
            angle -= degrees; // update angle
        }
        public void PenUp() // lift pen
        {
            penDown = false;
        }
        public void PenDown() // lower pen
        {
            penDown = true;
        }
        public void SetColor(Color color) // set pen color
        {
            pen.Color = color;
        }
        public void SetWidth(float width) // set pen width
        {
            pen.Width = width;
        }
        public void GoTo(float newX, float newY) // absolute movement to specified coordinates
        {
            if (penDown)
            {
                g.DrawLine(pen, x, y, newX, newY);
            }
            x = newX;
            y = newY;
        }
        public void SetHeading(float degrees) // set absolute heading
        {
            angle = degrees;
        }
        public void PushState() // save current state
        {
            // push current state onto stack
            stateStack.Push(new TurtleState
            {
                X = x,
                Y = y,
                Angle = angle,
                PenDown = penDown,
                PenColor = pen.Color,
                PenWidth = pen.Width
            });
        }
        public void PopState() // restore saved state
        {
            if (stateStack.Count == 0) return;
            // restore state from stack
            var s = stateStack.Pop();
            x = s.X;
            y = s.Y;
            angle = s.Angle;
            penDown = s.PenDown;
            pen.Color = s.PenColor;
            pen.Width = s.PenWidth;
        }
        public float Heading() // get current heading
        {
            return angle;
        }
        public PointF Position() // get current position
        {
            return new PointF(x, y);
        }
    }
}
