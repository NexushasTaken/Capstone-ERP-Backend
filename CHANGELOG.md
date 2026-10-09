# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [4.0.0] - 2026-10-09

### Added

- `GET /api/Dashboard/inventory/forecast/{productId}` returns one product's last 26 weeks of demand and its 4 forecast weeks, for the demand chart.

### Changed

- **BREAKING:** `GET /api/Dashboard/inventory/forecast` forecasts demand per product instead of a stock-out date per inventory item. Each row has `productId`, `name`, `expectedDemand`, `lowDemand` and `busyDemand` (next 4 weeks, 95% range), `stockOnHand` across all warehouses, `weeksLeft`, `runsOutAround`, `suggestedOrder`, `method`, `historyWeeks` and the product's backtest error percents. The page also returns `needOrderCount`, `accuracy` (AI vs. a simple 4-week average over the last 12 weeks) and `generatedAt`. Rows are ordered by the earliest run-out date.
- The forecast uses our own Singular Spectrum Analysis (SSA) with a yearly window for products with 116+ weeks of sales, and a 4-week average for newer products. In testing it was off by about ±25% against ±30% for the simple average.
- The forecast refreshes when it is more than a day old, and only one refresh runs at a time.

### Removed

- **BREAKING:** `inventoryId` and `earliestStockOutDay` in the forecast response. The `forecast per product` migration deletes the old forecast rows.
- The ML.NET packages.

### Fixed

- A forced forecast (`forceForecast=true`) no longer fails with a 500.
- A stale forecast is now regenerated; before, the age check was reversed.

## [3.0.0] - 2026-10-08

### Changed

- **BREAKING:** An inventory item is now a stock record: one per product per warehouse. `POST /api/Inventory/insert` rejects a product that is already stocked in the chosen warehouse.
- **BREAKING:** `POST /api/Inventory/insert` takes `productId`, `warehouseId`, `quantity` and `reorderPoint`. `name` and `dateArrived` are gone.
- **BREAKING:** `PATCH /api/Inventory/patch` takes only `id` and `reorderPoint`. The product and warehouse of an item can no longer be changed.
- `name` in `GET /api/Inventory/all`, movement velocity and the stock-out forecast is now the product name. Search and the A–Z / Z–A sorts use the product name.
- Inventory audit log messages name the item as "product at warehouse".

### Removed

- **BREAKING:** `dateArrived` in `GET /api/Inventory/all`, and its `dateFrom` / `dateTo` filters.

### Fixed

- Adding an inventory item now logs its opening stock in the movement history.
- Movement history rows now record the stock level after the change, so the forecast no longer reads them as zero stock.
- `PATCH /api/Inventory/patch` rejects a reorder point of 0.

## [2.6.0] - 2026-10-06

### Added

- `POST /api/Order/insert` accepts `discountPercent` (0 to 100), a percent discount on the order total. It defaults to 0.
- Order and sale list endpoints return `subtotal`, `discountPercent` and `discountAmount` for each order.

### Changed

- `total` on order and sale list endpoints is now the amount after the discount.
- Dashboard sales totals and growth are calculated after discounts.


## [2.5.0] - 2026-10-06

### Added

- Order list endpoints return `updated_At` for each order, the time its status last changed.

## [2.4.0] - 2026-10-06

### Added

- `GET /api/Product/all` returns `createdByName`, `updated_At` and `updatedByName` for each product, so clients can show who created and last updated it. Names of deleted accounts are still returned.

## [2.3.2] - 2026-10-06

### Security

- `GET /api/User/all` now requires authentication (`[Authorize]`). Previously it was accessible anonymously.

### Fixed

- `PATCH /api/Order/status/patch` now validates status transitions and rejects invalid ones: completed or cancelled orders cannot be changed, shipped orders cannot go back to processing, only delivery orders can be shipped, and duplicate status changes are rejected.

## [2.3.1] - 2026-10-06

### Security

- The Product, Category, Inventory, Order, Audit Log and Dashboard endpoints now require a signed-in user; requests without a valid `AccessToken` get 401.
- `/api/Sale/*` is limited to the `owner` role, as are creating, editing and deleting products, categories, inventory items, warehouses and delivery drivers, restocking and marking items as damaged. A `secretary` gets 403 on those.

## [2.3.0] - 2026-10-05

### Added

