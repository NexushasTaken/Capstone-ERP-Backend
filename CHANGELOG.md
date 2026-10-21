# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [16.0.0] - 2026-10-21

### Added

- Stock adjustments: `POST /api/Inventory/adjust` (owner only) takes `{ id, reasonId, quantity, note }`. Reasons: 1 damaged and 2 lost take `quantity` out, 3 found adds it, and 4 count correction takes the counted amount and saves the difference. The note is optional (up to 500 characters). Taking out more than is in stock, or a count that matches the stock, returns 400.
- `GET /api/Inventory/adjustment/item?id=` lists an item's adjustments, newest first, as `{ reasonId, reason, quantity, note, saleId, created_At }`. `quantity` is signed, and `saleId` is set for damaged customer returns.
- Adding a stock record (Add inventory or product import) saves an "opening stock" adjustment for its starting quantity.
- A `received` stock-movement label for stock that will come from supplier purchases.

### Changed

- **BREAKING:** stock-movement labels are now `sale` (was `purchase`, written when an order is completed), `return`, `adjustment` (was `restock`) and `received`. The `damage` label is gone, and its rows are now `adjustment`. The forecast still counts only sales and returns.
- **BREAKING:** audit action 7 is `Adjust` (was `CurrentItemDamage`).
- A damaged customer return still adds the units back as a return, then takes them out as a damaged adjustment linked to the return. The return reason becomes the note.
- Existing damage reports were moved into stock adjustments with the damaged reason.

### Removed

- **BREAKING:** `PATCH /api/Inventory/restock`, `PATCH /api/Inventory/markasdamage`, `GET /api/Inventory/damage/item` and `GET /api/Inventory/inventoryLabel/forInsert`. Use `POST /api/Inventory/adjust` and `GET /api/Inventory/adjustment/item`.

## [15.0.0] - 2026-10-17

### Changed

- **BREAKING:** `GET /api/Dashboard/inventory/forecast/{productId}` returns up to 52 weeks of `history` before the forecast (was 13), and each of `pastYears` has 65 `weeks`: the 52 weeks before the same start date that year and the 13 from it (was 13 and 13). `sameWeeksTotal` is unchanged. Clients that read `weeks` from index 0 must skip the first 39 weeks to get the old 26.

## [14.0.0] - 2026-10-16

### Removed

- **BREAKING:** `GET /api/Dashboard/sales`, the dashboard's sales overview (monthly totals and growth).
- **BREAKING:** `GET /api/Inventory/movement/velocity`, the Fast/Stable/Slow movement classification of inventory items.

## [13.0.0] - 2026-10-16

### Added

- A `Discounts` table with the discount choices and their rates: none (0%), senior (20%), pwd (20%), solo_parent (10%) and custom (no fixed rate).
- `GET /api/Order/discounts` lists them as `{ id, type, percent }`; `percent` is `null` for custom.

### Changed

- **BREAKING:** `POST /api/Order/insert` and `PUT /api/Order/put` take `discountId` instead of `discountType`. For every discount except custom, the saved `discountPercent` is copied from the `Discounts` table and the sent one is ignored. A custom discount must be more than 0, and only the owner can set one, as before. An unknown `discountId` returns 400.
- Orders in `GET /api/Order/all` and sales in `GET /api/Sale/all` carry `discountId` as well as `discountType`.
- Each order keeps its own `discountPercent`, so a rate change in the table never changes old orders. Existing orders were moved to the matching `Discounts` row.

## [12.0.0] - 2026-10-16

### Removed

- **BREAKING:** the discount ID number. Orders in `GET /api/Order/all` and sales in `GET /api/Sale/all` no longer carry `discountIdNumber`, and `POST /api/Order/insert` and `PUT /api/Order/put` ignore it. The column is dropped from the database.

## [11.1.0] - 2026-10-16

### Changed

- `discountIdNumber` is optional for the senior citizen, PWD and solo parent discounts on `POST /api/Order/insert` and `PUT /api/Order/put`. A blank one is saved as `null`.

## [11.0.0] - 2026-10-16

### Changed

- **BREAKING:** `POST /api/Order/insert` and `PUT /api/Order/put` take `discountType` (`none`, `senior`, `pwd`, `solo_parent` or `custom`) and `discountIdNumber`. Senior citizen and PWD discounts are fixed at 20% and solo parent at 10% (RA 9994, RA 10754, RA 11861), and they need an ID number. `none` must be 0%, and `custom` takes any percent above 0 up to 100. Clients that send only `discountPercent` get 400 unless it is 0.
- Only the owner can set a custom discount. A secretary can pick the presets, and can edit an order that has the owner's custom discount as long as they leave that discount unchanged; otherwise the API returns 400.
- Orders in `GET /api/Order/all` and sales in `GET /api/Sale/all` carry `discountType` and `discountIdNumber`. The order edit audit message names the discount type, e.g. "discount none → senior citizen 20%".
- Existing orders with a discount become `custom`; the rest become `none`.

