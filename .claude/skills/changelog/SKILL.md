---
name: changelog
description: Versioning and commit rules for this repo — add a CHANGELOG.md entry, bump the SemVer version and write a Conventional Commit message. Use before EVERY commit in this repo, and whenever asked to log a change, bump the version or cut a release.
---

# Changelog, versioning and commits

Every change that reaches a commit follows three standards:

- Commit messages: [Conventional Commits 1.0.0](https://www.conventionalcommits.org/en/v1.0.0/)
- Version numbers: [Semantic Versioning 2.0.0](https://semver.org/)
- `CHANGELOG.md` at the repo root: [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/)

The changelog entry, the version bump and the code change go in **one commit**.

## 1. Commit message

```
<type>(<optional scope>)<optional !>: <description>

<optional body: why, not what>

<optional footers, e.g. BREAKING CHANGE: ..., Refs: ...>
```

- `description`: imperative, lowercase start, no trailing period, ≤ ~72 chars. E.g. `feat(accounts): page, search and sort accounts on the server`.
- Types: `feat` (new feature), `fix` (bug fix), `perf`, `refactor`, `revert`, `build`, `docs`, `style`, `test`, `ci`, `chore`.
- Scope is a short area name, matching earlier commits where possible (`accounts`, `products,categories`, `search`, `orders`, `inventory`, `theme`, `deps`, ...).
- Breaking change: add `!` before the colon (`feat(api)!: ...`) **and** a `BREAKING CHANGE: <what breaks and how to migrate>` footer.
- The version bump rides along in the change's own commit; there is no separate release commit. `chore(release): x.y.z` is only for a version change with no code change (e.g. the initial 1.0.0).
- End the message with any attribution trailer the session requires (e.g. `Co-Authored-By:`).

## 2. Which version bump

Versions are `MAJOR.MINOR.PATCH` and we are at ≥ 1.0.0, so breaking changes bump MAJOR.

| Commit | Bump | Changelog section |
|---|---|---|
| anything with `!` / `BREAKING CHANGE` | MAJOR | under its normal section, entry starts with `**BREAKING:**` |
| `feat` | MINOR | Added (new thing) or Changed (new behaviour of an existing thing) |
| `fix` | PATCH | Fixed (Security if it fixes a vulnerability) |
| `perf`, `refactor`, `revert`, `build` that changes runtime or dependencies | PATCH | Changed / Removed / Security, whichever fits |
| `docs`, `style`, `test`, `ci`, `chore`, `build` with no runtime effect | none | no entry, no bump, no tag |

- One commit with several changes → the **highest** bump wins; all entries go in that one version section.
- Bumping resets the parts to the right: `1.4.2` → minor → `1.5.0`; → major → `2.0.0`.
- Never reuse or skip a version. Read the current version from the version file (section 3), not from memory.

## 3. Where the version lives

- **Backend** (.NET): `<Version>x.y.z</Version>` in the first `<PropertyGroup>` of `ERP.csproj`.
- **Frontend** (Next.js): `"version"` in `package.json`. Bump with `npm version x.y.z --no-git-tag-version` so `package-lock.json` is updated too.

## 4. CHANGELOG.md format

```markdown
# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2026-10-06

### Added

- Export the orders table to CSV.

### Fixed

- Product search no longer ignores the selected category.

## [1.0.0] - 2026-10-05
...
```

- Newest version first, directly under the header. Date is today, `YYYY-MM-DD`.
- Only these subsections, in this order, and only the ones that have entries: **Added, Changed, Deprecated, Removed, Fixed, Security**.
- Entries are short, user-facing sentences: what changed for the person using the app or calling the API. No file names, no commit hashes, no "refactored X into Y" unless it changes behaviour.
- Backend entries name the affected endpoint when there is one, e.g. ``- `GET /api/User/accounts` supports `search`, `sortBy` and `sortDir`.``
- Never edit or delete entries in released versions, except to fix a typo.

## 5. Procedure (every commit)

1. Finish the change (and, in the Frontend, run the repo's format → `tsc` → lint → build checks).
2. Pick the commit type and scope (section 1) and the bump (section 2).
3. If the type has no bump: commit with the Conventional message. Done.
4. Otherwise compute the new version from the version file, then:
   - add a `## [x.y.z] - YYYY-MM-DD` section to the top of `CHANGELOG.md` with the entries;
   - write the new version into the version file (section 3).
5. Stage the change **plus** `CHANGELOG.md` and the version file(s), and make one commit.
6. Tag it: `git tag -a vx.y.z -m "vx.y.z"`. Don't push commits or tags unless the user asks.

If several commits are being made in a row, each releasing commit gets its own version and changelog section.
