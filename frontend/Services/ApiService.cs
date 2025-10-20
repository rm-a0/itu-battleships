using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BattleshipsAvalonia.Models;

namespace BattleshipsAvalonia.Services;

public class ApiService
{
    private static readonly HttpClient Client = new() { BaseAddress = new Uri("http://localhost:5000/") };
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    private async Task<T> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await Client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(json, Options);
            return result ?? throw new InvalidOperationException($"Failed to deserialize response from {endpoint}");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP error in GET {endpoint}: {ex.Message}");
            throw;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON deserialization error in GET {endpoint}: {ex.Message}");
            throw;
        }
    }

    private async Task PostAsync(string endpoint, object? body = null, bool silent = false)
    {
        try
        {
            var content = body != null
                ? new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
                : null;
            var response = await Client.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            if(!silent) Console.WriteLine($"HTTP error in POST {endpoint}: {ex.Message}");
            throw;
        }
    }

    public async Task<CurrentScreen> GetCurrentScreenAsync()
    {
        return await GetAsync<CurrentScreen>("api/screen");
    }

    public async Task SetCurrentScreenAsync(string screen)
    {
        await PostAsync("api/screen", new { current_screen = screen });
    }

    public async Task<Grid> GetPlayerGridAsync()
    {
        return await GetAsync<Grid>("api/player-grid");
    }

    public async Task UpdatePlayerGridAsync(Grid grid, string? mode = null)
    {
        await PostAsync("api/player-grid", new { player_grid = grid, mode });
    }

    public async Task<Grid> GetPcGridAsync()
    {
        return await GetAsync<Grid>("api/pc-grid");
    }

    public async Task UpdatePcGridAsync(int? gridSize = null, string[][]? tiles = null)
    {
        await PostAsync("api/pc-grid", new { gridSize, tiles });
    }

    public async Task<GameSettings> GetSettingsAsync()
    {
        return await GetAsync<GameSettings>("api/settings");
    }

    public async Task UpdateSettingsAsync(string selectedBoard)
    {
        await PostAsync("api/settings", new { selectedBoard });
    }

    public async Task<PlanningData> GetPlanningDataAsync()
    {
        return await GetAsync<PlanningData>("api/planning");
    }

    public async Task<List<Ship>> GetAvailableShipsAsync()
    {
        var data = await GetAsync<Dictionary<string, List<Ship>>>("api/planning/available-ships");
        return data["available_ships"] ?? new List<Ship>();
    }

    public async Task<PlacedShip?> GetActiveShipAsync()
    {
        var data = await GetAsync<Dictionary<string, PlacedShip?>>("api/planning/active-ship");
        return data["active_ship"];
    }

    public async Task<Dictionary<string, string>> GetShipColorsAsync()
    {
        return await GetAsync<Dictionary<string, string>>("api/planning/colors");
    }

    public async Task AddPlacedShipAsync(PlacedShip ship)
    {
        var body = new
        {
            ship = new Ship
            {
                Id = ship.Id,
                Size = ship.Size,
                Color = ship.Color,
                Rotation = ship.Rotation,
                Name = ship.Name
            },
            row = ship.Row,
            col = ship.Col
        };
        await PostAsync("api/planning/placed-ships", body);
    }

    public async Task SetAvailableShipsAsync()
    {
        await PostAsync("api/set-available-ships");
    }

    public async Task RemoveActiveShipAsync()
    {
        await PostAsync("api/planning/remove-active-ship");
    }

    public async Task RotateActiveShipAsync()
    {
        await PostAsync("api/planning/rotate-active-ship");
    }

    public async Task ClearGridAsync()
    {
        await PostAsync("api/planning/clear-grid");
    }

    public async Task RestoreAvailableShipsAsync()
    {
        await PostAsync("api/planning/restore-available-ships");
    }

    public async Task CreateGridsAsync(int gridSize)
    {
        var emptyGrid = new Grid
        {
            GridSize = gridSize,
            Tiles = new string[gridSize][]
                .Select(_ => new string[gridSize].Select(_ => "empty").ToArray())
                .ToArray()
        };
        await UpdatePlayerGridAsync(emptyGrid, mode: "reset");
        await UpdatePcGridAsync(gridSize, emptyGrid.Tiles);
    }

    public async Task<string[][]> UpdateGridCellAsync(int row, int col, string target, string[][] currentGrid)
    {
        if (target == "pc")
        {
            currentGrid[row][col] = currentGrid[row][col].StartsWith("ship") ? "hit" : "miss";
            await UpdatePcGridAsync(tiles: currentGrid);
        }
        else
        {
            currentGrid[row][col] = currentGrid[row][col].StartsWith("ship") ? "hit" : "miss";
            await UpdatePlayerGridAsync(new Grid { Tiles = currentGrid });
        }
        return currentGrid;
    }

    public bool CheckForWin(string[][] grid)
    {
        return grid.All(row => row.All(cell => !cell.StartsWith("ship")));
    }

    public async Task SetActiveOrPlaceShipAsync(int row, int col, PlacedShip? activeShip = null)
    {
        if (activeShip == null)
        {
            await PostAsync("api/planning/set-active-placed", new { row, col });
        }
        else
        {
            var body = new
            {
                ship = new Ship
                {
                    Id = activeShip.Id,
                    Size = activeShip.Size,
                    Color = activeShip.Color,
                    Rotation = activeShip.Rotation,
                    Name = activeShip.Name
                },
                row,
                col
            };
            try { await PostAsync("api/planning/placed-ships", body, silent: true); }
            catch { await PostAsync("api/planning/set-active-placed", new { row, col }); }
        }
    }

    public async Task DeselectActiveShipAsync()
    {
        await PostAsync("api/planning/deselect-active");
    }
}
