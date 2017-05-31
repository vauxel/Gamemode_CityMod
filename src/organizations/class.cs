// ============================================================
// Project          -      CityMod
// Description      -      Organization Class
// ============================================================

function CM_Organizations::createOrganization(%this, %ownerBL_ID, %name, %type) {
	if(!CM_Players.dataExists(%ownerBL_ID)) {
		CMError(2, "CityModOrganization::createOrganization() ==> A player by the ID of \"" @ %ownerBL_ID @ "\" does not exist");
		return "ERROR DOES_NOT_EXIST";
	}

	if(!strLen(%name)) {
		CMError(2, "CityModOrganization::createOrganization() ==> Invalid \"%name\" given -- cannot be blank");
		return "ERROR INVALID_NAME";
	}

	if(strLen(%name) > (%maxLen = $CM::Config::Organizations::MaxOrganizationNameLength)) {
		CMError(2, "CityModOrganization::createOrganization() ==> Invalid \"%name\" given -- too long (" @ strLen(%name) SPC ">" SPC %maxLen @ ")");
		return "ERROR INVALID_NAME_LENGTH";
	}

	if((%type !$= "group") && (%type !$= "company")) {
		CMError(2, "CityModOrganization::createOrganization() ==> Invalid \"%type\" given -- must be either 'group' or 'company'");
		return "ERROR INVALID_TYPE";
	}

	%id = 1;
	// If one plus one doesn't equal two then you have bigger problems other than this loop not running
	while((1 + 1) == 2) {
		if(!isFile(%this.getDataPath(%id))) {
			break;
		}
		%id++;
	}

	%organization = %this.addData(%id);
	%organization.owner = %ownerBL_ID;
	%organization.name = %name;
	%organization.type = properText(%type);
	%organization.founded = CM_Tick.getLongDate();
	%organization.founder = %ownerBL_ID;
	%organization.account = CM_Bank.registerAccount("organization", %id);

	%organization.createJob("Default Job", "This job's description has not been set", 0, 0, false);

	return %id;
}

function CityModOrganization::disband(%this) {
	// This seems too simple...
	CM_Bank.resolveAccountNumber(%organization.account).closeAccount();
	%this.delete();
}

