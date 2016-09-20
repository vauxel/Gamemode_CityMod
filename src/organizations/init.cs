// ============================================================
// Project          -      CityMod
// Description      -      Organizations Module Initialization
// ============================================================

exec("./class.cs");
exec("./tasks.cs");
exec("./player.cs");

if(!isObject(CM_Organizations)) {
	new ScriptObject(CM_Organizations) {
		class = "CityModDatabase";
		dataClass = "CityModOrganization";
		path = $CM::Config::Path::Data @ "organizations";
	};

	CM_Organizations.addField("Name", "Default Name");
	CM_Organizations.addField("Owner", 888888);
	CM_Organizations.addField("Type", "Group");
	CM_Organizations.addField("Open", true);
	CM_Organizations.addField("Hidden", false);
	CM_Organizations.addField("Description", "A Default Description");
	CM_Organizations.addField("Jobs", Array());
	CM_Organizations.addField("Members", Array());
	CM_Organizations.addField("Invited", Array());
	CM_Organizations.addField("Applications", Array());
	CM_Organizations.addField("Account", 0);
	CM_Organizations.loadAllData();

	CityModOrganizationsModule.add(CM_Organizations);
}

//if(!isObject(CM_OrganizationJobTasks) && isObject(CM_Organizations)) {
//	new ScriptGroup(CM_OrganizationJobTasks);
//	CM_OrganizationJobTasks.registerTask("TEST_TASK", "Test Task", "Task activated through the console");
//
//	CityModOrganizationsModule.add(CM_Organizations);
//}