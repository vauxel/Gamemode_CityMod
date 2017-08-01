// ============================================================
// Project          -      CityMod
// Description      -      Database Code
// ============================================================

function CityModDatabase::onAdd(%this) {
	if(!strLen(%this.path)) {
		CMError(0, "CityModDatabase::onAdd", "No file path given");
		%this.schedule(0, "delete");
		return "ERROR";
	}

	if((getSubStr(%this.path, strLen(%this.path) - 1, 1) !$= "/") && (getSubStr(%this.path, strLen(%this.path) - 1, 1) !$= "\\")) {
		%this.path = %this.path @ "/";
	}

	if(!strLen(%this.dataClass)) {
		%this.dataClass = "CityModDatabaseData";
	}

	%this.fieldTable = Map();
	%this.dataTable = Map();
}

function CityModDatabase::onRemove(%this) {
	if(%this.dataTable.keys.length != 0) {
		%this.saveAllData();
	}

	%this.fieldTable.delete();
	%this.dataTable.delete();
}

function CityModDatabase::getDataPath(%this, %id) {
	return (%this.path @ %id @ ".data");
}

function CityModDatabase::dataExists(%this, %id) {
	return (isFile(%this.getDataPath(%id)) || %this.isDataLoaded(%id));
}

function CityModDatabase::isDataLoaded(%this, %id) {
	return isExplicitObject(%this.dataTable.get(%id));
}

function CityModDatabase::createDataSO(%this, %id) {
	return (new ScriptObject() {
		class = %this.dataClass;
		dataID = strLen(%id) ? %id : "";
	} @ "\x01");
}

function CityModDatabase::addField(%this, %name, %defaultValue) {
	if(!strLen(%name)) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::addField", "No field name given");
		return "ERROR";
	}

	if(strPos("0123456789", getSubStr(%name, 0, 1)) != -1) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::addField", "Name cannot start with a number");
		return "ERROR";
	}

	if((%name $= "class") || (%name $= "datablock") || (%name $= "id")) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::addField", "Name specified is an illegal field name");
		return "ERROR";
	}

	%name = strReplace(stripMLControlChars(%name), " ", "_");

	if(!strLen(%defaultValue)) {
		%defaultValue = "0";
	}

	%this.fieldTable.set(strLwr(%name), %defaultValue);

	if(%this.dataTable.keys.length != 0) {
		for(%i = 0; %i < %this.dataTable.keys.length; %i++) {
			%this.syncData(%this.dataTable.keys.value[%i]);
		}
	}

	CMInfo(1, nonempty(%this.getName(), %this.getID()), "Added field," SPC %name @ ", with default value," SPC %defaultValue);
}

function CityModDatabase::removeField(%this, %name) {
	if(!strLen(%name)) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::removeField", "No field name given");
		return "ERROR";
	}

	%name = strReplace(stripMLControlChars(%name), " ", "_");

	if(!%this.fieldTable.exists(%name)) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::removeField", "Field \"" @ %name @ "\" does not exist");
		return "ERROR";
	}

	%this.fieldTable.remove(%name);

	if(%this.dataTable.keys.length != 0) {
		for(%i = 0; %i < %this.dataTable.keys.length; %i++) {
			%this.syncData(%this.dataTable.keys.value[%i], true);
		}
	}

	CMInfo(1, nonempty(%this.getName(), %this.getID()), "Removed field," SPC %name);
}

function CityModDatabase::getData(%this, %id) {
	if(!%this.dataExists(%id)) {
		return "";
	}

	if(%this.dataExists(%id) && !%this.isDataLoaded(%id)) {
		%this.loadData(%id);
	}

	return %this.dataTable.get(%id);
}

