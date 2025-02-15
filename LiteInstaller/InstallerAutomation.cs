using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace LiteInstaller
{
    public class InstallerAutomation
    {
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        public static void Run(
            string sfxPath,
            string folderPath1, string folderPath2,
            int expectedFileCount1, int expectedFolderCount1,
            int expectedFileCount2, int expectedFolderCount2,
            int clickX, int clickY)
        {
            var sfxProcess = Process.Start(sfxPath);
            var processesBefore = Process.GetProcesses().Select(p => p.Id).ToHashSet();

            var targetProcess = WaitForProcess(processesBefore);
            if (targetProcess == null) return;

            ClickAtPosition(clickX, clickY);

            WaitForFolderItemCounts(folderPath1, folderPath2, expectedFileCount1, expectedFolderCount1, expectedFileCount2, expectedFolderCount2);

            CloseProcess(targetProcess);
        }

        private static Process WaitForProcess(HashSet<int> processesBefore)
        {
            for (int i = 0; i < 30; i++)
            {
                Thread.Sleep(1000);
                var newProcess = Process.GetProcesses()
                    .FirstOrDefault(p => !processesBefore.Contains(p.Id) && p.MainWindowHandle != IntPtr.Zero);

                if (newProcess != null)
                    return newProcess;
            }

            return null;
        }

        private static void WaitForFolderItemCounts(string folderPath1, string folderPath2, int expectedFileCount1, int expectedFolderCount1, int expectedFileCount2, int expectedFolderCount2)
        {
            while (true)
            {
                var (fileCount1, folderCount1) = GetFolderItemCounts(folderPath1);
                var (fileCount2, folderCount2) = GetFolderItemCounts(folderPath2);

                if (fileCount1 == expectedFileCount1 && folderCount1 == expectedFolderCount1 &&
                    fileCount2 == expectedFileCount2 && folderCount2 == expectedFolderCount2)
                    break;

                Thread.Sleep(2000);
            }
        }

        private static (int fileCount, int folderCount) GetFolderItemCounts(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return (0, 0);

            try
            {
                var files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
                var directories = Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories);

                return (files.Length, directories.Length);
            }
            catch
            {
                return (0, 0);
            }
        }

        private static void ClickAtPosition(int x, int y)
        {
            SetCursorPos(x, y);
            Thread.Sleep(100);
            mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
        }

        private static void CloseProcess(Process process)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill();
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при закрытии процесса: {ex.Message}");
            }
        }
    }
}
