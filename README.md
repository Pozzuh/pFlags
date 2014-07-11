pFlags Plugin
================

This is a plugin for [Nukem's MW3 server addon](http://www.itsmods.com/forum/Thread-Release-MW3-Server-Addon--7734.html). It allows you to teleport between flags placed by admins.


Requirements
------------
- Nukem's server addon v1.190+
- MW3 Dedicated server v1.4.382+
- .NET Framework 3.0


Admin usage
-----------
1. To install the plugin, place pFlagPlugin.dll and Newtonsoft.Json.dll into %ServerRoot%/plugins/.
2. Place the pFlags folder in /addon/ in your %ServerRoot%/addon folder.
3. Use the !coords command to find your current coordinates, and add them to <mapname>.json inside the pFlags folder. **Note: the files should be valid json. Use [this](http://jsonlint.com/) to validate.**

- auto_use_in: true/false sets whether or not the in-flag should automatically TP the player.
- hide_out: true/false sets whether or not the out-flag should be hidden.
- bothways: true/false sets whether or not the flags work both ways. Using this with auto_use_in enabled will give you stupid results.
- model_in & model_out: sets which model for the flag should be used. Keep in mind every map has specific flag models loaded. "prop_flag_neutral" will always work. (Map specific: prop_flag_sas, prop_flag_speznas, prop_flag_delta) 

Player usage
-----------
Press the activate button (default: f) whilst standing on a flag.

Compiling
---------
- Make sure the project is set to .NET framework 3.0.
- Reference json.net
- Add addon.dll from %ServerRoot%/dist/ to your project.