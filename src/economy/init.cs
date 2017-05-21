// ============================================================
// Project          -      CityMod
// Description      -      Organizations Module Initialization
// ============================================================

exec("./bank.cs");
exec("./money.cs");

if(!isObject(CM_Bank)) {
	new ScriptObject(CM_Bank) {
		class = "CityModDatabase";
		dataClass = "CityModBankAccount";
		path = $CM::Config::Path::Data @ "bank";
	};

	CM_Bank.addField("Type", "player");
	CM_Bank.addField("Owner", 888888);
	CM_Bank.addField("Number", "NONE");
	CM_Bank.addField("PIN", 999999);
	CM_Bank.addField("Balance", 0);
	CM_Bank.addField("Ledger", Array());
	CM_Bank.loadAllData();

	CityModEconomyModule.add(CM_Bank);
}

CM_Players.addField("Account", "NONE");