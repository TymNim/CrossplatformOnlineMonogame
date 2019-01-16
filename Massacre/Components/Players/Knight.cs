using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Massacre.Components.Players {
    
    public class Knight : Component{
    
        #region 
        
        // A crutch --- rewrite
        private int _WINDOWHEIGHT = 560;
        
        //standing
        private Texture2D _textureStandLeft;
        private Texture2D _textureStandRight;
        // atacking
        private Texture2D _textureMeleeLeftStart;
        private Texture2D _textureMeleeLeftEnd;
        private Texture2D _textureMeleeRightStart;
        private Texture2D _textureMeleeRightEnd;
        //walking
        private Texture2D[] _texturesWalkLeft = new Texture2D[ 4 ];
        private Texture2D[] _texturesWalkRight = new Texture2D[ 4 ];
        
        private SoundEffect _swordSound;
        
        // A crutch --- rewrite
        private int[] _map;

        #endregion
        
        //public Color PenColour { get; set; }
        //private SpriteFont _font; 
        // font and pen will be needed in case to display names over the players
        
        #region Properties
        
        public Vector2 Position;
        public Vector2 Size { get; set; }
        public Rectangle Rectangle {
            get {
                return new Rectangle( (int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y );
            }
        }
        
        // left / right
        public string Side = "Left";
        //public bool IsDead { set; get; } // unrealized idea
        public string State { set; get; }
        public string Name { set; get; }
        public string Team { set; get; }
        
        private Vector2 _velocity = new Vector2( 0, 0 );
        
        // animation in ms
        private const int _animationInterval = 150;
        
        //last updated animation
        private long _lastUpdated = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        
        private Texture2D _lastTexture;
        
        #endregion
    
        
        #region Methods
        
        // default constructor
        public Knight( Dictionary<string, Texture2D> textures, int[] map, SoundEffect swordSound ) {
        
            _textureStandLeft = textures[ "StandLeft" ];
            _textureStandRight = textures[ "StandRight" ];
        
            _textureMeleeLeftStart = textures[ "MeleeLeft1" ];
            _textureMeleeLeftEnd = textures[ "MeleeLeft2" ];
            _textureMeleeRightStart = textures[ "MeleeRight1" ];
            _textureMeleeRightEnd = textures[ "MeleeRight2" ];
        
            _texturesWalkLeft[0] = textures[ "WalkLeft1" ];
            _texturesWalkLeft[1] = textures[ "WalkLeft2" ];
            _texturesWalkLeft[2] = textures[ "WalkLeft3" ];
            _texturesWalkLeft[3] = textures[ "WalkLeft4" ];
            
            _texturesWalkRight[0] = textures[ "WalkRight1" ];
            _texturesWalkRight[1] = textures[ "WalkRight2" ];
            _texturesWalkRight[2] = textures[ "WalkRight3" ];
            _texturesWalkRight[3] = textures[ "WalkRight4" ];
            
            _map = map;
            
            _lastTexture = _textureStandLeft;
            
            _swordSound = swordSound;
            
        
        }
        
        // draws the knight
        public override void Draw( GameTime gameTime, SpriteBatch spriteBatch ) {
        
            var colour = Color.White;
            
            // decides what texture to draw
            long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            var activeTexture = _lastTexture;
            if ( now - _lastUpdated >= _animationInterval ) {
    
                _lastUpdated = now;
                activeTexture = findTexture( );
                _lastTexture = activeTexture;
                
            }
            
            // a crutch - 2 different textures have different width. some size modifications need to be done.
            if ( activeTexture == _textureMeleeRightEnd )
                spriteBatch.Draw( activeTexture, new Rectangle( Rectangle.X , Rectangle.Y , (int)( Rectangle.Width  * 1.25 ), Rectangle.Height), colour);
            else if ( activeTexture == _textureMeleeLeftEnd )
                spriteBatch.Draw( activeTexture, new Rectangle( (int)(Rectangle.X - 0.3 * Rectangle.Width) , Rectangle.Y , (int)( Rectangle.Width  * 1.25 ), Rectangle.Height), colour);
            else
                spriteBatch.Draw( activeTexture, Rectangle, colour);
                        
        }

        // updates possition and current texture
        public override void Update( GameTime gameTime ) {
            
            #region Possiton and velocity managing
            if ( _WINDOWHEIGHT - _map[ (int)Position.X ] > (int)Position.Y + (int)Size.Y )
                _velocity.Y  += 0.5f;
            
            if ( State == "Left" ) {
            
                Side = "Left";
                _velocity.X = -4;
                
                if ( Position.X > 0 )
                    if ( _WINDOWHEIGHT - _map[ (int)Position.X + (int)_velocity.X ] >= (int)Position.Y + (int)Size.Y )
                        Position.X += _velocity.X;
            
            }
            
            if ( State == "Right" ) {
            
                Side = "Right";
                _velocity.X = 4;
                
                if ( Position.X < _map.Length - Size.X )
                    if ( _WINDOWHEIGHT - _map[ (int)Position.X + (int)Size.X ] >= (int)Position.Y + (int)Size.Y )
                        Position.X += _velocity.X;
            
            }
            
            if ( State == "Down" ) {
            
                _velocity.Y += 2;
                Position.Y += _velocity.Y;
                
            }
            
            if ( State == "Up" ) {

                if ( Position.Y >= _WINDOWHEIGHT - _map[ (int)Position.X ] - (int)Size.Y ) {
                
                    _velocity.Y -= 12;
                    State = "Released";

                }  
                
            }
            
            Position.Y += _velocity.Y;
            
            if ( Position.Y > _WINDOWHEIGHT - _map[ (int)Position.X ] - (int)Size.Y ) {
                Position.Y = _WINDOWHEIGHT - _map[ (int)Position.X ] - (int)Size.Y;
            }
            
            if ( Position.Y  == _WINDOWHEIGHT - _map[ (int)Position.X ] - (int)Size.Y )
                _velocity.Y = 0;
                
            #endregion
          
        }
        
        
        // defines what texture is next to draw
        private Texture2D findTexture( ) {
            if ( State == "Left" ) { 
            
                if ( _lastTexture == _texturesWalkLeft[0] )
                    return _texturesWalkLeft[1];
                else if ( _lastTexture == _texturesWalkLeft[1] )
                    return _texturesWalkLeft[2];
                else if ( _lastTexture == _texturesWalkLeft[2] )
                    return _texturesWalkLeft[3];
                else if ( _lastTexture == _texturesWalkLeft[3] )
                    return _texturesWalkLeft[0];
                else  
                    return _texturesWalkLeft[0];
            
            } else if ( State == "Right" ) {

                if ( _lastTexture == _texturesWalkRight[0] )
                    return _texturesWalkRight[1];
                else if ( _lastTexture == _texturesWalkRight[1] )
                    return _texturesWalkRight[2];
                else if ( _lastTexture == _texturesWalkRight[2] )
                    return _texturesWalkRight[3];
                else if ( _lastTexture == _texturesWalkRight[3] )
                    return _texturesWalkRight[0];
                else  
                    return _texturesWalkRight[0];

            } else if ( State == "Hit" && Side == "Left" ) {
                _swordSound.Play( 1f, 0f, 0f );
                if ( _lastTexture == _textureMeleeLeftEnd )
                    return _textureMeleeLeftStart;
                else  
                    return _textureMeleeLeftEnd;
            
            } else if ( State == "Hit" && Side == "Right" ) {
                _swordSound.Play( 0.5f, 0f, 0f );
                if ( _lastTexture == _textureMeleeRightEnd )
                    return _textureMeleeRightStart;
                else  
                    return _textureMeleeRightEnd;

            } else if ( Side == "Left" ) {
                return _textureStandLeft;
            } else if ( Side == "Left" ) {
                return _textureStandRight;
            }
            
            return _textureStandRight;
            
        }
    
        #endregion
    }

}
