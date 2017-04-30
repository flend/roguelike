using System;
using System.Diagnostics;

namespace RogueBasin
{
    public static class GraphVizUtils
    {

        public static void RunGraphVizPDF(string graphVizLocation, string filename)
        {
            RunGraphViz(graphVizLocation, "pdf", filename);
        }

        /// <summary>
        /// Uses graphviz to make a png from a dot. No filename extension on parameter
        /// </summary>
        /// <param name="filename"></param>
        public static void RunGraphViz(string graphVizLocation, string format, string filename)
        {
            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = graphVizLocation;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = filename + ".dot -T" + format + " -o " + filename + "." + format;

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry("Failed to run graphviz: " + ex.Message);
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
