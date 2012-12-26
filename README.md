AgateDemo
===========

Testing ground for a sprite-based pseudo-isometric RPG/roguelike thing.

You control a few units against a horde of randomly-moving creatures that attack when they get close.
On your units' turns, a menu will appear with Move, Attack and Wait as options.
 * Wait ends your turn and does nothing else.
 * Move lets a unit move up to 6 squares, ending when you press Z.  You can cancel with X to return to the action selection.
 * Attack lets you select one of your unit's skills, which may have a range it can be used at.  Move the cursor onto an enemy within range and press Z, or cancel with X.

You can both move and attack in any order on your turn. You can't attack twice or move twice in one turn.
If you can't do one or the other, just Wait after doing whatever it is that your unit *is* able to do.
Creatures have a health number that shows over their sprite for as long as you hold down the S key, which you can do at any time.
When attacked, a creature's health goes down, and if it reaches 0 the creature is destroyed.

There's no victory condition right now, just survive as long as you can while the monsters try to eat you and each other.

Building
========
There's a Visual Studio 2010 solution provided, but if you download the zip-archived (and possibly the tgz-archived) copy of the source from GitHub instead of cloning it,
a DLL this project uses may be blocked by some firewalls (I know Norton does it).  That will make the project mysteriously fail until the DLL (AgateOTK.dll) is unblocked.
 * The easiest way to avoid this is to go into the File Properties of the zip archive (having selected the zip, Alt+Enter in Windows Explorer) and
click Unblock if it is an option.  Then you can correctly extract the files and open the solution.
 * The hard way is to compile the current master version of AgateLib and, after building, copy the files from AgateLib's Binaries
directory to this project's root/AgateDemo/ folder, set the files to have "Copy to Output Directory" set to "Copy Always", and set the references of the project to use the assemblies you just built.

Credits
=======
Significant credit to Kawa on #rgrd (Quakenet IRC).

License
=======
The AgateDemo code is licensed under the Mozilla Public License, and so is AgateLib.
The sprites used are probably licensed under the NetHack General Public License, since they were distributed as part of a larger project with that license, but
since I don't speak Japanese and the identity of the author of the sprites has disappeared from the internet, I can't be certain.  See LICENSE.txt for more information and the text of these two licenses.

*If you are the author of the nh3d.bmp tileset, contact me via GitHub message if you have any concerns about the usage of your sprites (that I added on to).*
