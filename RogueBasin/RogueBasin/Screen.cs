using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;
using Console = System.Console;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

namespace RogueBasin {

    //Represents our screen
    public class Screen
    {
        static Screen instance = null;

        //Console/screen size
        public int Width { get; set; }
        public int Height { get; set; }

        public bool DebugMode { get; set; }

        /// <summary>
        /// Show flashes on attacks and thrown projectiles
        /// </summary>
        public bool CombatAnimations { get; set; }

        public bool SetTargetInRange = false;

        //Top left coord to start drawing the map at
        Point mapTopLeft;

        /// <summary>
        /// Dimensions of message display area
        /// </summary>
        Point msgDisplayTopLeft;
        public int msgDisplayNumLines;

        Point statsDisplayTopLeft;
        Point princessStatsTopLeft;

        Point hitpointsOffset;
        Point maxHitpointsOffset;
        Point overdriveHitpointsOffset;
        Point speedOffset;
        Point worldTickOffset;
        Point levelOffset;

        Point armourOffset;
        Point damageOffset;
        Point playerLevelOffset;

       

        Point specialMoveStatusLine;
        Point spellStatusLine;
        Point trainStatsLine;

        Point calendarOffset;

        Color inFOVTerrainColor = ColorPresets.White;
        Color seenNotInFOVTerrainColor = ColorPresets.Gray;
        Color neverSeenFOVTerrainColor;
        Color inMonsterFOVTerrainColor = ColorPresets.Blue;


        Color statsColor = ColorPresets.White;

        Color creatureColor = ColorPresets.White;
        Color itemColor = ColorPresets.Red ;
        Color featureColor = ColorPresets.White;

        Color hiddenColor = ColorPresets.Black;

        Color charmBackground = ColorPresets.DarkKhaki;
        Color passiveBackground = ColorPresets.DarkMagenta;
        Color uniqueBackground = ColorPresets.DarkCyan;
        Color inRangeBackground = ColorPresets.DeepSkyBlue;
        Color inRangeAndAggressiveBackground = ColorPresets.Purple;
        Color stunnedBackground = ColorPresets.DarkCyan;
        Color investigateBackground = ColorPresets.DarkGreen;
        Color pursuitBackground = ColorPresets.DarkRed;
        Color normalBackground = ColorPresets.Black;
        Color normalForeground = ColorPresets.White;
        Color targettedBackground = ColorPresets.DarkGray;

        Color targetBackground = ColorPresets.White;
        Color targetForeground = ColorPresets.Black;

        Color literalColor = ColorPresets.BurlyWood;
        Color literalTextColor = ColorPresets.White;

        Color headingColor = ColorPresets.Yellow;

        Color messageColor = ColorPresets.White;

        Color soundColor = ColorPresets.Yellow;

        //Keep enough state so that we can draw each screen
        string lastMessage = "";

        //Inventory
        Point inventoryTL;
        Point inventoryTR;
        Point inventoryBL;

        //Training
        Point trainingTL;
        Point trainingTR;
        Point trainingBL;

        //For examining
        public Monster CreatureToView { get; set; }
        public Item ItemToView { get; set; }

        bool displayInventory;

        const int missileDelay = 250;
        const int meleeDelay = 100;
        
        /// <summary>
        /// Equipment screen is displayed
        /// </summary>
        bool displayEquipment;

        /// <summary>
        /// Select new equipment screen is displayed
        /// </summary>
        bool displayEquipmentSelect;

        bool displaySpecialMoveMovies;

        bool displaySpells;

        bool displayTrainingUI;

        public int MsgLogWrapWidth { get; set; }

        //Death members
        public List<string> TotalKills { get; set; }
        public List<string> DeathPreamble { get; set; }

        Point DeathTL { get; set; }
        int DeathWidth { get; set; }
        int DeathHeight { get; set; }

        int selectedInventoryIndex;
        int topInventoryIndex;

        Inventory currentInventory;
        List<EquipmentSlotInfo> currentEquipment;
        string inventoryTitle;
        string inventoryInstructions;

        Point movieTL = new Point(5, 5);
        int movieWidth = 80;
        int movieHeight = 25;
        uint movieMSBetweenFrames = 500;

        /// <summary>
        /// Targetting mode
        /// </summary>
        bool targettingMode = false;

        /// <summary>
        /// Targetting cursor
        /// </summary>
        public Point Target { get; set; }

        public TargettingType TargetType { get; set; }

        public int TargetRange { get; set; }
        public double TargetPermissiveAngle { get; set; }

        //Current movie
        List <MovieFrame> movieFrames;

        public Color PCColor { get; set;}

        public bool SeeAllMonsters { get; set; }
        public bool SeeAllMap { get; set; }

        public static Screen Instance
        {
            get
            {
                if (instance == null)
                    instance = new Screen();
                return instance;
            }
        }


        Screen()
        {
            Width = 90;
            Height = 35;

            DebugMode = false;
            CombatAnimations = true;

            mapTopLeft = new Point(5, 5);

            msgDisplayTopLeft = new Point(0, 1);
            msgDisplayNumLines = 3;

            inventoryTL = new Point(5, 5);
            inventoryTR = new Point(85, 5);
            inventoryBL = new Point(5, 30);

            trainingTL = new Point(15, 10);
            trainingTR = new Point(75, 10);
            trainingBL = new Point(15, 25);

            princessStatsTopLeft = new Point(7, 32);

            MsgLogWrapWidth = inventoryTR.x - inventoryTL.x - 4;

            calendarOffset = new Point(20, 0);

            specialMoveStatusLine = new Point(7, 33);
            spellStatusLine = new Point(7, 34);
            trainStatsLine = new Point(7, 30);
           
            //Colors
            neverSeenFOVTerrainColor = Color.FromRGB(90, 90, 90);

            TotalKills = null;

            DeathTL = new Point(1, 1);
            DeathWidth = 89;
            DeathHeight = 34;

            PCColor = ColorPresets.White;

            trainingStatsRecord = new List<TrainStats>();

            SeeAllMonsters = true;
            SeeAllMap = true;
        }

        //Setup the screen
        public void InitialSetup()
        {

            CustomFontRequest fontReq = new CustomFontRequest("tallfont.png", 8, 16, CustomFontRequestFontTypes.LayoutAsciiInColumn);
            RootConsole.Width = Width;
            RootConsole.Height = Height;
            RootConsole.WindowTitle = "FlatlineRL";
            RootConsole.Fullscreen = false;
            RootConsole.Font = fontReq;
            /*
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.PrintLine("Hello world!", 30, 30, LineAlignment.Left);
            rootConsole.Flush();
            */
            Console.WriteLine("debug test message.");

        }

        

        public void DrawTriangularTarget(Point origin, Point target, int range, double spreadAngle, Color foregroundColor, Color backgroundColor, char drawChar)
        {

            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.ForegroundColor = foregroundColor;
            rootConsole.BackgroundColor = backgroundColor;

            List<Point> splashSquares = GetPointsForTriangularTarget(origin, target, range, spreadAngle);
            foreach (Point p in splashSquares)
            {
                rootConsole.PutChar(p.x, p.y, drawChar);
            }

            rootConsole.ForegroundColor = normalForeground;
            rootConsole.BackgroundColor = normalBackground;

        }


        public void DrawCircularTarget(Point location, Color foregroundColor, Color backgroundColor, int size, char drawChar)
        {

            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.ForegroundColor = foregroundColor;
            rootConsole.BackgroundColor = backgroundColor;

            List<Point> splashSquares = GetPointsForCircularTarget(location, size);
            foreach (Point p in splashSquares)
            {
                rootConsole.PutChar(p.x, p.y, drawChar);
            }

            rootConsole.ForegroundColor = normalForeground;
            rootConsole.BackgroundColor = normalBackground;

        }


        /// <summary>
        /// Draw a fireball effect
        /// </summary>
        /// <param name="sqs"></param>
        /// <param name="color"></param>
        public void DrawFlashSquares(List<Point> sqs, Color color)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw the screen as normal
            Draw();

            //Draw the flash overlay

            foreach (Point sq in sqs)
            {
                rootConsole.ForegroundColor = color;
                rootConsole.PutChar(mapTopLeft.x + sq.x, mapTopLeft.y + sq.y, '*');
            }

            rootConsole.ForegroundColor = normalForeground;

            FlushConsole();

            //Wait
            TCODSystem.Sleep(200);

            //Redraw
            Draw();
            FlushConsole();
        }

        /// <summary>
        /// Returns the points in a triangular target from origin to target
        /// </summary>
        /// <param name="location"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public List<Point> GetPointsForTriangularTarget(Point origin, Point target, int range, double fovAngle)
        {
            List<Point> triangularPoints = new List<Point>();

            double angle = DirectionUtil.AngleFromOriginToTarget(origin, target);

            for (int i = origin.x - range; i < origin.x + range; i++)
            {
                for (int j = origin.y - range; j < origin.y + range; j++)
                {
                    if (i >= 0 && i < this.Width && j >= 0 && j < this.Height)
                    {
                        if (CreatureFOV.TriangularFOV(origin, angle, range, i, j, fovAngle))
                        {
                            triangularPoints.Add(new Point(i, j));
                        }
                    }
                }
            }

            return triangularPoints;
        }

        /// <summary>
        /// Returns the points in a circular target
        /// </summary>
        /// <param name="location"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public List<Point> GetPointsForCircularTarget(Point location, int size)
        {
            List<Point> splashSquares = new List<Point>();

            for (int i = location.x - size; i < location.x + size; i++)
            {
                for (int j = location.y - size; j < location.y + size; j++)
                {
                    if (i >= 0 && i < Width && j >= 0 && j < Height)
                    {

                        if (Math.Pow(i - location.x, 2) + Math.Pow(j - location.y, 2) < Math.Pow(size, 2))
                        {
                            splashSquares.Add(new Point(i, j));
                        }
                    }
                }
            }

            return splashSquares;
        }

        /// <summary>
        /// Get points on a line in order.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<Point> GetPathLinePoints(Point start, Point end)
        {
            List<Point> pointsToRet = new List<Point>();

            TCODLineDrawing.InitLine(start.x, start.y, end.x, end.y);
            //Don't draw the first char (where the player is)

            int currentX = start.x;
            int currentY = start.y;

            bool finishedLine = false;

            do
            {
                int lastX = currentX;
                int lastY = currentY;

                finishedLine = TCODLineDrawing.StepLine(ref currentX, ref currentY);

                if (currentX >= 0 && currentX < Width && currentY >= 0 && currentY < Height)
                    pointsToRet.Add(new Point(currentX, currentY));
            } while (!finishedLine);

            return pointsToRet;
        }

        /// <summary>
        /// Draw a flash effect of a line
        /// Start is the origin (line is not drawn here, but origin used to calculate shape of line)
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="color"></param>
        public void DrawFlashLine(Point start, Point end, Color color)
        {
            DrawFlashLine(start, end, color, 200, true);
        }

        protected char LineChar(int deltaX, int deltaY) {

            char drawChar = '-';

            if (deltaX < 0 && deltaY < 0)
                drawChar = '\\';
            else if (deltaX < 0 && deltaY > 0)
                drawChar = '/';
            else if (deltaX > 0 && deltaY < 0)
                drawChar = '/';
            else if (deltaX > 0 && deltaY > 0)
                drawChar = '\\';
            else if (deltaY == 0)
                drawChar = '-';
            else if (deltaX == 0)
                drawChar = '|';

            return drawChar;
        }

        /// <summary>
        /// Draw a line following a path on the console.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="color"></param>
        protected void DrawPathLine(Point start, Point end, Color foregroundColor, Color backgroundColor)
        {
            DrawPathLine(start, end, foregroundColor, backgroundColor, (char)0);
        }

        /// <summary>
        /// Draw a line following a path on the console. Override default line drawing
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="color"></param>
        protected void DrawPathLine(Point start, Point end, Color foregroundColor, Color backgroundColor, char drawChar)
        {

            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw the line overlay

            //Cast a line between the start and end
            TCODLineDrawing.InitLine(start.x, start.y, end.x, end.y);
            //Don't draw the first char (where the player is)

            rootConsole.ForegroundColor = foregroundColor;
            rootConsole.BackgroundColor = backgroundColor;

            int currentX = start.x;
            int currentY = start.y;

            bool finishedLine = false;

            do {
                int lastX = currentX;
                int lastY = currentY;

                finishedLine = TCODLineDrawing.StepLine(ref currentX, ref currentY);

                char c;
                if (drawChar == 0)
                    c = LineChar(currentX - lastX, currentY - lastY);
                else
                    c = drawChar;

                rootConsole.PutChar(currentX, currentY, c);
            } while (!finishedLine);
                        
            rootConsole.ForegroundColor = normalForeground;
            rootConsole.BackgroundColor = normalBackground;
        }


