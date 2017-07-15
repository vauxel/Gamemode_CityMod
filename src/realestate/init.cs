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
	CM_RealEstate.addField("Proprietorship", "player");
	CM_RealEstate.addField("Size", "32x32");
	CM_RealEstate.addField("Description", "This property's description has not been set.");
	CM_RealEstate.addField("Price", 0);
	CM_RealEstate.addField("Selling", true);
	CM_RealEstate.addField("Brick", -1);

	CityModRealEstateModule.add(CM_RealEstate);
}

CM_BricksSaver.addSavedField("propertyID");