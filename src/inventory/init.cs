// ============================================================
// Project          -      CityMod
// Description      -      Inventory Module Initialization
// ============================================================

exec("./class.cs");
exec("./item.cs");
exec("./player.cs");

CM_Players.addField("Inventory", CityModInventory(6, 6));
CM_BricksSaver.addSavedField("inventory");