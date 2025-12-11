import express, { Request, Response } from 'express';
import fs from 'fs';
import cors from 'cors';
import path from 'path';
import { IPlanningData, IPlacedShip, IShip, IAiShotRequest, IAiShotResponse } from './data_interfaces';

const app = express();
const PORT = 5000;

// path for json for settings
const settingsPath = path.join(__dirname, '..', 'data', 'game_settings.json');

// path for json for screen
const screenPath = path.join(__dirname, '..', 'data', 'curr_screen.json');

// path for json for player grid
const playerGridPath = path.join(__dirname, '..', 'data', 'player_grid.json');

// path for json for pc grid (or 2nd player)
const pcGridPath = path.join(__dirname, '..', 'data', 'pc_grid.json');

// path for json for ships
const planningPath = path.join(__dirname, '..', 'data', 'planning.json');

app.use(cors());
app.use(express.json());

// ------ GRID PLAYER START

// Endpoint to get actual grid for player
app.get("/api/player-grid", (req: Request, res: Response) => {
  fs.readFile(planningPath, 'utf8', (err: NodeJS.ErrnoException | null, data: string) => {
    if (err) {
      console.log('Error reading ships:', err);
      return res.status(500).json({ success: false, message: 'Error reading ships data' });
    }

    const planningData: IPlanningData = JSON.parse(data);
    return res.json(planningData.player_grid);
  });
});

// Endpoint to change players grid
app.post("/api/player-grid", (req: Request, res: Response) => {
  const { player_grid, mode } = req.body;

  fs.readFile(planningPath, 'utf8', (err: NodeJS.ErrnoException | null, data: string) => {
    if (err) {
      console.error("Error reading planning data:", err);
      return res.status(500).json({ error: "Could not read planning data" });
    }

    let planningData: IPlanningData;
    try {
      planningData = JSON.parse(data);
    } catch (parseError) {
      console.error("Error parsing planning data:", parseError);
      return res.status(500).json({ error: "Invalid planning data format" });
    }

    planningData.player_grid = player_grid || planningData.player_grid;

    if (mode === "reset") {
      planningData.placed_ships = null; // Reset placed ships
      planningData.active_ship = null; // Reset active ships
    }

    fs.writeFile(planningPath, JSON.stringify(planningData, null, 2), (writeErr) => {
      if (writeErr) {
        console.error("Error saving grid data:", writeErr);
        return res.status(500).json({ error: "Could not save grid data" });
      }
      res.status(200).json({ message: "Grid updated successfully", planningData });
    });

  })
});

// ------ GRID PLAYER END

// ------ GRID PC START

// Endpoint to get actual PC grid (or 2nd player)
app.get("/api/pc-grid", (req: Request, res: Response) => {
  fs.readFile(pcGridPath, "utf8", (err, data) => {
    if (err) {
      return res.status(500).json({ error: "Could not read grid data" });
    }
    res.json(JSON.parse(data));
  });
});

// Endpoint to change or initialize PC grid
app.post("/api/pc-grid", (req: Request, res: Response) => {
  const { gridSize, tiles, initializeWithShips } = req.body;

  // Fetch data from file
  fs.readFile(pcGridPath, "utf8", (err, data) => {
    if (err) {
      console.error("Error reading grid data:", err);
      return res.status(500).json({ error: "Could not read grid data" });
    }

    let currentGrid;
    try {
      currentGrid = JSON.parse(data);
    } catch (parseError) {
      console.error("Error parsing grid data:", parseError);
      return res.status(500).json({ error: "Invalid grid data format" });
    }

    // Default grid if none provided
    const updatedGrid = {
      gridSize: gridSize || currentGrid.gridSize || 10,
      tiles: tiles || currentGrid.tiles || Array(gridSize || 10).fill(null).map(() => Array(gridSize || 10).fill("empty")),
    };

    // Initialize with ships if requested
    if (initializeWithShips) {
      // Fetch ship data from planning.json
      fs.readFile(planningPath, 'utf8', (err, planningDataRaw) => {
        if (err) {
          console.error("Error reading planning data:", err);
          return res.status(500).json({ error: "Could not read planning data" });
        }

        let planningData: IPlanningData;
        try {
          planningData = JSON.parse(planningDataRaw);
        } catch (parseError) {
          console.error("Error parsing planning data:", parseError);
          return res.status(500).json({ error: "Invalid planning data format" });
        }

        if (!planningData.all_ships || planningData.all_ships.length === 0) {
          return res.status(400).json({ error: "No ships available in planning data" });
        }

        // Initialize grid with ships
        updatedGrid.tiles = initializePcGridWithShips(updatedGrid.gridSize, updatedGrid.tiles, planningData.all_ships);

        // Write new data to file
        fs.writeFile(pcGridPath, JSON.stringify(updatedGrid, null, 2), (writeErr) => {
          if (writeErr) {
            console.error("Error saving grid data:", writeErr);
            return res.status(500).json({ error: "Could not save grid data" });
          }
          res.status(200).json({ message: "PC grid initialized with ships", updatedGrid });
        });
      });
    } else {
      // Update without ships
      fs.writeFile(pcGridPath, JSON.stringify(updatedGrid, null, 2), (writeErr) => {
        if (writeErr) {
          console.error("Error saving grid data:", writeErr);
          return res.status(500).json({ error: "Could not save grid data" });
        }
        res.status(200).json({ message: "Grid updated successfully", updatedGrid });
      });
    }
  });
});

