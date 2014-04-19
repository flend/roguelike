using System;
using System.Windows.Forms;

namespace RogueBasin
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string [] args)
        {
            //try
            //{
                using (RogueBase d = new RogueBase())
                {
                    return d.Run(args);
                }
           // }
            //catch (Exception e)
            //{
             //   MessageBox.Show("Error occurred, please check the log file. Terminating. Error: " + e.Message);
             //   return 1;
            //}
        }
    }
}
