# SuperAdventure

A Windows Forms RPG game built in C# where players can explore a world, battle monsters, complete quests, and manage their inventory.

## Features

- **World Exploration**: Navigate through different locations using cardinal directions (North, South, East, West)
- **Combat System**: Battle various monsters with different weapons and items
- **Inventory Management**: Collect, use, and manage various items and equipment
- **Quest System**: Complete quests to earn rewards
- **Save System**: Progress is automatically saved to XML files

## Project Structure

- `Engine/`: Core game logic and data structures
  - `Creatures/`: Player and monster classes
  - `Items/`: Game items and inventory system
  - `Quests/`: Quest-related functionality
  - `World/`: Location and world management
  - `Misc/`: Miscellaneous utilities

- `SuperAdventure/`: Windows Forms UI implementation
  - `SuperAdventure.cs`: Main game form and UI logic
  - `Program.cs`: Application entry point

## Requirements

- .NET Framework
- Windows OS (Windows Forms application)

## Getting Started

1. Clone the repository
2. Open `SuperAdventure.sln` in Visual Studio
3. Build and run the project
4. Start your adventure!

## License

See the [LICENSE](LICENSE) file for details.