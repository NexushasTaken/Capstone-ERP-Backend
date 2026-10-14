# Backend map (ASP.NET Core, namespace `ERP`)

Stack: EF Core + Npgsql (Postgres), FluentValidation, ML.NET's `ForecastBySsa` for the forecast, called from `Repository/Services/SSA/SsaForecaster.cs` (tuned: smoothed input, stabilizing off; the earlier textbook SSA lives in git history before 7.1.0). DI is in `Program.cs` (services and data are `AddScoped`, `ResponseHelper` is a singleton). Frontend rules live in `../Frontend/.claude/skills/frontend-conventions`.

## Layers

`Controllers/<Area>Controller/` → `Repository/Services/<Area>/` (logic, validation, audit log) → `Repository/Data/<Area>Data/` (EF queries) → `Repository/Model/` + `Repository/DatabaseContext.cs`.
Every service and data class has an interface under `Repository/Interface/`. View models and request DTOs are in `Repository/ViewModel/<Area>/`; validators in `Repository/Configuration/Validation/`.

## Conventions

- Controllers stay thin and return `StatusCode(200, _response.Status(200, true, "msg", content))`.
- Data classes extend `BaseData` (`BaseQuery<T>(withTracking)`, `Save`, `SaveChanges`).
- Entities pick a base from `Repository/Model/BaseModel.cs`: `KeyModel` (Id only: lookups, forecast tables), `CreatedModel` (+ `Created_By`/`Created_At`: append-only rows like `InventoryTransaction`, `DamagedInventory`), `UpdatedModel` (+ `Updated_By`/`Updated_At`: edited rows that are never retired on their own: `Order` (cancelling is a status), `UserInformation` (follows its account), and `OrderLine` (hard-deleted when removed)), or `BaseModel` (+ delete audit columns and `IsActive` soft delete: things retired in real life but still referenced by history, like drivers, products, warehouses). Every list, count and aggregate query on a `BaseModel` entity must filter `IsActive` (a non-nullable `bool`, `true` by default).
- A sale is a `Sale` row (`CreatedModel`), written by `OrderService.UpdateOrderStatus` when an order is completed (terminal, so never edited): `Created_At` = sold at, `Created_By` = who completed it, `Total` = amount charged net of discount. Sales figures (Sales list, dashboard chart) read `Sale`; line details come from its order's `OrderLine`s.
- Services log with `_auditLog.Log(AuditModuleEnum.X, AuditActionEnum.Y, ...)` and set `Created_By` / `Updated_By` from `_auditLog.CurrentUserId`.
- Paging: call `PageQueryValidator.Ensure(page, pageSize)` in the service. Page responses carry the items, `PageCount` and `Rows`.
- Search: `SearchPattern.Contains` (ILike) and `SearchPattern.TryParseId` for `INV-12` style ids.
- Sort: an int param mapped through an enum or `switch`, always ending in `ThenBy(Id)` so paging is stable.
- Filters: guarded `if (x > 0) query = query.Where(...)` inside one `FilteringQuery` that both the list and the count query call.
- Auth: `[Authorize(Roles = "owner")]`; role values are lowercase.
- `DateTime` filters must be `DateTimeKind.Utc` for Npgsql.

## Copy these when adding something

| Task | Model files |
|---|---|
| Filtered, sorted, paged list | `InventoryController.GetInventory`, `InventoryService.GetInventories`, `InventoryData.FilteringQuery`; simpler: `AuditLogData.FilteringQuery` |
| CRUD with validator + audit log | `CategoryController` + `CategoryData`, or the warehouse methods in `InventoryService` |
| New column or index | edit the model / `DatabaseContext.cs`, then `dotnet ef migrations add "<lowercase words>"` |
| Lookup seed values | "Seeding Default Values" in `DatabaseContext.cs`; enums in `Repository/Configuration/Enum/GlobalEnum.cs` |
| Status from stock level | `Repository/Configuration/Helper/ReorderRatio.cs` |

## Commands and workflow

- `dotnet build`; `dotnet ef migrations add <name>`. Ask before `dotnet ef database update`.
- `../scripts/main.py` seeds Postgres directly (not through EF).
- **Before any commit:** run `dotnet csharpier format . && dotnet build`, then load the `Backend:changelog` skill (CHANGELOG entry, SemVer bump in `ERP.csproj`, Conventional Commit, tag).
