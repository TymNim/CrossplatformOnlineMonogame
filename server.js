
"use strict"

const gameWidth = 1000;
const landHeight = 120;

var net = require( 'net' )


var connections = []


var games = []

var clients = []

//setInterval( function( ) { console.log( games ) }, 5000 )

var server = net.createServer( );

server.on( "connection", function ( socket ) {

	var remoteAddress = socket.remoteAddress + ":" + socket.remotePort

	console.log( "new client connection on %s", remoteAddress )


	socket.on( "data", function ( data ) {

		console.log( "Data from %s : %s", remoteAddress, data )
//		socket.write( "Received: " + data )

		var action = String(data).split( "=>" )[ 0 ]
		var options = String(data).split( "=>" )[ 1 ].split( "&&" )

		if ( action == "createGame" ) {

			socket.write( createGame( options, remoteAddress ) )

		}

		if ( action == "joinGame" ) {

			socket.write( joinGame( options, remoteAddress ) )

			/*for ( var i in games[ clients[ remoteAddress ].gameName ].players ) {
				if ( games[ clients[ remoteAddress ].gameName ].players[ i ].id == remoteAddress ) {
					name = games[ clients[ remoteAddress ].gameName ].players[ i ].name
					team = games[ clients[ remoteAddress ].gameName ].players[ i ].team
					x = games[ clients[ remoteAddress ].gameName ].players[ i ].position.x
					y = games[ clients[ remoteAddress ].gameName ].players[ i ].position.y
				}

			}

			broadcast( clients[ remoteAddress ].gameName, "+" + name + "/" + team + "/" + x + "/" + y, name )
			*/

		}

		if ( action == "action" ) {

			doAction( options, remoteAddress )

		}

		if ( action == "downloadGame" ) {

			socket.write( getGame( clients[ remoteAddress ].gameName ) )

		}

		if ( action == "getFlags" ) {

			socket.write( games[ options[ 0 ] ].blueFlagPosition + " " + games[ options[ 0 ] ].redFlagPosition )

		}

		if ( action == "getPlayer" ) {

			socket.write( playerData( options ) )

		}

		if ( action == "coordinate" ) {

			gameCoordinates( clients[ remoteAddress ].gameName, options, remoteAddress )

		}

		if ( action == "kill" ) {

			kill( clients[ remoteAddress ].gameName, options )

		}

		if ( action == "getScore" ) {

			socket.write( "*" + games[ options[0] ].score.team1 + ":" + games[ options[0] ].score.team2 )

		}


	})


	socket.once( "close", function ( ) {

//		console.log( "---Index : " + clientIndex + "\nclient : " + clients[ remoteAddress ] + "\naddr : " + remoteAddress )

		if ( clients[ remoteAddress ].gameName != "" ) {

			var index = -1;
			var name = "";
			for ( var i in games[ clients[ remoteAddress ].gameName ].players ) {

				if ( games[ clients[ remoteAddress ].gameName ].players[ i ].id == remoteAddress ) {
					index = i
					name = games[ clients[ remoteAddress ].gameName ].players[ i ].name
				}

			}

			if ( index >= 0 ) {

				if ( games[ clients[ remoteAddress ].gameName ].players[ index ].team == "team1" )
					games[ clients[ remoteAddress ].gameName ].team1--
				else
					games[ clients[ remoteAddress ].gameName ].team2--
				games[ clients[ remoteAddress ].gameName ].players.splice( index, 1 )

			}

			if ( games[ clients[ remoteAddress ].gameName ].players.length == 0) {

				delete games[ clients[ remoteAddress ].gameName ]

			} else {

				broadcast( clients[ remoteAddress ].gameName, "-" + name )

			}

		}
		delete clients[ remoteAddress ]
		console.log( "Connection from %s closed.", remoteAddress )

	})

	socket.on ( "error", function ( err ) {

		console.log( "Connection %s error : %s", remoteAddress, err.message )

	})

	//connections.push( socket )

	var client = { }
		client[ "socket" ] = socket
		client[ "gameName" ] = ""

	clients[ remoteAddress ] = client

//	setInterval( function( ) { socket.write( "here?" ) }, 3000 )
})

server.listen( 8080, function ( ) {

	console.log( "Server listening to %j", server.address( ) )

})


