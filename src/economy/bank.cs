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
	brickFile = $CM::Config::Path::Mod @ "res/bricks/atm/atm.blb";

	isCityModBrick = true;
	citymodBrick["Type"] = "PROP";
	citymodBrick["Restriction"] = "NONE";

	iconName = $CM::Config::Path::Mod @ "res/bricks/atm/atm.png";
	uiName = "ATM";
};

// ============================================================
// Section 2 - Commands
// ============================================================
function servercmdCM_Bank_requestCredentials(%client) {
	%account = CM_Players.getData(%client.bl_id).account;
	%pin = CM_Bank.resolveAccountNumber(%account).pin;
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

	commandtoclient(%client, 'CM_Bank_setBalance', CM_Bank.resolveAccountNumber(%account).balance);
}

function servercmdCM_Bank_requestLedger(%client, %account, %pin) {
	if(!CM_Bank.verifyLogin(%account, %pin)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_B_rL(1)", "INVALID_LOGIN");
		return;
	}

	%ledger = CM_Bank.resolveAccountNumber(%account).ledger;
	for(%i = 0; %i < %ledger.length; %i++) {
		%ledgeritem = %ledger.value[%i];
		commandtoclient(%client, 'CM_Bank_addLedgerRecord', getField(%ledgeritem, 0), getField(%ledgeritem, 2), getField(%ledgeritem, 1));
	}
}

function servercmdCM_Bank_depositAll(%client, %account, %pin) {
	if(!CM_Bank.verifyLogin(%account, %pin)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_B_dA(1)", "INVALID_LOGIN");
		return;
	}

	%inventory = CM_Players.getData(%client.bl_id).inventory;

	%total = 0;

	for(%y = 0; %y < %inventory.size["Y"]; %y++) {
		for(%x = 0; %x < %inventory.size["X"]; %x++) {
			%slot = %inventory.getSlot(%x, %y);

			if(%slot $= "") {
				continue;
			}

			%name = %slot.get("Name");

			if(%name $= CMHundredDollarItem.uiName) {
				%total += 100 * %slot.get("Count");
				%inventory.setSlot(%x, %y, "");
			} else if(%name $= CMFiftyDollarItem.uiName) {
				%total += 50 * %slot.get("Count");
				%inventory.setSlot(%x, %y, "");
			} else if(%name $= CMTwentyDollarItem.uiName) {
				%total += 20 * %slot.get("Count");
				%inventory.setSlot(%x, %y, "");
			} else if(%name $= CMTenDollarItem.uiName) {
				%total += 10 * %slot.get("Count");
				%inventory.setSlot(%x, %y, "");
			} else if(%name $= CMFiveDollarItem.uiName) {
				%total += 5 * %slot.get("Count");
				%inventory.setSlot(%x, %y, "");
			} else if(%name $= CMOneDollarItem.uiName) {
				%total += 1 * %slot.get("Count");
				%inventory.setSlot(%x, %y, "");
			}
		}
	}

	if(%total == 0) {
		commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You don't have any money to deposit");
		return;
	}

	%return = CM_Bank.resolveAccountNumber(%account).addFunds(%total, "Deposit");

	if(firstWord(%return) $= "ERROR") {
		commandtoclient(%client, 'CM_errorMessage', "CM_B_dA(2)", getWord(%return, 1));
		return;
	}

	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "Successfully deposited $" @ commaSeparateAmount(%total));
}

function servercmdCM_Bank_withdrawAmount(%client, %account, %pin, %amount) {
	if(!CM_Bank.verifyLogin(%account, %pin)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_B_wA(1)", "INVALID_LOGIN");
		return;
	}

	%return = CM_Bank.resolveAccountNumber(%account).removeFunds(%amount, "Withdrawal");

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "INVALID_AMOUNT":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must enter a valid amount to withdraw");
				return;
			case "INSUFFICIENT_FUNDS":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You do not have enough funds to withdraw the specified amount!");
				return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_B_wA(2)", getWord(%return, 1)); return;
		}
	}

	%totali = %amount;

	// Hundreds
	%hundreds = mFloor(%amount / 100);
	%amount -= %hundreds * 100;

	// Fifties
	%fifties = mFloor(%amount / 50);
	%amount -= %fifties * 50;

	// Twenties
	%twenties = mFloor(%amount / 20);
	%amount -= %twenties * 20;

	// Tens
	%tens = mFloor(%amount / 10);
	%amount -= %tens * 10;

	// Fives
	%fives = mFloor(%amount / 5);
	%amount -= %fives * 5;

	// Ones
	%ones = mFloor(%amount / 1);
	%amount -= %ones * 1;

	%totalf = (%hundreds * 100) + (%fifties * 50) + (%twenties * 20) + (%tens * 10) + (%fives * 5) + (%ones * 1);

	if(%totali != %totalf) {
		// Houston, we have a problem
		CMError(1, "servercmdCM_Bank_withdrawAmount() ==> Amount calculated does not equal amount desired (" @ %totalf SPC "!=" SPC %totali @ ")");
		return;
	}

	%inventory = CM_Players.getData(%client.bl_id).inventory;

	if(%hundreds != 0) {
		%inventory.addItem("ITEM", CMHundredDollarItem.uiName, CMHundredDollarItem, %hundreds);
	}

	if(%fifties != 0) {
		%inventory.addItem("ITEM", CMFiftyDollarItem.uiName, CMFiftyDollarItem, %fifties);
	}

	if(%twenties != 0) {
		%inventory.addItem("ITEM", CMTwentyDollarItem.uiName, CMTwentyDollarItem, %twenties);
	}

	if(%tens != 0) {
		%inventory.addItem("ITEM", CMTenDollarItem.uiName, CMTenDollarItem, %tens);
	}

	if(%fives != 0) {
		%inventory.addItem("ITEM", CMFiveDollarItem.uiName, CMFiveDollarItem, %fives);
	}

	if(%ones != 0) {
		%inventory.addItem("ITEM", CMOneDollarItem.uiName, CMOneDollarItem, %ones);
	}

	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "Successfully withdrew $" @ commaSeparateAmount(%totalf));
}

