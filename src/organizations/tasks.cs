// ============================================================
// Project          -      CityMod
// Description      -      Organization Tasks Code
// ============================================================

function GameConnection::progressTask(%client, %id, %amount, %callertype, %caller) {
	if(!CM_TasksInfo.recordExists(%id)) {
		CMError(2, "GameConnection::progressTask", "Invalid \"%id\" given -- does not exist");
		return;
	}

	if((%callertype !$= "CONSOLE") && !isObject(%caller)) {
		CMError(2, "GameConnection::progressTask", "\"%caller\" given does not exist");
		return;
	}

	%clientData = CM_Players.getData(%client.bl_id);

	if(%callertype $= "BRICK") {
		%property = %caller.getProperty();

		if(!isObject(%property)) {
			CMError(2, "GameConnection::progressTask", "Property for the brick by the ID of" SPC %caller SPC "does not exist");
			return;
		}

		%propertyData = CM_RealEstate.getData(%property.propertyID);

		if(%propertyData.proprietorship !$= "organization") {
			return;
		}

		CM_Organizations.getData(%propertyData.owner).progressJobTask(%client.bl_id, %id, %amount);
	} else {
		for(%i = 0; %i < %clientData.organizations.length; %i++) {
			%organization = CM_Organizations.getData(%clientData.organizations.value[%i]);
			%organization.progressJobTask(%client.bl_id, %id, %amount);
		}
	}

	servercmdCM_Infopanel_requestTasks(%client);
}

function CityModOrganization::refreshMemberTasks(%this, %bl_id) {
	if(!strLen(%bl_id)) {
		CMError(2, "CityModOrganization::refreshMemberTasks() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return;
	}

	%memberIndex = %this.findMember(%bl_id);

	if(%memberIndex == -1) {
		CMError(2, "CityModOrganization::refreshMemberTasks() ==> The member BL_ID of" SPC %bl_id SPC "is not a member of the organization");
		return;
	}

	%member = %this.members.value[%memberIndex];

	%member.get("Current Tasks").clear();
	%member.get("Completed Tasks").clear();

	%jobTasks = %this.jobs.get(%member.get("JobID")).get("Tasks");

	for(%i = 0; %i < %jobTasks.length; %i++) {
		%member.get("Current Tasks").push(getField(%jobTasks.value[%i], 0) TAB "0");
	}
}

function CityModOrganization::progressJobTask(%this, %bl_id, %taskID, %amount) {
	if(!strLen(%bl_id)) {
		CMError(2, "CityModOrganization::progressJobTask() ==> Invalid \"%bl_id\" given -- cannot be blank");
		return;
	}

	if(!strLen(%amount)) {
		%amount = 1;
	}

	if(!isInteger(%amount)) {
		CMError(2, "CityModOrganization::progressJobTask() ==> Invalid \"%amount\" given -- must be a valid integer");
		return;
	}

	%memberIndex = %this.findMember(%bl_id);

	if(%memberIndex == -1) {
		CMError(2, "CityModOrganization::progressJobTask() ==> The member BL_ID of" SPC %bl_id SPC "is not a member of the organization");
		return;
	}

	%member = %this.members.value[%memberIndex];

	if(!%this.jobExists(%member.get("JobID"))) {
		CMError(2, "CityModOrganization::progressJobTask() ==> A job by the ID of" SPC %member.get("JobID") SPC "does not exist");
		return;
	}

	%job = %this.jobs.get(%member.get("JobID"));
	%taskIndex = %job.get("Tasks").find(%taskID, "field:0");

	if(%taskIndex == -1) {
		return;
	}

	%currentTaskIndex = %member.get("Current Tasks").find(%taskID, "field:0");

	if(%currentTaskIndex == -1) { // Player has already completed the task
		return;
	} else {
		%amount += getField(%member.get("Current Tasks").value[%currentTaskIndex], 1);

		if(%amount >= getField(%job.get("Tasks").value[%taskIndex], 1)) {
			if(%job.get("Type") $= "salary") {
				%member.get("Current Tasks").pop(%currentTaskIndex);
				%member.get("Completed Tasks").push(%taskID);
			} else if(%job.get("Type") $= "commission") {
				while(%amount >= getField(%job.get("Tasks").value[%taskIndex], 1)) {
					%amount -= getField(%job.get("Tasks").value[%taskIndex], 1);
					%member.get("Completed Tasks").push(%taskID);
				}

				%member.get("Current Tasks").value[%currentTaskIndex] = %taskID TAB %amount;
			}
		} else {
			%member.get("Current Tasks").value[%currentTaskIndex] = %taskID TAB %amount;
		}
	}
}