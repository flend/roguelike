namespace RogueBasin.Algorithms
{
    /** Provides field of view services */
    public interface IFieldOfView
    {
        /// <summary>
        /// Update internal map representation, gets enum map for fov
        /// </summary>
        void updateFovMap(int level, FovMap byteMap);

        /// <summary>
        /// Update internal map representation, point by point
        /// </summary>
        void updateFovMap(int level, Point point, FOVTerrain newTerrain);

        /// <summary>
        /// Calculate fov from origin point. Subsequent calls to CheckTileFOV will use this origin
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="sightRadius"></param>
        void CalculateFOV(int level, Point origin, int sightRadius);

        /// <summary>
        /// Is tile viewable? Should be called after CalculateFOVFromPoint
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        bool CheckTileFOV(int level, Point pointToCheck);
    }
}