function servercmdCM_Bank_transferAmount(%client, %account, %pin, %amount, %recipient) {
	if(!CM_Bank.verifyLogin(%account, %pin)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_B_tA(1)", "INVALID_LOGIN");
		return;
	}

	%return = CM_Bank.resolveAccountNumber(%account).transferFunds(%recipient, %amount);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "INVALID_AMOUNT":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must enter a valid amount to transfer");
				return;
			case "INVALID_RECIPIENT":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "An account with the given account number does not exist");
				return;
			case "INSUFFICIENT_FUNDS":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You do not have enough funds to transfer the specified amount!");
				return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_B_tA(2)", getWord(%return, 1)); return;
		}
	}

	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "Successfully transferred $" @ commaSeparateAmount(%amount) SPC "to account #" @ %recipient);
}

// ============================================================
// Section 3 - Class Functions
// ============================================================
function CM_Bank::registerAccount(%this, %type, %owner) {
	if(!strLen(%owner)) {
		CMError(2, "CM_Bank::registerAccount() ==> Invalid \"%owner\" given -- cannot be blank");
		return "ERROR INVALID_OWNER";
	}

	if(%type $= "player") {
		if(!CM_Players.dataExists(%owner)) {
			CMError(2, "CM_Bank::registerAccount() ==> A player by the ID of \"" @ %owner @ "\" does not exist");
			return "ERROR OWNER_DOES_NOT_EXIST";
		}
	} else if(%type $= "organization") {
		if(!CM_Organizations.dataExists(%owner)) {
			CMError(2, "CM_Bank::registerAccount() ==> An organization by the ID of \"" @ %owner @ "\" does not exist");
			return "ERROR OWNER_DOES_NOT_EXIST";
		}
	} else {
		CMError(2, "CM_Bank::registerAccount() ==> Invalid \"%type\" given -- must be either \"player\" or \"organization\"");
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

function CM_Bank::closeAccount(%this) {

}

function CM_Bank::generateAccountNumber(%this, %type, %id) {
	if(!strLen(%type) || ((%type !$= "player") && (%type !$= "organization"))) {
		CMError(2, "CM_Bank::generateAccountNumber() ==> Invalid \"%type\" given -- must be either \"player\" or \"organization\"");
		return "ERROR INVALID_ID";
	}

	if(!strLen(%id) || !isNumber(%id)) {
		CMError(2, "CM_Bank::generateAccountNumber() ==> Invalid \"%id\" given -- must be an integer");
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
		if((%account = %this.dataTable.get(%this.dataTable.keys.value[%i])).number $= %number) {
			return %account;
		}
	}

	return -1;
}

function CM_Bank::verifyLogin(%this, %account, %pin) {
	%account = %this.resolveAccountNumber(%account);

	if(%account != -1) {
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
	if(!strLen(%amount) || !isNumber(%amount) || (%amount == 0)) {
		CMError(2, "CityModBankAccount::addFunds() ==> Invalid \"%amount\" given -- cannot be blank");
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
	if(!strLen(%amount) || !isNumber(%amount) || (%amount == 0)) {
		CMError(2, "CityModBankAccount::removeFunds() ==> Invalid \"%amount\" given -- cannot be blank");
		return "ERROR INVALID_AMOUNT";
	}

	%amount = mAbs(%amount);

	if(%amount > %this.balance) {
		CMError(2, "CityModBankAccount::removeFunds() ==> Account does not have the sufficient funds");
		return "ERROR INSUFFICIENT_FUNDS";
	}

	if(!strLen(%message)) {
		%message = "Generic Transaction";
	}

	%this.balance -= %amount;
	%this.addLedgerItem(-%amount, %message);
}

function CityModBankAccount::transferFunds(%this, %recipient, %amount, %message) {
	if(!strLen(%amount) || !isNumber(%amount) || (%amount == 0)) {
		CMError(2, "CityModBankAccount::transferFunds() ==> Invalid \"%amount\" given -- cannot be blank");
		return "ERROR INVALID_AMOUNT";
	}

	if((%recipientAccount = CM_Bank.resolveAccountNumber(%recipient)) == -1) {
		CMError(2, "CityModBankAccount::transferFunds() ==> Recipient account does not exist");
		return "ERROR INVALID_RECIPIENT";
	}

	%amount = mAbs(%amount);

	if(%amount > %this.balance) {
		CMError(2, "CityModBankAccount::transferFunds() ==> Source account does not have the sufficient funds");
		return "ERROR INSUFFICIENT_FUNDS";
	}

	if(!strLen(%message)) {
		%message = "Funds Transfer";
	}

	%this.removeFunds(%amount, "Transfer (" @ %recipient @ ")");
	%recipientAccount.addFunds(%amount, "Transfer (" @ %this.number @ ")");
}

function CityModPlayer::getBankAccount(%this) {
	return CM_Bank.resolveAccountNumber(%this.account);
}
