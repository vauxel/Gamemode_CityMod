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