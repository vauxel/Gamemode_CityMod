// ============================================================
// Project          -      CityMod
// Description      -      Player Skills
// ============================================================
// Sections
//   1: Commands
//   2: Skillset Info Functions
//   3: Player Functions
// ============================================================

// ============================================================
// Section 1 - Commands
// ============================================================
function servercmdCM_Skills_requestSkillsets(%client) {
	%playerSkills = CM_Players.getData(%client.bl_id).skills;
	for(%i = 0; %i < CM_SkillsInfo.skillsets.keys.length; %i++) {
		%skillsetName = CM_SkillsInfo.skillsets.get(%skillset = CM_SkillsInfo.skillsets.keys.value[%i]);
		commandtoclient(%client, 'CM_Skills_addSkillset', %skillset, %skillsetName, %playerSkills.getPoints(%skillset), %playerSkills.getLevel(%skillset), %playerSkills.getXPPercent(%skillset));
	}
}

function servercmdCM_Skills_requestSkills(%client, %skillset) {
	if((%skillsetSO = CM_SkillsInfo.getSkillSet(%skillset)) $= "") {
		return;
	}

	%index = 1;
	while((%skill = %skillsetSO.getRecord("order", %index)) !$= "") {
		commandtoclient(%client, 'CM_Skills_addSkill', %skillset, %skill, %skillsetSO.getRecord(%skill, "Name"), %skillsetSO.getRecord(%skill, "Points"), %skillsetSO.getRecord(%skill, "Description"), %skillsetSO.getRecord(%skill, "Reqs"));
		%index++;
	}
}

function servercmdCM_Skills_requestPlayerSkills(%client) {
	%playerSkills = CM_Players.getData(%client.bl_id).skills;
	for(%i = 0; %i < %playerSkills.list.length; %i++) {
		%skill = %playerSkills.list.value[%i];
		commandtoclient(%client, 'CM_Skills_addPlayerSkill', %skill);
	}
}

function servercmdCM_Skills_unlockSkill(%client, %skillset, %skill) {
	if((%skillsetSO = CM_SkillsInfo.getSkillSet(%skillset)) $= "") {
		commandtoclient(%client, 'CM_errorMessage', "CM_S_uS(1)", "INVALID_SKILLSET");
		return;
	}

	if(!%skillsetSO.recordExists(%skill)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_S_uS(2)", "INVALID_SKILL");
		return;
	}

	%playerSkills = CM_Players.getData(%client.bl_id).skills;
	%skillID = %skillset @ ":" @ %skill;

	if(%playerSkills.hasSkill(%skillID)) {
		commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You have already unlocked this skill!");
		return;
	}

	if(%playerSkills.getPoints(%skillID) < %skillsetSO.getRecord(%skill, "Points")) {
		commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You don't have enough points to unlock this skill!");
		return;
	}

	if(!%playerSkills.meetsSkillReqs(%skillID)) {
		commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You don't meet the requirements for this skill!");
		return;
	}

	%playerSkills.addPoints(-%skillsetSO.getRecord(%skill, "Points"));
	%playerSkills.addSkill(%skillID);

	commandtoclient(%client, 'CM_Skills_showSkillUnlocked', %skillsetSO.getRecord(%skill, "Name"));
	commandtoclient(%client, 'CM_Skills_refreshSkills');
}

// ============================================================
// Section 2 - Skillset Info Functions
// ============================================================
function CM_SkillsInfo::onRemove(%this) {
	%this.skillsets.delete();
}

function CM_SkillsInfo::getSkillSet(%this, %name) {
	if(%this.skillsets.get(%name) $= "") {
		return "";
	}

	return %this.getAttribute(%name);
}

function CM_SkillsInfo::getSkillSetByName(%this, %name) {
	for(%i = 0; %i < %this.skillsets.keys.length; %i++) {
		if(%this.skillsets.get(%skillset = %this.skillsets.keys.value[%i]) $= %name) {
			return %this.getSkillSet(%skillsetID);
		}
	}
}

function CM_SkillsInfo::skillSetExists(%this, %skillset) {
	%skillset = %this.getSkillSet(%skillset);
	return (%skillset !$= "") && isObject(%skillset);
}

function CM_SkillsInfo::skillSetNameExists(%this, %name) {
	%skillset = %this.getSkillSetByName(%name);
	return (%skillset !$= "") && isObject(%skillset);
}

function CM_SkillsInfo::addSkillSet(%this, %rawname, %name) {
	if(%this.skillSetExists(%rawname)) {
		return;
	}

	%this.skillsets.set(%rawname, %name);
	%this.setAttribute(%rawname, new ScriptObject() {
    class = "CityModInfoDB";
    path = $CM::Config::Path::Mod @ "res/info/skills/" @ %rawname;
  });
}

