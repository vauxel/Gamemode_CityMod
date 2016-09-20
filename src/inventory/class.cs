// ============================================================
// Project          -      CityMod
// Description      -      Inventory Class
// ============================================================

function CityModInventory(%sizeX, %sizeY) {
	return (new ScriptObject() {
		class = "CityModInventory";
		sizeX = %sizeX $= "" ? 6 : %sizeX;
		sizeY = %sizeY $= "" ? 6 : %sizeY;
	} @ "\x01");
}

function CityModInventory::onAdd(%this) {
	if(!strLen(%this.sizeX)) {
		CMError(1, "CityModInventory::onAdd", "No \"X\" size given");
		%this.schedule(0, "delete");
		return "ERROR";
	}

	if(!strLen(%this.sizeY)) {
		CMError(1, "CityModInventory::onAdd", "No \"X\" size given");
		%this.schedule(0, "delete");
		return "ERROR";
	}

	if(%this.sizeX <= 0) {
		CMError(1, "CityModInventory::onAdd", "Invalid \"X\" size given");
		%this.schedule(0, "delete");
		return "ERROR";
	}

	if(%this.sizeY <= 0) {
		CMError(1, "CityModInventory::onAdd", "Invalid \"Y\" size given");
		%this.schedule(0, "delete");
		return "ERROR";
	}
}

function CityModInventory::onRemove(%this) {
	for(%y = 0; %y < %this.size["Y"]; %y++) {
		for(%x = 0; %x < %this.size["X"]; %x++) {
			if(isExplicitObject(%object = %this.getSlot(%x, %y))) {
				%object.delete();
			}
		}
	}
}

function CityModInventory::findAvailableSlot(%this, %item) {
	if(%item $= "") {
		for(%y = 0; %y < %this.size["Y"]; %y++) {
			for(%x = 0; %x < %this.size["X"]; %x++) {
				%slot = %this.getSlot(%x, %y);

				if(%slot $= "") {
					return %x SPC %y SPC false;
				}
			}
		}
	} else {
		for(%y = 0; %y < %this.size["Y"]; %y++) {
			for(%x = 0; %x < %this.size["X"]; %x++) {
				%slot = %this.getSlot(%x, %y);

				if(%slot $= "") {
					if(%emptySlot $= "") {
						%emptySlot = %x SPC %y SPC false;
					}
				} else if(%slot.get("Name") $= %item.get("Name")) {
					return %x SPC %y SPC true;
				}
			}
		}

		if(%emptySlot !$= "") {
			return %emptySlot;
		}
	}

	return "FULL";
}

function CityModInventory::getSlot(%this, %x, %y) {
	if(!strLen(%x) || (%x < 0) || (%x >= %this.size["X"])) {
		CMError(2, "CityModInventory::getSlot", "Invalid \"%x\" position");
		return "ERROR";
	}

	if(!strLen(%y) || (%y < 0) || (%y >= %this.size["Y"])) {
		CMError(2, "CityModInventory::getSlot", "Invalid \"%y\" position");
		return "ERROR";
	}

	return %this.slot[%x, %y];
}

function CityModInventory::setSlot(%this, %x, %y, %value, %type) {
	if(!strLen(%x) || (%x < 0) || (%x >= %this.size["X"])) {
		CMError(2, "CityModInventory::getSlot", "Invalid \"%x\" position");
		return "ERROR";
	}

	if(!strLen(%y) || (%y < 0) || (%y >= %this.size["Y"])) {
		CMError(2, "CityModInventory::getSlot", "Invalid \"%y\" position");
		return "ERROR";
	}

	if((%value !$= "") && !isMapObject(%value)) {
		CMError(2, "CityModInventory::setSlot", "Invalid \"%value\" -- If not empty, it must be a \"MapObject\" with, at the minimum, a \"Name\" key");
		return "ERROR";
	}

	if(!strLen(%value)) {
		%type = "DELETE";
	} else if(!strLen(%type)) {
		%type = "COPY";
	}

	if((%type !$= "LINK") && isExplicitObject(%this.slot[%x, %y])) {
		%this.slot[%x, %y].delete();
	}

	if(%type $= "DELETE") {
		%this.slot[%x, %y] = "";
	} else {
		%this.slot[%x, %y] = (%type $= "LINK") ? %value : %value.copy();
	}
}

function CityModInventory::isSlotOccupied(%this, %x, %y) {
	return isExplicitObject(%this.getSlot(%x, %y));
}

function CityModInventory::moveSlotContents(%this, %oldx, %oldy, %newx, %newy) {
	if((%oldx SPC %oldy) $= (%newx SPC %newy)) {
		return; // Moving slot contents into the same slot will not be counted as an error
	}

	if(!%this.isSlotOccupied(%oldx, %oldy)) {
		CMError(2, "CityModInventory::moveSlotContents", "\"Old\" Slot is empty");
		return "ERROR";
	}

	if(%this.isSlotOccupied(%newx, %newy)) {
		CMError(2, "CityModInventory::moveSlotContents", "\"New\" Slot is occupied");
		return "ERROR";
	}

	%this.setSlot(%newx, %newy, %this.getSlot(%oldx, %oldy), "COPY");
	%this.setSlot(%oldx, %oldy, "");
}

