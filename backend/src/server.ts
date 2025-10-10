import express, { Request, Response } from 'express';
import fs from 'fs';
import cors from 'cors';
import path from 'path';
import { IPlanningData, IPlacedShip, IShip } from './data_interfaces';

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

// Endpoint to change pc grid
app.post("/api/pc-grid", (req: Request, res: Response) => {
  const { gridSize, tiles } = req.body;

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

    // Actualize only sended parts
    const updatedGrid = {
      gridSize: gridSize || currentGrid.gridSize,
      tiles: tiles || currentGrid.tiles
    };

    // Write new data to file
    fs.writeFile(pcGridPath, JSON.stringify(updatedGrid, null, 2), (writeErr) => {
      if (writeErr) {
        console.error("Error saving grid data:", writeErr);
        return res.status(500).json({ error: "Could not save grid data" });
      }
      res.status(200).json({ message: "Grid updated successfully", updatedGrid });
    });
  });
});

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
  const { selectedBoard } = req.body;

  fs.readFile(settingsPath, 'utf8', (err: NodeJS.ErrnoException | null, data: string) => {
    if (err) {
      return res.status(500).json({ success: false, message: 'Error reading settings' });
    }

    let settings: {
      selectedBoard: string;
    } = JSON.parse(data);

    settings.selectedBoard = selectedBoard || settings.selectedBoard;

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
  })
});

// ------ SETTINGS END

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

// Endpoint to add placed ship
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
    
    // create new placed ship object
    const newPlacingShip: IPlacedShip = {
      id: ship.id,
      size: ship.size,
      color: ship.color,
      rotation: ship.rotation,
      name: ship.name,
      row: row,
      col: col
    }

    planningData.placed_ships.push(newPlacingShip);

    // get and remove ship from available ships
    if (planningData.available_ships) {
      planningData.available_ships = planningData.available_ships.filter((s) => s.id !== ship.id);
    }

    fs.writeFile(planningPath, JSON.stringify(planningData, null, 2), (writeErr) => {
      if (writeErr) {
        console.error("Error saving placed ship:", writeErr);
        return res.status(500).json({ error: "Could not save placed ship" });
      }
      res.status(200).json({ message: "Placed ship added successfully", ship });
    });
  });
});

// Endpoint to set active ship
app.post('/api/planning/handle-active-ship', (req: Request, res: Response) => {
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

    if (planningData.player_grid.tiles[row][col] !== "empty") {
      const shipName = planningData.player_grid.tiles[row][col];
      const activeShip = planningData.active_ship;

      if (shipName === activeShip?.name) {
        // Unset active ship if clicked on the same ship
        planningData.active_ship = null;
      } else {
        planningData.active_ship = planningData.placed_ships?.find(ship => ship.name === shipName) || null;
        console.log(`active ship: ${planningData.active_ship?.name}`);
      }
      
      // Update info about active ship
      fs.writeFile(planningPath, JSON.stringify(planningData, null, 2), (writeErr) => {
        if (writeErr) {
          console.error("Error saving active ship:", writeErr);
          return res.status(500).json({ error: "Could not save active ship" });
        }
        res.status(200).json({ message: "Active ship set successfully", active_ship: activeShip });
      });
    }
  });
});