        public void DrawFlashLine(Point start, Point end, Color color, int timeMS, bool screenUpdate) {

            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw the screen as normal
            Draw();

            //Draw the line overlay

            //Cast a line between the start and end
            TCODLineDrawing.InitLine(start.x, start.y, end.x, end.y);

            int currentX = start.x;
            int currentY = start.y;

            bool finishedLine = TCODLineDrawing.StepLine(ref currentX, ref currentY);
            
            int deltaX = end.x - start.x;
            int deltaY = end.y - start.y;

            char drawChar = '-';

            if(deltaX < 0 && deltaY < 0)
                drawChar = '\\';
            else if(deltaX < 0 && deltaY > 0)
                drawChar = '/';
            else if(deltaX > 0 && deltaY < 0)
                drawChar = '/';
            else if(deltaX > 0 && deltaY > 0)
                drawChar = '\\';
            else if(deltaY == 0)
                drawChar = '-';
            else if(deltaX == 0)
                drawChar = '|';

            rootConsole.ForegroundColor = color;

            while(!finishedLine) {

                rootConsole.PutChar(mapTopLeft.x + currentX, mapTopLeft.y + currentY, drawChar);
                finishedLine = TCODLineDrawing.StepLine(ref currentX, ref currentY);

            }

            rootConsole.ForegroundColor = normalForeground;

            //For screenshots only
            //KeyPress userKey = Keyboard.WaitForKeyPress(true);

            //Redraw
            if (screenUpdate)
            {
                //Draw line overlay
                FlushConsole();

                //Wait
                TCODSystem.Sleep((uint)timeMS);

                //Draw normally
                Draw();
                FlushConsole();
            }
        }

