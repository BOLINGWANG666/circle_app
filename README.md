# Circle - 2D Roguelike Survival Game 

**Developer:** WANG FUMING  
**Framework:** .NET MAUI (C#)

Circle is a high-intensity 2D survival action game where players must navigate a character through swarms of geometric enemies. Level up, choose your upgrades wisely, and survive the bullet-hell chaos.

## Key Features

* **Comprehensive Save & Load System:** Features a robust persistence layer using **SQLite**. [cite_start]The game automatically snapshots player progress, including HP, ATK, CD, Dodge chance, Level, and remaining time, allowing for seamless session recovery.
* **Data-Driven Architecture (JSON):** All spell and skill data are decoupled from core logic and managed via a centralized **JSON structure**. [cite_start]This allows for rapid balancing and high scalability of the upgrade system.
* [cite_start]**Deep Combat Mechanics:** * **Dodge System:** Implemented a statistical dodge mechanic (initial 5% + upgrades) with visual "Miss" feedback to enhance survivability.
    * [cite_start]**Elite Enemy AI:** Added large elite enemies that fire **projectiles** to prevent simple kiting strategies and force active repositioning.
* **Advanced Physics & Swarm AI:** Utilizes an **elastic repulsion algorithm** to prevent enemy overlapping. [cite_start]This ensures that enemy swarms move organically and maintain consistent hitbox density.
* [cite_start]**Character Selection:** A dedicated selection screen where players can preview unique character stats (HP, ATK, CD, and Dodge) before starting the battle.
* **HCI-Optimized UX:** Includes a graceful **Pause and Quit** system. [cite_start]The native "Exit" button ensures the game state is properly handled and saved according to Human-Computer Interaction principles.

## Technical Stack

* **Language:** C# 11 / .NET 8
* **Frontend:** XAML (MAUI) with custom graphics rendering for entities and particles.
* [cite_start]**State Management:** Frame-based game loop (12ms interval) with high-performance object pooling for enemies and projectiles.
* [cite_start]**Storage:** SQLite for persistent save slots and JSON for static game data.

## How to Play

1.  [cite_start]**Character Select:** Click on the character circle to view stats and confirm your choice.
2.  [cite_start]**Movement:** Use the dynamic on-screen joystick to navigate the battlefield[cite: 28, 29].
3.  **Upgrades:** Collect blue gems to level up. [cite_start]Choose one of three random cards (Atk, CD, Heal, or Dodge) to strengthen your build[cite: 41, 54].
4.  [cite_start]**Survival:** Dodge enemy projectiles and survive until the timer reaches zero[cite: 24].

## 🔗 Repository
[https://github.com/BOLINGWANG666/circle_app](https://github.com/BOLINGWANG666/circle_app)