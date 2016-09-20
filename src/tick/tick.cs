// ============================================================
// Project          -      CityMod
// Description      -      Core Tick Code
// ============================================================

function CM_Tick::onAdd(%this) {
	if(!strLen(%this.tickSpeed)) {
		CMError(0, "CM_Tick::onAdd", "Tick speed not given");
		%this.schedule(0, "delete");
		return "ERROR";
	}

	%this.saveSet = new SimSet();
	%this.tickCount = 0;

	%this.doTick();
}

function CM_Tick::doTick(%this) {
	if(isEventPending(%this.tick)) {
		cancel(%this.tick);
	}

	%this.onTick();
	%this.tick = %this.schedule(%this.tickSpeed, "doTick");
}

function CM_Tick::onTick(%this) { %this.tickCount++; }