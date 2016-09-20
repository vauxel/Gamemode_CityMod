// ============================================================
// Project          -      CityMod
// Description      -      Player Inventory Commands
// ============================================================

function servercmdCM_Inventory_moveItem(%client, %id, %oldX, %oldY, %newX, %newY) {
	if(%id $= "CLIENT") {
		%inventory = CM_Players.getData(%client.bl_id).inventory;
	} else {
		%inventory = %id;
	}

	%inventory.moveSlotContents(%oldX, %oldY, %newX, %newY);
}

function servercmdCM_Inventory_transferItem(%client, %oldID, %oldX, %oldY, %newID, %newX, %newY) {
	if(%oldID $= "CLIENT") {
		%inventory1 = CM_Players.getData(%client.bl_id).inventory;
	} else {
		%inventory1 = %oldID;
	}

	if(%newID $= "CLIENT") {
		%inventory2 = CM_Players.getData(%client.bl_id).inventory;
	} else {
		%inventory2 = %newID;
	}

	%inventory1.transferSlotContents(%inventory2, %oldX, %oldY, %newX, %newY);
}

function servercmdCM_Inventory_swapItem(%client, %id, %x1, %y1, %x2, %y2) {
	if(%id $= "CLIENT") {
		%inventory = CM_Players.getData(%client.bl_id).inventory;
	} else {
		%inventory = %id;
	}

	%inventory.swapSlotContents(%x1, %y1, %x2, %y2);
}

function servercmdCM_Inventory_transferSwapItem(%client, %firstID, %firstX, %firstY, %secondID, %secondX, %secondY) {
	if(%firstID $= "CLIENT") {
		%inventory1 = CM_Players.getData(%client.bl_id).inventory;
	} else {
		%inventory1 = %firstID;
	}

	if(%secondID $= "CLIENT") {
		%inventory2 = CM_Players.getData(%client.bl_id).inventory;
	} else {
		%inventory2 = %secondID;
	}

	%inventory1.transferSwapSlotContents(%inventory2, %firstX, %firstY, %secondX, %secondY);
}

function servercmdCM_Inventory_dropItem(%client, %id, %x, %y) {
	if(%id $= "CLIENT") {
		%inventory = CM_Players.getData(%client.bl_id).inventory;
	} else {
		%inventory = %id;
	}

	if(%y $= "") {
		%y = %inventory.size["Y"] - 1;
	}

	%item = %inventory.getSlot(%x, %y);

	if(%item $= "") {
		servercmdCM_Inventory_requestSlotData(%client, %id, %x, %y);
		return;
	}

	%client.player.dropInventoryItem(%item);

	if((%item.get("Count") $= "") || (%item.get("Count") <= 1)) {
		%client.player.unmountInventorySlot();
		%inventory.setSlot(%x, %y, "");
	} else {
		%item.set("Count", %item.get("Count") - 1);
	}

	servercmdCM_Inventory_requestSlotData(%client, %id, %x, %y);
}

function servercmdCM_Inventory_splitItem(%client, %id, %x, %y, %newcount) {
	if(%id $= "CLIENT") {
		%inventory = CM_Players.getData(%client.bl_id).inventory;
	} else {
		%inventory = %id;
	}

	%item = %inventory.getSlot(%x, %y);

	if((%item.get("Count") $= "") || (%item.get("Count") < 2)) {
		return;
	}

	if((%newcount <= 0) || ((%item.get("Count") - %newcount) <= 0)) {
		return;
	}

	%newslot = %inventory.findAvailableSlot();
	%newitem = %item.copy();
	%newitem.set("Count", %newcount);
	%inventory.setSlot(getWord(%newslot, 0), getWord(%newslot, 1), %newitem, "LINK");
	%item.set("Count", %item.get("Count") - %newcount);

	servercmdCM_Inventory_requestSlotData(%client, %id, %x, %y);
	servercmdCM_Inventory_requestSlotData(%client, %id, getWord(%newslot, 0), getWord(%newslot, 1));
}

