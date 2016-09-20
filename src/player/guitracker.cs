// ============================================================
// Project          -      CityMod
// Description      -      Client GUI Tracking
// ============================================================

function servercmdCM_GUI_onClose(%client) {
	%client.closeGui();
}

function GameConnection::openGui(%this, %name, %source) {
	if(%this.currentGUI["Name"] !$= "") {
		%this.closeGUI(%name);
	}

	%this.currentGUI["Name"] = %name;
	%this.currentGUI["Source"] = isObject(%source) ? %source : "";

	commandtoclient(%this, 'CM_openGUI', %name);
	%this.onOpenGui(%name, %source);
}

function GameConnection::closeGui(%this, %name) {
	if((%this.currentGUI["Name"] $= "") || (%this.currentGUI["Name"] !$= %name)) {
		return;
	}

	commandtoclient(%this, 'CM_closeGUI', %name);
	%this.onCloseGui(%this.currentGUI["Name"], %this.currentGUI["Source"]);

	%this.currentGUI["Name"] = "";
	%this.currentGUI["Source"] = "";
}

function GameConnection::onOpenGui(%name, %source) { }
function GameConnection::onCloseGui(%name, %source) { }

package CityMod_Player_GUITracker {
	function GameConnection::onDeath(%this, %obj, %killer, %type, %area) {
		if(%this.currentGUI["Name"] !$= "") {
			%this.closeGui(%this.currentGUI["Name"]);
		}

		parent::onDeath(%this, %obj, %killer, %type, %area);
	}
};

if(isPackage(CityMod_Player_GUITracker))
	deactivatePackage(CityMod_Player_GUITracker);
activatePackage(CityMod_Player_GUITracker);