function CityModDatabase::addData(%this, %id) {
	if(%this.dataExists(%id)) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::addData", "Data for ID" SPC %id SPC "already exists");
		return "ERROR";
	}

	if(strLen(%id)) {
		if(!isInteger(%id)) {
			CMError(1, nonempty(%this.getName(), %this.getID()) @ "::addData", "ID must be an integer");
			return "ERROR";
		}
	} else {
		%id = 1;
		while(isFile(%this.getDataPath(%id))) {
			%id++;
		}
	}

	%data = %this.createDataSO(%id);

	for(%i = 0; %i < %this.fieldTable.keys.length; %i++) {
		%name = %this.fieldTable.keys.value[%i];
		%value = %this.fieldTable.get(%name);
		if(getSubStr(%value, 0, 1) $= ">") {
			%data.setAttribute(%name, eval(getSubStr(%value, 1, strLen(%value) - 1) @ ";"));
		} else if(isExplicitObject(%value)) {
			%data.setAttribute(%name, %value.copy());
		} else {
			%data.setAttribute(%name, %value);
		}
	}

	%this.dataTable.set(%id, %data);
	CMInfo(1, nonempty(%this.getName(), %this.getID()), "Added data for ID," SPC %id);

	%this.saveData(%id);
	return %data;
}

function CityModDatabase::syncData(%this, %id, %dataTrim) {
	if(!strLen(%id)) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::syncData", "ID not given");
		return "ERROR";
	}

	if(!%this.isDataLoaded(%id)) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::syncData", "Data for ID" SPC %id SPC "has not been loaded or does not exist");
		return "ERROR";
	}

	// Check if "fieldTable" contains fields that "Data" does not have -- if so, add the field to "Data"
	for(%i = 0; %i < %this.fieldTable.keys.length; %i++) {
		%key = %this.fieldTable.keys.value[%i];
		if(%this.dataTable.get(%id).getAttribute(%key) $= "") {
			%this.dataTable.get(%id).setAttribute(%key, %this.fieldTable.get(%key));
		}
	}

	// Optional: Check if "Data" contains fields that "fieldTable" no longer has -- if so, delete the field from "Data"
	if(%dataTrim == true) {
		%data = %this.dataTable.get(%id); %index = 0;
		while((%field = %data.getTaggedField(%index)) !$= "") {
			%name = getField(%field, 0);

			if(%name $= "dataID") {
				%index++;
				continue;
			}

			if(!%this.fieldTable.keys.contains(%name)) {
				%deleteList = setField(%deleteList, getFieldCount(%deleteList), %name);
			}

			%index++;
		}

		for(%i = 0; %i < getFieldCount(%deleteList); %i++) {
			%data.deleteAttribute(getField(%deleteList, %i));
		}
	}
}

function CityModDatabase::loadData(%this, %id) {
	if(!strLen(%id)) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::loadData", "ID not given");
		return "ERROR";
	}

	if(!%this.dataExists(%id)) {
		%this.addData(%id);
		return;
	}

	if(%this.isDataLoaded(%id)) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::loadData", "Data for ID" SPC %id SPC "has already been loaded");
		return "ERROR";
	}

	%file = new FileObject();

	if(!%file.openForRead(%this.getDataPath(%id))) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::loadData", "File \"" @ %this.getDataPath(%id) @ "\" does not exist");
		%file.delete();
		return "ERROR";
	}

	%data = %this.createDataSO(%id);

	%currentField["name"] = "";
	%currentField["data"] = "";

	while(!%file.isEOF()) {
		%line = %file.readLine();

		if(getSubStr(%line, 0, 1) $= "#") {
			if(%currentField["name"] !$= "") {
				%data.setAttribute(%currentField["name"], Stringify::parse(%currentField["data"]));
			}

			%currentField["name"] = getField(getSubStr(%line, 1, strLen(%line) - 1), 0);
			%currentField["data"] = getFields(%line, 1);
		} else {
			%currentField["data"] = %currentField["data"] @ %line;
		}

		if(%file.isEOF()) {
			%data.setAttribute(%currentField["name"], Stringify::parse(%currentField["data"]));
		}
	}

	%file.close();
	%file.delete();

	%this.dataTable.set(%id, %data);
	CMInfo(1, nonempty(%this.getName(), %this.getID()), "Loaded data for ID," SPC %id);

	%this.syncData(%id, true);
	return %data;
}

