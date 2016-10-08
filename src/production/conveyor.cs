// ============================================================
// Project          -      CityMod
// Description      -      Conveyor Bricks Code
// ============================================================
// Sections
//   1: Datablocks
//   2: Package
// ============================================================

// ============================================================
// Section 1 - Datablocks
// ============================================================

datablock fxDTSBrickData(brickCMConveyorStraightData) {
	brickFile = $CM::Config::Path::Mod @ "res/bricks/conveyors/conveyor_straight.blb";

	isCityModBrick = true;
	citymodBrick["Type"] = "PROP";
	citymodBrick["Restriction"] = "NONE";

	iconName = $CM::Config::Path::Mod @ "res/bricks/conveyors/conveyor_straight.png";
	uiName = "Conveyor - Straight";
};

datablock fxDTSBrickData(brickCMConveyorTurnData) {
	brickFile = $CM::Config::Path::Mod @ "res/bricks/conveyors/conveyor_turn.blb";

	isCityModBrick = true;
	citymodBrick["Type"] = "PROP";
	citymodBrick["Restriction"] = "NONE";

	iconName = $CM::Config::Path::Mod @ "res/bricks/conveyors/conveyor_turn.png";
	uiName = "Conveyor - Turn";
};

// ============================================================
// Section 2 - Package
// ============================================================

//package CityMod_Production_Conveyor {
//
//};
//
//if(isPackage(CityMod_Production_Conveyor))
//	deactivatePackage(CityMod_Production_Conveyor);
//activatePackage(CityMod_Production_Conveyor);