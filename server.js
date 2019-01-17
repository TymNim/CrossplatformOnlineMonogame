
"use strict"

const GAMEWIDTH = 1000 	// width of game window in px
const LANDHEIGHT = 120 	// height of the land in games in px

const PORT = 8080		// port will be used for game server

var net = require( 'net' )

var games = { }		// all online games

var clients = { } 	// all online clients

var server = net.createServer( );

server.on( "connection", function ( socket ) {

	// remote address is unique so it will be used as id
	var remoteAddress = socket.remoteAddress + ":" + socket.remotePort

	console.log( "new client connection on %s", remoteAddress )

	// if there's any data from client - deal with it
	socket.on( "data", function ( data ) {

		// standard action=>option1&&option2&&etc
		var action = String(data).split( "=>" )[ 0 ]
		var options = String(data).split( "=>" )[ 1 ].split( "&&" )

		if ( action == "createGame" )
			// creates a new game
			socket.write( createGame( options[ 0 ], options[ 1 ], remoteAddress ) )

		if ( action == "joinGame" )
			// joins player to the game
			socket.write( joinGame( options[ 0 ], options[ 1 ], remoteAddress ) )

		if ( action == "action" )
			// any actions in a game - left, right, up, down, hit
			doAction( options, remoteAddress )

		if ( action == "getFlags" )
			// sends banners position
			socket.write( getFlagsPositon( options[ 0 ] ) )

		if ( action == "getPlayer" )
			// sends player data by index in a game
			socket.write( getPlayerData( options[ 0 ], options[ 1 ] ) )

		if ( action == "coordinate" )
			// updates player coordinates
			refreshThisPlayerCoordinates( clients[ remoteAddress ].gameName, options, remoteAddress )

		if ( action == "kill" )
			// kills players were killed
			killPlayers( clients[ remoteAddress ].gameName, options )

		if ( action == "getScore" )
			// sends score by game name
			socket.write( getScore( options[ 0 ] ) )

	})

	// when client has disconnected
	socket.once( "close", function ( ) {

		if ( clients[ remoteAddress ].gameName != "" ) {
			// removes player from the game if they're playing

			var index = -1;
			var name = "";
			// defines player's name and index
			for ( var i in games[ clients[ remoteAddress ].gameName ].players ) {

				if ( games[ clients[ remoteAddress ].gameName ].players[ i ].id == remoteAddress ) {
					index = i
					name = games[ clients[ remoteAddress ].gameName ].players[ i ].name
				}

			}

			// use index to remove the player form his team
			if ( index >= 0 ) {

				if ( games[ clients[ remoteAddress ].gameName ].players[ index ].team == "team1" )
					games[ clients[ remoteAddress ].gameName ].team1--
				else
					games[ clients[ remoteAddress ].gameName ].team2--
				games[ clients[ remoteAddress ].gameName ].players.splice( index, 1 )

			}

			if ( games[ clients[ remoteAddress ].gameName ].players.length == 0)
				// remove game from <games> if there's no players
				delete games[ clients[ remoteAddress ].gameName ]
			else
				// tell others that the player left the game
				broadcast( clients[ remoteAddress ].gameName, "-" + name )

		}

		// remove client from <clients>
		delete clients[ remoteAddress ]
		console.log( "Connection from %s closed.", remoteAddress )

	})

	socket.on ( "error", function ( err ) {

		console.log( "Connection %s error : %s", remoteAddress, err.message )

	})

	// adds client <to clients>
	var client = { }
		client[ "socket" ] = socket
		client[ "gameName" ] = ""

	clients[ remoteAddress ] = client

})

// server listents on <PORT>
server.listen( PORT, function ( ) {

	console.log( "Server listening to %j", server.address( ) )

})

// creates a new game
function createGame( gameName, userName, id ) {

	if ( games[ gameName ] )
		// if game already exists
		return "410:GONE"

	var newGame = {}
		newGame[ "players" ] = []
		newGame[ "maxNumberOfPlayers" ] = 12
		newGame[ "team1" ] = 0
		newGame[ "team2" ] = 0
		newGame[ "redFlagPosition" ] = GAMEWIDTH - 60
		newGame[ "blueFlagPosition" ] = 60
		newGame[ "score" ] = { team1 : 0, team2 : 0 }

	games[ gameName ] = newGame

	return "200:OK"

}

