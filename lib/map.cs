// ============================================================
// Project          -      CityMod
// Description      -      Map Object Helper
// ============================================================

function Map() {
	if(!isObject(MapObjects)) {
		new ScriptGroup(MapObjects);
	}

	%map = new ScriptObject() { class = "MapObject"; } @ "\x01";
	MapObjects.add(%map);

	return %map;
}

function isMapObject(%object) {
	return isExplicitObject(%object) && (%object.class $= "MapObject");
}

function MapObject::onAdd(%this) {
	%this.keys = Array();
}

function MapObject::onRemove(%this) {
	for(%i = 0; %i < %this.keys.length; %i++) {
		if(isExplicitObject(%object = %this.get(%this.keys.at(%i)))) {
			%object.schedule(0, "delete");
		}
	}

	%this.keys.delete();
}

function MapObject::debug(%this) {
	echo("Map" SPC %this.getID() SPC "dump:");
	echo(" - Map keys count:" SPC %this.keys.length);

	for(%i = 0; %i < %this.keys.length; %i++) {
		%key = %this.keys.at(%i);
		echo(" - [\"" @ %key @ "\"] = \"" @ %this.get(%key) @ "\"");
	}
}

function MapObject::set(%this, %key, %value) {
	if(!%this.keys.contains(%key)) {
		%this.keys.push(%key);
	}

	%this._set(%key, %value);
	return %this;
}

function MapObject::_set(%this, %key, %value) {
	%this.value[%key] = %value;
}

function MapObject::get(%this, %key) {
	return %this.value[%key];
}

function MapObject::remove(%this, %key) {
	%index = %this.keys.find(%key);

	if(%index == -1) {
		return;
	}

	%value = %this.get(%key);
	%this.keys.pop(%index);
	%this._set(%key, "");

	return %value;
}

function MapObject::exists(%this, %key) {
	return %this.keys.contains(%key);
}

function MapObject::keys(%this) {
	return %this.keys.copy();
}

function MapObject::values(%this) {
	%array = Array();

	for(%i = 0; %i < %this.keys.length; %i++) {
		%array.push(%this.get(%this.keys.at(%i)));
	}

	return %array;
}

function MapObject::copy(%this) {
	%map = Map();

	for(%i = 0; %i < %this.keys.length; %i++) {
		%key = %this.keys.value[%i];
		%map.set(%key, %this.get(%key));
	}

	return %map;
}