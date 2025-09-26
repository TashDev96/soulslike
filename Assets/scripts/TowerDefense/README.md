# Tower Defense Game Prototype

A tower defense game prototype using RVO agents as zombie units and immediate hit detection for towers.

## Features

- **RVO-based Zombies**: Zombies use the existing RVO (Reciprocal Velocity Obstacles) system for pathfinding and movement
- **Immediate Hit Towers**: Towers attack zombies instantly when in range (no projectiles)
- **Money System**: Players earn money for each hit and bonus money for kills
- **Tycoon-style Gameplay**: No player HP - zombies just disappear when reaching the goal (no bonus money)
- **Progressive Waves**: Each wave increases zombie health and money rewards

## Setup Instructions

1. **Create the Game Scene**:
   - Create an empty GameObject and add the `TowerDefenseGameSetup` component
   - Set up spawn points by creating empty GameObjects and assigning them to the setup component
   - Create a goal position GameObject and assign it to the setup component

2. **Configure RVO System**:
   - Ensure the RVO system is properly initialized (SampleGameObjects should be in the scene)
   - The existing RVOAgentManager and related components should be present

3. **Create Tower Prefab**:
   - Create a GameObject with the `BasicTowerPrefab` component
   - The component will automatically create visual elements (cylinder base, cube turret)
   - Save as a prefab and assign to the GameManager

4. **Setup Zombie Assets**:
   - Assign a `BakedMeshSequence` for zombie animation
   - Assign a Material for zombie rendering
   - Configure in the ZombieManager component

5. **UI Setup**:
   - The system will automatically create basic UI if none exists
   - Or create a Canvas with GameUI component for custom UI

## Game Components

### Core Classes

- **ZombieUnit**: Extends RVOAgent with health, damage, and money systems
- **TowerUnit**: Handles tower attack logic with immediate hit detection
- **ZombieManager**: Manages zombie spawning, waves, and RVO integration
- **GameManager**: Handles money system, tower placement, and game flow
- **GameUI**: Displays money, wave info, and game statistics

### Key Features

- **Money System**: Earn money on each hit + bonus for kills
- **Wave Progression**: Automatic wave advancement with increasing difficulty
- **Tower Placement**: Click-to-place towers with collision detection
- **Visual Feedback**: Attack lines show when towers fire

## Controls

- **Left Click**: Place tower (if you have enough money)
- **Mouse**: Hover to see potential tower placement

## Customization

All game parameters can be adjusted in the inspector:
- Tower damage, range, and attack speed
- Zombie health and money values
- Spawn rates and wave progression
- Starting money and tower costs

## Technical Notes

- Uses existing RVO system for zombie pathfinding
- Integrates with SkinnedMeshInstancing for efficient zombie rendering
- No physics-based projectiles - instant hit detection for performance
- Event-driven architecture for loose coupling between systems
