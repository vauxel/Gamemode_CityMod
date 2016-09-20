// ============================================================
// Project          -      CityMod
// Description      -      Brick Initialization
// ============================================================

exec("./restriction.cs");
exec("./saver.cs");
exec("./property.cs");
exec("./container.cs");

if(!isObject(CM_BricksSaver)) {
	new ScriptObject(CM_BricksSaver) {
		path = $CM::Config::Path::Data @ "saves";
		fileExtension = "cms";
	};

	CityModBricksModule.add(CM_BricksSaver);
}

CM_BricksSaver.addSavedField("inventory");