export interface IShip {
  id: string;
  size: number;
  color: string;
  rotation: number;
  name: string;
}

export interface IPlacedShip {
  id: string;
  size: number;
  color: string;
  rotation: number;
  name: string;
  // Additional properties for placed ships
  row: number;
  col: number;
}

export interface IPlanningData {
  player_grid: {
    gridSize: number;
    tiles: string[][];
  };
  all_ships: IShip[] | null,
  available_ships: IShip[] | null;
  placed_ships: IPlacedShip[] | null;
  active_ship: IPlacedShip | null;
}
