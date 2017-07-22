// ============================================================
// Project          -      CityMod
// Description      -      Organization Tasks Code
// ============================================================

function GameConnection::completeTask(%client, %id, %callertype, %caller) {
	if(!CM_TasksInfo.recordExists(%id)) {
		CMError(2, "GameConnection::completeTask", "Invalid \"%id\" given -- does not exist");
		return;
	}

	if(!isObject(%caller)) {
		CMError(2, "GameConnection::completeTask", "\"%caller\" given does not exist");
		return;
	}

	%clientData = CM_Players.getData(%client.bl_id);

	if(%callertype $= "BRICK") {
		%property = %caller.getProperty();

		if(!isObject(%property)) {
			CMError(2, "GameConnection::completeTask", "Property for the brick by the ID of" SPC %caller SPC "does not exist");
			return;
		}

		%propertyData = CM_RealEstate.getData(%property.propertyID);

		if(%propertyData.proprietorship !$= "organization") {
			return;
		}

		CM_Organizations.getData(%propertyData.owner).completeJobTask(%bl_id, %id);
	} else {
		for(%i = 0; %i < %clientData.organizations.length; %i++) {
			%organization = CM_Organizations.getData(%clientData.organizations.value[%i]);
			%organization.completeJobTask(%bl_id, %id);
		}
	}
}

function CityModOrganization::completeJobTask(%this, %bl_id, %taskID) {
	if(!strLen(%bl_id)) {
		CMError(2, "CityModOrganization::completeJobTask() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return;
	}

	%member = %this.findMember(%bl_id);

	if(%member == -1) {
		CMError(2, "CityModOrganization::completeJobTask() ==> The member BL_ID of" SPC %bl_id SPC "is not a member of the organization");
		return;
	}

	if(!%this.jobExists(%member.get("JobID"))) {
		CMError(2, "CityModOrganization::completeJobTask() ==> A job by the ID of" SPC %member.get("JobID") SPC "does not exist");
		return;
	}

	%job = %this.jobs.get(%member.get("JobID"));
	%taskIndex = %job.get("Tasks").find(%taskID);

	if(%taskIndex == -1) {
		return;
	}

	if((%job.get("Type") $= "salary") && inWords(%member.get("Completed Tasks"), %taskID)) {
		return;
	}

	%member.set("Completed Tasks", setWord(%member.get("Completed Tasks"), getWordCount(%member.get("Completed Tasks")), %taskID));
}