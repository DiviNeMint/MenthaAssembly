using MenthaAssembly.OpenCL.Primitives;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MenthaAssembly.OpenCL
{
    public class OpenCLCompiler
    {
        public ReadOnlyCollection<OpenCLDevice> Devices { get; }

        private readonly OpenCLContext Context;

        public OpenCLCompiler() : this(OpenCLCore.Platforms.SelectMany(i => i.Devices)
                                                           .FirstOrDefault(i => i.Type == OpenCLDeviceTypes.GPU))
        { }

        public OpenCLCompiler(params OpenCLDevice[] Devices)
        {
            this.Devices = new ReadOnlyCollection<OpenCLDevice>(Devices ?? new OpenCLDevice[0]);
            this.Context = new OpenCLContext(Devices, Devices.Select(i => new OpenCLContextProperty(OpenCLContextPropertyName.Platform, i.Platform.Handle)), null, IntPtr.Zero);
        }

        public OpenCLKernel[] Compile(params string[] Sources)
        {
            if (Sources.Length == 0)
                throw new ArgumentNullException(nameof(Sources), $"{nameof(Sources)} can't be empty.");

            OpenCLProgram Program = new OpenCLProgram(Context, Sources);
            return Program.Build(null, null, null, IntPtr.Zero).ToArray();
        }

    }
}
