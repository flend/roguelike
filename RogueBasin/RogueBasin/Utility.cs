using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Linq;

namespace RogueBasin
{
    public static class Utility
    {
        public static int d20()
        {
            return 1 + Game.Random.Next(20);
        }

        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> items, int count)
        {
            int i = 0;
            foreach (var item in items)
            {
                if (count == 1)
                    yield return new T[] { item };
                else
                {
                    foreach (var result in GetPermutations(items.Skip(i + 1), count - 1))
                        yield return new T[] { item }.Concat(result);
                }

                ++i;
            }
        }

        public static IEnumerable<IEnumerable<T>> QuickPerm<T>(this IEnumerable<T> set)
        {
            int N = set.Count();
            int[] a = new int[N];
            int[] p = new int[N];

            var yieldRet = new T[N];

            List<T> list = new List<T>(set);

            int i, j, tmp; // Upper Index i; Lower Index j

            for (i = 0; i < N; i++)
            {
                // initialize arrays; a[N] can be any type
                a[i] = i + 1; // a[i] value is not revealed and can be arbitrary
                p[i] = 0; // p[i] == i controls iteration and index boundaries for i
            }
            yield return list;
            //display(a, 0, 0);   // remove comment to display array a[]
            i = 1; // setup first swap points to be 1 and 0 respectively (i & j)
            while (i < N)
            {
                if (p[i] < i)
                {
                    j = i % 2 * p[i]; // IF i is odd then j = p[i] otherwise j = 0
                    tmp = a[j]; // swap(a[j], a[i])
                    a[j] = a[i];
                    a[i] = tmp;

                    //MAIN!

                    for (int x = 0; x < N; x++)
                    {
                        yieldRet[x] = list[a[x] - 1];
                    }
                    yield return yieldRet;
                    //display(a, j, i); // remove comment to display target array a[]

                    // MAIN!

                    p[i]++; // increase index "weight" for i by one
                    i = 1; // reset index i to 1 (assumed)
                }
                else
                {
                    // otherwise p[i] == i
                    p[i] = 0; // reset p[i] to zero
                    i++; // set new index value for i (increase by one)
                } // if (p[i] < i)
            } // while(i < N)
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
                    if (width <= maxWidth || true)
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

        //Includes start and end points
        public static IEnumerable<Point> GetPointsOnLine(Point start, Point end)
        {
            var startV = new Vector3(start.x, start.y, 0);
            var endV = new Vector3(end.x, end.y, 0); 

            var delta = endV - startV;
            delta.Normalize();

            yield return start;

            if (start == end)
                yield break;

            Vector3 currentLocation = startV;
            Point currentPoint;
            Point lastPoint = start;
                do {
                    currentLocation += delta;
                    currentPoint = new Point((int)Math.Round(currentLocation.X), (int)Math.Round(currentLocation.Y));
                    //If we go diagonal in blocks of 1, we will hit the some squares multiple times
                    if(currentPoint != lastPoint)
                        yield return currentPoint;
                    lastPoint = currentPoint;
                } while(currentPoint != end);

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

        /// <summary>
        /// Get the distance in terms of how many steps required
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static int GetPathDistanceBetween(MapObject obj1, MapObject obj2)
        {
            return GetPathDistanceBetween(obj1.LocationMap, obj2.LocationMap);
        }

        public static int GetPathDistanceBetween(Point p1, Point p2)
        {
            List<Point> pts = Utility.GetPointsOnLine(p1, p2).ToList();
            return pts.Count;
        }

        public static int GetPathDistanceBetween(MapObject obj1, Point p2)
        {
            return GetPathDistanceBetween(obj1.LocationMap, p2);
        }

        public static int GetManhattenPathDistanceBetween(Point p1, Point p2)
        {
            return Math.Abs(p1.x - p2.x) + Math.Abs(p1.y - p2.y);
        }

        public static int GetManhattenPathDistanceBetween(MapObject obj1, MapObject obj2)
        {
            return GetManhattenPathDistanceBetween(obj1.LocationMap, obj2.LocationMap);
        }

        public static int GetManhattenPathDistanceBetween(MapObject obj1, Point p2)
        {
            return GetManhattenPathDistanceBetween(obj1.LocationMap, p2);
        }

        /// <summary>
        /// Return the distance between 2 objects on the map
        /// -1 means they are on different levels
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        public static double GetDistanceBetween(MapObject obj1, MapObject obj2)
        {

            if (obj1.LocationLevel != obj2.LocationLevel)
            {
                return -1.0;
            }

            double distance = Math.Sqrt(Math.Pow(obj1.LocationMap.x - obj2.LocationMap.x, 2.0) + Math.Pow(obj1.LocationMap.y - obj2.LocationMap.y, 2.0));
            return distance;
        }

        /// <summary>
        /// Return the distance between an objects and a point on the same level
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        public static double GetDistanceBetween(MapObject obj1, Point p2)
        {
            double distance = Math.Sqrt(Math.Pow(obj1.LocationMap.x - p2.x, 2.0) + Math.Pow(obj1.LocationMap.y - p2.y, 2.0));
            return distance;
        }

        /// <summary>
        /// Return the distance between an objects and a point on the same level
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        public static double GetDistanceBetween(Point p1, Point p2)
        {
            double distance = Math.Sqrt(Math.Pow(p1.x - p2.x, 2.0) + Math.Pow(p1.y - p2.y, 2.0));
            return distance;
        }


        /// <summary>
        /// Range test for consistency
        /// </summary>
        /// <param name="point"></param>
        /// <param name="start"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        internal static bool TestRange(Point x1, Point x2, double range)
        {
            if (Utility.GetDistanceBetween(x1, x2) > range + 0.1)
            {
                return false;
            }
            else
                return true;

        }

        internal static bool TestRange(Point x1, Point x2, int range)
        {
            return TestRange(x1, x2, (double)range);

        }

        internal static bool TestRange(MapObject x1, MapObject x2, int range)
        {
            return TestRange(x1, x2, (double)range);

        }

        internal static bool TestRange(MapObject x1, MapObject x2, double range)
        {
            return TestRange(x1.LocationMap, x2.LocationMap, range);

        }

        /// <summary>
        /// Test if target is in range of shooter and in FOV (normal gun check). Needs a calculated fov for shooter
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="range"></param>
        /// <param name="fov"></param>
        /// <returns></returns>
        internal static bool TestRangeFOVForWeapon(MapObject shooter, MapObject target, double range, CreatureFOV fov)
        {
            //Check range
            if (!Utility.TestRange(shooter, target, range))
            {
                return false;
            }
            //Check FOV
            return fov.CheckTileFOV(target.LocationMap.x, target.LocationMap.y);

        }

        /// <summary>
        /// Test if target is in range of shooter and in FOV (normal gun check). Needs a calculated fov for shooter
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="range"></param>
        /// <param name="fov"></param>
        /// <returns></returns>
        internal static bool TestRangeFOVForWeapon(MapObject shooter, Point target, double range, CreatureFOV fov)
        {
            //Check range
            if (!Utility.TestRange(shooter.LocationMap, target, range))
            {
                return false;
            }
            //Check FOV
            return fov.CheckTileFOV(target.x, target.y);

        }

    }
}
