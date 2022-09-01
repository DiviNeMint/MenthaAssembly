namespace MenthaAssembly.Devices
{
    public enum PrinterStatus : uint
    {
        /// <summary>
        /// The printer is idle.
        /// </summary>
        Idle = 0x0,

        /// <summary>
        ///The printer is paused.
        /// </summary>
        Paused = 0x00000001,

        /// <summary>
        ///The printer is in an error state.
        /// </summary>
        Error = 0x00000002,

        /// <summary>
        ///The printer is being deleted as a result of a client's call to RpcDeletePrinter. No new jobs can be submitted on existing printer objects for that printer.
        /// </summary>
        PendingDeletion = 0x00000004,

        /// <summary>
        ///Paper is stuck in the printer.
        /// </summary>
        PaperJam = 0x00000008,

        /// <summary>
        ///The printer is out of paper.
        /// </summary>
        PaperOut = 0x00000010,

        /// <summary>
        ///The printer is in a manual feed state.
        /// </summary>
        ManualFeed = 0x00000020,

        /// <summary>
        ///The printer has an unspecified paper problem.
        /// </summary>
        PaperProblem = 0x00000040,

        /// <summary>
        ///The printer is offline.
        /// </summary>
        Offline = 0x00000080,

        /// <summary>
        ///The printer is in an active input or output state.
        /// </summary>
        IOActive = 0x00000100,

        /// <summary>
        ///The printer is busy.
        /// </summary>
        Busy = 0x00000200,

        /// <summary>
        ///The printer is printing.
        /// </summary>
        Printing = 0x00000400,

        /// <summary>
        ///The printer's output bin is full.
        /// </summary>
        OutputBinFull = 0x00000800,

        /// <summary>
        ///The printer is not available for printing.
        /// </summary>
        NotAvailable = 0x00001000,

        /// <summary>
        ///The printer is waiting.
        /// </summary>
        Waiting = 0x00002000,

        /// <summary>
        ///The printer is processing a print job.
        /// </summary>
        Processing = 0x00004000,

        /// <summary>
        ///The printer is initializing.
        /// </summary>
        Initializing = 0x00008000,

        /// <summary>
        ///The printer is warming up.
        /// </summary>
        WarmingUp = 0x00010000,

        /// <summary>
        ///The printer is low on toner.
        /// </summary>
        TonerLow = 0x00020000,

        /// <summary>
        ///The printer is out of toner.
        /// </summary>
        NoToner = 0x00040000,

        /// <summary>
        ///The printer cannot print the current page.
        /// </summary>
        PagePunt = 0x00080000,

        /// <summary>
        ///The printer has an error that requires the user to do something.
        /// </summary>
        UserIntervention = 0x00100000,

        /// <summary>
        ///The printer has run out of memory.
        /// </summary>
        OutOfMemory = 0x00200000,

        /// <summary>
        ///The printer door is open.
        /// </summary>
        DoorOpen = 0x00400000,

        /// <summary>
        ///The printer status is unknown.<186>
        /// </summary>
        ServerUnknown = 0x00800000,

        /// <summary>
        ///The printer is in power-save mode.<184>
        /// </summary>
        PowerSave = 0x01000000,

        /// <summary>
        ///The printer is offline.<185>
        /// </summary>
        ServerOffline = 0x02000000,

    }
}