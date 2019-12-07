const functions = require('firebase-functions');
// The Firebase Admin SDK to access the Firebase Realtime Database.
const admin = require('firebase-admin');
admin.initializeApp();

/* function: reduceHP
 * description: reduces the HP of a player by 5
 * trigger: https call
 */
exports.reduceHP = functions.https.onCall((data, context) => {
	const enemyID = data.enemyId;
	const attackerID = data.attackerId;

	admin.database().ref('/users/'+enemyID+'/health').transaction(health => {
		if(health === null)
			return null;
		else if(health === 0)
			return 0;
		else {
			admin.database().ref('/users/'+enemyID+'/lastAttackedBy').set(attackerID);
			return health - 5;
		}
	})
});

/* function: joinLobby
 * description: adds a user to a lobby
 * trigger: https call
 */
exports.joinLobby = functions.https.onCall((data, context) => {
	const playerID = data.playerId;
	const playerUsername = data.username;
	const lobbyID = data.lobbyId;

	admin.database().ref('/users/'+playerID+'/lobby').transaction(lobby => {
		admin.database().ref('/lobbies/'+lobbyID+'/players/'+playerID).set(playerUsername);
		return lobbyID;
	}, function(){
		admin.database().ref('/lobbies/'+lobbyID+'/playerNum').transaction(count => {
			return count + 1;
		})
	})
});

/* function: exitLobby
 * description: removes a user from a lobby
 * trigger: https call
 */
exports.exitLobby = functions.https.onCall((data, context) => {
	const playerID = data.playerId;
	const lobbyID = data.lobbyId;

	admin.database().ref('/users/'+playerID+'/lobby').transaction(lobby => {
		admin.database().ref('/lobbies/'+lobbyID+'/players/'+playerID).remove();
		return "null";
	}, function(){
		admin.database().ref('/lobbies/'+lobbyID+'/playerNum').transaction(count => {
			return count - 1;
		})
	})
});

/* function: setActive
 * description: sets the lobby as active when min player achieved 
 *		so no one can leave but more people can join
 * trigger: playerNum >= 4
 */
exports.setActive = functions.database.ref('/lobbies/{lobbyId}/playerNum')
	.onUpdate((change, context) => {
	
	if(parseInt(change.after.val().toString(), 10) >= 2) {
		var lobbyRef = change.after.ref.parent;
		var isActiveRef = lobbyRef.child('isActive');

		isActiveRef.transaction(time => {
			return 1;
		})
	}
});

/* function: setInactive
 * description: sets the lobby as inactive when game is over
 * trigger: playerNum == 1
 */
exports.setInactive = functions.database.ref('/lobbies/{lobbyId}/playerNum')
	.onUpdate((change, context) => {
	
	if(parseInt(change.after.val().toString(), 10) === 1) {
		var lobbyRef = change.after.ref.parent;
		var isActiveRef = lobbyRef.child('isActive');

		isActiveRef.transaction(time => {
			return 0;
		})
	}
});

/* function: setInProgress
 * description: sets the game in progress when timer is over so no one
 *		can join lobby
 * trigger: timer == 0
 */
exports.setInProgress = functions.database.ref('/lobbies/{lobbyId}/timer')
	.onUpdate((change, context) => {
	
	if(parseInt(change.after.val().toString(), 10) === 0) {
		var lobbyRef = change.after.ref.parent;
		var isActiveRef = lobbyRef.child('inProgress');

		isActiveRef.transaction(time => {
			return 1;
		})
	}
});

/* function: setInProgress
 * description: sets the game not in progress when game is over
 * trigger: playerNum == 1
 */
exports.setNotInProgress = functions.database.ref('/lobbies/{lobbyId}/playerNum')
	.onUpdate((change, context) => {
	
	if(parseInt(change.after.val().toString(), 10) === 1) {
		var lobbyRef = change.after.ref.parent;
		var isActiveRef = lobbyRef.child('inProgress');

		isActiveRef.transaction(time => {
			return 0;
		})
	}
});

