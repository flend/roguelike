using System;
using System.Runtime.InteropServices;
using System.Text;

namespace libtcodWrapper
{
    /// <summary>
    /// Calculates Field Of View (FOV) information 
    /// </summary>
    public class TCODFov : IDisposable
    {
        internal IntPtr m_mapPtr;   //Used by TCODPathFinding

        /// <summary>
        /// Create a representation of a given map 
        /// </summary>
        /// <param name="width">Width of map</param>
        /// <param name="height">Height of map</param>
        public TCODFov(int width, int height)
        {
            m_mapPtr = TCOD_map_new(width, height);
            TCOD_map_clear(m_mapPtr);
        }
        
        /// <summary>
        /// Reset map to all cells being blocking
        /// </summary>
        public void ClearMap()
        {
            TCOD_map_clear(m_mapPtr);
        }

        /// <summary>
        /// Destory unmanaged FOV resource
        /// </summary>
        public void Dispose()
        {
            TCOD_map_delete(m_mapPtr);
        }

        /// <summary>
        /// Set a map cell's properties
        /// </summary>
        /// <param name="x">x (width) to update</param>
        /// <param name="y">y (height) to update</param>
        /// <param name="transparent">Light can pass through cell?</param>
        /// <param name="walkable">Creatures can pass through cell?</param>
        public void SetCell(int x, int y, bool transparent, bool walkable)
        {
            TCOD_map_set_properties(m_mapPtr, x, y, transparent, walkable);
        }

        /// <summary>
        /// Get a map cell's properties
        /// </summary>
        /// <param name="x">x (width) to update</param>
        /// <param name="y">y (height) to update</param>
        /// <param name="transparent">Light can pass through cell?</param>
        /// <param name="walkable">Creatures can pass through cell?</param>
        public void GetCell(int x, int y, out bool transparent, out bool walkable)
        {
            transparent = TCOD_map_is_transparent(m_mapPtr, x, y);
            walkable = TCOD_map_is_walkable(m_mapPtr, x, y);
        }

        /// <summary>
        /// Recalculate FOV information based upon player location and lighit radius
        /// </summary>
        /// <param name="playerX">Player x coord</param>
        /// <param name="playerY">Player y coor</param>
        /// <param name="radius">Radius of sight. 0 means unlimited sight radius</param>
        public void CalculateFOV(int playerX, int playerY, int radius)
        {
            TCOD_map_compute_fov(m_mapPtr, playerX, playerY, radius);
        }

        /// <summary>
        /// Is a cell currently visible?
        /// </summary>
        /// <param name="x">x coord of cell</param>
        /// <param name="y">y coord of cell</param>
        /// <returns>Is visible?</returns>
        public bool CheckTileFOV(int x, int y)
        {
            return TCOD_map_is_in_fov(m_mapPtr, x, y);
        }

        #region DllImport
        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_map_new(int width, int height);

        // set all cells as solid rock (cannot see through nor walk)
        [DllImport(DLLName.name)]
        private extern static void TCOD_map_clear(IntPtr map);

        // change a cell properties
        [DllImport(DLLName.name)]
        private extern static void TCOD_map_set_properties(IntPtr map, int x, int y, bool is_transparent, bool is_walkable);

        // destroy a map
        [DllImport(DLLName.name)]
        private extern static void TCOD_map_delete(IntPtr map);

        // calculate the field of view (potentially visible cells from player_x,player_y)
        [DllImport(DLLName.name)]
        private extern static void TCOD_map_compute_fov(IntPtr map, int player_x, int player_y, int max_radius);

        // check if a cell is in the last computed field of view
        [DllImport(DLLName.name)]
        private extern static bool TCOD_map_is_in_fov(IntPtr map, int x, int y);

        // retrieve properties from the map
        [DllImport(DLLName.name)]
        private extern static bool TCOD_map_is_transparent(IntPtr map, int x, int y);
        
        [DllImport(DLLName.name)]
        private extern static bool TCOD_map_is_walkable(IntPtr map, int x, int y);

        #endregion
    }
}
