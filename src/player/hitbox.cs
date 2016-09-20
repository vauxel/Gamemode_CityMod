// ============================================================
// Project          -      CityMod
// Description      -      Hit Locaton Functionality
// ============================================================

function Player::getLimbDamage(%this, %limb) {
  return CM_Players.getData(%this.client.bl_id).limbs.get(%limb);
}

function Player::setLimbDamage(%this, %limb, %damage) {
  CM_Players.getData(%this.client.bl_id).limbs.set(%limb, %damage);

  if(%this.getLimbDamage("arms") >= $CM::Config::Players::MaxDamage["arms"]) {
    %this.unmountInventorySlot();
  }

  if(%this.getLimbDamage("legs") >= $CM::Config::Players::MaxDamage["legs"]) {
    %this.setDatablock(PlayerSlowCM);
  }
}

package CityMod_Hit_Location {
  function Player::mountInventorySlot(%player, %x, %y) {
    if(!isObject(%player)) {
      return;
    }

    if(%player.getLimbDamage("arms") >= $CM::Config::Players::MaxDamage["arms"]) {
      commandToClient(%player.client, 'centerPrint', "Your arms are too hurt to do that!", 1);
      return;
    }

    parent::mountInventorySlot(%player, %x, %y);
  }

  function ProjectileData::Damage(%this, %obj, %col, %fade, %pos, %normal) {
    if(%col.getClassName() $= "Player") {
      %hit = %col.getRegion(%pos, true);

      if(%hit $= "head") {
        %damage = ($CM::Config::Players::DamagePrec["head"] / 100) * %this.directDamage;
        %col.setLimbDamage("head", %col.getLimbDamage("head") + %this.directDamage);
      } else if(%hit $= "chest") {
        %damage = ($CM::Config::Players::DamagePrec["torso"] / 100) * %this.directDamage;
        %col.setLimbDamage("torso", %col.getLimbDamage("torso") + %this.directDamage);
      } else if ( %hit $= "hip" ) {
        %damage = ($CM::Config::Players::DamagePrec["hip"] / 100) * %this.directDamage;
        %col.setLimbDamage("hip", %col.getLimbDamage("hip") + %this.directDamage);
      } else if ( %hit $= "larm" || %hit $= "rarm" ) {
        %damage = ($CM::Config::Players::DamagePrec["arms"] / 100) * %this.directDamage;
        %col.setLimbDamage("arms", %col.getLimbDamage("arms") + %this.directDamage);
      } else if ( %hit $= "lleg" || %hit $= "rleg" ) {
        %damage = ($CM::Config::Players::DamagePrec["legs"] / 100) * %this.directDamage;
        %col.setLimbDamage("legs", %col.getLimbDamage("legs") + %this.directDamage);
      } else {
        %damage = %this.directDamage;
      }

      %col.damage(%obj, %pos, %damage, %this.directDamageType);
    } else {
      return parent::Damage(%this, %obj, %col, %fade, %pos, %normal);
    }
  }

  function GameConnection::onDeath(%this, %sourceObject, %sourceClient, %damageType, %damLoc, %obj) {
    %this.player.setLimbDamage("head", 0);
    %this.player.setLimbDamage("torso", 0);
    %this.player.setLimbDamage("hip", 0);
    %this.player.setLimbDamage("arms", 0);
    %this.player.setLimbDamage("legs", 0);

    return parent::onDeath(%this, %sourceObject, %sourceClient, %damageType, %damLoc, %obj);
  }

  function GameConnection::spawnPlayer(%client) {
    parent::spawnPlayer(%client);

    if(isObject(%client.player)) {
      if(%client.player.getLimbDamage("legs") >= $CM::Config::Players::MaxDamage["legs"]) {
        %client.player.setDatablock(PlayerSlowCM);
      }
    }
  }
};

if(isPackage(CityMod_Hit_Location))
	deactivatePackage(CityMod_Hit_Location);
activatePackage(CityMod_Hit_Location);