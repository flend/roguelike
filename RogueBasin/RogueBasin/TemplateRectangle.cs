using System;
using System.Collections;
using System.Drawing;

namespace RogueBasin
{
    public sealed class TemplateRectangle : IEnumerable
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int Left;
        public readonly int Top;

        public int Bottom
        {
            get
            {
                return Top + Height - 1;
            }
        }

        public int Right
        {
            get
            {
                return Left + Width - 1;
            }
        }

        public TemplateRectangle(int left, int top, int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Can't construct rectangle with zero or negative dimensions");

            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }
        
        public TemplateRectangle GetOverlapRectangle(TemplateRectangle other)
        {
            //Wrap rectangle calls

            Rectangle thisRect = new Rectangle(this.Left, this.Top, this.Right - this.Left, this.Bottom - this.Top);
            Rectangle otherRect = new Rectangle(other.Left, other.Top, other.Right - other.Left, other.Bottom - other.Top);

            if (!thisRect.IntersectsWith(otherRect))
                return null;

            Rectangle intersectRect = Rectangle.Intersect(thisRect, otherRect);

            return new TemplateRectangle(intersectRect.Left, intersectRect.Top, intersectRect.Width + 1, intersectRect.Height + 1);
        }

        public IEnumerator GetEnumerator()
        {
            return new MyEnumerator(this);
        }

        //private enumerator class
        private class MyEnumerator : IEnumerator
        {
            private int iter_x;
            private int iter_y;

            private bool set = false;

            TemplateRectangle parent;

            public MyEnumerator(TemplateRectangle parent)
            {
                this.parent = parent;
            }

            //IEnumerator
            public bool MoveNext()
            {
                if (!set)
                {
                    Reset();
                    set = true;
                    return true;
                }

                iter_x++;
                if (iter_x > parent.Right)
                {
                    iter_x = parent.Left;
                    iter_y++;
                }

                return (iter_y <= parent.Bottom);
            }

            //IEnumerator
            public void Reset()
            {
                iter_x = parent.Left;
                iter_y = parent.Top;
            }

            //IEnumerator
            public object Current
            {
                get
                {
                    return new Point(iter_x, iter_y);
                }
            }
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            TemplateRectangle p = obj as TemplateRectangle;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (Left == p.Left) && (Top == p.Top) && (Width == p.Width) && (Height == p.Height);
        }

        public bool Equals(TemplateRectangle p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (Left == p.Left) && (Top == p.Top) && (Width == p.Width) && (Height == p.Height);
        }

        public override int GetHashCode()
        {
            return (Left + Top) ^ (Width + Height);
        }
    }
}