// Endpoint to remove ships from Grid
app.post('/api/clear-grid', (req: Request, res: Response) => {
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

    // Clear the grid
    for (let i = 0; i < planningData.player_grid.tiles.length; i++) {
      for (let j = 0; j < planningData.player_grid.tiles[i].length; j++) {
        planningData.player_grid.tiles[i][j] = "empty";
      }
    }

    // return ships from grid to available ships
    if (planningData.placed_ships) {
      planningData.available_ships = planningData.available_ships || [];
      planningData.placed_ships.forEach((ship: IPlacedShip) => {
        const availableShip: IShip = {
          id: ship.id,
          size: ship.size,
          color: ship.color,
          rotation: 0, // was ship.rotation
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
})

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

// ENdpoint to remove active ship
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
      return false;
    }

    // Remove active ship grid
    if (planningData.active_ship !== null) {
      if (planningData.active_ship.rotation === 0) {
        for (let j = planningData.active_ship.col; j < planningData.active_ship.col + planningData.active_ship.size; j++) {
          planningData.player_grid.tiles[planningData.active_ship.row][j] = "empty";
        }
      } else {
        // If vertical rotation
        for (let i = planningData.active_ship?.row; i < planningData.active_ship.row + planningData.active_ship.size; i++) {
          planningData.player_grid.tiles[i][planningData.active_ship.col] = "empty";
        }
      }
    }

    const newAvailableShip: IShip = {
      id: planningData.active_ship?.id || "",
      size: planningData.active_ship?.size || 0,
      color: planningData.active_ship?.color || "",
      rotation: 0,
      name: planningData.active_ship?.name || ""
    }
    planningData.available_ships?.push(newAvailableShip); // Add active ship to available ships

    planningData.placed_ships = planningData.placed_ships?.filter((ship: IPlacedShip) => ship.id !== planningData.active_ship?.id) || null; // Remove active ship from placed ships
    planningData.active_ship = null; // Clear active ship

    fs.writeFile(planningPath, JSON.stringify(planningData, null, 2), (writeErr) => {
      if (writeErr) {
        console.error("Error removing active ship:", writeErr);
        return res.status(500).json({ error: "Could not remove active ship" });
      }
      res.status(200).json({ message: "Active ship removed successfully" });
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
      return;
    }

    const direction = planningData.active_ship.rotation === 0 ? 90 : 0;
    let canBePlaced = true;

    // check if ship can be placed
    if (direction === 0) {
      if (planningData.active_ship.col + planningData.active_ship.size > planningData.player_grid.gridSize) {
        return;
      }

      for (let i = planningData.active_ship.col + 1; i < planningData.active_ship.col + planningData.active_ship.size; i++) {
        if (planningData.player_grid.tiles[planningData.active_ship.row][i] !== "empty") {
          canBePlaced = false;
          break;
        }
      }
    } else {
      if (planningData.active_ship.row + planningData.active_ship.size > planningData.player_grid.gridSize) {
        return;
      }

      for (let j = planningData.active_ship.row + 1; j < planningData.active_ship.row + planningData.active_ship.size; j++) {
        if (planningData.player_grid.tiles[j][planningData.active_ship.col] !== "empty") {
          canBePlaced = false;
          break;
        }
      }
    }

    if (!canBePlaced) {
      return;
    }

    // Remove old ship from grid
    if (planningData.active_ship.rotation === 0) {
      for (let j = planningData.active_ship.col; j < planningData.active_ship.col + planningData.active_ship.size; j++) {
        planningData.player_grid.tiles[planningData.active_ship.row][j] = "empty";
      }
    } else {
      for (let i = planningData.active_ship.row; i < planningData.active_ship.row + planningData.active_ship.size; i++) {
        planningData.player_grid.tiles[i][planningData.active_ship.col] = "empty";
      }
    }

    // Put new (rotated) ship
    if (direction === 0) {
      for (let i = planningData.active_ship.col; i < planningData.active_ship.col + planningData.active_ship.size; i++) {
        planningData.player_grid.tiles[planningData.active_ship.row][i] = planningData.active_ship.name;
      }
    } else {
      for (let j = planningData.active_ship.row; j < planningData.active_ship.row + planningData.active_ship.size; j++) {
        planningData.player_grid.tiles[j][planningData.active_ship.col] = planningData.active_ship.name;
      }
    }

    // set rotation in active ship
    planningData.active_ship.rotation = direction;
    
    // change direction in placed_ships
    if (planningData.placed_ships !== null) {
      for (let i = 0; i < planningData.placed_ships?.length; i++) {
        if (planningData.placed_ships[i].name === planningData.active_ship.name) {
          planningData.placed_ships[i].rotation = direction;
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
    })
  });
});

// Endpoint to get colors from available ships
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
        console.error("Error rotating active ship:", writeErr);
        return res.status(500).json({ error: "Could not rotate active ship" });
      }
      res.status(200).json({ message: "Available ships restored successfully" });
    })
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