function createGame( options, id ) {

	var gameName = options[ 0 ]
	var userName = options[ 1 ]

	if ( games[ gameName ] )
		return "410:ALREADY EXISTS"

	var map = generateMap( )

	var newGame = {}
		newGame[ "players" ] = []
		newGame[ "maxNumberOfPlayers" ] = 12
		newGame[ "team1" ] = 0
		newGame[ "team2" ] = 0
		newGame[ "map" ] = map
		newGame[ "redFlagPosition" ] = gameWidth - 60
		newGame[ "blueFlagPosition" ] = 60
		newGame[ "score" ] = { team1 : 0, team2 : 0 }

	games[ gameName ] = newGame

	return "200:OK"
}

function generateMap( ) {

	var game = []

	for ( var i = 0; i < gameWidth / 8; i++ ) {
		game.push( landHeight * 2 )
	}

	for ( var i = 0; i < 6 * gameWidth / 8; i++ ) {
		game.push( landHeight )
	}

	for ( var i = 0; i < gameWidth / 8; i++ ) {
		game.push( landHeight * 2)
	}

	return game

}


function joinGame( options, id ){

	var gameName = options[ 0 ]
    var userName = options[ 1 ]

	if (! games[ gameName ] ) {

		return "404:NOT FOUNT"

	}

	for ( var i in games[ gameName ].players ) {

		if ( games[ gameName ].players[ i ].name == userName )
			return "403:FORBIDDEN"

	}

	console.log( userName + " connected to " + gameName )
	clients[ id ].gameName = gameName

	var team

	if ( games[ gameName ].team1 <= games[ gameName ].team2 ) {

		team = "team1"
		games[ gameName ].team1++

	} else {

		team = "team2"
        games[ gameName ].team2++

	}

	var newPlayer = {}
		newPlayer[ "name" ] = userName
		newPlayer[ "id" ] = id
		newPlayer[ "team" ] = team
		newPlayer[ "position" ] = { x : ( team == "team1" ) ? 60 : gameWidth - 60, y : 3 * landHeight }

	games[ gameName ].players.push( newPlayer )

	broadcast( gameName, "+" + userName + "/" + team + "/" + newPlayer.position.x + "/" + newPlayer.position.y, userName )

	return "200:OK"

}


function doAction( actions, id ) {

	//console.log( id + " did some actions : [ " + actions + " ]" )

	var gameName = clients[ id ].gameName

	var name = ""

	for ( var i in games[ gameName ].players )
		if ( games[ gameName ].players[ i ].id == id )
			name = games[ gameName ].players[ i ].name



	var action = actions.join( "&" )

	var dataToSend = name + ":" + action

	broadcast( gameName, dataToSend )
	//for (

}


function broadcast( gameName, data, except ) {

	for ( var i in games[ gameName ].players ) {
		if ( games[ gameName ].players[ i ].name  != except )
			clients[ games[ gameName ].players[ i ].id ].socket.write( data )

	}

}


function getGame( gameName ) {

	return games[ gameName ].map.join( " " )

}

function playerData( options ) {

	var gameName = options[ 0 ]
	var playerIndex = options[ 1 ]

	if ( playerIndex >= games[ gameName ].players.length ) {

		return "404:NOT FOUND"

	}

	var name = games[ gameName ].players[ playerIndex ].name
	var team = games[ gameName ].players[ playerIndex ].team
	var x = games[ gameName ].players[ playerIndex ].position.x
	var y = games[ gameName ].players[ playerIndex ].position.y

	return "200:" + name + "/" + team + "/" + x + "/" + y


}

function gameCoordinates( gameName, options, id ) {

	for ( var i in games[ gameName ].players ) {
		if ( id == games[ gameName ].players[ i ].id ) {

			games[ gameName ].players[ i ].position.x = options[ 0 ]
			games[ gameName ].players[ i ].position.y = options[ 1 ]

			console.log( games[ gameName ].players[ i ].name + "'s from " + gameName + " were updated to " + games    [ gameName ].players[ i ].position.x + ":" + games[ gameName ].players[ i ].position.y )

		}
	}

}

function kill( gameName, options ) {

	for ( var i in options ) {
		for ( var j in games[ gameName ].players ) {

			if ( games[ gameName ].players[ j ].name == options[ i ] ) {

				if ( games[ gameName ].players[ j ].team == "team1" ) {

					games[ gameName ].score.team2++

				} else {

					games[ gameName ].score.team1++

				}

			}

		}

	}

	var data = "!kill:" + options.join( "/" );

	broadcast( gameName, data )

}

