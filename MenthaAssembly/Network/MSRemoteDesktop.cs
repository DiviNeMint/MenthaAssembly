using System;
using System.Diagnostics;

namespace MenthaAssembly.Network
{
    public class MSRemoteDesktop
    {
        public string Domain { get; set; }

        public string Host { get; set; }

        public string Username { get; set; }

        public string Password { set; get; }

        public bool KeepCredentials { set; get; } = true;

        public bool IsFullScreen { set; get; } = true;

        private Process RemoteDesktop;
        public bool Login(int Timeout = 3000)
        {
            if (CreateCredentials(Timeout))
            {
                RemoteDesktop = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\mstsc.exe"),
                        WindowStyle = ProcessWindowStyle.Normal,
                        Arguments = $"{(IsFullScreen ? "/f " : string.Empty)}/v {Host}"
                    }
                };

                RemoteDesktop.Start();

                return true;
            }

            return false;
        }
        public void Logout()
        {
            if (!RemoteDesktop.HasExited)
                RemoteDesktop?.Kill();

            if (!KeepCredentials)
                DeleteCredentials();
        }

        private bool CreateCredentials(int Timeout)
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

        ~MSRemoteDesktop()
        {
            Logout();
        }

    }
}
