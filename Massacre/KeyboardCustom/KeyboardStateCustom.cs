using System;
using Microsoft.Xna.Framework.Input;

namespace Massacre.KeyboardCustom
{

    // customized KeyboardState class to be able to decide if pressed button is new but no the same one
    public class KeyboardStateCustom
    {
    
        private KeyboardState _state = Keyboard.GetState();
        private long _timeKeyPressed = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    
        
        public bool IsNew( KeyboardState state ) {
            
            long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            
            if ( state != _state ) {
            
                _state = state;
                _timeKeyPressed = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            
            } else if ( now - _timeKeyPressed > 1000 ) { 
            
                _timeKeyPressed = now;
                
            
            } else {
                
                return false;
                
            }
            
            
            return true;
        
        }
        
        
        
    }
}
