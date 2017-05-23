// ============================================================
// Project          -      CityMod
// Description      -      Organization Tasks Code
// ============================================================

function CM_TasksInfo::completeTask(%this, %id, %caller) {
	if(!strLen(%id)) {
		CMError(2, "CM_TasksInfo::completeTask", "Invalid \"%id\" given -- cannot be blank");
		return;
	}

	if(!strLen(%caller)) {
		CMError(2, "CM_TasksInfo::completeTask", "Invalid \"%caller\" given -- cannot be blank");
		return;
	}

	if(!isObject(%caller)) {
		CMError(2, "CM_TasksInfo::completeTask", "Invalid \"%caller\" given -- does not exist");
		return;
	}

	if(!isObject(%this.getTask(%id))) {
		CMError(2, "CM_TasksInfo::completeTask", "A task with the \"%id\" given does not exist");
		return;
	}

	// To-Do
}