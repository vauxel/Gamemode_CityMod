// ============================================================
// Project          -      CityMod
// Description      -      Property Bricks Code
// ============================================================
// Sections
//   1: Datablocks
//   2: Package
// ============================================================

// ============================================================
// Section 1 - Datablocks
// ============================================================

datablock TriggerData(CMPropertyTrigger) {
	tickPeriodMS = 500;
};

datablock fxDTSBrickData(brickCM16x16PropertyData) {
	brickFile = $CM::Config::Path::Mod @ "res/bricks/properties/16x16Property.blb";

	isCityModBrick = true;
	citymodBrick["Type"] = "PROPERTY";
	citymodBrick["Restriction"] = "ADMIN";

	iconName = $CM::Config::Path::Mod @ "res/bricks/properties/16x16Property.png";
	uiName = "16x Property";
};

datablock fxDTSBrickData(brickCM32x32PropertyData) {
	brickFile = $CM::Config::Path::Mod @ "res/bricks/properties/32x32Property.blb";

	isCityModBrick = true;
	citymodBrick["Type"] = "PROPERTY";
	citymodBrick["Restriction"] = "ADMIN";

	iconName = $CM::Config::Path::Mod @ "res/bricks/properties/32x32Property.png";
	uiName = "32x Property";
};

// ============================================================
// Section 2 - Package
// ============================================================

package CityMod_Bricks_Property {
	function fxDTSBrick::onCMPropertyPlant(%brick) {
		parent::onCMPropertyPlant(%brick);

		if(strLen(%brick.propertyID) && CM_RealEstate.dataExists(%brick.propertyID)) {
			CM_RealEstate.getData(%brick.propertyID).linkPropertyBrick(%brick.getID());
		}
	}

	function fxDTSBrick::getProperty(%brick) {
		%property = %brick;
		while(isObject(%property) && (%property.getDataBlock().citymodBrick["Type"] !$= "PROPERTY")) {
			%property = %property.getDownBrick(0);
		}

		if(!isObject(%property)) {
			return -1;
		}

		return %property;
	}
};

if(isPackage(CityMod_Bricks_Property))
	deactivatePackage(CityMod_Bricks_Property);
activatePackage(CityMod_Bricks_Property);