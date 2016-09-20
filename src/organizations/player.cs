// ============================================================
// Project          -      CityMod
// Description      -      Player Organization Commands
// ============================================================
// Sections
//   1: Misc
//   2: Requests
//   3: Commands
// ============================================================

// ============================================================
// Section 1 - Misc
// ============================================================

function servercmdCM_Organizations_manageOrganization(%client, %id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_mO(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 1) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_mO(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	commandtoclient(%client, 'CM_Organizations_manageOrganization', %id, %organization.name);
}

// ============================================================
// Section 2 - Requests
// ============================================================

function servercmdCM_Organizations_requestOrganizations(%client) {
	for(%i = 0; %i < CM_Organizations.dataTable.keys.length; %i++) {
		%organization = CM_Organizations.getData(%id = CM_Organizations.dataTable.keys.value[%i]);

		if(%organization.hidden && !%organization.isInvited(%client.bl_id) && (%organization.memberExists(%client.bl_id) == -1)) {
			continue;
		}

		commandtoclient(%client, 'CM_Organizations_addOrganization', %id, %organization.type, %organization.open, %organization.owner, %organization.name, (%organization.members.length + 1), %organization.getJobOpenings(), ((%organization.memberExists(%client.bl_id) != -1) || (%organization.owner == %client.bl_id)));
	}
}

function servercmdCM_Organizations_requestUserPrivelegeLevel(%client, %id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rUPL(1)", "INVALID_ID");
		return;
	}

	commandtoclient(%client, 'CM_Organizations_setUserPrivelegeLevel', CM_Organizations.getData(%id).getUserPrivelegeLevel(%client.bl_id));
}

