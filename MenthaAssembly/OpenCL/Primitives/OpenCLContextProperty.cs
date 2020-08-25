using System;

namespace MenthaAssembly.OpenCL
{
    internal struct OpenCLContextProperty
    {
        public IntPtr Handle { get; }

        public OpenCLContextPropertyName Name { get; }

        public OpenCLContextProperty(OpenCLContextPropertyName Name, IntPtr Handle)
        {
            this.Name = Name;
            this.Handle = Handle;
        }

    }
}
