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

package CityMod_Production_Conveyor {
	function fxDTSBrick::onCMPropPlant(%brick) {
		parent::onCMPropPlant(%brick);

		if(%brick.getDataBlock().getName() $= "brickCMConveyorStraightData") {
			%brick.moveConveyorItems();
		} else if(%brick.getDataBlock().getName() $= "brickCMConveyorTurnData") {
			%brick.moveConveyorItems(true);
		}
	}

	function fxDTSBrick::moveConveyorItems(%brick, %turn) {
		%position = getWord(%brick.getWorldBoxCenter(), 0) SPC getWord(%brick.getWorldBoxCenter(), 1) SPC getWord(%brick.getWorldBox(), 5);
		%boxSize = "1.5 1.5 0.8";

		if(%turn) {
			switch(%brick.getAngleID()) {
				case 0: %objVel = "-1 -1 0";
				case 1: %objVel = "1 -1 0";
				case 2: %objVel = "-1 -1 0";
				case 3: %objVel = "1 -1 0";
			}
		} else {
			switch(%brick.getAngleID()) {
				case 0: %objVel = "0 -1 0";
				case 1: %objVel = "1 0 0";
				case 2: %objVel = "0 1 0";
				case 3: %objVel = "-1 0 0";
			}
		}

		initContainerBoxSearch(%position, %boxSize, $TypeMasks::ItemObjectType | $TypeMasks::PlayerObjectType);
		while(%searchObj = containerSearchNext()) {
			if(getWord(%searchObj.getPosition(), 2) < getWord(%brick.getWorldBox(), 5)) {
				continue;
			}

			if(%searchObj.getType() & $TypeMasks::ItemObjectType) {
				%searchObj.setVelocity(vectorAdd(vectorScale(%objVel, 2), %searchObj.getVelocity()));
			} else if(%searchObj.getType() & $TypeMasks::PlayerObjectType) {
				%searchObj.setVelocity(vectorAdd(vectorScale(%objVel, 6), %searchObj.getVelocity()));
			}
		}

		%brick.schedule(500, "moveConveyorItems", %turn);
	}
};

if(isPackage(CityMod_Production_Conveyor))
	deactivatePackage(CityMod_Production_Conveyor);
activatePackage(CityMod_Production_Conveyor);