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
                { return str.Substring(0, cutIndex).Trim() + appendWhenCut; }
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
                        wrappedMsgs.Add(trimmedMsg);
                        //make our allMsgs smaller
                        allMsgs = allMsgs.Substring(trimmedMsg.Length);
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
    }
}
