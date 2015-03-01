using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SdlDotNet.Graphics;
using SdlDotNet.Core;


namespace RogueBasin
{
    /// <summary>
    /// Renders a multi-layer tile map onto the screen
    /// </summary>
    class MapRendererSDLDotNet : IMapRenderer
    {

        private Surface m_VideoScreen;
        private Surface m_Background;
        private Surface m_Foreground;
        private System.Drawing.Point m_ForegroundPosition;

        /// <summary>
        /// Render the map, with TL in map at mapOffset. Screenviewport is the screen viewport in tile dimensions (for now)
        /// </summary>
        /// <param name="mapToRender"></param>
        /// <param name="mapOffset"></param>
        /// <param name="screenViewport"></param>
        public void RenderMap(TileEngine.TileMap mapToRender, Point mapOffset, Rectangle screenViewport)
        {

            m_Background = (new Surface(@"DemoBackground.png")).Convert(m_VideoScreen, true, false);
            m_Foreground = (new Surface(@"DemoForeground.png")).Convert(m_VideoScreen, true, false);
            m_Foreground.Transparent = true;
            m_Foreground.TransparentColor = System.Drawing.Color.FromArgb(255, 0, 255);
            m_ForegroundPosition = new System.Drawing.Point(m_VideoScreen.Width / 2 - m_Foreground.Width / 2,
                                              m_VideoScreen.Height / 2 - m_Foreground.Height / 2);

            m_VideoScreen.Blit(m_Background);
            m_VideoScreen.Blit(m_Foreground, m_ForegroundPosition);
            m_VideoScreen.Update();
        }

        public void Sleep(ulong milliseconds)
        {
        }

        public void Setup(int width, int height)
        {
            m_VideoScreen = Video.SetVideoMode(640, 480, 32, false, false, false, true);
        }


        public void Flush()
        {

        }

        public void Clear()
        {

        }

        public void DrawFrame(int x, int y, int width, int height, bool clear, Color color)
        {

        }

        public void PutChar(int x, int y, char c, Color color)
        {

        }

        public void PrintStringRect(string msg, int x, int y, int width, int height, libtcodWrapper.LineAlignment alignment, Color color)
        {

        }

        public void PrintString(string msg, int x, int y, libtcodWrapper.LineAlignment alignment, Color color)
        {

        }

        public void ClearRect(int x, int y, int width, int height)
        {

        }
    }
}
