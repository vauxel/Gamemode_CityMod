// ============================================================
// Project          -      CityMod
// Description      -      Global Config Variables
// ============================================================
// Sections
//   1: Config
//     1.1: Path
//     1.2: Tick
//     1.3: Organizations
//     1.4: Players
// ============================================================

// ============================================================
// Section 1 - Config
// ============================================================

// 0 = Off / Only Critical Messages
// 1 = Semi-important Messages
// 2 = All Messages (Debugging Only)
$CM::MessageLevel = 2;

// ------------------------------------------------------------
// Section 1.1 - Path
// ------------------------------------------------------------
$CM::Config::Path::Data = "config/server/CityMod/"; // Path to data storage.
$CM::Config::Path::Mod = "Add-Ons/Gamemode_CityMod/"; // Path to add-on.

// ------------------------------------------------------------
// Section 1.2 - Tick
// ------------------------------------------------------------
$CM::Config::Tick::Speed = 2500; // Speed of the ticks.

// ------------------------------------------------------------
// Section 1.3 - Organizations
// ------------------------------------------------------------
$CM::Config::Organizations::MaxOrganizationsPerPlayer = 2;
$CM::Config::Organizations::MaxOrganizationNameLength = 32;
$CM::Config::Organizations::MaxOrganizationDescLength = 192;
$CM::Config::Organizations::MaxJobs = 25;
$CM::Config::Organizations::MaxJobNameLength = 28;
$CM::Config::Organizations::MaxJobDescLength = 192;
$CM::Config::Organizations::MaxJobSalary = 100000;
$CM::Config::Organizations::MaxJobOpenings = 1000;
$CM::Config::Organizations::MaxJobTasks = 20;

// ------------------------------------------------------------
// Section 1.4 - Players
// ------------------------------------------------------------
$CM::Config::Players::MaxBuildHeight = 256;
$CM::Config::Players::MaxSkillLevel = 100;

$CM::Config::Players::SkillXPMultiplier = 1.25;

$CM::Config::Players::DamagePrec["head"] = 200; // %
$CM::Config::Players::DamagePrec["torso"] = 100; // %
$CM::Config::Players::DamagePrec["legs"] = 2; // %
$CM::Config::Players::DamagePrec["arms"] = 10; // %
$CM::Config::Players::DamagePrec["hip"] = 10; // %

$CM::Config::Players::MaxDamage["head"] = 125;
$CM::Config::Players::MaxDamage["torso"] = 100;
$CM::Config::Players::MaxDamage["legs"] = 50;
$CM::Config::Players::MaxDamage["arms"] = 50;
