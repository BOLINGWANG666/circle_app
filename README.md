# Circle - 2D Roguelike Survival Game 

**Developer:** WANG FUMING  
**Framework:** .NET MAUI (C#)

Circle is a high-intensity 2D action game where you navigate a hero through endless waves of geometric foes. Gain experience, pick the right upgrades, and survive the bullet-hell chaos.

##  Key Features

* **Session Persistence:** Powered by SQLite, the game constantly records your ongoing run. Your health, attack power, cooldowns, evasion rate, and remaining time are securely stored so you can pick up exactly where you left off.
* **JSON Configured Abilities:** The stats for upgrades and skills are separated from the main codebase. Using a centralized JSON configuration makes it incredibly easy to balance the game and add new powers in the future.
* **Engaging Combat:** * **Evasion:** A probability-based dodge feature (starting at 5%) provides visual "Miss" indicators, adding a layer of luck and survivability.
    * **Ranged Foes:** Large elite targets will shoot energy blasts at you. This forces you to stay on the move rather than just dragging melee enemies in a big circle.
* **Anti-Clumping Physics:** A customized mathematical repulsion formula stops enemies from stacking on top of one another. They push each other apart to form a natural, organic swarm.
* **Pre-Battle Setup:** A clean interface lets you preview the starting attributes of different heroes before diving into the action.
* **Safe Exit System:** Built with good Human-Computer Interaction in mind, the pause menu features a dedicated exit button that safely wraps up your session instead of forcing a hard app closure.

##  Technical Stack

* **Language:** C# 11 / .NET 8
* **UI & Rendering:** XAML (MAUI) combined with dynamic code-behind for rendering entities and hit-texts.
* **Performance:** Uses a custom twelve-millisecond update loop, paired with object pools for both the geometric foes and their energy blasts to maintain smooth framerates.
* **Data Storage:** SQLite handles the save slots, while JSON manages the static game configurations.

##  How to Play

1.  **Pick a Hero:** Tap the avatar icon to check their starting stats and confirm.
2.  **Move Around:** Drag the virtual joystick to steer clear of the red squares.
3.  **Grow Stronger:** Gather blue diamonds to fill your experience bar. Upon leveling up, select one of three random cards to boost your power.
4.  **Stay Alive:** Avoid the enemies and survive until the clock runs out!

## 🔗 Repository
[https://github.com/BOLINGWANG666/circle_app](https://github.com/BOLINGWANG666/circle_app)
