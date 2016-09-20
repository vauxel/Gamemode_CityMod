// ============================================================
// Project          -      CityMod
// Description      -      Organization Tasks Code
// ============================================================

function CM_OrganizationJobTasks::onAdd(%this) {
	if(%this.getClassName() !$= "ScriptGroup") {
		CMError(0, "CM_OrganizationJobTasks::onAdd", "CM_OrganizationJobTasks was not instantiated as a ScriptGroup");
		%this.delete();
		return;
	}
}

function CM_OrganizationJobTasks::onRemove(%this) {
	%this.deleteAll();
}

function CM_OrganizationJobTasks::getTask(%this, %id) {
	if(!strLen(%id)) {
		CMError(2, "CM_OrganizationJobTasks::getTask", "Invalid \"%id\" given -- cannot be blank");
		return;
	}

	for(%i = 0; %i < %this.getCount(); %i++) {
		if(%this.getObject(%i).taskID $= %id) {
			return %this.getObject(%i);
		}
	}
}

function CM_OrganizationJobTasks::registerTask(%this, %id, %name, %description) {
	if(!strLen(%id)) {
		CMError(2, "CM_OrganizationJobTasks::registerTask", "Invalid \"%id\" given -- cannot be blank");
		return;
	}

	if(!strLen(%name)) {
		CMError(2, "CM_OrganizationJobTasks::registerTask", "Invalid \"%name\" given -- cannot be blank");
		return;
	}

	if(!strLen(%description)) {
		CMError(2, "CM_OrganizationJobTasks::registerTask", "Invalid \"%description\" given -- cannot be blank");
		return;
	}

	if(isObject(%this.getTask(%id))) {
		CMError(2, "CM_OrganizationJobTasks::registerTask", "A task with the \"%id\" given already exists");
		return;
	}

	%task = new ScriptObject() {
		taskID = %id;
		taskName = %name;
		taskDescription = %name;
	};

	%this.add(%task);
}

function CM_OrganizationJobTasks::unregisterTask(%this, %id) {
	if(!strLen(%id)) {
		CMError(2, "CM_OrganizationJobTasks::unregisterTask", "Invalid \"%id\" given -- cannot be blank");
		return;
	}

	if(!isObject(%this.getTask(%id))) {
		CMError(2, "CM_OrganizationJobTasks::unregisterTask", "A task with the \"%id\" given does not exist");
		return;
	}

	%this.getTask(%id).delete();
}

function CM_OrganizationJobTasks::completeTask(%this, %id, %caller) {
	if(!strLen(%id)) {
		CMError(2, "CM_OrganizationJobTasks::completeTask", "Invalid \"%id\" given -- cannot be blank");
		return;
	}

	if(!strLen(%caller)) {
		CMError(2, "CM_OrganizationJobTasks::completeTask", "Invalid \"%caller\" given -- cannot be blank");
		return;
	}

	if(!isObject(%caller)) {
		CMError(2, "CM_OrganizationJobTasks::completeTask", "Invalid \"%caller\" given -- does not exist");
		return;
	}

	if(!isObject(%this.getTask(%id))) {
		CMError(2, "CM_OrganizationJobTasks::completeTask", "A task with the \"%id\" given does not exist");
		return;
	}

	// To-Do
}