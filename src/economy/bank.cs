// ============================================================
// Project          -      CityMod
// Description      -      Bank Class / Commands
// ============================================================
// Sections
//   1: Datablocks
//   2: Commands
//   3: Class Functions
// ============================================================

// ============================================================
// Section 1 - Datablocks
// ============================================================
datablock fxDTSBrickData(brickCMATMData) {
	brickFile = $CM::Config::Path::Mod @ "res/bricks/ATM/ATM.blb";

	isCityModBrick = true;
	citymodBrick["Type"] = "PROP";
	citymodBrick["Restriction"] = "NONE";

	iconName = $CM::Config::Path::Mod @ "res/bricks/ATM/ATM.png";
	uiName = "ATM";
};

// ============================================================
// Section 2 - Commands
// ============================================================
function servercmdCM_Bank_requestCredentials(%client) {
	%account = CM_Players.getData(%client.bl_id).account;
	%pin = CM_Bank.getData(CM_Bank.resolveAccountNumber(%account)).pin;
	commandtoclient(%client, 'CM_Bank_setCredentials', %account, %pin);
}

function servercmdCM_Bank_requestLogin(%client, %account, %pin) {
	if(CM_Bank.verifyLogin(%account, %pin)) {
		commandtoclient(%client, 'CM_Bank_loginSuccessful', %account, %pin, %client.bl_id, CM_Players.getData(%client.bl_id).name);
	} else {
		commandtoclient(%client, 'CM_Bank_loginFailed');
	}
}

function servercmdCM_Bank_requestBalance(%client, %account, %pin) {
	if(!CM_Bank.verifyLogin(%account, %pin)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_B_rB(1)", "INVALID_LOGIN");
		return;
	}

	commandtoclient(%client, 'CM_Bank_setBalance', CM_Bank.getData(CM_Bank.resolveAccountNumber(%account)).balance);
}

function servercmdCM_Bank_requestLedger(%client, %account, %pin) {
	if(!CM_Bank.verifyLogin(%account, %pin)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_B_rL(1)", "INVALID_LOGIN");
		return;
	}

	%ledger = CM_Bank.getData(CM_Bank.resolveAccountNumber(%account)).ledger;
	for(%i = 0; %i < %ledger.length; %i++) {
		%ledgeritem = %ledger.value[%i];
		commandtoclient(%client, 'CM_Bank_addLedgerRecord', getField(%ledgeritem, 0), getField(%ledgeritem, 2), getField(%ledgeritem, 1));
	}
}

function servercmdCM_Bank_depositAmount(%client, %account, %pin, %amount) {
	if(!CM_Bank.verifyLogin(%account, %pin)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_B_dA(1)", "INVALID_LOGIN");
		return;
	}

	%inventory = CM_Players.getData(%client.bl_id).inventory;

	for(%y = 0; %y < %inventory.size["Y"]; %y++) {
		for(%x = 0; %x < %inventory.size["X"]; %x++) {
			%slot = %this.getSlot(%x, %y);

			if(%slot $= "") {
				return %x SPC %y SPC false;
			}
		}
	}
}

// ============================================================
// Section 3 - Class Functions
// ============================================================
function CM_Bank::registerAccount(%this, %type, %owner) {
	if(!strLen(%owner)) {
		warn("CM_Bank::createOrganization() ==> Invalid \"%owner\" given -- cannot be blank");
		return "ERROR INVALID_OWNER";
	}

	if(%type $= "player") {
		if(!CM_Players.dataExists(%owner)) {
			warn("CM_Bank::createOrganization() ==> A player by the ID of \"" @ %owner @ "\" does not exist");
			return "ERROR OWNER_DOES_NOT_EXIST";
		}
	} else if(%type $= "organization") {
		if(!CM_Organizations.dataExists(%owner)) {
			warn("CM_Bank::createOrganization() ==> An organization by the ID of \"" @ %owner @ "\" does not exist");
			return "ERROR OWNER_DOES_NOT_EXIST";
		}
	} else {
		warn("CM_Bank::createOrganization() ==> Invalid \"%type\" given -- must be either \"player\" or \"organization\"");
		return "ERROR INVALID_TYPE";
	}

	%id = 1;
	while(isFile(%this.getDataPath(%id))) {
		%id++;
	}

	%account = %this.addData(%id);
	%account.type = %type;
	%account.owner = %owner;
	%account.number = %this.generateAccountNumber(%type, %id);
	%account.pin = %this.generatePIN();

	return %account.number;
}

