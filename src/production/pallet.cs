// ============================================================
// Project          -      CityMod
// Description      -      Pallet Bricks Code
// ============================================================
// Sections
//   1: Datablocks
//   2: Package
// ============================================================

// ============================================================
// Section 1 - Datablocks
// ============================================================

datablock fxDTSBrickData(brickCMPalletData) {
	brickFile = $CM::Config::Path::Mod @ "res/bricks/pallets/pallet.blb";

	isCityModBrick = true;
	citymodBrick["Type"] = "PROP";
	citymodBrick["Restriction"] = "NONE";

	iconName = $CM::Config::Path::Mod @ "res/bricks/pallets/pallet.png";
	uiName = "Pallet";
};

datablock fxDTSBrickData(brickCMPalletPlasticData) {
	brickFile = $CM::Config::Path::Mod @ "res/bricks/pallets/pallet_plastic.blb";

	isCityModBrick = true;
	citymodBrick["Type"] = "PROP";
	citymodBrick["Restriction"] = "NONE";

	iconName = $CM::Config::Path::Mod @ "res/bricks/pallets/pallet_plastic.png";
	uiName = "Pallet - Plastic";
};

datablock fxDTSBrickData(brickCMPalletMetalData) {
	brickFile = $CM::Config::Path::Mod @ "res/bricks/pallets/pallet_metal.blb";

	isCityModBrick = true;
	citymodBrick["Type"] = "PROP";
	citymodBrick["Restriction"] = "NONE";

	iconName = $CM::Config::Path::Mod @ "res/bricks/pallets/pallet_metal.png";
	uiName = "Pallet - Metal";
};

// ============================================================
// Section 2 - Package
// ============================================================

//package CityMod_Production_Pallet {
//
//};
//
//if(isPackage(CityMod_Production_Pallet))
//	deactivatePackage(CityMod_Production_Pallet);
//activatePackage(CityMod_Production_Pallet);