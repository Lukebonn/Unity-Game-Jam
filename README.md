The  2026 Official Unity Game Jam will be a celebration of game development where veterans, hobbyists, students and game devs of all kinds will come together to do what they love most; make games.



Join the Unity community from November 5th to November 13th for a week-long game jam. You will have a full seven days to build a game around a theme revealed at the kickoff livestream, and a couple of extra days on top to get it submitted without panicking.



Throughout the duration of the jam, we invite you to hang with your fellow game devs in the Unity Discord.



Join the jam today to stay in the loop with updates and to guarantee you won't miss out on the festivities.



We look forward to jamming with y'all at the 2026 Official Unity Game Jam!



Check out last year's  Unity 20th Anniversary Game Jam page for inspiration and to get hype!





## How the scripts talk to each other

### Legend

```mermaid
flowchart TB
    A1["A"] -->|"direct call, works today"| B1["B"]
    A2["A"] ==>|"raises event, works today"| B2["B"]
    A3["A"] -.->|"wired or planned, not firing yet"| B3["B"]
    OPEN["open slot, not written yet"]:::open

    classDef open stroke-dasharray: 5 5
```

### 1. Direct calls between scripts

```mermaid
flowchart LR
    HHUD["HUD/PlayerHealthHUD.cs"] -->|"reads HP + armor every frame"| PH["Player/PlayerHealth.cs"]
    SHUD["HUD/StaminaHUD.cs"] -->|"reads stamina every frame"| PS["Player/PlayerStamina.cs"]
    SPD["HUD/SpeedHUD.cs"] -->|"reads CurrentSpeed, IsSprinting"| PC["Player/PlayerCombat.cs"]
    GUN["HUD/GunHUD.cs"] -->|"reads fire rate, damage, IsInstakill"| PC
    PC -->|"sprinting: TryUse()"| PS
    PH -->|"HP reaches 0: disables"| PC
    PC ==>|"OnFire (UnityEvent, wired in Inspector)"| LS["Audio/LayeredSound.cs<br/>(Audio_PlayFire prefab)"]
    PC -->|"bullet raycast hits: TakeDamage()"| EB["Enemies/EnemyBase.cs (abstract)"]
    AI["Enemies/EnemyMeleeAI, EnemyDashAI,<br/>EnemyRangedAI, TestEnemy"] ---|"inherit"| EB
    AI -->|"melee + dash hits: TakeDamage()"| PH
    AI -->|"ranged: Initialize()"| PROJ["Enemies/EnemyProjectile.cs"]
    PROJ -->|"hits: TakeDamage()"| PH
    WS["Enemies/WaveSpawner.cs<br/>(WaveManager)"] -->|"spawns, then Apply()"| ESC["Enemies/EnemyScaler.cs"]
    EB ==>|"OnAnyEnemyDied (its own static event)"| WS
    LU["Player/LevelUp.cs"] -->|"keys 1-6: TrySpendDie() until none landed"| TRAY["HUD/UnrolledDiceUI.cs"]
    TRAY -->|"spawns, Roll()"| DIE["HUD/DiceUI.cs<br/>(Die prefab, a d20)"]
    DIE -->|"is it the lucky number?"| LN["Dice/LuckyNumber.cs<br/>(static, saved with PlayerPrefs)"]
    LE["Dice/LuckyEffects.cs"] -->|"KillAll: TakeDamage()"| EB
    LE -->|"Instakill: StartInstakill()"| PC
    GM["GameManager.cs"] -->|"Enter, Editor only: AddDie()"| TRAY
    GM -->|"timer stops when isDead"| PH
    POP["HUD/RollPopHUD.cs"] -->|"between rolls: shows"| LN
    TIME["HUD/SurvivalTimerHUD.cs"] -->|"reads SurvivedSeconds"| GM
    GO["Menus/GameOverUI.cs"] -->|"reads time"| GM
    GO -->|"reads wave, kills"| WS
    GO -->|"reads Level"| LU
    MM["Menus/MainMenu.cs<br/>(MainMenu scene)"] -->|"lucky buttons change it"| LN
    CAM["CameraFollow.cs<br/>(Main Camera, follows Player)"]
```

### 2. SignalBus events

Every kill raises OnEnemyDeath, which puts a d20 in the tray. When a die stops rolling it raises OnDieLanded with its number and its lucky effect (None unless it rolled your lucky number). Keys 1-6 (or numpad 1-6) put every landed die into one stat, and each die raises that stat's event with its number. HUD scripts never listen to the stat events; they read each number from the script that owns it. Debug key (Editor only): Enter adds a die to the tray.

```mermaid
flowchart LR
    KEYS["Keys 1-6 or numpad 1-6"] --> LU["Player/LevelUp.cs"]
    EB["Enemies/EnemyBase.cs"]
    DIE["HUD/DiceUI.cs"]

    subgraph BUS["SignalBus.cs (static events)"]
        E_DEATH(["OnEnemyDeath"])
        E_DIE(["OnDieLanded(int value, LuckyEffect)"])
        E_DEAD(["OnPlayerDied"])
        E_HP(["OnLvlHealth(int roll)"])
        E_STA(["OnLvlStamina(int roll)"])
        E_MOV(["OnLvlMovement(int roll)"])
        E_RPM(["OnLvlRPM(int roll)"])
        E_DMG(["OnLvlDamage(int roll)"])
        E_ARM(["OnLvlArmor(int roll)"])
    end

    EB ==>|"Die(): InvokeEnemyDeath()"| E_DEATH
    E_DEATH ==>|"AddDie()"| TRAY["HUD/UnrolledDiceUI.cs"]

    DIE ==>|"stopped rolling: InvokeDieLanded()"| E_DIE
    E_DIE ==>|"Pop()"| POP["HUD/RollPopHUD.cs"]
    E_DIE ==>|"lucky: KillAll or Instakill"| LE["Dice/LuckyEffects.cs"]

    PH["Player/PlayerHealth.cs"] ==>|"HP reaches 0: InvokePlayerDied()"| E_DEAD
    E_DEAD ==>|"shows the panel"| GO["Menus/GameOverUI.cs"]

    LU ==>|"key 1"| E_HP
    LU ==>|"key 2"| E_STA
    LU ==>|"key 3"| E_MOV
    LU ==>|"key 4"| E_RPM
    LU ==>|"key 5"| E_DMG
    LU ==>|"key 6"| E_ARM

    E_HP ==>|"HandleHlthLvlUp()"| PH
    E_ARM ==>|"HandleArmorLvlUp()"| PH
    E_STA ==>|"HandleStaminaLvlUp()"| PS["Player/PlayerStamina.cs"]
    E_MOV ==>|"HandleMoveLvlUp()"| PC["Player/PlayerCombat.cs"]
    E_RPM ==>|"HandleRPMLvlUp()"| PC
    E_DMG ==>|"HandleDamageLvlUp()"| PC
```

### 3. Scenes and prefabs

- Build order: scene 0 is `MainMenu`, scene 1 is the game (`SampleScene` for now). The menu's Play loads scene 1 and Esc or the game-over Main Menu button loads scene 0, by number rather than name, so a renamed or replaced level scene still works.
- `Prefabs/HUD.prefab` is the whole game Canvas (bars, speedometer, gun readouts, level badge, roll pop, timer, dice tray, game over). `Prefabs/GameManager.prefab` holds GameManager, SignalBus and LuckyEffects.
- To use them in another level scene, drop both prefabs in. Their scripts find the Player, GameManager and WaveSpawner on their own if nothing is dragged into those fields, and PlayerCombat adds a PlayerStamina to the player if it's missing.
