using System;
using System.IO;
using System.Text;

namespace RogueBasin
{
    class MapExport
    {
        public MapExport()
        {

        }

        public void ExportMapToTextFile(Map mapToExport, string filename)
        {
            try
            {
                //Open logfile
                Directory.CreateDirectory("maps");
                using (StreamWriter writer = new StreamWriter("maps/" + filename, false))
                {
                    for (int j = 0; j < mapToExport.height; j++)
                    {
                        StringBuilder mapLine = new StringBuilder();

                        for (int i = 0; i < mapToExport.width; i++)
                        {
                            //Defaults
                            char screenChar = StringEquivalent.TerrainChars[mapToExport.mapSquares[i, j].Terrain];
                            mapLine.Append(screenChar);
                        }
                        writer.WriteLine(mapLine);
                    }
                }
            }
            catch (Exception e)
            {
                LogFile.Log.LogEntryDebug("Failed to write file " + e.Message, LogDebugLevel.High);
            }
        }
    }
}
