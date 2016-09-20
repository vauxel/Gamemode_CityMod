// ============================================================
// Project          -      CityMod
// Description      -      Money Items
// ============================================================
// Sections
//   1: One Dollar Datablocks
//     1.1: Item Datablock
//     1.2: Image Datablock
//   2: Five Dollar Datablocks
//     2.1: Item Datablock
//     2.2: Image Datablock
//   3: Ten Dollar Datablocks
//     3.1: Item Datablock
//     3.2: Image Datablock
//   4: Twenty Dollar Datablocks
//     4.1: Item Datablock
//     4.2: Image Datablock
//   5: Fifty Dollar Datablocks
//     5.1: Item Datablock
//     5.2: Image Datablock
//   6: Hundred Dollar Datablocks
//     6.1: Item Datablock
//     6.2: Image Datablock
//   7: Functions
// ============================================================

// ============================================================
// Section 1 - One Dollar Datablocks
// ============================================================

// ------------------------------------------------------------
// Section 1.1 - Item Datablock
// ------------------------------------------------------------
datablock ItemData(CMOneDollarItem) {
	category = "Weapon";
	className = "Weapon";

	shapeFile = $CM::Config::Path::Mod @ "res/items/Money/One/MoneyH.dts";
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	uiName = "$1 Bill";
	iconName = "";
	doColorShift = true;
	colorShiftColor = "0.0 0.501 0.0 1.0";

	image = CMOneDollarImage;
	canDrop = true;

	isMoney = true;
	monetaryValue = 1;
};

// ------------------------------------------------------------
// Section 1.2 - Image Datablock
// ------------------------------------------------------------
datablock ShapeBaseImageData(CMOneDollarImage) {
	shapeFile = $CM::Config::Path::Mod @ "res/items/Money/One/MoneyV.dts";
	emap = true;

	mountPoint = 0;
	offset = "0 0.05 0.05";

	correctMuzzleVector = false;
	//eyeOffset = "0.7 1.2 -0.75";

	className = "WeaponImage";

	item = CMOneDollarItem;
	ammo = " ";
	projectile = "";
	projectileType = "";

	melee = true;
	doRetraction = false;
	armReady = true;

	doColorShift = CMOneDollarItem.doColorShift;
	colorShiftColor = CMOneDollarItem.colorShiftColor;

	stateName[0]                     = "Activate";
	stateTimeoutValue[0]             = 0.0;
	stateTransitionOnTimeout[0]      = "Ready";

	stateName[1]                     = "Ready";
	stateTransitionOnTriggerDown[1]  = "PreFire";
	stateAllowImageChange[1]         = true;

	stateName[2]                     = "PreFire";
	stateScript[2]                   = "onPreFire";
	stateAllowImageChange[2]         = true;
	stateTimeoutValue[2]             = 0.01;
	stateTransitionOnTimeout[2]      = "Fire";

	stateName[3]                     = "Fire";
	stateTransitionOnTimeout[3]      = "CheckFire";
	stateTimeoutValue[3]             = 0.15;
	stateFire[3]                     = true;
	stateAllowImageChange[3]         = true;
	stateSequence[3]                 = "Fire";
	stateScript[3]                   = "onFire";
	stateWaitForTimeout[3]		       = true;
	stateSequence[3]                 = "Fire";

	stateName[4]                     = "CheckFire";
	stateTransitionOnTriggerUp[4]    = "StopFire";

	stateName[5]                     = "StopFire";
	stateTransitionOnTimeout[5]      = "Ready";
	stateTimeoutValue[5]             = 0.01;
	stateAllowImageChange[5]         = true;
	stateWaitForTimeout[5]           = true;
	stateSequence[5]                 = "StopFire";
	stateScript[5]                   = "onStopFire";
};

// ============================================================
// Section 2 - Five Dollar Datablocks
// ============================================================

// ------------------------------------------------------------
// Section 2.1 - Item Datablock
// ------------------------------------------------------------
datablock ItemData(CMFiveDollarItem : CMOneDollarItem) {
	shapeFile = $CM::Config::Path::Mod @ "res/items/Money/Five/MoneyH.dts";
	uiName = "$5 Bill";

	image = CMFiveDollarImage;
	monetaryValue = 5;
};

// ------------------------------------------------------------
// Section 2.2 - Image Datablock
// ------------------------------------------------------------
datablock ShapeBaseImageData(CMFiveDollarImage : CMOneDollarImage) {
	superClass = CMOneDollarImage;
	shapeFile = $CM::Config::Path::Mod @ "res/items/Money/Five/MoneyV.dts";

	item = CMFiveDollarItem;
};

// ============================================================
// Section 3 - Ten Dollar Datablocks
// ============================================================