- `GET /api/Inventory/all` supports `warehouseId`, `categoryId`, `minQuantity`, `maxQuantity`, `dateFrom` and `dateTo` filters, and each item includes `categoryName`.

### Changed

- The inventory list keeps a stable order when items tie on the sort field, so paging no longer repeats or skips rows.

### Fixed

- `GET /api/Inventory/all` rejects a `page` or `pageSize` below 1 with a 400 instead of an error.
- `GET /api/Inventory/status/count` no longer counts deleted items.

## [2.2.0] - 2026-10-05

### Changed

- The `name` search on the Product, Category, Warehouse, Inventory, Order, Sale, Driver and Account lists now matches every text column shown in that list (e.g. a product's category, an inventory item's warehouse and status, an order's type, status and customer).
- Those searches also match the record's id, either as a bare number (`12`) or as shown in the list (`PR-12`, `INV-3`, `ORD-7`, `SAL-7`, `ACC-5`).

## [2.1.0] - 2026-10-05

### Added

- Validation failures (400) include an `errors` object mapping each rejected field to its messages, e.g. `{ "name": ["Product Name is required"], "orderLines.1.quantity": ["Quantity is required"] }`. Keys are camelCase paths matching the request JSON. `message` still holds the first error.
- All rejected fields are reported at once instead of only the first one.
- `POST /api/Order/insert` rejects an order with no order lines.

### Changed

- Model-binding failures (missing required query parameters, malformed JSON) use the same `{ status, success, message, errors }` body instead of ASP.NET's ProblemDetails.
- `POST /api/User/accounts` and the credentials update report a duplicate email under `errors.email`.

### Fixed

- Invalid `page`/`pageSize` on list endpoints, an empty `categoryName` on `POST /api/Category/insert` and an invalid id on category delete return 400 instead of 500.

## [2.0.0] - 2026-10-05

### Changed

- **BREAKING:** `GET /api/Inventory/warehouse/all` is now paged and sortable. It takes `page`, `pageSize`, `name` (case-insensitive search on name or address) and `filter` (0 newest first, 1 Id, 2 name A - Z, 3 name Z - A), and returns `{ warehouses, pageCount, rows }`. Each warehouse includes `stocks` (active item count) and `created_At`. The "All Warehouse Record" total row is no longer returned.
- **BREAKING:** `GET` dashboard inventory overview returns `totalStock` (total quantity of active inventory) instead of `totalWareHouseCapacity`.

### Removed

- **BREAKING:** Warehouse capacity. `POST /api/Inventory/warehouse/insert` and `PATCH /api/Inventory/warehouse/patch` no longer accept `capicity`, and the `Capacity` column is dropped from the database.
- Adding or moving inventory into a warehouse is no longer rejected with "Warehouse is full".

## [1.1.0] - 2026-10-05

### Added

- `DELETE /api/User/accounts/{id}` soft-deletes an account (owner only). The default administrator and the caller's own account cannot be deleted.

### Changed

- Deleted accounts no longer appear in `GET /api/User/accounts`, cannot log in, and lose their existing session on the next authorize check; their email can be reused for a new account. They still appear in the audit log user filter.

## [1.0.0] - 2026-10-05

First versioned release of the ERP API.

### Added

- JWT authentication: `POST /api/User/Login`, `POST /api/User/Logout` and `GET /api/User/authorize`.
- Account management: list (`GET /api/User/accounts`, with server-side paging, search and sort), create accounts and change roles; the default administrator's role is locked.
- Self-service profile and credential updates (`/api/User/me/profile`, `/api/User/me/credentials`); changing credentials requires the current password.
- Default accounts and lookup data (roles, statuses, types) seeded on startup, with identity sequences kept in sync.
- Products and categories: create, update, delete, products without a category, and server-side paging, case-insensitive search and sort.
- Inventory: stock levels, restocking, inventory transactions, marking items as damaged, damaged and movement item views, and movement velocity.
- Warehouses: create, update and delete.
- Orders: create, update, status changes and status counts, with paging and filtering.
- Drivers: create, update and delete, plus rider lookup.
- Sales views and the sales overview.
- Dashboard summaries and an inventory demand forecast (ML.NET time series).
- Audit logs for user actions, with filtering.

### Fixed

- Inventory, driver, sales and order search is case-insensitive.
