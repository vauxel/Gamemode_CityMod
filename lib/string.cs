// ============================================================
// Project          -      CityMod
// Description      -      String Support Functions
// ============================================================

function isNumber(%string) {
	return (isInteger(%string) || isFloat(%string));
}

function isInteger(%string) {
	return (%string $= (%string | 0));
}

function isFloat(%string) {
	return (%string $= (%string + 0));
}

function isExplicitObject(%string) {
	if(%string $= "") {
		return false;
	}

	return ((getSubStr(%string, strLen(%string) - 1, 1) $= "\x01") && isObject(%string));
}

function properText(%text) {
	if(strLen(%text) == 1)
		return %text;

	for(%i = 0; %i < getFieldCount(%text); %i++) {
		%field = getField(%text, %i);

		for(%j = 0; %j < getWordCount(%field); %j++) {
			%word = getWord(%field, %j);
			%field = setWord(%field, %j, strUpr(getSubStr(%word, 0, 1)) @ strLwr(getSubStr(%word, 1, strLen(%word))));
		}

		%text = setField(%text, %i, %field);
	}

	return %text;
}

function searchString(%haystack, %needle) {
	%haystackLen = strLen(%haystack);
	%needleLen = strLen(%needle);

	%matches = false;
	for(%i = 0; %i < %haystackLen; %i++) {
		if(getSubStr(%haystack, %i, 1) !$= getSubStr(%needle, 0, 1)) {
			continue;
		}

		%matches = true;
		for(%j = 1; %j < %needleLen; %j++) {
			if(getSubStr(%needle, %j, 1) !$= getSubStr(%haystack, %i + %j, 1)) {
				%matches = false;
				%i += %j;
				break;
			}
		}

		if(%matches) {
			break;
		}
	}

	return %matches;
}

function getIndex(%haystack, %needle, %stringtype) {
	if(%stringtype $= "")
		%stringtype = "word";

	if(%stringtype $= "word") {
		for(%index = 0; %index < getWordCount(%haystack); %index++) {
			if(getWord(%haystack, %index) $= %needle) {
				return %index;
			}
		}
	} else if(%stringtype $= "field") {
		for(%index = 0; %index < getFieldCount(%haystack); %index++) {
			if(getField(%haystack, %index) $= %needle) {
				return %index;
			}
		}
	}

	return -1;
}

function removeWords(%string, %word) {
	while(!inWords(%string, %word)) {
		for(%a = 0; getWordCount(%string) >= %a; %a++) {
			%found = getWord(%string, %a);
			if(%found $= %word) {
				%string = removeWord(%string, %a);
			}
		}
	}

	return %string;
}

function removeFields(%string, %field) {
	while(!inFields(%string, %word)) {
		for(%a = 0; getFieldCount(%string) >= %a; %a++) {
			%found = getField(%string, %a);
			if(%found $= %word) {
				%string = removeField(%string, %a);
			}
		}
	}

	return %string;
}

function inWords(%a, %b) {
	for(%i = 0; %i < getWordCount(%a); %i++) {
		if(getWord(%a, %i) $= %b) {
			return true;
		}
	}

	return false;
}

function inFields(%a, %b) {
	for(%i = 0; %i < getFieldCount(%a); %i++) {
		if(getField(%a, %i) $= %b) {
			return true;
		}
	}

	return false;
}

function pad(%number, %length) {
	if(%length $= "") {
		%length = 2;
	}

	while(strLen(%number) < %length) {
		%number = "0" @ %number;
	}

	return %number;
}

function nonempty(%param1, %param2, %param3, %param4, %param5, %param6, %param7, %param8, %param9, %param10, %param11, %param12, %param13, %param14, %param15, %param16) {
	for(%i = 1; %i <= 16; %i++) {
		if((%param[%i] $= "") || (%param[%i] == 0)) {
			continue;
		}

		return %param[%i];
	}
}

function colorCodeToML(%string) {
	%string = strReplace(%string, "\c0", "<color:ff0000>"); // Red
	%string = strReplace(%string, "\c1", "<color:0000ff>"); // Blue
	%string = strReplace(%string, "\c2", "<color:00ff00>"); // Green
	%string = strReplace(%string, "\c3", "<color:ffff00>"); // Yellow
	%string = strReplace(%string, "\c4", "<color:00ffff>"); // Cyan
	%string = strReplace(%string, "\c5", "<color:ff00ff>"); // Purple
	%string = strReplace(%string, "\c6", "<color:ffffff>"); // White
	%string = strReplace(%string, "\c7", "<color:606060>"); // Grey
	%string = strReplace(%string, "\c8", "<color:000000>"); // Black
	return %string;
}

function suffixAmount(%amount, %floatLength) {
	if(%amount < 1000) {
		return %amount;
	}

	if(%floatLength $= "") {
		%floatLength = 2;
	}

	%exponent = mFloor(mLog(%amount) / mLog(1000));
	return mFloatLength(%amount / mPow(1000, %exponent), %floatLength) @ getSubStr("KMBT", %exponent - 1, 1);
}

function commaSeparateAmount(%amount) {
	if((%decimal = strStr(%amount, ".")) != -1) {
		%start = %decimal;
		%float = getSubStr(%amount, %decimal, strLen(%amount) - %decimal);
	} else {
		%start = strLen(%amount);
	}

	for(%i = %start - 1; %i >= 0; %i--) {
		%string = getSubStr(%amount, %i, 1) @ %string;

		if(((%start - %i) % 3 == 0) && (%i != 0)) {
			%string = "," @ %string;
		}
	}

	if(%float !$= "") {
		%string = %string @ %float;
	}

	return %string;
}

function ordinalNumber(%number) {
	%ending[0] = "st"; %ending[1] = "nd"; %ending[2] = "rd"; %ending[3] = "th";
	return %number @ (%number < 11 || %number > 13 ? %ending[getMin((%number - 1) % 10, 3)] : "th");
}