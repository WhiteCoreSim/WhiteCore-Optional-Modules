#WhiteCore-Optional-Modules
==========================

This is the Optional Modules Repository for WhiteCore

*NOTE:  As of Version 0.9.2, the WhiteCore repository format has changed so ensure that you are using the correct format for your repo.
This should not be a problem if you use the latest commits of the WhiteCore-Dev or a release version >= 0.9.2*

# Installation

There are two ways to install these modules, auto-installation, or manual compilation

## Automated
1. Start WhiteCore.exe or WhiteCore.Server.exe
2. Type 'compile module <path to the build.am of the module that you want>' into the console and it will install the module for you and tell you how to use or configure it.

## Manual Compilation and installation:
Copy the selected directory to the addon-modules of the main source code directory.
Each module should be in it's own tree and the root of the tree should contain a file named "prebuild.xml".

*The prebuild.xml should only contain <Project> and associated child tags. 
The <?xml>, <Prebuild>, <Solution> and <Configuration> tags should not be included since the add-on modules prebuild.xml will be inserted directly into the main prebuild.xml*

The module source code will be included in the main WhiteCore solution when the "runprebuild" script is executed for your system.


#Notes
Latest update of May 15th, 2015

All modules available compile correctly.
##FlexibleWind: 
  Needs the AForge.Math.dll copied to the WhiteCoreSim/bin folder
##FreeswitchVoice:
  Has not been tested fully so there may be issues
##IARModifierGUI:
  Is a standalone program, still largely untested.  Have a goo backup if you are modifying the DefaultInventory.iar
##IrCChat:
  The 'group' chat is still under development and may work in unexpected ways.  The Region chat is fine.
