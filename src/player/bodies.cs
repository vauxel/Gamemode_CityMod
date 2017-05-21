// ============================================================
// Project          -      CityMod
// Description      -      Player Death Physics
// ============================================================

package CityMod_Player_Death {
	function Armor::onDisabled(%this, %player, %enabled) {
		Parent::onDisabled(%this, %player, %enabled);

		%vehicle = new WheeledVehicle() {
			dataBlock = CMCorpseVehicle;
			client = %player.client;
		};

		MissionCleanup.add(%vehicle);

		%vehicle.setTransform(%player.getTransform());
	}
};

if(isPackage(CityMod_Player_Death))
	deactivatePackage(CityMod_Player_Death);
activatePackage(CityMod_Player_Death);