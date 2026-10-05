# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
