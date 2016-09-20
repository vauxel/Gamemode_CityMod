// ============================================================
// Project          -      CityMod
// Description      -      Data Serialization and Parsing
// ============================================================

function Stringify::serialize(%data, %type) {
	if(%type $= "") {
		%type = Stringify::interpretDataType(%data);
	}

	switch$(%type) {
		case "null": return "null";
		case "string": return "\"" @ expandEscape(expandEscape(%data)) @ "\"";
		case "number": return %data > 0 ? "+" @ %data : %data;
		case "bool": return %data ? "true" : "false";
		case "map":
			for(%i = 0; %i < %data.keys.length; %i++) {
				%map = %map @ Stringify::serialize(%data.keys.value[%i], "string") SPC "=" SPC Stringify::serialize(%data.get(%data.keys.value[%i]));

				if(%i != (%data.keys.length - 1)) {
					%map = %map @ ", ";
				}
			}

			return "map:{" @ %map @ "}";
		case "array":
			for(%i = 0; %i < %data.length; %i++) {
				%array = %array @ Stringify::serialize(%data.value[%i]);

				if(%i != (%data.length - 1)) {
					%array = %array @ ", ";
				}
			}

			return "array:{" @ %array @ "}";
		case "object":
			switch$(%data.getClassName()) {
				case "SimObject": %objectType = "SO";
				case "SimGroup": %objectType = "SG";
				case "SimSet": %objectType = "SS";
				case "ScriptObject": %objectType = "so";
				case "ScriptGroup": %objectType = "sg";
			}

			if(strLen(%data.class)) {
				%object = (%object !$= "" ? (%object @ ", ") : "") @ "class = " @ Stringify::serialize(%data.class, "string");
			}

			if(strLen(%data.getName())) {
				%object = (%object !$= "" ? (%object @ ", ") : "") @ "name = " @ Stringify::serialize(%data.getName(), "string");
			}

			if(%data.getTaggedField(0) $= "") {
				return "object[" @ %objectType @ "]:{" @ %object @ "}";
			}

			%fieldIndex = 0;
			while((%field = %data.getTaggedField(%fieldIndex)) !$= "") {
				%fields = %fields @ ((%fieldIndex != 0) ? ", " : "") @ Stringify::serialize(getField(%field, 0), "string") SPC "=" SPC Stringify::serialize(getFields(%field, 1));
				%fieldIndex++;
			}

			%object = (%object !$= "" ? (%object @ ", ") : "") @ "fields = " @ "{" @ %fields @ "}";

			return "object[" @ %objectType @ "]:{" @ %object @ "}";
		default:
			CMError(1, "Stringify::serialize", "Could not serialize type \"" @ %type @ "\" given");
			return "";
	}
}

function Stringify::interpretDataType(%value) {
	if(!strLen(%value)) {
		return "null";
	}

	if((%value $= true) || (%value $= false)) {
		return "bool";
	}

	if(isExplicitObject(%value)) {
		if(isMapObject(%value)) {
			return "map";
		}

		if(isArrayObject(%value)) {
			return "array";
		}

		return "object";
	}

	if(isNumber(%value)) {
		return "number";
	}

	return "string";
}

function Stringify::parse(%string, %index) {
	if(!strLen(%string)) {
		CMError(1, "Stringify::parse", "Invalid \"%string\" given -- cannot be blank");
		return "";
	}

	$Stringify::currentString = %string;
	$Stringify::currentStringIndex = isInteger(%index) ? %index : 0;

	%type = Stringify::interpretType();
	%parsed = Stringify::parseType(%type);

	return %parsed;
}

function Stringify::skipWhitespace() {
	%strLen = strLen($Stringify::currentString);
	for(%i = $Stringify::currentStringIndex; %i < %strLen; %i++) {
		if(strPos("\t\r\n ", getSubStr($Stringify::currentString, %i, 1)) == -1) {
			$Stringify::currentStringIndex = %i;
			return;
		}
	}
}

