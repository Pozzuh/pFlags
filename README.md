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
1. To install the plugin, place pFlagPlugin.dll and Newtonsoft.Json.dll into %ServerRoot%/plugins/. **Note: do not rename the .dll file, this will cause issues.** 
2. Place the pFlags folder in /addon/ in your %ServerRoot%/addon folder.
3. Use the !coords command to find your current coordinates, and add them to <mapname>.json inside the pFlags folder. **Note: the files should be valid json. use [this](http://jsonlint.com/) to validate.

Player usage
-----------
Press the activate button (default: f) whilst standing on a flag.

Compiling
---------
- Make sure the project is set to .NET framework 3.0.
- Reference json.net
- Add addon.dll from %ServerRoot%/dist/ to your project.