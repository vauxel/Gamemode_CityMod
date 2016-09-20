// ============================================================
// Project          -      CityMod
// Description      -      Player Database Hooks
// ============================================================

if(!isObject(CM_Players)) {
	new ScriptObject(CM_Players) {
		class = "CityModDatabase";
		dataClass = "CityModPlayer";
		path = $CM::Config::Path::Data @ "players";
	};

	CityModDatabaseModule.add(CM_Players);
}

package CityMod_Database_Player {
	function GameConnection::autoAdminCheck(%client) {
		parent::autoAdminCheck(%client);

		CM_Players.loadData(%client.bl_id);
	}

	function GameConnection::onClientLeaveGame(%client) {
		CM_Players.unLoadData(%client.bl_id);

		parent::onClientLeaveGame(%client);
	}
};

if(isPackage(CityMod_Database_Player))
	deactivatePackage(CityMod_Database_Player);
activatePackage(CityMod_Database_Player);