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

	commandtoclient(%client, 'CM_Organizations_manageOrganization', %id, %organization.name);
}

// ============================================================
// Section 2 - Requests
// ============================================================

function servercmdCM_Organizations_requestOrganizations(%client) {
	for(%i = 0; %i < CM_Organizations.dataTable.keys.length; %i++) {
		%organization = CM_Organizations.getData(%id = CM_Organizations.dataTable.keys.value[%i]);

		if(%organization.hidden && !%organization.isInvited(%client.bl_id) && !%organization.memberExists(%client.bl_id)) {
			continue;
		}

		commandtoclient(%client, 'CM_Organizations_addOrganization', %id, %organization.type, %organization.open, CM_Players.getData(%organization.owner).name, %organization.name, (%organization.members.length + 1), %organization.getJobOpenings(), (%organization.memberExists(%client.bl_id) || (%organization.owner == %client.bl_id)));
	}
}

function servercmdCM_Organizations_requestUserPrivilegeLevel(%client, %id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rUPL(1)", "INVALID_ID");
		return;
	}

	commandtoclient(%client, 'CM_Organizations_setUserPrivilegeLevel', CM_Organizations.getData(%id).getUserPrivilegeLevel(%client.bl_id));
}

function servercmdCM_Organizations_requestOverviewInfo(%client, %id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rOI(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	%openings = 0;
	%avgpay = 0;

	for(%i = 0; %i < %organization.jobs.keys.length; %i++) {
		%job = %organization.jobs.get(%organization.jobs.keys.value[%i]);
		%openings += %job.get("Openings");
		%avgpay += (%job.get("Pay") / %organization.jobs.keys.length);
	}

	if(!%organization.memberExists(%client.bl_id)) {
		commandtoclient(%client, 'CM_Organizations_setOverviewInfo', %organization.name, %organization.founded, CM_Players.getData(%organization.founder).name, CM_Players.getData(%organization.owner).name, %organization.members.length, %organization.jobs.keys.length, %openings, %avgpay);
	} else {
		%member = %organization.members.value[%organization.findMember(%client.bl_id)];
		%memberJob = %organization.jobs.get(%member.get("JobID"));

		commandtoclient(%client, 'CM_Organizations_setOverviewInfo', %organization.name, %organization.founded, CM_Players.getData(%organization.founder).name, CM_Players.getData(%organization.owner).name, %organization.members.length, %organization.jobs.keys.length, %openings, %avgpay, %member.get("Hired"), %memberJob.get("Name"), %memberJob.get("Pay"), %member.get("Infractions"));
	}
}

function servercmdCM_Organizations_requestGeneralInfo(%client, %id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rGI(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 1) {
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

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 1) {
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
		%hired = %organization.members.value[%i].get("Hired");
		%infractions = %organization.members.value[%i].get("Infractions");

		commandtoclient(%client, 'CM_Organizations_addMember', %bl_id, %name, %jobID, %jobName, %hired, %infractions);
	}
}

function servercmdCM_Organizations_requestJobConstraints(%client) {
	commandtoclient(%client, 'CM_Organizations_setJobConstraints', $CM::Config::Organizations::MaxJobDescLength, $CM::Config::Organizations::MaxJobPay);
}

function servercmdCM_Organizations_requestJobModification(%client, %id, %jobID) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJM(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJM(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	if(!%organization.jobExists(%jobID)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJM(3)", "INVALID_JOBID");
		return;
	}

	%job = %organization.jobs.get(%jobID);

	commandtoclient(%client, 'CM_Organizations_setJobModification', %job.get("Name"), %job.get("Description"), %job.get("Pay"), %job.get("Openings"), %job.get("Auto Accept"));
}

function servercmdCM_Organizations_requestJobs(%client, %id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJ(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 1) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJ(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	for(%i = 0; %i < %organization.jobs.keys.length; %i++) {
		%job = %organization.jobs.get(%organization.jobs.keys.value[%i]);
		commandtoclient(%client, 'CM_Organizations_addJob', %organization.jobs.keys.value[%i], %job.get("Name"));
	}
}

function servercmdCM_Organizations_requestJobGroups(%client, %id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJ(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 1) {
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

function servercmdCM_Organizations_requestAllSkills(%client) {
	for(%i = 0; %i < CM_SkillsInfo.skillsets.keys.length; %i++) {
		%skillset = CM_SkillsInfo.skillsets.keys.value[%i];
		%skillsetName = CM_SkillsInfo.getSkillSetName(%skillset);

		for(%j = 0; %j < CM_SkillsInfo.getSkillset(%skillset).recordsList.length; %j++) {
			%skill = CM_SkillsInfo.getSkillset(%skillset).recordsList.value[%j];
			%skillName = CM_SkillsInfo.getSkillset(%skillset).getRecord(%skill, "Name");

			if(%skillName $= "order") {
				continue;
			}

			commandtoclient(%client, 'CM_Organizations_addJobAllSkill', %skillset, %skillsetName, %skill, %skillName);
		}
	}
}

function servercmdCM_Organizations_requestJobSkills(%client, %id, %jobID) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJS(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJS(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	if(!%organization.jobExists(%jobID)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJS(3)", "NONEXISTENT_JOB");
		return;
	}

	for(%i = 0; %i < CM_SkillsInfo.skillsets.keys.length; %i++) {
		%skillsetName = CM_SkillsInfo.getSkillSetName(%skillset = CM_SkillsInfo.skillsets.keys.value[%i]);
		%skillsetSO = CM_SkillsInfo.getSkillSet(%skillset);
		while((%skill = %skillsetSO.getRecord("order", %index)) !$= "") {
			commandtoclient(%client, 'CM_Organizations_addJobSkill', %skillset, %skillsetName, %skill, %skillsetSO.getRecord(%skill, "Name"));
			%index++;
		}
	}
}

function servercmdCM_Organizations_requestAllTasks(%client) {
	for(%i = 0; %i < CM_TasksInfo.recordsList.length; %i++) {
		%taskID = CM_TasksInfo.recordsList.value[%i];
		%taskName = CM_TasksInfo.getRecord(%taskID, "Name");
		%taskDescription = CM_TasksInfo.getRecord(%taskID, "Description");

		commandtoclient(%client, 'CM_Organizations_addJobAllTask', %taskID, %taskName, %taskDescription);
	}
}

function servercmdCM_Organizations_requestJobTasks(%client, %id, %jobID) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJT(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJT(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	if(!%organization.jobExists(%jobID)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJT(3)", "NONEXISTENT_JOB");
		return;
	}

	for(%i = 0; %i < %organization.jobs.get(%jobID).get("Tasks").length; %i++) {
		%taskID = %organization.jobs.get(%jobID).get("Tasks").value[%i];
		%taskName = CM_TasksInfo.getRecord(%taskID, "Name");
		%taskDescription = CM_TasksInfo.getRecord(%taskID, "Description");

		commandtoclient(%client, 'CM_Organizations_addJobTask', %taskID, %taskName, %taskDescription);
	}
}

function servercmdCM_Organizations_requestJobType(%client, %id, %jobID) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJT(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJT(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	if(!%organization.jobExists(%jobID)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJT(3)", "NONEXISTENT_JOB");
		return;
	}

	commandtoclient(%client, 'CM_Organizations_setJobTaskType', %organization.jobs.get(%jobID).get("Type"));
}

// Deprecated
function servercmdCM_Organizations_requestInvitations(%client, %id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rI(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 1) {
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

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rA(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	for(%i = 0; %i < %organization.applications.length; %i++) {
		%application = %organization.applications.value[%i];
		commandtoclient(%client, 'CM_Organizations_addApplication',
			%application.get("BL_ID"),
			CM_Players.getData(%application.get("BL_ID")).name,
			%application.get("JobID"),
			%organization.jobs.get(%application.get("JobID")).get("Name")
		);
	}
}

function servercmdCM_Organizations_requestBalance(%client, %id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rB(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
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

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
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

function servercmdCM_Organizations_disbandOrganization(%client, %id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_dO(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 3) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_dO(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	%organization.disband();
	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "As of" SPC CM_Tick.getLongDate() @ ", the organization formerly known as" SPC %organization.name SPC "is now defunct.");
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
			%player = CM_Players.getData(%client.bl_id);
			commandtoclient(%client, 'CM_Organizations_viewAvailableJobs', %id, %organization.name, Stringify::serialize(CM_Players.getData(%client.bl_id).skills, true));

			for(%i = 0; %i < %organization.jobs.keys.length; %i++) {
				%job = %organization.jobs.get(%organization.jobs.keys.value[%i]);

				if(%job.get("Openings") < 1) {
					continue;
				}

				commandtoclient(%client, 'CM_Organizations_addAvailableJob', %job.get("Name"), %job.get("Description"), %job.get("Pay"), %job.get("Type"), %job.get("Openings"), %job.get("Auto Accept"));

				for(%i = 0; %i < %job.get("Prerequisites").length; %i++) {
					%skillID = %job.get("Prerequisites").value[%i];
					%skillset = getWord(strReplace(%skillID, ":", " "), 0);
					%skill = getWord(strReplace(%skillID, ":", " "), 1);

					commandtoclient(%client, 'CM_Organizations_addAvailableJobSkill', %skillID, CM_SkillsInfo.getSkillSetName(%skillset), CM_SkillsInfo.getSkillSet(%skillset).getRecord(%skill, "Name"));
				}
			}
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

function servercmdCM_Organizations_leaveOrganization(%client, %id) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_lO(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 1) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_lO(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	%return = %organization.kickMember(%client.bl_id);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "NONEXISTENT_MEMBER":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A Member in this organization with the BL_ID of" SPC %client.bl_id SPC "does not exist!");
				return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_O_lO(3)", getWord(%return, 1)); return;
		}
	}

	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You have left the organization," SPC %organization.name);
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

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
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

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
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

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
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

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
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

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
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

function servercmdCM_Organizations_createJob(%client, %id, %name, %description, %pay, %openings, %autoaccept) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_cJ(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_cJ(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	%return = %organization.createJob(%name, %description, %pay, %openings, %autoaccept);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "MAX_JOB_AMOUNT": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "There are too many jobs in this Organization!"); return;
			case "INVALID_NAME": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The name of a Job cannot be blank!"); return;
			case "INVALID_NAME_LENGTH": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The name given is too long!"); return;
			case "INVALID_DESC": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The description of a Job cannot be blank!"); return;
			case "INVALID_DESC_LENGTH": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The description given is too long!"); return;
			case "INVALID_PAY": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The pay of a Job cannot be blank or a non-integer!"); return;
			case "INVALID_PAY_AMT": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The pay given is out of the valid range!"); return;
			case "INVALID_OPENINGS": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The openings amount of a Job cannot be blank or a non-integer!"); return;
			case "INVALID_OPENINGS_AMT": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The openings amount given is out of the valid range!"); return;
			case "INVALID_AUTOACCEPT": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The auto-accept value given must be a boolean!"); return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_O_cJ(3)", getWord(%return, 1)); return;
		}
	}

	%jobID = %organization.jobs.keys.value[%organization.jobs.keys.length];

	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A Job with the ID of \"" @ %jobID @ "\" and a name of \"" @ %name @ "\" has been created.");
	commandtoclient(%client, 'CM_Organizations_addJob', %jobID, %name);
	commandtoclient(%client, 'CM_Organizations_closeJobModification');
}

function servercmdCM_Organizations_updateJob(%client, %id, %jobID, %name, %description, %pay, %openings, %autoaccept) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_uJ(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_uJ(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	%return = %organization.updateJob(%jobID, %name, %description, %pay, %openings, %autoaccept);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "INVALID_JOBID": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must specify a valid, integer ID of a Job to update!"); return;
			case "NONEXISTENT_JOB": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A Job by this ID does not exist!"); return;
			case "INVALID_NAME": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The name of a Job cannot be blank!"); return;
			case "INVALID_NAME_LENGTH": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The name given is too long!"); return;
			case "INVALID_DESC": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The description of a Job cannot be blank!"); return;
			case "INVALID_DESC_LENGTH": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The description given is too long!"); return;
			case "INVALID_PAY": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The pay of a Job cannot be blank or a non-integer!"); return;
			case "INVALID_PAY_AMT": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The pay given is out of the valid range!"); return;
			case "INVALID_OPENINGS": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The openings amount of a Job cannot be blank or a non-integer!"); return;
			case "INVALID_OPENINGS_AMT": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The openings amount given is out of the valid range!"); return;
			case "INVALID_AUTOACCEPT": commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The auto-accept value given must be a boolean!"); return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_O_cJ(3)", getWord(%return, 1)); return;
		}
	}

	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The job," SPC %name SPC "(#" @ %jobID @ "), has been successfully updated.");
	commandtoclient(%client, 'CM_Organizations_closeJobModification');
	commandtoclient(%client, 'CM_Organizations_updateJobName', %jobID, %name);
}

function servercmdCM_Organizations_deleteJob(%client, %id, %jobID) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_dJ(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
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

function servercmdCM_Organizations_setJobType(%client, %id, %jobID, %type) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_sJT(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_sJT(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	if((%type !$= "commission") && (%type !$= "salary")) {
		commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "Invalid job payment type given, must be either \"commission\" or \"salary\"");
	}

	%organization.jobs.get(%jobID).set("Type", strLwr(%type));
}

function servercmdCM_Organizations_addJobSkill(%client, %id, %jobID, %skillID) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_aJS(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_aJS(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	%return = %organization.addJobSkill(%jobID, %skillID);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "INVALID_JOBID":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must specify a valid, integer ID of a Job to add a Skill to it!");
				return;
			case "NONEXISTENT_JOB":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A Job by this ID does not exist!");
				return;
			case "INVALID_SKILLID":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must specify a valid, non-blank ID of a Skill to add!");
				return;
			case "NONEXISTENT_SKILL":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A Skill by this ID does not exist!");
				return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_O_aJS(3)", getWord(%return, 1)); return;
		}
	}

	%skillset = getWord(strReplace(%skillID, ":", " "), 0);
	%skill = getWord(strReplace(%skillID, ":", " "), 1);

	commandtoclient(%client, 'CM_Organizations_addJobSkill', %skillset, CM_SkillsInfo.getSkillSetName(%skillset), %skill, CM_SkillsInfo.getSkillSet(%skillset).getRecord(%skill, "Name"));
}

function servercmdCM_Organizations_removeJobSkill(%client, %id, %jobID, %skillID) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJS(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJS(2)", "INSUFFICIENT_PERMISSION");
		return;
	}

	%return = %organization.removeJobSkill(%jobID, %skillID);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "INVALID_JOBID":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must specify a valid, integer ID of a Job to remove a Skill from it!");
				return;
			case "NONEXISTENT_JOB":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A Job by this ID does not exist!");
				return;
			case "INVALID_SKILLID":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must specify a valid, non-blank ID of a Skill to remove!");
				return;
			case "NONEXISTENT_SKILL":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "A Skill by this ID does not exist!");
				return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_O_rJS(3)", getWord(%return, 1)); return;
		}
	}

	commandtoclient(%client, 'CM_Organizations_removeJobTask', getWord(strReplace(%skillID, ":", " "), 0), getWord(strReplace(%skillID, ":", " "), 1));
}

function servercmdCM_Organizations_addJobTask(%client, %id, %jobID, %taskID) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_aJT(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
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

	commandtoclient(%client, 'CM_Organizations_addJobTask', %taskID, CM_TasksInfo.getRecord(%taskID, "Name"));
}

function servercmdCM_Organizations_removeJobTask(%client, %id, %jobID, %taskID) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_rJT(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
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

	commandtoclient(%client, 'CM_Organizations_removeJobTask', %taskID);
}

function servercmdCM_Organizations_changeMemberJob(%client, %id, %bl_id, %jobID) {
	if(!strLen(%id) || !CM_Organizations.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_O_cMJ(1)", "INVALID_ID");
		return;
	}

	%organization = CM_Organizations.getData(%id);

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
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

	if(%organization.getUserPrivilegeLevel(%client.bl_id) < 2) {
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
