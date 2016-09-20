// ============================================================
// Project          -      CityMod
// Description      -      Tick Daycycle Code
// ============================================================

package CityMod_Tick_Daycycle {
	function CM_Tick::onAdd(%this) {
		parent::onAdd(%this);

		if(!strLen(%this.savePath)) {
			CMError(0, "CM_Tick::onAdd", "No file path given");
			%this.schedule(0, "delete");
			return "ERROR";
		}

		// Month Names
		%this.monthName[0] = "January";
		%this.monthName[1] = "February";
		%this.monthName[2] = "March";
		%this.monthName[3] = "April";
		%this.monthName[4] = "May";
		%this.monthName[5] = "June";
		%this.monthName[6] = "July";
		%this.monthName[7] = "August";
		%this.monthName[8] = "September";
		%this.monthName[9] = "October";
		%this.monthName[10] = "November";
		%this.monthName[11] = "December";

		// Month Days
		%this.monthDays[0] = 31;
		%this.monthDays[1] = 28;
		%this.monthDays[2] = 31;
		%this.monthDays[3] = 30;
		%this.monthDays[4] = 31;
		%this.monthDays[5] = 30;
		%this.monthDays[6] = 31;
		%this.monthDays[7] = 31;
		%this.monthDays[8] = 30;
		%this.monthDays[9] = 31;
		%this.monthDays[10] = 30;
		%this.monthDays[11] = 31;

		%this.minute = 0;
		%this.hour = 0;
		%this.day = 0;
		%this.month = 0;
		%this.year = 0;

		//%this.latitude = 42.773969;
		//%this.longitude = -76.182169;

		if(isFile(%this.savePath @ "time.dat")) {
			CM_Tick.loadTime();
		}
	}

	function CM_Tick::saveTime(%this) {
		%data = %this.minute TAB %this.hour TAB %this.day TAB %this.month TAB %this.year;

		%fo = new FileObject();
		%fo.openForWrite(%this.savePath @ "time.dat");
		%fo.writeLine(%data);
		%fo.close();
		%fo.delete();
	}

	function CM_Tick::loadTime(%this) {
		%fo = new FileObject();
		%fo.openForRead(%this.savePath @ "time.dat");
		%data = %fo.readLine();
		%fo.close();
		%fo.delete();

		%this.minute = getField(%data, 0);
		%this.hour = getField(%data, 1);
		%this.day = getField(%data, 2);
		%this.month = getField(%data, 3);
		%this.year = getField(%data, 4);
	}

	function CM_Tick::passTime(%this) {
		if(%this.minute >= 59) {
			%this.minute = 0;
			%this.hour++;

			%this.onHour();
		}
		else {
			%this.minute++;
		}

		if(%this.hour > 23) {
			%this.hour = 0;
			%this.day++;

			%this.onDay();
		}

		if((%this.day + 1) > %this.monthDays[%this.month]) {
			%this.day = 0;
			%this.month++;
			%this.onMonth();
		}

		if(%this.month > 11) {
			%this.month = 0;
			%this.year++;
			%this.onYear();
		}
	}

	function CM_Tick::getTimeInSeconds(%this) {
		return ((%this.hour * 60) * 60) + (%this.minute * 60);
	}

	function CM_Tick::getTimeString(%this) {
		return getTimeString(%this.getTimeInSeconds());
	}

	function CM_Tick::getDayOfYear(%this) {
		%days = 0;

		for(%i = 0; %i < %this.month; %i++) {
			%days += %this.monthDays[%i];
		}

		%days += %this.day;
		return %days;
	}

	function CM_Tick::getDayOfWeek(%this) {
		return %this.day % 7;
	}

	function CM_Tick::getDayOfWeekName(%this) {
		switch(%this.getDayOfWeek()) {
			case 0: return "Monday";
			case 1: return "Tuesday";
			case 2: return "Wednesday";
			case 3: return "Thursday";
			case 4: return "Friday";
			case 5: return "Saturday";
			case 6: return "Sunday";
			default: return "ERROR";
		}
	}

	function CM_Tick::getFractionalHour(%this) {
		return %this.hour + (%this.minute / 60);
	}

	function CM_Tick::getFractionalYear(%this) {
		return mDegToRad((360 / 365.25) * ((%this.getDayOfYear() + 1) + (%this.getFractionalHour() + 1) / 24));
	}

	function CM_Tick::getFormattedTime(%this, %twelvehour) {
		if(%twelvehour) {
			%hour = %this.hour == 0 ? 12 : ((%this.hour > 12) ? (%this.hour - 12) : %this.hour);
			return pad(%hour, 2) @ ":" @ pad(%this.minute, 2) SPC ((%this.hour >= 12) ? "PM" : "AM");
		} else {
			return pad(%this.hour, 2) @ ":" @ pad(%this.minute, 2);
		}
	}

	function CM_Tick::getFormattedDate(%this) {
		return %this.monthName[%this.month] SPC (%this.day + 1) @ "," SPC %this.year;
	}

	function CM_Tick::getLongDate(%this) {
		// Want to use the DMY calendar format; using MDY due to its prevalency in the U.S.
		return %this.monthName[%this.month] SPC (%this.day + 1) @ "," SPC %this.year;
	}

	function CM_Tick::getShortDate(%this) {
		// Want to use the DMY calendar format; using MDY due to its prevalency in the U.S.
		return pad(%this.month + 1, 2) @ "/" @ pad(%this.day + 1, 2) @ "/" @ %this.year;
	}

	function CM_Tick::syncDayCycle(%this) {
		if(!isObject(Sun)) {
			return;
		}

		Sun.elevation = (%this.getTimeInSeconds() * (360 / 86400));
		//Sun.elevation = mRadToDeg(%this.getSunElevationAngle());
		//Sun.azimuth = mRadToDeg(%this.getSunAzimuthAngle());

		Sun.sendUpdate();
	}

	//function CM_Tick::getSunEquationOfTime(%this) {
	//	%y = %this.getFractionalYear();
	//	return 229.18 * (0.000075 + 0.001868 * mCos(%y) - 0.032077 * mSin(%y) - 0.014615 * mCos(2 * %y) - 0.040849 * mSin(2 * %y));
	//}

	//function CM_Tick::getSunDeclination(%this) {
	//	%y = %this.getFractionalYear();
	//	return 0.006918 - 0.399912 * mCos(%y) + 0.070257 * mSin(%y) - 0.006758 * mCos(2 * %y) + 0.000907 * mSin(2 * %y) - 0.002697 * mCos(3 * %y) + 0.00148 * mSin(3 * %y);
	//}

	//function CM_Tick::getSunSolarHourAngle(%this) {
	//	return (($pi * 2) / 24) * (%this.getFractionalHour() - 12) + mDegToRad(%this.longitude) + %this.getSunTimeCorrection();
	//}

	//function CM_Tick::getSunZenithAngle(%this) {
	//	%decl = %this.getSunDeclination();
	//	return mAcos(mSin(mDegToRad(%this.latitude)) * mSin(%decl) + mCos(mDegToRad(%this.latitude)) * mCos(%decl) * mCos(%this.getSunSolarHourAngle()));
	//}

	//function CM_Tick::getSunElevationAngle(%this) {
	//	%decl = %this.getSunDeclination();
	//	return mAsin(mSin(mDegToRad(%this.latitude)) * mSin(%decl) + mCos(mDegToRad(%this.latitude)) * mCos(%decl) * mCos(%this.getSunSolarHourAngle()));
	//}

	//function CM_Tick::getSunAzimuthAngle(%this) {
	//	%decl = %this.getSunDeclination();
	//	return mAcos((mSin(%decl) * mCos(mDegToRad(%this.latitude)) - mCos(%this.getSunSolarHourAngle()) * mCos(%decl) * mSin(mDegToRad(%this.latitude))) / (mSin(%this.getSunZenithAngle())));
	//}

	//function CM_Tick::debugSun(%this) {
	//	echo("Date / Time       =" SPC %this.getLongDate() SPC "/" SPC %this.getFormattedTime() SPC "(" @ %this.getFormattedTime(true) @ ")");
	//	echo("Equation of Time  =" SPC %this.getSunEquationOfTime());
	//	echo("Declination       =" SPC %this.getSunDeclination() SPC ":" SPC mRadToDeg(%this.getSunDeclination()));
	//	echo("Time Correction   =" SPC %this.getSunTimeCorrection() SPC ":" SPC mRadToDeg(%this.getSunTimeCorrection()));
	//	echo("Solar Hour Angle  =" SPC %this.getSunSolarHourAngle() SPC ":" SPC mRadToDeg(%this.getSunSolarHourAngle()));
	//	echo("Zenith Angle      =" SPC %this.getSunZenithAngle() SPC ":" SPC mRadToDeg(%this.getSunZenithAngle()));
	//	echo("Elevation Angle   =" SPC %this.getSunElevationAngle() SPC ":" SPC mRadToDeg(%this.getSunElevationAngle()));
	//	echo("Azimuth Angle     =" SPC %this.getSunAzimuthAngle() SPC ":" SPC mRadToDeg(%this.getSunAzimuthAngle()));
	//}

	function CM_Tick::onTick(%this) {
		parent::onTick(%this);

		%this.passTime();
		%this.syncDayCycle();
	}

	function CM_Tick::onHour(%this) { }

	function CM_Tick::onDay(%this) {
		%this.saveTime();

		if(%this.getDayOfWeek() == 0) {
			%this.onWeek();
		}
	}

	function CM_Tick::onWeek(%this) { }
	function CM_Tick::onMonth(%this) { }
	function CM_Tick::onYear(%this) { }
};

if(isPackage(CityMod_Tick_Daycycle))
	deactivatePackage(CityMod_Tick_Daycycle);
activatePackage(CityMod_Tick_Daycycle);