using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
