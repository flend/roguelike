using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;
using Console = System.Console;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using System.Linq;

namespace RogueBasin {

    /// <summary>
    /// Contains code that renders to the screen and some utility functions.
    /// Is not serialized, so should not contain any game state.
    /// Contains a bit of state about overlay screens etc.
    /// </summary>
    public class Screen
    {
        public enum TileLevel {
            Terrain = 0,
            Features = 1,
            Items = 2,
            CreatureDecoration = 3,
            Creatures = 4,
            Animations = 5,
            TargettingUI = 6
        }

        static Screen instance = null;

        //Console/screen size
        public int Width { get; set; }
        public int Height { get; set; }

        public bool DebugMode { get; set; }

        /// <summary>
        /// Show flashes on attacks and thrown projectiles
        /// </summary>
        public bool CombatAnimations { get; set; }

        int ShowRoomNumbering { get; set; }
        /// <summary>
        /// Total modes available for show room numbering
        /// </summary>
        int MaxShowRoomNumbering = 2;

        public bool SetTargetInRange = false;

        //Top left coord to start drawing the map at
        //Set by DrawMap
        Point mapTopLeft;

        Point mapTopLeftBase;
        Point mapBotRightBase;

        /// <summary>
        /// Dimensions of message display area
        /// </summary>
        Point msgDisplayTopLeft;
        Point msgDisplayBotRight;
        public int msgDisplayNumLines;

        Point statsDisplayTopLeft;
        Point statsDisplayBotRight;

        Point hitpointsOffset;     

        Color inFOVTerrainColor = ColorPresets.White;
        Color seenNotInFOVTerrainColor = ColorPresets.Gray;
        Color neverSeenFOVTerrainColor;
        Color inMonsterFOVTerrainColor = ColorPresets.Blue;

        Color statsColor = ColorPresets.Khaki;
        Color nothingColor = ColorPresets.Gray;

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
        Color targettedBackground = ColorPresets.DarkSlateGray;

        Color frameColor = ColorPresets.MediumSeaGreen;

        Color targetBackground = ColorPresets.White;
        Color targetForeground = ColorPresets.Black;

        Color literalColor = ColorPresets.BurlyWood;
        Color literalTextColor = ColorPresets.White;

        Color headingColor = ColorPresets.Yellow;

        Color messageColor = ColorPresets.CadetBlue;

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

        const int missileDelay = 250;
        const int meleeDelay = 100;
        
        public int MsgLogWrapWidth { get; set; }

        //Death members
        public List<string> TotalKills { get; set; }
        public List<string> DeathPreamble { get; set; }

        Point DeathTL { get; set; }
        int DeathWidth { get; set; }
        int DeathHeight { get; set; }

        Point movieTL = new Point(0, 0);
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

        public int ViewportScrollSpeed { get; set; }

        public uint MessageQueueWidth { get; private set; }

        public static Screen Instance
        {
            get
            {
                if (instance == null)
                    instance = new Screen();
                return instance;
            }
        }

        /// <summary>
        /// Master tile map for displaying the screen
        /// </summary>
        TileEngine.TileMap tileMap;

        /// <summary>
        /// Viewable area TL offset
        /// </summary>
        private Point viewTL;
        private Point viewBR;

        private int ViewableWidth {get; set; }
        private int ViewableHeight {get; set; }

        char explosionIcon = (char)505;

        public int LevelToDisplay
        {
            get; set;
        }

        Screen()
        {
            Width = 60;
            Height = 35;

            ViewableWidth = 37;
            ViewableHeight = 30;

            ViewportScrollSpeed = 1;

            viewTL = new Point(0, 0);
            SetViewBRFromTL();

            LevelToDisplay = 0;

            DebugMode = false;
            CombatAnimations = true;

            msgDisplayTopLeft = new Point(2, 1);
            msgDisplayBotRight = new Point(57, 3);

            MessageQueueWidth = (uint)(msgDisplayBotRight.y - msgDisplayBotRight.x);

            msgDisplayNumLines = 3;

            mapTopLeftBase = new Point(2, 6);
            mapBotRightBase = new Point(38, 32);

            statsDisplayTopLeft = new Point(40, 6);
            statsDisplayBotRight = new Point(57, 32);

            inventoryTL = new Point(5, 5);
            inventoryTR = new Point(55, 5);
            inventoryBL = new Point(5, 30);

            trainingTL = new Point(15, 10);
            trainingTR = new Point(45, 10);
            trainingBL = new Point(15, 25);

            MsgLogWrapWidth = inventoryTR.x - inventoryTL.x - 4;

            //Colors
            neverSeenFOVTerrainColor = Color.FromRGB(90, 90, 90);

            TotalKills = null;

            DeathTL = new Point(1, 1);
            DeathWidth = 59;
            DeathHeight = 34;

            PCColor = ColorPresets.White;

            SeeAllMonsters = false;
            SeeAllMap = false;
        }

