// ============================================================
// Project          -      CityMod
// Description      -      Real Estate Property Class
// ============================================================

function CityModREProperty::linkPropertyBrick(%this, %brickID) {
	if(!strLen(%brickID)) {
		CMError(2, "CityModREProperty::linkProperty()", "Invalid \"%brickID\" given -- cannot be blank");
		return "ERROR INVALID_ID";
	}

	if(!isObject(%brickID)) {
		CMError(2, "CityModREProperty::linkProperty()", "A brick by the ID of \"" @ %brickID @ "\" does not exist");
		return "ERROR NONEXISTENT_ID";
	}

	%this.brick = %brickID;
}

function CityModREProperty::updateProperty(%this, %name, %description, %price) {
	if(!strLen(%name)) {
		CMError(2, "CityModREProperty::updateProperty() ==> Invalid \"%name\" given -- cannot be blank");
		return "ERROR INVALID_NAME";
	}

	if(strLen(%name) > $CM::Config::Properties::MaxNameLength) {
		CMError(2, "CityModREProperty::updateProperty() ==> Invalid \"%name\" given -- too long (" @ strLen(%name) SPC ">" SPC $CM::Config::Properties::MaxNameLength @ ")");
		return "ERROR INVALID_NAME_LENGTH";
	}

	if(!strLen(%description)) {
		CMError(2, "CityModREProperty::updateProperty() ==> Invalid \"%description\" given -- cannot be blank");
		return "ERROR INVALID_DESC";
	}

	if(strLen(%description) > $CM::Config::Properties::MaxDescLength) {
		CMError(2, "CityModREProperty::updateProperty() ==> Invalid \"%description\" given -- too long (" @ strLen(%description) SPC ">" SPC $CM::Config::Properties::MaxDescLength @ ")");
		return "ERROR INVALID_DESC_LENGTH";
	}

	if(!strLen(%price) || !isNumber(%price)) {
		CMError(2, "CityModREProperty::updateProperty() ==> Invalid \"%price\" given -- cannot be blank or a non-integer");
		return "ERROR INVALID_PRICE";
	}

	if((%price < 0) || (%price > $CM::Config::Properties::MaxPrice)) {
		CMError(2, "CityModREProperty::updateProperty() ==> Invalid \"%price\" given -- out of bounds (0 < " @ %price @ " < " @ $CM::Config::Properties::MaxPrice @ ")");
		return "ERROR INVALID_PRICE_AMT";
	}

	%this.name = %name;
	%this.description = %description;
	%this.price = %price;
}

function CityModREProperty::sellProperty(%this, %buyer) {
	if(!strLen(%buyer)) {
		CMError(2, "CityModREProperty::sellProperty()", "Invalid \"%buyer\" given -- cannot be blank");
		return "ERROR INVALID_BUYER";
	}

	if(!CM_Players.dataExists(%buyer)) {
		CMError(2, "CityModREProperty::sellProperty()", "A player by the BL_ID of \"" @ %buyer @ "\" does not exist");
		return "ERROR NONEXISTENT_BLID";
	}

	%buyerAcct = CM_Bank.resolveAccountNumber(CM_Players.getData(%buyer).account);

	if(%buyerAcct == -1) {
		CMError(2, "CityModREProperty::sellProperty()", "The buyer by the BL_ID of \"" @ %buyer @ "\" does not have a bank account");
		return "ERROR NO_BUYER_BANK_ACCT";
	}

	%return = %buyerAcct.transferFunds(CM_Players.getData(%this.owner).account, %this.price);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "INVALID_RECIPIENT":
				CMError(2, "CityModREProperty::sellProperty()", "The owner of the property by the ID of \"" @ %this.dataID @ "\" does not have a bank account");
				return "NO_OWNER_BANK_ACCT";
			case "INSUFFICIENT_FUNDS":
				CMError(2, "CityModREProperty::sellProperty()", "The buyer does not have sufficient funds in their bank account");
				return "INSUFFICIENT_FUNDS";
		}
	}

	%this.owner = %buyer;
}

function CityModREProperty::transferProperty(%this, %recipient, %type) {
	if(!strLen(%recipient)) {
		CMError(2, "CityModREProperty::transferProperty()", "Invalid \"%recipient\" given -- cannot be blank");
		return "ERROR INVALID_RECIPIENT";
	}

	if(%type $= "player") {
		if(!CM_Players.dataExists(%recipient)) {
			CMError(2, "CityModREProperty::transferProperty()", "A player by the BL_ID of \"" @ %recipient @ "\" does not exist");
			return "ERROR NONEXISTENT_ID";
		}
	} else if(%type $= "organization") {
		if(!CM_Organizations.dataExists(%recipient)) {
			CMError(2, "CityModREProperty::transferProperty()", "An organization by the ID of \"" @ %recipient @ "\" does not exist");
			return "ERROR NONEXISTENT_ID";
		}
	} else {
		CMError(2, "CityModREProperty::transferProperty()", "The recipient type of \"" @ %type @ "\" is invalid");
		return "ERROR INVALID_TYPE";
	}

	%this.proprietorship = %type;
	%this.owner = %recipient;
}