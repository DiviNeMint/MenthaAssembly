using MenthaAssembly.OpenCL.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace MenthaAssembly.OpenCL
{
    public class OpenCLPlatform
    {
        public IntPtr Handle { get; }

        public string Name { get; }

        public string Profile { get; }

        public string Vendor { get; }

        public string Version { get; }

        public string[] Extensions { get; }

        public ReadOnlyCollection<OpenCLDevice> Devices { get; }

        public OpenCLPlatform(IntPtr pPlatform)
        {
            Handle = pPlatform;

            byte[] Buffer = new byte[512];

            // Descriptions
            Name = GetPlatformInfo(OpenCLPlatformProperty.Name, Buffer);
            Profile = GetPlatformInfo(OpenCLPlatformProperty.Name, Buffer);
            Vendor = GetPlatformInfo(OpenCLPlatformProperty.Vendor, Buffer);
            Version = GetPlatformInfo(OpenCLPlatformProperty.Version, Buffer);
            Extensions = GetPlatformInfo(OpenCLPlatformProperty.Extensions, Buffer).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Devices
            Devices = new ReadOnlyCollection<OpenCLDevice>(GetDevices().ToArray());
        }

        private string GetPlatformInfo(OpenCLPlatformProperty Flag, byte[] Buffer)
        {
            OpenCLCore.GetPlatformInfo(this.Handle, Flag, Buffer.Length, Buffer, out int ReadSize);
            return Encoding.ASCII.GetString(Buffer, 0, ReadSize).Trim('\0');
        }

        private IEnumerable<OpenCLDevice> GetDevices()
        {
            if (OpenCLCore.GetDeviceIDs(Handle, OpenCLDeviceTypes.All, 0, null, out int Count) == OpenCLErrorCode.Success)
            {
                IntPtr[] pDevices = new IntPtr[Count];

                if (OpenCLCore.GetDeviceIDs(Handle, OpenCLDeviceTypes.All, Count, pDevices, out _) == OpenCLErrorCode.Success)
                    foreach (IntPtr pDevice in pDevices)
                        yield return new OpenCLDevice(this, pDevice);
            }
        }

    }
}
