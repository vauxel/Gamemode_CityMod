// ============================================================
// Project          -      CityMod
// Description      -      Library Initialization
// ============================================================
// Sections
//   1: Misc Objects & Functions
//   2: Initialization
// ============================================================

// ============================================================
// Section 1 - Misc Objects & Functions
// ============================================================

function CMCore::execLibraries(%list) {
	for(%i = 0; %i < getFieldCount(%list); %i++) {
		exec("./" @ strLwr(getSubStr(getField(%list, %i), 0, 1) @ getSubStr(getField(%list, %i), 1, strLen(getField(%list, %i)))) @ ".cs");
	}
}

// ============================================================
// Section 2 - Initialization
// ============================================================

$CM::LibraryList = (
	"string" TAB
	"vector" TAB
	"global" TAB
	"array" TAB
	"map" TAB
	"stringify" TAB
	"brick" TAB
	"item" TAB
	"hitboxes"
);

CMCore::execLibraries($CM::LibraryList);
