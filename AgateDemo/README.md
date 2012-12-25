AgateDemo
===========

Testing ground for a sprite-based pseudo-isometric RPG/roguelike thing.

You control a few units against a horde of randomly-moving creatures that attack when they get close.
On your units' turns, a menu will appear with Move, Attack and Wait as options.
 * Wait ends your turn and does nothing else.
 * Move lets a unit move up to 6 squares, ending when you press Z.  You can cancel with X to return to the action selection.
 * Attack lets you select one of your unit's skills, which may have a range it can be used at.  Move the cursor onto an enemy within range and press Z, or cancel with X.

Creatures have a health number that flashes on their sprite, or in a few cases (such as for snakes and other small creatures), above it.
When attacked their health goes down, and if it reaches 0 the creature is destroyed.

There's no victory condition right now, just survive as long as you can while the monsters try to eat you and each other.

Credits
=======
Significant credit to Kawa on #rgrd (Quakenet IRC).

License
=======
The AgateDemo code is licensed under the Mozilla Public License, and so is AgateLib.
The sprites used are probably licensed under the NetHack General Public License, since they were distributed as part of a larger project with that license, but
since I don't speak Japanese and the identity of the author of the sprites has disappeared from the internet, I can't be certain.  See LICENSE.txt for more information and the text of these two licenses.

*If you are the author of the nh3d.bmp tileset, contact me via GitHub message if you have any concerns about the usage of your sprites (that I added on to).*