function servercmdCM_Inventory_requestData(%client, %id) {
	if(%id $= "CLIENT") {
		%inventory = CM_Players.getData(%client.bl_id).inventory;
	} else {
		%inventory = %id;
	}

	for(%y = 0; %y < %inventory.size["Y"]; %y++) {
		for(%x = 0; %x < %inventory.size["X"]; %x++) {
			if(%inventory.isSlotOccupied(%x, %y)) {
				%item = %inventory.getSlot(%x, %y);
				commandtoclient(%client, 'CM_Inventory_setSlot', %id, %x, %y, %item.get("Type"), %item.get("Name"), %item.get("Count"));
			} else {
				commandtoclient(%client, 'CM_Inventory_clearSlot', %id, %x, %y);
			}
		}
	}
}

function servercmdCM_Inventory_requestSlotData(%client, %id, %x, %y) {
	if(%id $= "CLIENT") {
		%inventory = CM_Players.getData(%client.bl_id).inventory;
	} else {
		%inventory = %id;
	}

	if(%inventory.isSlotOccupied(%x, %y)) {
		%item = %inventory.getSlot(%x, %y);
		commandtoclient(%client, 'CM_Inventory_setSlot', %id, %x, %y, %item.get("Type"), %item.get("Name"), %item.get("Count"));
	} else {
		commandtoclient(%client, 'CM_Inventory_clearSlot', %id, %x, %y);
	}
}

function servercmdCM_Inventory_useItem(%client, %slot) {
	if(!isNumber(%slot)) {
		return;
	}

	if(%client.player.isInventorySlotMounted()) {
		%client.player.unmountInventorySlot();
	}

	%inventory = CM_Players.getData(%client.bl_id).inventory;

	%x = mClamp(%slot, 0, %inventory.size["X"] - 1);
	%y = %inventory.size["Y"] - 1;

	if(%inventory.isSlotOccupied(%x, %y)) {
		%client.player.mountInventorySlot(%x, %y);
	}
}

function servercmdCM_Inventory_unUseItem(%client) {
	if(%client.player.isInventorySlotMounted()) {
		%client.player.unmountInventorySlot();
	}
}

function servercmdCM_Inventory_toggleUseItem(%client, %slot) {
	if(!isNumber(%slot)) {
		return;
	}

	if(!isObject(%client.player)) {
		return;
	}

	if(%client.player.isInventorySlotMounted()) {
		%client.player.unmountInventorySlot();
	} else {
		%inventory = CM_Players.getData(%client.bl_id).inventory;

		%x = mClamp(%slot, 0, %inventory.size["X"] - 1);
		%y = %inventory.size["Y"] - 1;

		%client.player.mountInventorySlot(%x, %y);
	}
}

