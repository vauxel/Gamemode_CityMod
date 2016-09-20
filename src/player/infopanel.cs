// ============================================================
// Project          -      CityMod
// Description      -      Info Panel Updating
// ============================================================

function CityModPlayer::updateStats(%this, %client) {
	if(%client $= "") {
		%client = findclientbyBL_ID(%this.blid);
	}

	%stats = Array();
	%stats.push("BLID" TAB %this.blid); // For testing

	%statsString = Stringify::serialize(%stats, "array");
	%stats.delete();

	commandtoclient(%client, 'CM_Infopanel_updateName', %this.name);
	commandtoclient(%client, 'CM_Infopanel_updateStats', %statsString);
}

function servercmdCM_Infopanel_requestStats(%client) {
	CM_Players.getData(%client.bl_id).updateStats(%client);
}

function servercmdCM_Infopanel_requestSkillsets(%client) {
	%playerData = CM_Players.getData(%client.bl_id).skills;
	%skillsets = Array();

	for(%i = 0; %i < CM_SkillsInfo.skillsets.keys.length; %i++) {
		%skillset = CM_SkillsInfo.skillsets.keys.value[%i];
		%skillsets.push(CM_SkillsInfo.skillsets.get(%skillset) TAB %playerData.getLevel(%skillset) TAB (%playerData.getXPPercent(%skillset)));
	}

	commandtoclient(%client, 'CM_Infopanel_updateSkillsets', Stringify::serialize(%skillsets, "array"));
	%skillsets.delete();
}

package CityMod_Player_Infopanel {
	function GameConnection::applyBodyParts(%this) {
		parent::applyBodyParts(%this);
		commandtoclient(%this, 'CM_Infopanel_refreshAvatar');
	}
};

if(isPackage(CityMod_Player_Infopanel))
	deactivatePackage(CityMod_Player_Infopanel);
activatePackage(CityMod_Player_Infopanel);