function CityModOrganization::findMember(%this, %bl_id) {
	if(!strLen(%bl_id)) {
		CMError(2, "CityModOrganization::findMember() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return "ERROR INVALID_NAME";
	}

	%index = -1;

	for(%i = 0; %i < %this.members.length; %i++) {
		if(%this.members.value[%i].get("BLID") $= %bl_id) {
			%index = %i;
			break;
		}
	}

	return %index;
}

function CityModOrganization::memberExists(%this, %bl_id) {
	return %this.findMember(%bl_id) != -1;
}

function CityModOrganization::jobExists(%this, %jobID) {
	if(!strLen(%jobID)) {
		CMError(2, "CityModOrganization::jobExists() ==> Invalid \"%jobID\" given -- cannot be blank");
		return "ERROR INVALID_NAME";
	}

	return %this.jobs.exists(%jobID);
}

function CityModOrganization::getUserPrivilegeLevel(%this, %bl_id) {
	%level = 0; // No Privilege

	if((%memberIndex = %this.findMember(%bl_id)) != -1) {
		%level = 1; // Member Privilege

		if(%this.members.value[%memberIndex].get("isModerator") == true) {
			%level = 2; // Moderator Privilege
		}
	}

	if(%this.owner == %bl_id) {
		%level = 3; // Owner Privilege
	}

	return %level;
}

function CityModOrganization::createJob(%this, %name, %description, %pay, %openings, %autoaccept) {
	if(%this.jobs.keys.length >= $CM::Config::Organizations::MaxJobs) {
		CMError(2, "CityModOrganization::createJob() ==> Too many jobs already exist");
		return "ERROR MAX_JOB_AMOUNT";
	}

	if(!strLen(%name)) {
		CMError(2, "CityModOrganization::createJob() ==> Invalid \"%name\" given -- cannot be blank");
		return "ERROR INVALID_NAME";
	}

	if(strLen(%name) > $CM::Config::Organizations::MaxJobNameLength) {
		CMError(2, "CityModOrganization::createJob() ==> Invalid \"%name\" given -- too long (" @ strLen(%name) SPC ">" SPC $CM::Config::Organizations::MaxJobNameLength @ ")");
		return "ERROR INVALID_NAME_LENGTH";
	}

	if(!strLen(%description)) {
		CMError(2, "CityModOrganization::createJob() ==> Invalid \"%description\" given -- cannot be blank");
		return "ERROR INVALID_DESC";
	}

	if(strLen(%description) > $CM::Config::Organizations::MaxJobDescLength) {
		CMError(2, "CityModOrganization::createJob() ==> Invalid \"%description\" given -- too long (" @ strLen(%description) SPC ">" SPC $CM::Config::Organizations::MaxJobDescLength @ ")");
		return "ERROR INVALID_DESC_LENGTH";
	}

	if(!strLen(%pay) || !isNumber(%pay)) {
		CMError(2, "CityModOrganization::createJob() ==> Invalid \"%pay\" given -- cannot be blank or a non-integer");
		return "ERROR INVALID_PAY";
	}

	if((%pay < 0) || (%pay > $CM::Config::Organizations::MaxJobPay)) {
		CMError(2, "CityModOrganization::createJob() ==> Invalid \"%pay\" given -- out of bounds (0 < " @ %pay @ " < " @ $CM::Config::Organizations::MaxJobPay @ ")");
		return "ERROR INVALID_PAY_AMT";
	}

	if(!strLen(%openings) || !isNumber(%openings)) {
		CMError(2, "CityModOrganization::createJob() ==> Invalid \"%openings\" given -- cannot be blank or a non-integer");
		return "ERROR INVALID_OPENINGS";
	}

	if((%openings < 0) || (%openings > $CM::Config::Organizations::MaxJobOpenings)) {
		CMError(2, "CityModOrganization::createJob() ==> Invalid \"%openings\" given -- out of bounds (0 < " @ %openings @ " < " @ $CM::Config::Organizations::MaxJobOpenings @ ")");
		return "ERROR INVALID_OPENINGS_AMT";
	}

	if((%autoaccept != true) && (%autoaccept != false)) {
		CMError(2, "CityModOrganization::createJob() ==> Invalid \"%autoaccept\" given -- must be either true or false");
		return "ERROR INVALID_AUTOACCEPT";
	}

	%jobID = -1;

	for(%i = 1; %i <= $CM::Config::Organizations::MaxJobs; %i++) {
		if(!%this.jobExists(%i)) {
			%jobID = %i;
			break;
		}
	}

	if(%jobID == -1) {
		CMError(2, "CityModOrganization::createJob() ==> No unoccupied job id found");
		return "ERROR NO_JOBID_FOUND";
	}

	%job = Map();
	%job.set("Name", %name);
	%job.set("Description", %description);
	%job.set("Pay", %pay);
	%job.set("Openings", %openings);
	%job.set("Auto Accept", %autoaccept ? true : false);
	%job.set("Prerequisites", Array());
	%job.set("Tasks", Array());
	%job.set("Type", "commision");

	%this.jobs.set(%jobID, %job);
}

function CityModOrganization::updateJob(%this, %jobID, %name, %description, %pay, %openings, %autoaccept) {
	if(!strLen(%jobID) || !isNumber(%jobID)) {
		CMError(2, "CityModOrganization::updateJob() ==> Invalid \"%jobID\" given -- cannot be blank or a non-integer");
		return "ERROR INVALID_JOBID";
	}

	if(!%this.jobExists(%jobID)) {
		CMError(2, "CityModOrganization::updateJob() ==> A job by the ID of" SPC %jobID SPC "does not exist");
		return "ERROR NONEXISTENT_JOB";
	}

	if(!strLen(%name)) {
		CMError(2, "CityModOrganization::updateJob() ==> Invalid \"%name\" given -- cannot be blank");
		return "ERROR INVALID_NAME";
	}

	if(strLen(%name) > $CM::Config::Organizations::MaxJobNameLength) {
		CMError(2, "CityModOrganization::updateJob() ==> Invalid \"%name\" given -- too long (" @ strLen(%name) SPC ">" SPC $CM::Config::Organizations::MaxJobNameLength @ ")");
		return "ERROR INVALID_NAME_LENGTH";
	}

	if(!strLen(%description)) {
		CMError(2, "CityModOrganization::updateJob() ==> Invalid \"%description\" given -- cannot be blank");
		return "ERROR INVALID_DESC";
	}

	if(strLen(%description) > $CM::Config::Organizations::MaxJobDescLength) {
		CMError(2, "CityModOrganization::updateJob() ==> Invalid \"%description\" given -- too long (" @ strLen(%description) SPC ">" SPC $CM::Config::Organizations::MaxJobDescLength @ ")");
		return "ERROR INVALID_DESC_LENGTH";
	}

	if(!strLen(%pay) || !isNumber(%pay)) {
		CMError(2, "CityModOrganization::updateJob() ==> Invalid \"%pay\" given -- cannot be blank or a non-integer");
		return "ERROR INVALID_PAY";
	}

	if((%pay < 0) || (%pay > $CM::Config::Organizations::MaxJobPay)) {
		CMError(2, "CityModOrganization::updateJob() ==> Invalid \"%pay\" given -- out of bounds (0 < " @ %pay @ " < " @ $CM::Config::Organizations::MaxJobPay @ ")");
		return "ERROR INVALID_PAY_AMT";
	}

	if(!strLen(%openings) || !isNumber(%openings)) {
		CMError(2, "CityModOrganization::updateJob() ==> Invalid \"%openings\" given -- cannot be blank or a non-integer");
		return "ERROR INVALID_OPENINGS";
	}

	if((%openings < 0) || (%openings > $CM::Config::Organizations::MaxJobOpenings)) {
		CMError(2, "CityModOrganization::updateJob() ==> Invalid \"%openings\" given -- out of bounds (0 < " @ %openings @ " < " @ $CM::Config::Organizations::MaxJobOpenings @ ")");
		return "ERROR INVALID_OPENINGS_AMT";
	}

	if((%autoaccept != true) && (%autoaccept != false)) {
		CMError(2, "CityModOrganization::updateJob() ==> Invalid \"%autoaccept\" given -- must be either true or false");
		return "ERROR INVALID_AUTOACCEPT";
	}

	%job = %this.jobs.get(%jobID);
	%job.set("Name", %name);
	%job.set("Description", %description);
	%job.set("Pay", %pay);
	%job.set("Openings", %openings);
	%job.set("Auto Accept", %autoaccept ? true : false);
}

function CityModOrganization::addJobSkill(%this, %jobID, %skillID) {
	if(!strLen(%jobID)) {
		CMError(2, "CityModOrganization::addJobSkill() ==> Invalid \"%jobID\" given -- cannot be blank");
		return "ERROR INVALID_JOBID";
	}

	if(!%this.jobExists(%jobID)) {
		CMError(2, "CityModOrganization::addJobSkill() ==> A job by the ID of" SPC %jobID SPC "does not exist");
		return "ERROR NONEXISTENT_JOB";
	}

	if(!strLen(%skillID)) {
		CMError(2, "CityModOrganization::addJobSkill() ==> Invalid \"%skillID\" given -- cannot be blank");
		return "ERROR INVALID_SKILLID";
	}

	if(!CM_SkillsInfo.skillIDExists(%skillID)) {
		CMError(2, "CityModOrganization::addJobSkill() ==> A skill by the id of" SPC %skillID SPC "does not exist");
		return "ERROR NONEXISTENT_SKILL";
	}

	%this.jobs.get(%jobID).get("Prerequisites").push(%skillID);
}

function CityModOrganization::removeJobSkill(%this, %jobID, %skillID) {
	if(!strLen(%jobID)) {
		CMError(2, "CityModOrganization::removeJobSkill() ==> Invalid \"%jobID\" given -- cannot be blank");
		return "ERROR INVALID_JOBID";
	}

	if(!%this.jobExists(%jobID)) {
		CMError(2, "CityModOrganization::removeJobSkill() ==> A job by the ID of" SPC %jobID SPC "does not exist");
		return "ERROR NONEXISTENT_JOB";
	}

	if(!strLen(%skillID)) {
		CMError(2, "CityModOrganization::removeJobSkill() ==> Invalid \"%skillID\" given -- cannot be blank");
		return "ERROR INVALID_SKILLID";
	}

	%index = %this.jobs.get(%jobID).get("Prerequisites").find(%skillID);

	if(%index == -1) {
		CMError(2, "CityModOrganization::removeJobSkill() ==> A skill by the id of \"" @ %skillID @ "\" does not exist");
		return "ERROR NONEXISTENT_SKILL";
	}

	%this.jobs.get(%jobID).get("Prerequisites").pop(%index);
}

function CityModOrganization::addJobTask(%this, %jobID, %taskID) {
	if(!strLen(%jobID)) {
		CMError(2, "CityModOrganization::addJobTask() ==> Invalid \"%jobID\" given -- cannot be blank");
		return "ERROR INVALID_JOBID";
	}

	if(!%this.jobExists(%jobID)) {
		CMError(2, "CityModOrganization::addJobTask() ==> A job by the ID of" SPC %jobID SPC "does not exist");
		return "ERROR NONEXISTENT_JOB";
	}

	if(!strLen(%taskID)) {
		CMError(2, "CityModOrganization::addJobTask() ==> Invalid \"%taskID\" given -- cannot be blank");
		return "ERROR INVALID_TASKID";
	}

	if(!CM_TasksInfo.recordExists(%taskID)) {
		CMError(2, "CityModOrganization::addJobTask() ==> A task by the ID of" SPC %taskID SPC "does not exist");
		return "ERROR NONEXISTENT_TASK";
	}

	if(%this.jobs.get(%jobID).get("Tasks").length >= $CM::Config::Organizations::MaxJobTasks) {
		CMError(2, "CityModOrganization::addJobTask() ==> Too many tasks already exist for this job");
		return "ERROR MAX_TASK_AMOUNT";
	}

	%this.jobs.get(%jobID).get("Tasks").push(%taskID);
}

function CityModOrganization::removeJobTask(%this, %jobID, %taskID) {
	if(!strLen(%jobID)) {
		CMError(2, "CityModOrganization::removeJobTask() ==> Invalid \"%jobID\" given -- cannot be blank");
		return "ERROR INVALID_JOBID";
	}

	if(!%this.jobExists(%jobID)) {
		CMError(2, "CityModOrganization::removeJobTask() ==> A job by the ID of" SPC %jobID SPC "does not exist");
		return "ERROR NONEXISTENT_JOB";
	}

	if(!strLen(%taskID)) {
		CMError(2, "CityModOrganization::removeJobTask() ==> Invalid \"%taskID\" given -- cannot be blank");
		return "ERROR INVALID_TASKID";
	}

	%taskIndex = %this.jobs.get(%jobID).get("Tasks").find(%taskID);

	if(%taskIndex == -1) {
		CMError(2, "CityModOrganization::deleteJob() ==> A task by the ID of" SPC %taskID SPC "does not exist in this job");
		return "ERROR NONEXISTENT_TASK";
	}

	%this.jobs.get(%jobID).get("Tasks").pop(%taskIndex);
}

function CityModOrganization::deleteJob(%this, %jobID) {
	if(!strLen(%jobID)) {
		CMError(2, "CityModOrganization::deleteJob() ==> Invalid \"%jobID\" given -- cannot be blank");
		return "ERROR INVALID_JOBID";
	}

	if(!%this.jobExists(%jobID)) {
		CMError(2, "CityModOrganization::deleteJob() ==> A job by the ID of" SPC %jobID SPC "does not exist");
		return "ERROR NONEXISTENT_JOB";
	}

	if(%this.jobs.keys.length == 1) {
		CMError(2, "CityModOrganization::deleteJob() ==> The last Job in the Organization cannot be deleted");
		return "ERROR LAST_JOB";
	}

	for(%i = 0; %i < %this.members.length; %i++) {
		%member = %this.members.value[%i];

		if(%member.get("JobID") $= %jobID) {
			%member.set("JobID", %this.jobs.get(%this.jobs.keys.value[0]));
		}
	}

	%this.jobs.get(%jobID).delete();
	%this.jobs.remove(%jobID);
}

function CityModOrganization::getJobOpenings(%this) {
	%count = 0;

	for(%i = 0; %i < %this.jobs.keys.length; %i++) {
		%count += %this.jobs.get(%this.jobs.keys.value[%i]).get("Openings");
	}

	return %count;
}

function CityModOrganization::employPlayer(%this, %bl_id, %jobID) {
	if(!strLen(%bl_id)) {
		CMError(2, "CityModOrganization::employPlayer() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return "ERROR INVALID_BLID";
	}

	if(!CM_Players.dataExists(%bl_id)) {
		CMError(2, "CityModOrganization::employPlayer() ==> A player by the BL_ID of \"" @ %bl_id @ "\" does not exist");
		return "ERROR NONEXISTENT_BLID";
	}

	if(%this.memberExists(%bl_id)) {
		CMError(2, "CityModOrganization::employPlayer() ==> A member by the BLID of" SPC %bl_id SPC "already exists");
		return "ERROR MEMBER_EXISTS";
	}

	if(!strLen(%jobID)) {
		CMError(2, "CityModOrganization::employPlayer() ==> Invalid \"%jobID\" given -- cannot be blank");
		return "ERROR INVALID_JOBID";
	}

	if(!%this.jobExists(%jobID)) {
		CMError(2, "CityModOrganization::employPlayer() ==> A job by the ID of \"" @ %jobID @ "\" does not exist");
		return "ERROR NONEXISTENT_JOB";
	}

	if(%this.isInvited(%bl_id)) {
		%this.unInvitePlayer(%bl_id);
	}

	if(%this.hasSentApplication(%bl_id)) {
		%this.removeApplication(%bl_id);
	}

	%member = Map();
	%member.set("BLID", %bl_id);
	%member.set("JobID", %jobID);
	%member.set("isModerator", false);
	%member.set("Infractions", 0);
	%member.set("Hired", CM_Tick.getLongDate());

	%this.members.push(%member);
}

function CityModOrganization::changeMemberJob(%this, %bl_id, %jobID) {
	if(!strLen(%bl_id)) {
		CMError(2, "CityModOrganization::changeMemberJob() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return "ERROR INVALID_BLID";
	}

	%index = %this.findMember(%bl_id);

	if(%index == -1) {
		CMError(2, "CityModOrganization::changeMemberJob() ==> A member by the BLID of" SPC %bl_id SPC "does not exist");
		return "ERROR NONEXISTENT_MEMBER";
	}

	if(!strLen(%jobID)) {
		CMError(2, "CityModOrganization::changeMemberJob() ==> Invalid \"%jobID\" given -- cannot be blank");
		return "ERROR INVALID_JOBID";
	}

	if(!%this.jobExists(%jobID)) {
		CMError(2, "CityModOrganization::changeMemberJob() ==> A job by the ID of \"" @ %jobID @ "\" does not exist");
		return "ERROR NONEXISTENT_JOB";
	}

	if(%this.members.value[%index].get("JobID") $= %jobID) {
		CMError(2, "CityModOrganization::changeMemberJob() ==> The current member's job ID is the same as the \"%jobID\" given");
		return "ERROR SAME_JOB";
	}

	%this.members.value[%index].set("JobID", %jobID);
}

function CityModOrganization::kickMember(%this, %bl_id) {
	if(!strLen(%bl_id)) {
		CMError(2, "CityModOrganization::kickMember() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return "ERROR INVALID_BLID";
	}

	%index = %this.findMember(%bl_id);

	if(%index == -1) {
		CMError(2, "CityModOrganization::kickMember() ==> A member by the BLID of" SPC %bl_id SPC "does not exist");
		return "ERROR NONEXISTENT_MEMBER";
	}

	%this.members.value[%index].delete();
	%this.members.pop(%index);
}

function CityModOrganization::isInvited(%this, %bl_id) {
	if(!strLen(%bl_id)) {
		CMError(2, "CityModOrganization::isInvited() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return "ERROR INVALID_BLID";
	}

	for(%i = 0; %i < %this.invited.count; %i++) {
		if(firstWord(%this.invited.value[%i]) == %bl_id) {
			return true;
		}
	}

	return false;
}

function CityModOrganization::invitePlayer(%this, %bl_id, %jobID) {
	if(!strLen(%bl_id)) {
		CMError(2, "CityModOrganization::invitePlayer() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return "ERROR INVALID_BLID";
	}

	if(!CM_Players.dataExists(%bl_id)) {
		CMError(2, "CityModOrganization::invitePlayer() ==> A player by the BL_ID of \"" @ %bl_id @ "\" does not exist");
		return "ERROR NONEXISTENT_BLID";
	}

	if(%this.memberExists(%bl_id)) {
		CMError(2, "CityModOrganization::invitePlayer() ==> BLID \"" @ %bl_id @ "\" is already a part of the organization");
		return "ERROR IN_ORGANIZATION";
	}

	if(%this.isInvited(%bl_id)) {
		CMError(2, "CityModOrganization::invitePlayer() ==> BLID \"" @ %bl_id @ "\" has already been invited to the organization");
		return "ERROR ALREADY_INVITED";
	}

	%this.invited.push(%bl_id @ (%this.jobExists(%jobID) ? (" " @ %jobID) : ""));
}

function CityModOrganization::unInvitePlayer(%this, %bl_id) {
	if(!strLen(%bl_id)) {
		CMError(2, "CityModOrganization::unInvitePlayer() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return "ERROR INVALID_BLID";
	}

	for(%i = 0; %i < %this.invited.count; %i++) {
		if(firstWord(%this.invited.value[%i]) == %bl_id) {
			%index = %i;
			break;
		}
	}

	if(%index $= "") {
		CMError(2, "CityModOrganization::unInvitePlayer() ==> An invitation for BLID \"" @ %bl_id @ "\" does not exist");
		return "ERROR NOT_INVITED";
	}

	%this.invited.pop(%index);
}

function CityModOrganization::hasSentApplication(%this, %bl_id) {
	if(!strLen(%bl_id)) {
		CMError(2, "CityModOrganization::hasSentApplication() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return "ERROR INVALID_BLID";
	}

	for(%i = 0; %i < %this.applications.length; %i++) {
		if(%this.applications.value[%i].get("BL_ID") == %bl_id) {
			return true;
		}
	}

	return false;
}

function CityModOrganization::addApplication(%this, %bl_id, %jobID, %message) {
	if(!strLen(%bl_id)) {
		CMError(2, "CityModOrganization::addApplication() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return "ERROR INVALID_BLID";
	}

	if(%this.memberExists(%bl_id)) {
		CMError(2, "CityModOrganization::addApplication() ==> BLID \"" @ %bl_id @ "\" is already a part of the organization");
		return "ERROR IN_ORGANIZATION";
	}

	if(%this.isInvited(%bl_id)) {
		CMError(2, "CityModOrganization::addApplication() ==> BLID \"" @ %bl_id @ "\" has already been invited to this organization");
		return "ERROR ALREADY_INVITED";
	}

	if(%this.hasSentApplication(%bl_id)) {
		CMError(2, "CityModOrganization::addApplication() ==> An application for BLID \"" @ %bl_id @ "\" has already been sent");
		return "ERROR ALREADY_SENT";
	}

	if(!strLen(%jobID)) {
		CMError(2, "CityModOrganization::addApplication() ==> Invalid \"%jobID\" given -- cannot be blank");
		return "ERROR INVALID_JOBID";
	}

	if(!%this.jobExists(%jobID)) {
		CMError(2, "CityModOrganization::addApplication() ==> A job by the ID of" SPC %jobID SPC "does not exist");
		return "ERROR NONEXISTENT_JOB";
	}

	%application = Map();
	%application.set("BL_ID", %bl_id);
	%application.set("JobID", %jobID);
	%application.set("Message", %message);

	%this.applications.push(%application);
}

function CityModOrganization::removeApplication(%this, %bl_id) {
	if(!strLen(%bl_id)) {
		CMError(2, "CityModOrganization::removeApplication() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return "ERROR INVALID_BLID";
	}

	if(!%this.hasSentApplication(%bl_id)) {
		CMError(2, "CityModOrganization::removeApplication() ==> An application for BLID \"" @ %bl_id @ "\" does not exist");
		return "ERROR NO_REQUEST";
	}

	%index = -1;

	for(%i = 0; %i < %this.applications.length; %i++) {
		if(%this.applications.value[%i].get("BL_ID") == %bl_id) {
			%index = %i;
		}
	}

	%this.applications.pop(%index);
}

function CityModOrganization::acceptApplication(%this, %bl_id) {
	for(%i = 0; %i < %this.applications.length; %i++) {
		if(%this.applications.value[%i].get("BL_ID") == %bl_id) {
			%jobID = %this.applications.value[%i].get("JobID");
		}
	}

	%this.employPlayer(%bl_id, %jobID);
}

function CityModOrganization::getBankAccount(%this) {
	return CM_Bank.resolveAccountNumber(%this.account);
}

function CityModOrganization::payEmployees(%this) {
	%account = %this.getBankAccount();

	if(%account.balance <= 0) {
		return;
	}

	%payment = 0;

	for(%i = 0; %i < %this.members.length; %i++) {
		if(%this.jobs.get(%this.members.value[%i].get("JobID")).get("Type") $= "salary") {
			%payment += %this.jobs.get(%this.members.value[%i].get("JobID")).get("Pay");
		}
	}

	if(%payment > %account.balance) {
		return;
	}

	for(%i = 0; %i < %this.members.length; %i++) {
		if(%this.jobs.get(%this.members.value[%i].get("JobID")).get("Type") !$= "salary") {
			continue;
		}

		%memberAccount = CM_Bank.resolveAccountNumber(CM_Players.getData(%this.members.value[%i].get("BLID")).account);

		if(%memberAccount == -1) {
			continue;
		}

		%memberAccount.addFunds(%this.jobs.get(%this.members.value[%i].get("JobID")).get("Pay"), "Organization Paycheck");
	}

	%account.removeFunds(%payment, "Salary/Commision Payout");
}

package CityMod_Organizations {
	function CM_Tick::onWeek(%this) {
		parent::onWeek(%this);

		if(isObject(CM_Organizations)) {
			for(%i = 0; %i < CM_Organizations.dataTable.keys.length; %i++) {
				CM_Organizations.dataTable.get(CM_Organizations.dataTable.keys.value[%i]).payEmployees();
			}
		}
	}
};

if(isPackage(CityMod_Organizations))
	deactivatePackage(CityMod_Organizations);
activatePackage(CityMod_Organizations);