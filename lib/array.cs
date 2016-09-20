// ============================================================
// Project          -      CityMod
// Description      -      Array Object Helper
// ============================================================

function Array() {
	if(!isObject(ArrayObjects)) {
		new ScriptGroup(ArrayObjects);
	}

	%array = new ScriptObject() { class = "ArrayObject"; } @ "\x01";
	ArrayObjects.add(%array);

	return %array;
}

function isArrayObject(%object) {
	return isExplicitObject(%object) && (%object.class $= "ArrayObject");
}

function ArrayObject::onAdd(%this) {
	%this.length = 0;
}

function ArrayObject::debug(%this) {
	echo("Array" SPC %this.getID() SPC "dump:");
	echo(" - Array length:" SPC %this.length);

	for(%i = 0; %i < %this.length; %i++) {
		echo(" - [" @ %i @ "] = \"" @ %this.at(%i) @ "\"");
	}
}

function ArrayObject::onRemove(%this) {
	for(%i = 0; %i < %this.length; %i++) {
		if(isExplicitObject(%object = %this.at(%i))) {
			%object.schedule(0, "delete");
		}
	}
}

function ArrayObject::_set(%this, %index, %value) {
	%this.value[%index] = %value;
}

function ArrayObject::_get(%this, %index) {
	return %this.value[%index];
}

function ArrayObject::at(%this, %index) {
	if(%index < 0 || %index > (%this.length - 1)) {
		return;
	}

	return %this._get(%index);
}

function ArrayObject::push(%this, %value) {
	%this._set(%this.length, %value);
	%this.length++;

	return %this;
}

function ArrayObject::pop(%this, %index) {
	if(!strLen(%index)) {
		%index = (%this.length - 1);
	}

	if(%index < 0 || %index > (%this.length - 1)) {
		return;
	}

	%this.length--;

	for(%i = %index; %i < %this.length; %i++) {
		%this._set(%i, %this._get(%i + 1));
	}

	%this._set(%this.length, "");

	return %this;
}

function ArrayObject::concat(%this, %array) {
	if(!strLen(%array) || !isObject(%array)) {
		return;
	}

	for(%i = 0; %i < %this.length; %i++) {
		%this.push(%array.at(%i));
	}

	return %this;
}

function ArrayObject::count(%this, %value) {
	%count = 0;
	for(%i = 0; %i < %this.length; %i++) {
		if(%this._get(%i) $= %value) {
			%count++;
		}
	}

	return %count;
}

function ArrayObject::find(%this, %value, %filter) {
	if((%filter !$= "") && ((%delimiterPos = strPos(%filter, "_")) == -1)) {
		return -1;
	}

	if(%filter $= "") {
		%method = 0;
	} else {
		switch$(getSubStr(%filter, 0, %delimiterPos)) {
			case "field": %method = 1;
			case "word": %method = 2;
			case "start": %method = 3;
			default: %method = 0;
		}

		if(%method != 0) {
			%methodIndex = getSubStr(%filter, %delimiterPos + 1, strLen(%filter) - (%delimiterPos + 1));
		}
	}

	for(%i = 0; %i < %this.length; %i++) {
		%element = %this._get(%i);
		%elementLen = strLen(%element);

		switch(%method) {
			case 0:
				if(%element $= %value) {
					return %i;
				}
			case 1:
				if(getField(%element, %methodIndex) $= %value) {
					return %i;
				}
			case 2:
				if(getWord(%element, %methodIndex) $= %value) {
					return %i;
				}
			case 3:
				%methodIndex = (%methodIndex > %elementLen ? %elementLen : %methodIndex);
				if(getSubStr(%element, 0, %methodIndex) $= %value) {
					return %i;
				}
			case 4:
				%methodIndex = (%elementLen - %methodIndex < 0 ? %elementLen : %methodIndex);
				if(getSubStr(%element, %elementLen - %methodIndex, %methodIndex) $= %value) {
					return %i;
				}
		}
	}

	return -1;
}

function ArrayObject::contains(%this, %value, %filter) {
	return (%this.find(%value, %filter) != -1);
}

function ArrayObject::remove(%this, %value) {
	%index = %this.find(%value);

	if(%index != -1) {
		%this.pop(%index);
	}

	return %this;
}

function ArrayObject::clear(%this) {
	for(%i = 0; %i < %this.length; %i++) {
		%this._set(%i, "");
	}

	%this.length = 0;
	return %this;
}

function ArrayObject::copy(%this) {
	%array = Array();

	for(%i = 0; %i < %this.length; %i++) {
		%array.push(%this._get(%i));
	}

	return %array;
}