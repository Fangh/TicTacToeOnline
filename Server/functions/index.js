const functions = require('firebase-functions');
const admin = require('firebase-admin');

admin.initializeApp();

// // Create and Deploy Your First Cloud Functions
// // https://firebase.google.com/docs/functions/write-firebase-functions
//
// exports.helloWorld = functions.https.onRequest((request, response) => {
//  response.send("Hello from Firebase!");
// });


// exports.addMessage = functions.https.onRequest(async (req, res) =>
// {
//     const original = req.query.text;
//     const snapshot = await admin.database().ref('/messages').push({original : original});
//     res.redirect(303, snapshot.ref.toString());
// });

exports.deleteGames = functions.database.ref('/games').onUpdate((snapshot, context) =>
{
  const gamesDeleted = [];
  const snapValue = snapshot.after.val();
  const gamesDictionnary = Object.entries(snapValue); //games : [ [id, gamedata], [id2, gameData] ]

  gamesDictionnary.forEach(game => 
  {
    if(game[1].id === "SERVER")
      return;

      functions.logger.log(game[0] + "=> id");
      functions.logger.log(game[1].id + "=> game.id");

      if(game[1].id == undefined)
      {
        functions.logger.log(game[0] + " ID is undefined. Deleting it.");
        admin.database().ref('/games/' + game[0]).remove();
        gamesDeleted.push(game.id);

      }

    if (game[1].winner !== 0)
    {
      functions.logger.log(game[1].id + " is finished. Deleting it.");
      admin.database().ref('/games/' + game[1].id).remove();
      gamesDeleted.push(game.id);
    }

    if (new Date() - game[1].updatedTime > 604800000) //1week in ms
    {
      functions.logger.log(game[1].id + " has not been updated since one week. Deleting it.");
      admin.database().ref('/games/' + game[1].id).remove();
      gamesDeleted.push(game.id);
    }

  });

  return gamesDeleted;
});

//functions.pubsub.schedule('every 1 hours').onRun((context)
exports.removeInactivePlayers = functions.database.ref('/games').onUpdate((snapshot, context) =>
{
  const snapValue = snapshot.after.val();
  const gamesDictionnary = Object.entries(snapValue); //games : [ [id, gamedata], [id2, gameData] ]
  const disconnectedPlayersFromGames = [];
  gamesDictionnary.forEach(game => 
  {
    if(game[1].id === "SERVER")
      return;

    if (new Date() - game[1].updatedTime > 3600000) //1hours in ms
    {
      functions.logger.log(game[1].id + " has not been updated since one hour. Diconnect everybody.");
      admin.database().ref("/games/").child(game[1].id).child("updatedTime").set(new Date());
      admin.database().ref("/games/").child(game[1].id).child("player1").child("isConnected").set(false);
      admin.database().ref("/games/").child(game[1].id).child("player2").child("isConnected").set(false);
      disconnectedPlayersFromGames.push(game[1].id);
    }
  });

  return disconnectedPlayersFromGames;
});

exports.showGames = functions.https.onRequest(async (req, res) =>
{
  const snapshot = await admin.database().ref('/games').val();
  functions.logger.log("database = " + snapshot);
});

exports.sendMessage = functions.database.ref('/games/{gameId}').onUpdate((snapshot, context) =>
{
  const thisGame = snapshot.after.val();
  var returnValue = "none";
  // functions.logger.log("gameId that was changed = " +thisGame.id);
  // functions.logger.log("game that was changed = " +thisGame);

  if (thisGame.winner === 0)
  {
    if(thisGame.currentTurn === 1 && !thisGame.player1.isConnected && thisGame.player1.token !== "")
      SendNotificationTo(thisGame.id, thisGame.player1.id, thisGame.player1.token, "Next Turn", thisGame.player1.id + " it's your turn to play in " + thisGame.id);
    else if(thisGame.currentTurn === 2 && !thisGame.player2.isConnected && thisGame.player2.token !== "")
      SendNotificationTo(thisGame.id, thisGame.player2.id, thisGame.player2.token, "Next Turn", thisGame.player2.id + " it's your turn to play in " + thisGame.id);
  }

  function SendNotificationTo(_gameId, _playerId, _playerToken, _title, _content)
  {
    // functions.logger.log('Trying to send a notif to ' + _playerId + ' from game '+_gameId);
    if (_playerToken.includes("COMPUTERTOKEN_")) //do not send notif to computers
      return returnValue = "computer token";

    var message = {
      notification: {
        title: _title,
        body: _content
      },
      data:{
        gameId: _gameId
      },
      token: _playerToken
    };
    admin.messaging().send(message)
      .then((response) => 
      {
        // Response is a message ID string.
        // functions.logger.log('Successfully sent message:', response);
        returnValue = "notif send to " + _playerId;
        return response;
      })
      .catch((error) => 
      {
        functions.logger.error('Error sending message:', error);
        returnValue = "notif failed to send to " + _playerId;
      });
  }

  return returnValue;
});