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
        if(game[1].winner !== 0)
        {
            console.log(game[1].id + " is finished. Deleting it.");
            admin.database().ref('/games/'+game[1].id).remove();
            gamesDeleted.push(game.id);
        }
    });

 return gamesDeleted;
})

exports.showGames = functions.https.onRequest(async (req, res) =>
{
    const snapshot = await admin.database().ref('/games').val();
    console.log("database = "+snapshot);
    
})