using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Massacre.Components;
using Massacre.Components.Controllers;
using Massacre.Components.Map;
using Massacre.Components.Players;
using Massacre.KeyboardCustom;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Massacre.Desktop {
    
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MassacreGame : Game {
        
        #region Globals
        
        private TcpClient _tcpClient;
        
        private const string _HOST = "127.0.0.1";   // address of the server
        private const int _PORT = 8080;             // port that server listens to
        
        private Task _serverComunicationTask; // task to receive stream from server
        
        private KeyboardStateCustom _keyboardState = new KeyboardStateCustom( );
        
        private string _actionLog; // all tasks received from server are gonne be writen here
    
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        
        private string _gameState = "MainMenu"; // game state - StartingMenu, GameCreation, Game, About, etc
        
        private Component _focusedItem;
        
        private Dictionary<string, Texture2D> _blueKnightTextures = new Dictionary<string, Texture2D>( );
        private Dictionary<string, Texture2D> _redKnightTextures = new Dictionary<string, Texture2D>( );
        private Dictionary<string, Texture2D> _bannerTextures = new Dictionary<string, Texture2D>( );
        
        private Dictionary<string, List<Component>> _gameComponents = new Dictionary<string, List<Component>> {
        // container for all components on the page
            { "MainMenu",   new List<Component>( ) },    // Done
            { "JoinGame",   new List<Component>( ) },    // Done
            { "CreateGame", new List<Component>( ) },    // Done
            { "Credentials",    new List<Component>( ) },    // Done
            { "Main",       new List<Component>( ) },    // Done
            { "ErrorPage",  new List<Component>( ) },    // Done
        };
        
        private const int _WINDOWHEIGHT = 560;
        private const int _WINDOWWIDTH = 1000;
        
        private const int _GROUNDHEIGHT = 120;
        
        private int[] map;
        
        private const int _KNIGHTSIZE = 48;
        
        private long lastTimeCoorded = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond; 
                    // will be updated each <interval> indicates if knights need to refresh their possition
        private int coordingInterval = 200; // interval in ms how often to check if knights possition is right
        
        private string _myName = "";    // user name, name of the knight the user controls
        private string _gameName = "";  // name of the current game
        
        private SoundEffectInstance _menuMusic; // medieval background music
        private SoundEffect _swordSound;        // sword sound effect
        
        #endregion
       
         
        #region UI MonoGame

        public MassacreGame( ) {
        
            graphics = new GraphicsDeviceManager( this );
            Content.RootDirectory = "Content";
            
            _serverComunicationTask = new Task( ReseiveNetworkStream ); // receives data from the server
            _actionLog = ""; // Log that is parsed eahc time game updates if there's new tasks
        
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize( ) {
        
            IsMouseVisible = true;
            
            Window.TextInput += TextInputHandler; 

            graphics.PreferredBackBufferWidth = 1000;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = 560;   // set this value to the desired height of your window
            graphics.ApplyChanges();

            base.Initialize( );
        
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent( ) {
        
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch( GraphicsDevice );
            
            // Controllers
            Texture2D buttonTexture = Content.Load<Texture2D>( "Components/Buttons/MenuButton" );
            Texture2D buttonHovered = Content.Load<Texture2D>( "Components/Buttons/MenuButtonHovered" );
            Texture2D buttonPressed = Content.Load<Texture2D>( "Components/Buttons/MenuButtonPressed" );
            
            // Map components
            Texture2D skyTexture = Content.Load<Texture2D>( "Components/Map/Landscape/Sky" );
            Texture2D groundInTexture = Content.Load<Texture2D>( "Components/Map/Landscape/dirtfull" );
            Texture2D gorundLeftTexture = Content.Load<Texture2D>( "Components/Map/Landscape/grasstopleft" );
            Texture2D groundRigthTexture = Content.Load<Texture2D>( "Components/Map/Landscape/grasstopright" );
            Texture2D groundTopTexture = Content.Load<Texture2D>( "Components/Map/Landscape/grasstop" );
            
            // Banners
            _bannerTextures.Add( "BlueFlag", Content.Load<Texture2D>( "Components/Map/Banners/BlueFlag" ) );
            _bannerTextures.Add( "BlueFlagC", Content.Load<Texture2D>( "Components/Map/Banners/BlueFlagC" ) );
            _bannerTextures.Add( "RedFlag", Content.Load<Texture2D>( "Components/Map/Banners/RedFlag" ) );
            _bannerTextures.Add( "RedFlagC", Content.Load<Texture2D>( "Components/Map/Banners/RedFlagC" ) );
            
            // Blue Knight
            _blueKnightTextures.Add( "StandLeft", Content.Load<Texture2D>( "Components/Map/Knights/Blue/StandLeft" ) );
            _blueKnightTextures.Add( "StandRight", Content.Load<Texture2D>( "Components/Map/Knights/Blue/StandRight" ) );
            
            _blueKnightTextures.Add( "MeleeLeft1", Content.Load<Texture2D>( "Components/Map/Knights/Blue/MeleLeft1" ) );
            _blueKnightTextures.Add( "MeleeLeft2", Content.Load<Texture2D>( "Components/Map/Knights/Blue/MeleLeft2" ) );
            _blueKnightTextures.Add( "MeleeRight1", Content.Load<Texture2D>( "Components/Map/Knights/Blue/MeleRight1" ) );
            _blueKnightTextures.Add( "MeleeRight2", Content.Load<Texture2D>( "Components/Map/Knights/Blue/MeleRight2" ) );
            
            _blueKnightTextures.Add( "WalkLeft1", Content.Load<Texture2D>( "Components/Map/Knights/Blue/WalkLeft1" ) );
            _blueKnightTextures.Add( "WalkLeft2", Content.Load<Texture2D>( "Components/Map/Knights/Blue/WalkLeft2" ) );
            _blueKnightTextures.Add( "WalkLeft3", Content.Load<Texture2D>( "Components/Map/Knights/Blue/WalkLeft3" ) );
            _blueKnightTextures.Add( "WalkLeft4", Content.Load<Texture2D>( "Components/Map/Knights/Blue/WalkLeft4" ) );
            
            _blueKnightTextures.Add( "WalkRight1", Content.Load<Texture2D>( "Components/Map/Knights/Blue/WalkRight1" ) );
            _blueKnightTextures.Add( "WalkRight2", Content.Load<Texture2D>( "Components/Map/Knights/Blue/WalkRight2" ) );
            _blueKnightTextures.Add( "WalkRight3", Content.Load<Texture2D>( "Components/Map/Knights/Blue/WalkRight3" ) );
            _blueKnightTextures.Add( "WalkRight4", Content.Load<Texture2D>( "Components/Map/Knights/Blue/WalkRight4" ) );
            
            // Red Knight
            _redKnightTextures.Add( "StandLeft", Content.Load<Texture2D>( "Components/Map/Knights/Red/StandLeft" ) );
            _redKnightTextures.Add( "StandRight", Content.Load<Texture2D>( "Components/Map/Knights/Red/StandRight" ) );
            
            _redKnightTextures.Add( "MeleeLeft1", Content.Load<Texture2D>( "Components/Map/Knights/Red/MeleLeft1" ) );
            _redKnightTextures.Add( "MeleeLeft2", Content.Load<Texture2D>( "Components/Map/Knights/Red/MeleLeft2" ) );
            _redKnightTextures.Add( "MeleeRight1", Content.Load<Texture2D>( "Components/Map/Knights/Red/MeleRight1" ) );
            _redKnightTextures.Add( "MeleeRight2", Content.Load<Texture2D>( "Components/Map/Knights/Red/MeleRight2" ) );
            
            _redKnightTextures.Add( "WalkLeft1", Content.Load<Texture2D>( "Components/Map/Knights/Red/WalkLeft1" ) );
            _redKnightTextures.Add( "WalkLeft2", Content.Load<Texture2D>( "Components/Map/Knights/Red/WalkLeft2" ) );
            _redKnightTextures.Add( "WalkLeft3", Content.Load<Texture2D>( "Components/Map/Knights/Red/WalkLeft3" ) );
            _redKnightTextures.Add( "WalkLeft4", Content.Load<Texture2D>( "Components/Map/Knights/Red/WalkLeft4" ) );
            
            _redKnightTextures.Add( "WalkRight1", Content.Load<Texture2D>( "Components/Map/Knights/Red/WalkRight1" ) );
            _redKnightTextures.Add( "WalkRight2", Content.Load<Texture2D>( "Components/Map/Knights/Red/WalkRight2" ) );
            _redKnightTextures.Add( "WalkRight3", Content.Load<Texture2D>( "Components/Map/Knights/Red/WalkRight3" ) );
            _redKnightTextures.Add( "WalkRight4", Content.Load<Texture2D>( "Components/Map/Knights/Red/WalkRight4" ) );
            
            // Fonts
            SpriteFont font = Content.Load<SpriteFont>( "Fonts/OldeEnglish" );
            
            //Audios
            _swordSound = Content.Load<SoundEffect>( "Audio/Sword" );
            _menuMusic = Content.Load<SoundEffect>( "Audio/Medieval" ).CreateInstance();
            _menuMusic.IsLooped = true;
            _menuMusic.Play( );
            
            _gameComponents[ "MainMenu" ] = MainMenuItems( buttonTexture, buttonHovered, buttonPressed, font );
            _gameComponents[ "Credentials" ] = CredentialsItems( buttonTexture, buttonHovered, buttonPressed, font );
            _gameComponents[ "CreateGame" ] = CreateGameItems( buttonTexture, buttonHovered, buttonPressed, font );
            _gameComponents[ "JoinGame" ] = JoinGameItems( buttonTexture, buttonHovered, buttonPressed, font );
            _gameComponents[ "Main" ] = MainItems( buttonTexture, buttonHovered, buttonPressed, font, skyTexture, groundInTexture, gorundLeftTexture, groundRigthTexture, groundTopTexture);
            

        }
        
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent( ) {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update( GameTime gameTime ) {
    
            // refreshes knights possition if it's time
            long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if ( _gameState == "Main" && now - lastTimeCoorded >= coordingInterval ) {
                
                lastTimeCoorded = now;
                
                Vector2 coords = MyCoords( );
                
                string data = "coordinate=>" + coords.X + "&&" + coords.Y;
                
                RequestAsimetric( data ); 
            
            }
            
            // if actionLog is not empty - new tasks received. 
            string[] actions = new string[ 0 ];
            if ( _actionLog.Length > 0 ) {
            
                // all knight tasks received
                actions = _actionLog.Split( '/' );                
                 
                _actionLog = "";
                 
            }
            
            // applies all tasks received to the UI
            try{
                foreach ( Component component in _gameComponents[ _gameState ] ) {
                    
                    if ( component is Knight knight )
                    
                        foreach ( string action in actions )
                        
                            if ( knight.Name == action.Split( ':' )[ 0 ] )
                            
                                knight.State = action.Split( ':' )[ 1 ];
                
                    component.Update( gameTime );
                    
                }
                
            } catch ( InvalidOperationException exc ) { }
            
            
            // if any controlling button's pressed, tell server about that
            KeyboardState key = Keyboard.GetState( );
            
            if ( _gameState == "Main" ) {
            
                if ( _keyboardState.IsNew( key ) ) {
                
                    if ( key.IsKeyDown( Keys.A ) )
                        RequestAsimetric( "action=>Left" );
                        
                    else if ( key.IsKeyDown( Keys.W ) )
                        RequestAsimetric( "action=>Up" );
                    
                    else if ( key.IsKeyDown( Keys.S ) )
                        RequestAsimetric( "action=>Down" );
                    
                    else if ( key.IsKeyDown( Keys.D ) )
                        RequestAsimetric( "action=>Right" );
                    
                    else if ( key.IsKeyDown( Keys.Space ) ) {
                        RequestAsimetric( "action=>Hit" );
                        TryToKill( );
                        
                    } else
                        RequestAsimetric( "action=>Released" );
                        
                }
            }
                    
            base.Update( gameTime );
            
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw( GameTime gameTime ) {
            
            // Background black
            GraphicsDevice.Clear( Color.Black );
            
            spriteBatch.Begin( );
            
            // draw everything that is in current game state 
            foreach ( var component in _gameComponents[ _gameState ] )
                    component.Draw( gameTime, spriteBatch );

            spriteBatch.End( );

            base.Draw( gameTime );
            
        }
        
        #endregion
        
        
        #region UI generating / degenerating
        
        // Generate list of Components that are gonna be drawn as main menu
        private List<Component> MainMenuItems( Texture2D buttonTexture, Texture2D buttonHovered, Texture2D buttonPressed, SpriteFont font ) { 
            
            Button joinGameButton = new Button( buttonTexture, buttonHovered, buttonPressed, font ) {
                
                Position = new Vector2( 740, 20 ),
                Size = new Vector2( 250, 72 ),
                Text = "Join Game",
                
            };
               
            Button newGameButton = new Button( buttonTexture, buttonHovered, buttonPressed, font ) {
                
                Position = new Vector2( 740, 112 ),
                Size = new Vector2( 250, 72 ),
                Text = "New Game",
                
            };
            
            Button credentialsButton = new Button( buttonTexture, buttonHovered, buttonPressed, font ) {
                
                Position = new Vector2( 740, 204 ),
                Size = new Vector2( 250, 72 ),
                Text = "Credentials",
                
            };
                        
            Button exitButton = new Button( buttonTexture, buttonHovered, buttonPressed, font ) {
                
                Position = new Vector2( 740, 296 ),
                Size = new Vector2( 250, 72 ),
                Text = "Exit",
                
            };            
            
            Label welcomeLabel = new Label( font ) {
            
                PenColour = Color.White,
                Position = new Vector2( 300, 100 ),
                Text = "Welcome To The Massacre!",
            
            };
            
            joinGameButton  .Click += JoinGame_Click;
            newGameButton   .Click += NewGame_Click;
            credentialsButton   .Click += Credentials_Click;
            exitButton      .Click += ExitApp;
            
            return new List<Component> {
                newGameButton,
                joinGameButton,
                credentialsButton,
                exitButton,
                welcomeLabel,
            };

        }
        
        // Generate list of Components that are gonna be drawn as credentials info
        private List<Component> CredentialsItems ( Texture2D buttonTexture, Texture2D buttonHovered, Texture2D buttonPressed, SpriteFont font ) {
        
            Button backButton = new Button( buttonTexture, buttonHovered, buttonPressed, font ) {
            
                Position = new Vector2( 20, 20 ),
                Size = new Vector2( 144, 72 ),
                Text = "Back"
            
            };
            
            Label authorLabel = new Label( font ) {
            
                Position = new Vector2( 500, 260 ),
                Text = "Created by Me =)"
            
            };
            
            backButton.Click += BackToMainMenu;
            
            return new List<Component> {
                
                backButton,
                authorLabel
                
            };


        }
        
        // Generate list of Components that are gonna be drawn as create game interface
        private List<Component> CreateGameItems ( Texture2D buttonTexture, Texture2D buttonHovered, Texture2D buttonPressed, SpriteFont font ) {
        
            Button backButton = new Button( buttonTexture, buttonHovered, buttonPressed, font ) {
                
                Position = new Vector2( 20, 20),
                Size = new Vector2( 144, 72 ),
                Text = "Back",
                
            };
            
            Input userNameInput  = new Input( buttonTexture, buttonPressed, font, "Your Name" ) {
            
                Position = new Vector2( 640, 100 ),
                Size = new Vector2 ( 340, 72 ),
                PaddingLeft = 32,
            
            };
            
            Input gameNameInput  = new Input( buttonTexture, buttonPressed, font, "Game Name" ) {
            
                Position = new Vector2( 640, 200 ),
                Size = new Vector2 ( 340, 72 ),
                PaddingLeft = 32,
            
            };
            
            Button createGameButton = new Button( buttonTexture, buttonHovered, buttonPressed, font ) {
                
                Position = new Vector2( 720, 300),
                Size = new Vector2( 144, 72 ),
                Text = "Create",
                
            };
            
            Label errorLabel = new Label( font ) {
            
                PenColour = Color.White,
                Position = new Vector2( 300, 280 ),
                Text = "",
            
            };
            
            backButton      .Click += BackToMainMenu_Click;
            userNameInput   .Click += InputField_Click;
            gameNameInput   .Click += InputField_Click;
            createGameButton.Click += CreateGameButton_Click;
            
            return new List<Component> {
                
                backButton,
                userNameInput,
                gameNameInput,
                createGameButton,
                errorLabel,
                
            };
        
        }
        
        // Generate list of Components that are gonna be drawn as join game interface
        private List<Component> JoinGameItems ( Texture2D buttonTexture, Texture2D buttonHovered, Texture2D buttonPressed, SpriteFont font ) {
        
            Button backButton = new Button( buttonTexture, buttonHovered, buttonPressed, font ) {
                
                Position = new Vector2( 20, 20),
                Size = new Vector2( 144, 72 ),
                Text = "Back",
                
            };
            
            Input userNameInput  = new Input( buttonTexture, buttonPressed, font, "Your Name" ){
            
                Position = new Vector2( 640, 100 ),
                Size = new Vector2 ( 340, 72 ),
                PaddingLeft = 32,
            
            };
            
            Input gameNameInput  = new Input( buttonTexture, buttonPressed, font, "Game Name" ){
            
                Position = new Vector2( 640, 200 ),
                Size = new Vector2 ( 340, 72 ),
                PaddingLeft = 32,
            
            };
            
            Button joinGameButton = new Button( buttonTexture, buttonHovered, buttonPressed, font ) {
                
                Position = new Vector2( 720, 300),
                Size = new Vector2( 144, 72 ),
                Text = "Join",
                
            };
            
            Label errorLabel = new Label( font ) {
            
                PenColour = Color.White,
                Position = new Vector2( 300, 280 ),
                Text = "",
            
            };
            
            backButton      .Click += BackToMainMenu_Click;
            userNameInput   .Click += InputField_Click;
            gameNameInput   .Click += InputField_Click;
            joinGameButton  .Click += JoinGameButton_Click;
            
            return new List<Component> { 
            
                backButton,
                userNameInput,
                gameNameInput,
                joinGameButton,
                errorLabel,
            
            };
        
        }
        
        // Generate list of Components that are gonna be drawn as main game interface
        private List<Component> MainItems ( Texture2D buttonTexture, Texture2D buttonHovered, Texture2D buttonPressed, SpriteFont font, Texture2D skyTexture, Texture2D groundDirtfull, Texture2D groundLeftTop, Texture2D groundRightTop, Texture2D groundTop ) {
        
            Button escButton = new Button( buttonTexture, buttonHovered, buttonPressed, font ) {
                
                Position = new Vector2( 20, 20),
                Size = new Vector2( 100, 72 ),
                Text = "esc",
                
            };
            
            SkyAndGround sky = new SkyAndGround( skyTexture ) {
                
                Position = new Vector2( 0, 0 ),
                Size = new Vector2( _WINDOWWIDTH, _WINDOWHEIGHT - _GROUNDHEIGHT )
            
            };
            
            #region Ground 
            
            SkyAndGround ground1 = new SkyAndGround( groundTop ){ //left - 100
            
                Position = new Vector2( 0, _WINDOWHEIGHT - 2 *_GROUNDHEIGHT ),
                Size = new Vector2( _WINDOWWIDTH / 8, _GROUNDHEIGHT )
            
            };
            SkyAndGround ground2 = new SkyAndGround( groundDirtfull ){ //left - 100
            
                Position = new Vector2( 0, _WINDOWHEIGHT - _GROUNDHEIGHT ),
                Size = new Vector2( _WINDOWWIDTH / 8, _GROUNDHEIGHT )
            
            };
            SkyAndGround ground3 = new SkyAndGround( groundTop ){ //left - 200
            
                Position = new Vector2( _WINDOWWIDTH / 8, _WINDOWHEIGHT - _GROUNDHEIGHT ),
                Size = new Vector2( _WINDOWWIDTH / 8, _GROUNDHEIGHT )
                
            };
            SkyAndGround ground4 = new SkyAndGround( groundTop ){ //left - 300
            
                Position = new Vector2( 2 * _WINDOWWIDTH / 8, _WINDOWHEIGHT - _GROUNDHEIGHT ),
                Size = new Vector2( _WINDOWWIDTH / 8, _GROUNDHEIGHT )
                
            };
            SkyAndGround ground5 = new SkyAndGround( groundTop ){ //left - 400
            
                Position = new Vector2( 3 * _WINDOWWIDTH / 8, _WINDOWHEIGHT - _GROUNDHEIGHT ),
                Size = new Vector2( _WINDOWWIDTH / 8, _GROUNDHEIGHT )
                
            };
            SkyAndGround ground6 = new SkyAndGround( groundTop ){ //left - 500
            
                Position = new Vector2( 4 * _WINDOWWIDTH / 8, _WINDOWHEIGHT - _GROUNDHEIGHT ),
                Size = new Vector2( _WINDOWWIDTH / 8, _GROUNDHEIGHT )
                
            };
            SkyAndGround ground7 = new SkyAndGround( groundTop ){ //left - 500
            
                Position = new Vector2( 5 * _WINDOWWIDTH / 8, _WINDOWHEIGHT - _GROUNDHEIGHT ),
                Size = new Vector2( _WINDOWWIDTH / 8, _GROUNDHEIGHT )
                
            };
            SkyAndGround ground8 = new SkyAndGround( groundTop ){ //left - 700
            
                Position = new Vector2( 6 * _WINDOWWIDTH / 8, _WINDOWHEIGHT - _GROUNDHEIGHT ),
                Size = new Vector2( _WINDOWWIDTH / 8, _GROUNDHEIGHT )
                
            };
            SkyAndGround ground9 = new SkyAndGround( groundDirtfull ){ 
              
                Position = new Vector2( 7 * _WINDOWWIDTH / 8, _WINDOWHEIGHT - _GROUNDHEIGHT ),
                Size = new Vector2( _WINDOWWIDTH / 8, _GROUNDHEIGHT )
                
            };
            SkyAndGround ground10 = new SkyAndGround( groundTop ){ 
             
                Position = new Vector2( 7 * _WINDOWWIDTH / 8, _WINDOWHEIGHT - 2 * _GROUNDHEIGHT ),
                Size = new Vector2( _WINDOWWIDTH / 8, _GROUNDHEIGHT )
                
            };

            #endregion

            Label scoreLabel = new Label( font ){

                Text = "",
                PenColour = Color.Black,
                Position = new Vector2( 500, 40 ),
                  
            };
            
            escButton.Click += StopGame_Click;
            
            return new List<Component> { 
            
                sky,
                ground1,
                ground2,
                ground3,
                ground4,
                ground5,
                ground6,
                ground7,
                ground8,
                ground9,
                ground10,
                escButton,
                scoreLabel,
                
            };
        
        }
        
        // Removes all knights and banners from the main view when game is over
        private void CleanUpMain( ) {
        
            int index = 0;
            int mainItemNumber = _gameComponents[ "Main" ].Count;
            
            while ( index < mainItemNumber ) {
            
                if ( _gameComponents[ "Main" ][ index ] is Knight 
                    || _gameComponents[ "Main" ][ index ] is Banner ) {
                    
                        _gameComponents["Main"].Remove( _gameComponents[ "Main" ][ index ] );
                        mainItemNumber--;
                    
                } else {
                
                    index++;
                
                }
            
            }
            
        }
         
        #endregion
        
        
        #region Onclick events
        
        // ends game
        private void StopGame_Click( object sender, EventArgs e ) {
            
            DisconnectTcpServer( );
            
            if ( _focusedItem != null )
                ( (Input)_focusedItem ).IsFocused = false;
                
            _focusedItem = null;
            
            ( (Label)_gameComponents[ "CreateGame" ][4] ).Text = ""; 
            ( (Label)_gameComponents[ "JoinGame" ][4] ).Text = "";
            
            _gameState = "MainMenu";
            
            _serverComunicationTask = new Task( ReseiveNetworkStream );
            
            CleanUpMain( );
            
            
        
        }
        
        // returns back to main menu
        private void BackToMainMenu_Click( object sender, EventArgs e ) {
            
            DisconnectTcpServer( );
            
            if ( _focusedItem != null )
                ( (Input)_focusedItem ).IsFocused = false;
            _focusedItem = null;
            
            _gameState = "MainMenu";
        
        }
        
        // joins game
        private void JoinGameButton_Click( object sender, EventArgs e ) {
        
            _myName = ( (Input)_gameComponents[ "JoinGame" ][1] ).Text;
            _gameName = ( (Input)_gameComponents[ "JoinGame" ][2] ).Text;
            JoinGameRequest( );
        
        }
        
        // sets focus on clicked input field
        private void InputField_Click( object sender, EventArgs e ) {
            
            if ( _focusedItem != null ) 
                ( (Input)_focusedItem ).IsFocused = false;
            
            _focusedItem = (Input)sender;
            
            ( (Input)_focusedItem ).IsFocused = true;
        
        }
        
        // creates new game
        private void CreateGameButton_Click( object sneder, EventArgs e ){
        
            _myName = ( (Input)_gameComponents[ "CreateGame" ][1] ).Text;
            _gameName = ( (Input)_gameComponents[ "CreateGame" ][2] ).Text;
            CreateNewGameRequest( );
        
        }
        
        // allows to create a new game
        private void NewGame_Click( object sender, EventArgs e ) {
        
            ConnectTcpServer( );           
        
            _gameState = "CreateGame";
            
        }
        
        // allows to join a game
        private void JoinGame_Click( object sender, EventArgs e) { 
        
            ConnectTcpServer( );
        
            _gameState = "JoinGame";
        
        }

        // move to credentials page
        private void Credentials_Click( object sender, EventArgs e ) { 
        
            _gameState = "Credentials";
        
        }
        
        // returns back to main menu from credentials page 
        private void BackToMainMenu( object sender, EventArgs e ) {
            
            _gameState = "MainMenu";
            
        }
        
        // exits app
        private void ExitApp ( object sender, EventArgs e ) {
        
            Exit( );
        
        }
        
        #endregion
        
        
        #region Keybord reading
        
        private void TextInputHandler ( object sender, TextInputEventArgs args ) {
            
            // input fields can anly be on CreateGame and JoinGame pages 
            if ( ( _gameState != "CreateGame" ) && ( _gameState != "JoinGame" ) )
                return;
            
            Keys pressedKey = args.Key;
            
            // removes last character of the input if backspace or delete button is pressed
            if ( ( pressedKey == Keys.Delete ) ||  ( pressedKey == Keys.Back ) ) {
                
                if ( ( (Input)_focusedItem ).Text.Length > 0 ) { 
                
                    ( (Input)_focusedItem ).Text = ( (Input)_focusedItem ).Text
                        .Remove( ( (Input)_focusedItem ).Text.Length - 1 );
                        
                }
                
                return;
            
            }
            
            // add pressed button value to the selected input element
            string pressedKeyValue = args.Character.ToString( );

            Regex gameNamePatern = new Regex( @"^[a-zA-Z0-9 ]$" );
            
            if ( ( _focusedItem is Input ) && gameNamePatern.IsMatch( pressedKeyValue ) )
                ( (Input)_focusedItem ).Text += pressedKeyValue;
        
        }
        
        #endregion
        
        
        #region TCP staff
        
        // connects to the server 
        private void ConnectTcpServer( ) {
            
            if ( _tcpClient != null ) {
            
                Console.WriteLine( "Connection is already opened." );
                
                return;
            
            } 
            
            try {
                Console.WriteLine( "TRYING TO CONNECT TO THE SERVER" );
                _tcpClient = new TcpClient( );
                Console.WriteLine( "NEW TCP CLIENT" );
                _tcpClient.Connect( _HOST, _PORT );
                
                Console.WriteLine( "Connection was established successfully." );
            
            } catch ( Exception exc ) {
            
                Console.WriteLine( "CANNOT ESTABLISH A NEW CONNECTION" );
                _tcpClient = null;
                
            }
            
            
        
        } 
        
        // disconnects from the server
        private void DisconnectTcpServer( ) {
        
            if ( _tcpClient == null ) {
                
                Console.WriteLine( "Connection is not open or already closed." );
                
                return;
                
            }
                
            try {
            
                _tcpClient.Close( );
                
            } catch ( Exception exc ) {
            } finally {
            
                _tcpClient = null;
                
            }
            
            Console.WriteLine( "Connection was closed successfully." ); 
        
        }
        
        // require the server to create a new game
        private void CreateNewGameRequest( ) { 

            // if _tcpClient is null means there's no connection made due different reasons   
            if ( _tcpClient == null ) {
            
                ( (Label)_gameComponents[ "CreateGame" ][4] ).Text =  "Connection is not open.";
                return;
                
            }
            
            // data for server
            string data = "createGame=>" + _gameName + "&&" +  _myName;
            
            // response from the server on our request
            string response = RequestSimetric( data );
            
            string responseCode = response.Split( ':' )[ 0 ];
            
            // if 200, game was created successfully and we can join it
            if ( responseCode == "200" ) {
                
                JoinGameRequest( );
                
            } else if ( responseCode == "410" ) {

                ( (Label)_gameComponents[ "CreateGame" ][4] ).Text = "OOPS: This game already exists.";

            }
        
        } 
        
        // require the server to join the game
        private void JoinGameRequest( ) {
            
            // if _tcpClient is null no requests can bу made
            if ( _tcpClient == null ) {
            
                ( (Label)_gameComponents[ "JoinGame" ][4] ).Text = "Connection is not open.";
                return;
                
            }
            
            string data = "joinGame=>" + _gameName + "&&" +  _myName;
            
            string response = RequestSimetric( data );
            
            string responseCode = response.Split( ':' )[ 0 ];
            
            if ( responseCode == "200" ) {
                
                // prepares the map
                map = LoadMap( );
                
                int[] flags = FlagsPosition( );
                
                AddBanners( flags[ 0 ], flags[ 1 ] );
                
                // gets the score
                string scoreResponse = RequestSimetric( "getScore=>" + _gameName );
                UpdateScore( scoreResponse.Substring( 1, scoreResponse.Length - 1 ) );
                
                GetPlayers( );
                
                // removes focus from the focused input element
                if ( _focusedItem != null )
                    ( (Input)_focusedItem ).IsFocused = false;
                _focusedItem = null;
                
                _gameState = "Main";
                
                // starts receiving the server data stream
                _serverComunicationTask.Start();
            
            }  else if ( responseCode == "404" ) { 

                ( (Label)_gameComponents[ "JoinGame" ][4] ).Text = "Oops: This game is not found.";
        
            } else if ( responseCode == "403" ) { 

                ( (Label)_gameComponents[ "JoinGame" ][4] ).Text = "Oops: This name is already taken.";
            
            } else if ( responseCode == "410" ) {
                
                ( (Label)_gameComponents[ "JoinGame" ][4] ).Text = "Oops: This game is already full.";
                
            }
            
        }
        
        // sends request to the server that is supposed to get a server response right away. Returns response as a string
        private string RequestSimetric( string request ) {
        
            // sending data
            NetworkStream nwStream = _tcpClient.GetStream( );
            byte[ ] bytesToSend = Encoding.ASCII.GetBytes( request );
            
            nwStream.Write( bytesToSend, 0, bytesToSend.Length );
            
            // receiving data
            byte[ ] bytesToRead = new byte[ _tcpClient.ReceiveBufferSize ];
            int bytesRead = nwStream.Read( bytesToRead, 0, _tcpClient.ReceiveBufferSize );
            
            string response = Encoding.ASCII.GetString( bytesToRead, 0, bytesRead );
            
            return response;
        
        }
        
        // sends data to server without need to response right away
        private void RequestAsimetric( string request ) {
        
            // sending data
            NetworkStream nwStream = _tcpClient.GetStream( );
            byte[ ] bytesToSend = Encoding.ASCII.GetBytes( request );
            
            nwStream.Write( bytesToSend, 0, bytesToSend.Length );
        
        }
        
        // recursion function that reads network stream for new messages
        private void ReseiveNetworkStream( ) {
            
            // conditions to stop reading
            if ( _tcpClient == null ) return;
            if ( _gameState != "Main" ) return;
            
            // receiving response
            NetworkStream nwStream = _tcpClient.GetStream( );
            
            byte[ ] bytesToRead = new byte[ _tcpClient.ReceiveBufferSize ];
            int bytesRead = nwStream.Read( bytesToRead, 0, _tcpClient.ReceiveBufferSize );
            
            string response = Encoding.ASCII.GetString( bytesToRead, 0, bytesRead );
            
            // name cannot start with '+', '-', '!', '*'
            if ( response[0] == '+' )       // adding palyer
                AddPlayer( response.Substring( 1, response.Length - 1 ) );
            else if ( response[0] == '-' )  // removes player
                RemovePlayer( response.Substring( 1, response.Length - 1 ) );
            else if ( response[0] == '!' )  // kills player
                KillPlayers( response.Substring( 1, response.Length - 1 ) ); 
            else if ( response[0] == '*' )  // udates score
                UpdateScore( response.Substring( 1, response.Length - 1 ) );
            else                            // actions were received
                _actionLog += "/" + response;
            
            // recursion - waiting for other messages from the server
            ReseiveNetworkStream( );
            
        }
        
        #endregion
        
        
        #region Game process managing
        
        // bad code. map is to big to send it in one response. map is always the same anyway.
        private int[] LoadMap( ) {
            
            // bad code, rewrite!
            string mapData = "240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 120 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240 240";
            
            return Array.ConvertAll( mapData.Split( ' ' ), elem => int.Parse( elem ) );
        
        }
        
        // returns banners position. [0] - blue flag, [1] - red flag. 
        private int[] FlagsPosition( ) {
        
            string data = "getFlags=>" + _gameName;
            
            string mapData = RequestSimetric( data );
            
            return Array.ConvertAll( mapData.Split( ' ' ), elem => int.Parse( elem ) );
            
        
        }
        
        //  adds all players to the game
        private void GetPlayers( ) {
        
            int playerNumber = 0;
            
            string response;
            
            while ( true ) {
            
                string data = "getPlayer=>" + _gameName + "&&" + playerNumber.ToString( );
                
                response = RequestSimetric( data );
                
                if ( response.Split( ':' )[ 0 ] != "200" ) { break; }
                
                // 0 - name
                // 1 - team
                // 2 - positionX
                // 3 - positionY
                string[] playerData = response.Split( ':' )[ 1 ].Split( '/' );
                
                Knight knight = CreateKnight( playerData[ 0 ], playerData[ 1 ], Convert.ToInt32( playerData[ 2 ] ), Convert.ToInt32( playerData[ 3 ] ) );
                
                _gameComponents[ "Main" ].Add( knight );                
           
                playerNumber++;
            
            } 
        }
        
        // banners add banners textures on it's possitions
        private void AddBanners( int blueFlagPositionX, int redFlagPositionX ) {
        
            const int bannerSize = 80;
       
            Banner blueBanner = new Banner( _bannerTextures[ "BlueFlag" ], _bannerTextures[ "BlueFlagC" ] ) {
            
                Position = new Vector2( blueFlagPositionX - bannerSize / 2 , _WINDOWHEIGHT - map[ blueFlagPositionX ] - bannerSize ),
                Size = new Vector2( bannerSize, bannerSize ),
            
            };
            
            Banner redBanner = new Banner( _bannerTextures[ "RedFlag" ], _bannerTextures[ "RedFlagC" ] ) {
            
                Position = new Vector2( redFlagPositionX - bannerSize / 2, _WINDOWHEIGHT - map[ redFlagPositionX ] - bannerSize ),
                
                Size = new Vector2( bannerSize, bannerSize ),
            
            };
            
            _gameComponents[ "Main" ].Add( blueBanner );
            _gameComponents[ "Main" ].Add( redBanner );
            
        }
        
        // creates a knight based on parameters were sent
        private Knight CreateKnight( string name, string team , int x, int y) {
        
            return new Knight( ( team == "team1" ) ? _blueKnightTextures : _redKnightTextures, map, _swordSound ) {
                
                Position = new Vector2( x, y),
                Size = new Vector2( _KNIGHTSIZE, _KNIGHTSIZE ),
                Name = name,
                Team = team,
                
            };
        
        }
        
        // adds a new player if someone attended the game
        private void AddPlayer( string data ) {
        
            string[] playerData = data.Split( '/' );
            
            Knight knight = CreateKnight( playerData[ 0 ], playerData[ 1 ], Convert.ToInt32( playerData[ 2 ] ), _WINDOWHEIGHT - Convert.ToInt32( playerData[ 3 ] ) );
            
            _gameComponents[ "Main" ].Add( knight );
            
        }
        
        // removes player if someone left the game
        private void RemovePlayer( string data ) {
        
            string[] playerData = data.Split( '/' );
        
            string name = playerData[ 0 ];
            
            Knight toRemove = FindKnightByName( name );
            
            if ( toRemove != null ) _gameComponents[ "Main" ].Remove( toRemove );
            
        
        }
        
        // finds a knight by its name
        private Knight FindKnightByName( string name ) {
            
            foreach ( Component component in _gameComponents[ "Main" ] ) {
            
                if ( component is Knight knight ) {
                
                    if ( knight.Name == name )
                        return knight;
                
                }
            
            }
            
            return null;
        
        }
        
        // gets goordinates of the player
        private Vector2 MyCoords( ) {
        
            Knight myself = FindKnightByName( _myName );
            
            return ( myself == null ) ? new Vector2( -1, -1 ) : myself.Position;
        
        }
        
        // checks if anyone can be killed around the player when the player hits
        private void TryToKill( ) {
        
            Knight me = FindKnightByName( _myName );
            
            Vector2 position = me.Position;
            
            float[ ] vulnerableByX = new float[ 2 ]; // range that the player can kill others by X
            float[ ] vulnerableByY = new float[ 2 ]; // range that the player can kill others by Y
            
            // finding range by Y
            vulnerableByY[ 0 ] = position.Y;
            vulnerableByY[ 1 ] = position.Y + _KNIGHTSIZE;
            
            // finding range by X
            if ( me.Side == "Left" ) {
            
                vulnerableByX[ 0 ] = position.X - _KNIGHTSIZE;
                vulnerableByX[ 1 ] = position.X + _KNIGHTSIZE;
            
            } else {
            
                vulnerableByX[ 0 ] = position.X;
                vulnerableByX[ 1 ] = position.X + ( _KNIGHTSIZE );

            }
            
            List<string> killedKnightNames = new List<string>( );
            
            // defines all knight that's just been killed
            foreach ( Component component in _gameComponents[ "Main" ] )
                if ( component is Knight knight )
                    if ( ( knight.Position.X >= vulnerableByX[ 0 ] ) && ( knight.Position.X <= vulnerableByX[ 1 ] ) )
                        if ( ( knight.Position.Y >= vulnerableByY[ 0 ] ) && ( knight.Position.Y <= vulnerableByY[ 1 ] ) )
                            if ( knight.Team != me.Team )
                                killedKnightNames.Add( knight.Name );
            
            // if someone is killed, tell server about that
            if ( killedKnightNames.Count > 0 ) {
            
                string data = "kill=>" + string.Join( "&&", killedKnightNames );
                RequestAsimetric( data );
            
            }

        } 

        // manages knight that were killed
        void KillPlayers( string data ) {
            
            // killed players
            string[] players = data.Split( ':' )[ 1 ].Split( '/' );
            
            for ( int i = 0; i < players.Length; i++ )
                for ( int j = 0; j < _gameComponents[ "Main" ].Count; j++ )
                    if ( _gameComponents[ "Main" ][ j ] is Knight knight )
                        if ( knight.Name == players[ i ] )
                            knight.Position = new Vector2( ( knight.Team == "team1" ) ? 60 : 940, 200 ); 
                                // move killed player on the start possition of his team
            
            // update score request
            string scoreRequ = "getScore=>" + _gameName;
            RequestAsimetric( scoreRequ );
        
        }
        
        // updates score label value
        private void UpdateScore( string scoreLine ) {
        
            ( (Label)_gameComponents[ "Main" ][12] ).Text = scoreLine;
        
        }
        
        #endregion
        
    }
}
