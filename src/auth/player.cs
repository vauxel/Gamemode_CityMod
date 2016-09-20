// ============================================================
// Project          -      CityMod
// Description      -      Player Authentication
// ============================================================

package CityMod_Auth_Player {
	function GameConnection::sendCMAuthConfirmation(%client) {
		commandtoclient(%client, 'CM_authConfirmed');
	}

	function GameConnection::onConnectRequest(%client, %e1, %lanname, %name, %e2, %e3, %e4, %rtb, %payload) {
		if(firstWord(%payload) $= "CITYMOD") {
			echo(" - Connection request from client with the CityMod Client");

			%client.hasCMClient = true;
			%client.schedule(10, "sendCMAuthConfirmation");
		}

		parent::onConnectRequest(%client, %e1, %lanname, %name, %e2, %e3, %e4, %rtb, %payload);
	}

	function GameConnection::autoAdminCheck(%client) {
		parent::autoAdminCheck(%client);

		CM_Players.getData(%client.bl_id).ip = %client.getRawIP();
		CM_Players.getData(%client.bl_id).blid = %client.bl_id;
		CM_Players.getData(%client.bl_id).name = %client.name;
	}
};

if(isPackage(CityMod_Auth_Player))
	deactivatePackage(CityMod_Auth_Player);
activatePackage(CityMod_Auth_Player);