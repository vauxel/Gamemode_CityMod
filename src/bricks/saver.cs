// ============================================================
// Project          -      CityMod
// Description      -      Brick Saver
// ============================================================
// Sections
//   1: Package
// ============================================================

// ============================================================
// Section 1 - Package
// ============================================================

package CityMod_Bricks_Saver {
	function CM_BricksSaver::onAdd(%this) {
		if(!strLen(%this.path)) {
			CMError(1, "CM_BricksSaver::onAdd", "No file path given");
			%this.schedule(0, "delete");
			return "ERROR";
		}

		if((getSubStr(%this.path, strLen(%this.path) - 1, 1) !$= "/") && (getSubStr(%this.path, strLen(%this.path) - 1, 1) !$= "\\")) {
			%this.path = %this.path @ "/";
		}

		if(!strLen(%this.fileExtension)) {
			CMError(1, "CM_BricksSaver::onAdd", "No file extension given");
			%this.schedule(0, "delete");
			return "ERROR";
		}

		%this.savedFields = Array();
	}

	function CM_BricksSaver::onRemove(%this) {
		%this.savedFields.delete();
	}

	function CM_BricksSaver::addSavedField(%this, %field) {
		if(!strLen(%field)) {
			return;
		}

		if(%this.savedFields.contains(%field)) {
			return;
		}

		%this.savedFields.push(%field);
	}

	function CM_BricksSaver::removeSavedField(%this, %field) {
		if(!strLen(%field)) {
			return;
		}

		if(!%this.savedFields.contains(%field)) {
			return;
		}

		%this.savedFields.remove(%field);
	}

	function CM_BricksSaver::save(%this) {
		if(%this.isBusy == true) {
			CMError(1, "CM_BricksSaver::save", "CM_BricksSaver is currently busy");
			return;
		}

		if($Game::MissionCleaningUp) {
			CMError(1, "CM_BricksSaver::save", "Mission Cleanup is currently busy");
			return;
		}

		%this.isBusy = true;
		%startTime = getRealTime();

		%brickCount = 0;

		%file = new FileObject();
		%file.openForWrite(%this.path @ getRealTime() @ "." @ %this.fileExtension);
		%file.writeLine("This is a CityMod Blockland save file.  Do whatever you want with it.");

		for(%i = 0; %i < mainBrickGroup.getCount(); %i++) {
			%group = mainBrickGroup.getObject(%i);

			for(%j = 0; %j < %group.getCount(); %j++) {
				%brick = %group.getObject(%j);
				%brickCount++;

				// Defaults
				%file.writeLine(
					%brick.getDataBlock().getName() TAB
					%brick.getPosition() TAB
					%brick.getAngleID() SPC %brick.isBasePlate() TAB
					%brick.getColorID() SPC %brick.getColorFXID() SPC %brick.getShapeFXID() TAB
					%brick.isRayCasting() SPC %brick.isColliding() SPC %brick.isRendering()
				);

				// Owner
				if(((%owner = getBrickGroupFromObject(%brick).bl_id) != 888888) && !$Server::LAN) {
					%file.writeLine("+" SPC "OWNER" TAB %owner);
				}

				if(strLen(%brick.getName())) {
					%file.writeLine("+" SPC "NAME" TAB %brick.getName());
				}

				// Print
				if(%brick.getDataBlock().hasPrint) {
					%printTexture = getPrintTexture(%brick.getPrintId());

					if(strLen(%printTexture)) {
						%printPath = filePath(%printTexture);
						%printName = getSubStr(%printPath, strPos(%printPath, "_") + 1, strPos(%printPath, "_", 14) - 14) @ "/" @ fileBase(%printTexture);

						if(strLen($printNameTable[%printName])) {
							%file.writeLine("+" SPC "PRINT" TAB %printName);
						}
					} else {
						%file.writeLine("+" SPC "PRINT" TAB "Letters/-space");
					}
				}

				// Emitter
				if(isObject(%brick.emitter)) {
					%file.writeLine("+" SPC "EMITTER" TAB %brick.emitter.emitter.uiName TAB %brick.emitterDirection);
				}

				// Audio Emitter
				if(isObject(%brick.audioEmitter)) {
					%file.writeLine("+" SPC "AUDIOEMITTER" TAB %brick.audioEmitter.getProfileID().uiName);
				}

				// Light
				if(%brick.getLightID() >= 0) {
					%file.writeLine("+" SPC "LIGHT" TAB %brick.getLightID().getDataBlock().uiName);
				}

				// Events
				for(%k = 0; %k < %brick.numEvents; %k++) {
					%targetClass = %brick.eventTargetIdx[%k] >= 0 ? getWord(getField($InputEvent_TargetListfxDTSBrick_[%brick.eventInputIdx[%k]], %brick.eventTargetIdx[%k]), 1) : "fxDtsBrick";
					%paramList = $OutputEvent_parameterList[%targetClass, %brick.eventOutputIdx[%k]];
					%params = "";

					for(%l = 0; %l < 4; %l++) {
						if(firstWord(getField(%paramList, %l)) $= "dataBlock" && isObject(%brick.eventOutputParameter[%k, %l + 1]))
							%params = %params TAB %brick.eventOutputParameter[%k, %l + 1];
						else
							%params = %params TAB %brick.eventOutputParameter[%k, %l + 1];
					}

					%file.writeLine("+" SPC "EVENT" TAB %k TAB
						%brick.eventEnabled[%k] TAB
						%brick.eventInput[%k] TAB
						%brick.eventDelay[%k] TAB
						%brick.eventTarget[%k] TAB
						%brick.eventNT[%k] TAB
						%brick.eventOutput[%k] @ %params
					);
				}

				// Saved Fields
				for(%m = 0; %m < %this.savedFields.length; %m++) {
					%savedFieldContents = "";
					eval("%savedFieldContents = " @ %brick @ "." @ %this.savedFields.value[%m] @ ";");

					if(%savedFieldContents !$= "") {
						%file.writeLine("-" SPC strUpr(%this.savedFields.value[%m]) TAB Stringify::serialize(%savedFieldContents));
					}
				}
			}
		}

		%endTime = getRealTime();

		%file.close();
		%file.delete();

		%this.isBusy = false;
		CMInfo(1, "CM_BricksSaver", "Saved" SPC %brickCount SPC "Bricks in" SPC ((%endTime - %startTime) / 1000) SPC "seconds");
	}

	function CM_BricksSaver::load(%this, %timestamp) {
		if(!strLen(%timestamp)) {
			CMError(1, "CM_BricksSaver::load", "Invalid \"%timestamp\" given -- cannot be blank");
			return;
		}

		if(%this.isBusy == true) {
			CMError(1, "CM_BricksSaver::load", "CM_BricksSaver is currently busy");
			return;
		}

		if(!isFile(%this.path @ %timestamp @ "." @ %this.fileExtension)) {
			CMError(1, "CM_BricksSaver::load", "A save for this timestamp does not exist");
			return;
		}

		%this.isBusy = true;
		%startTime = getRealTime();

		%file = new FileObject();
		%file.openForRead(%this.path @ %timestamp @ "." @ %this.fileExtension);

		if(%file.readLine() !$= "This is a CityMod Blockland save file.  Do whatever you want with it.") {
			CMError(1, "CM_BricksSaver::load", "This save file is invalid");
			%file.close();
			%file.delete();
			return;
		}

		%lastCreatedBrick = "";
		%brickCount = 0;

		while(!%file.isEOF()) {
			%line = %file.readLine();

			if((firstWord(%line) $= "+") || (firstWord(%line) $= "-")) {
				if(isObject(%lastCreatedBrick)) {
					%this.parseLoadLine(%line, %lastCreatedBrick);
				}
			} else {
				%datablock = getField(%line, 0);
				%position = getField(%line, 1);

				%angleID = getWord(getField(%line, 2), 0);
				%isBaseplate = getWord(getField(%line, 2), 1);

				%colorID = getWord(getField(%line, 3), 0);
				%colorFxID = getWord(getField(%line, 3), 1);
				%shapeFxID = getWord(getField(%line, 3), 2);

				%isRayCasting = getWord(getField(%line, 4), 0);
				%isColliding = getWord(getField(%line, 4), 1);
				%isRendering = getWord(getField(%line, 4), 2);

				if(!strLen(%datablock) || !isObject(%datablock) || !strLen(%position) || !strLen(%angleID) || !strLen(%isBaseplate) || !strLen(%colorID) ||
					!strLen(%colorFxID) || !strLen(%shapeFxID) || !strLen(%isRayCasting) || !strLen(%isColliding) || !strLen(%isRendering)) {
					continue;
				}

				if(%angleID == 0) {
					%rotation = "1 0 0 0";
				} else if(%angleID == 1) {
					%rotation = "0 0 1" SPC $piOver2;
				} else if(%angleID == 2) {
					%rotation = "0 0 1" SPC $pi;
				} else if(%angleID == 3) {
					%rotation = "0 0 -1" SPC $piOver2;
				}

				%lastCreatedBrick = new fxDTSBrick() {
					datablock = %datablock;
					angleID = %angleID;
					isBaseplate = %isBaseplate;
					colorID = %colorID;
					colorFxID = %colorFxID;
					shapeFxID = %shapeFxID;
					stackBL_ID = 888888;
				};

				%lastCreatedBrick.setTransform(%position @ "  " @ %rotation);
				%lastCreatedBrick.trustCheckFinished();
				%plantError = %lastCreatedBrick.plant();

				if((%plantError != 0) && (%plantError != 2)) {
					%lastCreatedBrick.delete();
					%lastCreatedBrick = "";
				}

				if(isObject(%lastCreatedBrick)) {
					BrickGroup_888888.add(%lastCreatedBrick);
					%brickCount++;
				}
			}
		}

		%endTime = getRealTime();

		%file.close();
		%file.delete();

		%this.isBusy = false;
		CMInfo(1, "CM_BricksSaver", "Loaded" SPC %brickCount SPC "Bricks in" SPC ((%endTime - %startTime) / 1000) SPC "seconds");
	}

	function CM_BricksSaver::parseLoadLine(%this, %line, %brick) {
		if(%this.isBusy != true) {
			CMError(2, "CM_BricksSaver::parseLoadLine", "CM_BricksSaver is currently not busy");
			return;
		}

		if(!strLen(%line)) {
			CMError(1, "CM_BricksSaver::parseLoadLine", "Invalid \"%line\" given -- cannot be blank");
			return;
		}

		if(!strLen(%brick) || !isObject(%brick)) {
			CMError(1, "CM_BricksSaver::parseLoadLine", "Invalid \"%brick\" given -- cannot be blank or nonexistant");
			return;
		}

		if($Game::MissionCleaningUp) {
			CMError(1, "CM_BricksSaver::parseLoadLine", "Mission Cleanup is currently busy");
			return;
		}

		if(firstWord(%line) $= "+") { // Normal Field
			%type = getWord(getField(%line, 0), 1);
			%value = getField(%line, 1);

			if(!strLen(%type)) {
				CMError(1, "CM_BricksSaver::parseLoadLine", "\"Type Word\" could not be found");
				return;
			}

			if(!strLen(%value)) {
				CMError(1, "CM_BricksSaver::parseLoadLine", "\"Value Word\" could not be found");
				return;
			}

			switch$(%type) {
				case "OWNER":
					%ownerBrickGroup = "BrickGroup_" @ %value;

					if(isObject(%ownerBrickGroup)) {
						%ownerBrickGroup = %ownerBrickGroup.getID();
					} else {
						%ownerBrickGroup = new SimGroup("BrickGroup_" @ %value);
						%ownerBrickGroup.bl_id = %value;
						%ownerBrickGroup.client = isObject(%client = findClientByBL_ID(%value)) ? %client : "";
						%ownerBrickGroup.name = isObject(%client) ? %client.name : "\c1BL_ID:" SPC %value @ "\c1\c0";
						mainBrickGroup.add(%ownerBrickGroup);
					}

					%brick.stackBL_ID = %value;
					BrickGroup_888888.remove(%brick);
					%ownerBrickGroup.add(%brick);
				case "NAME":
					%brick.setNTObjectName(%value);
				case "PRINT":
					%printID = $printNameTable[%value];

					if(!strLen(%printID)) {
						%printID = $printNameTable["Letters/-space"];
					}

					%brick.setPrint(%printID);
			}
		} else if(firstWord(%line) $= "-") { // Special Field
			%type = getWord(getField(%line, 0), 1);
			%value = Stringify::parse(getField(%line, 1));

			if(%this.savedFields.contains(%type)) {
				eval(%brick @ "." @ strLwr(%type) SPC "=" SPC %value @ ";");
			}
		}
	}
};

if(isPackage(CityMod_Bricks_Saver))
	deactivatePackage(CityMod_Bricks_Saver);
activatePackage(CityMod_Bricks_Saver);