using System;
using System.Runtime.InteropServices;
using System.Text;

namespace libtcodWrapper
{
    /// <summary>
    /// Generates various types of "noise" using libtcod perlin.
    /// </summary>
    public class TCODNoise : IDisposable
    {
        /// <summary>
        /// Create noise object.
        /// </summary>
        /// <param name="dimensions">Number of dimensions, for perlin and simplex value should be >0 and less than or equal to 4, and less than or equal to 3 for wavelet</param>
        public TCODNoise(int dimensions)
        {
            m_dimensions = dimensions;
            m_instance = TCOD_noise_new(dimensions, NoiseDefaultHurst, NoiseDefaultLacunarity, IntPtr.Zero);
        }

        /// <summary>
        /// Create noise object.
        /// </summary>
        /// <param name="dimensions">Number of dimensions</param>
        /// <param name="random">Random Generator</param>
        public TCODNoise(int dimensions, TCODRandom random)
        {
            m_dimensions = dimensions;
            m_instance = TCOD_noise_new(dimensions, NoiseDefaultHurst, NoiseDefaultLacunarity, random.m_instance);
        }

        /// <summary>
        /// Create noise object.
        /// </summary>
        /// <param name="dimensions">Number of dimensions</param>
        /// <param name="hurst">Hurst</param>
        /// <param name="lacunarity">Lacunarity</param>
        public TCODNoise(int dimensions, double hurst, double lacunarity)
        {
            m_dimensions = dimensions;
            m_instance = TCOD_noise_new(dimensions, (float)hurst, (float)lacunarity, IntPtr.Zero);
        }

        /// <summary>
        /// Create noise object.
        /// </summary>
        /// <param name="dimensions">Number of dimensions</param>
        /// <param name="hurst">Hurst</param>
        /// <param name="lacunarity">Lacunarity</param>
        /// <param name="random">Random Generator</param>
        public TCODNoise(int dimensions, double hurst, double lacunarity, TCODRandom random)
        {
            m_dimensions = dimensions;
            m_instance = TCOD_noise_new(dimensions, (float)hurst, (float)lacunarity, random.m_instance);
        }

        /// <summary>
        /// Get Perlin Noise
        /// </summary>
        /// <param name="f">An array of coordinates</param>
        /// <returns>Perlin noise for that point (-1.0 - 1.0) </returns>
        public float GetPerlinNoise(float[] f)
        {
            CheckDimension(f.Length);
            return TCOD_noise_perlin(m_instance, f);
        }

        /// <summary>
        /// Get Perlin fractional Brownian Motion
        /// </summary>
        /// <param name="f">An array of coordinates</param>
        /// <param name="octaves">Number of iterations. (0-127)</param>
        /// <returns>Browian motion for that point (-1.0 - 1.0)</returns>
        public float GetPerlinBrownianMotion(float[] f, float octaves)
        {
            CheckDimension(f.Length);
            return TCOD_noise_fbm_perlin(m_instance, f, octaves);
        }

        /// <summary>
        /// Get Perlin Turbulence
        /// </summary>
        /// <param name="f">An array of coordinates</param>
        /// <param name="octaves">Number of iterations. (0-127)</param>
        /// <returns>Turbulence for that point (-1.0 - 1.0)</returns>
        public float GetPerlinTurbulence(float[] f, float octaves)
        {
            CheckDimension(f.Length);
            return TCOD_noise_turbulence_perlin(m_instance, f, octaves);
        }

        /// <summary>
        /// Get Simplex Noise
        /// </summary>
        /// <param name="f">An array of coordinates</param>
        /// <returns>Perlin noise for that point (-1.0 - 1.0) </returns>
        public float GetSimplexNoise(float[] f)
        {
            CheckDimension(f.Length);
            return TCOD_noise_simplex(m_instance, f);
        }

        /// <summary>
        /// Get Simplex fractional Brownian Motion
        /// </summary>
        /// <param name="f">An array of coordinates</param>
        /// <param name="octaves">Number of iterations. (0-127)</param>
        /// <returns>Browian motion for that point (-1.0 - 1.0)</returns>
        public float GetSimplexBrownianMotion(float[] f, float octaves)
        {
            CheckDimension(f.Length);
            return TCOD_noise_fbm_simplex(m_instance, f, octaves);
        }