## [10.0.0] - 2026-10-16

### Added

- `POST /api/Sale/return` (owner and secretary) records a customer return from a completed sale: `{ saleId, reason, items: [{ productId, quantity, conditionId }] }`. Any product and any quantity up to what is still unreturned can come back, over as many returns as needed; more than that returns 400 naming the product. Good units (`conditionId` 1) go back to stock with a Return movement; damaged units (2) get a Return movement, then a Damage movement and a damage record, so stock is unchanged. The refund is the units' share of what was paid, after the order discount. Each product is audit-logged under Orders as Stock returned or Damaged (return).
- `GET /api/Sale/returns?page&pageSize&name&from&to` lists returns newest first, with product, customer, quantity, condition, refund, reason and who processed it. `name` matches the product, the customer or a sale id like `SAL-12`; `from`/`to` are Philippine days on the return date.

### Changed

- **BREAKING:** the Sales endpoints are open to the secretary role too; `GET /api/Sale/report` stays owner only.
- Sales in `GET /api/Sale/all` carry `refunded` and `totalAfterRefunds`, and each product carries `productId`, `returnedQuantity` and `soldUnitPrice`. `totalAmount` is now the total after refunds.
- The dashboard sales overview subtracts refunds, counted in the month of the sale so it matches the Sales list.
- The sales report PDF adds Refunded and After refunds to the summary and tables, and its Gross / Net total labels are now Subtotal / Total.

### Removed

- **BREAKING:** `restockType` on `PATCH /api/Inventory/restock` and `damagedType` on `PATCH /api/Inventory/markasdamage`. A restock is always stock received, and marking as damaged always takes the units out of stock. Customer returns are recorded with `POST /api/Sale/return`.

## [9.2.0] - 2026-10-16

### Added

- `POST /api/Category/import`, `POST /api/Product/import` and `POST /api/Order/driver/import` (owner only) add many rows at once from `{ rows: [...] }`, up to 1000 per request. Every row is checked first; if any row fails, nothing is saved and the 400 lists errors as `rows.N.<field>`. Each added row gets its own audit log entry.
- Product import rows take a category by name (matched ignoring case, blank means none) and an optional `quantity` + `reorderPoint`; when both are given, the product gets its stock record and a Restock movement for the opening stock, the same as `POST /api/Inventory/insert`.
- Import rejects names that already exist or repeat within the file, and product rows whose category doesn't exist.
- `POST /api/Category/import/check` and `POST /api/Product/import/check` (owner only) run the existing-name, repeated-name and unknown-category checks without saving, so a file can be checked as soon as it is picked.

## [9.1.0] - 2026-10-16

### Added

- `GET /api/Sale/report?period=daily|weekly|monthly&date=yyyy-MM-dd` downloads a sales report PDF (owner only). Daily lists each sale of that day; weekly has one row per day of its Monday-to-Sunday week; monthly has one row per week of its month. Every report has a summary (sales, gross, discounts, net total, average sale) and the top 10 products by revenue. `date` is a Philippine day and defaults to today; any other `period` returns 400.

## [9.0.0] - 2026-10-16

### Changed

- **BREAKING:** An inventory item is one stock record per product (the store is the only stock location). Adding a second active record for the same product returns 400 "This product already has a stock record."
- **BREAKING:** `POST /api/Inventory/insert` no longer takes `warehouseId`.
- **BREAKING:** `GET /api/Inventory/all` no longer takes `wareHousePresent` or `warehouseId`, no longer returns `warehouseId` / `warehouseName`, and its search no longer matches warehouse names. Sort `6` (warehouse) is gone.
- **BREAKING:** The movement velocity list no longer returns `warehouse`.
- Order stock shortage messages no longer name a warehouse.

### Removed

- **BREAKING:** The Warehouse module: `GET /api/Inventory/warehouse/all`, `POST /api/Inventory/warehouse/insert`, `PATCH /api/Inventory/warehouse/patch`, `DELETE /api/Inventory/warehouse/delete`, the `Warehouses` table, `Inventories.WarehouseId` and the `Warehouse` audit module (6). Run `dotnet ef database update` on an emptied database, then re-seed with `scripts/main.py`; the new unique index fails if a product still has several active stock records.
- **BREAKING:** The order pickup address: `pickUpAddress` is gone from order create, edit, list and details, from sale details, and from the `Orders` table.