// Helper function to initialize PC grid with random ship placements
function initializePcGridWithShips(gridSize: number, tiles: string[][], ships: IShip[]): string[][] {
  const newTiles = tiles.map(row => [...row]); // Clone tiles
  const placedShips: IPlacedShip[] = [];

  // Randomly place each ship
  for (const ship of ships) {
    let placed = false;
    const maxAttempts = 100; // Prevent infinite loops
    let attempts = 0;

    while (!placed && attempts < maxAttempts) {
      const rotation = Math.random() < 0.5 ? 0 : 90; // Randomly choose horizontal or vertical
      const maxRow = rotation === 0 ? gridSize - 1 : gridSize - ship.size;
      const maxCol = rotation === 0 ? gridSize - ship.size : gridSize - 1;
      if (maxRow < 0 || maxCol < 0) {
        attempts++;
        continue; // Skip if ship is too large for grid
      }

      const row = Math.floor(Math.random() * (maxRow + 1));
      const col = Math.floor(Math.random() * (maxCol + 1));

      // Check if the position is valid
      let canPlace = true;
      if (rotation === 0) {
        for (let j = col; j < col + ship.size; j++) {
          if (newTiles[row][j] !== "empty") {
            canPlace = false;
            break;
          }
        }
      } else {
        for (let i = row; i < row + ship.size; i++) {
          if (newTiles[i][col] !== "empty") {
            canPlace = false;
            break;
          }
        }
      }

      // Place the ship if valid
      if (canPlace) {
        if (rotation === 0) {
          for (let j = col; j < col + ship.size; j++) {
            newTiles[row][j] = `ship-${ship.name}`;
          }
        } else {
          for (let i = row; i < row + ship.size; i++) {
            newTiles[i][col] = `ship-${ship.name}`;
          }
        }
        placedShips.push({ ...ship, row, col, rotation });
        placed = true;
      }
      attempts++;
    }
    if (!placed) {
      console.warn(`Could not place ship ${ship.name} after ${maxAttempts} attempts`);
    }
  }

  return newTiles;
}

// ------ GRID PC END

// ------ SCREEN START

// Endpoint to get actual screen
app.get('/api/screen', (req: Request, res: Response) => {
  fs.readFile(screenPath, 'utf8',  (err: NodeJS.ErrnoException | null, data: string) => {
    if (err) {
      console.log('Error reading screen:', err);
      return res.status(500).json({ success: false, message: 'Error reading screen data' });
    }
    res.json(JSON.parse(data));
  });
});

// Endpoint to update actual screen
app.post('/api/screen', (req: Request, res: Response) => {
  const { current_screen } = req.body;

  fs.writeFile(screenPath, JSON.stringify({ current_screen }, null, 2), (err) => {
    if (err) {
      return res.status(500).json({ success: false, message: 'Error updating screen data' });
    }
    res.json({ success: true });
  });
});

// ------ SCREEN END

// ------ SETTINGS START

// Endpoint to get settings
app.get('/api/settings', (req: Request, res: Response) => {
  fs.readFile(settingsPath, 'utf8', (err: NodeJS.ErrnoException | null, data: string) => {
    if (err) {
      console.log('Error reading settings:', err);
      return res.status(500).json({ success: false, message: 'Error reading settings' });
    }
    res.json(JSON.parse(data));
  });
});

