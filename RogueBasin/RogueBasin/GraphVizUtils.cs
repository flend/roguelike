using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueBasin
{
    public static class GraphVizUtils
    {

        /// <summary>
        /// Uses graphviz to make a png from a dot. No filename extension on parameter
        /// </summary>
        /// <param name="filename"></param>
        public static void RunGraphVizPNG(string filename)
        {
            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "c:\\Program Files (x86)\\Graphviz2.36\\bin\\dot.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = filename + ".dot -Tpng -o " + filename + ".png";

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch
            {
                // Log error.
            }
        }

        /// <summary>
        /// Launches a new form to display the png. No extension in parameter.
        /// </summary>
        /// <param name="filename"></param>
        public static void DisplayPNGInChildWindow(string filename)
        {
            string pngFilename = filename + ".png";

            ImageDisplay displayForm = new ImageDisplay();
            displayForm.AssignImage(pngFilename);
            displayForm.Text = pngFilename;

            displayForm.Show();
        }
    }
}