## [8.1.0] - 2026-10-15

### Added

- `GET /api/Sale/range` returns the first and last day (Philippine time) that has a sale, or nulls when there are none, so the Sales date filter can limit its calendar.

## [8.0.1] - 2026-10-15

### Fixed

- `GET /api/Dashboard/sales` includes the `to` day. Before, the range stopped the day before, so the default range (the 1st of this month as `to`) left the current month out.
- `GET /api/Dashboard/sales` lists months in date order across years (November 2025 before January 2026). Before, it sorted by month number only.
- The growth figure of `GET /api/Dashboard/sales` compares against the same number of days just before `from`, now that `to` is included.

## [8.0.0] - 2026-10-15

### Added

- A `Sales` table: one row per completed order, written when the order is completed through `PATCH /api/Order/status/patch`, with when it sold (`Created_At`), who completed it (`Created_By`) and the amount charged after discount (`Total`). The migration fills it for orders that are already completed. Run `dotnet ef database update`.
- `GET /api/Sale/all` takes `from` and `to` (`yyyy-MM-dd`, both inclusive, Philippine days) and returns 400 when `from` is after `to`.
- `GET /api/Sale/all` returns `totalAmount`, the sum of every sale matching the filters (not just the page).

### Changed

- **BREAKING:** `GET /api/Sale/all` sorts with new `filter` codes: 0 newest first, 1 oldest first, 2 total high to low, 3 total low to high, 4 customer A to Z, 5 customer Z to A. The old product and quantity codes are gone (they never took effect).
- **BREAKING:** A sale's `id` in `GET /api/Sale/all` is now the sale's own id, and `SAL-n` search matches it; it used to be the order id.
- A sale's date is when the order was completed, not when it was created.
- `GET /api/Dashboard/sales` reads the `Sales` table and counts days and months in Philippine time, so a month's total matches the Sales list for that month.

### Fixed

- Searching `GET /api/Sale/all` by product name returns each matching sale with all of its products and its full total. Before, it dropped the sale's other products and showed a smaller subtotal.

## [7.1.0] - 2026-10-13

### Changed

- The demand forecast (`GET /api/Dashboard/inventory/forecast` and the chart and backtest endpoints) now uses ML.NET's SSA (`ForecastBySsa`, window 52, rank 5) instead of our own SSA. It is fed a 4-week average of the weekly demand and runs with stabilizing off; the 95% range is still the spread of real weeks around the smoothed line. On the 5-year seed backtest (13 tests of 12 hidden weeks) it misses by 23.5%, against 24.5% for our previous SSA and 31.3% for the 4-week average. Responses are unchanged.

## [7.0.2] - 2026-10-13

### Fixed

- `POST /api/Order/insert` rejects an order when a product doesn't have enough stock ("Not enough stock for …: n available, m needed") and saves nothing. Before, it saved the order for the units available, emptied the stock and returned 200.
- `POST /api/Category/insert` and `PATCH /api/Category/patch` reject a name another category already has ("A category with this name already exists."), ignoring case.
- `POST /api/Product/insert` and `PATCH /api/Product/patch` reject a name another product already has ("A product with this name already exists."), ignoring case.
- `PATCH /api/Inventory/restock` rejects a quantity of 0 or less. Before, a negative quantity lowered the stock.

## [7.0.1] - 2026-10-13

### Fixed

- `IsActive` now defaults to `true` in the database (it defaulted to `false` since 6.3.2) on categories, products, warehouses, inventories, drivers and user accounts. Run `dotnet ef database update`; existing rows are unchanged.

## [7.0.0] - 2026-10-12

### Changed

- **BREAKING:** `GET /api/Order/riders` is now `GET /api/Order/driver/active`. It still returns the active drivers for the order form.
- **BREAKING:** `POST /api/Order/insert` and `PUT /api/Order/put` take `deliveryDriverId` instead of `deliveryRiderId`, the same name the order responses already use.

## [6.3.3] - 2026-10-12

### Changed

- The orders table's misspelled `PibkupAddress` column is renamed to `PickUpAddress`. Run `dotnet ef database update`; the migration renames the column in place, so existing addresses are kept. Responses are unchanged (`pickUpAddress`).

## [6.3.2] - 2026-10-12

### Changed

- `IsActive` is now required (`true` or `false`, never empty) on categories, products, warehouses, inventories, drivers and user accounts. Run `dotnet ef database update`; rows with an empty `IsActive` were already hidden and become `false`. Responses are unchanged.