// Endpoint to update settings
app.post('/api/settings', (req: Request, res: Response) => {
  const { selectedBoard, difficulty } = req.body;

  fs.readFile(settingsPath, 'utf8', (err: NodeJS.ErrnoException | null, data: string) => {
    if (err) {
      return res.status(500).json({ success: false, message: 'Error reading settings' });
    }

    let settings: {
      selectedBoard: string;
      difficulty?: string;
    } = JSON.parse(data);

    settings.selectedBoard = selectedBoard || settings.selectedBoard;
    if (difficulty !== undefined) {
      settings.difficulty = difficulty;
    }

    fs.writeFile(settingsPath, JSON.stringify(settings, null, 2), (err: NodeJS.ErrnoException | null) => {
      if (err) {
        return res.status(500).json({ success: false, message: 'Error updating settings' });
      }
      res.json(settings);
    });
  });
});

// Endpoint to set available ships
app.post('/api/set-available-ships', (req: Request, res: Response) => {
  fs.readFile(planningPath, 'utf-8', (err: NodeJS.ErrnoException | null, data: string) => {
    if (err) {
      return res.status(500).json({ success: false, message: 'Error reading planning data' });
    }

    let planningData: IPlanningData;
    try {
      planningData = JSON.parse(data);
    } catch (parseError) {
      console.error("Error parsing planning data:", parseError);
      return res.status(500).json({ error: "Invalid planning data format" });
    }

    planningData.available_ships = [];
    planningData.all_ships?.forEach((ship) => {
      const newAvailableShip: IShip = {
        id: ship.id,
        size: ship.size,
        color: ship.color,
        rotation: ship.rotation,
        name: ship.name
      }
      planningData.available_ships?.push(newAvailableShip);
    })

    fs.writeFile(planningPath, JSON.stringify(planningData, null, 2), (writeErr) => {
      if (writeErr) {
        console.error("Error removing active ship:", writeErr);
        return res.status(500).json({ error: "Could not remove active ship" });
      }
      res.status(200).json({ message: "Active ship removed successfully" });
    });
  });
});

// ------ SETTINGS END

// ------ AI SHOOTING START

// AI shooting state for medium/hard difficulty
// Note: This is a global state and works for single-player games.
// For multi-player support, this should be stored per game session.
interface AiState {
  lastHit?: { row: number; col: number };
  targetQueue: { row: number; col: number }[];
  huntMode: boolean;
}

const aiState: AiState = {
  targetQueue: [],
  huntMode: false
};

// Helper function: Get valid (untargeted) cells
function getValidCells(tiles: string[][], gridSize: number): { row: number; col: number }[] {
  const validCells: { row: number; col: number }[] = [];
  for (let r = 0; r < gridSize; r++) {
    for (let c = 0; c < gridSize; c++) {
      if (tiles[r][c] !== "hit" && tiles[r][c] !== "miss") {
        validCells.push({ row: r, col: c });
      }
    }
  }
  return validCells;
}

// Easy AI: Random shooting
function aiShootEasy(tiles: string[][], gridSize: number): { row: number; col: number } | null {
  const validCells = getValidCells(tiles, gridSize);
  if (validCells.length === 0) return null;
  return validCells[Math.floor(Math.random() * validCells.length)];
}

// Medium AI: Random + basic targeting after hit
function aiShootMedium(tiles: string[][], gridSize: number): { row: number; col: number } | null {
  const validCells = getValidCells(tiles, gridSize);
  if (validCells.length === 0) return null;

  // If we have targets in queue, shoot them
  while (aiState.targetQueue.length > 0) {
    const target = aiState.targetQueue.shift()!;
    if (tiles[target.row][target.col] !== "hit" && tiles[target.row][target.col] !== "miss") {
      return target;
    }
  }

  // Random shot
  return validCells[Math.floor(Math.random() * validCells.length)];
}

// Hard AI: Advanced targeting with prediction
function aiShootHard(tiles: string[][], gridSize: number): { row: number; col: number } | null {
  const validCells = getValidCells(tiles, gridSize);
  if (validCells.length === 0) return null;

  // If we have targets in queue, prioritize them
  while (aiState.targetQueue.length > 0) {
    const target = aiState.targetQueue.shift()!;
    if (tiles[target.row][target.col] !== "hit" && tiles[target.row][target.col] !== "miss") {
      return target;
    }
  }

  // Advanced: Use checkerboard pattern for efficiency
  const checkerboardCells = validCells.filter(cell => (cell.row + cell.col) % 2 === 0);
  if (checkerboardCells.length > 0) {
    return checkerboardCells[Math.floor(Math.random() * checkerboardCells.length)];
  }

  // Fallback to any valid cell
  return validCells[Math.floor(Math.random() * validCells.length)];
}

