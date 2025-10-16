# Tower UI Setup Instructions

## Overview
The Tower UI system allows players to right-click on towers to view and upgrade them. The UI follows the tower in world space and displays tower information.

## Components Created

### 1. TowerUI.cs
- Displays tower name, level, and upgrade price
- Handles upgrade button clicks
- Follows tower position in world space
- Automatically updates UI based on tower state

### 2. UiManager.cs
- Handles right-click detection on towers
- Spawns TowerUI prefabs
- Manages active tower UIs
- Prevents multiple UIs for the same tower

### 3. Updated TowerConfig.cs
- Added `TowerName` property for display in UI

## Setup Instructions

### 1. Create Tower UI Prefab
1. Create a new UI Canvas if one doesn't exist
2. Create a UI Panel as child of Canvas
3. Add the following UI elements as children of the panel:
   - TextMeshPro for tower name (assign to `towerNameText`)
   - TextMeshPro for tower level (assign to `towerLevelText`) 
   - Button for upgrade (assign to `upgradeButton`)
   - TextMeshPro for upgrade price (assign to `upgradePriceText`)
   - Button for close (assign to `closeButton`)
4. Add TowerUI component to the panel
5. Assign all UI element references in the TowerUI component
6. Save as prefab

### 2. Setup UiManager
1. Create empty GameObject named "UiManager"
2. Add UiManager component
3. Assign the Tower UI prefab to `towerUIPrefab` field
4. Set `towerLayerMask` to include tower layer

### 3. Setup GameManager
1. Assign UiManager reference in GameManager (optional - will auto-create if not assigned)

## Usage
- Left-click on ground: Place tower (existing functionality)
- Right-click on tower: Open tower UI
- Click upgrade button: Upgrade tower if affordable
- Click close button or right-click elsewhere: Close UI

## Notes
- TowerUnit doesn't know about UI (decoupled design)
- UI automatically follows tower position
- Only one UI can be open per tower
- UI updates in real-time with tower state changes
- Upgrade button is disabled when not affordable or at max level
