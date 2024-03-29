# In the name of God
## Escape The Maze
A game where you have to traverse pseudo-randomly generated maze-like levels, reach the 'Exit point' while dodging 'Walls', optionally collecting 'Bonus points' for a score bonus and also 'Coins' which can be spent in the game's upgrade shop.

## Features
* (Pseudo-)Randomly-generated entities, wall colors & round (maze) layouts.
* An 'Upgrade shop' system which allows you to spend your hard-earned money on cool upgrades for your character.
* ...And much more!

## Game entities
The listed entries are updated with the latest game update.

<table>
	<tr>
		<th>Symbol</th>
		<th>Color</th>
		<th width="120">Name</th>
		<th width="80">Spawn chance</th>
		<th>Max occurrences</th>
		<th>Description</th>
	</tr>
	<tr align="center">
		<td>☻</td>
		<td>Green</td>
		<td>Player</td>
		<td>~</td>
		<td>1</td>
		<td align="left">Your main, playable character in the game.</td>
	</tr>
	<tr align="center">
		<td>■</td>
		<td>Red</td>
		<td>Exit Point</td>
		<td>~</td>
		<td>1</td>
		<td align="left">Your main destination in each round. reaching it will grant you 30 score.</td>
	</tr>
 	<tr align="center">
		<td>▓</td>
		<td>Random</td>
		<td>Wall</td>
		<td>~</td>
		<td>900</td>
		<td align="left">The main obstacles in the game. Colliding with one deducts a heart, 20 score and cause you to respawn, thus be extra careful around those.</td>
	</tr>
 	<tr align="center">
		<td>♦</td>
		<td>Yellow</td>
		<td>Bonus Point</td>
		<td>45%</td>
		<td>3</td>
		<td align="left">Collecting one increments your score by 20.</td>
	</tr>
 	<tr align="center">
		<td>$</td>
		<td>Green</td>
		<td>Coin</td>
		<td>40%</td>
		<td>2</td>
		<td align="left">Your main source of income, increases your balance by a random value ranging from $15 to $50.</td>
	</tr>
 	<tr align="center">
		<td>♥</td>
		<td>Purple</td>
		<td>First Aid Kit</td>
		<td>22.5%</td>
		<td>2</td>
		<td align="left">Restores a heart; however, you won't receive more if you already have 3 (5 if you've purchased the 'Toughnut' upgrade).</td>
	</tr>
 	<tr align="center">
		<td>ϴ</td>
		<td>Purple</td>
		<td>Hourglass</td>
		<td>25%</td>
		<td>1</td>
		<td align="left">Buys you some more time by incrementing the round time limit by 15 seconds.</td>
	</tr>
</table>

## Upgrades
The listed entries are updated with the latest game update.

