using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Massacre.Components.Controllers {

    public class Label : Component {
    
        SpriteFont _font;
        
        #region Properties
        public Vector2 Position { get; set; }
        public string Text { set; get; }
        public Color PenColour { get; set; }
        
        #endregion
        
        
        #region Methods
        
        // constructor
        public Label( SpriteFont font ) {
        
            _font = font;

            PenColour = Color.White;
            
        }
        
        // draws text
        public override void Draw( GameTime gameTime, SpriteBatch spriteBatch ) {    
                var x = Position.X - _font.MeasureString( Text ).X / 2;
                var y = Position.Y -  _font.MeasureString( Text ).Y / 2;

                spriteBatch.DrawString( _font, Text, new Vector2( x, y ), PenColour );
        }
        
        public override void Update( GameTime gameTime ) { } 
        
        #endregion
        
    }
}
