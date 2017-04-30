using System;

namespace RogueBasin
{

    public struct ActionResult
    {
        public readonly bool timeAdvances;
        public readonly bool centreOnPC;

        public ActionResult(bool timeAdvances, bool centreOnPC)
        {
            this.timeAdvances = timeAdvances;
            this.centreOnPC = centreOnPC;
        }
    }

    public enum ActionState
    {
        Running,
        Interactive
    }

    public class RogueBase
    {

        private InputEvents events;
        private GameTick gameTick;
        private InputHandler inputHandler;
        private Targetting targetting;
        private Running running;
        private PlayerActions playerActions;
        private GameActions gameActions;
        private SystemActions systemActions;

        //TODO: Put these in a configuration class
        public bool PlaySounds { get; set; }
        public bool PlayMusic { get; set; }
        private Boolean FunMode { get; set; }
        
        public bool GameStarted { get; set; }
        
        public InputHandler InputHandler {  get { return inputHandler; } }
        public InputEvents Events { get { return events; } }
        public SystemActions SystemActions { get { return systemActions; } }
        public Running Running { get { return running; } }

        public RogueBase()
        {

        }

        private void InitialiseGame(Dungeon dungeon)
        {
            //Setup input-related classes via dependency injection

            targetting = new Targetting(dungeon, dungeon.Player);
            running = new Running(dungeon.Player);

            playerActions = new PlayerActions(running, targetting);
            gameActions = new GameActions();
            systemActions = new SystemActions();
            inputHandler = new InputHandler(running, targetting, playerActions, gameActions, systemActions);
            gameTick = new GameTick(inputHandler);
            events = new InputEvents(gameTick, inputHandler);

            //This late circular injection could probably be fixed by refactoring out of InputHandler 
            //a smaller class for manipulating the input state
            //These classes all need to manipulate the current input state
            running.SetInputHandler(inputHandler);
            systemActions.SetInputHandler(inputHandler);
            targetting.SetInputHandler(inputHandler);

            events.SetupSDLDotNetEvents();
        }

        public void SetupSystem()
        {
            //Initial setup
            //See all debug messages
            LogFile.Log.DebugLevel = 0;

            //Load config
            Game.Config = new Config("config.txt");

            //Setup screen
            Screen.Instance.InitialSetup();

            //Setup global random
            Random rand = new Random();

            //Setup logfile
            try
            {
                LogFile.Log.LogEntry("Starting log file");
            }
            catch (Exception e)
            {
                Screen.Instance.ConsoleLine("Error creating log file: " + e.Message);
            }

            //Setup message queue
            Game.MessageQueue = new MessageQueue();

            SetupFromConfig();
        }

        public void SetupGameWithNewDungeon()
        {
            var dungeon = new Dungeon();
            InitialiseGame(dungeon);

            dungeon.Difficulty = GameDifficulty.Medium;
            dungeon.Player.Name = "A name";
            dungeon.Player.PlayItemMovies = true;

            dungeon.AllLocksOpen = false;

            if (Game.Config.DebugMode)
                dungeon.PlayerImmortal = true;

            Game.Dungeon = dungeon;
        }


        private void SetupFromConfig()
        {
            if(Game.Config.Sound)
            {   
                PlaySounds = true;
            }

            if(Game.Config.Music)
                PlayMusic = true;
        }


        public void InitializeScreen()
        {
            Screen.Instance.NeedsUpdate = true;
            Screen.Instance.CenterViewOnPoint(Game.Dungeon.Player.LocationLevel, Game.Dungeon.Player.LocationMap);
        }

        public void StartGame()
        {
            Game.Dungeon.Player.StartGameSetup();

            GameStarted = true;
            Screen.Instance.ShowMessageQueue = true;

            Game.Dungeon.CalculatePlayerFOV();
            Screen.Instance.CenterViewOnPoint(Game.Dungeon.Player.LocationLevel, Game.Dungeon.Player.LocationMap);

            Screen.Instance.NeedsUpdate = true;
        }
    }
}
