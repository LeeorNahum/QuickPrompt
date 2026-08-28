// qp: open Claude Code in the current folder with the rest of the command
// line as the first prompt.
//
//     qp what does this folder do? tldr please
//
// The prompt is taken from the raw command line rather than args[], so
// what the user typed reaches Claude unchanged. See AGENTS.md for the
// design rules. Build with build.cmd.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

internal static class Qp
{
    private static int Main()
    {
        // Decide this before Claude runs: once it exits, only the launcher
        // is attached either way.
        bool holdWindow = OwnsVisibleConsole();

        string cwd = Directory.GetCurrentDirectory();
        string exe = ClaudePath();
        if (exe == null)
        {
            Console.Error.WriteLine("qp: claude.exe was not found on PATH. Install Claude Code natively first.");
            Hold(holdWindow);
            return 9009;
        }

        // Ctrl+C belongs to Claude. The launcher just waits.
        Console.CancelKeyPress += delegate(object s, ConsoleCancelEventArgs e) { e.Cancel = true; };

        int code;
        try
        {
            var psi = new ProcessStartInfo(exe, Arguments(RawPrompt(), SessionName(cwd)));
            psi.UseShellExecute = false;
            psi.WorkingDirectory = cwd;
            using (Process p = Process.Start(psi))
            {
                p.WaitForExit();
                code = p.ExitCode;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("qp: could not start " + exe + ": " + ex.Message);
            code = 9009;
        }

        Hold(holdWindow);
        return code;
    }

    private static string Arguments(string prompt, string sessionName)
    {
        var argv = new List<string>();
        argv.Add("--dangerously-skip-permissions");
        argv.Add("--model");
        argv.Add("opus");
        argv.Add("--effort");
        argv.Add("high");
        argv.Add("-n");
        argv.Add(sessionName);
        if (prompt.Length > 0)
        {
            // Without the separator a prompt that starts with '-' is read
            // as flags.
            argv.Add("--");
            argv.Add(prompt);
        }
        return Join(argv);
    }

    // Everything after the program name, verbatim. The program name ends at
    // the first whitespace outside quotes, which is how the C runtime finds
    // argv[0], so partially quoted paths like "C:\a b"\qp.exe parse too.
    private static string RawPrompt()
    {
        string line = Environment.CommandLine;
        int i = 0;
        while (i < line.Length && char.IsWhiteSpace(line[i])) i++;
        bool quoted = false;
        while (i < line.Length)
        {
            char c = line[i];
            if (c == '"') quoted = !quoted;
            else if (!quoted && char.IsWhiteSpace(c)) break;
            i++;
        }
        while (i < line.Length && char.IsWhiteSpace(line[i])) i++;
        return line.Substring(i);
    }

    private static string SessionName(string cwd)
    {
        string trimmed = cwd.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string leaf = Path.GetFileName(trimmed);
        if (leaf.Length > 0) return leaf;
        if (trimmed.Length > 0) return trimmed;
        return "qp";
    }

    // First claude.exe found on PATH. Relative entries such as "." are
    // skipped so the folder being viewed can never supply the executable.
    private static string ClaudePath()
    {
        string path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(path)) return null;

        foreach (string dir in path.Split(';'))
        {
            string d = dir.Trim().Trim('"');
            if (d.Length == 0) continue;
            try
            {
                if (!Path.IsPathRooted(d)) continue;
                string candidate = Path.Combine(d, "claude.exe");
                if (File.Exists(candidate)) return candidate;
            }
            catch (ArgumentException) { }
        }
        return null;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint GetConsoleProcessList(uint[] processList, uint processCount);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    // True when this process is the only one attached to its console and a
    // person can see it, which is the case when Explorer or Win+R created
    // the window. Then the window would vanish the moment Claude exits, so
    // the launcher holds it. A shell, a pipe, or a hidden window never holds.
    private static bool OwnsVisibleConsole()
    {
        try
        {
            if (Console.IsInputRedirected) return false;
            if (GetConsoleProcessList(new uint[2], 2) != 1) return false;
            IntPtr window = GetConsoleWindow();
            return window != IntPtr.Zero && IsWindowVisible(window);
        }
        catch
        {
            return false;
        }
    }

    private static void Hold(bool holdWindow)
    {
        if (!holdWindow) return;
        Console.WriteLine();
        Console.WriteLine("Claude Code exited. Press any key to close this window.");
        try { Console.ReadKey(true); }
        catch (InvalidOperationException) { }
    }

    // Standard Windows argv quoting, so Claude receives each argument intact.
    private static string Join(List<string> argv)
    {
        var sb = new StringBuilder();
        foreach (string a in argv)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(Quote(a));
        }
        return sb.ToString();
    }

    private static string Quote(string arg)
    {
        if (arg.Length > 0 && arg.IndexOfAny(new char[] { ' ', '\t', '\n', '\v', '"' }) < 0) return arg;

        var sb = new StringBuilder();
        sb.Append('"');
        for (int i = 0; i < arg.Length; i++)
        {
            int slashes = 0;
            while (i < arg.Length && arg[i] == '\\') { i++; slashes++; }
            if (i == arg.Length) { sb.Append('\\', slashes * 2); break; }
            if (arg[i] == '"') { sb.Append('\\', slashes * 2 + 1); sb.Append('"'); }
            else { sb.Append('\\', slashes); sb.Append(arg[i]); }
        }
        sb.Append('"');
        return sb.ToString();
    }
}
