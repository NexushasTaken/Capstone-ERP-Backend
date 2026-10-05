# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
