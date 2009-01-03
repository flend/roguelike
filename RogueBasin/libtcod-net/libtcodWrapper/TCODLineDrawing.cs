using System;
using System.Runtime.InteropServices;
using System.Text;

namespace libtcodWrapper
{
    /// <summary>
    /// Plots points of a line using Bresenham algorithm
    /// </summary>
    /// <remarks>This class is not thread safe, nor is it safe to use more than one "instance" at the same time</remarks>
    public static class TCODLineDrawing
    {
        /// <summary>
        /// Initalize line drawing toolkit with beginning and end point
        /// </summary>
        /// <param name="xFrom">Beginning x coord</param>
        /// <param name="yFrom">Beginning y coord</param>
        /// <param name="xTo">Ending x coord</param>
        /// <param name="yTo">Ending y coord</param>
        public static void InitLine(int xFrom, int yFrom, int xTo, int yTo)
        {
            TCOD_line_init(xFrom, yFrom, xTo, yTo); 
        }

        /// <summary>
        /// Step to the next point on the line
        /// </summary>
        /// <param name="xCur">x Coord of next point of line</param>
        /// <param name="yCur">y Coord of next point of line</param>
        /// <returns>True if endpoint reached</returns>
        /// <remarks>Note: xCur and yCur must be initialized to the point previous to the one you want 
        /// on the line. See TCOD documentation for more details.</remarks>
        public static bool StepLine(ref int xCur, ref int yCur)
        {
            return TCOD_line_step(ref xCur, ref yCur);
        }

        #region DllImport
        [DllImport(DLLName.name)]
        private extern static void TCOD_line_init(int xFrom, int yFrom, int xTo, int yTo);

        //returns true when reached line endpoint
        [DllImport(DLLName.name)]
        private extern static bool TCOD_line_step(ref int xCur, ref int yCur);
        #endregion
    }
}