// Add adjacent cells to target queue when a hit is made
function addAdjacentTargets(row: number, col: number, gridSize: number): void {
  const adjacent = [
    { row: row - 1, col: col },
    { row: row + 1, col: col },
    { row: row, col: col - 1 },
    { row: row, col: col + 1 }
  ];

  for (const target of adjacent) {
    if (target.row >= 0 && target.row < gridSize && target.col >= 0 && target.col < gridSize) {
      aiState.targetQueue.push(target);
    }
  }
}

// Endpoint for AI shot
app.post("/api/ai-shot", (req: Request, res: Response) => {
  const { gridSize, tiles, difficulty }: IAiShotRequest = req.body;

  // Validate inputs
  if (gridSize == null || !tiles || !difficulty) {
    res.status(400).json({ error: "Missing required fields: gridSize, tiles, difficulty" });
  } else {
    let shot: { row: number; col: number } | null = null;

    // Select AI strategy based on difficulty
    switch (difficulty.toLowerCase()) {
      case "easy":
        shot = aiShootEasy(tiles, gridSize);
        break;
      case "medium":
        shot = aiShootMedium(tiles, gridSize);
        break;
      case "hard":
        shot = aiShootHard(tiles, gridSize);
        break;
      default:
        res.status(400).json({ error: "Invalid difficulty. Must be 'easy', 'medium', or 'hard'" });
        return;
    }

    if (!shot) {
      res.status(400).json({ error: "No valid cells to shoot" });
    } else {
      // Determine result - explicitly check if tile contains a ship
      const currentTile = tiles[shot.row][shot.col];
      const isShip = currentTile.startsWith("ship-") || (currentTile !== "empty" && currentTile !== "miss" && currentTile !== "hit");
      const result = isShip ? "hit" : "miss";

      // For medium/hard: Add adjacent targets when hit
      if ((difficulty.toLowerCase() === "medium" || difficulty.toLowerCase() === "hard") && result === "hit") {
        addAdjacentTargets(shot.row, shot.col, gridSize);
        aiState.lastHit = shot;
        aiState.huntMode = true;
      } else if (aiState.targetQueue.length === 0) {
        aiState.huntMode = false;
        aiState.lastHit = undefined;
      }

      const response: IAiShotResponse = {
        row: shot.row,
        col: shot.col,
        result: result
      };

      res.json(response);
    }
  }
});

// ------ AI SHOOTING END

// ------ PLANNING START

// Endpoint to get planning data
app.get('/api/planning', (req: Request, res: Response) => {
  fs.readFile(planningPath, 'utf8', (err: NodeJS.ErrnoException | null, data: string) => {
    if (err) {
      console.log('Error reading ships:', err);
      return res.status(500).json({ success: false, message: 'Error reading ships data' });
    }

    const planningData: IPlanningData = JSON.parse(data);
    return res.json(planningData);
  });
});

// Endpoint to get available ships
app.get('/api/planning/available-ships', (req: Request, res: Response) => {
  fs.readFile(planningPath, 'utf8', (err: NodeJS.ErrnoException | null, data: string) => {
    if (err) {
      console.error("Error reading planning data:", err);
      return res.status(500).json({ error: "Could not read planning data" });
    }

    let planningData: IPlanningData;
    try {
      planningData = JSON.parse(data);
      return res.json({ available_ships: planningData.available_ships });
    } catch (parseError) {
      console.error("Error parsing planning data:", parseError);
      return res.status(500).json({ error: "Invalid planning data format" });
    }
  });
});

