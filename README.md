# QuickPrompt

[![GitHub Release](https://img.shields.io/github/v/release/LeeorNahum/QuickPrompt?sort=semver)](https://github.com/LeeorNahum/QuickPrompt/releases/latest)

`qp` opens Claude Code in the folder you are looking at, with your question already sent. Type it into the File Explorer address bar:

```text
qp what does this folder do? tldr please
```

A terminal opens in that folder running Claude Code on Opus at medium effort, with the session named after the folder. Permission prompts are skipped, so use it only in folders you trust. `qp` on its own opens a session there without a prompt.

Everything after `qp` is the prompt. There are no flags. From the address bar or Win+R it is sent verbatim, including `&`, `|`, `%`, and quotes. From cmd, PowerShell, or Git Bash it works too, but the shell applies its own quoting rules first.

## Install

Requires Windows and [Claude Code](https://code.claude.com/docs/en/overview) installed natively, so that `claude.exe` is on your PATH.

Download `qp.exe` from the [latest release](https://github.com/LeeorNahum/QuickPrompt/releases/latest) into any folder on your PATH. Or build from source, which needs nothing beyond Windows:

```text
install.cmd
```

That compiles `qp.exe` and installs it to `%USERPROFILE%\.local\bin`, adding that folder to your PATH if it is not there yet.

## Set up with an AI coding agent

Paste this into Claude Code, Codex, or any coding agent:

> Install QuickPrompt from https://github.com/LeeorNahum/QuickPrompt: download `qp.exe` from the latest release into a folder on my PATH, confirm `claude.exe` is on my PATH too, then verify with `where.exe qp`.
