// ============================================================
// Project          -      CityMod
// Description      -      Player Datablocks
// ============================================================

datablock PlayerData(PlayerCM : PlayerStandardArmor) {
	uiName = "CityMod Player";

	canJet = 0;
};

datablock PlayerData(PlayerSlowCM : PlayerCM) {
	uiName = "CityMod Player Slow";

	maxForwardSpeed = 2;
	maxBackwardSpeed = 1;
	maxSideSpeed = 1;

	maxForwardCrouchSpeed = 0;
	maxBackwardCrouchSpeed = 0;
	maxSideCrouchSpeed = 0;

	maxJumpSpeed = 5;
};

datablock WheeledVehicleData(CMCorpseVehicle) {
	category = "Vehicles";
	shapeFile = "Add-ons/Item_Skis/deathvehicle.dts";
	emap = true;

	numMountPoints = 1;
	maxDamage = 999999;
	destroyedLevel = 200;

	mass = 200;
	massCenter = "0 0 1";
	massBox = "0.8 0.5 0.9";

	drag = 0.8;
	density = 1;
	integration = 4;
	bodyFriction = 1;
	bodyRestitution = 0.2;

	minImpactSpeed = 0.1;
	minRunOverSpeed = 100;
	collisionTol = 0.4;

	isSled = true;
};