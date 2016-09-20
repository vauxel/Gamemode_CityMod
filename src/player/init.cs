// ============================================================
// Project          -      CityMod
// Description      -      Player Module Initialization
// ============================================================

exec("./datablock.cs");
exec("./infopanel.cs");
exec("./skills.cs");
exec("./hitbox.cs");

if(!isObject(CM_SkillsInfo)) {
	new ScriptObject(CM_SkillsInfo) {
		skillsets = Map();
	};

	CM_SkillsInfo.addSkillSet("law", "Law");
	CM_SkillsInfo.addSkillSet("guns", "Gun Handling");

	CityModDatabaseModule.add(CM_SkillsInfo);
}

CM_Players.addField("Skills", (new ScriptObject() {
  class = "CityModPlayerSkills";
  list = Array(); // List (Array) of skills the player has

  // Skillset stats
  law = Map().set("Level", 0).set("XP", 0).set("Points", 0);
  guns = Map().set("Level", 0).set("XP", 0).set("Points", 0);
} @ "\x01"));

CM_Players.addField("Limbs", Map().set("head", 0).set("torso", 0).set("hip", 0).set("arms", 0).set("legs", 0));