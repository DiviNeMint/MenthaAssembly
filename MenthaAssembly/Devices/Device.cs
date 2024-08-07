using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Devices
{
    public static class Device
    {
        #region Windows API
        public enum RawInputDeviceType : uint
        {
            Mouse = 0,
            Keyboard = 1,
            HID = 2
        }

        internal enum DeviceInfoTypes
        {
            RIDI_PreparsedData = 0x20000005,
            RIDI_DeviceName = 0x20000007,
            RIDI_DeviceInfo = 0x2000000B
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct RID_Device_Info
        {
            [FieldOffset(0)]
            public int cbSize;
            [FieldOffset(4)]
            public RawInputDeviceType dwType;
            [FieldOffset(8)]
            public RID_Device_Info_Mouse Mouse;
            [FieldOffset(8)]
            public RID_Device_Info_Keyboard Keyboard;
            [FieldOffset(8)]
            public RID_Device_Info_HID HID;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RID_Device_Info_HID
        {
            public int dwVendorId;
            public int dwProductId;
            public int dwVersionNumber;
            public ushort usUsagePage;
            public ushort usUsage;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RID_Device_Info_Keyboard
        {
            public int dwType;
            public int dwSubType;
            public int dwKeyboardMode;
            public int dwNumberOfFunctionKeys;
            public int dwNumberOfIndicators;
            public int dwNumberOfKeysTotal;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RID_Device_Info_Mouse
        {
            public int dwId;
            public int dwNumberOfButtons;
            public int dwSampleRate;
            public int fHasHorizontalWheel;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RawInputDevice
        {
            public IntPtr hDevice;
            public RawInputDeviceType Type;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private unsafe static extern uint GetRawInputDeviceList([In, Out] RawInputDevice[] RawInputDeviceList, int* NumDevices, int Size);

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetRawInputDeviceInfoW")]
        private unsafe static extern uint GetRawInputDeviceInfo(IntPtr hDevice, DeviceInfoTypes uiCommand, void* pData, int* pcbSize);

        #endregion

        internal unsafe static RawInputDevice[] GetRawInputDevices()
        {
            int Length = 0;
            if (GetRawInputDeviceList(null, &Length, sizeof(RawInputDevice)) > 0 ||
                Length == 0)
                return [];

            RawInputDevice[] Devices = new RawInputDevice[Length];
            if (GetRawInputDeviceList(Devices, &Length, sizeof(RawInputDevice)) == 0)
                return [];

            foreach (RawInputDevice d in Devices)
            {
                Length = 0;
                GetRawInputDeviceInfo(d.hDevice, DeviceInfoTypes.RIDI_DeviceName, null, &Length);

                string Name;
                char[] Datas = new char[Length];
                fixed (char* pData = Datas)
                {
                    GetRawInputDeviceInfo(d.hDevice, DeviceInfoTypes.RIDI_DeviceName, pData, &Length);
                    Name = new(pData);
                }
                
                Debug.WriteLine($"Name : {Name}");

                Length = sizeof(RID_Device_Info);
                RID_Device_Info Info = new() { cbSize = Length };

                GetRawInputDeviceInfo(d.hDevice, DeviceInfoTypes.RIDI_DeviceInfo, &Info, &Length);

                Debug.WriteLine($"Type : {Info.dwType}");
                switch (Info.dwType)
                {
                    case RawInputDeviceType.Mouse:
                        {

                            break;
                        }
                    case RawInputDeviceType.Keyboard:
                        {

                            break;
                        }
                    case RawInputDeviceType.HID:
                        {

                            break;
                        }
                }
            }

            return Devices;
        }


        //[DllImport("user32.dll", SetLastError = true)]
        //private static extern uint SendInput(int NumberOfInputs, ref InputInfo Inputs, int SizeOfInputInfo);

        //[StructLayout(LayoutKind.Explicit)]
        //private struct InputInfo
        //{
        //    [FieldOffset(0)]
        //    public InputType Type;

        //    [FieldOffset(4)]
        //    public GlobalMouse.MouseInputInfo MouseInput;

        //    [FieldOffset(4)]
        //    public GlobalKeyboard.KeyboardInputInfo KeyboardInput;

        //    [FieldOffset(4)]
        //    public HardwareInputInfo HardwareInput;

        //}

        //[StructLayout(LayoutKind.Sequential)]
        //private struct HardwareInputInfo
        //{
        //    public int uMsg;
        //    public short wParamL;
        //    public short wParamH;
        //}

        //private enum InputType
        //{
        //    Mouse = 0,
        //    Keyboard,
        //    Hardware
        //}

        //internal static void Send(params GlobalMouse.MouseInputInfo[] Infos)
        //{
        //    InputInfo InputInfo = new InputInfo
        //    {
        //        Type = InputType.Mouse
        //    };

        //    foreach (GlobalMouse.MouseInputInfo Info in Infos)
        //    {
        //        InputInfo.MouseInput = Info;
        //        SendInput(1, ref InputInfo, Marshal.SizeOf<InputInfo>());
        //    }
        //}



    }
}
