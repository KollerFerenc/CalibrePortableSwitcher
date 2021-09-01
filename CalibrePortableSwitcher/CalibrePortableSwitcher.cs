using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using CommandDotNet;
using System.Diagnostics;

namespace CalibrePortableSwitcher
{
    public class CalibrePortableSwitcher
    {
        public CalibrePortableSwitcher()
        {

        }

        [Command(
            Name = "launch",
            Description = "Launch Calibre Portable.")]
        public void Launch(
            [Option(
                ShortName = "P",
                LongName = "path",
                Description = "Path to the Calibre Portable directory.")]
            string pathToDirectory)
        {
            bool isValidDirectory = CheckCalibreDirectory(pathToDirectory);

            bool isInstanceRunning = false;

            foreach (Process proc in Process.GetProcesses())
            {
                if (proc.ProcessName == "calibre")
                {
                    isInstanceRunning = true;
                    break;
                }
            }

            if (!isInstanceRunning)
            {
                Log.Information("Launching calibre-portable...");
                try
                {
                    using (Process proc = Process.Start(Path.Join(pathToDirectory, "calibre-portable.exe")))
                    {

                    }
                }
                catch (Exception ex)
                {
                    Log.Fatal($"Cannot launch calibre-portable\n{ ex.Message }");
                    Program.Exit(ErrorCode.CannotLaunchCalibrePortale);
                }
            }
            else
            {
                Log.Warning("Calibre process already running, cannot launch.");
            }
        }

        [Command(
            Name = "switchBinary",
            Description = "Switch Calibre Portable binary.")]
        public void SwitchBinary(
            [Option(
                ShortName = "P",
                LongName = "path",
                Description = "Path to the Calibre Portable directory.")]
            string pathToDirectory,
            [Option(
                ShortName = "B",
                LongName = "binary",
                Description = "Target binary.")]
            TargetBinary targetBinary,
            [Option(
                ShortName = "L",
                LongName = "launch",
                Description= "Launch Calibre Portable after switch.",
                BooleanMode=BooleanMode.Implicit)]
            bool launch,
            [Option(
                LongName = "dryRun",
                Description= "Dry run.",
                BooleanMode=BooleanMode.Implicit)]
            bool dryRun)
        {
            CheckCalibreDirectory(pathToDirectory);


            string calibre32Path = string.Empty;
            string calibre64Path = string.Empty;

            var directoryInfo = new DirectoryInfo(pathToDirectory);
            foreach (var subDir in directoryInfo.GetDirectories())
            {
                switch (subDir.Name)
                {
                    case "Calibre32":
                        calibre32Path = subDir.FullName;
                        break;
                    case "Calibre64":
                        calibre64Path = subDir.FullName;
                        break;
                    default:
                        break;
                }
            }

            bool success = false;

            if (calibre32Path != string.Empty && calibre64Path != string.Empty)
            {
                if (targetBinary == TargetBinary.Toggle)
                {
                    Log.Fatal("No active calibre folder detected, cannot toggle.");
                    Program.Exit(ErrorCode.CannotToggle);
                }
                else if (targetBinary == TargetBinary.Binary32)
                {
                    //SetAttribute(calibre32Path);

                    success = MoveDirectory(calibre32Path, Path.Join(pathToDirectory, "Calibre"), dryRun);

                    if (success)
                    {
                        if (dryRun)
                        {
                            Log.Information("[DRY] -> 32");
                        }
                        else
                        {
                            Log.Information(" -> 32");
                        }
                    }
                }
                else if (targetBinary == TargetBinary.Binary64)
                {
                    //SetAttribute(calibre64Path);

                    success = MoveDirectory(calibre64Path, Path.Join(pathToDirectory, "Calibre"), dryRun);

                    if (success)
                    {
                        if (dryRun)
                        {
                            Log.Information("[DRY] -> 64");
                        }
                        else
                        {
                            Log.Information(" -> 64");
                        }
                    }
                }
            }
            else if (calibre32Path == string.Empty)
            {
                if (targetBinary == TargetBinary.Toggle
                    || targetBinary == TargetBinary.Binary64)
                {
                    calibre32Path = Path.Join(pathToDirectory, "Calibre");

                    //SetAttribute(calibre32Path);
                    //SetAttribute(calibre64Path);

                    success = MoveDirectory(calibre32Path, Path.Join(pathToDirectory, "Calibre32"), dryRun);

                    if (success)
                        success = MoveDirectory(calibre64Path, Path.Join(pathToDirectory, "Calibre"), dryRun);

                    if (success)
                    {
                        if (dryRun)
                        {
                            Log.Information("[DRY] 32 -> 64");
                        }
                        else
                        {
                            Log.Information("32 -> 64");
                        }
                    }
                }
                else if (targetBinary == TargetBinary.Binary32)
                {
                    success = true;

                    if (dryRun)
                    {
                        Log.Information("[DRY] Already 32");
                    }
                    else
                    {
                        Log.Information("Already 32");
                    }
                }
            }
            else if (calibre64Path == string.Empty)
            {
                if (targetBinary == TargetBinary.Toggle
                    || targetBinary == TargetBinary.Binary32)
                {
                    calibre64Path = Path.Join(pathToDirectory, "Calibre");

                    //SetAttribute(calibre32Path);
                    //SetAttribute(calibre64Path);

                    success = MoveDirectory(calibre64Path, Path.Join(pathToDirectory, "Calibre64"), dryRun);

                    if (success)
                        success = MoveDirectory(calibre32Path, Path.Join(pathToDirectory, "Calibre"), dryRun);

                    if (success)
                    {
                        if (dryRun)
                        {
                            Log.Information("[DRY] 64 -> 32");
                        }
                        else
                        {
                            Log.Information("64 -> 32");
                        }
                    }
                }
                else if (targetBinary == TargetBinary.Binary64)
                {
                    success = true;

                    if (dryRun)
                    {
                        Log.Information("[DRY] Already 64");
                    }
                    else
                    {
                        Log.Information("Already 64");
                    }
                }
            }

            if (success && launch)
            {
                if (dryRun)
                {
                    Log.Information("[DRY] Should launch calibre-portable...");
                }
                else
                {
                    Launch(pathToDirectory);
                }
            }
        }

