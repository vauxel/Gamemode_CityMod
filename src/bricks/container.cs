// ============================================================
// Project          -      CityMod
// Description      -      Container Bricks Code
// ============================================================
// Sections
//   1: Datablocks
//   2: Package
// ============================================================

// ============================================================
// Section 1 - Datablocks
// ============================================================

datablock fxDTSBrickData(brickCMTestContainerData) {
	brickFile = $CM::Config::Path::Mod @ "res/bricks/Container/TestContainer.blb";

	isCityModBrick = true;
	citymodBrick["Type"] = "PROP";
	citymodBrick["Restriction"] = "NONE";

	iconName = $CM::Config::Path::Mod @ "";
	uiName = "Test Container";
};

// ============================================================
// Section 2 - Package
// ============================================================

package CityMod_Bricks_Container {
	function fxDTSBrick::onCMPropPlant(%brick) {
		parent::onCMPropPlant(%brick);

		if(%brick.getDataBlock().getName() $= "brickCMTestContainerData") {
			if(!strLen(%brick.inventory) || !isExplicitObject(%brick.inventory)) {
				%brick.inventory = CityModInventory(4, 4);
			}
		}
	}

	function fxDTSBrick::onCMPropActivate(%brick, %player, %position, %rotation) {
		parent::onCMPropActivate(%brick);

		if(%brick.getDataBlock().getName() $= "brickCMTestContainerData") {
			if(isExplicitObject(%brick.inventory)) {
				%name = %brick.getDataBlock().uiName;
				%sizeX = %brick.inventory.size["X"];
				%sizeY = %brick.inventory.size["Y"];

				commandtoclient(%player.client, 'CM_Inventory_openInventory', %brick.inventory.getID(), %name, %sizeX, %sizeY);
			}
		}
	}
};

if(isPackage(CityMod_Bricks_Container))
	deactivatePackage(CityMod_Bricks_Container);
activatePackage(CityMod_Bricks_Container);