        //Setup the screen
        public void InitialSetup()
        {
            int tileSize = 16;

            try
            {
                tileSize = Convert.ToInt16(Game.Config.Entries["tilesize"]);
            }
            catch (Exception)
            {
                LogFile.Log.LogEntryDebug("Error getting tilesize from config file", LogDebugLevel.High);
            }

            string tileMapFilename = "shroom_moved.png";
            if(tileSize == 32)
                tileMapFilename = "shroom_moved_big.png";

            //CustomFontRequest fontReq = new CustomFontRequest("tallfont.png", 8, 16, CustomFontRequestFontTypes.LayoutAsciiInColumn);
            CustomFontRequest fontReq = new CustomFontRequest(tileMapFilename, tileSize, tileSize, CustomFontRequestFontTypes.LayoutAsciiInRow);
            //CustomFontRequest fontReq = new CustomFontRequest("tallfont.png", 8, 16, CustomFontRequestFontTypes.LayoutAsciiInColumn);
            //CustomFontRequest fontReq = new CustomFontRequest("tallfont.png", 8, 16, CustomFontRequestFontTypes.LayoutAsciiInColumn);
            //CustomFontRequest fontReq = new CustomFontRequest("shroom_moved_big.png", 32, 32, CustomFontRequestFontTypes.LayoutAsciiInRow);
            //CustomFontRequest fontReq = new CustomFontRequest("shroom_moved.png", 16, 16, CustomFontRequestFontTypes.LayoutAsciiInRow);
            //CustomFontRequest fontReq = new CustomFontRequest("Anikki_square_20x20.bmp", 20, 20, CustomFontRequestFontTypes.LayoutAsciiInRow);
            //CustomFontRequest fontReq = new CustomFontRequest("Markvii.png", 12, 12, CustomFontRequestFontTypes.LayoutAsciiInRow);
            //CustomFontRequest fontReq = new CustomFontRequest("Tahin_16x16_rounded.png", 16, 16, CustomFontRequestFontTypes.LayoutAsciiInRow);
            //CustomFontRequest fontReq = new CustomFontRequest("msgothic.png", 16, 16, CustomFontRequestFontTypes.LayoutAsciiInRow);
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

        /// <summary>
        /// Cycle the room number display between different modes
        /// </summary>
        public void CycleRoomNumbering() {

            ShowRoomNumbering++;

            if(ShowRoomNumbering > MaxShowRoomNumbering)
                ShowRoomNumbering = 0;
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
        /// Centre the view on a point
        /// </summary>
        /// <param name="viewCenter"></param>
        public void CenterViewOnPoint(Point viewCenter)
        {
            int viewTLx = viewCenter.x - (int)Math.Floor((double)ViewableWidth / 2);
            int viewTLy = viewCenter.y - (int)Math.Floor((double)ViewableHeight / 2);

            viewTL = new Point(viewTLx, viewTLy);

            SetViewBRFromTL();
        }

        /// <summary>
        /// Centre the view on a point
        /// </summary>
        /// <param name="viewCenter"></param>
        public void CenterViewOnPoint(int level, Point viewCenter)
        {
            LevelToDisplay = level;
            CenterViewOnPoint(viewCenter);
        }


        private void SetViewBRFromTL()
        {
            viewBR = new Point(viewTL.x + ViewableWidth - 1, viewTL.y + ViewableHeight - 1);
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
        /// ASCII line character for a direction
        /// </summary>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        /// <returns></returns>
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
        /// Draw a line following a path on a tile layer.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="color"></param>
        protected void DrawPathLine(TileLevel layerNo, Point start, Point end, Color foregroundColor, Color backgroundColor)
        {
            DrawPathLine(layerNo, start, end, foregroundColor, backgroundColor, (char)0);
        }

        /// <summary>
        /// Draw a line following a path on a tile layer. Override default line drawing
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="color"></param>
        protected void DrawPathLine(TileLevel layerNo, Point start, Point end, Color foregroundColor, Color backgroundColor, char drawChar)
        {
            //Draw the line overlay

            //Cast a line between the start and end

            int lastX = start.x;
            int lastY = start.y;

            foreach(Point p in Utility.GetPointsOnLine(start, end)) {

                //Don't draw the first char (where the player is)
                if(p == start)
                    continue;

                if (!isViewVisible(p))
                    continue;

                char c;
                if (drawChar == 0)
                    c = LineChar(p.x - lastX, p.y - lastY);
                else
                    c = drawChar;

                lastX = p.x;
                lastY = p.y;

                tileMapLayer(layerNo)[ViewRelative(p)] = new TileEngine.TileCell(c);
                tileMapLayer(layerNo)[ViewRelative(p)].TileFlag = new LibtcodColorFlags(foregroundColor, backgroundColor);
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

        /// <summary>
        /// Scroll the viewport, by the delta
        /// </summary>
        /// <param name="delta"></param>
        public void ScrollViewport(Point delta) {

            viewTL += delta * ViewportScrollSpeed;
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
        Color normalMovieColor = ColorPresets.MediumSeaGreen;
        Color flashMovieColor = ColorPresets.Red;
        
        public void PlayMovie(List<MovieFrame> frames, bool keypressBetweenFrames)
        {
            try
            {
                movieFrames = frames;

                PlayMovieFrames(keypressBetweenFrames);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to play movie from frames " + ex.Message);
            }
        }

        private Tuple<int, int> CalculateWidthHeightFromLines(List<string> lines)
        {
            int width = 0;

            foreach (string row in lines)
            {
                if (row.Length > width)
                    width = row.Length;
            }

            var height = lines.Count;

            return new Tuple<int, int>(width, height);
        }

        public void PlayLog(LogEntry logEntry)
        {
            try
            {
                movieFrames = new List<MovieFrame>();
                var logFrame = new MovieFrame();
                var allLines = new List<string>();
                allLines.Add(logEntry.title);
                allLines.AddRange(logEntry.lines);
                logFrame.scanLines = allLines;
                var dimensions = CalculateWidthHeightFromLines(allLines);
                logFrame.width = dimensions.Item1;
                logFrame.height = dimensions.Item2;

                movieFrames.Add(logFrame);

                PlayMovieFrames(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to play movie from frames " + ex.Message);
            }
        }

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

                //Load whole movie
                bool loadSuccess = LoadMovie(filenameRoot);

                if (!loadSuccess)
                {
                    LogFile.Log.LogEntryDebug("Failed to load movie file: " + filenameRoot, LogDebugLevel.High);
                    return;
                }

                PlayMovieFrames(keypressBetweenFrames);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to play movie: " + filenameRoot + " : " + ex.Message);
            }
        }

        private void PlayMovieFrames(bool keypressBetweenFrames)
        {
            int frameNo = 0;

            int width = Width - movieTL.x;
            int height = Height - movieTL.y - 5;
            Point frameTL = new Point(5, 5);

            //Draw each frame of the movie
            foreach (MovieFrame frame in movieFrames)
            {
                //Flatline - centre on each frame
                 width = frame.width;
                 height = frame.height;

                int xOffset = (Width - movieTL.x * 2 - width) / 2;
                int yOffset = (Height - movieTL.y * 2 - height) / 2;

                frameTL = new Point(movieTL.x + xOffset, movieTL.y + yOffset);
                int frameOffset = 2;

                //Draw frame
                DrawFrame(frameTL.x - frameOffset, frameTL.y - frameOffset, width + 2 * frameOffset + 1, height + 2 * frameOffset, true);

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
                        PrintLineRect("Press any key to continue", frameTL.x + width / 2, frameTL.y + height + 2, width, 1, LineAlignment.Center);
                        Screen.Instance.FlushConsole();
                        KeyPress userKey = Keyboard.WaitForKeyPress(true);
                        Screen.Instance.Update();
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
            PrintLineRect("Press ENTER to continue", frameTL.x + width / 2, frameTL.y + height + 1, width, 1, LineAlignment.Center);

            Screen.Instance.FlushConsole();

            //Await keypress then redraw normal screen
            WaitForEnterKey();

            UpdateNoMsgQueue();
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
                    Color flashColor = normalMovieColor;

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
                            flashColor = flashMovieColor;
                            nextCharFlash = false;
                        }
                        else
                        {
                            flashColor = normalMovieColor;
                        }

                        PutChar(frameTL.x + coffset, frameTL.y + offset, c, flashColor);
                        coffset++;
                    }

                }
                else
                {
                    //Print whole line
                    PrintLineRect(line, frameTL.x, frameTL.y + offset, width, 1, LineAlignment.Left, normalMovieColor);
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

        /// <summary>
        /// Return the view area coords from map coords
        /// </summary>
        /// <param name="absolutePoint"></param>
        /// <returns></returns>
        private Point ViewRelative(Point absolutePoint)
        {
            return absolutePoint - viewTL;
        }

        /// <summary>
        /// Is this map coord visible in the view space?
        /// </summary>
        /// <param name="absolutePoint"></param>
        /// <returns></returns>
        private bool isViewVisible(Point absolutePoint)
        {
            Point viewLocation = ViewRelative(absolutePoint);

            if (viewLocation.x >= 0 && viewLocation.y >= 0
                && viewLocation.x < ViewableWidth && viewLocation.y < ViewableHeight)
                return true;

            return false;
        }

        private void DrawPC(int levelToDraw, Player player)
        {
            if (player.LocationLevel != levelToDraw)
                return;
         
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

            if (!isViewVisible(PClocation))
                return;

            tileMapLayer(TileLevel.Creatures)[ViewRelative(PClocation)] = new TileEngine.TileCell(player.Representation);
            tileMapLayer(TileLevel.Creatures)[ViewRelative(PClocation)].TileFlag = new LibtcodColorFlags(PCDrawColor);
        }


        /// <summary>
        /// Fully rebuild the layered, tiled map. All levels, excluding animations
        /// </summary>
        private void BuildTiledMap()
        {
            Dungeon dungeon = Game.Dungeon;
            Player player = dungeon.Player;

            tileMap = new TileEngine.TileMap(7, ViewableHeight, ViewableWidth);

            int levelToDisplay = LevelToDisplay;

            //Draw the map screen

            //Draw terrain (must be done first since sets some params)
            //First level in tileMap
            DrawMap(levelToDisplay, dungeon.Levels);

            //Draw locks
            DrawLocks(levelToDisplay, dungeon.Locks);

            //Draw fixed features
            DrawFeatures(levelToDisplay, dungeon.Features);

            //Draw items (will appear on top of staircases etc.)
            DrawItems(levelToDisplay, dungeon.Items);

            //Draw creatures
            DrawCreatures(levelToDisplay, dungeon.Monsters);

            //Draw PC
            DrawPC(levelToDisplay, player);

            //Draw targetting cursor
            if (targettingMode)
                DrawTargettingCursor();

        }

        //Draw the current dungeon map and objects
        private void Draw()
        {

            Dungeon dungeon = Game.Dungeon;
            Player player = dungeon.Player;

            //Clear screen
            ClearScreen();

            //Build a tile map to display the screen
            //In future, we probably don't want to pull this down each time

            //Either use a dirty tile system, or simply have a flag to not change typical levels
            //E.g. an animation only changes anim, targetting only changes targetting

            //Build the full tiled map representation
            BuildTiledMap();
            
            //Render tiled map to screen
            MapRendererLibTCod.RenderMap(tileMap, new Point(0, 0), new System.Drawing.Rectangle(mapTopLeft.x, mapTopLeft.y, mapBotRightBase.x - mapTopLeftBase.x + 1, mapBotRightBase.y - mapTopLeftBase.y + 1));

            //Draw Stats
            DrawStats(dungeon.Player);

            if (ShowMsgHistory)
                DrawMsgHistory();

            if (ShowClueList)
                DrawCluesList();

            if (ShowLogList)
                DrawLogList();

        }

        /// <summary>
        /// Draws an animated attack. This a top level function which is used instead of Draw() as an entry to screen
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <param name="size"></param>
        public void DrawAreaAttackAnimation(List <Point> targetSquares, Color color)
        {
            //Clone the list since we mangle it
            List<Point> mangledPoints = new List<Point>();
            foreach (Point p in targetSquares)
            {
                mangledPoints.Add(new Point(p));
            }

            //Don't rebuild the static map (items, creatures etc.) since it hasn't changed
            
            //Clear targetting
            tileMap.ClearLayer((int)TileLevel.TargettingUI);

            //Add animation points into the animation layer

            foreach (Point p in mangledPoints)
            {
                if (!isViewVisible(p))
                    continue;

                tileMapLayer(TileLevel.Animations)[ViewRelative(p)] = new TileEngine.TileCell(explosionIcon);
                tileMapLayer(TileLevel.Animations)[ViewRelative(p)].TileFlag = new LibtcodColorFlags(color, ColorPresets.Black);
            }

            //Render the full layered map (with these animations) on screen
            MapRendererLibTCod.RenderMap(tileMap, new Point(0, 0), new System.Drawing.Rectangle(mapTopLeft.x, mapTopLeft.y, mapBotRightBase.x - mapTopLeftBase.x + 1, mapBotRightBase.y - mapTopLeftBase.y + 1));
            FlushConsole();

            //Wait
            TCODSystem.Sleep(missileDelay);

            //Wipe the animation layer
            tileMap.ClearLayer((int)TileLevel.Animations);

            //Draw again without animations
            MapRendererLibTCod.RenderMap(tileMap, new Point(0, 0), new System.Drawing.Rectangle(mapTopLeft.x, mapTopLeft.y, mapBotRightBase.x - mapTopLeftBase.x + 1, mapBotRightBase.y - mapTopLeftBase.y + 1));
            FlushConsole();
        }


        private void DrawTargettingCursor()
        {
            Player player = Game.Dungeon.Player;

            //Draw the area of effect

            switch (TargetType)
            {
                case TargettingType.Line:

                    //Draw a line up to the target
                    DrawPathLine(TileLevel.TargettingUI, player.LocationMap, Target, targetForeground, targetBackground);
                    //Should improve the getlinesquare function to give nicer output so we could use it here too

                    break;

                case TargettingType.LineThrough:

                    //Cast a line which terminates on the edge of the map
                    Point projectedLine = Game.Dungeon.GetEndOfLine(player.LocationMap, Target, player.LocationLevel);

                    //Get the in-FOV points up to that end point
                    WrappedFOV currentFOV2 = Game.Dungeon.CalculateAbstractFOV(Game.Dungeon.Player.LocationLevel, Game.Dungeon.Player.LocationMap, 80);
                    List<Point> lineSquares = Game.Dungeon.GetPathLinePointsInFOV(Game.Dungeon.Player.LocationLevel, Game.Dungeon.Player.LocationMap, projectedLine, currentFOV2);

                    DrawExplosionOverSquaresAndCreatures(lineSquares);      

                    break;
                    
                case TargettingType.Rocket:
                    {
                        //Get potention explosion points
                        int size = 2;

                        List<Point> splashSquares = GetPointsForCircularTarget(Target, size);

                        //Draw a line up to the target square
                        DrawPathLine(TileLevel.TargettingUI, player.LocationMap, Target, targetForeground, targetBackground);

                        DrawExplosionOverSquaresAndCreatures(splashSquares); 

                    }
                    break;

                case TargettingType.Shotgun:
                    {
                        int size = TargetRange;
                        double spreadAngle = TargetPermissiveAngle;

                        CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);
                        List<Point> splashSquares = currentFOV.GetPointsForTriangularTargetInFOV(player.LocationMap, Target, Game.Dungeon.Levels[player.LocationLevel], size, spreadAngle);

                        DrawExplosionOverSquaresAndCreatures(splashSquares);
                    }
                    break;
            }

            //Highlight target if in range
            if (!isViewVisible(Target))
                return;

            Color backgroundColor = targetBackground;
            Color foregroundColor = targetForeground;

            if (SetTargetInRange)
            {
                backgroundColor = ColorPresets.Red;
            }

            char toDraw = '.';
            int monsterIdInSquare = tileMapLayer(TileLevel.Creatures)[ViewRelative(Target)].TileID;
            var monsterColorInSquare = tileMapLayer(TileLevel.Creatures)[ViewRelative(Target)].TileFlag as LibtcodColorFlags;
            if (monsterIdInSquare == -1)
            {
                monsterIdInSquare = tileMapLayer(TileLevel.Features)[ViewRelative(Target)].TileID;
                monsterColorInSquare = tileMapLayer(TileLevel.Terrain)[ViewRelative(Target)].TileFlag as LibtcodColorFlags;
            }
            if (monsterIdInSquare == -1)
            {
                monsterIdInSquare = tileMapLayer(TileLevel.Terrain)[ViewRelative(Target)].TileID;
                monsterColorInSquare = tileMapLayer(TileLevel.Terrain)[ViewRelative(Target)].TileFlag as LibtcodColorFlags;
            }
            
            if (monsterIdInSquare != -1)
                toDraw = (char)monsterIdInSquare;

            tileMapLayer(TileLevel.TargettingUI)[ViewRelative(Target)] = new TileEngine.TileCell(toDraw);
            if(monsterColorInSquare != null)
                tileMapLayer(TileLevel.TargettingUI)[ViewRelative(Target)].TileFlag = new LibtcodColorFlags(monsterColorInSquare.BackgroundColor, monsterColorInSquare.ForegroundColor);
            
        }

        private void DrawExplosionOverSquaresAndCreatures(List<Point> splashSquares)
        {
            //Draw each point as targetted
            foreach (Point p in splashSquares)
            {
                if (!isViewVisible(p))
                    continue;

                //If there's a monster in the square, draw it in red in the animation layer. Otherwise, draw an explosion
                char toDraw = explosionIcon;
                int monsterIdInSquare = tileMapLayer(TileLevel.Creatures)[ViewRelative(p)].TileID;

                if (monsterIdInSquare != -1)
                    toDraw = (char)monsterIdInSquare;

                tileMapLayer(TileLevel.TargettingUI)[ViewRelative(p)] = new TileEngine.TileCell(toDraw);
                tileMapLayer(TileLevel.TargettingUI)[ViewRelative(p)].TileFlag = new LibtcodColorFlags(ColorPresets.Red);
            }
        }


        /// <summary>
        /// Screen for end of game info
        /// </summary>
        public void DrawEndOfGameInfo(List<string> stuffToDisplay)
        {
            //Clear screen
            ClearScreen();

            //Draw frame
            DrawFrame(DeathTL.x, DeathTL.y, DeathWidth, DeathHeight, true);

            //Draw title
            PrintLineRect("End of game summary", DeathTL.x + DeathWidth / 2, DeathTL.y, DeathWidth, 1, LineAlignment.Center);

            //Draw preamble
            int count = 0;
            foreach (string s in stuffToDisplay)
            {
                PrintLineRect(s, DeathTL.x + 2, DeathTL.y + 2 + count, DeathWidth - 4, 1, LineAlignment.Left);
                count++;
            }

            //Draw instructions

            PrintLineRect("Press ENTER to continue...", DeathTL.x + DeathWidth / 2, DeathTL.y + DeathHeight - 1, DeathWidth, 1, LineAlignment.Center);
            FlushConsole();

            WaitForEnterKey();
        }



        /// <summary>
        /// Screen for player victory
        /// </summary>
        public void DrawVictoryScreen()
        {
         
            //Clear screen
            ClearScreen();

            //Draw frame
            DrawFrame(DeathTL.x, DeathTL.y, DeathWidth, DeathHeight, true);

            //Draw title
            PrintLineRect("VICTORY!", DeathTL.x + DeathWidth / 2, DeathTL.y, DeathWidth, 1, LineAlignment.Center);

            //Draw preamble
            int count = 0;
            foreach (string s in DeathPreamble)
            {
                PrintLineRect(s, DeathTL.x + 2, DeathTL.y + 2 + count, DeathWidth - 4, 1, LineAlignment.Left);
                count++;
            }

            //Draw kills

            PrintLineRect("Total Kills", DeathTL.x + DeathWidth / 2, DeathTL.y + 2 + count + 2, DeathWidth, 1, LineAlignment.Center);

            foreach (string s in TotalKills)
            {
                PrintLineRect(s, DeathTL.x + 2, DeathTL.y + 2 + count + 4, DeathWidth - 4, 1, LineAlignment.Left);
                count++;
            }

            //Draw instructions

            PrintLineRect("Press any key to exit...", DeathTL.x + DeathWidth / 2, DeathTL.y + DeathHeight - 1, DeathWidth, 1, LineAlignment.Center);
        }


        public bool ShowMsgHistory { get; set; }

        public bool ShowClueList { get; set; }

        public bool ShowLogList { get; set; }

        enum Direction { up, down, none };

        void ClearScreen()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.Clear();
        }

        /// <summary>
        /// Draws a frame on the screen
        /// </summary>
        void DrawFrame(int x, int y, int width, int height, bool clear)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame - same as inventory
            rootConsole.DrawFrame(x, y, width, height, clear);
        }

        /// <summary>
        /// Draws a frame on the screen in a particular color
        /// </summary>
        void DrawFrame(int x, int y, int width, int height, bool clear, Color color)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.ForegroundColor = color;

            //Draw frame - same as inventory
            rootConsole.DrawFrame(x, y, width, height, clear);

            rootConsole.ForegroundColor = ColorPresets.White;
        }

        /// <summary>
        /// Character-based drawing. Kept only for stats etc. in transitional period. All map stuff now works in the tile layer
        /// </summary>
        void PutChar(int x, int y, char c, Color color)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();
            rootConsole.ForegroundColor = color;

            rootConsole.PutChar(x, y, c);

            rootConsole.ForegroundColor = ColorPresets.White;
        }

