using System;
using System.Runtime.InteropServices;

namespace PowerManagerCOM
{
    [ComVisible(true)]
    [Guid("EC0B80F3-68F0-4C86-BBFF-6838955EBD9C")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IPowerManagerCom
    {
        DateTime GetLastSleepTime();

        DateTime GetLastWakeTime();

        bool GetSystemBatteryState();

        int GetSystemCoolingMode();
    }
}