namespace MenthaAssembly.IO
{
    public enum CRCAlgorithm
    {
        /// <summary>
        /// CRC-8  
        /// Width: 8, Poly: 0x07, Init: 0x00, RefIn: false, RefOut: false, XorOut: 0x00
        /// </summary>
        CRC8,

        /// <summary>
        /// CRC-8-CCITT  
        /// Width: 8, Poly: 0x07, Init: 0x00, RefIn: false, RefOut: false, XorOut: 0x55
        /// </summary>
        CRC8Ccitt,

        /// <summary>
        /// CRC-8-Dallas/Maxim  
        /// Width: 8, Poly: 0x31, Init: 0x00, RefIn: true, RefOut: true, XorOut: 0x00
        /// </summary>
        CRC8DallasMaxim,

        /// <summary>
        /// CRC-16-IBM / CRC-16-Modbus  
        /// Width: 16, Poly: 0x8005, Init: 0xFFFF, RefIn: true, RefOut: true, XorOut: 0x0000
        /// </summary>
        CRC16Ibm,

        /// <summary>
        /// CRC-16-CCITT (False)  
        /// Width: 16, Poly: 0x1021, Init: 0xFFFF, RefIn: false, RefOut: false, XorOut: 0x0000
        /// </summary>
        CRC16CcittFalse,

        /// <summary>
        /// CRC-16-XMODEM  
        /// Width: 16, Poly: 0x1021, Init: 0x0000, RefIn: false, RefOut: false, XorOut: 0x0000
        /// </summary>
        CRC16Xmodem,

        /// <summary>
        /// CRC-16-ARC / CRC-16-IBM (Init: 0x0000)  
        /// Width: 16, Poly: 0x8005, Init: 0x0000, RefIn: true, RefOut: true, XorOut: 0x0000
        /// </summary>
        CRC16Arc,

        /// <summary>
        /// CRC-16-USB  
        /// Width: 16, Poly: 0x8005, Init: 0xFFFF, RefIn: true, RefOut: true, XorOut: 0xFFFF
        /// </summary>
        CRC16Usb,

        /// <summary>
        /// CRC-32  
        /// Width: 32, Poly: 0x04C11DB7, Init: 0xFFFFFFFF, RefIn: true, RefOut: true, XorOut: 0xFFFFFFFF
        /// </summary>
        CRC32,

        /// <summary>
        /// CRC-32C (Castagnoli)  
        /// Width: 32, Poly: 0x1EDC6F41, Init: 0xFFFFFFFF, RefIn: true, RefOut: true, XorOut: 0xFFFFFFFF
        /// </summary>
        CRC32C,

        /// <summary>
        /// CRC-32K (Koopman)  
        /// Width: 32, Poly: 0x741B8CD7, Init: 0xFFFFFFFF, RefIn: true, RefOut: true, XorOut: 0xFFFFFFFF
        /// </summary>
        CRC32K,

        /// <summary>
        /// CRC-64-ECMA  
        /// Width: 64, Poly: 0x42F0E1EBA9EA3693, Init: 0x0000000000000000, RefIn: false, RefOut: false, XorOut: 0x0000000000000000
        /// </summary>
        CRC64Ecma,

        /// <summary>
        /// CRC-64-ISO  
        /// Width: 64, Poly: 0x000000000000001B, Init: 0xFFFFFFFFFFFFFFFF, RefIn: true, RefOut: true, XorOut: 0xFFFFFFFFFFFFFFFF
        /// </summary>
        CRC64Iso,

    }

}