// Endpoint to add or update placed ship (supports replacement/moving)
app.post('/api/planning/placed-ships', (req: Request, res: Response) => {
  const { ship, row, col } = req.body;

  fs.readFile(planningPath, 'utf8', (err: NodeJS.ErrnoException | null, data: string) => {
    if (err) {
      console.error("Error reading planning data:", err);
      return res.status(500).json({ error: "Could not read planning data" });
    }

    let planningData: IPlanningData;
    try {
      planningData = JSON.parse(data);
    } catch (parseError) {
      console.error("Error parsing planning data:", parseError);
      return res.status(500).json({ error: "Invalid planning data format" });
    }

    if (!planningData.placed_ships) {
      planningData.placed_ships = [];
    }

    const name = ship.name;
    let isNew = true;
    let oldRow = -1;
    let oldCol = -1;
    let oldRotation = 0;
    let oldSize = 0;

    // Check if ship is already placed (for moving/replacement)
    for (let i = 0; i < planningData.placed_ships.length; i++) {
      if (planningData.placed_ships[i].id === ship.id) {
        const oldShip = planningData.placed_ships[i];
        isNew = false;
        oldRow = oldShip.row;
        oldCol = oldShip.col;
        oldRotation = oldShip.rotation;
        oldSize = oldShip.size;

        // Clear old tiles (handle active- prefix if present)
        const tileValue = planningData.active_ship && planningData.active_ship.id === ship.id ? "active-" + name : name;
        if (oldRotation === 0) {
          for (let j = oldCol; j < oldCol + oldSize; j++) {
            if (planningData.player_grid.tiles[oldRow][j] === tileValue) {
              planningData.player_grid.tiles[oldRow][j] = "empty";
            }
          }
        } else {
          for (let k = oldRow; k < oldRow + oldSize; k++) {
            if (planningData.player_grid.tiles[k][oldCol] === tileValue) {
              planningData.player_grid.tiles[k][oldCol] = "empty";
            }
          }
        }

        // Update position and rotation
        planningData.placed_ships[i].row = row;
        planningData.placed_ships[i].col = col;
        planningData.placed_ships[i].rotation = ship.rotation;
        break;
      }
    }

    if (isNew) {
      // Create new placed ship
      const newPlacingShip: IPlacedShip = {
        id: ship.id,
        size: ship.size,
        color: ship.color,
        rotation: ship.rotation,
        name: ship.name,
        row: row,
        col: col
      };
      planningData.placed_ships.push(newPlacingShip);

      // Remove from available ships
      planningData.available_ships = planningData.available_ships?.filter(s => s.id !== ship.id) || [];
    }

    // Check if can place at new position (no overlap)
    let canPlace = true;
    if (ship.rotation === 0) {
      if (col + ship.size > planningData.player_grid.gridSize) {
        canPlace = false;
      } else {
        for (let j = col; j < col + ship.size; j++) {
          if (planningData.player_grid.tiles[row][j] !== "empty") {
            canPlace = false;
            break;
          }
        }
      }
    } else {
      if (row + ship.size > planningData.player_grid.gridSize) {
        canPlace = false;
      } else {
        for (let k = row; k < row + ship.size; k++) {
          if (planningData.player_grid.tiles[k][col] !== "empty") {
            canPlace = false;
            break;
          }
        }
      }
    }

    if (!canPlace) {
      // Restore old tiles if moving
      if (!isNew) {
        if (oldRotation === 0) {
          for (let j = oldCol; j < oldCol + oldSize; j++) {
            planningData.player_grid.tiles[oldRow][j] = name;
          }
        } else {
          for (let k = oldRow; k < oldRow + oldSize; k++) {
            planningData.player_grid.tiles[k][oldCol] = name;
          }
        }
      }
      return res.status(400).json({ error: "Cannot place ship: out of bounds or overlapping" });
    }

    // Set new tiles (without active- prefix)
    if (ship.rotation === 0) {
      for (let j = col; j < col + ship.size; j++) {
        planningData.player_grid.tiles[row][j] = name;
      }
    } else {
      for (let k = row; k < row + ship.size; k++) {
        planningData.player_grid.tiles[k][col] = name;
      }
    }

    // Deselect active ship
    planningData.active_ship = null;

    fs.writeFile(planningPath, JSON.stringify(planningData, null, 2), (writeErr) => {
      if (writeErr) {
        console.error("Error saving planning data:", writeErr);
        return res.status(500).json({ error: "Could not save planning data" });
      }
      res.status(200).json({ message: "Ship placed/updated successfully" });
    });
  });
});

// Endpoint to clear grid
app.post('/api/planning/clear-grid', (req: Request, res: Response) => {
  fs.readFile(planningPath, 'utf8', (err: NodeJS.ErrnoException | null, data: string) => {
    if (err) {
      console.error("Error reading planning data:", err);
      return res.status(500).json({ error: "Could not read planning data" });
    }

    let planningData: IPlanningData;
    try {
      planningData = JSON.parse(data);
    } catch (parseError) {
      console.error("Error parsing planning data:", parseError);
      return res.status(500).json({ error: "Invalid planning data format" });
    }

    // Clear tiles
    planningData.player_grid.tiles = planningData.player_grid.tiles.map(row => row.map(() => "empty"));

    // Restore available ships from placed
    if (planningData.available_ships && planningData.placed_ships) {
      planningData.placed_ships.forEach((ship: IPlacedShip) => {
        const availableShip: IShip = {
          id: ship.id,
          size: ship.size,
          color: ship.color,
          rotation: 0, // Reset rotation
          name: ship.name
        };
        planningData.available_ships?.push(availableShip);
      });
    }

    planningData.active_ship = null; // Reset active ship
    planningData.placed_ships = null; // Clear placed ships

    fs.writeFile(planningPath, JSON.stringify(planningData, null, 2), (writeErr) => {
      if (writeErr) {
        console.error("Error clearing grid:", writeErr);
        return res.status(500).json({ error: "Could not clear grid" });
      }
      res.status(200).json({ message: "Grid cleared successfully" });
    });
  });
});

