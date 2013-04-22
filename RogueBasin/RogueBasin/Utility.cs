using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace RogueBasin
{
    static class Utility
    {
        public static int d20()
        {
            return 1 + Game.Random.Next(20);
        }

        public static int DamageRoll(int damageBase)
        {
            return 1 + Game.Random.Next(damageBase);
        }

        /// <summary>
        /// Properly cuts the string on a white space and append appendWhenCut when cutted.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="appendWhenCut"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string SubstringWordCut(string str, string appendWhenCut, uint maxLength)
        {
            if (str.Length > maxLength)
            {
                str = str.Substring(0, (int)maxLength - appendWhenCut.Length);
                char[] cutPossible = new char[] { ' ', ',', '.', '?', '!', ':', ';', '-', '\n', '\r', '\t' };
                int cutIndex = str.LastIndexOfAny(cutPossible);
                if (cutIndex > 0)
                { return str.Substring(0, cutIndex) + appendWhenCut; }
                else
                { return str.Substring(0, (int)maxLength - appendWhenCut.Length) + appendWhenCut; }
            }
            return str;
        }

        /// <summary>
        /// Properly cuts the string on a white space and append appendWhenCut when cutted.
        /// Doesn't Trim so the length of returned string can be used to calculate how much has been taken off
        /// </summary>
        /// <param name="me"></param>
        /// <param name="appendWhenCut"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string SubstringWordCutAndNormalise(string str, string appendWhenCut, uint maxLength)
        {
            //Firstly replace any unusual white space with spaces
            str = str.Replace("\r\n", " ");
            str = str.Replace('\r', ' ');
            str = str.Replace('\n', ' ');

            if (str.Length > maxLength)
            {
                str = str.Substring(0, (int)maxLength - appendWhenCut.Length);
                char[] cutPossible = new char[] { ' ', ',', '.', '?', '!', ':', ';', '-', '\n', '\r', '\t' };
                int cutIndex = str.LastIndexOfAny(cutPossible);
                if (cutIndex > 0)
                { return str.Substring(0, cutIndex) + appendWhenCut; }
                else
                { return str.Substring(0, (int)maxLength - appendWhenCut.Length) + appendWhenCut; }
            }
            return str;
        }

        public static string RandomHiddenDescription()
        {
            //Return a random string . Need at least 14
            List<string> hiddenDesc = new List<string>() { "gold", "brown", "purple", "red", "orange", "metallic", "shiny", "effervesent", "sparkling", "black", "green", "putrid",
            "transparent", "opaque", "bubbling", "hissing", "sticky", "syrupy", "corrosive", "sweet-smelling", "dark" };

            return hiddenDesc[Game.Random.Next(hiddenDesc.Count)];

        }

        /// <summary>
        /// Load text file (.txt appended) and return as strings. Takes a maximum width (width) and sets the height
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static List<string> LoadTextFile(string filenameRoot, int maxWidth, out int height)
        {
            try
            {
                LogFile.Log.LogEntry("Loading text file: " + filenameRoot);

                Assembly _assembly = Assembly.GetExecutingAssembly();

                //MessageBox.Show("Showing all embedded resource names");

                //string[] names = _assembly.GetManifestResourceNames();
                //foreach (string name in names)
                //    MessageBox.Show(name);


                string filename = "RogueBasin.bin.Debug.text." + filenameRoot + ".txt";
                Stream _fileStream = _assembly.GetManifestResourceStream(filename);


                List<string> inputLines = new List<string>();

                //string currentFilename = filename + ".txt";

                //If this is the first frame check if there is at least one frame
                // if (!File.Exists(currentFilename))
                //{
                //    throw new ApplicationException("Can't find file: " + currentFilename);
                //}
                if (_fileStream == null)
                {
                    //LogFile.Log.LogEntry("can't find file: " + filename);
                    throw new ApplicationException("Can't find file");
                }

                //File exists, load the file
                using (StreamReader reader = new StreamReader(_fileStream))
                {
                    string thisLine;

                    inputLines = new List<string>();

                    while ((thisLine = reader.ReadLine()) != null)
                    {
                        inputLines.Add(thisLine);
                    }

                    //Calculate dimensions
                    int width = 0;

                    foreach (string row in inputLines)
                    {
                        if (row.Length > width)
                            width = row.Length;
                    }

                    //Do we need to wrap
                    if (width <= maxWidth)
                    {
                        //No, set height and return
                        height = inputLines.Count;

                        return inputLines;
                    }

                    //Yes, wrap

                    List<string> wrappedMsg = new List<string>();

                    //Stick all the messages together in one long string
                    //TODO: StringBuilder
                    string allMsgs = "";
                    foreach (string row in inputLines)
                    {
                        allMsgs += row + " ";
                    }

                    //Strip off the last piece of white space
                    allMsgs = allMsgs.Trim();

                    //Now make a list of trimmed msgs with <more> appended
                    List<string> wrappedMsgs = new List<string>();
                    do
                    {
                        //put function in utility
                        string trimmedMsg = Utility.SubstringWordCut(allMsgs, "", (uint)maxWidth);
                        int charsUsed = trimmedMsg.Length;
                        wrappedMsgs.Add(trimmedMsg.Trim());
                        //make our allMsgs smaller
                        allMsgs = allMsgs.Substring(charsUsed);
                    } while (allMsgs.Length > 0);

                    //Set height
                    height = wrappedMsgs.Count;

                    return wrappedMsgs;
                }
            }
            catch (Exception e)
            {
                LogFile.Log.LogEntry("Failed to load text file: " + e.Message);
                //This is unlikely to be a particularly bad error, so don't rethrow, just return null
                height = -1;
                return new List<string>();
            }
        }

        public static bool DoesSaveGameExist(string playerName)
        {
            //Save game filename
            string filename = playerName + ".sav";

            if (File.Exists(filename))
            {
                return true;
            }

            return false;
        }

        public static IEnumerable<Point> GetPointsOnLine(int x0, int y0, int x1, int y1)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                int t;
                t = x0; // swap x0 and y0
                x0 = y0;
                y0 = t;
                t = x1; // swap x1 and y1
                x1 = y1;
                y1 = t;
            }
            if (x0 > x1)
            {
                int t;
                t = x0; // swap x0 and x1
                x0 = x1;
                x1 = t;
                t = y0; // swap y0 and y1
                y0 = y1;
                y1 = t;
            }
            int dx = x1 - x0;
            int dy = Math.Abs(y1 - y0);
            int error = dx / 2;
            int ystep = (y0 < y1) ? 1 : -1;
            int y = y0;
            for (int x = x0; x <= x1; x++)
            {
                yield return new Point((steep ? y : x), (steep ? x : y));
                error = error - dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
            yield break;
        }

        /// <summary>
        /// See http://stackoverflow.com/questions/1322510/given-an-integer-how-do-i-find-the-next-largest-power-of-two-using-bit-twiddlin
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static uint nextPowerOf2(uint n) {

            n--;
            n |= n >> 1;   // Divide by 2^k for consecutive doublings of k up to 32,
            n |= n >> 2;   // and then or the results.
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            return ++n;        
        }
    

    }
}