function servercmdCM_Organizations_requestGeneralInfo(%client, %id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rGI(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 1) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rGI(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	commandtoclient(%client, 'CM_Organizations_setGeneralInfo', %organization.name, %organization.type, %organization.owner, %organization.open, %organization.hidden, %organization.description);
}

function servercmdCM_Organizations_requestMembers(%client, %id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rM(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 1) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rM(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	// Send the organization owner as a member
	commandtoclient(%client, 'CM_Organizations_addMember', %organization.owner, CM_Players.getData(%organization.owner).name, "", "", true);

	for(%i = 0; %i < %organization.members.length; %i++) {
		%bl_id = %organization.members.value[%i].get("BLID");
		%name = CM_Players.getData(%organization.members.value[%i].get("BLID")).name;
		%jobID = %organization.members.value[%i].get("JobID");
		%jobName = %organization.jobs.get(%organization.members.value[%i].get("JobID")).get("Name");

		commandtoclient(%client, 'CM_Organizations_addMember', %bl_id, %name, %jobID, %jobName);
	}
}

function servercmdCM_Organizations_requestJobConstraints(%client) {
	commandtoclient(%client, 'CM_Organizations_setJobConstraints', $CM::Config::Organizations::MaxJobDescLength, $CM::Config::Organizations::MaxJobSalary);
}

function servercmdCM_Organizations_requestJobModification(%client, %id, %jobID) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJM(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJM(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	if(!%organization.jobExists(%jobID)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJM(3)", "INVALID_JOBID");
		return;
	}

	%job = %organization.jobs.get(%jobID);

	commandtoclient(%client, 'CM_Organizations_setJobModification', %job.get("Name"), %job.get("Description"), $CM::Config::Organizations::MaxJobDescLength, %job.get("Salary"), $CM::Config::Organizations::MaxJobSalary, %job.get("Openings"), %job.get("Auto Accept"));
}

function servercmdCM_Organizations_requestJobs(%client, %id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJ(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 1) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJ(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	for(%i = 0; %i < %organization.jobs.keys.length; %i++) {
		%values = %organization.jobs.values();

		%name = %values.value[%i].get("Name");
		%salary = %values.value[%i].get("Salary");
		%tasksAmount = getWordCount(%values.value[%i].get("Tasks"));

		commandtoclient(%client, 'CM_Organizations_addJob', %organization.jobs.keys.value[%i], %name, %salary, %tasksAmount);
	}
}

function servercmdCM_Organizations_requestJobGroups(%client, %id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJ(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 1) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJ(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	for(%i = 0; %i < %organization.members.length; %i++) {
		%member = %organization.members.value[%i];
		%jobID = %member.get("JobID");
		%jobName = %organization.jobs.get(%jobID).get("Name");
		commandtoclient(%client, 'CM_Organizations_addJobGroupMember', %jobID, %jobName, CM_Players.getData(%member.get("BLID")).name);
	}
}

function servercmdCM_Organizations_requestJobSkills(%client, %id, %jobID) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJS(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJS(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	if(!%organization.jobExists(%jobID)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJS(3)", "NONEXISTENT_JOB");
		return;
	}

	for(%i = 0; %i < CM_SkillsInfo.skillsets.keys.length; %i++) {
		%skillsetName = CM_SkillsInfo.skillsets.get(%skillset = CM_SkillsInfo.skillsets.keys.value[%i]);
		%skillsetSO = CM_SkillsInfo.getSkillSet(%skillset);
		while((%skill = %skillsetSO.getRecord("order", %index)) !$= "") {
			commandtoclient(%client, 'CM_Organizations_addJobSkill', %skillset, %skillsetName, %skill, %skillsetSO.getRecord(%skill, "Name"));
			%index++;
		}
	}
}

function servercmdCM_Organizations_requestAllTasks(%client) {
	for(%i = 0; %i < CM_OrganizationJobTasks.getCount(); %i++) {
		%taskID = CM_OrganizationJobTasks.getObject(%i).taskID;
		%taskName = CM_OrganizationJobTasks.getObject(%i).taskName;
		%taskDescription = CM_OrganizationJobTasks.getObject(%i).taskDescription;

		commandtoclient(%client, 'CM_Organizations_addAllTask', %taskID, %taskName, %taskDescription);
	}
}

function servercmdCM_Organizations_requestJobTasks(%client, %id, %jobID) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJT(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 1) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJT(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	if(!%organization.jobExists(%jobID)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJT(3)", "NONEXISTENT_JOB");
		return;
	}

	for(%i = 0; %i < %organization.jobs.get(%jobID).get("Tasks").length; %i++) {
		%taskID = %organization.jobs.get(%jobID).get("Tasks").value[%i];
		%taskName = CM_OrganizationJobTasks.getTask(%taskID).taskName;

		commandtoclient(%client, 'CM_Organizations_addJobTask', %taskID, %taskName);
	}
}

// Deprecated
function servercmdCM_Organizations_requestInvitations(%client, %id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rI(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 1) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rI(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	for(%i = 0; %i < %organization.invited.length; %i++) {
		commandtoclient(%client, 'CM_Organizations_addOrganizationInvitation', %organization.invited.value[%i]);
	}
}

function servercmdCM_Organizations_requestApplications(%client, %id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rA(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rA(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	for(%i = 0; %i < %organization.applications.length; %i++) {
		%application = %organization.applications.value[%i];
		commandtoclient(%client, 'CM_Organizations_addApplication',
			getField(%application, 0),
			CM_Players.getData(getField(%application, 1)).name,
			getField(%application, 1),
			%organization.jobs.get(getField(%application, 1)).get("Name")
		);
	}
}

function servercmdCM_Organizations_requestBalance(%client, %id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rB(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rB(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	commandtoclient(%client, 'CM_Organizations_setBalance', %organization.getBankAccount().balance);
}

function servercmdCM_Organizations_requestLedger(%client, %id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rL(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rL(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	%account = %organization.getBankAccount();

	for(%i = 0; %i < %account.ledger.length; %i++) {
		%record = %account.ledger.value[%i];
		%month = getWord(strReplace(getField(%record, 0), "/", " "), 0);
		commandtoclient(%client, 'CM_Organizations_addLedgerRecord', %month, getField(%record, 2), getField(%record, 1), CM_Tick.month);
	}
}

// ============================================================
// Section 3 - Commands
// ============================================================

function servercmdCM_Organizations_createOrganization(%client, %name, %type) {
	%return = CM_Organizations.createOrganization(%client.bl_id, %name, %type);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "INVALID_NAME":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The name for an organization cannot be blank!");
				return;
			case "INVALID_NAME_LENGTH":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The name given is too long (" @ strLen(%name) @ "/" @ $CM::Config::Organizations::MaxOrganizationNameLength @ ")!");
				return;
			case "INVALID_TYPE":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The organization type given has to be either 'group' or 'company'!");
				return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_O_cO(1)", getWord(%return, 1)); return;
		}
	}

	commandtoclient(%client, 'CM_Organizations_manageOrganization', %return, %name);
	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You successfully created a" SPC properText(%type) SPC "Organization (ID #" @ %return @ ") by the name of" SPC %name @ "!");
}

function servercmdCM_Organizations_joinOrganization(%client, %id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_jO(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.hidden && !%organization.isInvited(%client.bl_id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_jO(2)", "HIDDEN");
		return;
	}

	if(!%organization.open && !%organization.isInvited(%client.bl_id)) {
		if(%organization.hasSentApplication(%client.bl_id)) {
			commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You can't apply more than once to the same organization!");
		} else {
			// TO-DO (When adding criminal system, change "false" parameter to reflect if the player was a criminal or not)
			%player = CM_Players.getData(%client.bl_id);
			commandtoclient(%client, 'CM_Organizations_openApplicationForm', %organization.name, %player.name, false, Stringify::serialize(%player.skills));
		}
		return;
	}

	%return = %organization.employPlayer(%client.bl_id, %organization.jobs.keys.value[0]);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "MEMBER_EXISTS":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You're already a part of this organization!");
				return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_O_jO(3)", getWord(%return, 1)); return;
		}
	}

	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You have joined the organization," SPC %organization.name @ "!");

	commandtoclient(%client, 'CM_Organizations_clearOrganizations');
	servercmdCM_Organizations_requestOrganizations(%client);
}

function servercmdCM_Organizations_sendApplication(%client, %id, %jobID) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_sA(1)", "INVALID_ID");
		return;
	}

	%return = CM_Organizations.getData(%id).addApplication(%client.bl_id, %jobID);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "IN_ORGANIZATION":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "Somehow, you're already a part of this organization.");
				return;
			case "ALREADY_INVITED":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You have already been invited to this organization.");
				return;
			case "ALREADY_SENT":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You cannot send more than one application!");
				return;
			case "INVALID_JOBID":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must select a job to apply to!");
				return;
			case "NONEXISTENT_JOB":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A job by this ID does not exist!");
				return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_O_sA(2)", getWord(%return, 1)); return;
		}
	}

	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "Your job application has been successfully submitted!");
}

function servercmdCM_Organizations_editField(%client, %id, %field, %value) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_eF(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_eF(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	switch$(%field) {
		case "name":
			if((%currLen = strLen(%value)) > (%maxLen = $CM::Config::Organizations::MaxOrganizationNameLength)) {
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The name given is too long (" @ %currLen @ "/" @ %maxLen @ ")!");
				return;
			}

			%organization.name = %value;
			commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The organization's name has been changed to \"" @ %value @ "\".");
		case "owner":
			if(!isInteger(%value)) {
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The value given is not a valid BLID!");
				return;
			}

			if(!CM_Players.dataExists(%value)) {
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A player by the BLID of" SPC %value SPC "does not exist!");
				return;
			}

			%organization.owner = %value;
			commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "Ownership of the organization has been transferred to BLID" SPC %value @ ".");
		case "open":
			if((%value != true) && (%value != false)) {
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "Something went wrong -- value must be either 'true' or 'false'!");
				return;
			}

			%organization.open = %value;
			commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The organization now" SPC (%value ? "does not require" : "requires") SPC "applications for entrance.");
		case "hidden":
			if((%value != true) && (%value != false)) {
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "Something went wrong -- value must be either 'true' or 'false'!");
				return;
			}

			%organization.hidden = %value;
			commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The organization is now" SPC (%value ? "not " : "") @ "visible to the public.");
		case "description":
			if((%currLen = strLen(%value)) > (%maxLen = $CM::Config::Organizations::MaxOrganizationDescLength)) {
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The description given is too long (" @ %currLen @ "/" @ %maxLen @ ")!");
				return;
			}

			%organization.description = %value;
			commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The organization's description has been changed to \"" @ %value @ "\".");
		default:
			commandtoclient(%client, 'CM_errorMessage', "CM_O_eF(2)", "INVALID_FIELD");
			return;
	}

	servercmdCM_Organizations_requestGeneralInfo(%client, %id);
}

function servercmdCM_Organizations_acceptApplication(%client, %id, %bl_id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_aA(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_aA(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	%return = %organization.acceptApplication(%bl_id);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "INVALID_BLID":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must specify a valid, integer BLID of a player to accept their application!");
				return;
			case "NO_REQUEST":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A player by the BLID of" SPC %bl_id SPC "has not sent an application.");
				return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_O_aA(3)", getWord(%return, 1)); return;
		}
	}

	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "BLID" SPC %bl_id @ "'s application was accepted.");
}

function servercmdCM_Organizations_declineApplication(%client, %id, %bl_id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_dA(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_dA(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	%return = %organization.removeApplication(%bl_id);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "INVALID_BLID":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must specify a valid, integer BLID of a player to decline their application!");
				return;
			case "NO_REQUEST":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A player by the BLID of" SPC %bl_id SPC "has not sent an application.");
				return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_O_dA(3)", getWord(%return, 1)); return;
		}
	}

	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "BLID" SPC %bl_id @ "'s application was declined.");
}

function servercmdCM_Organizations_sendInvitation(%client, %id, %bl_id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_sI(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_sI(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	%return = %organization.invitePlayer(%bl_id);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "INVALID_BLID":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must specify a valid, integer BLID of a player to invite!");
				return;
			case "NONEXISTENT_BLID":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A player by the BLID of" SPC %bl_id SPC "does not exist!");
				return;
			case "IN_ORGANIZATION":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "BLID" SPC %bl_id SPC "is already in the organization!");
				return;
			case "ALREADY_INVITED":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "BLID" SPC %bl_id SPC "has already been invited to the organization!");
				return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_O_sI(3)", getWord(%return, 1)); return;
		}
	}

	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A request to join the organization has been sent to BLID" SPC %bl_id @ ".");
}

