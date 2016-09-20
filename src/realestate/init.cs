// ============================================================
// Project          -      CityMod
// Description      -      Real Estate Module Initialization
// ============================================================

exec("./class.cs");

if(!isObject(CM_RealEstate)) {
	new ScriptObject(CM_RealEstate) {
		class = "CityModDatabase";
		dataClass = "CityModREProperty";
		path = $CM::Config::Path::Data @ "realestate";
	};

	CM_RealEstate.addField("Name", "Generic Property");
	CM_RealEstate.addField("Type", "Residential");
	CM_RealEstate.addField("Owner", 888888);

	CityModRealEstateModule.add(CM_RealEstate);
}

CM_BricksSaver.addSavedField("propertyID");