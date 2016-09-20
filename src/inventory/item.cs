// ============================================================
// Project          -      CityMod
// Description      -      Brick Placer Item
// ============================================================
// Sections
//   1: Datablocks
//   2: Functions
// ============================================================

// ============================================================
// Section 1 - Datablocks
// ============================================================
datablock ProjectileData(CMBrickPlacerProjectile : brickDeployProjectile) {
	explosion = brickDeployExplosion;
};

datablock ItemData(CMBrickPlacerItem) {
	category = "Weapon";
	className = "Weapon";

	shapeFile = "base/data/shapes/brickWeapon.dts";
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	uiName = "Brick Placer";
	iconName = "";
	doColorShift = true;
	colorShiftColor = "0.471 0.471 0.471 1.000";

	image = CMBrickPlacerImage;
	canDrop = false;
};

datablock ShapeBaseImageData(CMBrickPlacerImage : brickImage) {
	shapeFile = "base/data/shapes/brickWeapon.dts";
	emap = true;

	mountPoint = 0;
	offset = "0 0 0";

	correctMuzzleVector = false;
	eyeOffset = "0.7 1.2 -0.75";

	className = "WeaponImage";

	item = CMBrickPlacerItem;
	ammo = " ";
	projectile = CMBrickPlacerProjectile;
	projectileType = Projectile;

	melee = true;
	doRetraction = false;
	armReady = true;

	doColorShift = CMBrickPlacerItem.doColorShift;
	colorShiftColor = CMBrickPlacerItem.colorShiftColor;
};

// ============================================================
// Section 2 - Functions
// ============================================================
function CMBrickPlacerImage::onMount(%this, %obj, %slot) {
	%obj.playThread(1, "armReadyRight");
}

function CMBrickPlacerImage::onUnMount(%this, %obj, %slot) {
	%obj.playThread(1, "root");
}

function CMBrickPlacerImage::onPreFire(%this, %obj, %slot) {
	%obj.playThread(2, "activate");
}

function CMBrickPlacerImage::onStopFire(%this, %obj, %slot) {
	%obj.playThread(2, "root");
}

function CMBrickPlacerProjectile::onCollision(%this, %obj, %col, %fade, %pos, %normal) {
	if((%col.getClassName() !$= "fxDTSBrick") && (%col.getClassName() !$= "fxPlane")) {
		return;
	}

	%player = %obj.client.player;
	%item = CM_Players.getData(%player.client.bl_id).inventory.getSlot(getWord(%player.mountedInventorySlot, 0), getWord(%player.mountedInventorySlot, 1));

	if(!%player.isInventorySlotMounted() || (%item.get("Type") !$= "BRICK")) {
		return;
	}

	if(isObject(%player.tempBrick)) {
		%player.tempBrick.delete();
	}

	switch(getAngleIDFromPlayer(%player)) {
		case 0: %rotation = "1 0 0 0";
		case 1: %rotation = "0 0 1 90";
		case 2: %rotation = "0 0 1 180";
		case 3: %rotation = "0 0 -1 90";
	}

	%player.tempBrick = new fxDTSBrick() {
		datablock = %item.get("Data");
		position = vectorAdd(%pos, "0 0 0.4");
		rotation = %rotation;
		colorID = %player.client.currentColor;
		client = %player.client;
	};
}