function servercmdCM_Organizations_revokeInvitation(%client, %id, %bl_id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rI(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rI(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	%return = %organization.unInvitePlayer(%bl_id);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "INVALID_BLID":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must specify a valid, integer BLID of a player to revoke their invitation!");
				return;
			case "NOT_INVITED":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "An invitation for BLID" SPC %bl_id SPC "does not exist. They may have recently joined the organization.");
				return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_O_rI(3)", getWord(%return, 1)); return;
		}
	}

	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "BLID" SPC %bl_id @ "'s invitation into the organization has been revoked.");
}

function servercmdCM_Organizations_createJob(%client, %id, %name, %description, %salary, %openings, %autoaccept) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_cJ(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_cJ(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	%jobID = %organization.jobs.length;
	%return = %organization.createJob(%name, %description, %salary, %openings, %autoaccept);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "MAX_JOB_AMOUNT": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "There are too many jobs in this Organization!"); return;
			case "INVALID_NAME": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The name of a Job cannot be blank!"); return;
			case "INVALID_NAME_LENGTH": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The name given is too long!"); return;
			case "INVALID_DESC": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The description of a Job cannot be blank!"); return;
			case "INVALID_DESC_LENGTH": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The description given is too long!"); return;
			case "INVALID_SALARY": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The salary of a Job cannot be blank or a non-integer!"); return;
			case "INVALID_SALARY_AMT": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The salary given is out of the valid range!"); return;
			case "INVALID_OPENINGS": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The openings amount of a Job cannot be blank or a non-integer!"); return;
			case "INVALID_OPENINGS_AMT": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The openings amount given is out of the valid range!"); return;
			case "INVALID_AUTOACCEPT": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The auto-accept value given must be a boolean!"); return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_O_cJ(3)", getWord(%return, 1)); return;
		}
	}

	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A Job with the ID of \"" @ %jobID @ "\" and a name of \"" @ %name @ "\" has been created.");
	commandtoclient(%client, 'CM_Organizations_addJob', %jobID, %name);
	commandtoclient(%client, 'CM_Organizations_closeJobModification');
}

