using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Massacre.Components.Map {

    public class Banner : Component {
    
        #region
        
        private Texture2D _textureStand;
        private Texture2D _textureBeCared; // unrealized idea

        #endregion
        
        
        #region Properties
        
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public Rectangle Rectangle {
            get {
                return new Rectangle( (int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y );
            }
        }
        public bool IsCared { set; get; }        
        
        #endregion
        
    
        #region Methods
    
        // constructor
        public Banner( Texture2D textureS, Texture2D textureC ) {
        
            _textureStand = textureS;
            _textureBeCared = textureC;
        
        }
        
        // draws the banner on a battle field
        public override void Draw( GameTime gameTime, SpriteBatch spriteBatch ) {
        
            var colour = Color.White;
            
            var activeTexture =  _textureStand;
            
            var drawingRect = Rectangle;
            
            if ( IsCared ) {

                activeTexture = _textureBeCared;
                drawingRect = new Rectangle( Rectangle.X, Rectangle.Y + 20, Rectangle.Width, Rectangle.Height );

            }
                         
            spriteBatch.Draw( activeTexture, drawingRect, colour);
                        
        }

        public override void Update( GameTime gameTime ) { }
        
        #endregion
        
    }
    
}