package CityMod_Inventory_Player {
	function servercmdPlantBrick(%client) {
		if(!isObject(%client.player.tempBrick) || !%client.player.isInventorySlotMounted()) {
			return;
		}

		%brick = new fxDTSBrick() {
			datablock = %client.player.tempBrick.getDatablock();
			position = %client.player.tempBrick.position;
			rotation = %client.player.tempBrick.rotation;
			angleID = %client.player.tempBrick.angleID;
			colorID = %client.player.tempBrick.colorID;
			colorFxID = %client.player.tempBrick.colorFxID;
			shapeFxID = %client.player.tempBrick.shapeFxID;
			printID = %client.player.tempBrick.printID;
			client = %client;
			stackBL_ID = %client.bl_id;
			isPlanted = true;
		};

		%client.brickGroup.add(%brick);
		%error = %brick.plant();

		if(%error == 0) {
			if(isObject(%brick) && %brick.isPlanted()) {
				%brick.playSound(brickPlantSound);
				%brick.setTrusted(1);
				%client.undoStack.push(%brick TAB "PLANT");
			}
		} else {
			switch(%error) {
				case 1: %cmd = 'MsgPlantError_Overlap';
				case 2: %cmd = 'MsgPlantError_Float';
				case 3: %cmd = 'MsgPlantError_Stuck';
				case 4: %cmd = 'MsgPlantError_Unstable';
				case 5: %cmd = 'MsgPlantError_Buried';
				case 6: %cmd = 'MsgPlantError_TooFar';
				case 7: %cmd = 'MsgPlantError_TooLoud';
				case 8: %cmd = 'MsgPlantError_Limit';
			}

			messageClient(%client, %cmd);
			%brick.delete();
		}
	}

	function Armor::onCollision(%this, %obj, %col, %vec, %vecLen) {
		%inventory = CM_Players.getData(%obj.client.bl_id).inventory;

		if((%col.getClassName() $= "Item") && (%inventory.findAvailableSlot() !$= "FULL")) {
			%inventory.addItem("ITEM", %col.getDatablock().uiName, %col.getDatablock().getName());
			serverPlay3D(ItemPickup, %obj.getHackPosition());
			servercmdCM_Inventory_requestData(%obj.client, "CLIENT");

			%col.delete();
			return;
		}

		parent::onCollision(%this, %obj, %col, %vec, %vecLen);
	}

	function Player::dropInventoryItem(%player, %item) {
		if(!isMapObject(%item) || ((%item.get("Data") $= "") || !isObject(%item.get("Data")))) {
			CMError(2, "Player::dropInventoryItem", "Invalid Item given");
			return "ERROR";
		}

		%droppable = new Item() {
			datablock = %item.get("Data");
			position = %player.getPosition();
			canPickup = true;
		};

		if(isObject(%droppable)) {
			%droppable.setTransform(vectorAdd(%player.getEyePoint(), vectorScale(%player.getEyeVector(), 1)));
			%droppable.setVelocity(vectorScale(%player.getEyeVector(), 5));
		}
	}

	function Player::isInventorySlotMounted(%player) {
		return (%player.mountedInventorySlot !$= "") && CM_Players.getData(%player.client.bl_id).inventory.isSlotOccupied(getWord(%player.mountedInventorySlot, 0), getWord(%player.mountedInventorySlot, 1)) && isObject(%player.getMountedImage(0));
	}

	function Player::mountInventorySlot(%player, %x, %y) {
		if(%player.isInventorySlotMounted()) {
			CMError(2, "Player::mountInventorySlot", "There is already an inventory item mounted");
			return "ERROR";
		}

		if(!isObject(%item = CM_Players.getData(%player.client.bl_id).inventory.getSlot(%x, %y))) {
			CMError(2, "Player::mountInventorySlot", "Invalid Item given");
			return;
		}

		if((%item.get("Type") !$= "ITEM") && (%item.get("Type") !$= "BRICK")) {
			CMError(2, "Player::mountInventorySlot", "Can't mount non-Item or non-Brick item");
			return;
		}

		if(%item.get("Type") $= "BRICK") {
			%player.mountImage(CMBrickPlacerImage, 0);
		} else {
			if(%item.get("Data").getClassName() $= "ShapeBaseImageData") {
				%player.mountImage(%item.get("Data").getID(), 0);
			} else if(%item.get("Data").getClassName() $= "ItemData") {
				if(isObject(%item.get("Data").image)) {
					%player.mountImage(%item.get("Data").image.getID(), 0);
				}
			}
		}

		fixArmReady(%player);

		%player.mountedInventorySlot = %x SPC %y;
		serverPlay3D(weaponSwitchSound, %player.getHackPosition());
	}

	function Player::unmountInventorySlot(%player) {
		if(!%player.isInventorySlotMounted()) {
			CMError(2, "Player::unmountInventorySlot", "There is currently not an inventory item mounted");
			return "ERROR";
		}

		if((%player.getMountedImage(0).getID() == CMBrickPlacerImage.getID()) && isObject(%player.tempBrick)) {
			%player.tempBrick.delete();
		}

		%player.unmountImage(0);
		fixArmReady(%player);
		%player.mountedInventorySlot = "";
		serverPlay3D(weaponSwitchSound, %player.getHackPosition());
	}

	function fxDTSBrick::onSuccessfulPlant(%brick) {
		parent::onSuccessfulPlant(%brick);
		%client = %brick.getGroup().client;

		if(!%client.player.isInventorySlotMounted()) {
			return;
		}

		%inventory = CM_Players.getData(%client.bl_id).inventory;
		%mountedItem = %inventory.getSlot(%x = getWord(%client.player.mountedInventorySlot, 0), %y = getWord(%client.player.mountedInventorySlot, 1));

		if((%mountedItem.get("Count") $= "") || (%mountedItem.get("Count") <= 1)) {
			%client.player.unmountInventorySlot();
			%inventory.setSlot(%x, %y, "");
		} else {
			%mountedItem.set("Count", %mountedItem.get("Count") - 1);
		}

		servercmdCM_Inventory_requestSlotData(%client, "CLIENT", %x, %y);
	}
};

if(isPackage(CityMod_Inventory_Player))
	deactivatePackage(CityMod_Inventory_Player);
activatePackage(CityMod_Inventory_Player);