// Endpoint that returns active ship
app.get('/api/planning/active-ship', (req: Request, res: Response) => {
  fs.readFile(planningPath, 'utf8', (err: NodeJS.ErrnoException | null, data: string) => {
    if (err) {
      console.error("Error reading planning data:", err);
      return res.status(500).json({ error: "Could not read planning data" });
    }
    let planningData: IPlanningData;
    try {
      planningData = JSON.parse(data);
      return res.json({ active_ship: planningData.active_ship });
    } catch (parseError) {
      console.error("Error parsing planning data:", parseError);
      return res.status(500).json({ error: "Invalid planning data format" });
    }
  });
});

// Endpoint to deselect active ship (remove active- prefix only)
app.post('/api/planning/remove-active-ship', (req: Request, res: Response) => {
  fs.readFile(planningPath, 'utf8', (err: NodeJS.ErrnoException | null, data: string) => {
    if (err) {
      console.error("Error reading planning data:", err);
      return res.status(500).json({ error: "Could not read planning data" });
    }

    let planningData: IPlanningData;
    try {
      planningData = JSON.parse(data);
    } catch (parseError) {
      console.error("Error parsing planning data:", parseError);
      return res.status(500).json({ error: "Invalid planning data format" });
    }

    if (planningData.active_ship === null) {
      return res.status(400).json({ error: "No active ship to deselect" });
    }

    const as = planningData.active_ship;
    const name = as.name;
    const prefix = "active-";
    const tileValue = prefix + name;

    // Remove active- prefix from tiles, restoring the original ship name
    if (as.rotation === 0) {
      for (let j = as.col; j < as.col + as.size; j++) {
        if (planningData.player_grid.tiles[as.row][j] === tileValue) {
          planningData.player_grid.tiles[as.row][j] = name;
        }
      }
    } else {
      for (let i = as.row; i < as.row + as.size; i++) {
        if (planningData.player_grid.tiles[i][as.col] === tileValue) {
          planningData.player_grid.tiles[i][as.col] = name;
        }
      }
    }

    // Clear active ship without modifying placed_ships or available_ships
    planningData.active_ship = null;

    fs.writeFile(planningPath, JSON.stringify(planningData, null, 2), (writeErr) => {
      if (writeErr) {
        console.error("Error deselecting active ship:", writeErr);
        return res.status(500).json({ error: "Could not deselect active ship" });
      }
      res.status(200).json({ message: "Active ship deselected successfully" });
    });
  });
});