        /// <summary>
        /// Call after all drawing is complete to output onto screen
        /// </summary>
        public void FlushConsole()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.Flush();
        }

        

        public void TargettingModeOn() {
            targettingMode = true;
        }

        public void TargettingModeOff()
        {
            targettingMode = false;
        }

        /// <summary>
        /// Get the text from a movie
        /// </summary>
        /// <param name="movieRoot"></param>
        /// <returns></returns>
        public List<string> GetMovieText(string movieRoot)
        {
            bool loadSuccess = Screen.Instance.LoadMovie(movieRoot);

            if (!loadSuccess)
            {
                LogFile.Log.LogEntryDebug("Failed to load movie file: " + movieRoot, LogDebugLevel.High);
                return new List<string>();
            }

            List<string> outputText = new List<string>();

            //Concatenate the movie into a string list
            foreach (MovieFrame frame in movieFrames)
            {
                if (outputText.Count > 0)
                    outputText.Add("\n");

                outputText.AddRange(frame.scanLines);
            }

            return outputText;

        }

        /// <summary>
        /// Play the movie indicated by the filename root.
        /// </summary>
        /// <param name="root"></param>
        /// 
        Color normalMovieColor = ColorPresets.White;
        Color flashMovieColor = ColorPresets.Red;

        public void PlayMovie(string filenameRoot, bool keypressBetweenFrames)
        {
            if (filenameRoot == "" || filenameRoot.Length == 0)
            {
                LogFile.Log.LogEntryDebug("Not playing movie with no name", LogDebugLevel.Medium);
                return;
            }

            try
            {

                //Draw the basis of the screen
                Draw();

                //Get screen handle
                RootConsole rootConsole = RootConsole.GetInstance();

                //Load whole movie
                bool loadSuccess = LoadMovie(filenameRoot);

                if (!loadSuccess)
                {
                    LogFile.Log.LogEntryDebug("Failed to load movie file: " + filenameRoot, LogDebugLevel.High);
                    return;
                }

                //Use the width and height of the first frame to centre the movie
                //Unlikely to be any control codes on the first line
                int width = movieFrames[0].width;
                int height = movieFrames[0].height;

                int xOffset = (movieWidth - width) / 2;
                int yOffset = (movieHeight - height) / 2;

                Point frameTL = new Point(movieTL.x + xOffset, movieTL.y + yOffset);
                
                int frameNo = 0;

                //Draw each frame of the movie
                foreach (MovieFrame frame in movieFrames)
                {

                    //Draw frame
                    rootConsole.DrawFrame(movieTL.x, movieTL.y, movieWidth, movieHeight, true);

                    //Draw content
                    List<string> scanLines = frame.scanLines;

                    bool hasFlashingChars = DrawMovieFrame(frame.scanLines, frameTL, width, true);

                    if (hasFlashingChars)
                    {
                        //Wait and then redraw without the highlight to make a flash effect
                        Screen.Instance.FlushConsole();
                        TCODSystem.Sleep(movieMSBetweenFrames);
                        DrawMovieFrame(frame.scanLines, frameTL, width, false);
                    }

                    
                    if (keypressBetweenFrames == true)
                    {
                        //Don't ask for a key press if it's the last frame, one will happen below automatically
                        if (frameNo != movieFrames.Count - 1)
                        {
                            rootConsole.PrintLineRect("Press any key to continue", movieTL.x + movieWidth / 2, movieTL.y + movieHeight - 2, movieWidth, 1, LineAlignment.Center);
                            Screen.Instance.FlushConsole();
                            KeyPress userKey = Keyboard.WaitForKeyPress(true);
                        }
                    }
                    else
                    {
                        //Wait for the specified time

                        Screen.Instance.FlushConsole();
                        TCODSystem.Sleep(movieMSBetweenFrames);
                    }

                    frameNo++;
                }

                //Print press any key
                rootConsole.PrintLineRect("Press ENTER to continue", movieTL.x + movieWidth / 2, movieTL.y + movieHeight - 2, movieWidth, 1, LineAlignment.Center);

                Screen.Instance.FlushConsole();

                //Await keypress then redraw normal screen
                WaitForEnterKey();

                UpdateNoMsgQueue();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to play movie: " + filenameRoot + " : " + ex.Message);
            }
        }

        /// <summary>
        /// Wait for ENTER
        /// </summary>
        private void WaitForEnterKey()
        {
            while (true)
            {
                KeyPress userKey = Keyboard.WaitForKeyPress(true);

                if (userKey.KeyCode == KeyCode.TCODK_ENTER
                    || userKey.KeyCode == KeyCode.TCODK_KPENTER)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Draw a frame. If flashOn then highlight flashing squares in red
        /// </summary>
        /// <param name="scanLines"></param>
        /// <param name="frameTL"></param>
        /// <param name="width"></param>
        /// <param name="flashOn"></param>
        private bool DrawMovieFrame(List<string> scanLines, Point frameTL, int width, bool flashOn)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            int offset = 0;

            bool flashingChars = false;
            char flashChar = '£';

            foreach (string line in scanLines)
            {
                //Check for special characters
                if (line.Contains(flashChar.ToString()))
                {
                    //We will return this, so that the caller knows to call us again with flashOn = false
                    flashingChars = true;

                    //Print char by char
                    int coffset = 0;
                    bool nextCharFlash = false;
                    foreach (char c in line)
                    {
                        if (c == flashChar)
                        {
                            if (flashOn)
                            {
                                nextCharFlash = true;
                            }
                            //Skip this char
                            continue;
                        }

                        if (nextCharFlash)
                        {
                            rootConsole.ForegroundColor = flashMovieColor;
                            nextCharFlash = false;
                        }
                        else
                        {
                            rootConsole.ForegroundColor = normalMovieColor;
                        }

                        rootConsole.PutChar(frameTL.x + coffset, frameTL.y + offset, c);
                        coffset++;
                    }

                    //Reset flash color at the end of the line
                    rootConsole.ForegroundColor = normalMovieColor;
                }
                else
                {
                    //Print whole line
                    rootConsole.PrintLineRect(line, frameTL.x, frameTL.y + offset, width, 1, LineAlignment.Left);
                }
                offset++;
            }

            return flashingChars;
        }

        public bool LoadMovie(string filenameRoot)
        {
            try
            {
                LogFile.Log.LogEntry("Loading movie: " + filenameRoot);

                int frameNo = 0;

                movieFrames = new List<MovieFrame>();

                Assembly _assembly = Assembly.GetExecutingAssembly();

                //MessageBox.Show("Showing all embedded resource names");

                //string[] names = _assembly.GetManifestResourceNames();
                //foreach (string name in names)
                //    MessageBox.Show(name);

                do
                {
                    string filename = "RogueBasin.bin.Debug.movies." + filenameRoot + frameNo.ToString() + ".amf";
                    Stream _fileStream = _assembly.GetManifestResourceStream(filename);

                    //If this is the first frame check if there is at least one frame
                    if (frameNo == 0)
                    {
                        if (_fileStream == null)
                        {
                            throw new ApplicationException("Can't find file: " + filename);
                        }
                    }
                    //Otherwise, not finding a file just means the end of a movie

                    if (_fileStream == null)
                    {
                        break;
                    }

                    //File exists, load the frame
                    MovieFrame frame = new MovieFrame();

                    using (StreamReader reader = new StreamReader(_fileStream))
                    {
                        string thisLine;

                        frame.scanLines = new List<string>();

                        while ((thisLine = reader.ReadLine()) != null)
                        {
                            frame.scanLines.Add(thisLine);
                        }

                        //Set width and height

                        //Calculate dimensions
                        frame.width = 0;

                        foreach (string row in frame.scanLines)
                        {
                            if (row.Length > frame.width)
                                frame.width = row.Length;
                        }

                        frame.height = frame.scanLines.Count;

                        //Add the frame
                        movieFrames.Add(frame);

                        //Increment the frame no
                        frameNo++;
                    }
                } while (true);

                return true;
            }
            catch (Exception e)
            {
                LogFile.Log.LogEntry("Failed to load movie: " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Draws and updates the screen. Doesn't run message queue. Is this function really used? I think all the calls don't
        /// </summary>
        public void UpdateNoMsgQueue()
        {
            Screen.Instance.Draw();
            Screen.Instance.FlushConsole();
        }

        //Draw the current dungeon map and objects
        private void Draw()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            Dungeon dungeon = Game.Dungeon;
            Player player = dungeon.Player;

            //Clear screen
            rootConsole.Clear();

            //Draw the map screen

            //Draw terrain
            DrawMap(dungeon.PCMap);

            //Draw fixed features
            DrawFeatures(dungeon.Features);

            //Draw items (will appear on top of staircases etc.)
            DrawItems(dungeon.Items);

            //Draw creatures
            DrawCreatures(dungeon.Monsters);

            //Draw PC

            Point PClocation = player.LocationMap;
            Color PCDrawColor = PCColor;
            

            if (DebugMode)
            {
                MapSquare pcSquare = Game.Dungeon.Levels[player.LocationLevel].mapSquares[player.LocationMap.x, player.LocationMap.y];

                if (pcSquare.InMonsterFOV)
                {
                    PCDrawColor = Color.Interpolate(PCDrawColor, ColorPresets.Red, 0.4);
                }
            }

            rootConsole.ForegroundColor = PCDrawColor;
            rootConsole.PutChar(mapTopLeft.x + PClocation.x, mapTopLeft.y + PClocation.y, player.Representation);
            rootConsole.ForegroundColor = ColorPresets.White;

            //Draw Stats
            DrawStats(dungeon.Player);

            //Draw targetting cursor
            if (targettingMode)
                DrawTargettingCursor();

            //Draw any overlay screens
            if (displayInventory)
                DrawInventory();
            else if (displayEquipment)
                DrawEquipment();
            else if (displayEquipmentSelect)
                DrawEquipmentSelect();
            else if (displaySpecialMoveMovies)
                DrawMovieOverlay();
            else if (displaySpells)
                DrawSpellOverlay();
            else if (displayTrainingUI)
                DrawTrainingOverlay();
            else if (ShowXPScreen)
                DrawXPOverlay();
            else if (ShowMsgHistory)
                DrawMsgHistory();

        }

        /// <summary>
        /// Draw a shotgun blast. Pass in the actual damage sqs, since they are calculated in the OnFire()
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <param name="size"></param>
        public void DrawAreaAttack(List <Point> targetSquares)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();
            
            rootConsole.ForegroundColor = ColorPresets.Red;
            rootConsole.BackgroundColor = ColorPresets.Black;

            //Clone the list since we mangle it
            List<Point> mangledPoints = new List<Point>();
            foreach (Point p in targetSquares)
            {
                mangledPoints.Add(new Point(p));
            }

            //Offset origin and target onto map
            foreach (Point p in mangledPoints)
            {
                p.x += mapTopLeft.x;
                p.y += mapTopLeft.y;

                rootConsole.PutChar(p.x, p.y, '*');
            }

            rootConsole.ForegroundColor = normalForeground;

            //Draw line overlay
            FlushConsole();

            //Wait
            TCODSystem.Sleep(missileDelay);

            //Draw normally
            Draw();
            FlushConsole(); 
        }


        private void DrawTargettingCursor()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            Player player = Game.Dungeon.Player;

            int playerX = mapTopLeft.x + player.LocationMap.x;
            int playerY = mapTopLeft.y + player.LocationMap.y;

            int xLoc = mapTopLeft.x + Target.x;
            int yLoc = mapTopLeft.y + Target.y;

            //Draw the target

            char charAtPoint = rootConsole.GetChar(xLoc, yLoc);

            //Draw the area of effect

            switch (TargetType)
            {
                case TargettingType.Line:

  
                    //Draw a line up to the target
                    DrawPathLine(new Point(playerX, playerY), new Point(xLoc, yLoc), targetForeground, targetBackground);
                    break;

                case TargettingType.LineThrough:

                    //Cache original contents
                    List<Point> lineSquares = GetPathLinePoints(new Point(playerX, playerY), new Point(xLoc, yLoc));
                    List<char> linecontents = new List<char>();
                    foreach (Point p in lineSquares)
                    {
                        linecontents.Add(rootConsole.GetChar(p.x, p.y));
                    }

                    //Draw a line up to the target
                    DrawPathLine(new Point(playerX, playerY), new Point(xLoc, yLoc), targetForeground, targetBackground);

                    rootConsole.BackgroundColor = targetBackground;
                    rootConsole.ForegroundColor = ColorPresets.Red;

                    //Draw original contents back
                    int index = 0;
                    foreach (Point p in lineSquares)
                    {
                        rootConsole.PutChar(p.x, p.y, linecontents[index]);
                        index++;
                    }
                    break;

                case TargettingType.Rocket:
                    {
                        int size = 2;

                        //Cache original contents
                        List<Point> splashSquares = GetPointsForCircularTarget(new Point(xLoc, yLoc), size);
                        List<char> contents = new List<char>();
                        foreach (Point p in splashSquares)
                        {
                            contents.Add(rootConsole.GetChar(p.x, p.y));
                        }

                        //Draw a line up to the target
                        DrawPathLine(new Point(playerX, playerY), new Point(xLoc, yLoc), targetForeground, targetBackground);
                        //Circular target on the target
                        DrawCircularTarget(new Point(xLoc, yLoc), ColorPresets.Red, targetBackground, size, '*');

                        rootConsole.BackgroundColor = targetBackground;
                        rootConsole.ForegroundColor = ColorPresets.Red;

                        //Draw original contents back
                        int index2 = 0;
                        foreach (Point p in splashSquares)
                        {
                            rootConsole.PutChar(p.x, p.y, contents[index2]);
                            index2++;
                        }
                    }
                    break;

                case TargettingType.Shotgun:
                    {
                        int size = TargetRange;
                        double spreadAngle = TargetPermissiveAngle;

                        //Cache original contents
                        //List<Point> splashSquares = GetPointsForTriangularTarget(new Point(playerX, playerY), new Point(xLoc, yLoc), size);
                        
                        //Using FOV-modified version for extra accuracy!
                        CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);
                        List<Point> splashSquares = currentFOV.GetPointsForTriangularTargetInFOV(player.LocationMap, Target, size, spreadAngle);
                        //offset
                        foreach(Point p in splashSquares) {
                            p.x += mapTopLeft.x;
                            p.y += mapTopLeft.y;
                        }

                        List<char> contents = new List<char>();
                        foreach (Point p in splashSquares)
                        {
                            contents.Add(rootConsole.GetChar(p.x, p.y));
                        }

                        //Triangle target on the player
                        DrawTriangularTarget(new Point(playerX, playerY), new Point(xLoc, yLoc), size, spreadAngle, ColorPresets.Red, targetBackground, '*');
                        
                        rootConsole.BackgroundColor = targetBackground;
                        rootConsole.ForegroundColor = ColorPresets.Red;

                        //Draw original contents back
                        int index2 = 0;
                        foreach (Point p in splashSquares)
                        {
                            rootConsole.PutChar(p.x, p.y, contents[index2]);
                            index2++;
                        }
                    }
                    break;
            }

            if (SetTargetInRange)
            {
                rootConsole.BackgroundColor = ColorPresets.Yellow;
                rootConsole.ForegroundColor = targetForeground;
            }
            else
            {
                rootConsole.BackgroundColor = targetBackground;
                rootConsole.ForegroundColor = targetForeground;
            }

            rootConsole.PutChar(xLoc, yLoc, charAtPoint);

            rootConsole.BackgroundColor = normalBackground;
            rootConsole.ForegroundColor = normalForeground;

        }


        /// <summary>
        /// Screen for end of game info
        /// </summary>
        public void DrawEndOfGameInfo(List<string> stuffToDisplay)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            //Draw frame
            rootConsole.DrawFrame(DeathTL.x, DeathTL.y, DeathWidth, DeathHeight, true);

            //Draw title
            rootConsole.PrintLineRect("End of game summary...", DeathTL.x + DeathWidth / 2, DeathTL.y, DeathWidth, 1, LineAlignment.Center);

            //Draw preamble
            int count = 0;
            foreach (string s in stuffToDisplay)
            {
                rootConsole.PrintLineRect(s, DeathTL.x + 2, DeathTL.y + 2 + count, DeathWidth - 4, 1, LineAlignment.Left);
                count++;
            }

            //Draw instructions

            rootConsole.PrintLineRect("Press ENTER to continue...", DeathTL.x + DeathWidth / 2, DeathTL.y + DeathHeight - 1, DeathWidth, 1, LineAlignment.Center);
            Screen.Instance.FlushConsole();

            WaitForEnterKey();
        }


        /// <summary>
        /// Screen for player death
        /// </summary>
        public void DrawDeathScreen()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            //Draw frame
            rootConsole.DrawFrame(DeathTL.x, DeathTL.y, DeathWidth, DeathHeight, true);

            //Draw title
            rootConsole.PrintLineRect("And it was all going so well...", DeathTL.x + DeathWidth / 2, DeathTL.y, DeathWidth, 1, LineAlignment.Center);

            //Draw preamble
            int count = 0;
            foreach (string s in DeathPreamble)
            {
                rootConsole.PrintLineRect(s, DeathTL.x + 2, DeathTL.y + 2 + count, DeathWidth - 4, 1, LineAlignment.Left);
                count++;
            }

            //Draw kills

            rootConsole.PrintLineRect("Total Kills", DeathTL.x + DeathWidth / 2, DeathTL.y + 2 + count + 2, DeathWidth, 1, LineAlignment.Center);

            foreach (string s in TotalKills)
            {
                rootConsole.PrintLineRect(s, DeathTL.x + 2, DeathTL.y + 2 + count + 4, DeathWidth - 4, 1, LineAlignment.Left);
                count++;
            }

            //Draw instructions

            rootConsole.PrintLineRect("Press any key to exit...", DeathTL.x + DeathWidth / 2, DeathTL.y + DeathHeight - 1, DeathWidth, 1, LineAlignment.Center);
        }

        /// <summary>
        /// Screen for player victory
        /// </summary>
        public void DrawVictoryScreen()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            //Draw frame
            rootConsole.DrawFrame(DeathTL.x, DeathTL.y, DeathWidth, DeathHeight, true);

            //Draw title
            rootConsole.PrintLineRect("VICTORY!", DeathTL.x + DeathWidth / 2, DeathTL.y, DeathWidth, 1, LineAlignment.Center);

            //Draw preamble
            int count = 0;
            foreach (string s in DeathPreamble)
            {
                rootConsole.PrintLineRect(s, DeathTL.x + 2, DeathTL.y + 2 + count, DeathWidth - 4, 1, LineAlignment.Left);
                count++;
            }

            //Draw kills

            rootConsole.PrintLineRect("Total Kills", DeathTL.x + DeathWidth / 2, DeathTL.y + 2 + count + 2, DeathWidth, 1, LineAlignment.Center);

            foreach (string s in TotalKills)
            {
                rootConsole.PrintLineRect(s, DeathTL.x + 2, DeathTL.y + 2 + count + 4, DeathWidth - 4, 1, LineAlignment.Left);
                count++;
            }

            //Draw instructions

            rootConsole.PrintLineRect("Press any key to exit...", DeathTL.x + DeathWidth / 2, DeathTL.y + DeathHeight - 1, DeathWidth, 1, LineAlignment.Center);
        }

        /// <summary>
        /// Display inventory overlay
        /// </summary>
        private void DrawInventory()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame
            rootConsole.DrawFrame(inventoryTL.x, inventoryTL.y, inventoryTR.x - inventoryTL.x + 1, inventoryBL.y - inventoryTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect(inventoryTitle, (inventoryTL.x + inventoryTR.x) / 2, inventoryTL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Draw instructions
            rootConsole.PrintLineRect(inventoryInstructions, (inventoryTL.x + inventoryTR.x) / 2, inventoryBL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //List the inventory
            
            //Inventory area is slightly reduced from frame
            int inventoryListX = inventoryTL.x + 2;
            int inventoryListW = inventoryTR.x - inventoryTL.x - 4;
            int inventoryListY = inventoryTL.y + 2;
            int inventoryListH = inventoryBL.y - inventoryTL.y - 4;

            List<InventoryListing> inventoryList = currentInventory.InventoryListing;

            for (int i = 0; i < inventoryListH; i++)
            {
                int inventoryIndex = topInventoryIndex + i;

                //End of inventory
                if (inventoryIndex == inventoryList.Count)
                    break;

                //Create entry string
                char selectionChar = (char)((int)'a' + i);
                string entryString = "(" + selectionChar.ToString() + ") " + inventoryList[inventoryIndex].Description;

                //Print entry
                rootConsole.PrintLineRect(entryString, inventoryListX, inventoryListY + i, inventoryListW, 1, LineAlignment.Left);
            }
        }

        /// <summary>
        /// Draw a calendar overlay
        /// </summary>
        private void DrawCalendar()
        {

            Point calendarTL = new Point(58, 6);
            Point calendarBR = new Point(81, 16);


            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame
            rootConsole.DrawFrame(calendarTL.x, calendarTL.y, calendarBR.x - calendarTL.x + 1, calendarBR.y - calendarTL.y + 1, true);

            //Draw title
            //rootConsole.PrintLineRect("Calendar", (calendarTL.x + calendarBR.x) / 2, calendarTL.y, calendarBR.x - calendarTL.x, 1, LineAlignment.Center);

            //Draw calendar

            int monthNo = Game.Dungeon.GetDateMonth();
            int dayNo = Game.Dungeon.GetDateDay();

            //Draw month name
            string [] monthNames = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
            rootConsole.PrintLineRect(monthNames[monthNo - 1], (calendarTL.x + calendarBR.x) / 2, calendarTL.y, calendarBR.x - calendarTL.x, 1, LineAlignment.Center);

            Point calendarOffset = new Point(2, 2);

            Color selectedDayColor = ColorPresets.Red;
            Color normalDayColor = ColorPresets.White;

            //Draw days
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 7; i++)
                {
                    int thisDay = (j * 7 + i) + 1;

                    if (thisDay == dayNo)
                    {
                        rootConsole.ForegroundColor = selectedDayColor;
                    }

                    rootConsole.PrintLine(thisDay.ToString(), (calendarTL.x + calendarOffset.x + i * 3), calendarTL.y + calendarOffset.y + 2 * j, LineAlignment.Left);
                    rootConsole.ForegroundColor = normalDayColor;
                }
            }
        }

        /// <summary>
        /// Draw a stats box overlay
        /// </summary>
        private void DrawStatsBox()
        {

            Point statsBoxTL = new Point(58, 18);
            Point statsBoxBR = new Point(81, 27);

            Point statTitleOffset = new Point(3, 2);
            Point statDataOffset = new Point(17, 2);

            Color statNumberColor = ColorPresets.CadetBlue;

            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame
            rootConsole.DrawFrame(statsBoxTL.x, statsBoxTL.y, statsBoxBR.x - statsBoxTL.x + 1, statsBoxBR.y - statsBoxTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect("Statistics", (statsBoxTL.x + statsBoxBR.x) / 2, statsBoxTL.y, statsBoxBR.x - statsBoxTL.x, 1, LineAlignment.Center);

            //Draw PrincessRL training stats

            Player player = Game.Dungeon.Player;

            string trainHitpointsString = player.HitpointsStat.ToString();
            string trainMaxHitpointsString = player.MaxHitpointsStat.ToString();
            string trainAttackString = player.AttackStat.ToString();
            string trainSpeedString = player.SpeedStat.ToString();
            string trainCharmString = player.CharmStat.ToString();
            string trainMagicString = player.MagicStat.ToString();

            rootConsole.PrintLine("Stamina:", statsBoxTL.x + statTitleOffset.x, statsBoxTL.y + statTitleOffset.y + 0, LineAlignment.Left);
            rootConsole.PrintLine("Health:", statsBoxTL.x + statTitleOffset.x, statsBoxTL.y + statTitleOffset.y + 1, LineAlignment.Left);
            rootConsole.PrintLine("Combat Skill:", statsBoxTL.x + statTitleOffset.x, statsBoxTL.y + statTitleOffset.y + 2, LineAlignment.Left);
            rootConsole.PrintLine("Speed:", statsBoxTL.x + statTitleOffset.x, statsBoxTL.y + statTitleOffset.y + 3, LineAlignment.Left);
            rootConsole.PrintLine("Charm:", statsBoxTL.x + statTitleOffset.x, statsBoxTL.y + statTitleOffset.y + 4, LineAlignment.Left);
            rootConsole.PrintLine("Magic Skill:", statsBoxTL.x + statTitleOffset.x, statsBoxTL.y + statTitleOffset.y + 5, LineAlignment.Left);

            rootConsole.ForegroundColor = statNumberColor;

            rootConsole.PrintLine(trainHitpointsString, statsBoxTL.x + statDataOffset.x, statsBoxTL.y + statDataOffset.y + 0, LineAlignment.Left);
            rootConsole.PrintLine(trainMaxHitpointsString, statsBoxTL.x + statDataOffset.x, statsBoxTL.y + statDataOffset.y + 1, LineAlignment.Left);
            rootConsole.PrintLine(trainAttackString, statsBoxTL.x + statDataOffset.x, statsBoxTL.y + statDataOffset.y + 2, LineAlignment.Left);
            rootConsole.PrintLine(trainSpeedString, statsBoxTL.x + statDataOffset.x, statsBoxTL.y + statDataOffset.y + 3, LineAlignment.Left);
            rootConsole.PrintLine(trainCharmString, statsBoxTL.x + statDataOffset.x, statsBoxTL.y + statDataOffset.y + 4, LineAlignment.Left);
            rootConsole.PrintLine(trainMagicString, statsBoxTL.x + statDataOffset.x, statsBoxTL.y + statDataOffset.y + 5, LineAlignment.Left);

            rootConsole.ForegroundColor = ColorPresets.White;
        }

        /// <summary>
        /// Display equipment select overview
        /// </summary>
        private void DrawEquipmentSelect()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame
            rootConsole.DrawFrame(inventoryTL.x, inventoryTL.y, inventoryTR.x - inventoryTL.x + 1, inventoryBL.y - inventoryTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect(inventoryTitle, (inventoryTL.x + inventoryTR.x) / 2, inventoryTL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Draw instructions
            rootConsole.PrintLineRect(inventoryInstructions, (inventoryTL.x + inventoryTR.x) / 2, inventoryBL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //List the inventory

            //Inventory area is slightly reduced from frame
            int inventoryListX = inventoryTL.x + 2;
            int inventoryListW = inventoryTR.x - inventoryTL.x - 4;
            int inventoryListY = inventoryTL.y + 2;
            int inventoryListH = inventoryBL.y - inventoryTL.y - 4;

            List<InventoryListing> inventoryList = currentInventory.InventoryListing;

            for (int i = 0; i < inventoryListH; i++)
            {
                int inventoryIndex = topInventoryIndex + i;

                //End of inventory
                if (inventoryIndex == inventoryList.Count)
                    break;

                //Create entry string
                char selectionChar = (char)((int)'a' + i);
                string entryString = "(" + selectionChar.ToString() + ") " + inventoryList[inventoryIndex].Description;

                //Add equipped status
                //Only consider the first item in a stack, since equipped items can't stack
                Item firstItemInStack = currentInventory.Items[inventoryList[inventoryIndex].ItemIndex[0]];

                EquipmentSlotInfo equippedInSlot = currentEquipment.Find(x => x.equippedItem == firstItemInStack);

                if (equippedInSlot != null)
                {
                    entryString += " (equipped: " + StringEquivalent.EquipmentSlots[equippedInSlot.slotType] + ")";
                }

                //Print entry
                rootConsole.PrintLineRect(entryString, inventoryListX, inventoryListY + i, inventoryListW, 1, LineAlignment.Left);
            }
        }

        /// <summary>
        /// Display equipment select overview
        /// </summary>
        private void DrawEquipment()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame
            rootConsole.DrawFrame(inventoryTL.x, inventoryTL.y, inventoryTR.x - inventoryTL.x + 1, inventoryBL.y - inventoryTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect(inventoryTitle, (inventoryTL.x + inventoryTR.x) / 2, inventoryTL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Draw instructions
            rootConsole.PrintLineRect(inventoryInstructions, (inventoryTL.x + inventoryTR.x) / 2, inventoryBL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //List the inventory

            //Inventory area is slightly reduced from frame
            int inventoryListX = inventoryTL.x + 2;
            int inventoryListW = inventoryTR.x - inventoryTL.x - 4;
            int inventoryListY = inventoryTL.y + 2;
            int inventoryListH = inventoryBL.y - inventoryTL.y - 4;

            List<InventoryListing> inventoryList = currentInventory.EquipmentListing;

            for (int i = 0; i < inventoryListH; i++)
            {
                int inventoryIndex = topInventoryIndex + i;

                //End of inventory
                if (inventoryIndex == inventoryList.Count)
                    break;


                //Add equipped status
                //Only consider the first item in a stack, since equipped items can't stack
                Item firstItemInStack = currentInventory.Items[inventoryList[inventoryIndex].ItemIndex[0]];


                //Create entry string
                char selectionChar = (char)((int)'a' + i);
                string entryString = "(" + selectionChar.ToString() + ") " + firstItemInStack.SingleItemDescription; //+" (equipped)";

                //EquipmentSlotInfo equippedInSlot = currentEquipment.Find(x => x.equippedItem == firstItemInStack);

                //if (equippedInSlot != null)
                //{
                 //   entryString += " (equipped: " + StringEquivalent.EquipmentSlots[equippedInSlot.slotType] + ")";
                //}

                //Print entry
                rootConsole.PrintLineRect(entryString, inventoryListX, inventoryListY + i, inventoryListW, 1, LineAlignment.Left);
            }
        }

        /// <summary>
        /// Display movie screen overlay
        /// </summary>
        private void DrawMovieOverlay()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame - same as inventory
            rootConsole.DrawFrame(inventoryTL.x, inventoryTL.y, inventoryTR.x - inventoryTL.x + 1, inventoryBL.y - inventoryTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect("Special moves and spells known", (inventoryTL.x + inventoryTR.x) / 2, inventoryTL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Draw instructions
            rootConsole.PrintLineRect("Select move to replay movie or (x) to exit", (inventoryTL.x + inventoryTR.x) / 2, inventoryBL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //List the special moves known

            //Active area is slightly reduced from frame
            int inventoryListX = inventoryTL.x + 2;
            int inventoryListW = inventoryTR.x - inventoryTL.x - 4;
            int inventoryListY = inventoryTL.y + 2;
            int inventoryListH = inventoryBL.y - inventoryTL.y - 4;

            int moveIndex = 0;
            List<SpecialMove> knownMoves = new List<SpecialMove>();

            foreach (SpecialMove move in Game.Dungeon.SpecialMoves) {

                //Run out of room - won't happen as written
                if (moveIndex == inventoryListH)
                    break;

                //Don't list unknown moves
                if (!move.Known)
                    continue;

                knownMoves.Add(move);

                char selectionChar = (char)((int)'a' + moveIndex);
                string entryString = "(" + selectionChar.ToString() + ") " + move.MoveName(); //+" (equipped)";

                //Print entry
                rootConsole.PrintLineRect(entryString, inventoryListX, inventoryListY + moveIndex, inventoryListW, 1, LineAlignment.Left);

                moveIndex++;
            }

            List<Spell> knownSpells = new List<Spell>();

            foreach (Spell move in Game.Dungeon.Spells)
            {

                //Run out of room - won't happen as written
                if (moveIndex == inventoryListH)
                    break;

                //Don't list unknown moves
                if (!move.Known)
                    continue;

                knownSpells.Add(move);

                char selectionChar = (char)((int)'a' + moveIndex);
                string entryString = "(" + selectionChar.ToString() + ") " + move.SpellName(); //+" (equipped)";

                //Print entry
                rootConsole.PrintLineRect(entryString, inventoryListX, inventoryListY + moveIndex, inventoryListW, 1, LineAlignment.Left);

                moveIndex++;
            }
        }

        /// <summary>
        /// Display spell screen overlay
        /// </summary>
        private void DrawSpellOverlay()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame - same as inventory
            rootConsole.DrawFrame(inventoryTL.x, inventoryTL.y, inventoryTR.x - inventoryTL.x + 1, inventoryBL.y - inventoryTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect("Spells known", (inventoryTL.x + inventoryTR.x) / 2, inventoryTL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Draw instructions
            rootConsole.PrintLineRect("Select a spell to cast or (x) to exit", (inventoryTL.x + inventoryTR.x) / 2, inventoryBL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //List the special moves known

            //Active area is slightly reduced from frame
            int inventoryListX = inventoryTL.x + 2;
            int inventoryListW = inventoryTR.x - inventoryTL.x - 4;
            int inventoryListY = inventoryTL.y + 2;
            int inventoryListH = inventoryBL.y - inventoryTL.y - 4;

            int spellIndex = 0;
            List<Spell> knownSpells = new List<Spell>();

            foreach (Spell spell in Game.Dungeon.Spells)
            {

                //Run out of room - won't happen as written
                if (spellIndex == inventoryListH)
                    break;

                //Don't list unknown moves
                if (!spell.Known)
                    continue;

                knownSpells.Add(spell);

                char selectionChar = (char)((int)'a' + spellIndex);
                string entryString = "(" + selectionChar.ToString() + ") " + spell.SpellName() + " MP: " + spell.MPCost().ToString(); //+" (equipped)";

                //Print entry
                rootConsole.PrintLineRect(entryString, inventoryListX, inventoryListY + spellIndex, inventoryListW, 1, LineAlignment.Left);

                spellIndex++;
            }
        }

        public string TrainingTypeString { get; set; }

        bool trainingPause = true;

        public bool TrainingPause
        {
            get
            {
                return trainingPause;
            }
            set
            {
                trainingPause = value;
            }
        }

        List<TrainStats> trainingStatsRecord;

        public void ClearTrainingStatsRecord()
        {
            trainingStatsRecord.Clear();
        }

        public void AddTrainingStatsRecord(TrainStats newStats)
        {
            trainingStatsRecord.Add(newStats);
        }

        int trainingXTemp;
        int trainingYTemp;

        /// <summary>
        /// Display training overlay. Just put up the border and write some text. Calls from the caller will add info.
        /// </summary>
        private void DrawTrainingOverlay()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            Point statsHeaderOffset = new Point(10, 0);
            Point statsModOffset = new Point(15, 0);
            Point statsDayOffset = new Point(2, 0);

            Point fitnessOffset = new Point(0, 0);
            Point healthOffset = new Point(8, 0);
            Point speedOffset = new Point(16, 0);
            Point combatOffset = new Point(23, 0);
            Point charmOffset = new Point(30, 0);
            Point magicOffset = new Point(36, 0);

            //Draw frame - same as inventory
            rootConsole.DrawFrame(trainingTL.x, trainingTL.y, trainingTR.x - trainingTL.x + 1, trainingBL.y - trainingTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect("Training!", (trainingTL.x + trainingTR.x) / 2, trainingTL.y, trainingTR.x - trainingTL.x, 1, LineAlignment.Center);

            //Draw instructions
            rootConsole.PrintLineRect("Press (x) to exit", (trainingTL.x + trainingTR.x) / 2, trainingBL.y, trainingTR.x - trainingTL.x, 1, LineAlignment.Center);

            //Draw headings
            rootConsole.PrintLineRect(TrainingTypeString, (trainingTL.x + trainingTR.x) / 2, trainingTL.y + 2, trainingTR.x - trainingTL.x, 1, LineAlignment.Center);

            //Draw stats
            /*
            int headerY = trainingTL.y + 4;

            rootConsole.PrintLine("Stamina", trainingTL.x + statsHeaderOffset.x + fitnessOffset.x, headerY, LineAlignment.Left);
            rootConsole.PrintLine("Health", trainingTL.x + statsHeaderOffset.x + healthOffset.x, headerY, LineAlignment.Left);
            rootConsole.PrintLine("Speed", trainingTL.x + statsHeaderOffset.x + speedOffset.x, headerY, LineAlignment.Left);
            rootConsole.PrintLine("Combat", trainingTL.x + statsHeaderOffset.x + combatOffset.x, headerY, LineAlignment.Left);
            rootConsole.PrintLine("Charm", trainingTL.x + statsHeaderOffset.x + charmOffset.x, headerY, LineAlignment.Left);
            rootConsole.PrintLine("Magic", trainingTL.x + statsHeaderOffset.x + magicOffset.x, headerY, LineAlignment.Left);
            */
            //Work out the start day
            List<string> dayNames = new List<string>();
            
            if(Game.Dungeon.IsWeekday()) {
                dayNames.Add("Monday");
                dayNames.Add("Tuesday");
                dayNames.Add("Wednesday");
                dayNames.Add("Thursday");
                dayNames.Add("Friday");
            }
            else {
                dayNames.Add("Saturday");
                dayNames.Add("Sunday");
            }

            //Draw all updates
            int lineCount = 0;

            foreach (TrainStats stats in trainingStatsRecord)
            {
                //Pause
                if (trainingPause)
                    TCODSystem.Sleep(400);

                FlushConsole();

                string dayName;
                if (lineCount < dayNames.Count)
                    dayName = dayNames[lineCount];
                else
                {
                    dayName = "";
                    LogFile.Log.LogEntryDebug("Error - couldn't find right day name in training", LogDebugLevel.High);
                }



                trainingYTemp = trainingTL.y + 6 + lineCount;

                //Concatenate display string
                trainingXTemp = statsModOffset.x;

                rootConsole.ForegroundColor = ColorPresets.White;
                rootConsole.PrintLine(dayName + " : ", trainingTL.x + statsDayOffset.x, trainingYTemp, LineAlignment.Left);

                ProcessDelta("Stamina", stats.HitpointsStatDelta);
                ProcessDelta("Health", stats.MaxHitpointsStatDelta);
                ProcessDelta("Combat", stats.AttackStatDelta);
                ProcessDelta("Speed", stats.SpeedStatDelta);
                ProcessDelta("Charm", stats.CharmStatDelta);
                ProcessDelta("Magic", stats.MagicStatDelta);

                //No change
                if (trainingXTemp == statsModOffset.x)
                {
                    rootConsole.ForegroundColor = ColorPresets.White;
                    rootConsole.PrintLine("No change!", trainingTL.x + trainingXTemp, trainingYTemp, LineAlignment.Left);
                }

                lineCount++;
                
            }


            rootConsole.ForegroundColor = ColorPresets.White;
           
        }


        public int MagicInc { get; set; }
        public int CombatInc { get; set; }
        public int CharmInc { get; set; }

        public bool ShowXPScreen { get; set; }

        /// <summary>
        /// Display XP boost overlay
        /// </summary>
        private void DrawXPOverlay()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            Point statsHeaderOffset = new Point(10, 0);
            Point statsModOffset = new Point(15, 0);
            Point statsDayOffset = new Point(2, 0);

            Point fitnessOffset = new Point(0, 0);
            Point healthOffset = new Point(8, 0);
            Point speedOffset = new Point(16, 0);
            Point combatOffset = new Point(23, 0);
            Point charmOffset = new Point(30, 0);
            Point magicOffset = new Point(36, 0);

            //Draw frame - same as inventory
            rootConsole.DrawFrame(trainingTL.x, trainingTL.y, trainingTR.x - trainingTL.x + 1, trainingBL.y - trainingTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect("Your adventuring paid off!", (trainingTL.x + trainingTR.x) / 2, trainingTL.y, trainingTR.x - trainingTL.x, 1, LineAlignment.Center);

            //Draw instructions
            rootConsole.PrintLineRect("Press (ENTER) to exit", (trainingTL.x + trainingTR.x) / 2, trainingBL.y, trainingTR.x - trainingTL.x, 1, LineAlignment.Center);

            //Draw headings
            //rootConsole.PrintLineRect(TrainingTypeString, (trainingTL.x + trainingTR.x) / 2, trainingTL.y + 2, trainingTR.x - trainingTL.x, 1, LineAlignment.Center);

            //Draw stats
            /*
            int headerY = trainingTL.y + 4;

            rootConsole.PrintLine("Stamina", trainingTL.x + statsHeaderOffset.x + fitnessOffset.x, headerY, LineAlignment.Left);
            rootConsole.PrintLine("Health", trainingTL.x + statsHeaderOffset.x + healthOffset.x, headerY, LineAlignment.Left);
            rootConsole.PrintLine("Speed", trainingTL.x + statsHeaderOffset.x + speedOffset.x, headerY, LineAlignment.Left);
            rootConsole.PrintLine("Combat", trainingTL.x + statsHeaderOffset.x + combatOffset.x, headerY, LineAlignment.Left);
            rootConsole.PrintLine("Charm", trainingTL.x + statsHeaderOffset.x + charmOffset.x, headerY, LineAlignment.Left);
            rootConsole.PrintLine("Magic", trainingTL.x + statsHeaderOffset.x + magicOffset.x, headerY, LineAlignment.Left);
            */



            //Draw all updates
            int lineCount = 0;

            trainingYTemp = trainingTL.y + 6 + lineCount;

            trainingYTemp = trainingTL.y + 4 + lineCount;
            trainingXTemp = statsModOffset.x;

            rootConsole.ForegroundColor = ColorPresets.White;

            if (CombatInc > 0 || CharmInc > 0 || MagicInc > 0)
            {
                rootConsole.PrintLine("These stats increased:", trainingTL.x + trainingXTemp, trainingYTemp, LineAlignment.Left);
            }
            else
            {
                rootConsole.PrintLine("No stats increased this adventure!", trainingTL.x + trainingXTemp, trainingYTemp, LineAlignment.Left);
            }

            trainingYTemp = trainingTL.y + 6 + lineCount;

            //Concatenate display string
            trainingXTemp = statsModOffset.x;

            ProcessDelta("Combat", CombatInc);
            trainingXTemp = statsModOffset.x;
            trainingYTemp++;
            ProcessDelta("Charm", CharmInc);
            trainingXTemp = statsModOffset.x;
            trainingYTemp++;
            ProcessDelta("Magic", MagicInc);

            FlushConsole();
            WaitForEnterKey();

            rootConsole.ForegroundColor = ColorPresets.White;

        }

        public bool ShowMsgHistory { get; set; }

        enum Direction { up, down, none };

        /// <summary>
        /// Draw the msg history and allow the player to scroll
        /// </summary>
        private void DrawMsgHistory()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame - same as inventory
            rootConsole.DrawFrame(inventoryTL.x, inventoryTL.y, inventoryTR.x - inventoryTL.x + 1, inventoryBL.y - inventoryTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect("Message History", (inventoryTL.x + inventoryTR.x) / 2, inventoryTL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Draw instructions
            rootConsole.PrintLineRect("Press (up) or (down) to scroll or (x) to exit", (inventoryTL.x + inventoryTR.x) / 2, inventoryBL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Active area is slightly reduced from frame
            int inventoryListX = inventoryTL.x + 2;
            int inventoryListW = inventoryTR.x - inventoryTL.x - 4;
            int inventoryListY = inventoryTL.y + 2;
            int inventoryListH = inventoryBL.y - inventoryTL.y - 4;

            LinkedList<string> msgHistory = Game.MessageQueue.messageHistory;

            //Display list
            LinkedListNode<string> displayedMsg;
            LinkedListNode<string> topLineDisplayed = null;
            
            LinkedListNode<string> bottomTopLineDisplayed = msgHistory.Last;

            if (msgHistory.Count > 0)
            {
                //Find the line at the top of the screen when the list is fully scrolled down
                for (int i = 0; i < inventoryListH - 1; i++)
                {
                    if (bottomTopLineDisplayed.Previous != null)
                        bottomTopLineDisplayed = bottomTopLineDisplayed.Previous;
                }
                topLineDisplayed = bottomTopLineDisplayed;

                //Display the message log
                displayedMsg = topLineDisplayed;
                for (int i = 0; i < inventoryListH; i++)
                {
                    rootConsole.PrintLineRect(displayedMsg.Value, inventoryListX, inventoryListY + i, inventoryListW, 1, LineAlignment.Left);
                    displayedMsg = displayedMsg.Next;
                    if (displayedMsg == null)
                        break;
                }
            }

            Screen.Instance.FlushConsole();

            bool keepLooping = true;

            do
            {
                //Get user input
                KeyPress userKey = Keyboard.WaitForKeyPress(true);
                Direction dir = Direction.none;

                //Each state has different keys

                if (userKey.KeyCode == KeyCode.TCODK_CHAR)
                {
                    char keyCode = (char)userKey.Character;

                    if (keyCode == 'x')
                        keepLooping = false;

                    if (keyCode == 'j')
                    {
                        dir = Direction.up;
                    }

                    if (keyCode == 'k')
                    {
                        dir = Direction.down;
                    }
                }

                else
                {
                    //Special keys
                    switch (userKey.KeyCode)
                    {
                        case KeyCode.TCODK_UP:
                        case KeyCode.TCODK_KP8:
                            dir = Direction.up;
                            break;

                        case KeyCode.TCODK_KP2:
                        case KeyCode.TCODK_DOWN:
                            dir = Direction.down;
                            break;
                    }
                }

                if (msgHistory.Count > 0)
                {
                    if (dir == Direction.up)
                    {
                        if (topLineDisplayed.Previous != null)
                            topLineDisplayed = topLineDisplayed.Previous;
                    }
                    else if (dir == Direction.down)
                    {
                        if (topLineDisplayed != bottomTopLineDisplayed)
                            topLineDisplayed = topLineDisplayed.Next;
                    }

                    //Clear the rectangle
                    rootConsole.DrawRect(inventoryTL.x + 1, inventoryTL.y + 1, inventoryTR.x - inventoryTL.x - 1, inventoryBL.y - inventoryTL.y - 1, true);

                    //Display the message log
                    displayedMsg = topLineDisplayed;
                    for (int i = 0; i < inventoryListH; i++)
                    {
                        rootConsole.PrintLineRect(displayedMsg.Value, inventoryListX, inventoryListY + i, inventoryListW, 1, LineAlignment.Left);
                        displayedMsg = displayedMsg.Next;
                        if (displayedMsg == null)
                            break;
                    }
                }
                Screen.Instance.FlushConsole();

            } while (keepLooping);
        }

        private void ProcessDelta(string statName, int delta)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            string outString;
            Color colorToPrint;

            if (delta == 0)
                return;

            if (delta > 0)
            {
                outString = statName+ "(+" + delta.ToString() + ")";
                colorToPrint = ColorPresets.Green;
            }
            else
            {
                outString = statName + "(" + delta.ToString() + ")";
                colorToPrint = ColorPresets.Red;
            }
            rootConsole.ForegroundColor = colorToPrint;
            rootConsole.PrintLine(outString, trainingTL.x + trainingXTemp, trainingYTemp, LineAlignment.Left);

            trainingXTemp += outString.Length + 1;
        }
        /*
        /// <summary>
        /// Display training overlay. Just put up the border and write some text. Calls from the caller will add info.
        /// </summary>
        private void DrawTrainingOverlay()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame - same as inventory
            rootConsole.DrawFrame(trainingTL.x, trainingTL.y, trainingTR.x - trainingTL.x + 1, trainingBL.y - trainingTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect("Training!", (trainingTL.x + trainingTR.x) / 2, trainingTL.y, trainingTR.x - trainingTL.x, 1, LineAlignment.Center);

            //Draw headings
            rootConsole.PrintLineRect(TrainingTypeString, (trainingTL.x + trainingTR.x) / 2, trainingTL.y + 2, trainingTR.x - trainingTL.x, 1, LineAlignment.Center);

            //Draw stats
            string statsRow = "Fitness  Health  Speed  Combat  Charm  Magic";

            rootConsole.PrintLineRect(TrainingTypeString, (trainingTL.x + trainingTR.x) / 2, trainingTL.y + 4, trainingTR.x - trainingTL.x, 1, LineAlignment.Center);
        }*/


        /// <summary>
        /// Display equipment overlay
        /// </summary>
        private void DrawEquipmentOld()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Use frame and strings from inventory for now

            //Draw frame
            rootConsole.DrawFrame(inventoryTL.x, inventoryTL.y, inventoryTR.x - inventoryTL.x + 1, inventoryBL.y - inventoryTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect(inventoryTitle, (inventoryTL.x + inventoryTR.x) / 2, inventoryTL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Draw instructions
            rootConsole.PrintLineRect(inventoryInstructions, (inventoryTL.x + inventoryTR.x) / 2, inventoryBL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //List current slots & items if filled

            //Equipment area is slightly reduced from frame
            int inventoryListX = inventoryTL.x + 2;
            int inventoryListW = inventoryTR.x - inventoryTL.x - 4;
            int inventoryListY = inventoryTL.y + 2;
            int inventoryListH = inventoryBL.y - inventoryTL.y - 4;

            for (int i = 0; i < inventoryListH; i++)
            {
                int inventoryIndex = topInventoryIndex + i;

                //End of inventory
                if (inventoryIndex == currentEquipment.Count)
                    break;

                //Create entry string
                EquipmentSlotInfo currentSlot = currentEquipment[inventoryIndex];

                char selectionChar = (char)((int)'a' + i);
                string entryString = "(" + selectionChar.ToString() + ") " + StringEquivalent.EquipmentSlots[currentSlot.slotType] + ": ";
                if (currentSlot.equippedItem == null)
                    entryString += "Empty";
                else
                    entryString += currentSlot.equippedItem.SingleItemDescription;

                //Print entry
                rootConsole.PrintLineRect(entryString, inventoryListX, inventoryListY + i, inventoryListW, 1, LineAlignment.Left);
            }
        }

        private void DrawStats(Player player)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Are we in town or the wilderness? Don't show stats
            //if (player.LocationLevel < 2)
            //    return;

            statsDisplayTopLeft = new Point(66, 1);

            //Blank stats area
            rootConsole.DrawRect(statsDisplayTopLeft.x, statsDisplayTopLeft.y, Width - statsDisplayTopLeft.x, Height - statsDisplayTopLeft.y, true);

            //Mission
            Point missionOffset = new Point(0, 3);
            rootConsole.PrintLine("Location: " + player.LocationLevel.ToString(), statsDisplayTopLeft.x + missionOffset.x, statsDisplayTopLeft.y + missionOffset.y, LineAlignment.Left);
            rootConsole.PrintLine(DungeonInfo.LookupMissionName(player.LocationLevel), statsDisplayTopLeft.x + missionOffset.x, statsDisplayTopLeft.y + missionOffset.y + 1, LineAlignment.Left);
                
            hitpointsOffset = new Point(0, 5);
            Point weaponOffset = new Point(0, 8);
            Point utilityOffset = new Point(0, 13);
            Point viewOffset = new Point(0, 18);

            Point gameDataOffset = new Point(0, 23);
            //Draw HP Status

            int hpBarLength = 10;
            double playerHPRatio = player.Hitpoints / (double)player.MaxHitpoints;
            int hpBarEntries = (int)Math.Ceiling(hpBarLength * playerHPRatio);
            
            //It's easy for the player - make sure we're exact
            hpBarEntries = player.Hitpoints;

            rootConsole.ForegroundColor = statsColor;
            rootConsole.PrintLine("HP: ", statsDisplayTopLeft.x + hitpointsOffset.x, statsDisplayTopLeft.y + hitpointsOffset.y, LineAlignment.Left);

            for (int i = 0; i < hpBarLength; i++)
            {
                if (i < hpBarEntries)
                {
                    rootConsole.ForegroundColor = ColorPresets.Green;
                    rootConsole.PutChar(statsDisplayTopLeft.x + hitpointsOffset.x + 5 + i, statsDisplayTopLeft.y + hitpointsOffset.y, '*');
                }
                else
                {
                    rootConsole.ForegroundColor = ColorPresets.Gray;
                    rootConsole.PutChar(statsDisplayTopLeft.x + hitpointsOffset.x + 5 + i, statsDisplayTopLeft.y + hitpointsOffset.y, '*');
                }
            }

            rootConsole.ForegroundColor = statsColor;

            //Draw equipped weapon

            Item weapon = Game.Dungeon.Player.GetEquippedWeaponAsItem();

            string weaponStr = "Weapon: ";

            rootConsole.PrintLine(weaponStr, statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y, LineAlignment.Left);

            if (weapon != null)
            {
                IEquippableItem weaponE = weapon as IEquippableItem;
                
                weaponStr = weapon.SingleItemDescription;
                rootConsole.PrintLine(weaponStr, statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y + 1, LineAlignment.Left);

                //Ammo
                if (weaponE.HasFireAction())
                {
                    rootConsole.PrintLine("Am: ", statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y + 2, LineAlignment.Left);
        
                    //TODO infinite ammo?
                    int ammoBarLength = 10;
                    double weaponAmmoRatio = weaponE.RemainingAmmo() / (double) weaponE.MaxAmmo();
                    int ammoBarEntries = (int)Math.Ceiling(ammoBarLength * weaponAmmoRatio);

                    for (int i = 0; i < ammoBarLength; i++)
                    {
                        if (i < ammoBarEntries)
                        {
                            rootConsole.ForegroundColor = ColorPresets.Blue;
                            rootConsole.PutChar(statsDisplayTopLeft.x + weaponOffset.x + 5 + i, statsDisplayTopLeft.y + weaponOffset.y + 2, '*');
                        }
                        else
                        {
                            rootConsole.ForegroundColor = ColorPresets.Gray;
                            rootConsole.PutChar(statsDisplayTopLeft.x + weaponOffset.x + 5 + i, statsDisplayTopLeft.y + weaponOffset.y + 2, '*');
                        }
                    }
                }

                rootConsole.ForegroundColor = statsColor;
             
                //Uses

                string uses = "";
                if (weaponE.HasMeleeAction())
                {
                    uses += "melee ";
                }

                if (weaponE.HasFireAction())
                {
                    uses += "(f)ire ";
                }

                if (weaponE.HasThrowAction())
                {
                    uses += "(t)hrow ";
                }

                if (weaponE.HasOperateAction())
                {
                    uses += "(u)se";
                }

                rootConsole.PrintLine(uses, statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y + 3, LineAlignment.Left);
            }
            else
            {
                weaponStr = "Nothing";
                rootConsole.PrintLine(weaponStr, statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y + 1, LineAlignment.Left);
            }
            

            //Draw equipped utility

            Item utility = Game.Dungeon.Player.GetEquippedUtilityAsItem();

            string utilityStr = "Utility: ";
            rootConsole.PrintLine(utilityStr, statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y, LineAlignment.Left);

            if (utility != null)
            {
                utilityStr = utility.SingleItemDescription;
         
                rootConsole.PrintLine(utilityStr, statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y + 1, LineAlignment.Left);

                IEquippableItem utilityE = utility as IEquippableItem;

                string uses = "";
                
                if (utilityE.HasOperateAction())
                {
                    uses += "(U)se";
                }

                if (utilityE.HasThrowAction())
                {
                    uses += "(T)hrow ";
                }

                rootConsole.PrintLine(uses, statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y + 2, LineAlignment.Left);
            }
             
            else
            {
                utilityStr = "Nothing";
                rootConsole.PrintLine(utilityStr, statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y + 1, LineAlignment.Left);
            }

            //Draw what we can see
            
            //Creature takes precidence

            string viewStr = "Target: ";
            rootConsole.PrintLine(viewStr, statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y, LineAlignment.Left);

            if (CreatureToView != null && CreatureToView.Alive == true)
            {
                String nameStr = CreatureToView.SingleDescription;// +"(" + CreatureToView.Representation + ")";
                rootConsole.ForegroundColor = CreatureToView.RepresentationColor();
                rootConsole.PrintLine(nameStr, statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 1, LineAlignment.Left);
                rootConsole.ForegroundColor = statsColor;

                //Monster hp

                int mhpBarLength = 10;
                double mplayerHPRatio = CreatureToView.Hitpoints / (double)CreatureToView.MaxHitpoints;
                int mhpBarEntries = (int)Math.Ceiling(mhpBarLength * mplayerHPRatio);

                rootConsole.ForegroundColor = statsColor;
                rootConsole.PrintLine("HP: ", statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 2, LineAlignment.Left);

                for (int i = 0; i < mhpBarLength; i++)
                {
                    if (i < mhpBarEntries)
                    {
                        rootConsole.ForegroundColor = ColorPresets.Red;
                        rootConsole.PutChar(statsDisplayTopLeft.x + viewOffset.x + 5 + i, statsDisplayTopLeft.y + viewOffset.y + 2, '*');
                    }
                    else
                    {
                        rootConsole.ForegroundColor = ColorPresets.Gray;
                        rootConsole.PutChar(statsDisplayTopLeft.x + viewOffset.x + 5 + i, statsDisplayTopLeft.y + viewOffset.y + 2, '*');
                    }
                }


                //Behaviour
                rootConsole.ForegroundColor = statsColor;

                if (CreatureToView.InPursuit())
                {
                    rootConsole.ForegroundColor = pursuitBackground;
                    rootConsole.PrintLine("(In Pursuit)", statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 3, LineAlignment.Left);
                }
                else if (!CreatureToView.OnPatrol())
                {
                    rootConsole.ForegroundColor = investigateBackground;
                    rootConsole.PrintLine("(Investigating)", statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 3, LineAlignment.Left);
                }
                else
                {
                    rootConsole.ForegroundColor = statsColor;
                    rootConsole.PrintLine("(Neutral)", statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 3, LineAlignment.Left);
                }
            }
            else if (ItemToView != null)
            {
                String nameStr = ItemToView.SingleItemDescription;// +"(" + ItemToView.Representation + ")";
                rootConsole.ForegroundColor = ItemToView.GetColour();
                rootConsole.PrintLine(nameStr, statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 1, LineAlignment.Left);
                rootConsole.ForegroundColor = statsColor;

                IEquippableItem itemE = ItemToView as IEquippableItem;
                if (itemE != null)
                {
                    EquipmentSlot weaponSlot = itemE.EquipmentSlots.Find(x => x == EquipmentSlot.Weapon);
                    if(weaponSlot != null) {
                        rootConsole.PrintLine("(Weapon)", statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 2, LineAlignment.Left);
                    }
                    else
                        rootConsole.PrintLine("(Utility)", statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 2, LineAlignment.Left);
                }
            }

            //Game data
            rootConsole.ForegroundColor = statsColor;
            rootConsole.PrintLine("Deaths: " + Game.Dungeon.DungeonInfo.NoDeaths, statsDisplayTopLeft.x + gameDataOffset.x, statsDisplayTopLeft.y + gameDataOffset.y, LineAlignment.Left);
            rootConsole.PrintLine("Aborts: " + Game.Dungeon.DungeonInfo.NoAborts, statsDisplayTopLeft.x + gameDataOffset.x, statsDisplayTopLeft.y + gameDataOffset.y + 1, LineAlignment.Left);

            //Restore to normal colour - not nice
            rootConsole.ForegroundColor = ColorPresets.White;
        }

        private void DrawItems(List<Item> itemList)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Set default colour
            rootConsole.ForegroundColor = itemColor;

            //Could consider storing here and sorting to give an accurate representation of multiple objects

            foreach (Item item in itemList)
            {
                //Don't draw items on creatures
                if (item.InInventory)
                    continue;

                //Don't draw items on other levels
                if (item.LocationLevel != Game.Dungeon.Player.LocationLevel)
                    continue;

                //Colour depending on FOV (for development)
                MapSquare itemSquare = Game.Dungeon.Levels[item.LocationLevel].mapSquares[item.LocationMap.x, item.LocationMap.y];

                //Use the item's colour if it has one
                Color itemColorToUse = item.GetColour();

                IEquippableItem equipItem = item as IEquippableItem;
                if (equipItem != null)
                {
                    //Show no ammo items in a neutral colour
                    if (equipItem.HasFireAction() && equipItem.RemainingAmmo() == 0)
                        itemColorToUse = ColorPresets.Gray;
                }

                //Color itemColorToUse = itemColor;

                bool drawItem = true;

                if (itemSquare.InPlayerFOV || SeeAllMap)
                {
                   
                }
                else if (itemSquare.SeenByPlayerThisRun)
                {
                    //Not in FOV now but seen this adventure
                    //Don't draw items in squares seen in previous adventures (since the items have respawned)
                    itemColorToUse = Color.Interpolate(item.GetColour(), ColorPresets.Black, 0.5);
                }
                else
                {
                    //Never in FOV
                    if (DebugMode)
                    {
                        itemColorToUse = itemColor;
                    }
                    else
                    {
                        //Can't see it, don't draw it
                        drawItem = false;
                    }
                }

                if (drawItem)
                {
                    rootConsole.ForegroundColor = itemColorToUse;
                    rootConsole.PutChar(mapTopLeft.x + item.LocationMap.x, mapTopLeft.y + item.LocationMap.y, item.Representation);
                }
                //rootConsole.Flush();
                //KeyPress userKey = Keyboard.WaitForKeyPress(true);
            }

        }

        private void DrawFeatures(List<Feature> featureList)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Set default colour
            //rootConsole.ForegroundColor = featureColor;

            //Could consider storing here and sorting to give an accurate representation of multiple objects

            foreach (Feature feature in featureList)
            {
                //Don't draw features on other levels
                if (feature.LocationLevel != Game.Dungeon.Player.LocationLevel)
                    continue;

                //Colour depending on FOV (for development)
                MapSquare featureSquare = Game.Dungeon.Levels[feature.LocationLevel].mapSquares[feature.LocationMap.x, feature.LocationMap.y];

                Color featureColor = ColorPresets.White;

                bool drawFeature = true;

                if (featureSquare.InPlayerFOV || SeeAllMap)
                {
                    //In FOV
                    //rootConsole.ForegroundColor = inFOVTerrainColor;
                }
                else if (featureSquare.SeenByPlayer)
                {
                    //Not in FOV but seen
                    featureColor = Color.Interpolate(featureColor, ColorPresets.Black, 0.3);

                    //rootConsole.ForegroundColor = seenNotInFOVTerrainColor;
                }
                else
                {
                    //Never in FOV
                    if (DebugMode)
                    {
                        featureColor = neverSeenFOVTerrainColor;
                    }
                    else
                    {
                        //Used to be draw it in black. This is no different but nicer.
                        drawFeature = false;
                    }
                }

                if (drawFeature)
                {
                    rootConsole.ForegroundColor = featureColor;
                    rootConsole.PutChar(mapTopLeft.x + feature.LocationMap.x, mapTopLeft.y + feature.LocationMap.y, feature.Representation);
                }
            }

        }

        private void DrawCreatures(List<Monster> creatureList)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Set default colour
            //rootConsole.ForegroundColor = creatureColor;

            //Draw stuff about creatures which should be overwritten by other creatures
            foreach (Monster creature in creatureList)
            {
                if (creature.LocationLevel != Game.Dungeon.Player.LocationLevel)
                    continue;

                if (!creature.Alive)
                    continue;

                int monsterX = mapTopLeft.x + creature.LocationMap.x;
                int monsterY = mapTopLeft.y + creature.LocationMap.y;

                Color creatureColor = creature.RepresentationColor();
                rootConsole.ForegroundColor = creatureColor;

                MapSquare creatureSquare = Game.Dungeon.Levels[creature.LocationLevel].mapSquares[creature.LocationMap.x, creature.LocationMap.y];
                bool drawCreature = true;

                if (creatureSquare.InPlayerFOV)
                {
                    //In FOV
                    //rootConsole.ForegroundColor = creature.CreatureColor();
                }
                else if (creatureSquare.SeenByPlayer)
                {
                    //Not in FOV but seen
                    if (!DebugMode)
                        drawCreature = false;
                    //creatureColor = hiddenColor;
                }
                else
                {
                    //Never in FOV
                    if (!DebugMode)
                        drawCreature = false;

                }

                //In many levels in FlatlineRL, we can see all the monsters
                if (SeeAllMonsters)
                {
                    drawCreature = true;
                }

                if (!drawCreature)
                    continue;

                //Heading

                if (creature.ShowHeading())
                {
                    List<Point> headingMarkers;

                    if (creature.FOVType() == CreatureFOV.CreatureFOVType.Triangular)
                    {
                        headingMarkers = DirectionUtil.SurroundingPointsFromDirection(creature.Heading, new Point(monsterX, monsterY), 3);
                    }
                    else
                    {
                        //Base
                        headingMarkers = DirectionUtil.SurroundingPointsFromDirection(creature.Heading, new Point(monsterX, monsterY), 1);
                        
                        //Reverse first one
                        Point oppositeMarker = new Point(monsterX - (headingMarkers[0].x - monsterX), monsterY - (headingMarkers[0].y - monsterY));
                        headingMarkers.Add(oppositeMarker);
                    }

                    foreach (Point headingLoc in headingMarkers)
                    {
                        //Check heading is drawable

                        Map map = Game.Dungeon.Levels[creature.LocationLevel];

                        //LogFile.Log.LogEntryDebug("heading: " + creature.Representation + " loc: x: " + headingLoc.x.ToString() + " y: " + headingLoc.y.ToString(), LogDebugLevel.Low);

                        if (headingLoc.x > 0 && headingLoc.x < mapTopLeft.x + map.width
                            && headingLoc.y > 0 && headingLoc.y < mapTopLeft.y + map.height)// && Game.Dungeon.MapSquareIsWalkable(creature.LocationLevel, new Point(headingLoc.x, headingLoc.y))
                        {
                            //Draw as an coloring on the current icon
                            char charToOverwrite = rootConsole.GetChar(headingLoc.x, headingLoc.y);
                            //Dot is too hard to see
                            if (charToOverwrite == '.')
                                charToOverwrite = '\x9';
                            rootConsole.ForegroundColor = creature.RepresentationColor();

                            rootConsole.PutChar(headingLoc.x, headingLoc.y, charToOverwrite);
                        }
                    }
                }
            }

            foreach (Monster creature in creatureList)
            {
                //Not on this level
                if (creature.LocationLevel != Game.Dungeon.Player.LocationLevel)
                    continue;

                if (!creature.Alive)
                    continue;

                //Colour depending on FOV (for development)
                MapSquare creatureSquare = Game.Dungeon.Levels[creature.LocationLevel].mapSquares[creature.LocationMap.x, creature.LocationMap.y];
                Color creatureColor = creature.RepresentationColor();

                //Shouldn't really do this here but see if we can get away with it
                CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);

                bool drawCreature = true;

                if (creatureSquare.InPlayerFOV)
                {
                    //In FOV
                    //rootConsole.ForegroundColor = creature.CreatureColor();
                }
                else if (creatureSquare.SeenByPlayer)
                {
                    //Not in FOV but seen
                    if (!DebugMode)
                        drawCreature = false;
                    //creatureColor = hiddenColor;
                }
                else
                {
                    //Never in FOV
                    if (!DebugMode)
                        drawCreature = false;

                }

                //In many levels in FlatlineRL, we can see all the monsters
                if (SeeAllMonsters)
                {
                    drawCreature = true;
                }


                if (DebugMode)
                {
                    if (creatureSquare.InMonsterFOV)
                    {
                        creatureColor = Color.Interpolate(creatureColor, ColorPresets.Red, 0.4);
                    }

                    //Draw waypoints
                    MonsterFightAndRunAI monsterWithWP = creature as MonsterFightAndRunAI;
                    if (monsterWithWP != null &&
                        monsterWithWP.Waypoints.Count > 0)
                    {
                        int wpNo = 0;
                        foreach (Point wp in monsterWithWP.Waypoints)
                        {
                            int wpX = mapTopLeft.x + wp.x;
                            int wpY = mapTopLeft.y + wp.y;

                            rootConsole.PutChar(wpX, wpY, wpNo.ToString()[0]);
                            wpNo++;
                        }
                    }
                }

                if (drawCreature)
                {
                    rootConsole.ForegroundColor = creatureColor;
                    bool newBackground = false;
                    //Set background depending on status
                    if (creature.Charmed)
                    {
                        rootConsole.BackgroundColor = charmBackground;
                        newBackground = true;
                    }
                    else if (creature.Passive)
                    {
                        rootConsole.BackgroundColor = passiveBackground;
                        newBackground = true;
                    }
                    else if (creature == CreatureToView)
                    {
                        //targetted
                        rootConsole.BackgroundColor = targettedBackground;
                        newBackground = true;
                    }
                    else if (creature.StunnedTurns > 0)
                    {
                        rootConsole.BackgroundColor = stunnedBackground;
                        newBackground = true;
                    }

                    if (newBackground == false)
                    {

                        IEquippableItem weapon = Game.Dungeon.Player.GetEquippedWeapon();

                        if (weapon != null)
                        {

                            //In range firing
                            if (weapon.HasFireAction() && Dungeon.TestRangeFOVForWeapon(Game.Dungeon.Player, creature, weapon.RangeFire(), currentFOV))
                            {
                                rootConsole.BackgroundColor = inRangeBackground;
                                newBackground = true;
                            }
                            else
                            {
                                //In throwing range
                                if (weapon.HasThrowAction() && Dungeon.TestRangeFOVForWeapon(Game.Dungeon.Player, creature, weapon.RangeFire(), currentFOV))
                                {
                                    rootConsole.BackgroundColor = inRangeBackground;
                                    newBackground = true;
                                }
                            }

                            //Also agressive
                            if (newBackground == true && creature.InPursuit())
                            {
                                rootConsole.BackgroundColor = inRangeAndAggressiveBackground;
                            }
                        }
                    }

                    if (newBackground == false)
                    {
                        if (creature.InPursuit())
                        {
                            rootConsole.BackgroundColor = pursuitBackground;
                            newBackground = true;
                        }
                        else if (!creature.OnPatrol())
                        {
                            rootConsole.BackgroundColor = investigateBackground;
                            newBackground = true;
                        }
                        else if (creature.Unique)
                            rootConsole.BackgroundColor = uniqueBackground;
                        else
                            rootConsole.BackgroundColor = normalBackground;
                    }

                    //Creature
                    int monsterX = mapTopLeft.x + creature.LocationMap.x;
                    int monsterY = mapTopLeft.y + creature.LocationMap.y;

                    rootConsole.PutChar(monsterX, monsterY, creature.Representation);


                }
            }

            //Reset the background
            rootConsole.BackgroundColor = normalBackground;
        }

        public void DrawFOVDebug(int levelNo)
        {
            Map map = Game.Dungeon.Levels[levelNo];
            TCODFov fov = Game.Dungeon.FOVs[levelNo];

            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            for (int i = 0; i < map.width; i++)
            {
                for (int j = 0; j < map.height; j++)
                {
                    int screenX = mapTopLeft.x + i;
                    int screenY = mapTopLeft.y + j;

                    bool trans;
                    bool walkable;

                    fov.GetCell(i, j, out trans, out walkable);

                    Color drawColor = inFOVTerrainColor;

                    if (walkable)
                    {
                        drawColor = inFOVTerrainColor;
                    }
                    else
                    {
                        drawColor = inMonsterFOVTerrainColor;
                    }

                    rootConsole.ForegroundColor = drawColor;
                    char screenChar = StringEquivalent.TerrainChars[map.mapSquares[i, j].Terrain];
                    screenChar = '#';
                    rootConsole.PutChar(screenX, screenY, screenChar);

                    rootConsole.Flush();
                }
            }

        }
        public void SaveCurrentLevelToDisk()
        {
            Map map = Game.Dungeon.Levels[Game.Dungeon.Player.LocationLevel];

            StreamWriter outFile = new StreamWriter("outfile.txt");

            for (int i = 0; i < map.height; i++)
            {
                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < map.width; j++)
                {
                    sb.Append(StringEquivalent.TerrainChars[map.mapSquares[j, i].Terrain]);

                }
                outFile.WriteLine(sb.ToString());
            }

        }


        //Draw a map only (useful for debugging)
        public void DrawMapDebug(Map map)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            for (int i = 0; i < map.width; i++)
            {
                for (int j = 0; j < map.height; j++)
                {
                    int screenX = mapTopLeft.x + i;
                    int screenY = mapTopLeft.y + j;

                    char screenChar = StringEquivalent.TerrainChars[map.mapSquares[i, j].Terrain];

                    Color drawColor = inFOVTerrainColor;

                    if (!map.mapSquares[i, j].BlocksLight)
                    {
                        //In FOV
                        rootConsole.ForegroundColor = inFOVTerrainColor;
                    }
                    else
                    {
                        //Not in FOV but seen
                        rootConsole.ForegroundColor = seenNotInFOVTerrainColor;
                    }
                    rootConsole.PutChar(screenX, screenY, screenChar);
                }
            }
            
            //Flush the console
            rootConsole.Flush();
        }

        //Draw a map only (useful for debugging)
        public void DrawMapDebugHighlight(Map map, int x1, int y1, int x2, int y2)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            for (int i = 0; i < map.width; i++)
            {
                for (int j = 0; j < map.height; j++)
                {
                    int screenX = mapTopLeft.x + i;
                    int screenY = mapTopLeft.y + j;

                    char screenChar = StringEquivalent.TerrainChars[map.mapSquares[i, j].Terrain];

                    Color drawColor = inFOVTerrainColor;

                    if (i == x1 && j == y1)
                    {
                        drawColor = ColorPresets.Red;
                    }

                    if (i == x2 && j == y2)
                    {
                        drawColor = ColorPresets.Red;
                    }
                    rootConsole.ForegroundColor = drawColor;
                    /*
                    if (!map.mapSquares[i, j].BlocksLight)
                    {
                        //In FOV
                        rootConsole.ForegroundColor = inFOVTerrainColor;
                    }
                    else
                    {
                        //Not in FOV but seen
                        rootConsole.ForegroundColor = seenNotInFOVTerrainColor;
                    }*/
                    rootConsole.PutChar(screenX, screenY, screenChar);
                }
            }

            //Flush the console
            rootConsole.Flush();
        }

        private void DrawMap(Map map)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            for (int i = 0; i < map.width; i++)
            {
                for (int j = 0; j < map.height; j++)
                {
                    int screenX = mapTopLeft.x + i;
                    int screenY = mapTopLeft.y + j;

                    char screenChar;
                    Color baseDrawColor;
                    Color drawColor;

                    //Exception for literals
                    if (map.mapSquares[i, j].Terrain == MapTerrain.Literal)
                    {
                        screenChar = map.mapSquares[i, j].terrainLiteral;
                        if (screenChar >= 'A' && screenChar <= 'Z')
                            baseDrawColor = literalTextColor;
                        else if (screenChar >= 'a' && screenChar <= 'z')
                            baseDrawColor = literalTextColor;
                        else
                            baseDrawColor = literalColor;
                    }
                    else
                    {
                        screenChar = StringEquivalent.TerrainChars[map.mapSquares[i, j].Terrain];
                        baseDrawColor = StringEquivalent.TerrainColors[map.mapSquares[i, j].Terrain];
                    }

                    //In FlatlineRL you can normally see the whole map
                    if (map.mapSquares[i, j].InPlayerFOV || SeeAllMap)
                    {
                        //In FOV or in town
                        drawColor = baseDrawColor;
                    }
                    else if (map.mapSquares[i, j].SeenByPlayer)
                    {
                        //Not in FOV but seen
                        drawColor = Color.Interpolate(baseDrawColor, ColorPresets.Black, 0.4);

                        //rootConsole.ForegroundColor = seenNotInFOVTerrainColor;
                    }
                    else
                    {
                        //Never in FOV
                        if (DebugMode)
                            drawColor = Color.Interpolate(baseDrawColor, ColorPresets.Black, 0.6);
                        else
                            drawColor = hiddenColor;
                    }

                    //Monster FOV in debug mode
                    if (DebugMode)
                    {
                        //Draw player FOV explicitally
                        if (map.mapSquares[i, j].InPlayerFOV)
                        {
                            drawColor = Color.Interpolate(baseDrawColor, ColorPresets.Blue, 0.6);
                        }


                        //Draw monster FOV
                        if (map.mapSquares[i, j].InMonsterFOV)
                        {
                            drawColor = Color.Interpolate(baseDrawColor, ColorPresets.Red, 0.6);
                        }

                        //Draw sounds
                        if (map.mapSquares[i, j].SoundMag > 0.0001)
                        {
                            drawColor = Color.Interpolate(baseDrawColor, ColorPresets.Yellow, map.mapSquares[i, j].SoundMag);
                        }
                    }

                    rootConsole.ForegroundColor = drawColor;
                    rootConsole.PutChar(screenX, screenY, screenChar);
                }
            }

        }
        internal void ConsoleLine(string datedEntry)
        {
            Console.WriteLine(datedEntry);
        }

        internal void ClearMessageLine()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            lastMessage = null;

            ClearMessageBar();
        }

        /// <summary>
        /// Print message in message bar
        /// </summary>
        /// <param name="message"></param>
        internal void PrintMessage(string message)
        {
            PrintMessage(message, messageColor);
        }

        /// <summary>
        /// Print message in message bar
        /// </summary>
        /// <param name="message"></param>
        internal void PrintMessage(string message, Color color)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Update state
            lastMessage = message;

            //Clear message bar
            ClearMessageBar();

            //Display new message
            rootConsole.ForegroundColor = color;
            rootConsole.PrintLineRect(message, msgDisplayTopLeft.x, msgDisplayTopLeft.y, Width - msgDisplayTopLeft.x, msgDisplayNumLines, LineAlignment.Left);
        }

        /// <summary>
        /// Print message at any point on screen
        /// </summary>
        /// <param name="message"></param>
        internal void PrintMessage(string message, Point topLeft, int width)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Update state
            lastMessage = message;

            //Clear message bar
            rootConsole.DrawRect(topLeft.x, topLeft.y, width, 1, true);

            //Display new message
            rootConsole.PrintLineRect(message, topLeft.x, topLeft.y, width, 1, LineAlignment.Left);
        }

        void ClearMessageBar()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.DrawRect(msgDisplayTopLeft.x, msgDisplayTopLeft.y, Width - msgDisplayTopLeft.x, msgDisplayNumLines, true);
        }

        private void ResetOverlayScreens() {
            displayEquipment = false;
            displayEquipmentSelect = false;
            displayInventory = false;
            displaySpecialMoveMovies = false;
            displaySpells = false;
            displayTrainingUI = false;
        }

        public bool DisplayTrainingUI
        {
            set
            {
                if (value == true)
                {
                    ResetOverlayScreens();
                }

                displayTrainingUI = value;
            }
        }

        public bool DisplayInventory
        {
            set
            {
                if (value == true)
                {
                    ResetOverlayScreens();
                }
                
                displayInventory = value;
            }
        }

        public bool DisplayEquipment
        {
            set
            {
                if (value == true)
                {
                   ResetOverlayScreens();
                }

                displayEquipment = value;
            }
        }

        public bool DisplayEquipmentSelect
        {
            set
            {
                if (value == true)
                {
                    ResetOverlayScreens();
                }

                displayEquipmentSelect = value;
            }
        }

        public bool DisplaySpecialMoveMovies
        {
            set
            {
                if (value == true)
                {
                    ResetOverlayScreens();
                }
                displaySpecialMoveMovies = value;
            }
        }

        public bool DisplaySpells
        {
            set
            {
                if (value == true)
                {
                    ResetOverlayScreens();
                }
                displaySpells = value;
            }
        }

        public int SelectedInventoryIndex
        {
            set
            {
                selectedInventoryIndex = value;
            }
        }

        public int TopInventoryIndex
        {
            set
            {
                topInventoryIndex = value;
            }
        }

        public Inventory CurrentInventory
        {
            set
            {
                currentInventory = value;
            }
        }

        public List<EquipmentSlotInfo> CurrentEquipment
        {
            set
            {
                currentEquipment = value;
            }
        }

        /// <summary>
        /// String displayed at the top of the inventory
        /// </summary>
        public string InventoryTitle
        {
            set
            {
                inventoryTitle = value;
            }
        }

        /// <summary>
        /// String displayed at the bottom of the inventory
        /// </summary>
        public string InventoryInstructions
        {
            set
            {
                inventoryInstructions = value;
            }
        }

        /// <summary>
        /// Get a string from the user. Uses the message bar
        /// </summary>
        /// <returns></returns>
       
        internal string GetUserString(string introMessage)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            ClearMessageLine();

            PrintMessage(introMessage + ": ");
            FlushConsole();

            bool continueInput = true;

            int maxChars = 40;

            string userString = "";

            do
            {
                //Get user input
                KeyPress userKey = Keyboard.WaitForKeyPress(true);

                //Each state has different keys

                        if (userKey.KeyCode == KeyCode.TCODK_CHAR)
                        {
                            char keyCode = (char)userKey.Character;
                            if (userString.Length < maxChars)
                            {
                                userString += keyCode.ToString();
                            }
                        }
                        else {
                            //Special keys
                            switch (userKey.KeyCode)
                            {
                                case KeyCode.TCODK_0:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += "0";
                                    }
                                    break;
                                case KeyCode.TCODK_1:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += "1";
                                    }
                                    break;
                                case KeyCode.TCODK_2:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += "2";
                                    }
                                    break;
                                case KeyCode.TCODK_3:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += "3";
                                    }
                                    break;
                                case KeyCode.TCODK_4:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += "4";
                                    }
                                    break;
                                case KeyCode.TCODK_5:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += "5";
                                    }
                                    break;
                                case KeyCode.TCODK_6:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += "6";
                                    }
                                    break;
                                case KeyCode.TCODK_7:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += "7";
                                    }
                                    break;
                                case KeyCode.TCODK_8:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += "8";
                                    }
                                    break;
                                case KeyCode.TCODK_9:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += "9";
                                    }
                                    break;
                                case KeyCode.TCODK_SPACE:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += " ";
                                    }
                                    break;


                                case KeyCode.TCODK_ESCAPE:
                                    //Exit
                                    return null;
                                case KeyCode.TCODK_BACKSPACE:
                                    if (userString.Length != 0)
                                    {
                                        userString = userString.Substring(0, userString.Length - 1);
                                    }
                                    break;
                                case KeyCode.TCODK_ENTER:
                                case KeyCode.TCODK_KPENTER:
                                    //Exit with what we have
                                    return userString;
                            }
                        }

                        PrintMessage(introMessage + ": " + userString + "_");
                        FlushConsole();

            } while (continueInput);

            return null;
        }

        /// <summary>
        /// Get a string from the user. One line only.
        /// maxChars is the max length of the input string (not including the introMessage)
        /// </summary>
        /// <returns></returns>

        internal string GetUserString(string introMessage, Point topLeft, int maxChars)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            ClearMessageLine();

            PrintMessage(introMessage + "", topLeft, introMessage.Length + 2 + maxChars);
            FlushConsole();

            bool continueInput = true;

            string userString = "";

            do
            {
                //Get user input
                KeyPress userKey = Keyboard.WaitForKeyPress(true);

                //Each state has different keys

                if (userKey.KeyCode == KeyCode.TCODK_CHAR)
                {
                    char keyCode = (char)userKey.Character;
                    if (userString.Length < maxChars)
                    {
                        userString += keyCode.ToString();
                    }
                }
                else
                {
                    //Special keys
                    switch (userKey.KeyCode)
                    {
                        case KeyCode.TCODK_0:
                            if (userString.Length < maxChars)
                            {
                                userString += "0";
                            }
                            break;
                        case KeyCode.TCODK_1:
                            if (userString.Length < maxChars)
                            {
                                userString += "1";
                            }
                            break;
                        case KeyCode.TCODK_2:
                            if (userString.Length < maxChars)
                            {
                                userString += "2";
                            }
                            break;
                        case KeyCode.TCODK_3:
                            if (userString.Length < maxChars)
                            {
                                userString += "3";
                            }
                            break;
                        case KeyCode.TCODK_4:
                            if (userString.Length < maxChars)
                            {
                                userString += "4";
                            }
                            break;
                        case KeyCode.TCODK_5:
                            if (userString.Length < maxChars)
                            {
                                userString += "5";
                            }
                            break;
                        case KeyCode.TCODK_6:
                            if (userString.Length < maxChars)
                            {
                                userString += "6";
                            }
                            break;
                        case KeyCode.TCODK_7:
                            if (userString.Length < maxChars)
                            {
                                userString += "7";
                            }
                            break;
                        case KeyCode.TCODK_8:
                            if (userString.Length < maxChars)
                            {
                                userString += "8";
                            }
                            break;
                        case KeyCode.TCODK_9:
                            if (userString.Length < maxChars)
                            {
                                userString += "9";
                            }
                            break;
                        case KeyCode.TCODK_SPACE:
                            if (userString.Length < maxChars)
                            {
                                userString += " ";
                            }
                            break;


                        case KeyCode.TCODK_ESCAPE:
                            //Exit
                            return null;
                        case KeyCode.TCODK_BACKSPACE:
                            if (userString.Length != 0)
                            {
                                userString = userString.Substring(0, userString.Length - 1);
                            }
                            break;
                        case KeyCode.TCODK_ENTER:
                            //Exit with what we have
                            return userString;
                    }
                }

                PrintMessage(introMessage + "" + userString + "_", topLeft, introMessage.Length + 2 + maxChars);
                FlushConsole();

            } while (continueInput);

            return null;
        }

        internal bool YesNoQuestion(string introMessage, Point topLeft)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //ClearMessageLine();

            PrintMessage(introMessage + " (y / n):", topLeft, introMessage.Length + 8);
            FlushConsole();

            do
            {
                //Get user input
                KeyPress userKey = Keyboard.WaitForKeyPress(true);

                //Each state has different keys

                if (userKey.KeyCode == KeyCode.TCODK_CHAR)
                {

                   char keyCode = (char)userKey.Character;
                   switch (keyCode)
                   {
                       case 'y':
                           ClearMessageLine();
                           return true;
                           
                       case 'n':
                           ClearMessageLine();
                           return false;
                           
                   }
                }
            } while(true);
        }

        internal bool YesNoQuestion(string introMessage)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            ClearMessageLine();

            PrintMessage(introMessage + " (y / n):");
            FlushConsole();

            do
            {
                //Get user input
                KeyPress userKey = Keyboard.WaitForKeyPress(true);

                //Each state has different keys

                if (userKey.KeyCode == KeyCode.TCODK_CHAR)
                {

                    char keyCode = (char)userKey.Character;
                    switch (keyCode)
                    {
                        case 'y':
                            ClearMessageLine();
                            return true;

                        case 'n':
                            ClearMessageLine();
                            return false;

                    }
                }
            } while (true);
        }

        internal GameDifficulty DifficultyQuestion(string introMessage, Point topLeft)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //ClearMessageLine();

            PrintMessage(introMessage + " (e / m / h)", topLeft, introMessage.Length + 14);
            FlushConsole();

            do
            {
                //Get user input
                KeyPress userKey = Keyboard.WaitForKeyPress(true);

                //Each state has different keys

                if (userKey.KeyCode == KeyCode.TCODK_CHAR)
                {

                   char keyCode = (char)userKey.Character;
                   switch (keyCode)
                   {
                       case 'e':
                           ClearMessageLine();
                           return GameDifficulty.Easy;
                           
                       case 'm':
                           ClearMessageLine();
                           return GameDifficulty.Medium;

                       case 'h':
                           ClearMessageLine();
                           return GameDifficulty.Hard;
                           
                   }
                }
            } while(true);
        }

        /// <summary>
        /// Draw the screen and run the message queue
        /// </summary>
        public void Update()
        {
            //Draw screen 
            Draw();

            //Message queue - requires keyboard to advance messages - not sure about this yet
            Game.MessageQueue.RunMessageQueue();
        }



        /// <summary>
        /// Do a missile attack animation. creature firing from start to finish in color.
        /// Currently checks if the target and origin creature are in FOV but not the path itself
        /// Currently only used for MvP attacks
        /// </summary>
        /// <param name="LocationMap"></param>
        /// <param name="point"></param>
        /// <param name="point_3"></param>
        /// <param name="color"></param>
        internal void DrawMissileAttack(Creature originCreature, Creature target, CombatResults result, Color color)
        {
            if (!CombatAnimations)
                return;

            //Check that the player can see the action

            MapSquare creatureSquare = Game.Dungeon.Levels[originCreature.LocationLevel].mapSquares[originCreature.LocationMap.x, originCreature.LocationMap.y];
            MapSquare targetSquare = Game.Dungeon.Levels[target.LocationLevel].mapSquares[target.LocationMap.x, target.LocationMap.y];

            if (!creatureSquare.InPlayerFOV && !targetSquare.InPlayerFOV)
                return;
            
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //We need to path find a flash line so it doesn't go through any walls

            //To do: cache the map

            //Generate path object
            TCODPathFinding path = new TCODPathFinding(Game.Dungeon.FOVs[originCreature.LocationLevel], 1.0);
            path.ComputePath(originCreature.LocationMap.x, originCreature.LocationMap.y, target.LocationMap.x, target.LocationMap.y);

            //Draw a flash line

            //Draw the screen as normal
            Draw();

            //Flash the attacker
            /*
            if (creatureSquare.InPlayerFOV)
            {
                rootConsole.ForegroundColor = ColorPresets.White;
                rootConsole.PutChar(mapTopLeft.x + originCreature.LocationMap.x, mapTopLeft.y + originCreature.LocationMap.y, originCreature.Representation);
            }*/

            //Calculate and draw the line overlay

            int x, y;
            int lastX, lastY;
            int targetX = target.LocationMap.x;
            int targetY = target.LocationMap.y;

            path.GetPathOrigin(out x, out y);
            lastX = x; lastY = y;

            rootConsole.ForegroundColor = color;

            bool finishedPath = false;

            do
            {
                finishedPath = path.WalkPath(ref x, ref y, false);

                int deltaX = x - lastX;
                int deltaY = y - lastY;

                char drawChar = '-';

                if (deltaX < 0 && deltaY < 0)
                    drawChar = '\\';
                else if (deltaX < 0 && deltaY > 0)
                    drawChar = '/';
                else if (deltaX > 0 && deltaY < 0)
                    drawChar = '/';
                else if (deltaX > 0 && deltaY > 0)
                    drawChar = '\\';
                else if (deltaY == 0)
                    drawChar = '-';
                else if (deltaX == 0)
                    drawChar = '|';

                rootConsole.PutChar(mapTopLeft.x + x, mapTopLeft.y + y, drawChar);

                lastX = x;
                lastY = y;

            } while (x != targetX || y != targetY && x != lastX && y != lastY);

            path.Dispose();

            //Flash the target if they were damaged
            //Draw them in either case so that we overwrite the missile animation on the target square with the creature

            if (targetSquare.InPlayerFOV)
            {
                if (result == CombatResults.DefenderDamaged || result == CombatResults.DefenderDied)
                {
                    rootConsole.ForegroundColor = ColorPresets.Red;
                    rootConsole.PutChar(mapTopLeft.x + target.LocationMap.x, mapTopLeft.y + target.LocationMap.y, target.Representation);
                }
                else
                {
                    rootConsole.ForegroundColor = target.RepresentationColor();
                    rootConsole.PutChar(mapTopLeft.x + target.LocationMap.x, mapTopLeft.y + target.LocationMap.y, target.Representation);
                }
            }

            rootConsole.ForegroundColor = normalForeground;

            //Draw line overlay
            FlushConsole();

            //Wait
            TCODSystem.Sleep(missileDelay);

            //Draw normally
            Draw();
            FlushConsole();          
        }

        /// <summary>
        /// Do a melee attack animation
        /// </summary>
        /// <param name="monsterFightAndRunAI"></param>
        /// <param name="newTarget"></param>
        internal void DrawMeleeAttack(Creature creature, Creature newTarget, CombatResults result)
        {
            if (!CombatAnimations)
                return;

            //Check that the player can see the action

            MapSquare creatureSquare = Game.Dungeon.Levels[creature.LocationLevel].mapSquares[creature.LocationMap.x, creature.LocationMap.y];
            MapSquare targetSquare = Game.Dungeon.Levels[newTarget.LocationLevel].mapSquares[newTarget.LocationMap.x, newTarget.LocationMap.y];

            if (!creatureSquare.InPlayerFOV && !targetSquare.InPlayerFOV)
                return;

            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw screen normally
            //Necessary since on a player move, his old position will show unless we do this
            Draw();
            FlushConsole();

            //Flash the attacker
            /*
            Color creatureColor = creature.RepresentationColor();

            if (creatureSquare.InPlayerFOV)
            {
                rootConsole.ForegroundColor = ColorPresets.White;
                rootConsole.PutChar(mapTopLeft.x + creature.LocationMap.x, mapTopLeft.y + creature.LocationMap.y, creature.Representation);
            }
            */
            //Flash the attached creature

            if (targetSquare.InPlayerFOV)
            {
                if (result == CombatResults.DefenderDamaged || result == CombatResults.DefenderDied)
                {
                    rootConsole.ForegroundColor = ColorPresets.Red;
                    rootConsole.PutChar(mapTopLeft.x + newTarget.LocationMap.x, mapTopLeft.y + newTarget.LocationMap.y, newTarget.Representation);
                }
            }



            //Update the screen

            //Draw flash
            FlushConsole();

            //Wait
            TCODSystem.Sleep(meleeDelay);

            //Draw screen normally
            Draw();
            FlushConsole();
        }
    }
}
