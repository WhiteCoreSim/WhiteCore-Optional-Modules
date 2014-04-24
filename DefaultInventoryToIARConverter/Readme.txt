This module saves the current default inventory to an IAR file so they
can be used with the new IAR loaders in WhiteCore master. To run this
tool, do the following.

1.) Compile this module into WhiteCore by dropping it into the
addon-modules directory and running prebuild and then compiling. 

2.) Copy over the .ini file included in this directory into the
Configuration/Modules/ directory in the bin folder.

3.) When WhiteCore is restarted, and additional command
 'save default inventory [IAR Filename]' will be available. Execute this
command from the region console and it will create an IAR of the default
assets and inventory in the 'bin/DefaultInventory' folder.
If the optional IAR Filename is specified, then this will be used when the
inventory is exported.

March 2014