function Stringify::interpretType() {
	if(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 4) $= "null") {
		return "null";
	}

	if((getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 4) $= "true") ||
		(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 5) $= "false")) {
		return "bool";
	}

	if(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 6) $= "object") {
		$Stringify::currentStringIndex += 6;
		return "object";
	}

	if(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 6) $= "array:") {
		$Stringify::currentStringIndex += 6;
		return "array";
	}

	if(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 4) $= "map:") {
		$Stringify::currentStringIndex += 4;
		return "map";
	}

	%firstChar = getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 1);

	if(%firstChar $= "\"") {
		return "string";
	}

	if((%firstChar $= "+") || (%firstChar $= "-") || isNumber(%firstChar)) {
		return "number";
	}
}

function Stringify::parseType(%type) {
	%strLen = strLen($Stringify::currentString);
	switch$(%type) {
		case "null":
			$Stringify::currentStringIndex += 4;
			return "";
		case "bool":
			if(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 4) $= "true") {
				$Stringify::currentStringIndex += 4;
				return true;
			} else if(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 5) $= "false") {
				$Stringify::currentStringIndex += 5;
				return false;
			} else {
				CMError(1, "Stringify::parseType", "Parsing \"bool\", invalid bool given");
				return "";
			}
		case "string":
			$Stringify::currentStringIndex++;

			for(%i = $Stringify::currentStringIndex; %i < %strLen; %i++) {
				if((getSubStr($Stringify::currentString, %i, 1) $= "\"") && (getSubStr($Stringify::currentString, %i - 1, 1) !$= "\\")) {
					%endIndex = %i - 1; // Set the end pos to right before the '\"' character
					break;
				}
			}

			%length = (%endIndex - $Stringify::currentStringIndex) + 1;
			%string = collapseEscape(collapseEscape(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, %length)));
			$Stringify::currentStringIndex = (%endIndex + 2); // 1 to skip the '\"' character and another 1 to set the index to after the string

			return %string;
		case "number":
			%firstChar = getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 1);

			if((%firstChar $= "+") || (%firstChar $= "-")) {
				if(strPos("0123456789", getSubStr($Stringify::currentString, $Stringify::currentStringIndex + 1, 1)) == -1) {
					CMError(1, "Stringify::parseType", "Parsing \"number\", invalid number given");
					return "";
				}

				$Stringify::currentStringIndex++;

				if(%firstChar $= "-") {
					%isNegative = true;
				}
			}

			for(%i = $Stringify::currentStringIndex; %i < %strLen; %i++) {
				if(strPos("0123456789.", getSubStr($Stringify::currentString, %i, 1)) == -1) {
					%endIndex = (%i - 1);
					break;
				} else if(getSubStr($Stringify::currentString, %i, 1) $= ".") {
					if(strPos("0123456789", getSubStr($Stringify::currentString, %i + 1, 1)) == -1) {
						%endIndex = (%i - 1);
						break;
					}
				}

				if(%i == (%strLen - 1)) {
					%endIndex = %i;
					break;
				}
			}

			%length = (%endIndex - $Stringify::currentStringIndex) + 1;
			%number = (%isNegative == true ? "-" : "") @ getSubStr($Stringify::currentString, $Stringify::currentStringIndex, %length);
			$Stringify::currentStringIndex = (%endIndex + 1);

			return %number;
		case "map":
			if(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 1) !$= "{") {
				CMError(1, "Stringify::parseType", "Parsing \"map\", no object-start char ('{') found");
				return "";
			}

			$Stringify::currentStringIndex++;
			%map = Map();

			while(true) {
				Stringify::skipWhitespace();
				%firstChar = getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 1);

				if(%firstChar $= ",") {
					$Stringify::currentStringIndex++;
					continue;
				}

				if(%firstChar $= "}") {
					$Stringify::currentStringIndex++;
					break;
				}

				%name = Stringify::parseType("string");
				Stringify::skipWhitespace();

				if(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 1) !$= "=") {
					CMError(1, "Stringify::parseType", "Parsing \"map\", no set delimeter ('=') found");
					%map.delete();
					return "";
				}

				$Stringify::currentStringIndex++;
				Stringify::skipWhitespace();

				%map.set(%name, Stringify::parseType(Stringify::interpretType()));
			}

			return %map;
		case "array":
			if(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 1) !$= "{") {
				CMError(1, "Stringify::parseType", "Parsing \"array\", no object-start char ('{') found");
				return "";
			}

			$Stringify::currentStringIndex++;
			%array = Array();

			while(true) {
				Stringify::skipWhitespace();
				%firstChar = getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 1);

				if(%firstChar $= ",") {
					$Stringify::currentStringIndex++;
					continue;
				}

				if(%firstChar $= "}") {
					$Stringify::currentStringIndex++;
					break;
				}

				%array.push(Stringify::parseType(Stringify::interpretType()));
			}

			return %array;
		case "object":
			if(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 1) !$= "[") {
				CMError(1, "Stringify::parseType", "Parsing \"object\", no object-type char ('[') found -- defaulting to SimObject");
				%objectType = "SimObject";
			} else {
				$Stringify::currentStringIndex++;
				%objectType = getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 2);

				if(strCmp(%objectType, "SO") == 0) {
					%objectType = "SimObject";
				} else if(strCmp(%objectType, "SG") == 0) {
					%objectType = "SimGroup";
				} else if(strCmp(%objectType, "SS") == 0) {
					%objectType = "SimSet";
				} else if(strCmp(%objectType, "so") == 0) {
					%objectType = "ScriptObject";
				} else if(strCmp(%objectType, "sg") == 0) {
					%objectType = "ScriptGroup";
				} else {
					CMError(1, "Stringify::parseType", "Parsing \"object\", object-type \"" @ %objectType @ "\" not recognized -- defaulting to SimObject");
					%objectType = "SimObject";
				}

				$Stringify::currentStringIndex += 2;
				$Stringify::currentStringIndex++;
			}

			$Stringify::currentStringIndex++;

			if(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 1) !$= "{") {
				CMError(1, "Stringify::parseType", "Parsing \"" @ %type @ "\", no object-start char ('{') found");
				return "";
			}

			$Stringify::currentStringIndex++;

			%objectName = "";
			%objectClass = "";
			%objectFields = "";

			while(true) {
				Stringify::skipWhitespace();
				%firstChar = getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 1);

				if(%firstChar $= ",") {
					$Stringify::currentStringIndex++;
					continue;
				}

				if(%firstChar $= "}") {
					$Stringify::currentStringIndex++;
					break;
				}

				if(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 4) $= "name") {
					$Stringify::currentStringIndex += 4;
					%parameter = 1;
				}

				if(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 5) $= "class") {
					$Stringify::currentStringIndex += 5;
					%parameter = 2;
				}

				if(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 6) $= "fields") {
					$Stringify::currentStringIndex += 6;
					%parameter = 3;
				}

				if(%parameter $= "") {
					CMError(1, "Stringify::parseType", "Parsing \"" @ %type @ "\", no matching parameter found");
					return "";
				}

				Stringify::skipWhitespace();

				if(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 1) !$= "=") {
					CMError(1, "Stringify::parseType", "Parsing \"" @ %type @ "\", no set delimeter ('=') found");
					return "";
				}

				$Stringify::currentStringIndex++;
				Stringify::skipWhitespace();

				switch(%parameter) {
					case 1: %objectName = Stringify::parseType("string");
					case 2: %objectClass = Stringify::parseType("string");
					case 3: %objectFields = Stringify::parseType("map");
				}
			}

			if(isObject(%objectName)) {
				CMError(1, "Stringify::parseType", "Parsing \"" @ %type @ "\", ScriptObject by the name of \"" @ %objectName @ "\" already exists");
				return "";
			}

			%tempSO = new (%objectType)(%objectName);

			if(%objectClass !$= "") {
				%tempSO.class = %objectClass;
			}

			if((%objectFields !$= "") && isExplicitObject(%objectFields)) {
				for(%i = 0; %i < %objectFields.keys.length; %i++) {
					%tempSO.setAttribute(%objectFields.keys.value[%i], %objectFields.get(%objectFields.keys.value[%i]));
				}

				// This is an example of what NOT to do...
				%objectFields.keys.length = 0;
				%objectFields.delete();
			}

			%object = %tempSO.copy();
			%tempSO.delete();

			return %object;
		default:
			CMError(1, "Stringify::parseType", "Could not parse type \"" @ %type @ "\" given");
			return "";
	}
}