// Endpoint to rotate active ship
app.post('/api/planning/rotate-active-ship', (req: Request, res: Response) => {
  fs.readFile(planningPath, 'utf8', (err: NodeJS.ErrnoException | null, data: string) => {
    if (err) {
      console.error("Error reading planning data:", err);
      return res.status(500).json({ error: "Could not read planning data" });
    }

    let planningData: IPlanningData;
    try {
      planningData = JSON.parse(data);
    } catch (parseError) {
      console.error("Error parsing planning data:", parseError);
      return res.status(500).json({ error: "Invalid planning data format" });
    }

    if (!planningData.active_ship) {
      return res.status(400).json({ error: "No active ship to rotate" });
    }

    const as = planningData.active_ship;
    const name = as.name;
    const prefix = "active-";
    const oldTileValue = prefix + name;

    const newRotation = as.rotation === 0 ? 90 : 0;
    let canBePlaced = true;

    // Check if can place in new rotation
    if (newRotation === 0) {
      if (as.col + as.size > planningData.player_grid.gridSize) {
        canBePlaced = false;
      } else {
        for (let j = as.col; j < as.col + as.size; j++) {
          if (planningData.player_grid.tiles[as.row][j] !== "empty" && planningData.player_grid.tiles[as.row][j] !== oldTileValue) {
            canBePlaced = false;
            break;
          }
        }
      }
    } else {
      if (as.row + as.size > planningData.player_grid.gridSize) {
        canBePlaced = false;
      } else {
        for (let i = as.row; i < as.row + as.size; i++) {
          if (planningData.player_grid.tiles[i][as.col] !== "empty" && planningData.player_grid.tiles[i][as.col] !== oldTileValue) {
            canBePlaced = false;
            break;
          }
        }
      }
    }

    if (!canBePlaced) {
      return res.status(400).json({ error: "Cannot rotate ship: out of bounds or overlapping" });
    }

    // Clear old tiles
    if (as.rotation === 0) {
      for (let j = as.col; j < as.col + as.size; j++) {
        planningData.player_grid.tiles[as.row][j] = "empty";
      }
    } else {
      for (let i = as.row; i < as.row + as.size; i++) {
        planningData.player_grid.tiles[i][as.col] = "empty";
      }
    }

    // Set new tiles with active- prefix
    const newTileValue = prefix + name;
    if (newRotation === 0) {
      for (let j = as.col; j < as.col + as.size; j++) {
        planningData.player_grid.tiles[as.row][j] = newTileValue;
      }
    } else {
      for (let i = as.row; i < as.row + as.size; i++) {
        planningData.player_grid.tiles[i][as.col] = newTileValue;
      }
    }

    // Update rotation
    as.rotation = newRotation;

    // Update rotation in placed_ships
    if (planningData.placed_ships) {
      for (let i = 0; i < planningData.placed_ships.length; i++) {
        if (planningData.placed_ships[i].id === as.id) {
          planningData.placed_ships[i].rotation = newRotation;
          break;
        }
      }
    }

    fs.writeFile(planningPath, JSON.stringify(planningData, null, 2), (writeErr) => {
      if (writeErr) {
        console.error("Error rotating active ship:", writeErr);
        return res.status(500).json({ error: "Could not rotate active ship" });
      }
      res.status(200).json({ message: "Active ship rotated successfully", active_ship: planningData.active_ship });
    });
  });
});

// Endpoint to get colors from all ships
app.get('/api/planning/colors', (req: Request, res: Response) => {
  fs.readFile(planningPath, 'utf8', (err: NodeJS.ErrnoException | null, data: string) => {
    if (err) {
      console.error("Error reading planning data:", err);
      return res.status(500).json({ error: "Could not read planning data" });
    }

    let planningData: IPlanningData;
    try {
      planningData = JSON.parse(data);
    } catch (parseError) {
      console.error("Error parsing planning data:", parseError);
      return res.status(500).json({ error: "Invalid planning data format" });
    }

    let colors: Record<string, string> = {};
    planningData.all_ships?.forEach((ship) => {
      colors[ship.name] = ship.color;
    });

    return res.json(colors);
  });
});

// Endpoint to restore available ships
app.post('/api/planning/restore-available-ships', (req: Request, res: Response) => {
  fs.readFile(planningPath, 'utf8', (err: NodeJS.ErrnoException | null, data: string) => {
    if (err) {
      console.error("Error reading planning data:", err);
      return res.status(500).json({ error: "Could not read planning data" });
    }

    let planningData: IPlanningData;
    try {
      planningData = JSON.parse(data);
    } catch (parseError) {
      console.error("Error parsing planning data:", parseError);
      return res.status(500).json({ error: "Invalid planning data format" });
    }

    planningData.placed_ships?.forEach((ship) => {
      const newShip: IShip = {
        id: ship.id,
        size: ship.size,
        color: ship.color,
        rotation: ship.rotation,
        name: ship.name,
      }
      planningData.available_ships?.push(newShip);
    })

    fs.writeFile(planningPath, JSON.stringify(planningData, null, 2), (writeErr) => {
      if (writeErr) {
        console.error("Error restoring available ships:", writeErr);
        return res.status(500).json({ error: "Could not restore available ships" });
      }
      res.status(200).json({ message: "Available ships restored successfully" });
    })
  });
});