        /// <summary>
        /// Print a string in a rectangle
        /// </summary>
        void PrintLineRect(string msg, int x, int y, int width, int height, LineAlignment alignment)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.PrintLineRect(msg, x, y, width, height, alignment);
        }

        /// <summary>
        /// Print a string in a rectangle
        /// </summary>
        void PrintLineRect(string msg, int x, int y, int width, int height, LineAlignment alignment, Color color)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.ForegroundColor = color;
            rootConsole.PrintLineRect(msg, x, y, width, height, alignment);
            rootConsole.ForegroundColor = ColorPresets.White;
        }

        /// <summary>
        /// Print a string at a location
        /// </summary>
        void PrintLine(string msg, int x, int y, LineAlignment alignment)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.PrintLine(msg, x, y, alignment);
        }

        /// <summary>
        /// Print a string at a location
        /// </summary>
        void PrintLine(string msg, int x, int y, LineAlignment alignment, Color color)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();
            rootConsole.ForegroundColor = color;

            rootConsole.PrintLine(msg, x, y, alignment);
            rootConsole.ForegroundColor = ColorPresets.White;
        }

        /// <summary>
        /// Draw rectangle
        /// </summary>
        void DrawRect(int x, int y, int width, int height, bool clear)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.DrawRect(x, y, width, height, clear);
        }

        /// <summary>
        /// Draw the msg history and allow the player to scroll
        /// </summary>
        private void DrawMsgHistory()
        {
            //Draw frame - same as inventory
            DrawFrame(inventoryTL.x, inventoryTL.y, inventoryTR.x - inventoryTL.x + 1, inventoryBL.y - inventoryTL.y + 1, true);

            //Draw title
            PrintLineRect("Message History", (inventoryTL.x + inventoryTR.x) / 2, inventoryTL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Draw instructions
            PrintLineRect("Press (up) or (down) to scroll or (x) to exit", (inventoryTL.x + inventoryTR.x) / 2, inventoryBL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Active area is slightly reduced from frame
            int inventoryListX = inventoryTL.x + 2;
            int inventoryListW = inventoryTR.x - inventoryTL.x - 4;
            int inventoryListY = inventoryTL.y + 2;
            int inventoryListH = inventoryBL.y - inventoryTL.y - 4;

            LinkedList<string> msgHistory = Game.MessageQueue.messageHistory;

            //Display list
            DisplayStringList(inventoryListX, inventoryListW, inventoryListY, inventoryListH, msgHistory);
        }

        /// <summary>
        /// Draw the msg history and allow the player to scroll
        /// </summary>
        private void DrawCluesList()
        {
            //Draw frame - same as inventory
            DrawFrame(inventoryTL.x, inventoryTL.y, inventoryTR.x - inventoryTL.x + 1, inventoryBL.y - inventoryTL.y + 1, true);

            //Draw title
            PrintLineRect("Clue List", (inventoryTL.x + inventoryTR.x) / 2, inventoryTL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Draw instructions
            PrintLineRect("Press (up) or (down) to scroll or (x) to exit", (inventoryTL.x + inventoryTR.x) / 2, inventoryBL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Active area is slightly reduced from frame
            int inventoryListX = inventoryTL.x + 2;
            int inventoryListW = inventoryTR.x - inventoryTL.x - 4;
            int inventoryListY = inventoryTL.y + 2;
            int inventoryListH = inventoryBL.y - inventoryTL.y - 4;

            var allPlayerClueItems = Game.Dungeon.Player.Inventory.GetItemsOfType<Items.Clue>();
            var allPlayerClues = allPlayerClueItems.Select(i => i.ClueDescription);
            var noCluesForDoors = allPlayerClues.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());

            var cluesForDoorsAsStrings = noCluesForDoors.Select(kv => "(" + kv.Value + ")" + kv.Key);
            
            //Display list
            DisplayStringList(inventoryListX, inventoryListW, inventoryListY, inventoryListH, new LinkedList<string>(cluesForDoorsAsStrings));
        }

        /// <summary>
        /// Draw the msg history and allow the player to scroll
        /// </summary>
        private void DrawLogList()
        {
            //Draw frame - same as inventory
            DrawFrame(inventoryTL.x, inventoryTL.y, inventoryTR.x - inventoryTL.x + 1, inventoryBL.y - inventoryTL.y + 1, true);

            //Draw title
            PrintLineRect("Log List", (inventoryTL.x + inventoryTR.x) / 2, inventoryTL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Draw instructions
            PrintLineRect("Press (up) or (down) to scroll or (x) to exit", (inventoryTL.x + inventoryTR.x) / 2, inventoryBL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Active area is slightly reduced from frame
            int inventoryListX = inventoryTL.x + 2;
            int inventoryListW = inventoryTR.x - inventoryTL.x - 4;
            int inventoryListY = inventoryTL.y + 2;
            int inventoryListH = inventoryBL.y - inventoryTL.y - 4;

            var allPlayerLogItems = Game.Dungeon.Player.Inventory.GetItemsOfType<Items.Log>();
            var allPlayerLogEntriesSortedByLevel = allPlayerLogItems.GroupBy(i => i.LocationLevel).ToDictionary(gr => gr.Key, gr => gr.Select(i => i.LogEntry));
            
            List<string> clueLines = new List<string>();
            foreach (var kv in allPlayerLogEntriesSortedByLevel)
            {
                var level = kv.Key;
                clueLines.Add("-+-+-+-" + Game.Dungeon.DungeonInfo.LevelNaming[level].ToUpper() + "-+-+-+-");
                clueLines.Add("");

                foreach (var logEntry in kv.Value)
                {
                    clueLines.Add(logEntry.title);
                    clueLines.AddRange(logEntry.lines);
                    clueLines.Add("");
                }
            }

            //Display list
            DisplayStringList(inventoryListX, inventoryListW, inventoryListY, inventoryListH, new LinkedList<string>(clueLines));
        }

        private void DisplayStringList(int inventoryListX, int inventoryListW, int inventoryListY, int inventoryListH, LinkedList<string> msgHistory)
        {
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
                    PrintLineRect(displayedMsg.Value, inventoryListX, inventoryListY + i, inventoryListW, 1, LineAlignment.Left, normalMovieColor);
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
                bool page = false;

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

                        case KeyCode.TCODK_PAGEUP:
                            dir = Direction.up;
                            page = true;
                            break;

                        case KeyCode.TCODK_PAGEDOWN:
                            dir = Direction.down;
                            page = true;
                            break;
                    }
                }

                if (msgHistory.Count > 0)
                {
                    if (dir == Direction.up)
                    {
                        var iterations = 1;
                        if (page)
                            iterations = 20;

                        for (int i = 0; i < iterations;i++)
                            if (topLineDisplayed.Previous != null)
                                topLineDisplayed = topLineDisplayed.Previous;
                    }
                    else if (dir == Direction.down)
                    {
                        var iterations = 1;
                        if (page)
                            iterations = 20;

                        for (int i = 0; i < iterations; i++)
                            if (topLineDisplayed != bottomTopLineDisplayed)
                                topLineDisplayed = topLineDisplayed.Next;
                    }

                    //Clear the rectangle
                    DrawRect(inventoryTL.x + 1, inventoryTL.y + 1, inventoryTR.x - inventoryTL.x - 1, inventoryBL.y - inventoryTL.y - 1, true);

                    //Display the message log
                    displayedMsg = topLineDisplayed;
                    for (int i = 0; i < inventoryListH; i++)
                    {
                        PrintLineRect(displayedMsg.Value, inventoryListX, inventoryListY + i, inventoryListW, 1, LineAlignment.Left);
                        displayedMsg = displayedMsg.Next;
                        if (displayedMsg == null)
                            break;
                    }
                }
                Screen.Instance.FlushConsole();

            } while (keepLooping);
        }

        const char heartChar = (char)567;
        const char shieldChar = (char)561;
        const char ammoChar = (char)568;
        const char grenadeChar = (char)297;
        const char batteryChar = (char)308;

        Color orangeActivatedColor = ColorPresets.DarkOrange;
        Color batteryActivatedColor = ColorPresets.SlateBlue;
        Color orangeHighlightedColor = ColorPresets.Gold;
        Color orangeDisactivatedColor = ColorPresets.SaddleBrown;
        Color disabledColor = ColorPresets.DimGray;
        Color weaponColor = ColorPresets.LightSteelBlue;
        Color heartColor = ColorPresets.Crimson;

        private void DrawStats(Player player)
        {

            //Blank stats area
            //rootConsole.DrawRect(statsDisplayTopLeft.x, statsDisplayTopLeft.y, Width - statsDisplayTopLeft.x, Height - statsDisplayTopLeft.y, true);
            DrawFrame(statsDisplayTopLeft.x, statsDisplayTopLeft.y - 1, statsDisplayBotRight.x - statsDisplayTopLeft.x + 2, statsDisplayBotRight.y - statsDisplayTopLeft.y + 3, false, frameColor);

            int baseOffset = 2;

            //Mission
            Point missionOffset = new Point(baseOffset, 0);
            hitpointsOffset = new Point(baseOffset, 4);
            Point shieldOffset = new Point(baseOffset, 5);
            Point weaponOffset = new Point(baseOffset, 8);
            Point utilityOffset = new Point(baseOffset, 13);
            Point viewOffset = new Point(baseOffset, 19);
            Point cmbtOffset = new Point(baseOffset, 17);
            Point gameDataOffset = new Point(baseOffset, 24);

            var zoneName = "[" + (LevelToDisplay).ToString("00") + "] ";
            var zoneName2 = Game.Dungeon.DungeonInfo.LookupMissionName(LevelToDisplay);
            PrintLine(zoneName, statsDisplayTopLeft.x + missionOffset.x, statsDisplayTopLeft.y + missionOffset.y + 1, LineAlignment.Left, statsColor);
            PrintLine(zoneName2, statsDisplayTopLeft.x + missionOffset.x, statsDisplayTopLeft.y + missionOffset.y + 2, LineAlignment.Left, statsColor);
            
            //Draw HP Status

            int hpBarLength = 10;
            double playerHPRatio = player.Hitpoints / (double)player.MaxHitpoints;
            int hpBarEntries = (int)Math.Ceiling(hpBarLength * playerHPRatio);

            PrintLine("HP: ", statsDisplayTopLeft.x + hitpointsOffset.x, statsDisplayTopLeft.y + hitpointsOffset.y, LineAlignment.Left, statsColor);

            for (int i = 0; i < hpBarLength; i++)
            {
                if (i < hpBarEntries)
                {
                    PutChar(statsDisplayTopLeft.x + hitpointsOffset.x + 5 + i, statsDisplayTopLeft.y + hitpointsOffset.y, heartChar, heartColor);
                }
                else
                {
                    PutChar(statsDisplayTopLeft.x + hitpointsOffset.x + 5 + i, statsDisplayTopLeft.y + hitpointsOffset.y, heartChar, disabledColor);
                }
            }

            //Draw shield

            int shieldBarLength = 20;
            double playerShieldRatio = player.Shield / (double)player.MaxShield;
            int shieldBarEntries = (int)Math.Ceiling(shieldBarLength * playerShieldRatio);

            PrintLine("SD: ", statsDisplayTopLeft.x + shieldOffset.x, statsDisplayTopLeft.y + shieldOffset.y, LineAlignment.Left, statsColor);

            DrawShieldBar(player, shieldOffset, shieldBarEntries - 10);
            DrawShieldBar(player, shieldOffset + new Point(0, 1), Math.Min(shieldBarEntries, 10));

            //Draw equipped weapon

            Item weapon = Game.Dungeon.Player.GetEquippedWeaponAsItem();

            //string weaponStr = "Weapon: ";

            //PrintLine(weaponStr, statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y, LineAlignment.Left);

            if (weapon != null)
            {
                IEquippableItem weaponE = weapon as IEquippableItem;

                PutChar(statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y, weapon.Representation, weaponColor);

                string weaponStr = weapon.SingleItemDescription;
                PrintLine(weaponStr, statsDisplayTopLeft.x + weaponOffset.x + 2, statsDisplayTopLeft.y + weaponOffset.y, LineAlignment.Left, weaponColor);

                //Ammo
                if (weaponE.HasFireAction())
                {
                    PrintLine("Am: ", statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y + 3, LineAlignment.Left);
        
                    //TODO infinite ammo?
                    int ammoBarLength = 10;
                    double weaponAmmoRatio = weaponE.RemainingAmmo() / (double) weaponE.MaxAmmo();
                    int ammoBarEntries = (int)Math.Ceiling(ammoBarLength * weaponAmmoRatio);

                    for (int i = 0; i < ammoBarLength; i++)
                    {
                        if (i < ammoBarEntries)
                        {
                            PutChar(statsDisplayTopLeft.x + weaponOffset.x + 5 + i, statsDisplayTopLeft.y + weaponOffset.y + 3, ammoChar, orangeActivatedColor);
                        }
                        else
                        {
                            PutChar(statsDisplayTopLeft.x + weaponOffset.x + 5 + i, statsDisplayTopLeft.y + weaponOffset.y + 3, ammoChar, orangeDisactivatedColor);
                        }
                    }
                }
                else if (weaponE.HasThrowAction() || weaponE.HasOperateAction())
                {
                    PrintLine("Am: ", statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y + 3, LineAlignment.Left, statsColor);

                    //TODO infinite ammo?
                    int ammoBarLength = 10;
                    int ammoBarEntries = Math.Min(player.InventoryQuantityAvailable(weapon.GetType()), 10);
                    //int ammoBarEntries = (int)Math.Ceiling(ammoBarLength * weaponAmmoRatio);

                    for (int i = 0; i < ammoBarLength; i++)
                    {
                        if (i < ammoBarEntries)
                        {
                            PutChar(statsDisplayTopLeft.x + weaponOffset.x + 5 + i, statsDisplayTopLeft.y + weaponOffset.y + 3, weapon.Representation, orangeActivatedColor);
                        }
                        else
                        {
                            PutChar(statsDisplayTopLeft.x + weaponOffset.x + 5 + i, statsDisplayTopLeft.y + weaponOffset.y + 3, weapon.Representation, orangeDisactivatedColor);
                        }
                    }
                }
                
                /*
                //Uses
                int useYOffset = 3;

                string uses = "";
                if (weaponE.HasMeleeAction())
                {
                    PrintLine("Melee", statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y + useYOffset, LineAlignment.Left); 
                }
                else if (weaponE.HasFireAction() && weaponE.HasThrowAction())
                {
                    PrintLine("fire   throw", statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y + useYOffset, LineAlignment.Left);
                    PutChar(statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y + useYOffset, GetCharIconForLetter("F"), ColorPresets.White);
                    PutChar(statsDisplayTopLeft.x + weaponOffset.x + 7, statsDisplayTopLeft.y + weaponOffset.y + useYOffset, GetCharIconForLetter("T"), ColorPresets.White);
                }

                else if (weaponE.HasFireAction())
                {
                    PrintLine("fire", statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y + useYOffset, LineAlignment.Left);
                    PutChar(statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y + useYOffset, GetCharIconForLetter("F"), ColorPresets.White);
                }

                else if (weaponE.HasThrowAction())
                {
                    PrintLine("fire", statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y + useYOffset, LineAlignment.Left);
                    PutChar(statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y + useYOffset, GetCharIconForLetter("T"), ColorPresets.White);
                }*/

                //if (weaponE.HasOperateAction())
                //{
                //    uses += "(u)se";
                //}

               // PrintLine(uses, statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y + 4, LineAlignment.Left);
            }
            else
            {
                var weaponStr = "None";
                PrintLine(weaponStr, statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y, LineAlignment.Left, weaponColor);
            }

            //Draw weapon choices
            var weaponOptionRow = 1;
            var weaponIconXOffset = -3;
            foreach (var kv in ItemMapping.WeaponMapping)
            {
                Type weaponType = Game.Dungeon.Player.HeavyWeaponTranslation(kv.Value);

                DrawWeaponChar(weaponOffset + new Point(weaponIconXOffset + (kv.Key) * 3, weaponOptionRow), kv.Value, kv.Key);
            }
            
            //Draw energy bar and use keys

            int energyBarLength = 20;
            double playerEnergyRatio = player.Energy / (double)player.MaxEnergy;
            int energyBarEntries = (int)Math.Ceiling(energyBarLength * playerEnergyRatio);

            PrintLine("EN: ", statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y + 2, LineAlignment.Left, statsColor);

            DrawEnergyBar(player, utilityOffset + new Point(0, 2), energyBarEntries - 10);
            DrawEnergyBar(player, utilityOffset + new Point(0, 3), Math.Min(energyBarEntries, 10));

            //Enable wetware name
            var equippedWetware = player.GetEquippedWetware();

            if (equippedWetware != null)
            {
                var equippedWetwareItem = (equippedWetware as Item);

                PutChar(statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y, equippedWetwareItem.Representation, weaponColor);

                var wetwareStr = equippedWetwareItem.SingleItemDescription;
                PrintLine(wetwareStr, statsDisplayTopLeft.x + utilityOffset.x + 2, statsDisplayTopLeft.y + utilityOffset.y, LineAlignment.Left, weaponColor);
            }
            else
            {
                PrintLine("None", statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y, LineAlignment.Left, weaponColor);
            }

            //Draw all available wetware
            var wetwareOptionRow = 1;
            int offset = 0;
            foreach (var kv in ItemMapping.WetwareMapping)
            {
                DrawWetwareChar(utilityOffset + new Point(offset * 3, wetwareOptionRow), kv.Value, kv.Key.ToString());
                offset++;
            }

            //

            /*
            //Draw equipped utility

            Item utility = Game.Dungeon.Player.GetEquippedUtilityAsItem();

            string utilityStr = "Utility: ";
            PrintLine(utilityStr, statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y, LineAlignment.Left);

            if (utility != null)
            {
                utilityStr = utility.SingleItemDescription;
                PrintLine(utilityStr, statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y + 1, LineAlignment.Left, utility.GetColour());
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

                PrintLine(uses, statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y + 2, LineAlignment.Left);
            }
             
            else
            {
                utilityStr = "Nothing";
                PrintLine(utilityStr, statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y + 1, LineAlignment.Left, nothingColor);
            }

            //Effect active (add ors)
            if (player.effects.Count > 0)
            {
                PlayerEffect thisEffect = player.effects[0];

                if(thisEffect is PlayerEffectSimpleDuration) {

                    PlayerEffectSimpleDuration durationEffect = thisEffect as PlayerEffectSimpleDuration;

                    string effectName = thisEffect.GetName();
                    int effectRemainingDuration = durationEffect.GetRemainingDuration();
                    int effectTotalDuration = durationEffect.GetDuration();
                    Color effectColor = thisEffect.GetColor();

                    //Effect name

                    PrintLine("Effect: ", statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y + 3, LineAlignment.Left);

                    PrintLine(effectName, statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y + 4, LineAlignment.Left, effectColor);
                    //Duration

                    PrintLine("Tm: ", statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y + 5, LineAlignment.Left);

                    int ammoBarLength = 10;
                    double weaponAmmoRatio = effectRemainingDuration / (double) effectTotalDuration;
                    int ammoBarEntries = (int)Math.Ceiling(ammoBarLength * weaponAmmoRatio);

                    for (int i = 0; i < ammoBarLength; i++)
                    {
                        if (i < ammoBarEntries)
                        {
                            PutChar(statsDisplayTopLeft.x + utilityOffset.x + 5 + i, statsDisplayTopLeft.y + utilityOffset.y + 5, explosionIcon, ColorPresets.Gold);
                        }
                        else
                        {
                            PutChar(statsDisplayTopLeft.x + utilityOffset.x + 5 + i, statsDisplayTopLeft.y + utilityOffset.y + 5, explosionIcon, ColorPresets.Gray);
                        }
                    }
                }

            }*/

            //Draw what we can see
            
            //Creature takes precidence


            //string viewStr = "Target: ";
            //PrintLine(viewStr, statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y, LineAlignment.Left, statsColor);

            if (CreatureToView != null && CreatureToView.Alive == true)
            {

                               //Combat vs player

                var cover = player.GetPlayerCover(CreatureToView);
                if (cover.Item1 > 0)
                {
                    PrintLine("(hard cover)", statsDisplayTopLeft.x + cmbtOffset.x, statsDisplayTopLeft.y + cmbtOffset.y + 3, LineAlignment.Left, ColorPresets.Gold);
                }
                else if (cover.Item2 > 0)
                {
                    PrintLine("(soft cover)", statsDisplayTopLeft.x + cmbtOffset.x, statsDisplayTopLeft.y + cmbtOffset.y + 3, LineAlignment.Left, statsColor);
                }

                //PrintLine("Def: " + player.CalculateDamageModifierForAttacksOnPlayer(CreatureToView), statsDisplayTopLeft.x + cmbtOffset.x, statsDisplayTopLeft.y + cmbtOffset.y + 2, LineAlignment.Left, statsColor);
                //var cover = player.GetPlayerCover(CreatureToView);
                //PrintLine("C: " + cover.Item1 + "/" + cover.Item2, statsDisplayTopLeft.x + cmbtOffset.x, statsDisplayTopLeft.y + cmbtOffset.y + 3, LineAlignment.Left, statsColor);

                //Monster hp

                String nameStr = CreatureToView.SingleDescription;// +"(" + CreatureToView.Representation + ")";
                PrintLine(nameStr, statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 3, LineAlignment.Left, statsColor);


                int mhpBarLength = 10;
                double mplayerHPRatio = CreatureToView.Hitpoints / (double)CreatureToView.MaxHitpoints;
                int mhpBarEntries = (int)Math.Ceiling(mhpBarLength * mplayerHPRatio);

                PrintLine("HP: ", statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 4, LineAlignment.Left, statsColor);

                for (int i = 0; i < mhpBarLength; i++)
                {
                    if (i < mhpBarEntries)
                    {
                        PutChar(statsDisplayTopLeft.x + viewOffset.x + 5 + i, statsDisplayTopLeft.y + viewOffset.y + 4, heartChar, heartColor);
                    }
                    else
                    {
                        PutChar(statsDisplayTopLeft.x + viewOffset.x + 5 + i, statsDisplayTopLeft.y + viewOffset.y + 4, heartChar, disabledColor);
                    }
                }
                

                //Behaviour

                if (CreatureToView.StunnedTurns > 0)
                {
                    PrintLine("(Stunned: " + CreatureToView.StunnedTurns + ")", statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 5, LineAlignment.Left, stunnedBackground);
                }
                else if (CreatureToView.InPursuit())
                {
                    PrintLine("(Hostile)", statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 5, LineAlignment.Left, pursuitBackground);
                }
                else if (!CreatureToView.OnPatrol())
                {
                    PrintLine("(Investigating)", statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 5, LineAlignment.Left, investigateBackground);
                }
                else {
                    PrintLine("(Neutral)", statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 5, LineAlignment.Left, statsColor);
                }
            }
            else if (ItemToView != null)
            {
                String nameStr = ItemToView.SingleItemDescription;// +"(" + ItemToView.Representation + ")";
                PrintLine(nameStr, statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 1, LineAlignment.Left, ItemToView.GetColour());

                IEquippableItem itemE = ItemToView as IEquippableItem;
                if (itemE != null)
                {
                    EquipmentSlot weaponSlot = itemE.EquipmentSlots.Find(x => x == EquipmentSlot.Weapon);
                    if(weaponSlot != null) {
                        PrintLine("(Weapon)", statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 4, LineAlignment.Left);
                    }
                    else
                        PrintLine("(Utility)", statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 4, LineAlignment.Left);
                }
            }
            else
            {
            }

            //Combat stats
                string bonusStr = "";

                if (player.HasMeleeWeaponEquipped())
                {
                    var meleeBonus = player.CalculateMeleeAttackModifiersOnMonster(null);
                    bonusStr = meleeBonus.ToString("#.#") + "x";
                }
                else if (player.HasThrownWeaponEquipped())
                {
                    bonusStr = "";
                }
                else
                {
                    var rangedBonus = player.CalculateRangedAttackModifiersOnMonster(null);
                    bonusStr = rangedBonus.ToString("#.#") + "x";
                }

                PrintLine("Attk: " + bonusStr, statsDisplayTopLeft.x + cmbtOffset.x, statsDisplayTopLeft.y + cmbtOffset.y + 1, LineAlignment.Left, statsColor);
                
                //Defence
                var dodgeBonus = player.CalculateDamageModifierForAttacksOnPlayer(null);

                if (dodgeBonus < 0.71)
                {
                    PrintLine("(s. dodge)", statsDisplayTopLeft.x + cmbtOffset.x, statsDisplayTopLeft.y + cmbtOffset.y + 2, LineAlignment.Left, ColorPresets.Gold);
                }

                else if (dodgeBonus < 0.81)
                {
                    PrintLine("(dodge)", statsDisplayTopLeft.x + cmbtOffset.x, statsDisplayTopLeft.y + cmbtOffset.y + 2, LineAlignment.Left, statsColor);
                }
                
               

            /*
            //Game data
            PrintLine("Droids:", statsDisplayTopLeft.x + gameDataOffset.x, statsDisplayTopLeft.y + gameDataOffset.y, LineAlignment.Left);

            int noDroids = Game.Dungeon.DungeonInfo.MaxDeaths - Game.Dungeon.DungeonInfo.NoDeaths;

            for (int i = 0; i < noDroids; i++)
            {
                PutChar(statsDisplayTopLeft.x + gameDataOffset.x + 8 + i, statsDisplayTopLeft.y + gameDataOffset.y, Game.Dungeon.Player.Representation, Game.Dungeon.Player.RepresentationColor());
            }

            PrintLine("Aborts:", statsDisplayTopLeft.x + gameDataOffset.x, statsDisplayTopLeft.y + gameDataOffset.y + 1, LineAlignment.Left);

            int noAborts = Game.Dungeon.DungeonInfo.MaxAborts - Game.Dungeon.DungeonInfo.NoAborts;

            for (int i = 0; i < noAborts; i++)
            {
                PutChar(statsDisplayTopLeft.x + gameDataOffset.x + 8 + i, statsDisplayTopLeft.y + gameDataOffset.y + 1, 'X',ColorPresets.Red);
            }*/
        }

        private void DrawWetwareChar(Point utilityOffset, Type wetWareType, string wetwareChar)
        {
            var availableWetware = Game.Dungeon.Player.IsWetwareTypeAvailable(wetWareType);
            var equippedWetware = Game.Dungeon.Player.GetEquippedWetware();
            var disabledWetware = Game.Dungeon.Player.IsWetwareTypeDisabled(wetWareType);

            if (availableWetware)
            {
                Color colorToUse;
                if (disabledWetware)
                {
                    colorToUse = disabledColor;
                }
                else if (equippedWetware != null && equippedWetware.GetType() == wetWareType)
                {
                    colorToUse = orangeHighlightedColor;
                }
                else
                {
                    colorToUse = orangeActivatedColor;
                }

                PutChar(statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y, GetCharIconForLetter(wetwareChar), ColorPresets.White);
                //+evil points
                Item instance = (Item)Activator.CreateInstance(wetWareType);
                PutChar(statsDisplayTopLeft.x + utilityOffset.x + 1, statsDisplayTopLeft.y + utilityOffset.y, instance.Representation, colorToUse);
            }
        }

        private void DrawWeaponChar(Point utilityOffset, Type weaponType, int weaponNo)
        {
            var heavyWeaponType = Game.Dungeon.Player.HeavyWeaponTranslation(weaponType);

            var availableWeapon = Game.Dungeon.Player.IsWeaponTypeAvailable(weaponType) || Game.Dungeon.Player.IsWeaponTypeAvailable(heavyWeaponType);
            var equippedWeapon = Game.Dungeon.Player.GetEquippedWeapon();

            var thisWeaponEquipped = equippedWeapon != null && (equippedWeapon.GetType() == weaponType || equippedWeapon.GetType() == heavyWeaponType);

            Color colorToUse;
            if (!availableWeapon)
            {
                colorToUse = disabledColor;
            }
            else if (thisWeaponEquipped)
            {
                colorToUse = orangeHighlightedColor;
            }
            else
            {
                colorToUse = orangeActivatedColor;
            }

            if (weaponNo > 5)
                utilityOffset = utilityOffset + new Point(-15, 2);

            PutChar(statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y, GetCharIconForNumber(weaponNo), ColorPresets.White);
           
            //+evil points
            Item instance = (Item)Activator.CreateInstance(weaponType);
            PutChar(statsDisplayTopLeft.x + utilityOffset.x + 1, statsDisplayTopLeft.y + utilityOffset.y, instance.Representation, colorToUse);
        }

        private char GetCharIconForNumber(int no)
        {
            return (char)(no + 607);
        }

        private char GetCharIconForLetter(string letter)
        {
            switch (letter)
            {
                case "a":
                    return (char)621;
                case "t":
                    return (char)620;
                case "c":
                    return (char)631;
                case "f":
                    return (char)619;
                case "s":
                    return (char)622;
                case "d":
                    return (char)623;
                case "z":
                    return (char)634;
                default:
                    return (char)616;
            }
        }

        private void DrawShieldBar(Player player, Point shieldOffset, int shieldBarFirstBar)
        {
            for (int i = 0; i < shieldBarFirstBar; i++)
            {
                if (i < shieldBarFirstBar)
                {
                    Color shieldColor = player.IsEffectActive(typeof(PlayerEffects.ShieldEnhance)) ? ColorPresets.Yellow : orangeActivatedColor;

                    PutChar(statsDisplayTopLeft.x + shieldOffset.x + 5 + i, statsDisplayTopLeft.y + shieldOffset.y, shieldChar, shieldColor);
                }
                else
                {
                    Color shieldColor = player.ShieldIsDisabled ? orangeActivatedColor : orangeDisactivatedColor;

                    PutChar(statsDisplayTopLeft.x + shieldOffset.x + 5 + i, statsDisplayTopLeft.y + shieldOffset.y, shieldChar, shieldColor);
                }
            }
        }

        private void DrawEnergyBar(Player player, Point shieldOffset, int shieldBarFirstBar)
        {
            for (int i = 0; i < shieldBarFirstBar; i++)
            {
                if (i < shieldBarFirstBar)
                {
                    PutChar(statsDisplayTopLeft.x + shieldOffset.x + 5 + i, statsDisplayTopLeft.y + shieldOffset.y, batteryChar, batteryActivatedColor);
                }
                else
                {
                    PutChar(statsDisplayTopLeft.x + shieldOffset.x + 5 + i, statsDisplayTopLeft.y + shieldOffset.y, batteryChar, disabledColor);
                }
            }
        }

        private void DrawItems(int levelToDraw, List<Item> itemList)
        {

            //Could consider storing here and sorting to give an accurate representation of multiple objects

            foreach (Item item in itemList)
            {
                //Don't draw items on creatures
                if (item.InInventory)
                    continue;

                //Don't draw items on other levels
                if (item.LocationLevel != levelToDraw)
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

                    if (!isViewVisible(item.LocationMap))
                        continue;

                    tileMapLayer(TileLevel.Items)[ViewRelative(item.LocationMap)] = new TileEngine.TileCell(item.Representation);
                    tileMapLayer(TileLevel.Items)[ViewRelative(item.LocationMap)].TileFlag = new LibtcodColorFlags(itemColorToUse);
                }
                //rootConsole.Flush();
                //KeyPress userKey = Keyboard.WaitForKeyPress(true);
            }

        }

        private void DrawLocks(int levelToDraw, Dictionary<Location, List<Lock>> allLocks)
        {
            var locksOnThisLevel = allLocks.Where(kv => kv.Key.Level == levelToDraw).SelectMany(kv => kv.Value);

            foreach (var thisLock in locksOnThisLevel)
            {
                //Colour depending on FOV (for development)
                MapSquare featureSquare = Game.Dungeon.Levels[thisLock.LocationLevel].mapSquares[thisLock.LocationMap.x, thisLock.LocationMap.y];

                Color featureColor = thisLock.RepresentationColor();

                bool drawFeature = true;

                if (featureSquare.InPlayerFOV || SeeAllMap)
                {

                }
                else if (featureSquare.SeenByPlayer)
                {
                    //Not in FOV but seen
                    featureColor = Color.Interpolate(featureColor, ColorPresets.Black, 0.3);
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
                        drawFeature = false;
                    }
                }

                if (drawFeature)
                {
                    if (!isViewVisible(thisLock.LocationMap))
                        continue;

                    tileMapLayer(TileLevel.Features)[ViewRelative(thisLock.LocationMap)] = new TileEngine.TileCell(thisLock.Representation);
                    tileMapLayer(TileLevel.Features)[ViewRelative(thisLock.LocationMap)].TileFlag = new LibtcodColorFlags(featureColor);
                }
            }

        }

        private void DrawFeatures(int levelToDraw, List<Feature> featureList)
        {
            foreach (Feature feature in featureList)
            {
                //Don't draw features on other levels
                if (feature.LocationLevel != levelToDraw)
                    continue;

                //Colour depending on FOV (for development)
                MapSquare featureSquare = Game.Dungeon.Levels[feature.LocationLevel].mapSquares[feature.LocationMap.x, feature.LocationMap.y];

                Color featureColor = feature.RepresentationColor();

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
                    if (!isViewVisible(feature.LocationMap))
                        continue;

                    tileMapLayer(TileLevel.Features)[ViewRelative(feature.LocationMap)] = new TileEngine.TileCell(feature.Representation);
                    tileMapLayer(TileLevel.Features)[ViewRelative(feature.LocationMap)].TileFlag = new LibtcodColorFlags(featureColor, feature.RepresentationBackgroundColor());
                }
            }

        }

        private void DrawCreatures(int levelToDraw, List<Monster> creatureList)
        {
            //Draw stuff about creatures which should be overwritten by other creatures
            foreach (Monster creature in creatureList)
            {
                if (creature.LocationLevel != levelToDraw)
                    continue;

                if (!creature.Alive)
                    continue;

                Color creatureColor = creature.RepresentationColor();

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

                if (creature.ShowHeading() && creature.FOVType() == CreatureFOV.CreatureFOVType.Triangular)
                {
                    List<Point> headingMarkers;

                    if (creature.FOVType() == CreatureFOV.CreatureFOVType.Triangular)
                    {
                        headingMarkers = DirectionUtil.SurroundingPointsFromDirection(creature.Heading, creature.LocationMap, 3);
                    }
                    else
                    {
                        //Base
                        headingMarkers = DirectionUtil.SurroundingPointsFromDirection(creature.Heading, creature.LocationMap, 1);

                        //Reverse first one
                        Point oppositeMarker = new Point(creature.LocationMap.x - (headingMarkers[0].x - creature.LocationMap.x), creature.LocationMap.y - (headingMarkers[0].y - creature.LocationMap.y));
                        headingMarkers.Add(oppositeMarker);
                    }

                    foreach (Point headingLoc in headingMarkers)
                    {
                        //Check heading is drawable

                        Map map = Game.Dungeon.Levels[creature.LocationLevel];

                        //LogFile.Log.LogEntryDebug("heading: " + creature.Representation + " loc: x: " + headingLoc.x.ToString() + " y: " + headingLoc.y.ToString(), LogDebugLevel.Low);

                        if (headingLoc.x >= 0 && headingLoc.x < map.width
                            && headingLoc.y >= 0 && headingLoc.y < map.height)// && Game.Dungeon.MapSquareIsWalkable(creature.LocationLevel, new Point(headingLoc.x, headingLoc.y))
                        {
                            //Draw as a colouring on terrain

                            if (isViewVisible(headingLoc))
                            {
                                int featureChar = tileMapLayer(TileLevel.Features)[ViewRelative(headingLoc)].TileID;

                                if (featureChar != -1)
                                {
                                    char charToOverwrite = (char)featureChar;

                                    tileMapLayer(TileLevel.CreatureDecoration)[ViewRelative(headingLoc)] = new TileEngine.TileCell(charToOverwrite);
                                    tileMapLayer(TileLevel.CreatureDecoration)[ViewRelative(headingLoc)].TileFlag = new LibtcodColorFlags(creature.RepresentationColor());
                                }
                                else
                                {
                                    int terrainChar = tileMapLayer(TileLevel.Terrain)[ViewRelative(headingLoc)].TileID;

                                    if (terrainChar != -1)
                                    {
                                        char charToOverwrite = (char)terrainChar;
                                        //Dot is too hard to see
                                        if (charToOverwrite == StringEquivalent.TerrainChars[MapTerrain.Empty])
                                            charToOverwrite = (char)7;


                                        tileMapLayer(TileLevel.CreatureDecoration)[ViewRelative(headingLoc)] = new TileEngine.TileCell(charToOverwrite);
                                        tileMapLayer(TileLevel.CreatureDecoration)[ViewRelative(headingLoc)].TileFlag = new LibtcodColorFlags(creature.RepresentationColor());
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (Monster creature in creatureList)
            {
                //Not on this level
                if (creature.LocationLevel != levelToDraw)
                    continue;

                if (!creature.Alive)
                    continue;

                //Colour depending on FOV (for development)
                MapSquare creatureSquare = Game.Dungeon.Levels[creature.LocationLevel].mapSquares[creature.LocationMap.x, creature.LocationMap.y];
                Color creatureColor = creature.RepresentationColor();

                Color foregroundColor;
                Color backgroundColor;

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

                            if (isViewVisible(wp))
                            {
                                tileMapLayer(TileLevel.Creatures)[ViewRelative(wp)] = new TileEngine.TileCell(wpNo.ToString()[0]);
                                tileMapLayer(TileLevel.Creatures)[ViewRelative(wp)].TileFlag = new LibtcodColorFlags(monsterWithWP.RepresentationColor());
                            }

                            wpNo++;
                        }
                    }
                }

                if (drawCreature)
                {
                    foregroundColor = creatureColor;
                    backgroundColor = ColorPresets.Black;

                    bool newBackground = false;
                    //Set background depending on status
                    if (creature == CreatureToView)
                    {
                        //targetted
                        backgroundColor = targettedBackground;
                        newBackground = true;
                    }
                    else if (creature.Charmed)
                    {
                        //backgroundColor = charmBackground;
                        newBackground = true;
                    }
                    else if (creature.Passive)
                    {
                        //backgroundColor = passiveBackground;
                        newBackground = true;
                    }
                    else if (creature.StunnedTurns > 0)
                    {
                        //backgroundColor = stunnedBackground;
                        newBackground = true;
                    }

                    if (newBackground == false)
                    {

                        IEquippableItem weapon = Game.Dungeon.Player.GetEquippedWeapon();

                        if (weapon != null)
                        {

                            //In range firing
                            if (weapon.HasFireAction() && Utility.TestRangeFOVForWeapon(Game.Dungeon.Player, creature, weapon.RangeFire(), currentFOV))
                            {
                                //backgroundColor = inRangeBackground;
                                newBackground = true;
                            }
                            else
                            {
                                //In throwing range
                                if (weapon.HasThrowAction() && Utility.TestRangeFOVForWeapon(Game.Dungeon.Player, creature, weapon.RangeFire(), currentFOV))
                                {
                                    //backgroundColor = inRangeBackground;
                                    newBackground = true;
                                }
                            }

                            //Also agressive
                            if (newBackground == true && creature.InPursuit())
                            {
                                //backgroundColor = inRangeAndAggressiveBackground;
                            }
                        }
                    }

                    if (newBackground == false)
                    {
                        if (creature.InPursuit())
                        {
                            //backgroundColor = pursuitBackground;
                            newBackground = true;
                        }
                        else if (!creature.OnPatrol())
                        {
                            //backgroundColor = investigateBackground;
                            newBackground = true;
                        }
                       // else if (creature.Unique)
                            //backgroundColor = uniqueBackground;
                       // else
                           // backgroundColor = normalBackground;
                    }

                    //Creature

                    if (isViewVisible(creature.LocationMap))
                    {
                        tileMapLayer(TileLevel.Creatures)[ViewRelative(creature.LocationMap)] = new TileEngine.TileCell(creature.Representation);
                        tileMapLayer(TileLevel.Creatures)[ViewRelative(creature.LocationMap)].TileFlag = new LibtcodColorFlags(foregroundColor, backgroundColor);
                    }

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



        private void DrawMap(int level, List<Map> maps)
        {
            var map = maps[level];

            //Calculate where to draw the map

            int width = map.width;
            int height = map.height;

            int widthAvail = mapBotRightBase.x - mapTopLeftBase.x;
            int heightAvail = mapBotRightBase.y - mapTopLeftBase.y;

            //Draw frame
            DrawFrame(mapTopLeftBase.x - 1, mapTopLeftBase.y - 1, widthAvail + 3, heightAvail + 3, false, ColorPresets.Khaki);

            //Draw frame for msg here too
            DrawFrame(msgDisplayTopLeft.x - 1, msgDisplayTopLeft.y - 1, msgDisplayBotRight.x - msgDisplayTopLeft.x + 3, msgDisplayBotRight.y - msgDisplayTopLeft.y + 3, false, ColorPresets.MediumSeaGreen);

            //Put the map in the centre
            //mapTopLeft = new Point(mapTopLeftBase.x + (widthAvail - width) / 2, mapTopLeftBase.y + (heightAvail - height) / 2);
            //not appropriate with viewport approach
            mapTopLeft = mapTopLeftBase;

            for (int i = 0; i < map.width; i++)
            {
                for (int j = 0; j < map.height; j++)
                {
                    int screenX = mapTopLeft.x + i;
                    int screenY = mapTopLeft.y + j;

                    char screenChar;
                    Color baseDrawColor;
                    Color drawColor;

                    //Defaults
                    screenChar = StringEquivalent.TerrainChars[map.mapSquares[i, j].Terrain];
                    baseDrawColor = StringEquivalent.TerrainColors[map.mapSquares[i, j].Terrain];

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
                    else if (ShowRoomNumbering > 0 && (map.mapSquares[i, j].Terrain == MapTerrain.Empty || map.mapSquares[i,j].Terrain == MapTerrain.Void))
                    {
                        //Draw room ids as an overlay

                        if ((ShowRoomNumbering == 1 && map.mapSquares[i, j].Terrain == MapTerrain.Empty)
                            || ShowRoomNumbering == 2)
                        {
                            //Draw the room id (in empty areas only for SRN==1)

                            List<Color> colors = new List<Color>(new Color[] { ColorPresets.Yellow, ColorPresets.Gold, ColorPresets.RosyBrown, orangeDisactivatedColor, ColorPresets.LightGray, ColorPresets.Gray });

                            int roomId = map.roomIdMap[i, j];

                            int numberToDraw = roomId % 10;
                            int colorIndex = roomId / 10;

                            if (numberToDraw == -1)
                            {
                                screenChar = 'n';
                                baseDrawColor = ColorPresets.DarkGray;
                            }
                            else {
                                char r = Convert.ToChar(numberToDraw.ToString());
                                screenChar = r;
                                if (colorIndex >= colors.Count)
                                    colorIndex = colorIndex % colors.Count;
                                
                                baseDrawColor = colors[colorIndex];
                            }
                        }
                    }
                    else if (map.mapSquares[i, j].Terrain == MapTerrain.ClosedDoor || map.mapSquares[i, j].Terrain == MapTerrain.OpenDoor)
                    {
                        //Apply colours based on locks
                        screenChar = StringEquivalent.TerrainChars[map.mapSquares[i, j].Terrain];
                        baseDrawColor = StringEquivalent.TerrainColors[map.mapSquares[i, j].Terrain];

                        //Otherwise not locked

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

                    if (Game.Dungeon.Player.IsEffectActive(typeof(PlayerEffects.SeeFOV)) && map.mapSquares[i, j].InMonsterFOV)
                    {
                        drawColor = Color.Interpolate(baseDrawColor, ColorPresets.Green, 0.7);
                    }

                    Point mapTerrainLoc = new Point(i, j);

                    if (isViewVisible(mapTerrainLoc))
                    {
                        tileMapLayer(TileLevel.Terrain)[ViewRelative(mapTerrainLoc)] = new TileEngine.TileCell(screenChar);
                        tileMapLayer(TileLevel.Terrain)[ViewRelative(mapTerrainLoc)].TileFlag = new LibtcodColorFlags(drawColor);
                    }
                }
            }

        }

        /// <summary>
        /// Returns the requested tile layer for the master map
        /// </summary>
        /// <param name="levelId"></param>
        /// <returns></returns>
        internal TileEngine.TileLayer tileMapLayer(TileLevel levelId)
        {
            return tileMap.Layer[(int)levelId];
        }

        internal void ConsoleLine(string datedEntry)
        {
            Console.WriteLine(datedEntry);
        }

        internal void ClearMessageLine()
        {
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
            //Update state
            lastMessage = message;

            //Display new message
            PrintLineRect(message, msgDisplayTopLeft.x, msgDisplayTopLeft.y, msgDisplayBotRight.x - msgDisplayTopLeft.x + 1, msgDisplayNumLines, LineAlignment.Left, color);
        }

        /// <summary>
        /// Print message at any point on screen
        /// </summary>
        /// <param name="message"></param>
        internal void PrintMessage(string message, Point topLeft, int width)
        {

            //Update state
            lastMessage = message;

            //Clear message bar
            DrawRect(topLeft.x, topLeft.y, width, 1, true);

            //Display new message
            PrintLineRect(message, topLeft.x, topLeft.y, width, 1, LineAlignment.Left);
        }

        void ClearMessageBar()
        {

            DrawRect(msgDisplayTopLeft.x, msgDisplayTopLeft.y, msgDisplayBotRight.x - msgDisplayTopLeft.x - 1, msgDisplayBotRight.y - msgDisplayTopLeft.y - 1, true);
        }


        /// <summary>
        /// Get a string from the user. Uses the message bar
        /// </summary>
        /// <returns></returns>
       
        internal string GetUserString(string introMessage)
        {

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

        public bool YesNoQuestionWithFrame(string introMessage, int extrayOffset = 0)
        {
            var width = introMessage.Count() + 7;
            var height = 1;

            int xOffset = (Width - movieTL.x * 2 - width) / 2;
            int yOffset = (Height - movieTL.y * 2 - height) / 2;

            var frameTL = new Point(movieTL.x + xOffset, movieTL.y + yOffset + extrayOffset);
            int frameOffset = 2;

            //Draw frame
            DrawFrame(frameTL.x - frameOffset, frameTL.y - frameOffset, width + 2 * frameOffset + 1, height + 2 * frameOffset, true);

            FlushConsole();

            return YesNoQuestion(introMessage, new Point(frameTL.x, frameTL.y));
        }

        public GameDifficulty DifficultyQuestionWithFrame(string introMessage, int extrayOffset = 0)
        {
            var width = introMessage.Count() + 7;
            var height = 1;

            int xOffset = (Width - movieTL.x * 2 - width) / 2;
            int yOffset = (Height - movieTL.y * 2 - height) / 2;

            var frameTL = new Point(movieTL.x + xOffset, movieTL.y + yOffset + extrayOffset);
            int frameOffset = 2;

            //Draw frame
            DrawFrame(frameTL.x - frameOffset, frameTL.y - frameOffset, width + 2 * frameOffset + 1, height + 2 * frameOffset, true);

            FlushConsole();

            return DifficultyQuestion(introMessage, new Point(frameTL.x, frameTL.y));
        }

        internal bool YesNoQuestion(string introMessage, Point topLeft)
        {
            
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

        internal GameDifficulty DifficultyQuestion(string introMessage, Point topLeft) {
            
            PrintMessage(introMessage, topLeft, introMessage.Length + 8);
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
            } while (true);

            return GameDifficulty.Easy;
        }

        internal bool YesNoQuestion(string introMessage)
        {

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
            
            //Draw the screen as normal
            Draw();
            FlushConsole();

            //Flash the attacker
            /*
            if (creatureSquare.InPlayerFOV)
            {
                rootConsole.ForegroundColor = ColorPresets.White;
                rootConsole.PutChar(mapTopLeft.x + originCreature.LocationMap.x, mapTopLeft.y + originCreature.LocationMap.y, originCreature.Representation);
            }*/

            //Draw animation to animation layer

            //Calculate and draw the line overlay
            DrawPathLine(TileLevel.Animations, originCreature.LocationMap, target.LocationMap, color, ColorPresets.Black);

            //Flash the target if they were damaged
            //Draw them in either case so that we overwrite the missile animation on the target square with the creature

            if (targetSquare.InPlayerFOV)
            {
                Color colorToDraw = ColorPresets.Red;

                if (result == CombatResults.DefenderDamaged || result == CombatResults.DefenderDied)
                {
                    
                }
                else
                {
                    colorToDraw = target.RepresentationColor();
                }

                if (isViewVisible(target.LocationMap))
                {
                    tileMapLayer(TileLevel.Animations)[ViewRelative(target.LocationMap)] = new TileEngine.TileCell(target.Representation);
                    tileMapLayer(TileLevel.Animations)[ViewRelative(target.LocationMap)].TileFlag = new LibtcodColorFlags(colorToDraw);
                }
            }

            //Render the full layered map (with these animations) on screen
            MapRendererLibTCod.RenderMap(tileMap, new Point(0, 0), new System.Drawing.Rectangle(mapTopLeft.x, mapTopLeft.y, mapBotRightBase.x - mapTopLeftBase.x + 1, mapBotRightBase.y - mapTopLeftBase.y + 1));
            FlushConsole();

            //Wait
            TCODSystem.Sleep(missileDelay);

            //Wipe the animation layer
            tileMap.ClearLayer((int)TileLevel.Animations);

            //Draw the map normally
            MapRendererLibTCod.RenderMap(tileMap, new Point(0, 0), new System.Drawing.Rectangle(mapTopLeft.x, mapTopLeft.y, mapBotRightBase.x - mapTopLeftBase.x + 1, mapBotRightBase.y - mapTopLeftBase.y + 1));
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

            //Flash the attacked creature
            //Add flash to animation layer

            if (targetSquare.InPlayerFOV)
            {
                if (result == CombatResults.DefenderDamaged || result == CombatResults.DefenderDied)
                {
                    if (isViewVisible(newTarget.LocationMap))
                    {
                        tileMapLayer(TileLevel.Animations)[ViewRelative(newTarget.LocationMap)] = new TileEngine.TileCell(explosionIcon);
                        tileMapLayer(TileLevel.Animations)[ViewRelative(newTarget.LocationMap)].TileFlag = new LibtcodColorFlags(ColorPresets.Red);
                    }
                }
            }

            //Render the full layered map (with these animations) on screen
            MapRendererLibTCod.RenderMap(tileMap, new Point(0, 0), new System.Drawing.Rectangle(mapTopLeft.x, mapTopLeft.y, mapBotRightBase.x - mapTopLeftBase.x + 1, mapBotRightBase.y - mapTopLeftBase.y + 1));
            FlushConsole();

            //Wait
            TCODSystem.Sleep(meleeDelay);

            //Wipe the animation layer
            tileMap.ClearLayer((int)TileLevel.Animations);
                        
            //Draw the map normally
            MapRendererLibTCod.RenderMap(tileMap, new Point(0, 0), new System.Drawing.Rectangle(mapTopLeft.x, mapTopLeft.y, mapBotRightBase.x - mapTopLeftBase.x + 1, mapBotRightBase.y - mapTopLeftBase.y + 1));
            FlushConsole();
        }

    }
}