function servercmdCM_Organizations_updateJob(%client, %id, %jobID, %name, %description, %salary, %openings, %autoaccept) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_uJ(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_uJ(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	%return = %organization.updateJob(%jobID, %name, %description, %salary, %openings, %autoaccept);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "INVALID_JOBID": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must specify a valid, integer ID of a Job to update!"); return;
			case "NONEXISTENT_JOB": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A Job by this ID does not exist!"); return;
			case "INVALID_NAME": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The name of a Job cannot be blank!"); return;
			case "INVALID_NAME_LENGTH": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The name given is too long!"); return;
			case "INVALID_DESC": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The description of a Job cannot be blank!"); return;
			case "INVALID_DESC_LENGTH": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The description given is too long!"); return;
			case "INVALID_SALARY": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The salary of a Job cannot be blank or a non-integer!"); return;
			case "INVALID_SALARY_AMT": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The salary given is out of the valid range!"); return;
			case "INVALID_OPENINGS": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The openings amount of a Job cannot be blank or a non-integer!"); return;
			case "INVALID_OPENINGS_AMT": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The openings amount given is out of the valid range!"); return;
			case "INVALID_AUTOACCEPT": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The auto-accept value given must be a boolean!"); return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_O_cJ(3)", getWord(%return, 1)); return;
		}
	}

	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The job," SPC %name SPC "(#" @ %jobID @ "), has been successfully updated.");
	commandtoclient(%client, 'CM_Organizations_closeJobModification');
}

