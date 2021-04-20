using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Orcus.Utilities
{
    public static class ProcessExtensions
    {
        public static List<Process> GetProcessByFilename(string fileName)
        {
            var processes = new List<Process>();

            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.MainModule.FileName == fileName)
                    {
                        processes.Add(process);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return processes;
        }
    }
}