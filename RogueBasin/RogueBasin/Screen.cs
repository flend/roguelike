using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;
using Console = System.Console;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using System.Linq;
using System.Drawing;
using RogueBasin.LibTCOD;

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
            TerrainEffects = 1,
            Features = 2,
            Items = 3,
            Creatures = 4,
            CreatureDecoration = 5,
            CreatureStatus = 6,
            CreatureCover = 7,
            CreatureTarget = 8,
            CreatureLevel = 9,
            TargettingUI = 10,
            Animations = 11
        }

        static Screen instance = null;

        IMapRenderer mapRenderer;
        public bool NeedsUpdate { get; set; }

        //A prompt for the user to respond to
        string Prompt { get; set; }

        //Screen size
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }

        public int ScaledSpriteSize { get; set; }

        //UI size
        public double UIScaling { get; set; }

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

        //Viewport in tile coordinates
        Point mapTopLeftBase;
        Point mapBotRightBase;

        /// <summary>
        /// Dimensions of message display area
        /// </summary>
        Point msgDisplayTopLeft;
        Point msgDisplayBotRight;
        public int msgDisplayNumLines;

        Point hitpointsOffset;     

        System.Drawing.Color inFOVTerrainColor = System.Drawing.Color.White;
        System.Drawing.Color seenNotInFOVTerrainColor = System.Drawing.Color.Gray;
        System.Drawing.Color neverSeenFOVTerrainColor;
        System.Drawing.Color inMonsterFOVTerrainColor = System.Drawing.Color.Blue;

        System.Drawing.Color statsColor = System.Drawing.Color.FromArgb(255, 108, 215, 224);
        System.Drawing.Color nothingColor = System.Drawing.Color.Gray;

        System.Drawing.Color creatureColor = System.Drawing.Color.White;
        System.Drawing.Color itemColor = System.Drawing.Color.Red;
        System.Drawing.Color featureColor = System.Drawing.Color.White;

        System.Drawing.Color hiddenColor = System.Drawing.Color.Black;

        System.Drawing.Color charmBackground = System.Drawing.Color.DarkKhaki;
        System.Drawing.Color passiveBackground = System.Drawing.Color.DarkMagenta;
        System.Drawing.Color uniqueBackground = System.Drawing.Color.DarkCyan;
        System.Drawing.Color inRangeBackground = System.Drawing.Color.DeepSkyBlue;
        System.Drawing.Color inRangeAndAggressiveBackground = System.Drawing.Color.Purple;
        System.Drawing.Color stunnedBackground = System.Drawing.Color.DarkCyan;
        System.Drawing.Color investigateBackground = System.Drawing.Color.DarkGreen;
        System.Drawing.Color pursuitBackground = System.Drawing.Color.DarkRed;
        System.Drawing.Color normalBackground = System.Drawing.Color.Black;
        System.Drawing.Color normalForeground = System.Drawing.Color.White;
        System.Drawing.Color targettedBackground = System.Drawing.Color.DarkSlateGray;

        System.Drawing.Color statsFrameColor = System.Drawing.Color.MediumSeaGreen;
        System.Drawing.Color mapFrameColor = System.Drawing.Color.Khaki;

        System.Drawing.Color targetBackground = System.Drawing.Color.White;
        System.Drawing.Color targetForeground = System.Drawing.Color.Black;

        System.Drawing.Color literalColor = System.Drawing.Color.BurlyWood;
        System.Drawing.Color literalTextColor = System.Drawing.Color.White;

        System.Drawing.Color headingColor = System.Drawing.Color.Yellow;

        System.Drawing.Color messageColor = System.Drawing.Color.CadetBlue;
        System.Drawing.Color titleColor = System.Drawing.Color.CadetBlue;

        System.Drawing.Color soundColor = System.Drawing.Color.Yellow;

        System.Drawing.Color normalMovieColor = System.Drawing.Color.MediumSeaGreen;
        System.Drawing.Color flashMovieColor = System.Drawing.Color.Red;

        System.Drawing.Color promptColor = System.Drawing.Color.Orange;

        int movieFrameWidth = 4;

        const char heartChar = (char)567;
        const char shieldChar = (char)561;
        const char ammoChar = (char)568;
        const char grenadeChar = (char)297;
        const char batteryChar = (char)308;

        System.Drawing.Color orangeActivatedColor = System.Drawing.Color.DarkOrange;
        System.Drawing.Color batteryActivatedColor = System.Drawing.Color.SlateBlue;
        System.Drawing.Color orangeHighlightedColor = System.Drawing.Color.Gold;
        System.Drawing.Color orangeDisactivatedColor = System.Drawing.Color.SaddleBrown;
        System.Drawing.Color disabledColor = System.Drawing.Color.DimGray;
        System.Drawing.Color weaponColor = System.Drawing.Color.LightSteelBlue;
        System.Drawing.Color heartColor = System.Drawing.Color.Crimson;
        
        //Keep enough state so that we can draw each screen
        string lastMessage = "";

        //For examining
        public Monster CreatureToView { get; set; }
        public Item ItemToView { get; set; }
        public Feature FeatureToView { get; set; }

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

        public static int combationAnimationFrameDuration = 300; //ms
        public static int combatFastAnimationFrameDuration = 150;

        int smallTextSize = 12;
        int largeTextSize = 22;
        int largeTextSizeLineOffset = 11;

        /// <summary>
        /// Targetting mode
        /// </summary>
        bool targettingMode = false;

        /// <summary>
        /// Targetting cursor
        /// </summary>
        public Point Target { get; set; }

        public TargettingType TargetType { get; set; }
        public TargettingAction TargetAction { get; set; }

        public int TargetRange { get; set; }
        public double TargetPermissiveAngle { get; set; }

        public System.Drawing.Color PCColor { get; set;}

        public bool SeeAllMonsters { get; set; }
        public bool SeeAllMap { get; set; }

        public int ViewportScrollSpeed { get; set; }

        public uint MessageQueueWidth { get; private set; }

        public bool ExtraUI { get; set; }

        public static Screen Instance
        {
            get
            {
                if (instance == null)
                {
                    //instance = new Screen(new MapRendererLibTCod());
                    instance = new Screen(new MapRendererSDLDotNet());
                }
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

        static readonly string blueTargetTile = "bluetarget";
        static readonly string greenTargetTile = "greentarget";
        static readonly string redTargetTile = "redtarget";
        static readonly string blackTargetTile = "blacktarget";

        public int LevelToDisplay
        {
            get; set;
        }

        Screen(IMapRenderer renderer)
        {
            mapRenderer = renderer;

            ScreenWidth = 1285;
            ScreenHeight = 965;

            int nativeSpriteDim = 64;
            
            //Try these

            //int scaledSpriteDim = 64;
            int scaledSpriteDim = 32; 
            //int scaledSpriteDim = 128;
            //int scaledSpriteDim = 8;  //this appears to fail spectacularly but isn't very useful anyway
            

            //These control the map 
            ViewableWidth = ScreenWidth / scaledSpriteDim;
            ViewableHeight = ScreenHeight / scaledSpriteDim;

            ScaledSpriteSize = scaledSpriteDim;

            if (nativeSpriteDim != scaledSpriteDim)
            {
                mapRenderer.SetSpriteVideoSize(scaledSpriteDim, scaledSpriteDim);
            }

            UIScaling = 1;

            ViewportScrollSpeed = 1;

            viewTL = new Point(0, 0);
            SetViewBRFromTL();

            LevelToDisplay = 0;

            DebugMode = false;
            CombatAnimations = true;

            msgDisplayTopLeft = new Point(50, 50);
            msgDisplayBotRight = new Point(850, 100);

            MessageQueueWidth = (uint)(msgDisplayBotRight.y - msgDisplayBotRight.x);

            msgDisplayNumLines = 3;

            mapTopLeftBase = new Point(0, 0);
            mapBotRightBase = new Point(ViewableWidth, ViewableHeight);

            MsgLogWrapWidth = 80;

            //Colors
            neverSeenFOVTerrainColor = System.Drawing.Color.Gray;// Color.FromRGB(90, 90, 90);

            TotalKills = null;

            PCColor = System.Drawing.Color.White;

            SeeAllMonsters = false;
            SeeAllMap = false;

            NeedsUpdate = true;

            ExtraUI = true;
        }

        //Setup the screen
        public void InitialSetup()
        {
            mapRenderer.Setup(ScreenWidth, ScreenHeight);
        }

        public void ShowMessageLine(string msg, System.Drawing.Color color)
        {
            DrawTextWidth(msg, msgDisplayTopLeft, msgDisplayBotRight.x - msgDisplayTopLeft.x, color);
        }

        public void ShowMessageLine(string msg)
        {
            DrawTextWidth(msg, msgDisplayTopLeft, msgDisplayBotRight.x - msgDisplayTopLeft.x, messageColor);
        }

        public bool TargetSelected()
        {
            if (CreatureToView != null)
            {
                if (!Screen.Instance.CreatureToView.Alive)
                    return false;
                return true;
            }

            return ItemToView != null;
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
        protected void DrawPathLine(TileLevel layerNo, Point start, Point end, System.Drawing.Color foregroundColor, System.Drawing.Color backgroundColor)
        {
            DrawPathLine(layerNo, start, end, foregroundColor, backgroundColor, (char)0);
        }

        /// <summary>
        /// Draw a line following a path on a tile layer. Override default line drawing
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="color"></param>
        protected void DrawPathLine(TileLevel layerNo, Point start, Point end, System.Drawing.Color foregroundColor, System.Drawing.Color backgroundColor, char drawChar)
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
                if (layerNo == TileLevel.Animations)
                {
                    tileMapLayer(layerNo)[ViewRelative(p)].Animation = new TileEngine.Animation(combationAnimationFrameDuration);
                }
            }           
        }



        /// <summary>
        /// Call after all drawing is complete to output onto screen
        /// </summary>
        public void FlushConsole()
        {
            mapRenderer.Flush();
        }

        /// <summary>
        /// Scroll the viewport, by the delta
        /// </summary>
        /// <param name="delta"></param>
        public void ScrollViewport(Point delta) {

            viewTL += delta * ViewportScrollSpeed;

            RestrictViewpointToMap();
        }

        private void RestrictViewpointToMap()
        {
            Map mapToDisplay = Game.Dungeon.Levels[LevelToDisplay];

            var minX = 1 - ViewableWidth;

            if (viewTL.x < minX)
                viewTL = new Point(minX, viewTL.y);

            var minY = 1 - ViewableHeight;

            if (viewTL.y < minY)
                viewTL = new Point(viewTL.x, minY);

            //Player will centre screen, so this can't just be width - ViewableWidth
            var maxX = mapToDisplay.width - 1;

            if (viewTL.x > maxX)
                viewTL = new Point(maxX, viewTL.y);

            var maxY = mapToDisplay.height - 1;

            if (viewTL.y > maxY)
                viewTL = new Point(viewTL.x, maxY);
        }


        public void TargettingModeOn() {
            targettingMode = true;
        }

        public void TargettingModeOff()
        {
            targettingMode = false;
        }


        List<Movie> moviesToPlay = new List<Movie>();

        public void EnqueueMovie(string filenameRoot)
        {
            try
            {
                moviesToPlay.Add(LoadMovie(filenameRoot));
            }
            catch (Exception ex)
            {
                LogFile.Log.LogEntry("Failed to load movie " + filenameRoot);
            }
        }

        public void EnqueueMovie(Movie movie)
        {
            moviesToPlay.Add(movie);
        }

        public void DequeueFirstMovie()
        {
            if (moviesToPlay.Count == 0)
                return;

            moviesToPlay.RemoveAt(0);
        }

        public bool MoviesToPlay()
        {
            return moviesToPlay.Count > 0;
        }

        private void PlayFirstMovieInQueue() {

            if (moviesToPlay.Count == 0)
            {
                LogFile.Log.LogEntryDebug("No movies in queue", LogDebugLevel.High);
                return;
            }
                
            RenderMovie(moviesToPlay[0]);
        }

        private void RenderMovie(Movie movie)
        {
            int frameNo = 0;

            int frameBufferX = ScreenWidth / 8;
            int frameBufferY = ScreenHeight / 8;

            //Calculate dimensions
            int maxWidth = 0;
            int totalHeight = 0;

            int textHeight = 0;
            int textHeightWithSpacing = 0;

            foreach (MovieFrame frame in movie.Frames)
            {
                foreach (String scanLine in frame.ScanLines)
                {
                    Size textSize = mapRenderer.TextSize(scanLine, largeTextSize);
                    textHeight = textSize.Height;
                    textHeightWithSpacing = textHeight + largeTextSizeLineOffset;

                    if (textSize.Width > maxWidth)
                    {
                        maxWidth = textSize.Width;
                    }

                    totalHeight += textHeightWithSpacing;
                }
            }

            int bufferedWidth = maxWidth + frameBufferX;
            int bufferedHeight = totalHeight + frameBufferY;

            int widthOffset = Math.Max(0, (ScreenWidth - bufferedWidth) / 2);
            int heightOffset = Math.Max(0, (ScreenHeight - bufferedHeight) / 2);

            Point frameTL = new Point(widthOffset, heightOffset);
            Point movieTL = new Point(widthOffset + frameBufferX / 2, heightOffset + frameBufferY / 2);

            //Draw each frame of the movie
            foreach (MovieFrame frame in movie.Frames)
            {
                //Draw frame
                DrawFramePixel(frameTL.x, frameTL.y, bufferedWidth, bufferedHeight, true, System.Drawing.Color.White, movieFrameWidth);

                //Draw content
                List<string> scanLines = frame.ScanLines;
                DrawMovieFrame(frame.ScanLines, movieTL, maxWidth, textHeightWithSpacing);

                DrawText("Press ENTER to continue", new Point(frameTL.x + bufferedWidth / 2, frameTL.y + bufferedHeight - textHeightWithSpacing), LineAlignment.Center, titleColor);

                Screen.Instance.FlushConsole();
                frameNo++;
                
                //Multi-frame unsupported for now, enqueue multiple movies instead
                break;
            }
        }

        public void DrawFramePixel(int x, int y, int width, int height, bool clear, System.Drawing.Color color, int lineWidth)
        {
            if (clear)
            {
                mapRenderer.DrawRectangle(new Rectangle(x, y, width, height), System.Drawing.Color.Black);
            }

            for (int j = 0; j < lineWidth; j++) {
                mapRenderer.DrawLine(new Point(x, y + j), new Point(x + width, y + j), color);
                mapRenderer.DrawLine(new Point(x + width - j, y), new Point(x + width - j, y + height), color);
                mapRenderer.DrawLine(new Point(x + j, y), new Point(x + j, y + height), color);
                mapRenderer.DrawLine(new Point(x, y + height - j), new Point(x + width, y + height - j), color);
            }
        }


        /// <summary>
        /// Draw a frame. If flashOn then highlight flashing squares in red
        /// </summary>
        /// <param name="scanLines"></param>
        /// <param name="frameTL"></param>
        /// <param name="width"></param>
        /// <param name="flashOn"></param>
        private void DrawMovieFrame(List<string> scanLines, Point frameTL, int width, int lineOffset)
        {
            int offset = 0;

            foreach (string line in scanLines)
            {
                DrawText(line, new Point(frameTL.x + width / 2, frameTL.y + offset * lineOffset), LineAlignment.Center, normalMovieColor);
                offset++;
            }
        }

        public Movie LoadMovie(string filenameRoot)
        {

            LogFile.Log.LogEntry("Loading movie: " + filenameRoot);

            int frameNo = 0;

            var thisMovieFrames = new List<MovieFrame>();

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

                    frame.ScanLines = new List<string>();

                    while ((thisLine = reader.ReadLine()) != null)
                    {
                        frame.ScanLines.Add(thisLine);
                    }

                    //Add the frame
                    thisMovieFrames.Add(frame);

                    //Increment the frame no
                    frameNo++;
                }
            } while (true);

            return new Movie(thisMovieFrames);
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

        private System.Drawing.Color ColorInterpolate(System.Drawing.Color from, System.Drawing.Color to, double degree)
        {
            libtcodWrapper.Color interpolated = libtcodWrapper.Color.Interpolate(libtcodWrapper.Color.FromRGB(from.R, from.G, from.B), libtcodWrapper.Color.FromRGB(to.R, to.G, to.B), degree);
            return System.Drawing.Color.FromArgb(interpolated.Red, interpolated.Blue, interpolated.Green);
        }

        private void DrawPC(int levelToDraw, Player player)
        {
            if (player.LocationLevel != levelToDraw)
                return;
         
            Point PClocation = player.LocationMap;
            System.Drawing.Color PCDrawColor = PCColor;

            if (DebugMode)
            {
                MapSquare pcSquare = Game.Dungeon.Levels[player.LocationLevel].mapSquares[player.LocationMap.x, player.LocationMap.y];

                if (pcSquare.InMonsterFOV)
                {
                    PCDrawColor = ColorInterpolate(PCDrawColor, System.Drawing.Color.Red, 0.4);
                }
            }

            if (!isViewVisible(PClocation))
                return;

            char pcRepresentation = player.Representation;
            
            var colorToUse = PCDrawColor;

            var hasActiveWetware = player.GetEquippedWetware();

            if (hasActiveWetware != null)
            {
                colorToUse = System.Drawing.Color.LightSkyBlue;

                if (hasActiveWetware.GetType() == typeof(Items.StealthWare))
                    pcRepresentation = (char)256;
            }

            tileMapLayer(TileLevel.Creatures)[ViewRelative(PClocation)] = new TileEngine.TileCell(pcRepresentation);
            tileMapLayer(TileLevel.Creatures)[ViewRelative(PClocation)].TileFlag = new LibtcodColorFlags(colorToUse);
            tileMapLayer(TileLevel.Creatures)[ViewRelative(PClocation)].TileSprite = player.GameSprite;

            tileMapLayer(TileLevel.CreatureLevel)[ViewRelative(player.LocationMap)] = new TileEngine.TileCell("monster_level_" + player.Level);

            //Draw equipped weapons

            //Draw equipped ranged weapon
            Item weapon = player.GetEquippedRangedWeaponAsItem();
            
            //Draw equipped melee weapon
            Item meleeWeapon = player.GetEquippedMeleeWeaponAsItem();

            if(weapon != null && !(weapon is Items.Pistol)) {
                tileMapLayer(TileLevel.CreatureDecoration)[ViewRelative(PClocation)].TileSprite = weapon.GameSprite;
            }
            else if (meleeWeapon != null)
            {
                tileMapLayer(TileLevel.CreatureDecoration)[ViewRelative(PClocation)].TileSprite = meleeWeapon.GameSprite;
            }

            if (player.IsDodgeActive())
            {
                tileMapLayer(TileLevel.CreatureStatus)[ViewRelative(PClocation)].TileSprite = "running";
            }

            if (player.IsAimActive())
            {
                tileMapLayer(TileLevel.CreatureStatus)[ViewRelative(PClocation)].TileSprite = "aiming";
            }

            if (player.IsEffectActive(typeof(PlayerEffects.StealthBoost)))
            {
                tileMapLayer(TileLevel.CreatureStatus)[ViewRelative(PClocation)].TileSprite = "stealth";
            }
        }


        /// <summary>
        /// Fully rebuild the layered, tiled map. All levels, excluding animations
        /// </summary>
        private void BuildTiledMap()
        {
            Dungeon dungeon = Game.Dungeon;
            Player player = dungeon.Player;

            if(tileMap == null)
                tileMap = new TileEngine.TileMap((int)TileLevel.Animations + 1, ViewableHeight, ViewableWidth);

            tileMap.ClearLayer(TileLevel.Terrain);
            tileMap.ClearLayer(TileLevel.TerrainEffects);
            tileMap.ClearLayer(TileLevel.Features);
            tileMap.ClearLayer(TileLevel.Creatures);
            tileMap.ClearLayer(TileLevel.CreatureDecoration);
            tileMap.ClearLayer(TileLevel.CreatureStatus);
            tileMap.ClearLayer(TileLevel.CreatureCover);
            tileMap.ClearLayer(TileLevel.CreatureTarget);
            tileMap.ClearLayer(TileLevel.CreatureLevel);
            tileMap.ClearLayer(TileLevel.Items);
            tileMap.ClearLayer(TileLevel.TargettingUI);

            //Don't clear the animations layer

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

            if (Game.Base.GameStarted)
            {
                BuildTiledMap();

                //Render tiled map to screen
                mapRenderer.RenderMap(tileMap, new Point(0, 0), new System.Drawing.Rectangle(mapTopLeftBase.x, mapTopLeftBase.y, mapBotRightBase.x - mapTopLeftBase.x + 1, mapBotRightBase.y - mapTopLeftBase.y + 1));

                DrawUI();
            }

            if(MoviesToPlay())
                PlayFirstMovieInQueue();

            if (SpecialScreen != null)
            {
                SpecialScreen();
            }

            //Prompt for user
            if (Prompt != null)
            {
                DrawPrompt();
            }
            else if (ShowMessageQueue && SpecialScreen == null) {
                Game.MessageQueue.RunMessageQueue();
            }
            NeedsUpdate = false;

        }


        public Action SpecialScreen {
            get; set;
        }

        private System.Drawing.Color textColor = System.Drawing.Color.Khaki;

        public void CharacterSelectionScreen()
        {
            DrawFramePixel(0, 0, ScreenWidth, ScreenHeight, true, System.Drawing.Color.Black, 1);
            DrawFramePixel(ScreenWidth / 8, ScreenHeight / 8, 6 * ScreenWidth / 8, 6 * ScreenHeight / 8, true, System.Drawing.Color.Blue, movieFrameWidth);

            var titleColor = System.Drawing.Color.Khaki;

            var centreXOffset = ScreenWidth / 4;
            var centreYOffset = ScreenHeight / 4;

            var topY = centreYOffset / 2;
            var graphicsY = centreYOffset * 2 - centreYOffset / 2;

            var centreX = ScreenWidth / 2;

            var lanceCentre = new Point(centreXOffset, graphicsY);
            var crackCentre = new Point(centreXOffset * 2, graphicsY);
            var nerdCentre = new Point(centreXOffset * 3, graphicsY);

            var titleLineOffset = ScreenHeight / 32;

            DrawLargeText("Congratulations!", new Point(centreX, topY), LineAlignment.Center, statsColor);
            DrawLargeText("You VOLUNTEERed for the RoyaLe!", new Point(centreX, topY + titleLineOffset), LineAlignment.Center, textColor);
            DrawLargeText("VERY LITTLE (6 arenas)", new Point(centreX, topY + 2 * titleLineOffset), LineAlignment.Center, textColor);
            DrawLargeText("now stands between you and victory!", new Point(centreX, topY + 3 * titleLineOffset), LineAlignment.Center, textColor);

            DrawLargeText("State your name and history:", new Point(centreX, topY + titleLineOffset * 5), LineAlignment.Center, textColor);
            
            var characterOffset = ScreenHeight / 19;

            DrawText("[1] Lance", lanceCentre - new Point(0, characterOffset), LineAlignment.Center, statsColor);
            DrawSpriteByCentre("lance", lanceCentre);

            DrawText("[2] Crack", crackCentre - new Point(0, characterOffset), LineAlignment.Center, statsColor);
            DrawSpriteByCentre("crack", crackCentre);

            DrawText("[3] N3rd", nerdCentre - new Point(0, characterOffset), LineAlignment.Center, statsColor);
            DrawSpriteByCentre("nerd", nerdCentre);

            var textY = ScreenHeight / 11;
            var textWidth = (int)Math.Floor(ScreenWidth / 5.0);
            var textXOffset = (int)Math.Floor(-ScreenWidth / 10.0);

            var lanceText = "A disgraced athlete who was dismissed for his violent conduct in track and field. Ideal for the Arena.";
            DrawTextWidth(lanceText, lanceCentre + new Point(textXOffset, textY), textWidth, statsColor);

            var crackText = "In 2072 a commando was sent to prison by a military court for a crime he absolutely did commit. Now fighting for the pleasure of TV viewers everywhere!";
            DrawTextWidth(crackText, crackCentre + new Point(textXOffset, textY), textWidth, statsColor);

            var nerdText = "Having finally won GrandMaster league in popular video game 'Running Hunger Royale' the nerd was completely unprepared for his 'prize'!";
            DrawTextWidth(nerdText, nerdCentre + new Point(textXOffset, textY), textWidth, statsColor);

            var specialY = (int)Math.Floor(ScreenHeight / 3.2);

            var lanceSpecial = "Special: Keep moving for a melee and defence bonus!";
            DrawTextWidth(lanceSpecial, lanceCentre + new Point(textXOffset, specialY), textWidth, textColor);

            var crackSpecial = "Special: Halt in place for a ranged and defence bonus!";
            DrawTextWidth(crackSpecial, crackCentre + new Point(textXOffset, specialY), textWidth, textColor);

            var nerdSpecial = "Special: Sneak around and cause mayhem!";
            DrawTextWidth(nerdSpecial, nerdCentre + new Point(textXOffset, specialY), textWidth, textColor);

            var styleY = (int)Math.Floor(ScreenHeight / 2.1);

            if (Game.Dungeon.FunMode)
            {
                var funText1 = "FUN MODE!: Life's a laugh and death's a joke, it's true.";
                var funText2 = "[R]: For roguelike mode!";

                DrawText(funText1, new Point(centreX, graphicsY + styleY), LineAlignment.Center, statsColor);
                DrawText(funText2, new Point(centreX, graphicsY + styleY + titleLineOffset), LineAlignment.Center, textColor);
            }
            else
            {
                var funText1 = "ROGUELIKE MODE!: I learnt to Crawl before I could walk.";
                var funText2 = "[F]: For FUN mode!";

                DrawText(funText1, new Point(centreX, graphicsY + styleY), LineAlignment.Center, statsColor);
                DrawText(funText2, new Point(centreX, graphicsY + styleY + titleLineOffset), LineAlignment.Center, textColor);
            }
        }

        public void FunModeDeathScreen()
        {
            DrawFramePixel(0, 0, ScreenWidth, ScreenHeight, true, System.Drawing.Color.Black, 1);
            DrawFramePixel(ScreenWidth / 8, ScreenHeight / 8, 6 * ScreenWidth / 8, 6 * ScreenHeight / 8, true, System.Drawing.Color.Blue, movieFrameWidth);

            var titleColor = System.Drawing.Color.Khaki;

            var centreXOffset = ScreenWidth / 4;
            var centreYOffset = ScreenHeight / 4;

            var topY = centreYOffset / 2;
            var graphicsY = centreYOffset * 2 - centreYOffset / 2;

            var centreX = ScreenWidth / 2;

            var lanceCentre = new Point(centreXOffset, graphicsY);
            var crackCentre = new Point(centreXOffset * 2, graphicsY);
            var nerdCentre = new Point(centreXOffset * 3, graphicsY);

            var titleLineOffset = 30;

            DrawLargeText("Congratulations!", new Point(centreX, topY), LineAlignment.Center, statsColor);
            DrawLargeText("You DIED in the service of PRIME TIME TV!", new Point(centreX, topY + titleLineOffset), LineAlignment.Center, textColor);
            DrawLargeText("But...", new Point(centreX, topY + 3 * titleLineOffset), LineAlignment.Center, textColor);
            DrawLargeText("But...", new Point(centreX, topY + 4 * titleLineOffset), LineAlignment.Center, textColor);

            DrawLargeText("You think the TV execs let you off that easy?", new Point(centreX, topY + titleLineOffset * 6), LineAlignment.Center, textColor);

            string deathStr = "After just " + Game.Dungeon.NumberOfFunModeDeaths + " stupid deaths?";
            if (Game.Dungeon.NumberOfFunModeDeaths == 1)
            {
                deathStr = "After just one stupid death?";
            }
            DrawLargeText(deathStr, new Point(centreX, topY + titleLineOffset * 7), LineAlignment.Center, textColor);

            DrawLargeText("Press [F] to restart the area", new Point(centreX, topY + titleLineOffset * 9), LineAlignment.Center, statsColor);

            DrawLargeText("Oh yeah, you lose any fame you had...", new Point(centreX, topY + titleLineOffset * 10), LineAlignment.Center, textColor);
        }

        public int ArenaSelected { get; set; }
        public IEnumerable<Monster> ArenaMonsters { get; set; }
        public IEnumerable<Item> ArenaItems { get; set; }
        
        public void ArenaSelectionScreen()
        {
            var titleColor = System.Drawing.Color.Khaki;

            DrawFramePixel(0, 0, ScreenWidth, ScreenHeight, true, System.Drawing.Color.Black, 1);
            DrawFramePixel(ScreenWidth / 8, ScreenHeight / 8, 6 * ScreenWidth / 8, 6 * ScreenHeight / 8, true, System.Drawing.Color.Blue, movieFrameWidth);

            var centreXOffset = ScreenWidth / 4;
            var centreYOffset = ScreenHeight / 4;

            var topY = centreYOffset / 2;
            var graphicsY = centreYOffset * 2 - centreYOffset / 2;

            var centreX = ScreenWidth / 2;

            var lanceCentre = new Point(centreXOffset, graphicsY);
            var crackCentre = new Point(centreXOffset * 2, graphicsY);
            var nerdCentre = new Point(centreXOffset * 3, graphicsY);

            var titleLineOffset = 30;

            DrawLargeText("Entering arena: " + (Game.Dungeon.ArenaLevelNumber() + 1), new Point(centreX, topY), LineAlignment.Center, statsColor);
            DrawLargeText("Pick your poison!", new Point(centreX, topY + titleLineOffset), LineAlignment.Center, textColor);
            DrawLargeText("[Left] and [Right] to choose difficulty", new Point(centreX, topY + 2 * titleLineOffset), LineAlignment.Center, textColor);
            DrawLargeText("[F] to begin", new Point(centreX, topY + 3 * titleLineOffset), LineAlignment.Center, textColor);

            string difficultyText = null;
            if (Game.Dungeon.Levels.Count() - Game.Dungeon.Player.LocationLevel == 3)
            {
                difficultyText = "Easy (?)";
            }
            else if (Game.Dungeon.Levels.Count() - Game.Dungeon.Player.LocationLevel == 2)
            {
                difficultyText = "Hard (!)";
            }
            else
            {
                difficultyText = "You must be joking (!!!)";
            }

            DrawLargeText("Difficulty: " + difficultyText, new Point(centreX, topY + 4 * titleLineOffset), LineAlignment.Center, statsColor);

            var monsterStr = "Arena denizens:";
            DrawLargeText(monsterStr, new Point(centreX, topY + 6 * titleLineOffset), LineAlignment.Center, titleColor);

            var monsterTL = new Point(ScreenWidth / 8, topY + 7 * titleLineOffset + 30);
            var maxWidth = 3 * ScreenWidth / 4;
            var maxIcons = maxWidth / 64;

            for (int i = 0; i < ArenaMonsters.Count(); i++)
            {
                var monster = ArenaMonsters.ElementAt(i);
                var columnNo = i % maxIcons;
                var rowNo = (int)Math.Floor(i / (double)maxIcons);
                var thisPoint = new Point(columnNo * 64, rowNo * 96);
                Point offset = new Point(0, 0);
                if (monster.GameSprite == "boss")
                {
                    offset = new Point(-32, 32);
                    DrawSpriteByCentre(monster.GameSprite, monsterTL + thisPoint + offset);

                }
                else
                {
                    DrawSpriteByCentre(monster.GameSprite, monsterTL + thisPoint + offset);
                    DrawSpriteByCentre("monster_level_" + monster.Level, monsterTL + thisPoint + new Point(0, 32) + offset);
                }
            }

            var itemTL = new Point(ScreenWidth / 8, centreYOffset + 500);

            var equipStr = "Equipment:";
            DrawLargeText(equipStr, new Point(centreX, itemTL.y - (int)Math.Ceiling(titleLineOffset * 1.5)), LineAlignment.Center, titleColor);

            for (int i = 0; i < ArenaItems.Count(); i++)
            {
                var item = ArenaItems.ElementAt(i);
                var columnNo = i % maxIcons;
                var rowNo = (int)Math.Floor(i / (double)maxIcons);
                var thisPoint = new Point(columnNo * 64, rowNo * 96);
                DrawSpriteByCentre(item.GameSprite, itemTL + thisPoint);
            }

        }

        int textLineNumber;

        internal void EndOfGameScreen()
        {
            textLineNumber = 0;

            DrawFramePixel(0, 0, ScreenWidth, ScreenHeight, true, System.Drawing.Color.Black, 1);
            DrawFramePixel(ScreenWidth / 8, ScreenHeight / 8, 6 * ScreenWidth / 8, 6 * ScreenHeight / 8, true, System.Drawing.Color.Blue, movieFrameWidth);

            var titleColor = System.Drawing.Color.Khaki;

            var centreXOffset = ScreenWidth / 4;
            var centreYOffset = ScreenHeight / 8;

            var topY = centreYOffset / 2;
            var graphicsY = centreYOffset * 2 - centreYOffset / 2;

            var centreX = ScreenWidth / 2;

            Point centrePoint = new Point(centreX, centreYOffset);
            string headingText = "It's all over!";
            string statusText = "";

            if (EndOfGameQuit)
            {
                statusText = "You fell on your own pole and ended it all!";
            }
            else if (EndOfGameWon)
            {
                statusText = "You lived to your next TV contract!";
            }
            else
            {
                statusText = "Easy come, easy go.";
            }

            DrawNextLine(headingText, centrePoint, titleColor);

            DrawNextLine(statusText, centrePoint, titleColor);

            var totalFame = Game.Dungeon.Player.CombatXP + 150 * Game.Dungeon.Player.Level;

            string fameText = "Final fame: " + totalFame;

            DrawNextLine(fameText, centrePoint, titleColor);

            var viewingFigures = (int)Math.Round(100 * totalFame / (double)(300 * Dungeon.TotalArenas * 1.2 * 2));

            string fameStr = "Slime Mold";

            if (viewingFigures > 10)
            {
                fameStr = "Chance Boudreaux";
            }
            if (viewingFigures > 20)
            {
                fameStr = "Korben Dallas";
            }
            if (viewingFigures > 30)
            {
                fameStr = "Patrick Mason";
            }
            if (viewingFigures > 40)
            {
                fameStr = "John Spartan";
            }
            if (viewingFigures > 50)
            {
                fameStr = "Snake Plisken";
            }
            if (viewingFigures > 60)
            {
                fameStr = "Riddick";
            }
            if (viewingFigures > 70)
            {
                fameStr = "Mad Max";
            }
            if (viewingFigures > 80)
            {
                fameStr = "Jason Bourne";
            }
            if (viewingFigures > 90)
            {
                fameStr = "Ripley";
            }
            
            DrawNextLine("Your viewing figures: ", centrePoint, titleColor);
            DrawNextLine(viewingFigures.ToString() + "%!", centrePoint, statsColor);

            DrawNextLine("Your exploits grant you an honourable place in history as the new:", centrePoint, titleColor);
            DrawNextLine(fameStr, centrePoint, statsColor);

            DrawNextLine("Stats", centrePoint, titleColor);

            //Deaths

            if(Game.Dungeon.FunMode) {
                if (Game.Dungeon.NumberOfFunModeDeaths > 0)
                {
                    var deathStr = "You died: " + Game.Dungeon.NumberOfFunModeDeaths + " times (it's just for fun, right?)";
                    DrawNextLine(deathStr, centrePoint, statsColor);
                }

                if (EndOfGameWon)
                {
                    var deathStr = "Well, you won. Congrats and all that. Wanna play properly now?";
                    DrawNextLine(deathStr, centrePoint, statsColor);

                    if (Game.Dungeon.NumberOfFunModeDeaths == 0)
                    {
                        var deathStr2 = "And you didn't die? Bet you wish you'd pressed [R] now!";
                        DrawNextLine(deathStr2, centrePoint, statsColor);
                    }
                }
            }

            //Total kills
            var killRecord = Game.Dungeon.GetKillRecord();

            var killCount = "Opponents massacred: " + killRecord.killCount;
            DrawNextLine(killCount, centrePoint, statsColor);

            textLineNumber++;
            var thanks = "Thanks for playing another of our 7DRLs! -flend and ShroomArts";
            DrawNextLine(thanks, centrePoint, titleColor);

            textLineNumber+= 4;

            var nextGame = "Press RETURN to play again!";
            DrawNextLine(nextGame, centrePoint, titleColor);

            //Compose the obituary
            /*
            List<string> obString = new List<string>();

            obString.Add(fameStr);

            Game.Dungeon.SaveObituary(obString, killRecord.killStrings);
             */
        }

        private void DrawNextLine(string msg, Point centreOrigin, System.Drawing.Color color) {
            var pt = centreOrigin + new Point(0, textLineNumber * 40);
            DrawLargeText(msg, new Point(pt.x, pt.y), LineAlignment.Center, color);
            textLineNumber++;
        }

        public enum AttackType
        {
            Explosion, Bullet, Laser,
            Stun, Acid
        }

        /// <summary>
        /// Draws an animated attack.
        /// </summary>
        public void DrawAreaAttackAnimation(IEnumerable <Point> targetSquares, AttackType attackType, bool progressive = false, int animationDelay = 0)
        {
            string explosionSprite = "explosion";

            switch (attackType)
            {
                case AttackType.Bullet:
                    explosionSprite = "bullet";
                    break;

                case AttackType.Laser:
                    explosionSprite = "laz";
                    break;

                case AttackType.Stun:
                    explosionSprite = "paralexp";
                    break;

                case AttackType.Acid:
                    explosionSprite = "acidexp";
                    break;
            }

            if (!progressive)
                DrawAreaAttackAnimation(targetSquares, explosionSprite, animationDelay);
            else
                DrawAreaAttackAnimationProgressive(targetSquares, explosionSprite, animationDelay);
        }

        /// <summary>
        /// Draws an animated attack.
        /// </summary>
        public void DrawAreaAttackAnimation(IEnumerable<Point> targetSquares, string spriteName, int animationDelay = 0)
        {
            //Clone the list since we mangle it
            List<Point> mangledPoints = new List<Point>();
            foreach (Point p in targetSquares)
            {
                mangledPoints.Add(new Point(p));
            }

            //Add animation points into the animation layer

            foreach (Point p in mangledPoints)
            {
                if (!isViewVisible(p))
                    continue;

                tileMapLayer(TileLevel.Animations)[ViewRelative(p)] = new TileEngine.TileCell(explosionIcon);
                tileMapLayer(TileLevel.Animations)[ViewRelative(p)].TileSprite = spriteName;
                tileMapLayer(TileLevel.Animations)[ViewRelative(p)].Animation = new TileEngine.Animation(combationAnimationFrameDuration, animationDelay);

            }
        }

        /// <summary>
        /// Draws an animated attack.
        /// </summary>
        public void DrawAreaAttackAnimationProgressive(IEnumerable<Point> targetSquares, string spriteName, int animationDelay = 0)
        {
            //Clone the list since we mangle it
            List<Point> mangledPoints = new List<Point>();
            foreach (Point p in targetSquares)
            {
                mangledPoints.Add(new Point(p));
            }

            //Add animation points into the animation layer

            var frameTime = (int)Math.Round(combatFastAnimationFrameDuration / (double)mangledPoints.Count());

            int counter = 0;
            foreach (Point p in mangledPoints)
            {
                if (!isViewVisible(p))
                    continue;

                tileMapLayer(TileLevel.Animations)[ViewRelative(p)] = new TileEngine.TileCell(explosionIcon);
                tileMapLayer(TileLevel.Animations)[ViewRelative(p)].TileSprite = spriteName;
                tileMapLayer(TileLevel.Animations)[ViewRelative(p)].Animation = new TileEngine.Animation(frameTime, animationDelay + counter * frameTime);
                counter++;

            }
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

                    List<Point> thisLineSquares = Game.Dungeon.GetPathLinePoints(Game.Dungeon.Player.LocationMap, Target);
                    DrawTargettingOverSquaresAndCreatures(thisLineSquares);    
                    break;

                case TargettingType.LineThrough:

                    //Cast a line which terminates on the edge of the map
                    Point projectedLine = Game.Dungeon.GetEndOfLine(player.LocationMap, Target, player.LocationLevel);

                    //Get the in-FOV points up to that end point
                    WrappedFOV currentFOV2 = Game.Dungeon.CalculateAbstractFOV(Game.Dungeon.Player.LocationLevel, Game.Dungeon.Player.LocationMap, 80);
                    List<Point> lineSquares = Game.Dungeon.GetPathLinePointsInFOV(Game.Dungeon.Player.LocationLevel, Game.Dungeon.Player.LocationMap, projectedLine, currentFOV2);

                    DrawTargettingOverSquaresAndCreatures(lineSquares);      

                    break;
                    
                case TargettingType.Rocket:
                    {
                        //Todo

                    }
                    break;

                case TargettingType.Shotgun:
                    {
                        int size = TargetRange;
                        double spreadAngle = TargetPermissiveAngle;

                        CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);
                        List<Point> splashSquares = currentFOV.GetPointsForTriangularTargetInFOV(player.LocationMap, Target, Game.Dungeon.Levels[player.LocationLevel], size, spreadAngle);

                        DrawTargettingOverSquaresAndCreatures(splashSquares);
                    }
                    break;
            }

            //Highlight target if in range
            if (!isViewVisible(Target))
                return;

            //Draw actual target point
            if (SetTargetInRange)
            {
                var targetSprite = TargetAction == TargettingAction.Examine ? greenTargetTile : redTargetTile;
                tileMapLayer(TileLevel.TargettingUI)[ViewRelative(Target)] = new TileEngine.TileCell(targetSprite);
            }
            else
            {
                var targetSprite = blackTargetTile;
                tileMapLayer(TileLevel.TargettingUI)[ViewRelative(Target)] = new TileEngine.TileCell(targetSprite);
            }
                       

            /*
            System.Drawing.Color backgroundColor = targetBackground;
            System.Drawing.Color foregroundColor = targetForeground;

            if (SetTargetInRange)
            {
                backgroundColor = System.Drawing.Color.Red;
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
                tileMapLayer(TileLevel.TargettingUI)[ViewRelative(Target)].TileFlag = new LibtcodColorFlags(foregroundColor, backgroundColor);
             */
            
        }
        /*
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
                tileMapLayer(TileLevel.TargettingUI)[ViewRelative(p)].TileFlag = new LibtcodColorFlags(System.Drawing.Color.Red);
            }
        }*/

        private void DrawTargettingOverSquaresAndCreatures(List<Point> splashSquares)
        {
            //Draw each point as targetted
            foreach (Point p in splashSquares)
            {
                if (!isViewVisible(p))
                    continue;

                //If there's a monster in the square, draw it in red in the animation layer. Otherwise, draw an explosion

                var targetSprite = "orangetarget";
                tileMapLayer(TileLevel.TargettingUI)[ViewRelative(p)] = new TileEngine.TileCell(targetSprite);
                tileMapLayer(TileLevel.TargettingUI)[ViewRelative(p)].TileFlag = new LibtcodColorFlags(System.Drawing.Color.Red);
            }
        }



        public bool ShowMsgHistory { get; set; }

        public bool ShowClueList { get; set; }

        public bool ShowLogList { get; set; }

        enum Direction { up, down, none };

        void ClearScreen()
        {
            mapRenderer.Clear(); 
        }

        /// <summary>
        /// Draw rectangle
        /// </summary>
        void ClearRect(int x, int y, int width, int height)
        {
            mapRenderer.DrawRectangle(new Rectangle(x, y, width, height), System.Drawing.Color.Black);
        }

        private void DrawUISpriteByCentre(string id, Point point)
        {
            Size spriteDim = UISpriteSize(id);
            DrawUISprite(id, new Point(point.x - spriteDim.Width / 2, point.y - spriteDim.Height / 2));
        }

        private void DrawUISpriteByCentre(MapObject mapObject, Point point, double alpha = 1.0)
        {
            
            if (mapObject.UISprite != null)
            {
                Size spriteDim = UISpriteSize(mapObject.UISprite);
                DrawUISprite(mapObject.UISprite, new Point(point.x - spriteDim.Width / 2, point.y - spriteDim.Height / 2), alpha);
            }
            else
            {
                Size spriteDim = TraumaUISpriteSize(mapObject.Representation);
                LibtcodColorFlags colors = new LibtcodColorFlags(mapObject.RepresentationColor());
                
                //Apply an alphaing effect to the colors, since alphaing on Trauma sprites doesn't seem to work now
                if(alpha < 0.99) {
                   colors = new LibtcodColorFlags(ColorInterpolate(mapObject.RepresentationColor(), System.Drawing.Color.Black, alpha));
                }

                DrawTraumaUISprite(mapObject.Representation, new Point(point.x - spriteDim.Width / 2, point.y - spriteDim.Height / 2), colors, alpha);
            }
        }

        private void DrawColourizedTraumaUISpriteByCentre(MapObject mapObject, Point point, LibtcodColorFlags colorOverride)
        {
            Size spriteDim = TraumaUISpriteSize(mapObject.Representation);
            DrawTraumaUISprite(mapObject.Representation, new Point(point.x - spriteDim.Width / 2, point.y - spriteDim.Height / 2), colorOverride);
        }


        private void DrawSpriteByCentre(string id, Point point)
        {
            Size spriteDim = SpriteSize(id);
            DrawSprite(id, new Point(point.x - spriteDim.Width / 2, point.y - spriteDim.Height / 2));
        }
        
        private void DrawGraduatedBar(string id, double fullness, Rectangle barArea, double spacing)
        {
            //This isn't quite right since it ends with a gap
            Size barSize = UISpriteSize(id);
            double barSpacing = (barSize.Width * (1.0 + spacing));
            double oneBarProportion = barSpacing / barArea.Width;
            int barsToDraw = (int)Math.Floor(fullness / oneBarProportion);

            for (int i = 0; i < barsToDraw; i++)
            {
                int x = barArea.X + (int)Math.Floor(i * barSpacing);
                DrawUISprite(id, new Point(x, barArea.Y));
            }
        }

        private void DrawGraduatedBarVertical(string id, double fullness, Rectangle barArea, double spacing)
        {
            Size barSize = UISpriteSize(id);
            double barSpacing = (barSize.Height * (spacing));
            double totalBarsThatFit = (barArea.Height - barSpacing) / (barSize.Height + barSpacing);
            int barsToDraw = (int)Math.Ceiling(totalBarsThatFit * fullness);

            for (int i = 0; i < barsToDraw; i++)
            {
                int y = barArea.Y + (int)Math.Floor(i * (barSpacing + barSize.Height));
                DrawUISprite(id, new Point(barArea.X, y));
            }
        }

        Point playerUI_TL = new Point(0, 0);
        Point playerTextUI_TL = new Point(0, 0);
        Point playerTextUI_UsefulTL = new Point(0, 0);
        Point monsterTextUI_TL = new Point(0, 0);

        private Point UIScale(Point p)
        {
            return p * UIScaling;
        }

        private int UIScale(int coord)
        {
            return (int)Math.Round(coord * UIScaling);
        }

        private Size UIScale(Size size)
        {
            return new Size(UIScale(size.Width), UIScale(size.Height));
        }

        private void DrawUI()
        {
            Player player = Game.Dungeon.Player;

            //Calculate some point offsets
            Point rangedWeaponUICenter = UIScale(new Point(160, 92));
            Point meleeWeaponUICenter = UIScale(new Point(38, 92));
            Point utilityUICenter = UIScale(new Point(300, 92));
            Point wetwareUICenter = UIScale(new Point(390, 92));

            //Draw the UI background
            Size uiLeftDim = UISpriteSize("ui_left_extended");
            playerUI_TL = new Point(0, ScreenHeight - uiLeftDim.Height);

            DrawUISprite("ui_left_extended", new Point(playerUI_TL.x, playerUI_TL.y));

            if (ExtraUI)
            {
                Size uiMidDim = UISpriteSize("ui_mid");
                playerTextUI_TL = playerUI_TL + new Point(uiLeftDim.Width, -60);
                playerTextUI_UsefulTL = playerTextUI_TL + UIScale(new Point(0, 90));
                
                DrawUISprite("ui_mid", playerTextUI_TL);
                Size uiRightDim = UISpriteSize("ui_right");

                monsterTextUI_TL = new Point(ScreenWidth - uiRightDim.Width - uiMidDim.Width, ScreenHeight - uiMidDim.Height);
                DrawUISprite("ui_mid", monsterTextUI_TL);
            }

            //Draw equipped ranged weapon
            Item weapon = player.GetEquippedRangedWeaponAsItem();

            if (weapon != null)
            {
                IEquippableItem weaponE = weapon as IEquippableItem;
                RangedWeapon weaponR = weapon as RangedWeapon;

                DrawUISpriteByCentre(weapon, new Point(playerUI_TL.x + rangedWeaponUICenter.x, playerUI_TL.y + rangedWeaponUICenter.y));

                var rangedDamage = player.ScaleRangedDamage(weaponE, weaponE.DamageBase());
                
                //Draw bullets
                double weaponAmmoRatio = weaponE.RemainingAmmo() / (double)weaponE.MaxAmmo();
                var ammoBarTL = playerUI_TL + UIScale(new Point(86, 67));
                DrawGraduatedBarVertical("ui_bullet", weaponAmmoRatio, new Rectangle(ammoBarTL.ToPoint(), UIScale(new Size(20, 54))), 0.1);

                //Ranged Damage base
                var playerRangedTextOffset = UIScale(new Point(210, 117));
                var rangedStr = "DMG: " + rangedDamage;
                DrawSmallUIText(rangedStr, playerUI_TL + playerRangedTextOffset, LineAlignment.Center, statsColor);

                //Help
                var rangedHelpOffset = UIScale(new Point(218, 74));
                var rangedHelp = "(F)";
                DrawText(rangedHelp, playerUI_TL + rangedHelpOffset, LineAlignment.Center, statsColor);
            }

            //Draw equipped melee weapon
            Item meleeWeapon = player.GetEquippedMeleeWeaponAsItem();

            if (meleeWeapon != null)
            {
                IEquippableItem weaponE = meleeWeapon as IEquippableItem;

                DrawUISpriteByCentre(meleeWeapon, new Point(playerUI_TL.x + meleeWeaponUICenter.x, playerUI_TL.y + meleeWeaponUICenter.y));

                var meleeDamage = Game.Dungeon.Player.ScaleMeleeDamage(meleeWeapon, weaponE.MeleeDamage());

                var playerRangedTextOffset = UIScale(new Point(40, 117));
                var rangedStr = "DMG: " + meleeDamage;
                DrawSmallUIText(rangedStr, playerUI_TL + playerRangedTextOffset, LineAlignment.Center, statsColor);

            }

            //Draw equipped utility weapon
            Item utility = player.GetEquippedUtilityAsItem();

            if (utility != null)
            {
                DrawUISpriteByCentre(utility, playerUI_TL + utilityUICenter);

                //Draw no of items
                double itemNoRatio = Math.Min(player.GetNoItemsOfSameType(utility) / (double)10.0, 1.0);
                var itemNoBarTL = playerUI_TL + UIScale(new Point(255, 67));
                DrawGraduatedBarVertical("ui_bullet", itemNoRatio, new Rectangle(itemNoBarTL.ToPoint(), UIScale(new Size(20, 54))), 0.1);
            }

            var utilityHelpOffset = UIScale(new Point(316, 74));
            var utilityHelp = "(T)";
            DrawUIText(utilityHelp, playerUI_TL + utilityHelpOffset, LineAlignment.Center, statsColor);
            DrawSmallUIText("(E)", playerUI_TL + UIScale(new Point(280, 117)), LineAlignment.Center, statsColor);
            DrawSmallUIText("(R)", playerUI_TL + UIScale(new Point(316, 117)), LineAlignment.Center, statsColor);

            //Draw equipped wetware
            Item wetware = player.GetSelectedWetware();

            if (wetware != null)
            {
                //This only works for trauma sprites and needs refactoring for normal sprites

                LibtcodColorFlags wetwareRenderColor = new LibtcodColorFlags(wetware.RepresentationColor());
                
                if (player.GetTurnsDisabledForSelectedWetware() > 0)
                {
                    wetwareRenderColor = new LibtcodColorFlags(ColorInterpolate(wetware.RepresentationColor(), System.Drawing.Color.Black, 0.5));
                }
                else if (player.GetEquippedWetware() != null)
                {
                    wetwareRenderColor = new LibtcodColorFlags(ColorInterpolate(wetware.RepresentationColor(), System.Drawing.Color.Red, 0.5));
                }

                DrawColourizedTraumaUISpriteByCentre(wetware, playerUI_TL + wetwareUICenter, wetwareRenderColor);
            }

            //Draw energy
            double energyRatio = player.Energy / (double)player.MaxEnergy;
            var energyBarTL = playerUI_TL + UIScale(new Point(345, 67));
            DrawGraduatedBarVertical("ui_bullet", energyRatio, new Rectangle(energyBarTL.ToPoint(), UIScale(new Size(20, 54))), 0.1);

            //Draw wetware help
            var wetwareHelpOffset = UIScale(new Point(406, 74));
            var wetwareHelp = "(W)";
            DrawUIText(wetwareHelp, playerUI_TL + wetwareHelpOffset, LineAlignment.Center, statsColor);
            DrawSmallUIText("(A)", playerUI_TL + UIScale(new Point(370, 117)), LineAlignment.Center, statsColor);
            DrawSmallUIText("(S)", playerUI_TL + UIScale(new Point(406, 117)), LineAlignment.Center, statsColor);


            //Draw Shield
            double playerShieldRatio = player.Shield / (double)player.MaxShield;
            Point shieldOffset = playerUI_TL + UIScale(new Point(57, 34));
            Size shieldSize = UIScale(new Size(180, 12));
            DrawGraduatedBar("ui_bar", playerShieldRatio, new Rectangle(shieldOffset.ToPoint(), shieldSize), 0.2);

            //Draw HP
            double playerHPRatio = player.Hitpoints / (double)player.MaxHitpoints;
            Point hpOffset = playerUI_TL + UIScale(new Point(57, 13));
            Size hpSize = UIScale(new Size(180, 12));
            DrawGraduatedBar("ui_bar", playerHPRatio, new Rectangle(hpOffset.ToPoint(), hpSize), 0.2);

            //Draw HP digits
            var playerHPNuOffset = UIScale(new Point(25, 33));
            DrawUIText(player.Hitpoints.ToString(), playerUI_TL + playerHPNuOffset, LineAlignment.Center, statsColor);

            //Draw shield digits
            var playerFMNuOffset = UIScale(new Point(269, 33));
            DrawUIText(player.Shield.ToString(), playerUI_TL + playerFMNuOffset, LineAlignment.Center, statsColor);

            //Draw timers

            if (ExtraUI)
            {
                var playerHPTextOffset = UIScale(new Point(10, 0));
                var hpStr = "HP: " + player.Hitpoints + "/" + player.MaxHitpoints;
                DrawUIText(hpStr, playerTextUI_UsefulTL + playerHPTextOffset);

                //Draw Fame
                var playerFameTextOffset = UIScale(new Point(10, 60));
                var fameStr = "Fame: " + player.CombatXP;
                DrawText(fameStr, playerTextUI_UsefulTL + playerFameTextOffset);
                var playerExpFameTextOffset = UIScale(new Point(10, 75));
                //var fameExpStr = " [H]eal: " + player.GetHealXPCost() + " [L]evel: " + player.GetLevelXPCost();
                //DrawText(fameExpStr, playerUI_TL.Add(playerExpFameTextOffset));

                var playerMoveOffset = UIScale(new Point(170, 0));
                var moveStr = "Move: " + player.TurnsMoving;
                DrawText(moveStr, playerTextUI_UsefulTL + playerMoveOffset);

                var playerStationaryOffset = UIScale(new Point(170, 15));
                var actionStr = "No A: " + player.TurnsSinceAction;
                DrawText(actionStr, playerTextUI_UsefulTL + playerStationaryOffset);

                var playerRestOffset = UIScale(new Point(170, 30));
                var restStr = "Rest: " + player.TurnsInactive;
                DrawText(restStr, playerTextUI_UsefulTL + playerRestOffset);

                var dodgeBonusOffset = UIScale(new Point(170, 45));
                var dodgeStr = "Dodge: " + player.CalculateDamageModifierForAttacksOnPlayer(null, true);
                DrawText(dodgeStr, playerTextUI_UsefulTL + dodgeBonusOffset);
            }

            //Monster stats
            DrawFocusWindow();
        }

        public void DrawText(string msg, Point p)
        {
            mapRenderer.DrawText(msg, p.x, p.y, largeTextSize, LineAlignment.Left, statsColor);
        }

        public void DrawText(string msg, Point p, LineAlignment lineAlignment, System.Drawing.Color color)
        {
            mapRenderer.DrawText(msg, p.x, p.y, largeTextSize, lineAlignment, color);
        }

        public void DrawSmallText(string msg, Point p, LineAlignment lineAlignment, System.Drawing.Color color)
        {
            mapRenderer.DrawText(msg, p.x, p.y, smallTextSize, lineAlignment, color);
        }

        public void DrawLargeText(string msg, Point p, LineAlignment lineAlignment, System.Drawing.Color color)
        {
            mapRenderer.DrawText(msg, p.x, p.y, largeTextSize, lineAlignment, color);
        }

        public void DrawText(string msg, Point p, LineAlignment lineAlignment, int size, System.Drawing.Color color)
        {
            mapRenderer.DrawText(msg, p.x, p.y, size, lineAlignment, color);
        }

        public void DrawText(string msg, Point p, LineAlignment lineAlignment, System.Drawing.Color foregroundColor, System.Drawing.Color backgroundColor)
        {
            mapRenderer.DrawText(msg, p.x, p.y, largeTextSize, lineAlignment, foregroundColor, backgroundColor);
        }

        public void DrawSmallText(string msg, Point p, LineAlignment lineAlignment, System.Drawing.Color foregroundColor, System.Drawing.Color backgroundColor)
        {
            mapRenderer.DrawText(msg, p.x, p.y, smallTextSize, lineAlignment, foregroundColor, backgroundColor);
        }

        public void DrawLargeText(string msg, Point p, LineAlignment lineAlignment, System.Drawing.Color foregroundColor, System.Drawing.Color backgroundColor)
        {
            mapRenderer.DrawText(msg, p.x, p.y, largeTextSize, lineAlignment, foregroundColor, backgroundColor);
        }

        public void DrawText(string msg, Point p, LineAlignment lineAlignment, int size, System.Drawing.Color foregroundColor, System.Drawing.Color backgroundColor)
        {
            mapRenderer.DrawText(msg, p.x, p.y, size, lineAlignment, foregroundColor, backgroundColor);
        }

        void DrawTextWidth(string msg, Point p, int width, System.Drawing.Color color)
        {
            mapRenderer.DrawTextWidth(msg, p.x, p.y, largeTextSize, width, color);
        }

        private void DrawUIText(string msg, Point p)
        {
            DrawUIText(msg, p, LineAlignment.Left, statsColor);
        }

        private void DrawUIText(string msg, Point p, LineAlignment lineAlignment, System.Drawing.Color color)
        {
            mapRenderer.DrawText(msg, p.x, p.y, (int)Math.Round(largeTextSize * UIScaling), lineAlignment, color);
        }

        private void DrawSmallUIText(string msg, Point p, LineAlignment lineAlignment, System.Drawing.Color color)
        {
            mapRenderer.DrawText(msg, p.x, p.y, (int)Math.Round(smallTextSize * UIScaling), lineAlignment, color);
        }

        private void DrawFocusWindow()
        {
            Player player = Game.Dungeon.Player;


            //Calculate some point offsets
            
            Point rightUIIconCentre = UIScale(new Point(118, 152));
            Size uiRightDim = UISpriteSize("ui_right");

            Point monsterUI_TL = new Point(ScreenWidth - uiRightDim.Width, ScreenHeight - uiRightDim.Height);
            var monsterTextUI_UsefulTL = monsterTextUI_TL + UIScale(new Point(0, 90));

            //Draw UI background

            DrawUISprite("ui_right", new Point(monsterUI_TL.x, monsterUI_TL.y));

            //Creature picture (overwrite with frame)

            if (CreatureToView != null && CreatureToView.Alive == true)
            {
                DrawUISpriteByCentre(CreatureToView, monsterUI_TL + rightUIIconCentre);
            }

            DrawUISprite("frame", monsterUI_TL + UIScale(new Point(79, 107)));

            if (CreatureToView != null && CreatureToView.Alive == true)
            {

                //PrintLine("Def: " + player.CalculateDamageModifierForAttacksOnPlayer(CreatureToView), statsDisplayTopLeft.x + cmbtOffset.x, statsDisplayTopLeft.y + cmbtOffset.y + 2, LineAlignment.Left, statsColor);
                //var cover = player.GetPlayerCover(CreatureToView);
                //PrintLine("C: " + cover.Item1 + "/" + cover.Item2, statsDisplayTopLeft.x + cmbtOffset.x, statsDisplayTopLeft.y + cmbtOffset.y + 3, LineAlignment.Left, statsColor);

                //Monster hp
                double enemyHPRatio = CreatureToView.Hitpoints / (double)CreatureToView.MaxHitpoints;
                Point hpLocation = monsterUI_TL + UIScale(new Point(14, 92));
                Size hpSize = UIScale(new Size(95, 12));
                DrawGraduatedBar("ui_bar", enemyHPRatio, new Rectangle(hpLocation.ToPoint(), hpSize), 0.2);

                //Damage
                var monsterLVLTextOffset = UIScale(new Point(47, 142));
                var lvlStr = "LVL: " + CreatureToView.Level;
                DrawSmallUIText(lvlStr, monsterUI_TL + monsterLVLTextOffset, LineAlignment.Center, statsColor);

                var monsterDamageTextOffset = UIScale(new Point(47, 162));
                var dmStr = "DMG: " + CreatureToView.GetScaledDamage();
                DrawSmallUIText(dmStr, monsterUI_TL + monsterDamageTextOffset, LineAlignment.Center, statsColor);

                var monsterHPNumOffset = UIScale(new Point(134, 93));
                DrawUIText(CreatureToView.Hitpoints.ToString(), monsterUI_TL + monsterHPNumOffset, LineAlignment.Center, statsColor);

                if (ExtraUI)
                {
                    var lineOffset = 15;

                    var monsterHPTextOffset = UIScale(new Point(10, lineOffset * 0));
                    var hpStr = "HP: " + CreatureToView.Hitpoints + "/" + CreatureToView.MaxHitpoints;
                    DrawUIText(hpStr, monsterTextUI_UsefulTL + monsterHPTextOffset);


                    var cover = Game.Dungeon.Player.CalculateDamageModifierForAttacksOnPlayer(CreatureToView, true);
                    var monsterCoverNumberOffset = UIScale(new Point(10, lineOffset * 1));
                    var cvStr = "CV: " + cover;
                    DrawUIText(cvStr, monsterTextUI_UsefulTL + monsterCoverNumberOffset);

                    //Combat vs player

                    var coverItem = player.GetPlayerCover(CreatureToView);
                    string coverStr = "";
                    if (coverItem.Item1 > 0)
                    {
                        coverStr = "(hard cover)";
                    }
                    else if (coverItem.Item2 > 0)
                    {
                        coverStr = "(soft cover)";
                    }

                    var monsterCoverTextOffset = UIScale(new Point(100, lineOffset * 1));
                    DrawUIText(coverStr, monsterTextUI_UsefulTL + monsterCoverTextOffset);

                    //Behaviour

                    var behaviourTextOffset = UIScale(new Point(10, lineOffset * 2));

                    if (CreatureToView.StunnedTurns > 0)
                    {
                        DrawUIText("(Stunned: " + CreatureToView.StunnedTurns + ")", monsterTextUI_UsefulTL + behaviourTextOffset);
                    }
                    else if (CreatureToView.InPursuit())
                    {
                        DrawUIText("(Hostile)", monsterTextUI_UsefulTL + behaviourTextOffset);
                    }
                    else if (!CreatureToView.OnPatrol())
                    {
                        DrawUIText("(Investigating)", monsterTextUI_UsefulTL + behaviourTextOffset);
                    }
                    else
                    {
                        DrawUIText("(Neutral)", monsterTextUI_UsefulTL + behaviourTextOffset);

                    }
                }
            }

            else if (ItemToView != null)
            {
                DrawUISpriteByCentre(ItemToView, monsterUI_TL + rightUIIconCentre);
            }
            else if (FeatureToView != null)
            {
                DrawUISpriteByCentre(FeatureToView, monsterUI_TL + rightUIIconCentre);
            }
        }

        private Size UISpriteSize(string name)
        {
            var unscaledSize = mapRenderer.GetUISpriteDimensions(name);
            return new Size((int)Math.Round(unscaledSize.Width * UIScaling), (int)Math.Round(unscaledSize.Height * UIScaling));
        }

        private Size TraumaUISpriteSize(int id)
        {
            var unscaledSize = mapRenderer.GetTraumaSpriteDimensions(id);
            return new Size((int)Math.Round(unscaledSize.Width * UIScaling), (int)Math.Round(unscaledSize.Height * UIScaling));
        }

        private Size SpriteSize(string name)
        {
            return mapRenderer.GetUISpriteDimensions(name);
        }

        private void DrawUISprite(string name, Point p, double alpha = 1.0)
        {
            mapRenderer.DrawUISprite(name, p.x, p.y, UIScaling, alpha);
        }

        private void DrawTraumaUISprite(int id, Point p, LibtcodColorFlags flags, double alpha = 1.0)
        {
            mapRenderer.DrawTraumaUISprite(id, p.x, p.y, flags, UIScaling, alpha);
        }

        private void DrawSprite(string name, Point p, double alpha = 1.0)
        {
            mapRenderer.DrawUISprite(name, p.x, p.y, 1.0, alpha);
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
                System.Drawing.Color itemColorToUse = item.GetColour();

                IEquippableItem equipItem = item as IEquippableItem;
                if (equipItem != null)
                {
                    //Show no ammo items in a neutral colour
                    if (equipItem.HasFireAction() && equipItem.RemainingAmmo() == 0)
                        itemColorToUse = System.Drawing.Color.Gray;
                }

                //Color itemColorToUse = itemColor;

                bool drawItem = true;
                double spriteAlpha = 0.0;

                if (itemSquare.InPlayerFOV || SeeAllMap)
                {
                   
                }
                else if (itemSquare.SeenByPlayerThisRun)
                {
                    //Not in FOV now but seen this adventure
                    //Don't draw items in squares seen in previous adventures (since the items have respawned)
                    itemColorToUse = ColorInterpolate(item.GetColour(), System.Drawing.Color.Black, 0.5);
                    spriteAlpha = 0.3;
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

                    tileMapLayer(TileLevel.Items)[ViewRelative(item.LocationMap)] = new TileEngine.TileCell(item.GameSprite);
                    tileMapLayer(TileLevel.Items)[ViewRelative(item.LocationMap)].TileID = item.Representation;
                    tileMapLayer(TileLevel.Items)[ViewRelative(item.LocationMap)].TileFlag = new LibtcodColorFlags(itemColorToUse);
                    tileMapLayer(TileLevel.Items)[ViewRelative(item.LocationMap)].Transparency = spriteAlpha;

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

                System.Drawing.Color featureColor = thisLock.RepresentationColor();
                double spriteAlpha = 0.0;

                bool drawFeature = true;

                if (featureSquare.InPlayerFOV || SeeAllMap)
                {

                }
                else if (featureSquare.SeenByPlayer)
                {
                    //Not in FOV but seen
                    featureColor = ColorInterpolate(featureColor, System.Drawing.Color.Black, 0.3);
                    spriteAlpha = 0.3;
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
                    tileMapLayer(TileLevel.Features)[ViewRelative(thisLock.LocationMap)].Transparency = spriteAlpha;
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

                System.Drawing.Color featureColor = feature.RepresentationColor();
                double spriteAlpha = 1.0;
                bool drawFeature = true;

                if (featureSquare.InPlayerFOV || SeeAllMap)
                {
                    //In FOV
                    //rootConsole.ForegroundColor = inFOVTerrainColor;
                }
                else if (featureSquare.SeenByPlayer)
                {
                    //Not in FOV but seen
                    featureColor = ColorInterpolate(featureColor, System.Drawing.Color.Black, 0.3);
                    spriteAlpha = 0.3;

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

                    tileMapLayer(TileLevel.Features)[ViewRelative(feature.LocationMap)] = new TileEngine.TileCell(feature.GameSprite);
                    tileMapLayer(TileLevel.Features)[ViewRelative(feature.LocationMap)].TileID = feature.Representation;
                    tileMapLayer(TileLevel.Features)[ViewRelative(feature.LocationMap)].TileFlag = new LibtcodColorFlags(featureColor, feature.RepresentationBackgroundColor());
                    tileMapLayer(TileLevel.Features)[ViewRelative(feature.LocationMap)].Transparency = spriteAlpha;
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

                System.Drawing.Color creatureColor = creature.RepresentationColor();

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
                                tileMapLayer(TileLevel.CreatureDecoration)[ViewRelative(headingLoc)] = new TileEngine.TileCell(redTargetTile);
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
                System.Drawing.Color creatureColor = creature.RepresentationColor();

                System.Drawing.Color foregroundColor;
                System.Drawing.Color backgroundColor;

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
                        creatureColor = ColorInterpolate(creatureColor, System.Drawing.Color.Red, 0.4);
                    }

                    //Draw waypoints
                    MonsterFightAndRunAI monsterWithWP = creature as MonsterFightAndRunAI;
                    if (monsterWithWP != null &&
                        monsterWithWP.Waypoints.Count > 0)
                    {
                        int wpNo = 0;
                        foreach (Point wp in monsterWithWP.Waypoints)
                        {
                            int wpX = mapTopLeftBase.x + wp.x;
                            int wpY = mapTopLeftBase.y + wp.y;

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
                    backgroundColor = System.Drawing.Color.Transparent;
                    string overlapSprite = null;

                    //Overlay depends on status

                    if (creature.Charmed)
                    {

                    }
                    else if (creature.Passive)
                    {

                    }
                    else if (creature.StunnedTurns > 0)
                    {
                        overlapSprite = "stun";
                    }
                    else if (creature.Sleeping)
                    {
                        overlapSprite = "zzoverlay";
                    }
                    else if (creature.ReloadingTurns > 0)
                    {
                        overlapSprite = "reloading";
                    }
                    else
                    {
                        MonsterFightAndRunAI monsterWithAI = creature as MonsterFightAndRunAI;

                        if (monsterWithAI != null && monsterWithAI.AIState == SimpleAIStates.InvestigateSound)
                        {
                            overlapSprite = "investigating";
                        }
                    }

                    string targetSprite = null;
                    
                    IEquippableItem weapon = Game.Dungeon.Player.GetEquippedRangedWeapon();

                    if (weapon != null)
                    {

                        //In range firing
                        if (weapon.HasFireAction() && Utility.TestRangeFOVForWeapon(Game.Dungeon.Player, creature, weapon.RangeFire(), currentFOV))
                        {
                            targetSprite = blueTargetTile;
                        }
                        else
                        {
                            //In throwing range
                            if (weapon.HasThrowAction() && Utility.TestRangeFOVForWeapon(Game.Dungeon.Player, creature, weapon.RangeFire(), currentFOV))
                            {

                            }
                        }

                        //Also agressive
                        if (creature.InPursuit())
                        {
                        }
                    }

                    if (creature == Screen.Instance.CreatureToView)
                        targetSprite = greenTargetTile;

                    if (creature.InPursuit())
                    {
                        //backgroundColor = pursuitBackground;
                    }
                    else if (!creature.OnPatrol())
                    {
                        //backgroundColor = investigateBackground;
                    }
                    // else if (creature.Unique)
                    //backgroundColor = uniqueBackground;
                    // else
                    // backgroundColor = normalBackground;


                    //Cover
                    var cover = Game.Dungeon.Player.GetPlayerCover(creature);
                    string coverSprite = null;
                    double coverTransparency = 1.0;
                    if (cover.Item1 > 0)
                    {
                        coverSprite = "cover";
                        coverTransparency = 1.0;
                    }
                    else if (cover.Item2 > 0)
                    {
                        coverSprite = "cover";
                        coverTransparency = 0.5;
                    }

                    //Creature

                    if (isViewVisible(creature.LocationMap))
                    {
                        tileMapLayer(TileLevel.Creatures)[ViewRelative(creature.LocationMap)] = new TileEngine.TileCell(creature.GameSprite);
                        tileMapLayer(TileLevel.Creatures)[ViewRelative(creature.LocationMap)].TileID = creature.Representation;
                        tileMapLayer(TileLevel.Creatures)[ViewRelative(creature.LocationMap)].TileFlag = new LibtcodColorFlags(foregroundColor, backgroundColor);

                        tileMapLayer(TileLevel.CreatureDecoration)[ViewRelative(creature.LocationMap)].TileSprite = creature.GameOverlaySprite;

                        tileMapLayer(TileLevel.CreatureStatus)[ViewRelative(creature.LocationMap)] = new TileEngine.TileCell(overlapSprite);

                        tileMapLayer(TileLevel.CreatureCover)[ViewRelative(creature.LocationMap)] = new TileEngine.TileCell(coverSprite);
                        tileMapLayer(TileLevel.CreatureCover)[ViewRelative(creature.LocationMap)].Transparency = coverTransparency;

                        tileMapLayer(TileLevel.CreatureTarget)[ViewRelative(creature.LocationMap)] = new TileEngine.TileCell(targetSprite);

                        if(creature.Level != 0 && !(creature is Creatures.ArenaMaster))
                            tileMapLayer(TileLevel.CreatureLevel)[ViewRelative(creature.LocationMap)] = new TileEngine.TileCell("monster_level_" + creature.Level);

                        if (creature.HasAnimation)
                            tileMapLayer(TileLevel.Creatures)[ViewRelative(creature.LocationMap)].RecurringAnimation = creature.GetAnimation();
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
            //DrawFrame(mapTopLeftBase.x - 1, mapTopLeftBase.y - 1, widthAvail + 3, heightAvail + 3, false, mapFrameColor);

            //Draw frame for msg here too
            //DrawFrame(msgDisplayTopLeft.x - 1, msgDisplayTopLeft.y - 1, msgDisplayBotRight.x - msgDisplayTopLeft.x + 3, msgDisplayBotRight.y - msgDisplayTopLeft.y + 3, false, statsFrameColor);

            //Put the map in the centre
            //mapTopLeft = new Point(mapTopLeftBase.x + (widthAvail - width) / 2, mapTopLeftBase.y + (heightAvail - height) / 2);
            //not appropriate with viewport approach

            for (int i = 0; i < map.width; i++)
            {
                for (int j = 0; j < map.height; j++)
                {
                    int screenX = mapTopLeftBase.x + i;
                    int screenY = mapTopLeftBase.y + j;

                    char screenChar;
                    System.Drawing.Color baseDrawColor;
                    System.Drawing.Color drawColor;
                    double spriteTransparency = 1.0;
                    bool drawSquare = true;

                    string effectSprite = null;
                    double effectTransparency = 1.0;

                    //Defaults
                    screenChar = StringEquivalent.TerrainChars[map.mapSquares[i, j].Terrain];
                    string terrainSprite = null;
                    StringEquivalent.TerrainSprites.TryGetValue(map.mapSquares[i, j].Terrain, out terrainSprite);
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

                            List<System.Drawing.Color> colors = new List<System.Drawing.Color>(new System.Drawing.Color[] { System.Drawing.Color.Yellow, System.Drawing.Color.Gold, System.Drawing.Color.RosyBrown, orangeDisactivatedColor, System.Drawing.Color.LightGray, System.Drawing.Color.Gray });

                            int roomId = map.roomIdMap[i, j];

                            int numberToDraw = roomId % 10;
                            int colorIndex = roomId / 10;

                            if (numberToDraw == -1)
                            {
                                screenChar = 'n';
                                baseDrawColor = System.Drawing.Color.DarkGray;
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
 
                    if (map.mapSquares[i, j].InPlayerFOV || SeeAllMap)
                    {
                        //In FOV or in town
                        drawColor = baseDrawColor;
                    }
                    else if (map.mapSquares[i, j].SeenByPlayer)
                    {
                        //Not in FOV but seen
                        drawColor = ColorInterpolate(baseDrawColor, System.Drawing.Color.Black, 0.6);
                        spriteTransparency = 0.5;
                    }
                    else
                    {
                        //Never in FOV
                        if (DebugMode)
                        {
                            drawColor = ColorInterpolate(baseDrawColor, System.Drawing.Color.Black, 0.7);
                            spriteTransparency = 0.7;
                        }
                        else
                        {
                            drawColor = hiddenColor;
                            drawSquare = false;
                        }
                    }

                    //Monster FOV in debug mode
                    if (DebugMode)
                    {
                        //Draw player FOV explicitally
                        if (map.mapSquares[i, j].InPlayerFOV)
                        {
                            drawColor = ColorInterpolate(baseDrawColor, System.Drawing.Color.Blue, 0.6);
                            effectSprite = "greentarget";
                            effectTransparency = 0.5;
                        }

                        //Draw monster FOV
                        if (map.mapSquares[i, j].InMonsterFOV)
                        {
                            drawColor = ColorInterpolate(baseDrawColor, System.Drawing.Color.Red, 0.6);
                            effectSprite = "redtarget";
                            effectTransparency = 0.5;
                        }

                        //Draw monster stealth radius
                        if (map.mapSquares[i, j].InMonsterStealthRadius)
                        {
                            drawColor = ColorInterpolate(baseDrawColor, System.Drawing.Color.OrangeRed, 0.6);
                            effectSprite = "orangetarget";
                            effectTransparency = 0.5;
                        }

                        //Draw sounds
                        if (map.mapSquares[i, j].SoundMag > 0.0001)
                        {
                            effectSprite = "sound";
                            effectTransparency = map.mapSquares[i, j].SoundMag;
                            drawColor = ColorInterpolate(baseDrawColor, System.Drawing.Color.Yellow, map.mapSquares[i, j].SoundMag);
                        }
                    }

                    if (Game.Dungeon.Player.IsEffectActive(typeof(PlayerEffects.SeeFOV)) && map.mapSquares[i, j].InMonsterFOV)
                    {
                        drawColor = ColorInterpolate(baseDrawColor, System.Drawing.Color.Green, 0.7);
                    }

                    Point mapTerrainLoc = new Point(i, j);

                    if (isViewVisible(mapTerrainLoc) && drawSquare)
                    {
                        
                        tileMapLayer(TileLevel.Terrain)[ViewRelative(mapTerrainLoc)] = new TileEngine.TileCell(terrainSprite);
                        tileMapLayer(TileLevel.Terrain)[ViewRelative(mapTerrainLoc)].TileID = screenChar;
                        tileMapLayer(TileLevel.Terrain)[ViewRelative(mapTerrainLoc)].TileFlag = new LibtcodColorFlags(drawColor);
                        tileMapLayer(TileLevel.Terrain)[ViewRelative(mapTerrainLoc)].Transparency = spriteTransparency;
                        
                        tileMapLayer(TileLevel.TerrainEffects)[ViewRelative(mapTerrainLoc)] = new TileEngine.TileCell(effectSprite);
                        tileMapLayer(TileLevel.TerrainEffects)[ViewRelative(mapTerrainLoc)].Transparency = effectTransparency;
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
        internal void PrintMessage(string message, System.Drawing.Color color)
        {
            //Update state
            lastMessage = message;

            //Display new message
            DrawTextWidth(message, msgDisplayTopLeft, 800, color);
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
            ClearRect(topLeft.x, topLeft.y, width, 1);

            //Display new message
            DrawTextWidth(message, topLeft, width, System.Drawing.Color.White);
        }

        void ClearMessageBar()
        {

            //ClearRect(msgDisplayTopLeft.x, msgDisplayTopLeft.y, msgDisplayBotRight.x - msgDisplayTopLeft.x - 1, msgDisplayBotRight.y - msgDisplayTopLeft.y - 1);
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
        /// Draw the screen and run the message queue
        /// </summary>
        public void Update(int tickIncrement)
        {
            Boolean animationUpdate = false;

            //Expire any temporary animations
            animationUpdate = UpdateAnimations(tickIncrement);

            //Rerender tiles if user events has occurred or an animation has expired
            if (NeedsUpdate || animationUpdate)
            {
                //Draw screen 
                Draw();

                FlushConsole();
            }
        }

        public bool UpdateAnimations(int tickIncrement)
        {
            //May be called before map made
            if (tileMap == null)
                return false;

            var animationChangeOccurred = false;
            var animationLayer = tileMapLayer(TileLevel.Animations);

            for (int i = 0; i < tileMap.Rows; i++)
            {
                for (int j = 0; j < tileMap.Columns; j++)
                {
                    TileEngine.TileCell thisCell = animationLayer.Rows[i].Columns[j];

                    if (!thisCell.IsPresent())
                        continue;

                    TileEngine.Animation cellAnimation = thisCell.Animation;

                    if (cellAnimation == null)
                    {
                        LogFile.Log.LogEntryDebug("Cell animation at row: " + i + " column " + j + "is null", LogDebugLevel.High);
                        continue;
                    }

                    cellAnimation.CurrentFrame += tickIncrement;
                    if (cellAnimation.CurrentFrame > cellAnimation.DelayMS)
                    {
                        cellAnimation.Displayed = true;
                        animationChangeOccurred = true;
                    }

                    if (!cellAnimation.Displayed)
                        continue;

                    if (cellAnimation.CurrentFrame > cellAnimation.DurationMS + cellAnimation.DelayMS)
                    {
                        thisCell.Reset();
                        animationChangeOccurred = true;
                    }
                }
            }

            foreach (Monster m in Game.Dungeon.Monsters)
            {
                var animationChanged = m.IncrementAnimation(tickIncrement);
                if (animationChanged)
                    animationChangeOccurred = true;
            }

            return animationChangeOccurred;

        }

        private void DrawPrompt()
        {
            ClearMessageLine();
            PrintMessage(Prompt, promptColor);
        }

        public void ClearPrompt()
        {
            Prompt = null;
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
        internal void DrawMissileAttack(Creature originCreature, Creature target, CombatResults result, System.Drawing.Color color)
        {
            if (!CombatAnimations)
                return;

            //Check that the player can see the action

            MapSquare creatureSquare = Game.Dungeon.Levels[originCreature.LocationLevel].mapSquares[originCreature.LocationMap.x, originCreature.LocationMap.y];
            MapSquare targetSquare = Game.Dungeon.Levels[target.LocationLevel].mapSquares[target.LocationMap.x, target.LocationMap.y];

            if (!creatureSquare.InPlayerFOV && !targetSquare.InPlayerFOV)
                return;
            
            //Draw animation to animation layer

            //Calculate and draw the line overlay
            List<Point> thisLineSquares = Game.Dungeon.GetPathLinePoints(originCreature.LocationMap, target.LocationMap);
            DrawAreaAttackAnimation(thisLineSquares, AttackType.Bullet, true);    
            //DrawPathLine(TileLevel.Animations, originCreature.LocationMap, target.LocationMap, color, System.Drawing.Color.Black);

            //Flash the target if they were damaged
            //Draw them in either case so that we overwrite the missile animation on the target square with the creature

            if (targetSquare.InPlayerFOV)
            {
                System.Drawing.Color colorToDraw = System.Drawing.Color.Red;

                if (result == CombatResults.DefenderDamaged || result == CombatResults.DefenderDied)
                {
                    
                }
                else
                {
                    colorToDraw = target.RepresentationColor();
                }
                /*
                if (isViewVisible(target.LocationMap))
                {
                    tileMapLayer(TileLevel.Animations)[ViewRelative(target.LocationMap)] = new TileEngine.TileCell(target.Representation);
                    tileMapLayer(TileLevel.Animations)[ViewRelative(target.LocationMap)].TileFlag = new LibtcodColorFlags(colorToDraw);
                    tileMapLayer(TileLevel.Animations)[ViewRelative(target.LocationMap)].Animation = new TileEngine.Animation(combationAnimationFrameDuration);

                }*/
            }
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

            //Flash the attacked creature
            //Add flash to animation layer

            //need to add a melee splash

            if (targetSquare.InPlayerFOV)
            {
                /*
                if (result == CombatResults.DefenderDamaged || result == CombatResults.DefenderDied)
                {
                    if (isViewVisible(newTarget.LocationMap))
                    {
                        tileMapLayer(TileLevel.Animations)[ViewRelative(newTarget.LocationMap)] = new TileEngine.TileCell(newTarget.GameSprite);
                        tileMapLayer(TileLevel.Animations)[ViewRelative(newTarget.LocationMap)].TileFlag = new LibtcodColorFlags(System.Drawing.Color.Red);
                        tileMapLayer(TileLevel.Animations)[ViewRelative(newTarget.LocationMap)].Animation = new TileEngine.Animation(combationAnimationFrameDuration);
                    }
                }*/
            }
        }


        internal void ResetViewPanel()
        {
            if (!DebugMode)
            {
                Screen.Instance.CreatureToView = null;
                Screen.Instance.ItemToView = null;
                Screen.Instance.FeatureToView = null;
            }
        }

        internal void SetPrompt(string p)
        {
            Prompt = p;
        }

        internal void UpdateAnimations()
        {
            throw new NotImplementedException();
        }



        public bool EndOfGameQuit { get; set; }

        public bool EndOfGameWon { get; set; }

        public bool ShowMessageQueue { get; set; }

        public RogueBasin.Point PixelToCoord(System.Drawing.Point point)
        {
            var x = point.X / ScaledSpriteSize + viewTL.x;
            var y = point.Y / ScaledSpriteSize + viewTL.y;

            return new Point(x, y);
        }
    }

    static class ScreenExtensionMethods
    {
        public static System.Drawing.Point Add(this System.Drawing.Point p1, System.Drawing.Point p2)
        {
            return new System.Drawing.Point(p1.X + p2.X, p1.Y + p2.Y);
        }
    }
}
