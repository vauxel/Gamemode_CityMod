// ============================================================
// Project          -      CityMod
// Description      -      Tick Module Initialization
// ============================================================

exec("./tick.cs");
exec("./daycycle.cs");

if(!isObject(CM_Tick)) {
	new ScriptObject(CM_Tick) {
		tickSpeed = $CM::Config::Tick::Speed;
		savePath = $CM::Config::Path::Data;
	};

	CityModTickModule.add(CM_Tick);
}