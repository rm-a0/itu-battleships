├── backend/            # Directory containing the TypeScript backend implemented by xpribik00 and his team.
│
├── frontend/           # Directory containing the Avalonia UI application. 
│   │                   # All non–auto-generated files in this directory were created by me.
│   │
│   ├── App.axaml       # Application-level configuration, global styles, and startup logic.
│   ├── App.axaml.cs
│   │
│   ├── Assets/         # Contains static frontend resources such as icons, logos, and images.
│   │
│   ├── Converters/     # Value converters used by the XAML Views. 
│   │                   # Mostly responsible for converting domain values into UI-friendly values such as colors or brushes.
│   │
│   ├── Models/         # Data models used primarily for API request/response encapsulation.
│   │
│   ├── Services/       # Reusable frontend logic.
│   │
│   ├── ViewModels/     # Classes implementing the MVVM pattern.
│   │
│   ├── Views/          # XAML user interface definitions with optional code-behind
│   │                   # Each View corresponds directly to a ViewModel
│   │
│   ├── Program.cs      # Entry point of the Avalonia application.
│   │
│   └── ViewLocator.cs  # Responsible for resolving Views for each ViewModel at runtime. 
│
└── itu-battleships.sln

Note: 
I'm the author of all non-auto-generated files in the frontend.
I'm NOT the author of backend.
To run build and run the project please refer to README.md or use the following commands:

# Terminal 1
cd backend
npm install
npm start

# Terminal 2
cd frontend
dotnet build --framework net9.0
dotnet run --framework net9.0
