# MysteryRogueLite

Hello there. If you're looking to play the game, you can download it from https://iowa-my.sharepoint.com/:u:/g/personal/impope_uiowa_edu/ESo8zXOp_cRMmrPk7aQNy4gBlCFrgafJpWBTMikF67ax4w . That gives you the zip file, which you can unzip. Inside is a copy of this readme, and the Unity executable you can run. If for some reason you want to compile it yourself, download everything in this Github and import it into a blank Unity project.


Project Vault

Project Vault is a turn based, deckbuilding dungeon crawler, and your goal is to make it to the ladder of each floor to go deeper. You start with a basic deck of cards, and build up a better and more synergistic one as you play and defeat enemies.


How to play:

Use either WASD or the arrow keys to move. Click on cards to select them, then click on where you want to play it. You can right click on a card to open up an information screen about that card.

You have a deck of cards that represent actions you can take, and you draw one card from your deck every turn, up to a maximum hand size of 8. Other sources of card draw can bring you above this, for a total maximum of 12.

To play a card, click it to select it, then click one of the highlighted blue tiles to play the card on that tile. For example, you can play a damage dealing cards on an enemy. If you're not sure what a card does, right click on it for a tool tip.

Playing a card costs energy, which is the number in the orange circle near the top of the card. You refill to 3 energy at the start of your turn. Moving without the use of a card costs 1 energy and automatically ends your turn. Playing cards or moving also costs Spirit, and if your spirit gets too low, you'll be weakened. Regain spirit by going down floors.

When you play a card from your hand, it goes into your discard pile. You can also force discard a card from your hand by double clicking it, at the cost of Spirit. Once you are out of cards to draw from your deck, you will shuffle your discard pile into your draw pile. Click "Show All Cards" to open up a window where you can use the toggle buttons at the top to see your draw, discard, hand, and banish. Click the "Show All Cards" button again to close the window, or click off of the window.

Your goal is to explore the dungeon until you find the ladder leading down. You will sometimes encounter enemies that will move towards you and attack every turn. Play damage dealing cards to defeat them. Your health is the red bar above your spirit. If it hits 0, you lose.

You also can gain new cards for your deck. On the right, there is a button for "Get Card Reward". Once you have at least 30 coins (coins are dropped by defeated enemies, or in chests), you can click it and open a screen with three cards you can choose from. If you click one, it will be added to your hand, provided you have space for it. If you don't want any of the cards offered, you can click the button at the bottom to skip the card reward and regain 20 health. Buying new cards is extremely important, as it will improve the overall quality of your deck.


Advanced Rules:

Try to find treasure chests in the dungeon. They can contain money, card rewards, or special one use "hanafuda" cards. If you have a hanafuda card, press V to open your inventory and view it. You can play it like any other card, and they are powerful, but they can only be played once before being totally destroyed.

There is also a keyword, Echo. A card with an Echo effect will say "Echo: [Do something]". When a card with an Echo effect is discarded from another card's effect (IE, you didn't double click to discard it, or discard it by playing it), it will do something. Echo effects do not cost energy or spirit.

A card that says "Banish: X" will not go into the discard pile when played. Instead, it will go into the special "banish" pile and be unavailable for X floors, after which point they will go back into your discard pile. However, these cards tend to be quite powerful.

You might also find traps on the ground. Anything that steps on it (you or an enemy) will take some damage. If you hold down "L", you can force diagonal movement to get around these traps. You can also play movement based cards to get around them. Your starting footwork card also can be used around corners, making it very useful at avoiding traps, even those in hallways.

There are currently 4 status effects in the game, Defense, Momentum, Insight, and Spirit Loss. If you currently have a status effect, it will show up as an icon under the blue box in the top left. You can mouse over the icon for a description of what it does.

Defense acts as additional health that is removed before your normal health, but it decays at a rate of one per turn. It shows up as a blue shield icon.

Momentum boosts all flat instances of damage dealt by your next card by that amount, and is represented as a red sword icon. So if you have 5 momentum and play a card dealing 10 damage, you will deal 15 damage instead, and then reset to 0 momentum. Momentum works especially well with cards that hit multiple times, as it boosts each instance of damage. If you have 5 momentum and then play a card that deals 10 damage three times, it will deal (10+5) * 3 = 45 damage instead of its usual 30. Momentum decays by 1 each turn, but this decay is paused if you have an enemy currently chasing you.

Insight acts as a multiplier for your next source of damage, and is represented as a yellow light icon. Each point of insight makes your next source of damage deal +100% extra damage. So if you have 2 Insight, and play a card that deals 10 damage 3 times, it will deal (10 * (2 + 1) + 10 + 10) = 50 damage, then your insight will be reset to 0. With a card that hits multiple times, insight only applies to the first hit. Like momentum, this decays at a rate of 1 per turn, but this decay is paused if you have an enemy currently chasing you.

You get Spirit Loss for having low amounts of spirit, and it is represented by the dark blue spiral. Having 67-33% spirit is stage 1 Spirit Loss, having 33-0% is stage 2, and having 0 is stage 3. Each stage will lower the number of cards you draw and the number of cards you redraw up to. If you're out of spirit, you'll start losing health instead. If you're low on spirit, try to get to the next floor as quickly as you can.

Controls:

* WASD / arrow keys for movement.
* Press 'Enter' or Q to end your turn without moving.
* Click to (de)select cards / play them.
* Right click a card to open up the tooltip for that card.
* Double click to force discard a card.
* Press C to open up the card window.
* Press V toggle the visibility of your inventory of hanafuda cards.
* Press B to buy a card.
* Press P to toggle moving the camera. While moving the camera, you can scroll wheel in/out to zoom.
* Hold L to force diagonal movement.
* Press ESC to quit the game.

There are also cheat options you can press if you're stuck. As one of the developers, I would suggest you not use these, as it is not the intended experience. However, they have been left in the game in case you find the game too difficult or simply want to see more of the game.

* Press f1 to gain 30 coins.
* Press f2 to gain a random hanafuda card.
* Press f3 to gain back some spirit.
* Press f4 to lose spirit.
