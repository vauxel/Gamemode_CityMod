// ============================================================
// Project          -      CityMod
// Description      -      Global Support Functions
// ============================================================

function SimObject::hasMethod(%this, %name) {
	return isFunction(%this.getName(), %name) || isFunction(%this.class, %name) || isFunction(%this.getClassName(), %name);
}

function SimObject::call(%this, %method, %p1, %p2, %p3, %p4, %p5, %p6, %p7, %p8, %p9, %p10, %p11, %p12, %p13, %p14, %p15, %p16) {
	%lastValue = 0;

	for(%i = 1; %i <= 16; %i++) {
		if(strLen(%p[%i])) {
			%lastValue = %i;
		}
	}

	for(%i = 1; %i <= %lastValue; %i++) {
		%args = %args @ "\"" @ expandEscape(%p[%i]) @ "\"";

		if(%i != %lastValue) {
			%args = %args @ ", ";
		} else {
			break;
		}
	}

	eval(%this @ "." @ %method @ "(" @ %args @ ");");
}

function SimObject::copy(%this) {
	%name = %this.getName(); %this.setName("TempCopyName");
	%copy = (new (%this.getClassName())(%name : TempCopyName) @ "\x01");
	%this.setName(%name);
	return %copy;
}

// Credits to Port for the get/setAttribute functions
function SimObject::getAttribute(%this, %attr) {
	if(%attr $= "") {
		return "";
	}

	switch(stripos("_abcdefghijklmnopqrstuvwxyz", getSubStr(%attr, 0, 1))) {
		case  0: return %this._[getSubStr(%attr, 1, strlen(%attr))];
		case  1: return %this.a[getSubStr(%attr, 1, strlen(%attr))];
		case  2: return %this.b[getSubStr(%attr, 1, strlen(%attr))];
		case  3: return %this.c[getSubStr(%attr, 1, strlen(%attr))];
		case  4: return %this.d[getSubStr(%attr, 1, strlen(%attr))];
		case  5: return %this.e[getSubStr(%attr, 1, strlen(%attr))];
		case  6: return %this.f[getSubStr(%attr, 1, strlen(%attr))];
		case  7: return %this.g[getSubStr(%attr, 1, strlen(%attr))];
		case  8: return %this.h[getSubStr(%attr, 1, strlen(%attr))];
		case  9: return %this.i[getSubStr(%attr, 1, strlen(%attr))];
		case 10: return %this.j[getSubStr(%attr, 1, strlen(%attr))];
		case 11: return %this.k[getSubStr(%attr, 1, strlen(%attr))];
		case 12: return %this.l[getSubStr(%attr, 1, strlen(%attr))];
		case 13: return %this.m[getSubStr(%attr, 1, strlen(%attr))];
		case 14: return %this.n[getSubStr(%attr, 1, strlen(%attr))];
		case 15: return %this.o[getSubStr(%attr, 1, strlen(%attr))];
		case 16: return %this.p[getSubStr(%attr, 1, strlen(%attr))];
		case 17: return %this.q[getSubStr(%attr, 1, strlen(%attr))];
		case 18: return %this.r[getSubStr(%attr, 1, strlen(%attr))];
		case 19: return %this.s[getSubStr(%attr, 1, strlen(%attr))];
		case 20: return %this.t[getSubStr(%attr, 1, strlen(%attr))];
		case 21: return %this.u[getSubStr(%attr, 1, strlen(%attr))];
		case 22: return %this.v[getSubStr(%attr, 1, strlen(%attr))];
		case 23: return %this.w[getSubStr(%attr, 1, strlen(%attr))];
		case 24: return %this.x[getSubStr(%attr, 1, strlen(%attr))];
		case 25: return %this.y[getSubStr(%attr, 1, strlen(%attr))];
		case 26: return %this.z[getSubStr(%attr, 1, strlen(%attr))];
	}

	return "";
}

function SimObject::setAttribute(%this, %attr, %value) {
	if(%attr $= "") {
		return;
	}

	switch(stripos("_abcdefghijklmnopqrstuvwxyz", getSubStr(%attr, 0, 1))) {
		case  0: %this._[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  1: %this.a[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  2: %this.b[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  3: %this.c[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  4: %this.d[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  5: %this.e[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  6: %this.f[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  7: %this.g[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  8: %this.h[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  9: %this.i[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 10: %this.j[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 11: %this.k[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 12: %this.l[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 13: %this.m[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 14: %this.n[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 15: %this.o[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 16: %this.p[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 17: %this.q[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 18: %this.r[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 19: %this.s[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 20: %this.t[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 21: %this.u[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 22: %this.v[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 23: %this.w[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 24: %this.x[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 25: %this.y[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 26: %this.z[getSubStr(%attr, 1, strlen(%attr))] = %value;
	}
}

function SimObject::deleteAttribute(%this, %attr) {
	return %this.setAttribute(%attr, "");
}