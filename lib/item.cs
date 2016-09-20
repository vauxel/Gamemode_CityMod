// ============================================================
// Project          -      CityMod
// Description      -      Item Support Functions
// ============================================================

function Player::addItem(%player, %image) {
	%success = false;
	%client = %player.client;

	for(%i = 0; %i < %player.getDatablock().maxTools; %i++)
	{
		%tool = %player.tool[%i];
		if(%tool == 0) {
			%player.tool[%i] = %image;
			%player.weaponCount++;
			messageClient(%client, 'MsgItemPickup', '', %i, %image);
			%success = true;
			break;
		}
	}

	return %success;
}

function Player::removeItem(%this,%item) {
	if(!isObject(%this) || !isObject(%item.getID())) {
		return;
	}

	for(%i = 0; %i < %this.getDatablock().maxTools; %i++) {
		if(isObject(%this.tool[%i])) {
			%tool = %this.tool[%i].getID();

			if(%tool == %item.getID()) {
				%this.tool[%i] = 0;
				messageClient(%this.client, 'MsgItemPickup', '', %i, 0);

				if(%this.currTool == %i) {
					%this.updateArm(0);
					%this.unMountImage(0);
				}
			}
		}
	}
}

function Player::removeItemSlot(%this, %slot) {
	%this.tool[%slot] = 0;
	messageClient(%this.client, 'MsgItemPickup', '', %slot, 0);

	if(%this.currTool == %slot) {
		%this.updateArm(0);
		%this.unMountImage(0);
	}

	%this.weaponCount--;
}