// ------------------------------------------------------------
// Section 3.1 - Item Datablock
// ------------------------------------------------------------
datablock ItemData(CMTenDollarItem : CMOneDollarItem) {
	shapeFile = $CM::Config::Path::Mod @ "res/items/Money/Ten/MoneyH.dts";
	uiName = "$10 Bill";

	image = CMTenDollarImage;
	monetaryValue = 10;
};

// ------------------------------------------------------------
// Section 3.2 - Image Datablock
// ------------------------------------------------------------
datablock ShapeBaseImageData(CMTenDollarImage : CMOneDollarImage) {
	superClass = CMOneDollarImage;
	shapeFile = $CM::Config::Path::Mod @ "res/items/Money/Ten/MoneyV.dts";

	item = CMTenDollarItem;
};

// ============================================================
// Section 4 - Twenty Dollar Datablocks
// ============================================================

function CMTwentyDollarImage::onFire(%this, %player, %slot) {
	CMOneDollarImage::onFire(%this, %player, %slot);
}

// ------------------------------------------------------------
// Section 4.1 - Item Datablock
// ------------------------------------------------------------
datablock ItemData(CMTwentyDollarItem : CMOneDollarItem) {
	shapeFile = $CM::Config::Path::Mod @ "res/items/Money/Twenty/MoneyH.dts";
	uiName = "$20 Bill";

	image = CMTwentyDollarImage;
	monetaryValue = 20;
};

// ------------------------------------------------------------
// Section 4.2 - Image Datablock
// ------------------------------------------------------------
datablock ShapeBaseImageData(CMTwentyDollarImage : CMOneDollarImage) {
	superClass = CMOneDollarImage;
	shapeFile = $CM::Config::Path::Mod @ "res/items/Money/Twenty/MoneyV.dts";

	item = CMTwentyDollarItem;
};

// ============================================================
// Section 5 - Fifty Dollar Datablocks
// ============================================================

// ------------------------------------------------------------
// Section 5.1 - Item Datablock
// ------------------------------------------------------------
datablock ItemData(CMFiftyDollarItem : CMOneDollarItem) {
	shapeFile = $CM::Config::Path::Mod @ "res/items/Money/Fifty/MoneyH.dts";
	uiName = "$50 Bill";

	image = CMFiftyDollarImage;
	monetaryValue = 50;
};

// ------------------------------------------------------------
// Section 5.2 - Image Datablock
// ------------------------------------------------------------
datablock ShapeBaseImageData(CMFiftyDollarImage : CMOneDollarImage) {
	superClass = CMOneDollarImage;
	shapeFile = $CM::Config::Path::Mod @ "res/items/Money/Fifty/MoneyV.dts";

	item = CMFiftyDollarItem;
};

// ============================================================
// Section 6 - Hundred Dollar Datablocks
// ============================================================

// ------------------------------------------------------------
// Section 6.1 - Item Datablock
// ------------------------------------------------------------
datablock ItemData(CMHundredDollarItem : CMOneDollarItem) {
	shapeFile = $CM::Config::Path::Mod @ "res/items/Money/Hundred/MoneyH.dts";
	uiName = "$100 Bill";

	image = CMHundredDollarImage;
	monetaryValue = 100;
};

// ------------------------------------------------------------
// Section 6.2 - Image Datablock
// ------------------------------------------------------------
datablock ShapeBaseImageData(CMHundredDollarImage : CMOneDollarImage) {
	superClass = CMOneDollarImage;
	shapeFile = $CM::Config::Path::Mod @ "res/items/Money/Hundred/MoneyV.dts";

	item = CMHundredDollarItem;
};

// ============================================================
// Section 7 - Functions
// ============================================================
function CMOneDollarImage::onFire(%this, %player, %slot) {
	%start = %player.getEyePoint();
	%vec = vectorScale(%player.getMuzzleVector(%slot), getWord(%player.getScale(), 2) * 10);
	%end = vectorAdd(%start, %vec);
	%mask = $TypeMasks::FxBrickObjectType;

	%rayCast = containerRayCast(%start, %end, %mask);

	if(!%rayCast) {
		return;
	}

	%hitObj = getWord(%rayCast, 0);
	%hitPos = getWords(%rayCast, 1, 3);
	%hitNormal = getWords(%rayCast, 4, 6);

	warn(%this.getName() @ "::onFire [" @ %hitObj SPC %hitPos SPC %hitNormal @ "]");
}

function CMFiveDollarImage::onFire(%this, %player, %slot) {
	CMOneDollarImage::onFire(%this, %player, %slot);
}

function CMTenDollarImage::onFire(%this, %player, %slot) {
	CMOneDollarImage::onFire(%this, %player, %slot);
}

function CMFiftyDollarImage::onFire(%this, %player, %slot) {
	CMOneDollarImage::onFire(%this, %player, %slot);
}

function CMHundredDollarImage::onFire(%this, %player, %slot) {
	CMOneDollarImage::onFire(%this, %player, %slot);
}