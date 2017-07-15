// ============================================================
// Project          -      CityMod
// Description      -      Player Inventory Commands
// ============================================================

function servercmdCM_RealEstate_requestProperties(%client) {
	for(%i = 0; %i < CM_RealEstate.dataTable.keys.length; %i++) {
		%property = CM_RealEstate.getData(%id = CM_RealEstate.dataTable.keys.value[%i]);

		if(!%property.selling) {
			continue;
		}

		commandtoclient(%client, 'CM_RealEstate_addProperty', %id, %property.type, %property.name, %property.size, %property.price, %property.description, %property.owner, 0);
		// TO-DO: Get the property brick count
	}
}

function servercmdCM_RealEstate_requestPlayerProperties(%client) {
	for(%i = 0; %i < CM_RealEstate.dataTable.keys.length; %i++) {
		%property = CM_RealEstate.getData(%id = CM_RealEstate.dataTable.keys.value[%i]);

		if(%property.owner != %client.bl_id) {
			continue;
		}

		commandtoclient(%client, 'CM_RealEstate_addPlayerProperty', %id, %property.type, %property.name, %property.size, %property.selling, %property.price, %property.description, 0);
		// TO-DO: Get the property brick count
	}
}

function servercmdCM_RealEstate_purchaseProperty(%client, %id) {
	if(!strLen(%id) || !CM_RealEstate.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_RE_pP(1)", "INVALID_ID");
		return;
	}

	%property = CM_RealEstate.getData(%id);

	if(!%property.selling) {
		commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "This property is currently not for sale!");
		return;
	}

	%return = %property.sellProperty(%client.bl_id);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "INVALID_BUYER":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "Invalid buyer BLID given");
				return;
			case "NONEXISTENT_BLID":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The buyer does not exist");
				return;
			case "NO_BUYER_BANK_ACCT":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You must have a bank account in order to purchase this property!");
				return;
			case "NO_OWNER_BANK_ACCT":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "This property is currently unpurchasable because its owner does not have a bank account");
				return;
			case "INSUFFICIENT_FUNDS":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You do not have enough funds in your bank account to purchase this property!");
				return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_RE_pP(2)", getWord(%return, 1)); return;
		}
	}

	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You have successfully purchased the property," SPC %property.name SPC "(#" @ %id @ ")!");
	commandtoclient(%client, 'CM_RealEstate_closePropertyInfo');
	commandtoclient(%client, 'CM_RealEstate_openPlayerProperties');
}

function servercmdCM_RealEstate_transferProperty(%client, %id, %recipient, %type) {
	if(!strLen(%id) || !CM_RealEstate.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_RE_tP(1)", "INVALID_ID");
		return;
	}

	%property = CM_RealEstate.getData(%id);

	if(%property.owner != %client.bl_id) {
		commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You are not the owner of this property!");
		return;
	}

	%return = %property.transferProperty(%recipient, %type);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "INVALID_TYPE":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "Invalid recipient type given, must be either \"player\" or \"organization\"");
				return;
			case "INVALID_RECIPIENT":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "Invalid recipient BLID given");
				return;
			case "NONEXISTENT_ID":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The recipient does not exist");
				return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_RE_tP(2)", getWord(%return, 1)); return;
		}
	}

	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You have successfully transfered the property," SPC %property.name SPC "(#" @ %id @ "), to" SPC CM_Players.getData(%recipient).name);

	commandtoclient(%client, 'CM_RealEstate_closeTransfer');
	commandtoclient(%client, 'CM_RealEstate_closePlayerProperties');
}

function servercmdCM_RealEstate_setPropertyListingStatus(%client, %id, %value) {
	if(!strLen(%id) || !CM_RealEstate.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_RE_sPLS(1)", "INVALID_ID");
		return;
	}

	%property = CM_RealEstate.getData(%id);

	if(%property.owner != %client.bl_id) {
		commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You are not the owner of this property!");
		return;
	}

	if((%value != true) && (%value != false)) {
		return;
	}

	%property.selling = %value;
}

function servercmdCM_RealEstate_updatePropertyInfo(%client, %id, %name, %description, %price) {
	if(!strLen(%id) || !CM_RealEstate.dataExists(%id)) {
		commandtoclient(%client, 'CM_errorMessage', "CM_RE_uPI(1)", "INVALID_ID");
		return;
	}

	%property = CM_RealEstate.getData(%id);

	if(%property.owner != %client.bl_id) {
		commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "You are not the owner of this property!");
		return;
	}

	%return = %property.updateProperty(%this, %name, %description, %price);

	if(firstWord(%return) $= "ERROR") {
		switch$(getWord(%return, 1)) {
			case "INVALID_NAME":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The name of a property cannot be blank!");
				return;
			case "INVALID_NAME_LENGTH":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The name given is too long!");
				return;
			case "INVALID_DESC":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The description of a property cannot be blank!");
				return;
			case "INVALID_DESC_LENGTH":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The description given is too long!");
				return;
			case "INVALID_PRICE":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The price of a property cannot be blank or a non-integer!");
				return;
			case "INVALID_PRICE_AMT":
				commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The price given is out of the valid range!");
				return;
			default: commandtoclient(%client, 'CM_errorMessage', "CM_RE_uPI(2)", getWord(%return, 1)); return;
		}
	}

	commandtoclient(%client, 'CM_Notification_pushDialog', "OK", "The property," SPC %name SPC "(#" @ %id @ "), has been successfully updated.");
}