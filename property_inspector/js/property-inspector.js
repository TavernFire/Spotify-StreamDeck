// global websocket, used to communicate from/to Stream Deck software
// as well as some info about our plugin, as sent by Stream Deck software 
var websocket = null,
  uuid = null,
  inInfo = null,
  actionInfo = {},
  settingsModel = {
	ClientID: "",
  	ClientSecret: ""
  };

function connectElgatoStreamDeckSocket(inPort, inUUID, inRegisterEvent, inInfo, inActionInfo) {
  uuid = inUUID;
  actionInfo = JSON.parse(inActionInfo);
  inInfo = JSON.parse(inInfo);
  websocket = new WebSocket('ws://localhost:' + inPort);

  //initialize values
  if (actionInfo.payload.settings.settingsModel) {
	settingsModel.ClientID = actionInfo.payload.settings.settingsModel.ClientID;
    settingsModel.ClientSecret = actionInfo.payload.settings.settingsModel.ClientSecret;
  }

  document.getElementById('txtClientIDValue').value = settingsModel.ClientID;
  document.getElementById('txtClientSecretValue').value = settingsModel.ClientSecret;

  websocket.onopen = function () {
	var json = { event: inRegisterEvent, uuid: inUUID };
	// register property inspector to Stream Deck
	websocket.send(JSON.stringify(json));

  };

  websocket.onmessage = function (evt) {
	// Received message from Stream Deck
	var jsonObj = JSON.parse(evt.data);
	var sdEvent = jsonObj['event'];
	switch (sdEvent) {
	  case "didReceiveSettings":
		if (jsonObj.payload.settings.settingsModel.ClientID) {
		  settingsModel.ClientID = jsonObj.payload.settings.settingsModel.ClientID;
		  document.getElementById('txtClientIDValue').value = settingsModel.ClientID;
		}
	    if (jsonObj.payload.settings.settingsModel.ClientSecret) {
		  settingsModel.ClientSecret = jsonObj.payload.settings.settingsModel.ClientSecret;
		  document.getElementById('txtClientSecretValue').value = settingsModel.ClientSecret;
		}
		break;
	  default:
		break;
	}
  };
}

const setSettings = (value, param) => {
  if (websocket) {
	settingsModel[param] = value;
	var json = {
	  "event": "setSettings",
	  "context": uuid,
	  "payload": {
		"settingsModel": settingsModel
	  }
	};
	websocket.send(JSON.stringify(json));
  }
};

