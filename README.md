# InsTech Frontend Coding Task using Blazor - Solution

## How to run
First checkout the repo and enter the root folder of this repo using some CLI.

To run the app:
```bash
$> cd src
$> cd ins-tech-frontend-coding-task-blazor
$> dotnet run
```
In the browser go to http://localhost:5238/

To run the tests:
```bash
$> cd tests
$> cd ins-tech-frontend-coding-task-blazor.Tests
$> dotnet test
```

## User guide
- Drag&Drop vessel from the 'Vessels pool' to 'Anchorage' (from right to left)
- To rotate the vessel double-click it in the 'Vessels pool'
- To remove vessel from the 'Anchorage' back to the 'Vessels pool' drag&drop it somewhere outside the 'Anchorage' grid.

## Developers guide (Project structure)
- `src/ins-tech-frontend-coding-task-blazor/Pages/Index.razor` - implements the main page
- `src/ins-tech-frontend-coding-task-blazor/Components/AnchorageArea.razor` - implements Anchorage grid component
- `src/ins-tech-frontend-coding-task-blazor/Components/VesselPool.razor` - implements the list of all vessels 
- `src/ins-tech-frontend-coding-task-blazor/Components/VesselCard.razor` - implements individual vessel card component
- `src/ins-tech-frontend-coding-task-blazor/Services/BoardState.cs` - implements game core logic and state between components
- `src/ins-tech-frontend-coding-task-blazor/Services/FleetApiClient.cs` - implements api service. I use public proxy to avoid CORS issues - so the url I call is https://proxy.corsfix.com/?https://esa.instech.no/api/fleets/random instead of direct one https://esa.instech.no/api/fleets/random