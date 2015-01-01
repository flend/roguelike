using System;
using System.Runtime.InteropServices;
using System.Text;

namespace libtcodWrapper
{
    /// <summary>
    /// Represents an image loaded from disk or created in memory
    /// </summary>
    public class Image : IDisposable
    {
        private IntPtr m_instance;

        /// <summary>
        /// Creates a new image of a given side with root's background color (or black if none exists)
        /// </summary>
        /// <param name="width">Width of new image</param>
        /// <param name="height">Height of new image</param>
        public Image(int width, int height)
        {
            m_instance = TCOD_image_new(width, height);
        }

        /// <summary>
        /// Loads a .bmp image from disk
        /// </summary>
        /// <param name="filename">Filename or path</param>
        public Image(string filename)
        {
            m_instance = TCOD_image_load(new StringBuilder(filename));
        }

        /// <summary>
        /// Create image from current console state
        /// </summary>
        /// <param name="console">Console to take image of</param>
        public Image(Console console)
        {
            m_instance = TCOD_image_from_console(console.m_consolePtr);
        }

        /// <summary>
        /// Destory unmanaged image resource
        /// </summary>
        public void Dispose()
        {
            TCOD_image_delete(m_instance);
        }

        /// <summary>
        /// Clear an image to a specific background color
        /// </summary>
        /// <param name="color">Color to clear to</param>
        public void Clear(Color color)
        {
            TCOD_image_clear(m_instance, color); 
        }

        /// <summary>
        /// Write image to disk as .bmp file
        /// </summary>
        /// <param name="filename">Filename to create</param>
        public void SaveImageToDisc(string filename)
        {
            TCOD_image_save(m_instance, new StringBuilder(filename));
        }

        /// <summary>
        /// Get the current image's size
        /// </summary>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        public void GetSize(out int w, out int h)
        {
            TCOD_image_get_size(m_instance, out w, out h);
        }

        /// <summary>
        /// Get color of specific pixel in image
        /// </summary>
        /// <param name="x">Width</param>
        /// <param name="y">Height</param>
        /// <returns></returns>
        public Color GetPixel(int x, int y)
        {
            return TCOD_image_get_pixel(m_instance, x, y);
        }

        /// <summary>
        /// Is current pixel "transparent", key color
        /// </summary>
        /// <param name="x">x coord</param>
        /// <param name="y">y coord</param>
        /// <returns>Is Transparent?</returns>
        public bool GetPixelTransparency(int x, int y)
        {
            return TCOD_image_is_pixel_transparent(m_instance, x, y);
        }

        /// <summary>
        /// Use mipmaps to get average color of a region
        /// </summary>
        /// <param name="x0">Upper left corner x coord</param>
        /// <param name="y0">Upper left corner y coord</param>
        /// <param name="x1">Lower right corner x coord</param>
        /// <param name="y1">Lower right corner y coord</param>
        /// <returns></returns>
        public Color AverageColorOfRegion(float x0, float y0, float x1, float y1)
        {
            return TCOD_image_get_mipmap_pixel(m_instance, x0, y0, x1, y1);
        }

        /// <summary>
        /// Change color of given pixel
        /// </summary>
        /// <param name="x">x coord of pixel</param>
        /// <param name="y">y coord of pixel</param>
        /// <param name="col">Color to change pixel to</param>
        public void PutPixel(int x, int y, Color col)
        {
            TCOD_image_put_pixel(m_instance, x, y, col);
        }

        /// <summary>
        /// Set "Key Color", the transparent color of an image
        /// </summary>
        /// <param name="keyColor">Key Color</param>
        public void SetKeyColor(Color keyColor)
        {
            TCOD_image_set_key_color(m_instance, keyColor);
        }

        /// <summary>
        /// Blit entire image onto console
        /// </summary>
        /// <param name="console">Console target</param>
        /// <param name="x">x coord of center of image on console</param>
        /// <param name="y">y coord of center of image on console</param>
        /// <param name="background">How image affects background color</param>
        /// <param name="scalex">Width scaling factor</param>
        /// <param name="scaley">Height scaling factor</param>
        /// <param name="angle">Rotation angle in radians</param>
        public void Blit(Console console, float x, float y, Background background, double scalex, double scaley, double angle)
        {
            TCOD_image_blit(m_instance, console.m_consolePtr, x, y, background.m_value, (float)scalex, (float)scaley, (float)angle);
        }

        /// <summary>
        /// Blit part of a image to the console
        /// </summary>
        /// <param name="console">Console target</param>
        /// <param name="x">x coord of upper left of image on console</param>
        /// <param name="y">y coord of upper right of image on console</param>
        /// <param name="w">Width of part of image to blit</param>
        /// <param name="h">Height of part of image to blit</param>
        /// <param name="background">How image affects background color</param>
        public void BlitRect(Console console, int x, int y, int w, int h, Background background)
        {
            TCOD_image_blit_rect(m_instance, console.m_consolePtr, x, y, w, h, background.m_value);
        }

        #region DllImport
        [DllImport(DLLName.name)]
        private extern static void TCOD_image_blit_rect(IntPtr image, IntPtr console, int x, int y, int w, int h, /*BackgroundFlag*/ int bkgnd_flag);

        [DllImport(DLLName.name)]
        private extern static void TCOD_image_blit(IntPtr image, IntPtr console, float x, float y, /*BackgroundFlag*/ int bkgnd_flag, float scalex, float scaley, float angle);

        [DllImport(DLLName.name)]
        private extern static void TCOD_image_set_key_color(IntPtr image, Color key_color); 
        
        [DllImport(DLLName.name)]
        private extern static bool TCOD_image_is_pixel_transparent(IntPtr image, int x, int y);

        [DllImport(DLLName.name)]
        private extern static void TCOD_image_put_pixel(IntPtr image, int x, int y, Color col);

        [DllImport(DLLName.name)]
        private extern static Color TCOD_image_get_mipmap_pixel(IntPtr image, float x0, float y0, float x1, float y1);

        [DllImport(DLLName.name)]
        private extern static Color TCOD_image_get_pixel(IntPtr image, int x, int y);

        [DllImport(DLLName.name)]
        private extern static void TCOD_image_get_size(IntPtr image, out int w, out int h);
        
        [DllImport(DLLName.name)]
        private extern static void TCOD_image_save(IntPtr image, StringBuilder filename);

        [DllImport(DLLName.name)]
        private extern static void TCOD_image_clear(IntPtr image, Color color);

        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_image_from_console(IntPtr console);

        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_image_load(StringBuilder filename);

        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_image_new(int width, int height);

        [DllImport(DLLName.name)]
        private extern static void TCOD_image_delete(IntPtr image);
        #endregion
    }
}