function CM_SkillsInfo::getSkillIDFromName(%this, %name) {
	for(%i = 0; %i < %this.skillsets.keys.length; %i++) {
		%skillset = %this.getSkillSet(%skillsetID = %this.skillsets.keys.value[%i]);

		%index = 1;
		while((%skill = %skillset.getRecord("order", %index)) !$= "") {
			if(%skillset.getRecord(%skill, "Name") $= %name) {
				return (%skillsetID SPC %index);
			}

			%index++;
		}
	}
}

// ============================================================
// Section 3 - Player Functions
// ============================================================
function CityModPlayer::hasSkill(%this, %skillID) {
	return %this.skills.hasSkill(%skillID);
}

function CityModPlayerSkills::hasSkill(%this, %skillID) {
	return %this.list.contains(%skillID);
}

function CityModPlayerSkills::getSkillSet(%this, %name) {
	return %this.getAttribute(%name);
}

function CityModPlayerSkills::getPoints(%this, %skillset) { if(!CM_SkillsInfo.skillSetExists(%skillset)) { return; } return %this.getSkillSet(%skillset).get("Points"); }
function CityModPlayerSkills::getLevel(%this, %skillset) { if(!CM_SkillsInfo.skillSetExists(%skillset)) { return; } return %this.getSkillSet(%skillset).get("Level"); }

function CityModPlayerSkills::getCurrXP(%this, %skillset) {
	if(!CM_SkillsInfo.skillSetExists(%skillset)) {
		return;
	}

	return %this.getSkillSet(%skillset).get("XP");
}

function CityModPlayerSkills::getReqXP(%this, %skillset) {
	if(!CM_SkillsInfo.skillSetExists(%skillset)) {
		return;
	}

	return mFloor(10 * mPow(%this.getSkillSet(%skillset).get("Level") + 1, $CM::Config::Players::SkillXPMultiplier));
}

function CityModPlayerSkills::getXPPercent(%this, %skillset) {
	if(!CM_SkillsInfo.skillSetExists(%skillset)) {
		return;
	}

	return mFloatLength((%this.getCurrXP(%skillset) / %this.getReqXP(%skillset)) * 100, 2);
}

function CityModPlayerSkills::addXP(%this, %skillset, %value) {
	if(!CM_SkillsInfo.skillSetExists(%skillset)) {
		return;
	}

	if(%value $= "") {
		%value = 1;
	} else if(%value < 1) {
		%value = mAbs(%value);
	}

	%skillsetSO = %this.getSkillSet(%skillset);
	%value += %skillsetSO.get("XP");

	while(%value > 0) {
		%reqXP = %this.getReqXP(%skillset);
		if(%value >= %reqXP) {
			%value -= %reqXP;
			%this.addLevel(%skillset, 1);
		} else {
			%skillsetSO.set("XP", %value);
		}
	}
}

function CityModPlayerSkills::addLevel(%this, %skillset, %value) {
	if(!CM_SkillsInfo.skillSetExists(%skillset)) {
		return;
	}

	if(%value $= "") {
		%value = 1;
	} else if(%value < 1) {
		%value = mAbs(%value);
	}

	%skillset = %this.getSkillSet(%skillset);
	%skillset.set("Level", %skillset.get("Level") + %value);
	%skillset.set("Points", %skillset.get("Points") + %value);
}

function CityModPlayerSkills::addPoints(%this, %skillset, %value) {
	if(!CM_SkillsInfo.skillSetExists(%skillset)) {
		return;
	}

	if(%value $= "") {
		%value = 1;
	}

	%skillset = %this.getSkillSet(%skillset);
	%skillset.set("Points", %skillset.get("Points") + %value);
}

function CityModPlayerSkills::addSkill(%this, %skillID) {
	%skillset = getWord(strReplace(%skillID, ":", " "), 0);
	%skill = getWord(strReplace(%skillID, ":", " "), 1);

	if(!CM_SkillsInfo.skillSetExists(%skillset)) {
		return;
	}

	if(!CM_SkillsInfo.getSkillSet(%skillset).recordExists(%skill)) {
		return;
	}

	if(%this.list.contains(%skillID)) {
		return;
	}

	%this.list.push(%skillID);
}

function CityModPlayerSkills::removeSkill(%this, %skillID) {
	if((%index = %this.list.find(%skillID)) == -1) {
		return;
	}

	%this.list.pop(%index);
}

function CityModPlayerSkills::meetsSkillReqs(%this, %skillID) {
	%skillset = getWord(strReplace(%skillID, ":", " "), 0);
	%skill = getWord(strReplace(%skillID, ":", " "), 1);

	if(!CM_SkillsInfo.skillSetExists(%skillset)) {
		return;
	}

	if((%skillsetSO = CM_SkillsInfo.getSkillSet(%skillset)).recordExists(%skill) == false) {
		return;
	}

	%skillReqs = %skillsetSO.getRecord(%skill, "Reqs");

	if(%skillReqs $= "none") {
		return true;
	}

	for(%i = 0; %i < getWordCount(%skillReqs); %i++) {
		if(!%this.hasSkill(getWord(%skillReqs, %i))) {
			return false;
		}
	}

	return true;
}