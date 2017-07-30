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

	%statsString = Stringify::serialize(%stats, true, "array");
	%stats.delete();

	commandtoclient(%client, 'CM_Infopanel_updateName', %this.name);
	commandtoclient(%client, 'CM_Infopanel_updateStats', %statsString);
}

function servercmdCM_Infopanel_requestStats(%client) {
	CM_Players.getData(%client.bl_id).updateStats(%client);
}

function servercmdCM_Infopanel_requestSkillset(%client, %skillset) {
	if(!CM_SkillsInfo.skillSetExists(%skillset)) {
		return;
	}

	%playerData = CM_Players.getData(%client.bl_id).skills;
	commandtoclient(%client, 'CM_Infopanel_updateSkillset', %skillset, CM_SkillsInfo.skillsets.get(%skillset), %playerData.getLevel(%skillset), %playerData.getXPPercent(%skillset));
}

function servercmdCM_Infopanel_requestSkillsets(%client) {
	%playerData = CM_Players.getData(%client.bl_id).skills;

	commandtoclient(%client, 'CM_Infopanel_clearSkillsets');

	for(%i = 0; %i < CM_SkillsInfo.skillsets.keys.length; %i++) {
		%skillset = CM_SkillsInfo.skillsets.keys.value[%i];
		commandtoclient(%client, 'CM_Infopanel_addSkillset', %skillset, CM_SkillsInfo.skillsets.get(%skillset), %playerData.getLevel(%skillset), %playerData.getXPPercent(%skillset));
	}
}

function servercmdCM_Infopanel_requestTasks(%client) {
	%playerData = CM_Players.getData(%client.bl_id);

	commandtoclient(%client, 'CM_Infopanel_clearTasks');

	for(%i = 0; %i < %playerData.organizations.length; %i++) {
		%organization = CM_Organizations.getData(%playerData.organizations.value[%i]);
		%memberData = %organization.members.value[%organization.findMember(%client.bl_id)];
		%job = %organization.jobs.get(%memberData.get("JobID"));
		%jobTasks = %job.get("Tasks");

		%currentTasks = %memberData.get("Current Tasks");

		for(%j = 0; %j < %currentTasks.length; %j++) {
			%currentTask = %currentTasks.value[%j];
			%totalCount = getField(%jobTasks.value[%jobTasks.find(getField(%currentTask, 0), "field:0")], 1);
			commandtoclient(%client, 'CM_Infopanel_addTask', getField(%currentTask, 0), CM_TasksInfo.getRecord(getField(%currentTask, 0), "Name"), getField(%currentTask, 1), %totalCount);
		}

		//if(%job.get("Type") $= "commission") {
		//	continue;
		//}

		%completedTasks = %memberData.get("Completed Tasks");

		for(%j = 0; %j < %completedTasks.length; %j++) {
			%completedTask = %completedTasks.value[%j];
			%totalCount = getField(%jobTasks.value[%jobTasks.find(%completedTask, "field:0")], 1);
			commandtoclient(%client, 'CM_Infopanel_addTask', %completedTask, CM_TasksInfo.getRecord(%completedTask, "Name"), %totalCount, %totalCount);
		}
	}
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