/* function: resetLobby
 * description: removes the winning player from the lobby
 * trigger: isActive == 0
 */
 exports.resetLobby = functions.database.ref('lobbies/{lobbyId}/isActive')
	.onUpdate((change, context) => {
	
	if(parseInt(change.after.val().toString(), 10) === 0) {
		var lobbyRef = change.after.ref.parent;
		var playerNum = lobbyRef.child('playerNum');
		var players = lobbyRef.child('players');

		players.set(null);
		playerNum.transaction(num => {
			return 0;
		})
	}
});

/* function: setDeath
 * description: removes a player from a lobby when their HP is 0
 * trigger: player.health == 0
 */
exports.setDeath = functions.database.ref('/users/{userID}/health')
	.onUpdate((change, context) => {
	
	if(parseInt(change.before.val().toString(), 10) <= 0)
		return null;

	if(parseInt(change.after.val().toString(), 10) <= 0) {
		change.after.ref.parent.child('lobby').once('value').then(snap => {
			var lobbyID = snap.val();
			var lobbyRef = change.after.ref.parent.parent.parent.child('lobbies');
			var uid = context.params.userID.toString();
			return lobbyRef.child(lobbyID).child('playerNum').transaction(count => {
				lobbyRef.child(lobbyID).child('players').child(uid).remove();
				return count - 1;
			}, function(){
				change.after.ref.parent.child('lastAttackedBy').once('value').then(snap => {
					var killerID = snap.val();

					admin.database().ref('/users/'+killerID+'/kills').transaction(kills => {
						if(kills === null)
							return null;
						else
							return kills+1;
					}, function(){
						change.after.ref.parent.child('deaths').transaction(deaths => {
							if(deaths === null)
								return null;
							else
								return deaths+1;
						})
					})
				});
			})
		})
	}
});

/* function: startTimer
 * description: starts a 30 second timer when lobby is active
 * trigger: isActive == 1
 */
exports.startTimer = functions.database.ref('/lobbies/{lobbyId}/isActive')
	.onUpdate((change, context) => {
	
	if(parseInt(change.after.val().toString(), 10) === 1) {
		var lobbyRef = change.after.ref.parent;
		var timerRef = lobbyRef.child('timer');

		timerRef.once('value').then(snap => {
			let time = snap.val();

			return functionTimer(time, 1,
				elapsedTime => {
					timerRef.set(elapsedTime);
				})
				.then(totalTime => {
					console.log('Timer of ' + totalTime + ' has finished.');
					timerRef.set(30);
				})
				.then(() => new Promise(resolve => setTimeout(resolve, 1000)))
				.catch(error => console.error(error));
		})
	}
});

/* function: functionTimer
 * description: decrements a counter starting at seconds by delta every 1s
 */
function functionTimer (seconds, delta, call) {
    return new Promise((resolve, reject) => {
		outsideResolve = resolve;

        if (seconds > 300) {
            reject('execution would take too long...');
            return;
        }

        let interval = setInterval(onInterval, 1000);
        let elapsedSeconds = 0;

        function onInterval () {
            if (elapsedSeconds >= seconds) {
                clearInterval(interval);
                call(0);
                resolve(elapsedSeconds);
                return;
            }
            call(seconds - elapsedSeconds);
            elapsedSeconds += delta;
        }
    });
}

/* function: shrinkRadius
 * description: reduces the radius from 250 to 5 over the span of 5 minutes
 *		and resets it if the game is over
 * trigger: when inProgress gets set to 1 or 0
 */
exports.shrinkRadius = functions.database.ref('/lobbies/{lobbyId}/inProgress')
	.onUpdate((change, context) => {
	
	var lobbyRef = change.after.ref.parent;
	var radiusRef = lobbyRef.child('radius');
	var lobbyID = context.params.lobbyId;
	
	if(parseInt(change.after.val().toString(), 10) === 1) {
		radiusRef.once('value').then(snap => {
			let radius = parseInt(snap.val().toString());

			let interval = setInterval(() => {
				admin.database().ref('/lobbies/'+lobbyID+'/inProgress').once('value')
					.then(snap2 => {
					
					if(parseInt(snap2.val().toString()) === 0) {
						clearInterval(interval);
						radiusRef.set(250);
						return null;
					}
				})

				if(radius <= 10) {
						clearInterval(interval);
						return null;
				}

				radius = radius - 0.8;
				radiusRef.set(radius);
			}, 1000)
		})
	} else {
		radiusRef.set(250);
	}
});


/*
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
}*/