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

datablock fxDTSBrickData(brickCM16x16PropertyData : brick16x16fData) {
	isCityModBrick = true;
	citymodBrick["Type"] = "PROPERTY";
	citymodBrick["Restriction"] = "ADMIN";

	//iconName = $CM::Config::Path::Mod @ "res/bricks/properties/16x16Property.png";
	uiName = "16x Property";
};

datablock fxDTSBrickData(brickCM32x32PropertyData : brick32x32fData) {
	isCityModBrick = true;
	citymodBrick["Type"] = "PROPERTY";
	citymodBrick["Restriction"] = "ADMIN";

	//iconName = $CM::Config::Path::Mod @ "res/bricks/properties/32x32Property.png";
	uiName = "32x Property";
};

// ============================================================
// Section 2 - Package
// ============================================================

package CityMod_Bricks_Property {
	function fxDTSBrick::onCMPropertyPlant(%brick) {
		parent::onCMPropertyPlant(%brick);

		%brick.createTrigger("CMPropertyTrigger");

		if(strLen(%brick.propertyID) && CM_RealEstate.dataExists(%brick.propertyID)) {
			%propertyData = CM_RealEstate.getData(%brick.propertyID);
		} else {
			%propertyData = CM_RealEstate.addData();
			%brick.propertyID = %propertyData.dataID;
		}

		%propertyData.linkPropertyBrick(%brick.getID());
	}

	function CMPropertyTrigger::onEnterTrigger(%this, %trigger, %obj) {
		parent::onEnterTrigger(%this, %trigger, %obj);

		echo("ENTER" TAB %trigger TAB %obj);

		if(%obj.getClassName() $= "Player") {
			%propertyData = CM_RealEstate.getData(%trigger.brick.propertyID);

			if(strLen(%propertyData)) {
				if(%propertyData.owner == 888888) {
					%owner = "The City";
				} else if(%propertyData.proprietorship $= "player") {
					%owner = CM_Players.getData(%propertyData.owner).name;
				} else if(%propertyData.proprietorship $= "organization") {
					%owner = CM_Organizations.getData(%propertyData.owner).name;
				}

				commandToClient(%obj.client, 'CM_Property_updatePropertyInfo', %propertyData.name, %owner);
			}
		}
	}

	function CMPropertyTrigger::onLeaveTrigger(%this, %trigger, %obj) {
		parent::onLeaveTrigger(%this, %trigger, %obj);

		echo("LEAVE" TAB %trigger TAB %obj);

		if(%obj.getClassName() $= "Player") {
			commandToClient(%obj.client, 'CM_Property_closePropertyInfo');
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