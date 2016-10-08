// ==================================================================== \\
//   /$$$$$$  /$$   /$$               /$$      /$$                 /$$	\\
//  /$$__  $$|__/  | $$              | $$$    /$$$                | $$	\\
// | $$  \__/ /$$ /$$$$$$   /$$   /$$| $$$$  /$$$$  /$$$$$$   /$$$$$$$	\\
// | $$      | $$|_  $$_/  | $$  | $$| $$ $$/$$ $$ /$$__  $$ /$$__  $$	\\
// | $$      | $$  | $$    | $$  | $$| $$  $$$| $$| $$  \ $$| $$  | $$	\\
// | $$    $$| $$  | $$ /$$| $$  | $$| $$\  $ | $$| $$  | $$| $$  | $$	\\
// |  $$$$$$/| $$  |  $$$$/|  $$$$$$$| $$ \/  | $$|  $$$$$$/|  $$$$$$$  \\
//  \______/ |__/   \___/   \____  $$|__/     |__/ \______/  \_______/  \\
//                          /$$  | $$                                  	\\
//                         |  $$$$$$/                                  	\\
//                          \______/                                   	\\
// ==================================================================== \\

warn("CityMod ==> Loading Config...");
exec("./config.cs");

warn("CityMod ==> Loading Library...");
exec("./lib/init.cs");

warn("CityMod ==> Loading Source...");
exec("./src/init.cs");