using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Massacre.Components.Map {

    public class SkyAndGround : Component {

            
    
        #region 
        
        private Texture2D _texture;
        
        #endregion
        
        #region
        
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public Rectangle Rectangle {
            get {
                return new Rectangle( (int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y );
            }
        }
         
        #endregion
        
        
        #region Methods
        public SkyAndGround( Texture2D texture ) {
        
            _texture = texture;
            
        }
        
        public override void Draw( GameTime gameTime, SpriteBatch spriteBatch ) {
        
            var colour = Color.White;
                
            spriteBatch.Draw( _texture, Rectangle, colour);
            
        }

        public override void Update( GameTime gameTime ) { }
        
        #endregion
    
    }
    
}