function CityModInventory::swapSlotContents(%this, %x1, %y1, %x2, %y2) {
	if(!%this.isSlotOccupied(%x1, %y1) || !%this.isSlotOccupied(%x2, %y2)) {
		CMError(2, "CityModInventory::swapSlotContents", "One of the slots is not occupied");
		return "ERROR";
	}

	%oneContents = %this.getSlot(%x1, %y1);
	%twoContents = %this.getSlot(%x2, %y2);

	%this.setSlot(%x1, %y1, %twoContents, "LINK");
	%this.setSlot(%x2, %y2, %oneContents, "LINK");
}

function CityModInventory::transferSlotContents(%this, %to, %oldx, %oldy, %newx, %newy) {
	if(!isObject(%to) || (%to.class !$= "CityModInventory")) {
		CMError(2, "CityModInventory::transferSlotContents", "Invalid \"%to\" Inventory object given");
		return "ERROR";
	}

	if(!%this.isSlotOccupied(%oldx, %oldy)) {
		CMError(2, "CityModInventory::transferSlotContents", "\"Old\" Slot is empty");
		return "ERROR";
	}

	if(%to.isSlotOccupied(%newx, %newy)) {
		CMError(2, "CityModInventory::transferSlotContents", "\"New\" Slot is occupied");
		return "ERROR";
	}

	%to.setSlot(%newx, %newy, %this.getSlot(%oldx, %oldy), "COPY");
	%this.setSlot(%oldx, %oldy, "");
}

function CityModInventory::transferSwapSlotContents(%this, %other, %x1, %y1, %x2, %y2) {
	if(!isObject(%other) || (%other.class !$= "CityModInventory")) {
		CMError(2, "CityModInventory::transferSwapSlotContents", "Invalid \"%other\" Inventory object given");
		return "ERROR";
	}

	if(!%this.isSlotOccupied(%x1, %y1) || !%other.isSlotOccupied(%x2, %y2)) {
		CMError(2, "CityModInventory::swapSlotContents", "One of the slots is not occupied");
		return "ERROR";
	}

	%firstContents = %this.getSlot(%x1, %y1);
	%secondContents = %other.getSlot(%x2, %y2);

	%this.setSlot(%x1, %y1, %secondContents, "LINK");
	%other.setSlot(%x2, %y2, %firstContents, "LINK");
}

function CityModInventory::hasItem(%this, %name) {
	if(!strLen(%name)) {
		CMError(2, "CityModInventory::hasItem", "Invalid \"%name\"");
		return "ERROR";
	}

	for(%y = 0; %y < %this.size["Y"]; %y++) {
		for(%x = 0; %x < %this.size["X"]; %x++) {
			%slot = %this.getSlot(%x, %y);

			if(%slot $= "") {
				continue;
			}

			if(%name $= %slot.get("Name")) {
				return true;
			}
		}
	}

	return false;
}

function CityModInventory::addItem(%this, %type, %name, %datablock, %extra) {
	if(%type $= "") {
		CMError(2, "CityModInventory::addItem", "No \"%type\" given");
		return;
	}

	if(%name $= "") {
		CMError(2, "CityModInventory::addItem", "No \"%name\" given");
		return;
	}

	if((%type !$= "ITEM") && (%type !$= "BRICK") && (%type !$= "OBJECT")) {
		CMError(2, "CityModInventory::addItem", "Invalid \"%type\" given");
		return;
	} else {
		if(%datablock $= "") {
			CMError(2, "CityModInventory::addItem", "No \"%datablock\" given");
			return;
		}

		if(!isObject(%datablock)) {
			CMError(2, "CityModInventory::addItem", "\"%datablock\" given does not exist");
			return;
		}
	}

	%item = Map();
	%item.set("Count", 1);

	if(isMapObject(%extra)) {
		for(%i = 0; %i < %extra.keys.length; %i++) {
			%item.set(%extra.keys.value[%i], %extra.get(%extra.keys.value[%i]));
		}

		%extra.delete();
	}

	%item.set("Type", strUpr(%type));
	%item.set("Name", %name);
	%item.set("Data", %datablock);

	%slot = %this.findAvailableSlot(%item);

	if(%slot $= "FULL") {
		CMError(2, "CityModInventory::addItem", "Inventory is full");
		%item.delete();
		return "ERROR";
	}

	if(getWord(%slot, 2) == false) { // Empty slot
		%this.setSlot(getWord(%slot, 0), getWord(%slot, 1), %item, "LINK");
	} else { // Occupied, but stackable, slot
		%this.getSlot(getWord(%slot, 0), getWord(%slot, 1)).set("Count", %this.getSlot(getWord(%slot, 0), getWord(%slot, 1)).get("Count") + 1);
	}
}

function CityModInventory::getItemSlot(%this, %item) {
	if(!isMapObject(%item)) {
		CMError(2, "CityModInventory::getItemSlot", "Invalid \"%item\" given");
		return "ERROR";
	}

	for(%y = 0; %y < %this.size["Y"]; %y++) {
		for(%x = 0; %x < %this.size["X"]; %x++) {
			%slot = %this.getSlot(%x, %y);

			if(%slot $= "") {
				continue;
			}

			if(%slot.getID() == %item.getID()) {
				return %x SPC %y;
			}
		}
	}
}