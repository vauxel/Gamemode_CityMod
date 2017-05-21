// ============================================================
// Project          -      CityMod
// Description      -      Data Serialization and Parsing
// ============================================================

function Stringify::getIndentString(%indent) {
	%string = "";

	for(%i = 0; %i < %indent; %i++) {
		%string = %string @ "\t";
	}

	return %string;
}

function Stringify::serialize(%data, %compact, %type, %indent) {
	if(%compact $= "") {
		%compact = true;
	}

	if(%type $= "") {
		%type = Stringify::interpretDataType(%data);
	}

	if(!%compact) {
		if(%indent $= "") {
			%indent = 0;
		}

		%indentString = Stringify::getIndentString(%indent);
	}

	switch$(%type) {
		case "null": return "null";
		case "string": return "\"" @ expandEscape(expandEscape(%data)) @ "\"";
		case "number": return %data > 0 ? "+" @ %data : %data;
		case "bool": return %data ? "true" : "false";
		case "array":
			if(%data.length == 0) {
				return "[ ]";
			}

			for(%i = 0; %i < %data.length; %i++) {
				%array = %array @ (%compact ? "" : %indentString @ "\t") @ Stringify::serialize(%data.value[%i], %compact, "", %indent + 1);

				if(%i != (%data.length - 1)) {
					%array = %array @ (%compact ? ", " : ",\n");
				}
			}

			return "[" @ (%compact ? "" : "\n") @ %array @ (%compact ? "" : "\n" @ %indentString) @ "]";
		case "map":
			if(%data.keys.length == 0) {
				return "{ }";
			}

			for(%i = 0; %i < %data.keys.length; %i++) {
				%map = %map @ (%compact ? "" : %indentString @ "\t") @ Stringify::serialize(%data.keys.value[%i], true, "string") SPC "=" SPC Stringify::serialize(%data.get(%data.keys.value[%i]), %compact, "", %indent + 1);

				if(%i != (%data.keys.length - 1)) {
					%map = %map @ (%compact ? ", " : ",\n");
				}
			}

			return "{" @ (%compact ? "" : "\n") @ %map @ (%compact ? "" : "\n" @ %indentString) @ "}";
		case "object":
			switch$(%data.getClassName()) {
				case "SimObject": %objectType = "SO";
				case "SimGroup": %objectType = "SG";
				case "SimSet": %objectType = "SS";
				case "ScriptObject": %objectType = "so";
				case "ScriptGroup": %objectType = "sg";
			}

			%tags = "<!" @ %objectType @ ">";

			if(strLen(%data.class)) {
				%tags = %tags @ "<#" @ %data.class @ ">";
			}

			if(strLen(%data.getName())) {
				%tags = %tags @ "<@" @ %data.getName() @ ">";
			}

			if(%data.getTaggedField(0) $= "") {
				return "(" @ %tags @ ")";
			}

			%fieldIndex = 0;
			while((%field = %data.getTaggedField(%fieldIndex)) !$= "") {
				if(%fieldIndex != 0) {
					%object = %object @ (%compact ? ", " : ",\n");
				}

				%object = %object @ (%compact ? "" : %indentString @ "\t") @ Stringify::serialize(getField(%field, 0), true, "string") SPC "=" SPC Stringify::serialize(getFields(%field, 1), %compact, "", %indent + 1);
				%fieldIndex++;
			}

			return "(" @ %tags @ (%compact ? " " : "\n") @ %object @ (%compact ? "" : "\n" @ %indentString) @ ")";
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

	if(isNumber(%value)) {
		return "number";
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

	if((getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 4) $= "true") || (getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 5) $= "false")) {
		return "bool";
	}

	%firstChar = getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 1);

	if(%firstChar $= "\"") {
		return "string";
	}

	if((%firstChar $= "+") || (%firstChar $= "-") || isNumber(%firstChar)) {
		return "number";
	}

	if(%firstChar $= "[") {
		return "array";
	}

	if(%firstChar $= "{") {
		return "map";
	}

	if(%firstChar $= "(") {
		return "object";
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
					%endIndex = %i - 1;
					break;
				}
			}

			%length = (%endIndex - $Stringify::currentStringIndex) + 1;
			%string = collapseEscape(collapseEscape(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, %length)));
			$Stringify::currentStringIndex = %endIndex + 2;

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
			$Stringify::currentStringIndex = %endIndex + 1;

			return %number;
		case "array":
			if(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 1) !$= "[") {
				CMError(1, "Stringify::parseType", "Parsing \"array\", no object-start char ('[') found");
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

				if(%firstChar $= "]") {
					$Stringify::currentStringIndex++;
					break;
				}

				%array.push(Stringify::parseType(Stringify::interpretType()));
			}

			return %array;
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
		case "object":
			if(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 1) !$= "(") {
				CMError(1, "Stringify::parseType", "Parsing \"object\", no object-start char ('(') found");
				return "";
			}

			$Stringify::currentStringIndex++;

			%objectType = "";
			%objectClass = "";
			%objectName = "";

			while(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 1) $= "<") {
				$Stringify::currentStringIndex++;

				%tagChar = getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 1);

				if(%tagChar $= "!") {
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
				} else if((%tagChar $= "#") || (%tagChar $= "@")) {
					$Stringify::currentStringIndex++;

					for(%i = $Stringify::currentStringIndex; %i < %strLen; %i++) {
						if(getSubStr($Stringify::currentString, %i, 1) $= ">") {
							%endIndex = %i - 1;
							break;
						}
					}

					if(%tagChar $= "#") {
						%objectClass = getSubStr($Stringify::currentString, $Stringify::currentStringIndex, (%endIndex - $Stringify::currentStringIndex) + 1);
					} else if(%tagChar $= "@") {
						%objectName = getSubStr($Stringify::currentString, $Stringify::currentStringIndex, (%endIndex - $Stringify::currentStringIndex) + 1);
					}

					$Stringify::currentStringIndex = %endIndex + 1;
				}

				$Stringify::currentStringIndex++;
			}

			if((%objectName !$= "") && isObject(%objectName)) {
				CMError(1, "Stringify::parseType", "Parsing \"object\", " @ %objectType @ " by the name of \"" @ %objectName @ "\" already exists");
				return "";
			}

			%tempSO = new (%objectType)(%objectName);

			if(%objectClass !$= "") {
				%tempSO.class = %objectClass;
			}

			while(true) {
				Stringify::skipWhitespace();
				%firstChar = getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 1);

				if(%firstChar $= ",") {
					$Stringify::currentStringIndex++;
					continue;
				}

				if(%firstChar $= ")") {
					$Stringify::currentStringIndex++;
					break;
				}

				%name = Stringify::parseType("string");
				Stringify::skipWhitespace();

				if(getSubStr($Stringify::currentString, $Stringify::currentStringIndex, 1) !$= "=") {
					CMError(1, "Stringify::parseType", "Parsing \"object\", no set delimeter ('=') found");
					%tempSO.delete();
					return "";
				}

				$Stringify::currentStringIndex++;
				Stringify::skipWhitespace();

				%tempSO.setAttribute(%name, Stringify::parseType(Stringify::interpretType()));
			}

			%object = %tempSO.copy();
			%tempSO.delete();

			return %object;
		default:
			CMError(1, "Stringify::parseType", "Could not parse type \"" @ %type @ "\" given");
			return "";
	}
}