        private bool CheckCalibreDirectory(string pathToDirectory)
        {
            if (string.IsNullOrWhiteSpace(pathToDirectory))
            {
                Log.Fatal($"Invalid directory: { pathToDirectory }");
                Program.Exit(ErrorCode.InvalidPath);
            }
            else if (Directory.Exists(pathToDirectory))
            {
                var directoryInfo = new DirectoryInfo(pathToDirectory);
                var subDirs = directoryInfo.GetDirectories();

                if (subDirs.Length >= 2)
                {
                    bool calibreDir = false;
                    bool calibre32Dir = false;
                    bool calibre64Dir = false;

                    foreach (var item in subDirs)
                    {
                        switch (item.Name)
                        {
                            case "Calibre":
                                calibreDir = true;
                                break;
                            case "Calibre32":
                                calibre32Dir = true;
                                break;
                            case "Calibre64":
                                calibre64Dir = true;
                                break;
                            default:
                                break;
                        }
                    }

                    if (calibreDir && calibre32Dir && calibre64Dir)
                    {
                        Log.Fatal($"Calibre installation not found at: { pathToDirectory }");
                        Program.Exit(ErrorCode.CalibreNotFound);
                    }
                    else if ((calibreDir && calibre32Dir) || (calibreDir && calibre64Dir) || (calibre32Dir && calibre64Dir))
                    {
                        return true;
                    }
                    else
                    {
                        Log.Fatal($"Calibre installation not found at: { pathToDirectory }");
                        Program.Exit(ErrorCode.CalibreNotFound);
                    }
                }
                else
                {
                    Log.Fatal($"Calibre installation not found at: { pathToDirectory }");
                    Program.Exit(ErrorCode.CalibreNotFound);
                }
            }
            else
            {
                Log.Fatal($"Invalid directory: { pathToDirectory }");
                Program.Exit(ErrorCode.InvalidPath);
            }

            return false;
        }

        private bool MoveDirectory(string sourceDirName, string destDirName, bool dryRun)
        {
            try
            {
                if (!dryRun)
                {
                    Directory.Move(sourceDirName, destDirName);
                }
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Fatal(ex.Message);
                Program.Exit(ErrorCode.UnauthorizedAccess);
                return false;
            }
            catch (IOException ex)
            {
                Log.Fatal(ex.Message);
                Program.Exit(ErrorCode.UnauthorizedAccess);
                return false;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message);
                Program.Exit(ErrorCode.FatalUnknownError);
                return false;
            }
        }

        private void SetAttribute(string pathToDirectory)
        {
            foreach (var item in Directory.EnumerateFiles(pathToDirectory, "*", SearchOption.AllDirectories))
            {
                try
                {
                    File.SetAttributes(item, FileAttributes.Normal);
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
