using System;
using System.Runtime.InteropServices;
using System.Text;

namespace libtcodWrapper
{
    /// <summary>
    /// Produces random numbers from the Mersenne Twister
    /// </summary>
    public class TCODRandom : IDisposable
    {
        /// <summary>
        /// Create a new instance of a random number generator.
        /// </summary>
        public TCODRandom()
        {
            m_instance = TCOD_random_new();
        }

        /// <summary>
        /// Create new instance of a random number generator with a starting seed.
        /// </summary>
        /// <param name="seed">Intial Seed</param>
        public TCODRandom(uint seed)
        {
            m_instance = TCOD_random_new_from_seed(seed);
        }

        /// <summary>
        /// Destroy unmanaged random number generator
        /// </summary>
        public void Dispose()
        {
            TCOD_random_delete(m_instance);
        }

        /// <summary>
        /// Obtain a random integer in a given range
        /// </summary>
        /// <param name="min">Minimum number to generate</param>
        /// <param name="max">Maximum number to generate</param>
        /// <returns>Random Number</returns>
        public int GetRandomInt(int min, int max)
        {
            return TCOD_random_get_int(m_instance, min, max);
        }

        /// <summary>
        /// Obtain a random floating point number in a given range
        /// </summary>
        /// <param name="min">Minimum number to generate</param>
        /// <param name="max">Maximum number to generate</param>
        /// <returns>Random Number</returns>
        public float GetRandomFloat(double min, double max)
        {
            return TCOD_random_get_float(m_instance, (float)min, (float)max);
        }

        /// <summary>
        /// Obtain a random floating point number in a given range
        /// </summary>
        /// <param name="min">Minimum number to generate</param>
        /// <param name="max">Maximum number to generate</param>
        /// <returns>Random Number</returns>
        public float GetRandomFloat(float min, float max)
        {
            return TCOD_random_get_float(m_instance, min, max);
        }

        /// <summary>
        /// Deterministly obtain a random number based upon a string seed
        /// </summary>
        /// <param name="min">Minimum number to generate</param>
        /// <param name="max">Maximum number to generate</param>
        /// <param name="data">String to be a seed</param>
        /// <returns>Random Number</returns>
        public int GetIntFromByteArray(int min, int max, string data)
        {
            return TCOD_random_get_int_from_byte_array(min, max, new StringBuilder(data), data.Length);
        }

        internal IntPtr m_instance;

        #region DllImport
        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_random_new();

        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_random_new_from_seed(uint seed);

        [DllImport(DLLName.name)]
        private extern static int TCOD_random_get_int(IntPtr mersenne, int min, int max);
        
        [DllImport(DLLName.name)]
        private extern static float TCOD_random_get_float(IntPtr mersenne, float min, float max);

        [DllImport(DLLName.name)]
        private extern static int TCOD_random_get_int_from_byte_array(int min, int max, StringBuilder data, int len);

        [DllImport(DLLName.name)]
        private extern static void TCOD_random_delete(IntPtr mersenne);
        #endregion
    }
}
