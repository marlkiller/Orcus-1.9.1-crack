
            foreach (var window in windows.ToList())
            {
                bool belongsToProcess;
                if (!_windowHandles.TryGetValue(window.Handle, out belongsToProcess))
                {
                    uint processId;
                    NativeMethods.GetWindowThreadProcessId(window.Handle, out processId);

                    if (processId == 0)
                    {
                        windows.Remove(window);
                        continue;
                    }
                    if (processId == Process.Id)
                    {
                        _windowHandles.Add(window.Handle, true);
                        continue;
                    }

                    Process process;
                    try
                    {
                        process = Process.GetProcessById((int) processId);
                    }
                    catch (Exception)
                    {
                        _windowHandles.Add(window.Handle, false);
                        windows.Remove(window);
                        continue;
                    }

                    if (ParentProcessUtilities.GetParentProcess(process.Handle)?.Id == processId)
                    {
                        _windowHandles.Add(window.Handle, true);
                    }
                    else
                    {
                        _windowHandles.Add(window.Handle, false);
                        windows.Remove(window);
                    }
                }
                else if (!belongsToProcess)
                {
                    windows.Remove(window);
                }
            }
