// ============================================================
// Project          -      CityMod
// Description      -      Organization Class
// ============================================================

function CM_Organizations::createOrganization(%this, %ownerBL_ID, %name, %type) {
	if(!CM_Players.dataExists(%ownerBL_ID)) {
		warn("CityModOrganization::createOrganization() ==> A player by the ID of \"" @ %ownerBL_ID @ "\" does not exist");
		return "ERROR DOES_NOT_EXIST";
	}

	if(!strLen(%name)) {
		warn("CityModOrganization::createOrganization() ==> Invalid \"%name\" given -- cannot be blank");
		return "ERROR INVALID_NAME";
	}

	if(strLen(%name) > (%maxLen = $CM::Config::Organizations::MaxOrganizationNameLength)) {
		warn("CityModOrganization::createOrganization() ==> Invalid \"%name\" given -- too long (" @ strLen(%name) SPC ">" SPC %maxLen @ ")");
		return "ERROR INVALID_NAME_LENGTH";
	}

	if((%type !$= "group") && (%type !$= "company")) {
		warn("CityModOrganization::createOrganization() ==> Invalid \"%type\" given -- must be either 'group' or 'company'");
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
	%organization.account = CM_Bank.registerAccount("organization", %id);

	%organization.createJob("Default Job", "This job's description has not been set", 0, 0, true);

	return %id;
}

function CityModOrganization::memberExists(%this, %bl_id) {
	if(!strLen(%bl_id)) {
		warn("CityModOrganization::memberExists() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return "ERROR INVALID_NAME";
	}

	for(%i = 0; %i < %this.members.length; %i++) {
		if(%this.members.value[%i].get("BLID") $= %bl_id) {
			%index = %i;
			break;
		}
	}

	return (strLen(%index) ? %index : -1);
}

function CityModOrganization::jobExists(%this, %jobID) {
	if(!strLen(%jobID)) {
		warn("CityModOrganization::jobExists() ==> Invalid \"%jobID\" given -- cannot be blank");
		return "ERROR INVALID_NAME";
	}

	return isObject(%this.jobs.at(%jobID));
}

function CityModOrganization::getUserPrivelegeLevel(%this, %bl_id) {
	%level = 0; // No Privelege

	if((%memberIndex = %this.memberExists(%bl_id)) != -1) {
		%level = 1; // Member Privelege

		if(%this.members.value[%memberIndex].get("isModerator") == true) {
			%level = 2; // Moderator Privelege
		}
	}

	if(%this.owner == %bl_id) {
		%level = 3; // Owner Privelege
	}

	return %level;
}

function CityModOrganization::createJob(%this, %name, %description, %salary, %openings, %autoaccept) {
	if(%this.jobs.length >= $CM::Config::Organizations::MaxJobs) {
		warn("CityModOrganization::createJob() ==> Too many jobs already exist");
		return "ERROR MAX_JOB_AMOUNT";
	}

	if(!strLen(%name)) {
		warn("CityModOrganization::createJob() ==> Invalid \"%name\" given -- cannot be blank");
		return "ERROR INVALID_NAME";
	}

	if(strLen(%name) > $CM::Config::Organizations::MaxJobNameLength) {
		warn("CityModOrganization::createJob() ==> Invalid \"%name\" given -- too long (" @ strLen(%name) SPC ">" SPC $CM::Config::Organizations::MaxJobNameLength @ ")");
		return "ERROR INVALID_NAME_LENGTH";
	}

	if(!strLen(%description)) {
		warn("CityModOrganization::createJob() ==> Invalid \"%description\" given -- cannot be blank");
		return "ERROR INVALID_DESC";
	}

	if(strLen(%description) > $CM::Config::Organizations::MaxJobDescLength) {
		warn("CityModOrganization::createJob() ==> Invalid \"%description\" given -- too long (" @ strLen(%description) SPC ">" SPC $CM::Config::Organizations::MaxJobDescLength @ ")");
		return "ERROR INVALID_DESC_LENGTH";
	}

	if(!strLen(%salary) || !isNumber(%salary)) {
		warn("CityModOrganization::createJob() ==> Invalid \"%salary\" given -- cannot be blank or a non-integer");
		return "ERROR INVALID_SALARY";
	}

	if((%salary < 0) || (%salary > $CM::Config::Organizations::MaxJobSalary)) {
		warn("CityModOrganization::createJob() ==> Invalid \"%salary\" given -- out of bounds (0 < " @ %salary @ " < " @ $CM::Config::Organizations::MaxJobSalary @ ")");
		return "ERROR INVALID_SALARY_AMT";
	}

	if(!strLen(%openings) || !isNumber(%openings)) {
		warn("CityModOrganization::createJob() ==> Invalid \"%openings\" given -- cannot be blank or a non-integer");
		return "ERROR INVALID_OPENINGS";
	}

	if((%openings < 0) || (%openings > $CM::Config::Organizations::MaxJobOpenings)) {
		warn("CityModOrganization::createJob() ==> Invalid \"%openings\" given -- out of bounds (0 < " @ %openings @ " < " @ $CM::Config::Organizations::MaxJobOpenings @ ")");
		return "ERROR INVALID_OPENINGS_AMT";
	}

	if((%autoaccept != true) && (%autoaccept != false)) {
		warn("CityModOrganization::createJob() ==> Invalid \"%autoaccept\" given -- must be either true or false");
		return "ERROR INVALID_AUTOACCEPT";
	}

	%job = Map();
	%job.set("Name", %name);
	%job.set("Description", %description);
	%job.set("Salary", %salary);
	%job.set("Openings", %openings);
	%job.set("Auto Accept", %autoaccept ? true : false);
	%job.set("Prerequisites", Array());
	%job.set("Tasks", Array());

	%this.jobs.push(%job);
}

function CityModOrganization::updateJob(%this, %jobID, %name, %description, %salary, %openings, %autoaccept) {
	if(!strLen(%jobID) || !isNumber(%jobID)) {
		warn("CityModOrganization::updateJob() ==> Invalid \"%jobID\" given -- cannot be blank or a non-integer");
		return "ERROR INVALID_JOBID";
	}

	if(!%this.jobExists(%id, %jobID)) {
		warn("CityModOrganization::updateJob() ==> A job by the ID of" SPC %jobID SPC "does not exist");
		return "ERROR NONEXISTENT_JOB";
	}

	if(!strLen(%name)) {
		warn("CityModOrganization::updateJob() ==> Invalid \"%name\" given -- cannot be blank");
		return "ERROR INVALID_NAME";
	}

	if(strLen(%name) > $CM::Config::Organizations::MaxJobNameLength) {
		warn("CityModOrganization::updateJob() ==> Invalid \"%name\" given -- too long (" @ strLen(%name) SPC ">" SPC $CM::Config::Organizations::MaxJobNameLength @ ")");
		return "ERROR INVALID_NAME_LENGTH";
	}

	if(!strLen(%description)) {
		warn("CityModOrganization::updateJob() ==> Invalid \"%description\" given -- cannot be blank");
		return "ERROR INVALID_DESC";
	}

	if(strLen(%description) > $CM::Config::Organizations::MaxJobDescLength) {
		warn("CityModOrganization::updateJob() ==> Invalid \"%description\" given -- too long (" @ strLen(%description) SPC ">" SPC $CM::Config::Organizations::MaxJobDescLength @ ")");
		return "ERROR INVALID_DESC_LENGTH";
	}

	if(!strLen(%salary) || !isNumber(%salary)) {
		warn("CityModOrganization::updateJob() ==> Invalid \"%salary\" given -- cannot be blank or a non-integer");
		return "ERROR INVALID_SALARY";
	}

	if((%salary < 0) || (%salary > $CM::Config::Organizations::MaxJobSalary)) {
		warn("CityModOrganization::updateJob() ==> Invalid \"%salary\" given -- out of bounds (0 < " @ %salary @ " < " @ $CM::Config::Organizations::MaxJobSalary @ ")");
		return "ERROR INVALID_SALARY_AMT";
	}

	if(!strLen(%openings) || !isNumber(%openings)) {
		warn("CityModOrganization::updateJob() ==> Invalid \"%openings\" given -- cannot be blank or a non-integer");
		return "ERROR INVALID_OPENINGS";
	}

	if((%openings < 0) || (%openings > $CM::Config::Organizations::MaxJobOpenings)) {
		warn("CityModOrganization::updateJob() ==> Invalid \"%openings\" given -- out of bounds (0 < " @ %openings @ " < " @ $CM::Config::Organizations::MaxJobOpenings @ ")");
		return "ERROR INVALID_OPENINGS_AMT";
	}

	if((%autoaccept != true) && (%autoaccept != false)) {
		warn("CityModOrganization::updateJob() ==> Invalid \"%autoaccept\" given -- must be either true or false");
		return "ERROR INVALID_AUTOACCEPT";
	}

	%job = %this.jobs.at(%jobID);
	%job.set("Name", %name);
	%job.set("Description", %description);
	%job.set("Salary", %salary);
	%job.set("Openings", %openings);
	%job.set("Auto Accept", %autoaccept ? true : false);
}

function CityModOrganization::addJobSkill(%this, %jobID, %skill, %level) {
	if(!strLen(%jobID)) {
		warn("CityModOrganization::addJobSkill() ==> Invalid \"%jobID\" given -- cannot be blank");
		return "ERROR INVALID_JOBID";
	}

	if(!%this.jobExists(%jobID)) {
		warn("CityModOrganization::addJobSkill() ==> A job by the ID of" SPC %jobID SPC "does not exist");
		return "ERROR NONEXISTENT_JOB";
	}

	if(!strLen(%skill)) {
		warn("CityModOrganization::addJobSkill() ==> Invalid \"%skill\" given -- cannot be blank");
		return "ERROR INVALID_SKILL";
	}

	if(!CM_SkillsInfo.recordExists(%skill)) {
		warn("CityModOrganization::addJobSkill() ==> A skill by the name of" SPC %skill SPC "does not exist");
		return "ERROR NONEXISTENT_SKILL";
	}

	if(!strLen(%level) || !isInteger(%level)) {
		warn("CityModOrganization::addJobSkill() ==> Invalid \"%level\" given -- cannot be blank or non-integer");
		return "ERROR INVALID_LEVEL";
	}

	if((%level < 0) || (%level > $CM::Config::Players::MaxSkillLevel)) {
		warn("CityModOrganization::addJobSkill() ==> \"%level\" cannot be less than 0 or greater than" SPC $CM::Config::Players::MaxSkillLevel);
		return "ERROR INVALID_LEVEL";
	}

	%this.jobs.at(%jobID).get("Prerequisites").push(%skill TAB %level);
}

function CityModOrganization::editJobSkillLevel(%this, %jobID, %skill, %level) {
	if(!strLen(%jobID)) {
		warn("CityModOrganization::editJobSkillLevel() ==> Invalid \"%jobID\" given -- cannot be blank");
		return "ERROR INVALID_JOBID";
	}

	if(!%this.jobExists(%jobID)) {
		warn("CityModOrganization::editJobSkillLevel() ==> A job by the ID of" SPC %jobID SPC "does not exist");
		return "ERROR NONEXISTENT_JOB";
	}

	if(!strLen(%skill)) {
		warn("CityModOrganization::editJobSkillLevel() ==> Invalid \"%skill\" given -- cannot be blank");
		return "ERROR INVALID_SKILL";
	}

	%skillExists = false;
	for(%i = 0; %i < %this.jobs.at(%jobID).get("Prerequisites").length; %i++) {
		if(getField(%this.jobs.at(%jobID).get("Prerequisites").value[%i], 0) $= %skill) {
			%skillExists = true;
			%skillIndex = %i;
			break;
		}
	}

	if(!%skillExists) {
		return %this.addJobSkill(%jobID, %skill, %level);
	}

	if(!strLen(%level) || !isInteger(%level)) {
		warn("CityModOrganization::editJobSkillLevel() ==> Invalid \"%level\" given -- cannot be blank or non-integer");
		return "ERROR INVALID_LEVEL";
	}

	if(%level <= 0) {
		return %this.removeJobSkill(%jobID, %skill);
	}

	if(%level > $CM::Config::Players::MaxSkillLevel) {
		warn("CityModOrganization::addJobSkill() ==> \"%level\" cannot be greater than" SPC $CM::Config::Players::MaxSkillLevel);
		return "ERROR INVALID_LEVEL";
	}

	%this.jobs.at(%jobID).get("Prerequisites").value[%skillIndex] = %skill TAB %level;
}

function CityModOrganization::removeJobSkill(%this, %jobID, %skill) {
	if(!strLen(%jobID)) {
		warn("CityModOrganization::removeJobSkill() ==> Invalid \"%jobID\" given -- cannot be blank");
		return "ERROR INVALID_JOBID";
	}

	if(!%this.jobExists(%jobID)) {
		warn("CityModOrganization::removeJobSkill() ==> A job by the ID of" SPC %jobID SPC "does not exist");
		return "ERROR NONEXISTENT_JOB";
	}

	if(!strLen(%skill)) {
		warn("CityModOrganization::removeJobSkill() ==> Invalid \"%skill\" given -- cannot be blank");
		return "ERROR INVALID_SKILL";
	}

	%skillExists = false;
	for(%i = 0; %i < %this.jobs.at(%jobID).get("Prerequisites").length; %i++) {
		if(getField(%this.jobs.at(%jobID).get("Prerequisites").value[%i], 0) $= %skill) {
			%skillExists = true;
			%skillIndex = %i;
			break;
		}
	}

	if(!%skillExists) {
		warn("CityModOrganization::removeJobSkill() ==> A skill by the name of \"" @ %skill @ "\" does not exist");
		return "ERROR NONEXISTENT_SKILL";
	}

	%this.jobs.at(%jobID).get("Prerequisites").pop(%skillIndex);
}

function CityModOrganization::addJobTask(%this, %jobID, %taskID) {
	if(!strLen(%jobID)) {
		warn("CityModOrganization::addJobTask() ==> Invalid \"%jobID\" given -- cannot be blank");
		return "ERROR INVALID_JOBID";
	}

	if(!%this.jobExists(%jobID)) {
		warn("CityModOrganization::addJobTask() ==> A job by the ID of" SPC %jobID SPC "does not exist");
		return "ERROR NONEXISTENT_JOB";
	}

	if(!strLen(%taskID)) {
		warn("CityModOrganization::addJobTask() ==> Invalid \"%taskID\" given -- cannot be blank");
		return "ERROR INVALID_TASKID";
	}

	if(!isExplicitObject(CM_OrganizationJobTasks.getTask(%taskID))) {
		warn("CityModOrganization::addJobTask() ==> A task by the ID of" SPC %taskID SPC "does not exist");
		return "ERROR NONEXISTENT_TASK";
	}

	if(%this.jobs.at(%jobID).get("Tasks").length >= $CM::Config::Organizations::MaxJobTasks) {
		warn("CityModOrganization::addJobTask() ==> Too many tasks already exist for this job");
		return "ERROR MAX_TASK_AMOUNT";
	}

	%this.jobs.at(%jobID).get("Tasks").push(%taskID);
}

function CityModOrganization::removeJobTask(%this, %jobID, %taskID) {
	if(!strLen(%jobID)) {
		warn("CityModOrganization::removeJobTask() ==> Invalid \"%jobID\" given -- cannot be blank");
		return "ERROR INVALID_JOBID";
	}

	if(!%this.jobExists(%jobID)) {
		warn("CityModOrganization::removeJobTask() ==> A job by the ID of" SPC %jobID SPC "does not exist");
		return "ERROR NONEXISTENT_JOB";
	}

	if(!strLen(%taskID)) {
		warn("CityModOrganization::removeJobTask() ==> Invalid \"%taskID\" given -- cannot be blank");
		return "ERROR INVALID_TASKID";
	}

	%taskFound = false;
	for(%i = 0; %i < %this.jobs.at(%jobID).get("Tasks").length; %i++) {
		if(%this.jobs.at(%jobID).get("Tasks").value[%i] $= %taskID) {
			%taskFound = true;
			%index = %i;
			break;
		}
	}

	if(!%taskFound) {
		warn("CityModOrganization::deleteJob() ==> A task by the ID of" SPC %taskID SPC "does not exist in this job");
		return "ERROR NONEXISTENT_TASK";
	}

	%this.jobs.at(%jobID).get("Tasks").pop(%index);
}

function CityModOrganization::deleteJob(%this, %jobID) {
	if(!strLen(%jobID)) {
		warn("CityModOrganization::deleteJob() ==> Invalid \"%jobID\" given -- cannot be blank");
		return "ERROR INVALID_JOBID";
	}

	if(!%this.jobExists(%jobID)) {
		warn("CityModOrganization::deleteJob() ==> A job by the ID of" SPC %jobID SPC "does not exist");
		return "ERROR NONEXISTENT_JOB";
	}

	if((%this.jobs.length == 1) && (%this.jobs.value[0] $= %jobID)) {
		warn("CityModOrganization::deleteJob() ==> The last Job in the Organization cannot be deleted");
		return "ERROR LAST_JOB";
	}

	for(%i = 0; %i < %this.members.length; %i++) {
		%member = %this.members.value[%i];

		if(%member.get("JobID") $= %jobID) {
			%member.set("JobID", %this.jobs.value[0]);
		}
	}

	%this.jobs.at(%jobID).delete();
	%this.jobs.pop(%jobID);
}

function CityModOrganization::getJobOpenings(%this) {
	%count = 0;

	for(%i = 0; %i < %this.jobs.length; %i++) {
		%count += %this.jobs.value[%i].get("Openings");
	}

	return %count;
}

function CityModOrganization::employPlayer(%this, %bl_id, %jobID) {
	if(!strLen(%bl_id)) {
		warn("CityModOrganization::employPlayer() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return "ERROR INVALID_BLID";
	}

	if(!CM_Players.dataExists(%bl_id)) {
		warn("CityModOrganization::employPlayer() ==> A player by the BL_ID of \"" @ %bl_id @ "\" does not exist");
		return "ERROR NONEXISTENT_BLID";
	}

	if(%this.memberExists(%bl_id) != -1) {
		warn("CityModOrganization::employPlayer() ==> A member by the BLID of" SPC %bl_id SPC "already exists");
		return "ERROR MEMBER_EXISTS";
	}

	if(!strLen(%jobID)) {
		warn("CityModOrganization::employPlayer() ==> Invalid \"%jobID\" given -- cannot be blank");
		return "ERROR INVALID_JOBID";
	}

	if(!%this.jobExists(%jobID)) {
		warn("CityModOrganization::employPlayer() ==> A job by the ID of \"" @ %jobID @ "\" does not exist");
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

	%this.members.push(%member);
}

function CityModOrganization::changeMemberJob(%this, %bl_id, %jobID) {
	if(!strLen(%bl_id)) {
		warn("CityModOrganization::changeMemberJob() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return "ERROR INVALID_BLID";
	}

	%index = %this.memberExists(%bl_id);

	if(%index == -1) {
		warn("CityModOrganization::changeMemberJob() ==> A member by the BLID of" SPC %bl_id SPC "does not exist");
		return "ERROR NONEXISTENT_MEMBER";
	}

	if(!strLen(%jobID)) {
		warn("CityModOrganization::changeMemberJob() ==> Invalid \"%jobID\" given -- cannot be blank");
		return "ERROR INVALID_JOBID";
	}

	if(!%this.jobExists(%jobID)) {
		warn("CityModOrganization::changeMemberJob() ==> A job by the ID of \"" @ %jobID @ "\" does not exist");
		return "ERROR NONEXISTENT_JOB";
	}

	if(%this.members.value[%index].get("JobID") $= %jobID) {
		warn("CityModOrganization::changeMemberJob() ==> The current member's job ID is the same as the \"%jobID\" given");
		return "ERROR SAME_JOB";
	}

	%this.members.value[%index].set("JobID", %jobID);
}

function CityModOrganization::kickMember(%this, %bl_id) {
	if(!strLen(%bl_id)) {
		warn("CityModOrganization::kickMember() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return "ERROR INVALID_BLID";
	}

	%index = %this.memberExists(%bl_id);

	if(%index == -1) {
		warn("CityModOrganization::kickMember() ==> A member by the BLID of" SPC %bl_id SPC "does not exist");
		return "ERROR NONEXISTENT_MEMBER";
	}

	%this.members.value[%index].delete();
	%this.members.pop(%index);
}

function CityModOrganization::isInvited(%this, %bl_id) {
	if(!strLen(%bl_id)) {
		warn("CityModOrganization::isInvited() ==> Invalid \"%bl_id\" given -- cannot be blank");
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
		warn("CityModOrganization::invitePlayer() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return "ERROR INVALID_BLID";
	}

	if(!CM_Players.dataExists(%bl_id)) {
		warn("CityModOrganization::invitePlayer() ==> A player by the BL_ID of \"" @ %bl_id @ "\" does not exist");
		return "ERROR NONEXISTENT_BLID";
	}

	if(%this.memberExists(%bl_id)) {
		warn("CityModOrganization::invitePlayer() ==> BLID \"" @ %bl_id @ "\" is already a part of the organization");
		return "ERROR IN_ORGANIZATION";
	}

	if(%this.isInvited(%bl_id)) {
		warn("CityModOrganization::invitePlayer() ==> BLID \"" @ %bl_id @ "\" has already been invited to the organization");
		return "ERROR ALREADY_INVITED";
	}

	%this.invited.push(%bl_id @ (%this.jobExists(%jobID) ? (" " @ %jobID) : ""));
}

function CityModOrganization::unInvitePlayer(%this, %bl_id) {
	if(!strLen(%bl_id)) {
		warn("CityModOrganization::unInvitePlayer() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return "ERROR INVALID_BLID";
	}

	for(%i = 0; %i < %this.invited.count; %i++) {
		if(firstWord(%this.invited.value[%i]) == %bl_id) {
			%index = %i;
			break;
		}
	}

	if(%index $= "") {
		warn("CityModOrganization::unInvitePlayer() ==> An invitation for BLID \"" @ %bl_id @ "\" does not exist");
		return "ERROR NOT_INVITED";
	}

	%this.invited.pop(%index);
}

function CityModOrganization::hasSentApplication(%this, %bl_id) {
	if(!strLen(%bl_id)) {
		warn("CityModOrganization::hasSentApplication() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return "ERROR INVALID_BLID";
	}

	return %this.applications.contains(%bl_id, "field_0");
}

function CityModOrganization::addApplication(%this, %bl_id, %jobID) {
	if(!strLen(%bl_id)) {
		warn("CityModOrganization::addApplication() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return "ERROR INVALID_BLID";
	}

	if(%this.memberExists(%bl_id)) {
		warn("CityModOrganization::addApplication() ==> BLID \"" @ %bl_id @ "\" is already a part of the organization");
		return "ERROR IN_ORGANIZATION";
	}

	if(%this.isInvited(%bl_id)) {
		warn("CityModOrganization::addApplication() ==> BLID \"" @ %bl_id @ "\" has already been invited to this organization");
		return "ERROR ALREADY_INVITED";
	}

	if(%this.hasSentApplication(%bl_id)) {
		warn("CityModOrganization::addApplication() ==> An application for BLID \"" @ %bl_id @ "\" has already been sent");
		return "ERROR ALREADY_SENT";
	}

	if(!strLen(%jobID)) {
		warn("CityModOrganization::addApplication() ==> Invalid \"%jobID\" given -- cannot be blank");
		return "ERROR INVALID_JOBID";
	}

	if(!%this.jobExists(%jobID)) {
		warn("CityModOrganization::addApplication() ==> A job by the ID of" SPC %jobID SPC "does not exist");
		return "ERROR NONEXISTENT_JOB";
	}

	%this.applications.push(%bl_id TAB %jobID);
}

function CityModOrganization::removeApplication(%this, %bl_id) {
	if(!strLen(%bl_id)) {
		warn("CityModOrganization::removeApplication() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return "ERROR INVALID_BLID";
	}

	if(!%this.hasSentApplication(%bl_id)) {
		warn("CityModOrganization::removeApplication() ==> An application for BLID \"" @ %bl_id @ "\" does not exist");
		return "ERROR NO_REQUEST";
	}

	%index = %this.applications.find(%bl_id, "field_0");
	%this.applications.pop(%index);
}

function CityModOrganization::acceptApplication(%this, %bl_id) {
	%jobID = getField(%this.applications.value[%this.applications.find(%bl_id, "field_0")], 1);
	%this.employPlayer(%bl_id, %jobID);
}

function CityModOrganization::getBankAccount(%this) {
	return CM_Bank.getData(CM_Bank.resolveAccountNumber(%this.account));
}

function CityModOrganization::paySalaries(%this) {
	%account = %this.getBankAccount();

	if(%account.balance <= 0) {
		return;
	}

	%payment = 0;

	for(%i = 0; %i < %this.members.length; %i++) {
		%payment += %this.jobs.value[%this.members.value[%i].get("JobID")].get("Salary");
	}

	if(%payment > %account.balance) {
		return;
	}

	for(%i = 0; %i < %this.members.length; %i++) {
		%memberAccount = CM_Bank.resolveAccountNumber(CM_Players.getData(%this.members.value[%i].get("BLID")).account);

		if(%memberAccount $= "") {
			continue;
		}

		CM_Bank.getData(%memberAccount).addFunds(%this.jobs.value[%this.members.value[%i].get("JobID")].get("Salary"), "Salary Payment (" @ CM_Tick.getShortDate() @ ")");
	}

	%account.removeFunds(%payment, "Salary Payout (" @ CM_Tick.getShortDate() @ ")");
}

package CityMod_Organizations {
	function CM_Tick::onWeek(%this) {
		parent::onWeek(%this);

		if(isObject(CM_Organizations)) {
			for(%i = 0; %i < CM_Organizations.dataTable.keys.length; %i++) {
				CM_Organizations.dataTable.get(CM_Organizations.dataTable.keys.value[%i]).paySalaries();
			}
		}
	}
};

if(isPackage(CityMod_Organizations))
	deactivatePackage(CityMod_Organizations);
activatePackage(CityMod_Organizations);