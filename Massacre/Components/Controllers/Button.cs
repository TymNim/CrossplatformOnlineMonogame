using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Massacre.Components.Controllers {
    
    public class Button : Component {
        
        #region
         
        private MouseState _currentMouse;
        private SpriteFont _font;
        private bool _isHovering;
        private MouseState _previousMouse;
        private Texture2D _texture;
        private Texture2D _hoveredTexture;
        private Texture2D _pressedTexture;
        
        #endregion
        
        
        #region Properties
        
        public event EventHandler Click;
        public bool Clicked{ get; private set; }
        public Color PenColour { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public Rectangle Rectangle {
            get {
                return new Rectangle( (int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y );
            }
        }
        public string Text;
        
        #endregion
        
        
        #region Methods 
        
        // constructor
        public Button( Texture2D texture, Texture2D hovered, Texture2D pressed,  SpriteFont font ) {
        
            _texture = texture;
            _hoveredTexture = hovered;
            _pressedTexture = pressed;
            _font = font;

            PenColour = Color.White;
            
        }
        
        // drawing button with text
        public override void Draw( GameTime gameTime, SpriteBatch spriteBatch ) {
        
            var colour = Color.White;
            
            var activeTexture = _texture;
            
            if ( _isHovering ) 
                activeTexture = _hoveredTexture;
                
            if ( ( _currentMouse.LeftButton == ButtonState.Pressed ) && ( _isHovering ) )
                activeTexture = _pressedTexture;
            
            spriteBatch.Draw( activeTexture, Rectangle, colour);
            
            // Drawing text
            if (! string.IsNullOrEmpty( Text ) ) {
            
                var x = Rectangle.X + ( Rectangle.Width / 2 ) - ( _font.MeasureString( Text ).X / 2 );
                var y = Rectangle.Y + ( Rectangle.Height / 1.75f ) - ( _font.MeasureString( Text ).Y / 2 );

                spriteBatch.DrawString( _font, Text, new Vector2( x, y ), PenColour );
                
            }
            
        }
        
        // checks if hovered or pressed
        public override void Update( GameTime gameTime ) {
        
            _previousMouse = _currentMouse;
            _currentMouse = Mouse.GetState();

            var mouseRectangle = new Rectangle( _currentMouse.X, _currentMouse.Y, 1, 1 );

            _isHovering = false;
            
            
            if ( mouseRectangle.Intersects( Rectangle ) ) {
            
                _isHovering = true;

                if ( _currentMouse.LeftButton == ButtonState.Released && _previousMouse.LeftButton == ButtonState.Pressed ) {
                
                    Click?.Invoke( this, new EventArgs( ) );
                
                }
                
            }
            
        }

        #endregion
        
    }
    
}