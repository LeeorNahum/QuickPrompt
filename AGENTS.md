# QuickPrompt

`qp.exe` is a Windows launcher that opens Claude Code in the current folder with the rest of the command line sent as the first prompt. README.md says what it does and how it is used.

## Layout

- `qp.cs` is the whole program in one file, written to C# 5 so the compiler that ships inside Windows (`csc.exe` under `%SystemRoot%\Microsoft.NET`) builds it with no SDK installed.
- `build.cmd` compiles to `dist\qp.exe`, stamping the version from `VERSION` into the exe's file properties. `dist/` is ignored.
- `install.cmd` builds, then copies the exe to `%USERPROFILE%\.local\bin` and adds that folder to the user PATH if missing.
- `.github/workflows/release.yml` builds on a `v*` tag and creates the GitHub release with `qp.exe` attached.

## Design rules

- It stays a compiled executable. A `.cmd` entry point runs through `cmd /c`, which cuts the prompt at `&`, breaks on `| < > ^`, and strips `%`. The prompt is read from the raw command line, never from `args[]`, so what was typed reaches Claude unchanged.
- Nothing is interpreted. Everything after `qp` is the prompt, passed after `--` so text that starts with `-` is not read as flags.
- The defaults are the product: Opus, medium effort, permissions skipped, session named after the folder. No flags, no environment variables, no config file. Someone who wants different defaults edits `qp.cs` and rebuilds.
- Single file, no dependencies, no installer framework.

## Releasing

Downloads come from GitHub releases. `install.cmd` installs the current checkout for local iteration. To release: bump `VERSION`, commit, tag `v<VERSION>`, push the tag. The workflow builds and publishes, and re-running it for the same tag replaces the asset. Follow the `release-versioning` skill.

## Standing rules

- Run `skill-sync` before every commit.
- No em dashes in new text (see `no-em-dashes`).
- Review user-facing copy with `anti-backrooms` before it ships.