// joins player to the game they want
function joinGame( gameName, userName, id ){

	if (! games[ gameName ] )
		// if there's no game with this name
		return "404:NOT FOUNT"

	if ( games[ gameName ].team1 + games[ gameName ].team2 >= games[ gameName ].maxNumberOfPlayers )
		// if there's already maximum number of players
		return "410:GONE"

	for ( var i in games[ gameName ].players )
		if ( games[ gameName ].players[ i ].name == userName )
			// if players with this name exists
			return "403:FORBIDDEN"

	// if everything's fine =>
	clients[ id ].gameName = gameName

	var team

	// deciding which team to join
	if ( games[ gameName ].team1 <= games[ gameName ].team2 ) {

		team = "team1"
		games[ gameName ].team1++

	} else {

		team = "team2"
        games[ gameName ].team2++

	}

	// adding player to the game
	var newPlayer = {}
		newPlayer[ "name" ] = userName
		newPlayer[ "id" ] = id
		newPlayer[ "team" ] = team
		newPlayer[ "position" ] = { x : ( team == "team1" ) ? 60 : GAMEWIDTH - 60, y : 3 * LANDHEIGHT }
	games[ gameName ].players.push( newPlayer )

	// tells everyone except current user that there's new player
	broadcast( gameName, "+" + userName + "/" + team + "/" + newPlayer.position.x + "/" + newPlayer.position.y, userName )

	return "200:OK"

}

// prepares actions that were made by players to broadcast
function doAction( actions, id ) {

	var gameName = clients[ id ].gameName

	var name = ""

	for ( var i in games[ gameName ].players )
		if ( games[ gameName ].players[ i ].id == id )
			name = games[ gameName ].players[ i ].name

	var action = actions.join( "&" )

	var dataToSend = name + ":" + action

	broadcast( gameName, dataToSend )

}

// broadcasts <data> to all users in the <gameName>. <except> is optional
function broadcast( gameName, data, except ) {

	for ( var i in games[ gameName ].players )
		if ( games[ gameName ].players[ i ].name  != except )
			clients[ games[ gameName ].players[ i ].id ].socket.write( data )

}

// gets player data by index in game. called when client attend a game.
// there's different request for each player untill client gets 404 error
function getPlayerData( gameName, playerIndex ) {

	// if there's no players send 404 error, client will stop requiring players
	if ( playerIndex >= games[ gameName ].players.length )
		return "404:NOT FOUND"

	var name = games[ gameName ].players[ playerIndex ].name
	var team = games[ gameName ].players[ playerIndex ].team
	var x = games[ gameName ].players[ playerIndex ].position.x
	var y = games[ gameName ].players[ playerIndex ].position.y

	return "200:" + name + "/" + team + "/" + x + "/" + y

}

// updates player coordinates to make sure everyone is on the write possition
function refreshThisPlayerCoordinates( gameName, options, id ) {

	for ( var i in games[ gameName ].players ) {
		if ( id == games[ gameName ].players[ i ].id ) {

			games[ gameName ].players[ i ].position.x = options[ 0 ]
			games[ gameName ].players[ i ].position.y = options[ 1 ]

		}
	}

}

// increases score for each killed knight, broadcast data about killed knight
function killPlayers( gameName, killedPlayerNames ) {

	// increasing the score of opposite team for each player
	for ( var name in killedPlayerNames )
		for ( var player in games[ gameName ].players )
			if ( games[ gameName ].players[ player ].name == killedPlayerNames[ name ] )
				if ( games[ gameName ].players[ player ].team == "team1" )
					games[ gameName ].score.team2++
				else
					games[ gameName ].score.team1++

	// tell all that some knight just were killed
	var data = "!kill:" + killedPlayerNames.join( "/" );
	broadcast( gameName, data )

}

// returns score in required format
function getScore( gameName ) {

	return "*" + games[ gameName ].score.team1 + ":" + games[ gameName ].score.team2

}

// retruns flag possition in the game
function getFlagsPositon( gameName ) {

	return games[ gameName ].blueFlagPosition + " " + games[ gameName ].redFlagPosition

}