## [6.3.1] - 2026-10-12

### Removed

- Unused soft-delete columns (`IsActive`, `Deleted_By`, `Deleted_At`) from orders and user information. Orders are never deleted (cancelling is a status), and a user's information follows its account, whose own `IsActive` is unchanged. Run `dotnet ef database update`; responses are unchanged.

## [6.3.0] - 2026-10-12

### Changed

- `PUT /api/Order/put` writes an audit message that lists each change: products added or removed with their quantity, quantity changes (`5 → 2`) and the discount change. It used to show only the number of items.
- Order lines removed by an order edit are now deleted instead of hidden. Run `dotnet ef database update`; the migration deletes lines that earlier edits had hidden and drops the order lines' `IsActive`, `Deleted_By` and `Deleted_At` columns. Responses are unchanged.

## [6.2.2] - 2026-10-12

### Fixed

- `GET /api/Dashboard/sales` growth now compares completed sales with completed sales. The previous period also counted processing, shipped and cancelled orders, which made growth look lower than it was.

## [6.2.1] - 2026-10-12

### Removed

- The `Sales` table. It was written when an order completed but never read; sales figures already come from completed orders. No API changes.
- Unused audit columns from tables that never use them: lookup tables (order types/statuses, inventory statuses/labels, user roles) and forecast weeks keep only `Id`; forecast results keep `Id` and `Created_At`; inventory transactions and damage reports keep `Created_By`/`Created_At` but drop the update/delete columns and `IsActive`. Run `dotnet ef database update`; responses are unchanged.

## [6.2.0] - 2026-10-12

### Added

- `GET /api/Inventory/insert/product/all` returns `stock` for each product: its total quantity across all warehouses' active stock records.

## [6.1.0] - 2026-10-12

### Added

- `PUT /api/Order/put` edits a _Processing_ order: its items (add, remove, change quantity), discount, driver and pickup/delivery addresses. Order type and customer cannot change. Stock moves right away: removed or lowered quantities go back to the warehouse they came from, and added quantities are deducted like a new order. If any added quantity is short of stock, the whole edit is rejected with a 400 that lists the short products. Orders in any other status return a 400. Each edit is written to the audit log.
- `GET /api/Order/all` returns `deliveryDriverId` for each order and `productId` for each order line.

### Changed

- Order lines removed by an edit are soft-deleted and are left out of the order list and count, sales, the dashboard, and the stock returned when an order is cancelled.

## [6.0.0] - 2026-10-10

### Changed

- **BREAKING:** The forecast's accuracy fields are named after the method instead of "AI": `aiErrorPercent` is now `ssaErrorPercent` in every row and in `accuracy` of `GET /api/Dashboard/inventory/forecast`, in `GET /api/Dashboard/inventory/forecast/{productId}`, and in `GET /api/Dashboard/inventory/forecast/{productId}/backtest`. Clients must read the new name. The stored results keep their values; the database column is renamed by a migration.

## [5.1.0] - 2026-10-10

### Added

- `GET /api/Dashboard/inventory/forecast/{productId}/backtest` runs one product's backtest with a chosen length and end point: `hiddenWeeks` (4, 8, 12, 26 or 52; default 12) and `endWeeksAgo` (0, 13, 26 or 39; default 0). It returns each hidden week's real sales, the AI's forecast with its 95% range and the simple 4-week guess, up to 13 weeks before them, the weeks the AI learned from, and both error percents. `testable` is false when there are fewer than 104 weeks before the hidden ones. Other values return 400.

## [5.0.0] - 2026-10-09

### Changed

- **BREAKING:** `GET /api/Dashboard/inventory/forecast/{productId}` compares the forecast with the same season in earlier years. It returns the 13 weeks before the forecast and `pastYears`, newest first: for each earlier year, 26 weeks of demand lined up 13 weeks either side of the forecast start (52-week steps; `null` before the product's history) and `sameWeeksTotal`, the demand in the 4 weeks matching the forecast.

### Removed

- **BREAKING:** the `weeks` query parameter and `lastYear` in the demand chart response.

## [4.1.0] - 2026-10-09

### Added

- `GET /api/Dashboard/inventory/forecast` supports `search` (product name) and `needOrderOnly`. `needOrderCount` and `accuracy` still cover every product.
- `GET /api/Dashboard/inventory/forecast/{productId}` supports `weeks` (default 26; `0` shows the whole history) and returns `lastYear`: the demand 52 weeks before each shown and forecast week.

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