function CityModDatabase::loadAllData(%this) {
	for(%i = findFirstFile(%this.path @ "*.data"); isFile(%i); %i = findNextFile(%this.path @ "*.data")) {
		%id = strReplace(fileName(%i), ".data", "");
		if(!%this.isDataLoaded(%id)) {
			%this.loadData(%id);
		}
	}
}

function CityModDatabase::unLoadData(%this, %id) {
	if(!strLen(%id)) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::unLoadData", "ID not given");
		return "ERROR";
	}

	if(!%this.isDataLoaded(%id)) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::unLoadData", "Data for ID" SPC %id SPC "is not currently loaded");
		return "ERROR";
	}

	%this.saveData(%id);
	%this.dataTable.get(%id).delete();
	%this.dataTable.remove(%id);

	CMInfo(1, nonempty(%this.getName(), %this.getID()) @ " ==> Unloaded data for ID," SPC %id);
}

function CityModDatabase::saveData(%this, %id) {
	if(!strLen(%id)) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::saveData", "ID not given");
		return "ERROR";
	}

	if(!%this.isDataLoaded(%id)) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::saveData", "Data for ID" SPC %id SPC "has not been loaded or does not exist");
		return "ERROR";
	}

	%file = new FileObject();

	if(!%file.openForWrite(%this.getDataPath(%id))) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::saveData", "File \"" @ %this.getDataPath(%id) @ "\" could not be saved to");
		%file.delete();
		return "ERROR";
	}

	%data = %this.dataTable.get(%id); %index = 0;
	while((%field = %data.getTaggedField(%index)) !$= "") {
		%name = getField(%field, 0);

		if(%name $= "dataID") {
			%index++;
			continue;
		}

		%file.writeLine("#" @ %name TAB Stringify::serialize(getFields(%field, 1), false));
		%index++;
	}

	%file.close();
	%file.delete();

	CMInfo(1, nonempty(%this.getName(), %this.getID()), "Saved data for ID," SPC %id);
}

function CityModDatabase::saveAllData(%this) {
	if(%this.dataTable.keys.length == 0) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::saveAllData", "There are no active users in the database to save");
		return "ERROR";
	}

	for(%i = 0; %i < %this.dataTable.keys.length; %i++) {
		%this.saveData(%this.dataTable.keys.value[%i]);
	}
}

function CityModDatabase::resetData(%this, %id) {
	if(!strLen(%id)) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::resetData", "ID not given");
		return "ERROR";
	}

	if(!%this.dataExists(%id)) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::resetData", "Data for ID" SPC %id SPC "does not exist");
		return "ERROR";
	}

	if(!%this.isDataLoaded(%id)) {
		%this.loadData(%id);
		%unLoad = true;
	}

	%this.deleteData(%id);
	%this.addData(%id);

	if(%unLoad) {
		%this.unLoadData(%id);
	}
}

function CityModDatabase::deleteData(%this, %id) {
	if(!strLen(%id)) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::deleteData", "ID not given");
		return "ERROR";
	}

	if(!%this.dataExists(%id)) {
		CMError(1, nonempty(%this.getName(), %this.getID()) @ "::deleteData", "Data for ID" SPC %id SPC "does not exist");
		return "ERROR";
	}

	if(isFile(%this.getDataPath(%id))) {
		fileDelete(%this.getDataPath(%id));
	}

	if(%this.isDataLoaded(%id)) {
		%this.dataTable.get(%id).delete();
		%this.dataTable.remove(%id);
	}

	CMInfo(1, nonempty(%this.getName(), %this.getID()), "Deleted data for ID," SPC %id);
}