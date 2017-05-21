// ============================================================
// Project          -      CityMod
// Description      -      Source Initialization
// ============================================================
// Sections
//   1: Misc Objects & Functions
//   2: Initialization
// ============================================================

function CMError(%level, %function, %message) {
	if(%level > $CM::MessageLevel) {
		return;
	}

	warn("[CM Server]" SPC %function @ "() ==>" SPC %message);
	backtrace();
}

function CMInfo(%level, %topic, %message) {
	if(%level > $CM::MessageLevel) {
		return;
	}

	echo("[CM Server]" SPC %topic SPC "::" SPC %message);
}

function servercmdDevMode(%client) {
	%client.inCMDevMode = !%client.inCMDevMode;
	talk("Turned" SPC (%client.inCMDevMode ? "ON" : "OFF") SPC "CityMod Dev Mode for" SPC %client.name);
}

// ============================================================
// Section 1 - Misc Objects & Functions
// ============================================================

if(!isObject(CityModModules)) {
	new ScriptGroup(CityModModules){};
}

function CMCore::execModules(%list) {
	if(!isObject(CityModModules)) {
		new ScriptGroup(CityModModules){};
		MissionCleanup.add(CityModModules);
	}

	for(%i = 0; %i < getFieldCount(%list); %i++) {
		%module = strUpr(getSubStr(getField(%list, %i), 0, 1)) @ strLwr(getSubStr(getField(%list, %i), 1, strLen(getField(%list, %i))));

		if(!isObject("CityMod" @ %module @ "Module")) {
			new ScriptGroup("CityMod" @ %module @ "Module"){};
			eval("CityModModules.add(" @ ("CityMod" @ %module @ "Module") @ ");");
		}

		CMInfo(1, "CMCore", "Loading \"" @ %module @ "\" Source Module");
		exec("./" @ strLwr(%module) @ "/init.cs");
	}
}

// ============================================================
// Section 2 - Initialization
// ============================================================

$CM::ModuleList = (
	"database" TAB
	"auth" TAB
	"tick" TAB
	"bricks" TAB
	"items" TAB
	"inventory" TAB
	"economy" TAB
	"production" TAB
	"organizations" TAB
	"realestate" TAB
	"player"
);

CMCore::execModules($CM::ModuleList);