        /// <summary>
        /// Get Simplex Turbulence
        /// </summary>
        /// <param name="f">An array of coordinates</param>
        /// <param name="octaves">Number of iterations. (0-127)</param>
        /// <returns>Turbulence for that point (-1.0 - 1.0)</returns>
        public float GetSimplexTurbulence(float[] f, float octaves)
        {
            CheckDimension(f.Length);
            return TCOD_noise_turbulence_simplex(m_instance, f, octaves);
        }

        /// <summary>
        /// Get Wavelet Noise
        /// </summary>
        /// <param name="f">An array of coordinates</param>
        /// <returns>Perlin noise for that point (-1.0 - 1.0) </returns>
        public float GetWaveletNoise(float[] f)
        {
            CheckDimension(f.Length);
            return TCOD_noise_wavelet(m_instance, f);
        }

        /// <summary>
        /// Get Wavelet fractional Brownian Motion
        /// </summary>
        /// <param name="f">An array of coordinates</param>
        /// <param name="octaves">Number of iterations. (0-127)</param>
        /// <returns>Browian motion for that point (-1.0 - 1.0)</returns>
        public float GetWaveletBrownianMotion(float[] f, float octaves)
        {
            CheckDimension(f.Length);
            return TCOD_noise_fbm_wavelet(m_instance, f, octaves);
        }

        /// <summary>
        /// Get Wavelet Turbulence
        /// </summary>
        /// <param name="f">An array of coordinates</param>
        /// <param name="octaves">Number of iterations. (0-127)</param>
        /// <returns>Turbulence for that point (-1.0 - 1.0)</returns>
        public float GetWaveletTurbulence(float[] f, float octaves)
        {
            CheckDimension(f.Length);
            return TCOD_noise_turbulence_wavelet(m_instance, f, octaves);
        }

        /// <summary>
        /// Destory unmanaged noice generator.
        /// </summary>
        public void Dispose()
        {
            TCOD_noise_delete(m_instance);
        }

        /// <summary>
        /// Default hurst value for noise generator
        /// </summary>
        public const float NoiseDefaultHurst = 0.5f;

        /// <summary>
        /// Default Lacunarity value for noise generator
        /// </summary>
        public const float NoiseDefaultLacunarity = 2.0f;

        private IntPtr m_instance;
        private int m_dimensions;

        #region DllImport
        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_noise_new(int dimensions, float hurst, float lacunarity, IntPtr random);

        // basic perlin noise
        [DllImport(DLLName.name)]
        private extern static float TCOD_noise_perlin(IntPtr noise, float[] f);

        // fractional brownian motion
        [DllImport(DLLName.name)]
        private extern static float TCOD_noise_fbm_perlin(IntPtr noise, float[] f, float octaves);

        // turbulence
        [DllImport(DLLName.name)]
        private extern static float TCOD_noise_turbulence_perlin(IntPtr noise, float[] f, float octaves);

        // basic simplex noise
        [DllImport(DLLName.name)]
        private extern static float TCOD_noise_simplex(IntPtr noise, float[] f);

        // fractional brownian motion
        [DllImport(DLLName.name)]
        private extern static float TCOD_noise_fbm_simplex(IntPtr noise, float[] f, float octaves);

        // turbulence
        [DllImport(DLLName.name)]
        private extern static float TCOD_noise_turbulence_simplex(IntPtr noise, float[] f, float octaves);

        // basic wavelet noise
        [DllImport(DLLName.name)]
        private extern static float TCOD_noise_wavelet(IntPtr noise, float[] f);

        // fractional brownian motion
        [DllImport(DLLName.name)]
        private extern static float TCOD_noise_fbm_wavelet(IntPtr noise, float[] f, float octaves);

        // turbulence
        [DllImport(DLLName.name)]
        private extern static float TCOD_noise_turbulence_wavelet(IntPtr noise, float[] f, float octaves);

        [DllImport(DLLName.name)]
        private extern static void TCOD_noise_delete(IntPtr noise);
        #endregion

        private void CheckDimension(int dims)
        {
            if (m_dimensions != dims)
                throw new Exception("TCODNoise: Dimension mismatch");
        }
    };
}
