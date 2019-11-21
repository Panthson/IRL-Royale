const functions = require('firebase-functions');
// The Firebase Admin SDK to access the Firebase Realtime Database.
const admin = require('firebase-admin');
admin.initializeApp();


exports.shrinkRadius = functions.https.onRequest(async (req, res) => {
  let radius = 10;
  //GET RADIUS TO BE DYNAMIC BY PULLING FROM THE DB
  let current = await admin.database().ref('/lobbies/0/');
  console.log(current)
  let interval = setInterval(() => {
    reduceTime();
    if(radius === 0){
      clearInterval(interval);
      radius = 10
      admin.database().ref('/lobbies/0/').update({radius: radius});
      res.end()
    }
  }, 1000)
});

async function reduceTime(){
  radius--;
  admin.database().ref('/lobbies/0/').update({radius: radius});
}