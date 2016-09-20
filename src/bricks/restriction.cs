// ============================================================
// Project          -      CityMod
// Description      -      Brick Placement Restrictions
// ============================================================
// Sections
//   1: Package
// ============================================================

// ============================================================
// Section 1 - Package
// ============================================================

package CityMod_Bricks_Restriction {
	function fxDTSBrick::onLoadPlant(%brick) {
		if(!isObject(%brick.getGroup().client)) {
			%brick.onPlant(true);
		} else {
			%brick.onPlant(false);
		}
	}

	function fxDTSBrick::onPlant(%brick, %onLoad) {
		parent::onPlant(%brick, %onLoad);

		if(!isObject(%brick) || !%brick.isPlanted()) {
			return;
		}

		%client = %brick.getGroup().client;

		if(!%onLoad && !%client.inCMDevMode) {
			if(!%datablock.isCityModBrick || (%datablock.isCityModBrick && (%datablock.citymodBrick["Type"] !$= "PROPERTY"))) {
				// Lot Existence Check
				%property = %brick;
				while(isObject(%property) && (%property.getDataBlock().citymodBrick["Type"] !$= "PROPERTY")) {
					%property = %property.getDownBrick(0);
				}

				if(!isObject(%property)) {
					CMError(0, "fxDTSBrick::onPlant", "!%property");
					commandToClient(%client, 'centerPrint', "<font:Impact:32><color:FF0000>Brick is not on a Property!", 2);
					%brick.delete();
					return;
				}

				// Lot Bounds Check
				if(mFloor(getWord(%brick.rotation, 3)) == 90) {
					%boxSize = (%brick.getDataBlock().bricksizeY / 2) SPC (%brick.getDataBlock().bricksizeX / 2) SPC $CM::Config::Players::MaxBuildHeight;
				} else {
					%boxSize = (%brick.getDataBlock().bricksizeX / 2) SPC (%brick.getDataBlock().bricksizeY / 2) SPC $CM::Config::Players::MaxBuildHeight;
				}

				initContainerBoxSearch(%brick.getWorldBoxCenter(), %boxSize, $TypeMasks::TriggerObjectType);

				while(isObject(%trigger = containerSearchNext())) {
					if(%trigger.getDatablock() == CMPropertyTrigger.getID()) {
						%propertyTrigger = %trigger;
						break;
					}
				}

				if(isObject(%propertyTrigger)) {
					%propertyTriggerMinX = getWord(%propertyTrigger.getWorldBox(), 0);
					%propertyTriggerMinY = getWord(%propertyTrigger.getWorldBox(), 1);
					%propertyTriggerMinZ = getWord(%propertyTrigger.getWorldBox(), 2);

					%propertyTriggerMaxX = getWord(%propertyTrigger.getWorldBox(), 3);
					%propertyTriggerMaxY = getWord(%propertyTrigger.getWorldBox(), 4);
					%propertyTriggerMaxZ = getWord(%propertyTrigger.getWorldBox(), 5);

					%brickMinX = getWord(%brick.getWorldBox(), 0) + 0.0016;
					%brickMinY = getWord(%brick.getWorldBox(), 1) + 0.0013;
					%brickMinZ = getWord(%brick.getWorldBox(), 2) + 0.00126;

					%brickMaxX = getWord(%brick.getWorldBox(), 3) - 0.0016;
					%brickMaxY = getWord(%brick.getWorldBox(), 4) - 0.0013;
					%brickMaxZ = getWord(%brick.getWorldBox(), 5) - 0.00126;

					if((%brickMinX >= %propertyTriggerMinX) && (%brickMinY >= %propertyTriggerMinY) && (%brickMinZ >= %propertyTriggerMinZ)) {
						if((%brickMaxX <= %propertyTriggerMaxX) && (%brickMaxY <= %propertyTriggerMaxY) && (%brickMaxZ <= %propertyTriggerMaxZ)) {
							%withinBounds = true;
						}
					}
				}

				if(!%withinBounds) {
					commandToClient(%client, 'centerPrint', "<font:Impact:32><color:FF0000>Brick is outside of the bounds of a Property!", 2);
					%brick.delete();
					return;
				}
			}

			if(%datablock.isCityModBrick) {
				// Validify Plant Restrictions
				switch$(%datablock.citymodBrick["Restriction"]) {
					case "NONE": // Maybe do something here? Probably not.
					case "ADMIN":
						if(!%client.isAdmin) {
							commandToClient(%client, 'centerPrint', "<font:Impact:32><color:FF0000>Only Admins are allowed to place this brick!", 2);
							%brick.delete();
							return;
						}
					case "SUPER_ADMIN":
						if(!%client.isSuperAdmin) {
							commandToClient(%client, 'centerPrint', "<font:Impact:32><color:FF0000>Only Super Admins are allowed to place this brick!", 2);
							%brick.delete();
							return;
						}
					case "HOST":
						if(%client.bl_id != getNumKeyID()) {
							commandToClient(%client, 'centerPrint', "<font:Impact:32><color:FF0000>Only the Host is allowed to place this brick!", 2);
							%brick.delete();
							return;
						}
				}
			}
		}

		// Call CityMod-Brick-Specific Function
		%function = "onCM" @ properText(%datablock.citymodBrick["Type"]) @ "Plant";
		if(isFunction("fxDTSBrick", %function)) {
			%brick.call(%function);
		}

		if(isObject(%brick)) {
			%brick.onSuccessfulPlant();
		}
	}

	function fxDTSBrick::onSuccessfulPlant(%brick) { }
	function fxDTSBrick::onCMPropPlant(%brick) { }
	function fxDTSBrick::onCMPropertyPlant(%brick) { }

	function fxDTSBrick::onActivate(%brick, %player, %position, %rotation) {
		parent::onActivate(%brick, %player, %position, %rotation);

		if(!%brick.getDatablock().isCityModBrick) {
			return;
		}

		%client = %brick.getGroup().client;

		// Call Brick-Specific Function
		%function = "onCM" @ properText(%brick.getDatablock().citymodBrick["Type"]) @ "Activate";
		if(isFunction(%brick.getClassName(), %function)) {
			%brick.call(%function, %player, %position, %rotation);
		}
	}

	function fxDTSBrick::onCMPropActivate(%brick, %player, %position, %rotation) { }
};

if(isPackage(CityMod_Bricks_Restriction))
	deactivatePackage(CityMod_Bricks_Restriction);
activatePackage(CityMod_Bricks_Restriction);