<table>
	<tr>
		<th width="200">Name</th>
		<th width="70">Min. required level</th>
		<th width="250">Price</th>
		<th>Description</th>
	</tr>
	<tr align="center">
		<td>Level Upgrade</td>
		<td>1</td>
		<td>Dynamic (Current level * 1,000)</td>
		<td align="left">Levels up your character to level (Current level + 1).</td>
	</tr>
	<tr align="center">
		<td>Expanded Wallet</td>
		<td>2</td>
		<td>$2,500</td>
		<td align="left">Expands your maximum wallet capacity to $5,000.</td>
	</tr>
	<tr align="center">
		<td>Toughnut</td>
		<td>3</td>
		<td>$4,000</td>
		<td align="left">Increases your maximum heart capacity to 5.</td>
	</tr>
	<tr align="center">
		<td>The Quarter of Victory</td>
		<td>3</td>
		<td>$3,750</td>
		<td align="left">Increases the round time limit to 60 seconds.</td>
	</tr>
	<tr align="center">
		<td>Nicer Walls</td>
		<td>3</td>
		<td>$3,250</td>
		<td align="left">Reduces the wall collision chance to 70% and halves the score penalty (though, it won't prevent respawning).</td>
	</tr>
	<tr align="center">
		<td>The Boots of Leaping</td>
		<td>5</td>
		<td>$5,000</td>
		<td align="left">Hold 'Shift' along with the navigation keys (WASD only!) to double your character's movement speed and additionally, jump over walls!</td>
	</tr>
	<tr align="center">
		<td>3x Armor Points</td>
		<td>2</td>
		<td>$750</td>
		<td align="left">Armor points which are capable of absorbing the inflicted wall collision damage instead of your hearts for up to 3 times (You can purchase this upgrade again once you run out of armor points).</td>
	</tr>
</table>

## Controls
This game simply utilizes the standard WASD/Arrow keys in order to move your character around.

Though, you have to use the WASD keys (Arrow keys won't work) along with the Shift key in order to jump if you've purchased the 'Boots of Leaping' upgrade.

## Known issues/inconsistencies
### Possible round softlock:
~~On very rare occasions, there might be some walls completely surrounding the player and/or the exit point, making the round unbeatable. However, you can bypass them with the "The Boots of Leaping" upgrade.~~

This is fixed as of version `1.1.0`.

### Players' data corruption:
One small syntactical error in the output XML file (Located at `C:\Users\%Username%\AppData\Local\EscapeTheMaze\Users.xml`) can render the whole file useless. this normally wouldn't happen if you don't tamper with the file.

### Washed-out colors:
The game colors may look and feel different on some different windows platforms, notably on Windows 7 and 10 (Some colors on Windows 10 might look moderately washed-out because of Microsoft's new console colors).

## Notes
.NET 6 Desktop Runtime (either x86 or x64 depending on the game architecture you download) is required to run the game. You can download it [here](https://dotnet.microsoft.com/en-us/download/dotnet/6.0). (**Make sure to download the '.NET Desktop Runtime' version!**)

The suggested console settings are as follows:
* **Window width**: 100
* **Window height**: 30
* **Font family**: Consolas
* **Font size**: 16

You are suggested to set the font family to 'Consolas' and font size to '16' (other Monospaced fonts should work fine as well) for the optimal experience **(Trust me, you won't want to play the game with 'Raster fonts').**

Moreover, the game is configured to run at fixed 100 x 30 resolution, thus you shouldn't manually change the window & buffer sizes.

## Debug mode
Alternatively, you can run the game in 'Debug mode', which provides you with more information and control (e.g. Player's position, random' seed, importing and exporting users' data, etc.) over the game by running the game executable file with the `-debug` parameter.

Here is a list of currently supported parameters:

<table>
	<tr>
		<th>Parameter name</th>
		<th>Purpose</th>
	</tr>
	<tr align="center">
		<td>-debug</td>
		<td>Enables the Debug mode.</td>
	</tr>
	<tr align="center">
		<td>-displayfloodfillpath</td>
		<td>Displays the path navigated by the new flood-fill algorithm.</td>
	</tr>
</table>

## Images
<img src="https://github.com/Wirmaple73/EscapeTheMaze/assets/71328992/9a0c5650-bd39-4bbf-b331-fd4a89062a76" width="15%"></img> 
<img src="https://github.com/Wirmaple73/EscapeTheMaze/assets/71328992/4ad1f381-4b19-4a0e-9644-0d21519dbfc3" width="15%"></img> 
<img src="https://github.com/Wirmaple73/EscapeTheMaze/assets/71328992/37bf21c6-bb74-4ca2-b222-37b8f3ab5278" width="15%"></img> 
<img src="https://github.com/Wirmaple73/EscapeTheMaze/assets/71328992/c18f1e5f-9875-430b-8438-07d42300be2e" width="15%"></img> 
<img src="https://github.com/Wirmaple73/EscapeTheMaze/assets/71328992/5beadb97-ca5c-4ca8-a270-1f3a073403cd" width="15%"></img> 
<img src="https://github.com/Wirmaple73/EscapeTheMaze/assets/71328992/b2a7f21b-7c8f-4573-b6bb-ab7e1fef43f8" width="15%"></img> 

## Download link
Enough wall of text, you can download the latest release [here](https://github.com/Wirmaple73/EscapeTheMaze/releases/latest).

All downloaded audio files belong to their respective owners.<br>
Feel free to submit your suggestions and report any bugs you come across, Thanks.
