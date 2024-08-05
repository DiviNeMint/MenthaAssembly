using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Network
{
    public class RemoteDesktop
    {
        #region Windows API
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        #endregion

        internal static readonly ConcurrentObservableCollection<RemoteDesktop> InternalRemoteDesktops = [];
        public static IReadOnlyList<RemoteDesktop> RemoteDesktops { get; } = new ReadOnlyObservableCollection<RemoteDesktop>(InternalRemoteDesktops);

        public static int Timeout { set; get; } = 3000;

        public string Host { get; }

        public string Domain { get; }

        public string Username { get; }

        private readonly string Password;

        public bool KeepCredentials { set; get; } = true;

        internal RemoteDesktop(string Host, string Domain, string Username, string Password)
        {
            this.Host = Host;
            this.Domain = Domain;
            this.Username = Username;
            this.Password = Password;
        }

        /// <summary>
        /// Activates the window and displays it in its current size and position. 
        /// </summary>
        public bool ActivateWindow()
        {
            bool Result = ShowWindow(RemoteProcess.MainWindowHandle, 9);
            SetForegroundWindow(RemoteProcess.MainWindowHandle);
            return Result;
        }

        private Process RemoteProcess;
        private bool Login()
        {
            if (CreateCredentials())
            {
                RemoteProcess = new Process
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\mstsc.exe"),
                        WindowStyle = ProcessWindowStyle.Normal,
                        Arguments = $"/f /v {Host}"
                    }
                };
                RemoteProcess.Exited += (s, e) => InternalRemoteDesktops.Remove(this);

                RemoteProcess.Start();

                return true;
            }

            return false;
        }
        public void Logout()
        {
            if (!RemoteProcess.HasExited)
                RemoteProcess?.Kill();

            if (!KeepCredentials)
                DeleteCredentials();

            InternalRemoteDesktops.Remove(this);
        }

        private bool CreateCredentials()
        {
            Process CmdKey = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\cmdkey.exe"),
                    Arguments = $"/generic:TERMSRV/{Host} " +
                                $"/user:{(string.IsNullOrEmpty(Domain) ? Username : $"{Domain}\\{Username}")} " +
                                $"/pass:{Password}",
                    WindowStyle = ProcessWindowStyle.Hidden,
                }
            };
            CmdKey.Start();
            return CmdKey.WaitForExit(Timeout);
        }
        private void DeleteCredentials()
        {
            Process CmdKey = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\cmdkey.exe"),
                    Arguments = $@"/delete:TERMSRV/{Host}",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                }
            };
            CmdKey.Start();
        }

        public bool Equals(string Host, string Domain, string Username, string Password)
            => this.Host.Equals(Host) &&
               this.Username.Equals(Username) &&
               this.Password.Equals(Password) &&
               string.IsNullOrEmpty(this.Domain) ? string.IsNullOrEmpty(Domain) :
                                                   this.Domain.Equals(Domain);

        public override string ToString()
            => $"Host : {Host}, User : {(string.IsNullOrEmpty(Domain) ? Username : $"{Domain}\\{Username}")}";

        ~RemoteDesktop()
        {
            Logout();
        }

        public static RemoteDesktop Login(string Host, string Domain, string Username, string Password)
        {
            if (string.IsNullOrEmpty(Host))
                throw new ArgumentException($"Host is null or empty.");

            if (string.IsNullOrEmpty(Username))
                throw new ArgumentException($"Username is null or empty.");

            if (string.IsNullOrEmpty(Password))
                throw new ArgumentException($"Password is null or empty.");

            if (InternalRemoteDesktops.Handle(() => InternalRemoteDesktops.FirstOrDefault((i) => i.Equals(Host, Domain, Username, Password))) is RemoteDesktop Remote)
            {
                Remote.ActivateWindow();
                return Remote;
            }

            Remote = new RemoteDesktop(Host, Domain, Username, Password);
            if (Remote.Login())
            {
                InternalRemoteDesktops.Add(Remote);
                return Remote;
            }

            return null;
        }

    }
}