# Battleships Game
## Overview
Battleships game implementation with a Node.js backend for game state management and a C# Avalonia frontend for the user interface. Designed for a user interface design class project.

## Table of Contents
- [Summary](#summary)
- [How to Use](#how-to-use)
  - [Requirements](#requirements)
  - [Installation](#installation)
- [Implementation Details](#implementation-details)
  - [Code Structure](#code-structure)
  - [Backend API](#backend-api)
  - [Frontend UI](#frontend-ui)

## Summary
This project implements a single-player Battleships game where the player places ships on a grid and battles against a computer opponent. The backend handles game logic and state persistence using JSON files, while the frontend provides an interactive desktop UI built with Avalonia.

**Key Features:**
- Grid-Based Gameplay: Customizable board sizes (6x6, 7x7, 8x8, 9x9)
- AI Difficulty Levels: Three AI opponents (Easy, Medium, Hard) with different shooting strategies
- Ship Placement: Interactive planning phase with rotation and validation
- Turn-Based Combat: Player and PC attacks with hit/miss tracking
- Surrender Options: Direct surrender or surrender with confirmation modal
- State Management: API endpoints for screens, grids, settings, and AI shots
- Simple Persistence: JSON files for game data

## How to Use

### Requirements
- Node.js (with npm)
- .NET SDK 9.0
- Git

### Installation
1. Clone the repository:
   git clone <repo URL>
   cd battleships_game

2. Install backend dependencies:
   cd backend
   npm install

3. Restore frontend dependencies:
   cd ../frontend
   dotnet restore

4. Run the backend (in backend folder):
   cd ../backend
   npm start

5. Build and run the frontend (in frontend folder, must specify framework):
   cd ../frontend
   dotnet build --framework net9.0
   dotnet run --framework net9.0

## Implementation Details

### Code Structure
```txt
backend/
├── data/
│   ├── curr_screen.json       # Current screen state
│   ├── game_settings.json     # Board size settings
│   ├── pc_grid.json           # PC grid data
│   ├── planning.json          # Ship planning data
│   └── player_grid.json       # Player grid data
└── src/
    ├── data_interfaces.ts     # TypeScript interfaces
    └── server.ts              # Express server and API

frontend/
├── Models/                    # Data models (Grid, Ship, etc.)
├── Services/                  # ApiService.cs - HTTP communication
├── ViewModels/                # MVVM view models
├── Views/                     # Avalonia windows and controls
└── Program.cs                 # Entry point + DI setup
```

### Backend API
Express server running on http://localhost:5000
Main endpoints:
- /api/screen            → current game screen
- /api/settings          → selected board size and AI difficulty
- /api/player-grid       → player grid state
- /api/pc-grid           → computer grid state
- /api/planning/*        → ship placement, active ship, rotation, colors
- /api/ai-shot           → AI shot calculation with difficulty-based strategies

All data persisted in JSON files inside backend/data/.

### Frontend UI
Avalonia .NET 9.0 desktop application using MVVM pattern:
- Main window → board size selection (6x6 / 7x7 / 8x8 / 9x9) and AI difficulty (Easy / Medium / Hard)
- Planning window → interactive ship placement with drag/rotate
- Game window → turn-based combat with two surrender options (direct and confirmed)
- All UI updates via async calls to the Node.js backend

### AI Difficulty Levels
- **Easy**: Random shooting across the board
- **Medium**: Random shooting with basic targeting of adjacent cells after a hit
- **Hard**: Advanced shooting with checkerboard pattern optimization and targeting after hits
