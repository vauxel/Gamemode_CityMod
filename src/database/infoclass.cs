// ============================================================
// Project          -      CityMod
// Description      -      Database Code
// ============================================================

function CityModInfoDB::onAdd(%this) {
	if(!strLen(%this.path)) {
		CMError(0, "CityModInfoDB::onAdd", "No file path given");
		%this.schedule(0, "delete");
		return "ERROR";
	}

	if((getSubStr(%this.path, strLen(%this.path) - 1, 1) !$= "/") && (getSubStr(%this.path, strLen(%this.path) - 1, 1) !$= "\\")) {
		%this.path = %this.path @ "/";
	}

	%this.recordsList = Array();
	%this.loadRecords();
}

function CityModInfoDB::recordExists(%this, %recordName) {
	return (%this.recordsList.contains(%recordName) && isObject(%this._record[%recordName]));
}

function CityModInfoDB::recordFieldExists(%this, %recordName, %fieldName) {
	return (%this.recordExists(%recordName) && %this._record[%recordName].contains(%fieldName));
}

function CityModInfoDB::getRecord(%this, %recordName, %fieldName) {
	if(!%this.recordFieldExists(%recordName, %fieldName)) {
		return "";
	}

	return %this._record[%recordName, %fieldName];
}

function CityModInfoDB::loadRecord(%this, %recordName) {
	if(%this.recordExists(%recordName)) {
		%this.deleteRecord(%recordName);
	}

	if(!isFile(%fullPath = (%this.path @ %recordName @ ".info"))) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::loadRecord", "Invalid or nonexistent record given");
		return "ERROR";
	}

	%this.recordsList.push(%recordName);
	%this._record[%recordName] = Array();

	%file = new FileObject();
	%file.openForRead(%fullPath);

	while(!%file.isEOF()) {
		%line = %file.readLine();
		%this._record[%recordName, getField(%line, 0)] = getFields(%line, 1);
		%this._record[%recordName].push(getField(%line, 0));
	}

	%file.close();
	%file.delete();

	CMInfo(1, nonempty(%this.getName(), %this.getID()), "Loaded record for \"" @ %recordName @ "\"");
}

function CityModInfoDB::loadRecords(%this) {
	if(%this.recordsList.length > 0) {
		%this.deleteAllRecords();
	}

	%recordCount = 0;

	for(%i = findFirstFile(%this.path @ "*.info"); %i !$= ""; %i = findNextFile(%this.path @ "*.info")) {
		%this.loadRecord(fileBase(%i));
		%recordCount++;
	}

	CMInfo(1, nonempty(%this.getName(), %this.getID()), "Loaded (" @ %this.recordsList.length @ "/" @ %recordCount @ ") records");
}

function CityModInfoDB::deleteRecord(%this, %recordName) {
	if(!%this.recordExists(%recordName)) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::deleteRecord", "Can't delete nonexistent record");
		return "ERROR";
	}

	for(%i = 0; %i < %this._record[%recordName].length; %i++) {
		%this._record[%recordName, %this._record[%recordName].value[%i]] = "";
	}

	%this._record[%recordName].delete();
	%this._record[%recordName] = "";

	%this.recordsList.remove(%recordName);

	CMInfo(1, nonempty(%this.getName(), %this.getID()), "Deleted record for \"" @ %recordName @ "\"");
}

function CityModInfoDB::deleteAllRecords(%this) {
	if(%this.recordsList.length <= 0) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::deleteAllRecords", "No records to delete");
		return "ERROR";
	}

	%recordCount = %this.recordsList.length;

	for(%i = 0; %i < %this.recordsList.length; %i++) {
		%this.deleteRecord(%this.recordsList.value[%i]);
	}

	CMInfo(1, nonempty(%this.getName(), %this.getID()), "Deleted" SPC (%recordCount - %this.recordsList.length) SPC "records");
}