function CM_Bank::generateAccountNumber(%this, %type, %id) {
	if(!strLen(%type) || ((%type !$= "player") && (%type !$= "organization"))) {
		warn("CM_Bank::generateAccountNumber() ==> Invalid \"%type\" given -- must be either \"player\" or \"organization\"");
		return "ERROR INVALID_ID";
	}

	if(!strLen(%id) || !isNumber(%id)) {
		warn("CM_Bank::generateAccountNumber() ==> Invalid \"%id\" given -- must be an integer");
		return "ERROR INVALID_ID";
	}

	%number = (%type $= "player" ? 1 : 2) @ "-" @ pad(%id, 4) @ "-";
	setRandomSeed(getRandomSeed());

	for(%i = 1; %i <= 4; %i++) {
		%number = %number @ getRandom(0, 9);
	}

	%number = %number @ "-";

	for(%i = 1; %i <= 2; %i++) {
		%number = %number @ getRandom(0, 9);
	}

	return %number;
}

function CM_Bank::generatePIN(%this) {
	setRandomSeed(getRandomSeed());

	for(%i = 1; %i <= 6; %i++) {
		%number = %number @ getRandom(0, 9);
	}

	return %number;
}

function CM_Bank::resolveAccountNumber(%this, %number) {
	if(!strLen(%number)) {
		return -1;
	}

	for(%i = 0; %i < %this.dataTable.keys.length; %i++) {
		if(%this.dataTable.get(%account = %this.dataTable.keys.value[%i]).number $= %number) {
			return %account;
		}
	}

	return -1;
}

function CM_Bank::verifyLogin(%this, %account, %pin) {
	%account = %this.resolveAccountNumber(%account);

	if(%account != -1) {
		%account = %this.getData(%account);

		if(%account.pin == %pin) {
			return true;
		}
	}

	return false;
}

function CityModBankAccount::addLedgerItem(%this, %amount, %message) {
	%this.ledger.push(CM_Tick.getShortDate() TAB %amount TAB %message);
}

function CityModBankAccount::addFunds(%this, %amount, %message) {
	if(!strLen(%amount) || !isNumber(%number)) {
		warn("CityModBankAccount::addFunds() ==> Invalid \"%amount\" given -- cannot be blank");
		return "ERROR INVALID_AMOUNT";
	}

	%amount = mAbs(%amount);

	if(!strLen(%message)) {
		%message = "Generic Transaction";
	}

	%this.balance += %amount;
	%this.addLedgerItem(%amount, %message);
}

function CityModBankAccount::removeFunds(%this, %amount, %message) {
	if(!strLen(%amount) || !isNumber(%number)) {
		warn("CityModBankAccount::removeFunds() ==> Invalid \"%amount\" given -- cannot be blank");
		return "ERROR INVALID_AMOUNT";
	}

	if(%this.balance <= 0) {
		warn("CityModBankAccount::removeFunds() ==> Cannot remove funds from an account that does not have any");
		return "ERROR BANKRUPT";
	}

	%amount = mAbs(%amount);

	if(!strLen(%message)) {
		%message = "Generic Transaction";
	}

	%this.balance -= %amount;
	%this.addLedgerItem(-%amount, %message);
}

function CityModBankAccount::transferFunds(%this, %recipient, %amount, %message) {
	if(!strLen(%amount) || !isNumber(%number)) {
		warn("CityModBankAccount::transferFunds() ==> Invalid \"%amount\" given -- cannot be blank");
		return "ERROR INVALID_AMOUNT";
	}

	if((%recipientAccount = CM_Bank.resolveAccountNumber(%recipient)) $= "") {
		warn("CityModBankAccount::transferFunds() ==> Recipient account does not exist");
		return "ERROR INVALID_RECIPIENT";
	}

	%amount = mAbs(%amount);

	if(%amount > %this.balance) {
		warn("CityModBankAccount::transferFunds() ==> Source account does not have the sufficient funds");
		return "ERROR INSUFFICIENT_FUNDS";
	}

	if(!strLen(%message)) {
		%message = "Funds Transfer";
	}

	%this.removeFunds(%amount, "Funds Transfer (" @ %recipient @ ")");
	CM_Bank.getData(%recipientAccount).addFunds(%amount, "Funds Transfer (" @ %this.number @ ")");
}

function CityModPlayer::getBankAccount(%this) {
	return CM_Bank.getData(CM_Bank.resolveAccountNumber(%this.account));
}