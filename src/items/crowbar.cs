// ============================================================
// Project          -      CityMod
// Description      -      Crowbar Item
// ============================================================
// Sections
//   1: Datablocks
//     1.1: Item Datablock
//     1.2: Image Datablock
// ============================================================

// ============================================================
// Section 1 - Datablocks
// ============================================================

// ------------------------------------------------------------
// Section 1.1 - Item Datablock
// ------------------------------------------------------------
datablock ItemData(CMCrowbarItem)
{
	category = "Weapon";  // Mission editor category;
	className = "Weapon"; // For inventory system

	// Basic Item Properties
	shapeFile = $CM::Config::Path::Mod @ "res/items/Crowbar/Crowbar.dts";
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Crowbar";
	iconName = "";

	doColorShift = true;
	colorShiftColor = "0.458 0.028 0.0 1.0";

	// Dynamic properties defined by the scripts
	image = CMCrowbarImage;
	canDrop = true;
};

// ------------------------------------------------------------
// Section 1.2 - Image Datablock
// ------------------------------------------------------------
datablock ShapeBaseImageData(CMCrowbarImage)
{
	// Basic Item properties
	shapeFile = $CM::Config::Path::Mod @ "res/items/Crowbar/Crowbar.dts";
	emap = true;

	// Specify mount point & offset for 3rd person, and eye offset
	// for first person rendering.
	mountPoint = 0;
	offset = "0 0 0";

	// When firing from a point offset from the eye, muzzle correction
	// will adjust the muzzle vector to point to the eye LOS point.
	// Since this weapon doesn't actually fire from the muzzle point,
	// we need to turn this off.
	correctMuzzleVector = false;

	//eyeOffset = "0.7 1.2 -0.25";

	// Add the WeaponImage namespace as a parent, WeaponImage namespace
	// provides some hooks into the inventory system.
	className = "WeaponImage";

	// Projectile && Ammo.
	item = CMCrowbarItem;
	ammo = " ";
	projectile = "";
	projectileType = Projectile;

	doColorShift = true;
	colorShiftColor = "0.458 0.028 0.0 1.0";

	//melee particles shoot from eye node for consistancy
	melee = true;
	doRetraction = false;
	//raise your arm up or not
	armReady = true;

	//casing = " ";

	// Images have a state system which controls how the animations
	// are run, which sounds are played, script callbacks, etc. This
	// state system is downloaded to the client so that clients can
	// predict state changes and animate accordingly.  The following
	// system supports basic ready->fire->reload transitions as
	// well as a no-ammo->dryfire idle state.

	// Initial start up state
	stateName[0]                     = "Activate";
	stateTimeoutValue[0]             = 0.5;
	stateSound[0]                    = weaponSwitchSound;
};