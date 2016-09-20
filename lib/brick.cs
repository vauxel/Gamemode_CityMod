// ============================================================
// Project          -      CityMod
// Description      -      Brick Support Functions
// ============================================================

function fxDTSBrick::createTrigger(%this, %datablock) {
	%trigger = new Trigger() {
		datablock = %datablock;
		polyhedron = "0 0 0 1 0 0 0 -1 0 0 0 1";
	} @ "\x01";

	MissionCleanup.add(%trigger);

	%triggerX = %this.getDatablock().brickSizeX;
	%triggerY = %this.getDatablock().brickSizeY;

	if(mFloor(getWord(%this.rotation, 3)) == 90) {
		%trigger.setScale((%triggerY / 2) SPC (%triggerX / 2) SPC $CM::Config::Players::MaxBuildHeight);
	}	else {
		%trigger.setScale((%triggerX / 2) SPC (%triggerY / 2) SPC $CM::Config::Players::MaxBuildHeight);
	}

	%offsetPos = vectorAdd(vectorSub(%this.getWorldBoxCenter(), %trigger.getWorldBoxCenter()), "0 0 0.1");
	%trigger.setTransform(%offsetPos);

	%this.trigger = %trigger;
	%trigger.brick = %this;

	return %trigger;
}

package CityMod_BrickSupport {
	function fxDTSBrick::onRemove(%this) {
		if(isExplicitObject(%this.trigger)) {
			%this.trigger.delete();
		}

		parent::onRemove(%this);
	}
};

if(isPackage(CityMod_BrickSupport))
	deactivatePackage(CityMod_BrickSupport);
activatePackage(CityMod_BrickSupport);