// New endpoint to select a placed ship as active (for selection and visualization)
app.post('/api/planning/set-active-placed', (req: Request, res: Response) => {
  const { row, col } = req.body;

  fs.readFile(planningPath, 'utf8', (err: NodeJS.ErrnoException | null, data: string) => {
    if (err) {
      console.error("Error reading planning data:", err);
      return res.status(500).json({ error: "Could not read planning data" });
    }

    let planningData: IPlanningData;
    try {
      planningData = JSON.parse(data);
    } catch (parseError) {
      console.error("Error parsing planning data:", parseError);
      return res.status(500).json({ error: "Invalid planning data format" });
    }

    // Deselect current active ship if any (remove active- prefix)
    if (planningData.active_ship) {
      const as = planningData.active_ship;
      const name = as.name;
      const prefix = "active-";
      const tileValue = prefix + name;

      if (as.rotation === 0) {
        for (let j = as.col; j < as.col + as.size; j++) {
          if (planningData.player_grid.tiles[as.row][j] === tileValue) {
            planningData.player_grid.tiles[as.row][j] = name;
          }
        }
      } else {
        for (let i = as.row; i < as.row + as.size; i++) {
          if (planningData.player_grid.tiles[i][as.col] === tileValue) {
            planningData.player_grid.tiles[i][as.col] = name;
          }
        }
      }
      planningData.active_ship = null;
    }

    // Find the placed ship containing the clicked cell
    let selectedShip: IPlacedShip | null = null;
    for (const ship of planningData.placed_ships || []) {
      const isHorizontal = ship.rotation === 0;
      const startRow = ship.row;
      const startCol = ship.col;
      const endRow = isHorizontal ? startRow : startRow + ship.size - 1;
      const endCol = isHorizontal ? startCol + ship.size - 1 : startCol;

      if (row >= startRow && row <= endRow && col >= startCol && col <= endCol) {
        selectedShip = ship;
        break;
      }
    }

    if (selectedShip) {
      planningData.active_ship = selectedShip;
      const name = selectedShip.name;
      const tileValue = "active-" + name;

      if (selectedShip.rotation === 0) {
        for (let j = selectedShip.col; j < selectedShip.col + selectedShip.size; j++) {
          if (planningData.player_grid.tiles[selectedShip.row][j] === name) {
            planningData.player_grid.tiles[selectedShip.row][j] = tileValue;
          }
        }
      } else {
        for (let i = selectedShip.row; i < selectedShip.row + selectedShip.size; i++) {
          if (planningData.player_grid.tiles[i][selectedShip.col] === name) {
            planningData.player_grid.tiles[i][selectedShip.col] = tileValue;
          }
        }
      }
    }

    fs.writeFile(planningPath, JSON.stringify(planningData, null, 2), (writeErr) => {
      if (writeErr) {
        console.error("Error setting active ship:", writeErr);
        return res.status(500).json({ error: "Could not set active ship" });
      }
      res.status(200).json({ message: "Active ship set successfully", active_ship: planningData.active_ship });
    });
  });
});

// Endpoint to deselect the active ship (remove 'active-' prefix from grid tiles, set active_ship to null)
app.post('/api/planning/deselect-active', (req: Request, res: Response) => {
  fs.readFile(planningPath, 'utf8', (err: NodeJS.ErrnoException | null, data: string) => {
    if (err) {
      console.error("Error reading planning data:", err);
      return res.status(500).json({ error: "Could not read planning data" });
    }

    let planningData: IPlanningData;
    try {
      planningData = JSON.parse(data);
    } catch (parseError) {
      console.error("Error parsing planning data:", parseError);
      return res.status(500).json({ error: "Invalid planning data format" });
    }

    if (!planningData.active_ship) {
      return res.status(200).json({ message: "No active ship to deselect" });
    }

    const as = planningData.active_ship;
    const name = as.name;
    const prefix = "active-";
    const tileValue = prefix + name;

    if (as.rotation === 0) {
      for (let j = as.col; j < as.col + as.size; j++) {
        if (planningData.player_grid.tiles[as.row][j] === tileValue) {
          planningData.player_grid.tiles[as.row][j] = name;
        }
      }
    } else {
      for (let i = as.row; i < as.row + as.size; i++) {
        if (planningData.player_grid.tiles[i][as.col] === tileValue) {
          planningData.player_grid.tiles[i][as.col] = name;
        }
      }
    }
    planningData.active_ship = null;

    fs.writeFile(planningPath, JSON.stringify(planningData, null, 2), (writeErr) => {
      if (writeErr) {
        console.error("Error deselecting active ship:", writeErr);
        return res.status(500).json({ error: "Could not deselect active ship" });
      }
      res.status(200).json({ message: "Active ship deselected successfully" });
    });
  });
});

// ------ PLANNING END

// Endpoint for '/'
app.get('/', (req: Request, res: Response) => {
  res.send('Welcome to the Battleships Game Backend!');
});

app.listen(PORT, () => {
  console.log(`Server running on http://localhost:${PORT}`);
});

