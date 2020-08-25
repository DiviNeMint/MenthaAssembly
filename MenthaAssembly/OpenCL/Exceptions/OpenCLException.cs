using System;

namespace MenthaAssembly.OpenCL
{
    public class OpenCLException : Exception
    {
        public OpenCLErrorCode ErrorCode { get; }

        public OpenCLException(OpenCLErrorCode ErrorCode) : base(ErrorCode.ToString())
        {
            this.ErrorCode = ErrorCode;
        }

    }
}