function servercmdCM_Organizations_deleteJob(%client, %id, %jobID) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_dJ(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_dJ(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	%return = %organization.deleteJob(%jobID);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "INVALID_JOBID":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must specify a valid, integer ID of a Job to delete!");
				return;
			case "NONEXISTENT_JOB":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A Job by this ID does not exist!");
				return;
			case "LAST_JOB":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The last job in an Organzation cannot be deleted. Please create another job before deleting this one.");
				return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_O_dJ(3)", getWord(%return, 1)); return;
		}
	}

	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "Job #" @ %jobID SPC "has been deleted from the Organization. Any members holding this job have been reassigned.");
	commandtoclient(%client, 'CM_Organizations_deleteJob', %jobID);
}

function servercmdCM_Organizations_addJobTask(%client, %id, %jobID, %taskID) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_aJT(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_aJT(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	%return = %organization.addJobTask(%jobID, %taskID);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "INVALID_JOBID":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must specify a valid, integer ID of a Job to add a Task to it!");
				return;
			case "NONEXISTENT_JOB":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A Job by this ID does not exist!");
				return;
			case "INVALID_TASKID":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must specify a valid, non-blank ID of a Task to add!");
				return;
			case "NONEXISTENT_TASK":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A Task by this ID does not exist!");
				return;
			case "MAX_TASK_AMOUNT":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "There are too many tasks assigned to this Job!");
				return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_O_aJT(3)", getWord(%return, 1)); return;
		}
	}

	commandtoclient(%client, 'CM_Organizations_addJobTask', %taskID, CM_OrganizationJobTasks.getTask(%taskID).taskName);
}

function servercmdCM_Organizations_removeJobTask(%client, %id, %jobID, %taskID) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJT(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJT(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	%return = %organization.removeJobTask(%jobID, %taskID);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "INVALID_JOBID":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must specify a valid, integer ID of a Job to remove a Task from it!");
				return;
			case "NONEXISTENT_JOB":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A Job by this ID does not exist!");
				return;
			case "INVALID_TASKID":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must specify a valid, non-blank ID of a Task to remove!");
				return;
			case "NONEXISTENT_TASK":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A Task by this ID does not exist!");
				return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_O_rJT(3)", getWord(%return, 1)); return;
		}
	}

	commandtoclient(%client, 'CM_Organizations_removeOrganizationJobTask', %taskID);
}

function servercmdCM_Organizations_setJobSkillLevel(%client, %id, %jobID, %skill, %level) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_sJSL(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_sJSL(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	%return = %organization.editJobSkillLevel(%jobID, %skill, %level);

	if(firstWord(%return) $= "ERROR") {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_sJSL(3)", getWord(%return, 1)); return;
	}
}

function servercmdCM_Organizations_changeMemberJob(%client, %id, %bl_id, %jobID) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_cMJ(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_cMJ(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	%return = %organization.changeMemberJob(%bl_id, %jobID);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "INVALID_BLID":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must specify a valid, integer BL_ID of a Member to kick!");
				return;
			case "NONEXISTENT_MEMBER":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A Member in this organization with the BL_ID of" SPC %bl_id SPC "does not exist!");
				return;
			case "INVALID_JOBID":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must specify a valid, integer ID of a Job to delete!");
				return;
			case "NONEXISTENT_JOB":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A Job by this ID does not exist!");
				return;
			case "SAME_JOB":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The Job chosen is the same as the current Job assigned to this Member!");
				return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_O_cMJ(3)", getWord(%return, 1)); return;
		}
	}

	%jobName = %organization.jobs.get(%organization.members.value[%index].get("JobID")).get("Name");
	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "BL_ID" SPC %bl_id @ "'s Job was changed to" SPC %jobName SPC "(#" @ %jobID @ ").");
	commandtoclient(%client, 'CM_Organizations_changeOrganizationMemberJob', %bl_id, %organization.members.value[%index].get("JobID"), %jobName);
}

function servercmdCM_Organizations_kickMember(%client, %id, %bl_id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_kM(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivelegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_kM(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	%return = %organization.kickMember(%bl_id);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "INVALID_BLID":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must specify a valid, integer BL_ID of a Member to kick!");
				return;
			case "NONEXISTENT_MEMBER":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A Member in this organization with the BL_ID of" SPC %bl_id SPC "does not exist!");
				return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_O_kM(3)", getWord(%return, 1)); return;
		}
	}

	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "BL_ID" SPC %bl_id SPC "has been kicked from the organization!");
	commandtoclient(%client, 'CM_Organizations_deleteOrganizationMember', %bl_id);
}
