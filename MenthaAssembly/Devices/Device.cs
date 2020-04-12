using System.Linq;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Devices
{
    public static class Device
    {
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
