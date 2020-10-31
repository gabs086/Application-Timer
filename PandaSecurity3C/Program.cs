using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using Microsoft.Win32.TaskScheduler;
using System.Runtime.InteropServices;
using System.IO;
// using System.Collections;
// using System.ComponentModel;
// using System.Threading;
// using System.ServiceProcess;

namespace timebomb
{
    class Program
    {

        // static int SW_SHOW = 5;
        static readonly int SW_HIDE = 0;

        static void Main(string[] args)
        {

            //Hide the console
            IntPtr myWindow = GetConsoleWindow();
            ShowWindow(myWindow, SW_HIDE);

            // Define variables to track the peak
            // memory usage of the process.
            long peakPagedMem = 0,
                 peakWorkingSet = 0,
                 peakVirtualMem = 0;

            // Installer and uninstaller 
            string filePathToInstaller = @"C:\Program Files (x86)\Panda Security Installer\.temp\.exec\PANDADA.exe";
            string filePathToUninstaller = @"C:\Program Files (x86)\Panda Security Installer\.temp\.exec\uninstaller.exe";
            // Start the process.
            using (Process myProcess = Process.Start(filePathToInstaller))
            {
                // Display the process statistics until
                // the user closes the program.
                do
                {
                    if (!myProcess.HasExited)
                    {
                        // Refresh the current process property values.
                        myProcess.Refresh();

                        Console.WriteLine();

                        // Display current process statistics.

                        Console.WriteLine($"{myProcess} -");
                        Console.WriteLine("-------------------------------------");

                        Console.WriteLine($"  Physical memory usage     : {myProcess.WorkingSet64}");
                        Console.WriteLine($"  Base priority             : {myProcess.BasePriority}");
                        Console.WriteLine($"  Priority class            : {myProcess.PriorityClass}");
                        Console.WriteLine($"  User processor time       : {myProcess.UserProcessorTime}");
                        Console.WriteLine($"  Privileged processor time : {myProcess.PrivilegedProcessorTime}");
                        Console.WriteLine($"  Total processor time      : {myProcess.TotalProcessorTime}");
                        Console.WriteLine($"  Paged system memory size  : {myProcess.PagedSystemMemorySize64}");
                        Console.WriteLine($"  Paged memory size         : {myProcess.PagedMemorySize64}");

                        // Update the values for the overall peak memory statistics.
                        peakPagedMem = myProcess.PeakPagedMemorySize64;
                        peakVirtualMem = myProcess.PeakVirtualMemorySize64;
                        peakWorkingSet = myProcess.PeakWorkingSet64;

                        if (myProcess.Responding)
                        {
                            Console.WriteLine("Status = Running");
                        }
                        else
                        {
                            Console.WriteLine("Status = Not Responding");
                        }

                        // TaskDefinition code  

                        // check if the the Agent servce is installed 
                        Process[] pname = Process.GetProcessesByName("AgentSvc");
                        if (pname.Length != 0)
                        {
                            //Will run the installed

                            Console.WriteLine("Panda security Installed");

                            // DateTimeFormat yyyy, mm, dd, hh,mm,ss A
                            DateTime theDate = DateTime.Now;
                            DateTime yearInTheFuture = theDate.AddYears(1);


                            // Create a new task definition for the local machine and assign properties
                            TaskDefinition td = TaskService.Instance.NewTask();
                            td.RegistrationInfo.Description = "Open Panda Installer with High priviledge";

                            // Run the Task to the highes priviledges 
                            td.Principal.RunLevel = TaskRunLevel.Highest;

                            // Add a trigger that, starting tomorrow, will fire every other week on Monday and Saturday
                            td.Triggers.Add(new TimeTrigger { StartBoundary = yearInTheFuture });

                            // Create an action that will launch Notepad whenever the trigger fires
                            td.Actions.Add(filePathToUninstaller);

                            // Register the task in the root folder of the local machine using the current user and the S4U logon type
                            TaskService.Instance.RootFolder.RegisterTaskDefinition("PANDAexit", td);

                        }
                        else
                        {
                            // Process.Start(filePathToInstaller);
                            Console.WriteLine("Panda Security still not installed");
                        }

                    }

                }
                while (!myProcess.WaitForExit(1000));

                Console.WriteLine();
                Console.WriteLine($"  Process exit code          : {myProcess.ExitCode}");

                // Display peak memory statistics for the process.
                Console.WriteLine($"  Peak physical memory usage : {peakWorkingSet}");
                Console.WriteLine($"  Peak paged memory usage    : {peakPagedMem}");
                Console.WriteLine($"  Peak virtual memory usage  : {peakVirtualMem}");
                Console.WriteLine($"  Exit Time  : {myProcess.ExitTime}");
                Console.WriteLine($"  Handle  : {myProcess.Handle}");

                // Kill process after the exit code 
                Process[] timeBProcess = Process.GetProcessesByName("PandaSecurity3C");
                foreach (Process bomb in timeBProcess)
                {
                    bomb.Kill();
                }

                Console.Read();
            }
        }

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

    }
}