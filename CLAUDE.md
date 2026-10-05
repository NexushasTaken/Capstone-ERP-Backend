# Backend map (ASP.NET Core, namespace `ERP`)

Stack: EF Core + Npgsql (Postgres), FluentValidation, ML.NET (forecast). DI is in `Program.cs` (services and data are `AddScoped`, `ResponseHelper` is a singleton). Frontend rules live in `../Frontend/.claude/skills/frontend-conventions`.

## Layers

`Controllers/<Area>Controller/` → `Repository/Services/<Area>/` (logic, validation, audit log) → `Repository/Data/<Area>Data/` (EF queries) → `Repository/Model/` + `Repository/DatabaseContext.cs`.
Every service and data class has an interface under `Repository/Interface/`. View models and request DTOs are in `Repository/ViewModel/<Area>/`; validators in `Repository/Configuration/Validation/`.

## Conventions

- Controllers stay thin and return `StatusCode(200, _response.Status(200, true, "msg", content))`.
- Data classes extend `BaseData` (`BaseQuery<T>(withTracking)`, `Save`, `SaveChanges`).
- Entities extend `BaseModel` (audit columns + `IsActive` soft delete). Every list, count and aggregate query must filter `IsActive == true`.
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
- Before any commit, load the `Backend:changelog` skill (CHANGELOG entry, SemVer bump in `ERP.csproj`, Conventional Commit, tag).
