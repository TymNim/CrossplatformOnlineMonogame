using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Massacre.Components.Controllers {
    
    public class Input : Component {
    
        #region 
        private MouseState _currentMouse;
        private SpriteFont _font;
        private MouseState _previousMouse;
        private Texture2D _texture;
        private Texture2D _focusedTexture;
        private string _placeholder = ""; 
        #endregion
        
        
        #region Properties
        public event EventHandler Click;
        public bool Clicked{ get; private set; }
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public Rectangle Rectangle {
            get {
                return new Rectangle( (int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y );
            }
        }
        // Text
        public string Text;
        public int PaddingLeft { set; get; }
        public Color PenColour { get; set; }
        public bool IsFocused { get; set; }
        #endregion
        
        
        #region Methods
        
        // constructor
        public Input( Texture2D texture, Texture2D focused,  SpriteFont font, string placeholder ) {
        
            _texture = texture;
            _focusedTexture = focused;
            _font = font;
            _placeholder = placeholder;

            PenColour = Color.White;
            Text = "";
            IsFocused = false;
            
        }
        
        // draws textrue input with text in it
        public override void Draw( GameTime gameTime, SpriteBatch spriteBatch ) {
        
            var colour = Color.White;
            
            var activeTexture = _texture;
                
            if ( IsFocused )
                activeTexture = _focusedTexture;
                
            spriteBatch.Draw( activeTexture, Rectangle, colour);
            
            // Drawing text if the input field is not empty
            if (! string.IsNullOrEmpty( Text ) ) { // placing the text value
                
                var x = Rectangle.X + PaddingLeft;
                var y = Rectangle.Y + ( Rectangle.Height / 1.75f ) - ( _font.MeasureString( Text ).Y / 2 );
                
                string textToDisplay = Text;
                
                int pxTextLength = (int)_font.MeasureString( textToDisplay ).X;
                
                // not to show overflow text
                while ( pxTextLength > Size.X - 2 * PaddingLeft ) { 
                
                    textToDisplay = textToDisplay.Substring( 1, textToDisplay.Length - 1 );
                    
                    pxTextLength = (int)_font.MeasureString( textToDisplay ).X;
                    
                }
                
                spriteBatch.DrawString( _font, textToDisplay, new Vector2( x, y ), PenColour );
                
            } else { // Placing placeholder
            
                var x = Rectangle.X + PaddingLeft;
                var y = Rectangle.Y + ( Rectangle.Height / 1.75f ) - ( _font.MeasureString( _placeholder ).Y / 2 );
                
                spriteBatch.DrawString( _font, _placeholder, new Vector2( x, y ), Color.DarkGray );
                
            }
            
        }

        // checks if fiels is pressed
        public override void Update( GameTime gameTime ) {
        
            _previousMouse = _currentMouse;
            _currentMouse = Mouse.GetState();

            var mouseRectangle = new Rectangle( _currentMouse.X, _currentMouse.Y, 1, 1 );            
            
            if ( mouseRectangle.Intersects( Rectangle ) ) {
            
                if ( _currentMouse.LeftButton == ButtonState.Released && _previousMouse.LeftButton == ButtonState.Pressed ) {
                
                    Click?.Invoke( this, new EventArgs( ) );
                
                }
            
            }
            
            
                
            
            
        }
        
        #endregion
    }
    
}
