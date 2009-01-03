using System;
using System.Runtime.InteropServices;

namespace libtcodWrapper
{
    /// <summary>
    /// Callback made from pathfinding engine to determine cell pathfinding information
    /// </summary>
    /// <param name="xFrom">staring x coord</param>
    /// <param name="yFrom">starting y coord</param>
    /// <param name="xTo">ending x coord</param>
    /// <param name="yTo">ending y coord</param>
    /// <returns>"Cost" to pass through cell</returns>
    public delegate float TCODPathCallback(int xFrom, int yFrom, int xTo, int yTo);

    /// <summary>
    /// Calculates paths in maps using djikstra's algorithms
    /// </summary>
    public class TCODPathFinding : IDisposable
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate float TCODPathCallbackInternal(int xFrom, int yFrom, int xTo, int yTo, IntPtr nullPtr);

        float TCODPathCallInternal(int xFrom, int yFrom, int xTo, int yTo, IntPtr nullPtr)
        {
            return m_callback(xFrom, yFrom, xTo, yTo);
        }

        private IntPtr m_instance;
        private TCODPathCallback m_callback;
        private TCODPathCallbackInternal m_internalCallback;

        /// <summary>
        /// Create a new TCODPathFinding with a callback to determine cell information
        /// </summary>
        /// <param name="width">Map Width</param>
        /// <param name="height">Map Height</param>
        /// <param name="diagonalCost">Factor diagonal moves cost more</param>
        /// <param name="callback">Callback from path finder</param>
        public TCODPathFinding(int width, int height, double diagonalCost, TCODPathCallback callback)
        {
            m_callback = callback;
            m_internalCallback = new TCODPathCallbackInternal(this.TCODPathCallInternal);
            m_instance = TCOD_path_new_using_function(width, height, m_internalCallback, IntPtr.Zero, (float)diagonalCost);
        }

        /// <summary>
        /// Create new TCODPathFinding using map from TCODFov instance
        /// </summary>
        /// <param name="fovMap">Existing map</param>
        /// <param name="diagonalCost">Factor diagonal moves cost more</param>
        public TCODPathFinding(TCODFov fovMap, double diagonalCost)
        {
            m_instance = TCOD_path_new_using_map(fovMap.m_mapPtr, (float)diagonalCost);
        }

        /// <summary>
        /// Compute a path from source to destination. 
        /// </summary>
        /// <param name="origX">Starting point x coord</param>
        /// <param name="origY">Starting point y coord</param>
        /// <param name="destX">Destination point x coord</param>
        /// <param name="destY">Destination point y coord</param>
        /// <returns>IsPathFound?</returns>
        public bool ComputePath(int origX, int origY, int destX, int destY)
        {
            return TCOD_path_compute(m_instance, origX, origY, destX, destY);
        }


        /// <summary>
        /// Walk along a path. Fill x and y with previous step's coord to get next point.
        /// </summary>
        /// <param name="x">x coord</param>
        /// <param name="y">y coord</param>
        /// <param name="recalculateWhenNeeded">If path comes to abrupt end, can we spend time looking for route?</param>
        /// <returns>MoreToWalkAlong?</returns>
        public bool WalkPath(ref int x, ref int y, bool recalculateWhenNeeded)
        {
            return TCOD_path_walk(m_instance, ref x, ref y, recalculateWhenNeeded);
        }


        /// <summary>
        /// Query individual point on path
        /// </summary>
        /// <param name="index">0-based index of points in path list</param>
        /// <param name="x">x coord of point</param>
        /// <param name="y">y coord of point</param>
        public void GetPointOnPath(int index, out int x, out int y)
        {
            TCOD_path_get(m_instance, index, out x, out y);
        }

        /// <summary>
        /// Returns if path is empty of points
        /// </summary>
        /// <returns>IsEmpty?</returns>
        public bool IsPathEmpty()
        {
            return TCOD_path_is_empty(m_instance);
        }

        /// <summary>
        /// Get remainding points on path
        /// </summary>
        /// <returns>Path Size</returns>
        public int GetPathSize()
        {
            return TCOD_path_size(m_instance);
        }
        /// <summary>
        /// Get the origin of the path
        /// </summary>
        /// <param name="x">x coord of origin</param>
        /// <param name="y">y coord of origin</param>
        public void GetPathOrigin(out int x, out int y)
        {
            TCOD_path_get_origin(m_instance, out x, out y);
        }

        /// <summary>
        /// Get the destination of the path
        /// </summary>
        /// <param name="x">x coord of destination</param>
        /// <param name="y">y coord of destination</param>
        public void GetPathDestination(out int x, out int y)
        {
            TCOD_path_get_destination(m_instance, out x, out y);
        }

        /// <summary>
        /// Destory unmanaged pathfinding resource.
        /// </summary>
        public void Dispose()
        {
            TCOD_path_delete(m_instance);
        }

        #region DllImport
        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_path_new_using_function(int map_width, int map_height, TCODPathCallbackInternal func, IntPtr nullData, float diagonalCost);

        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_path_new_using_map(IntPtr map, float diagonalCost);

        [DllImport(DLLName.name)]
        private extern static bool TCOD_path_compute(IntPtr path, int origX, int origY, int destX, int destY);

        [DllImport(DLLName.name)]
        private extern static bool TCOD_path_walk(IntPtr path, ref int x, ref int y, bool recalculate_when_needed);

        [DllImport(DLLName.name)]
        private extern static bool TCOD_path_is_empty(IntPtr path);

        [DllImport(DLLName.name)]
        private extern static int TCOD_path_size(IntPtr path);

        [DllImport(DLLName.name)]
        private extern static void TCOD_path_get(IntPtr path, int index, out int x, out int y);

        [DllImport(DLLName.name)]
        private extern static void TCOD_path_get_origin(IntPtr path, out int x, out int y);

        [DllImport(DLLName.name)]
        private extern static void TCOD_path_get_destination(IntPtr path, out int x, out int y);

        [DllImport(DLLName.name)]
        private extern static void TCOD_path_delete(IntPtr path);
        #endregion

    }
}
