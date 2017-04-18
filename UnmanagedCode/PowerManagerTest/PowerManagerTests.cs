using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerManagerLibrary;

namespace PowerManagerTest
{
    [TestClass]
    public class PowerManagerTests
    {
        private readonly PowerManager _powerManager = new PowerManager();

        [TestMethod]
        public void LastSleepTimeTest()
        {
            var lastSleepTime = _powerManager.GetLastSleepTime();

            Console.WriteLine("Last sleep time: {0}", lastSleepTime);
        }

        [TestMethod]
        public void LastWakeTimeTest()
        {
            var lastWakeTime = _powerManager.GetLastWakeTime();

            Console.WriteLine("Last wake time: {0}", lastWakeTime);
        }

        [TestMethod]
        public void SystemBatteryStateTest()
        {
            var state = _powerManager.GetSystemBatteryState();

            Console.WriteLine("Battery Present: {0}", state.BatteryPresent);
            Console.WriteLine("Remaining Capacity: {0}", state.RemainingCapacity);
        }

        [TestMethod]
        public void SystemPowerInformationTest()
        {
            var info = _powerManager.GetSystemPowerInformation();

            Console.WriteLine("Current idle level: {0} %", info.Idleness);
            Console.WriteLine("Current system cooling mode: {0}", info.CoolingMode);
            Console.WriteLine("**(0 - active, 1 - passive, 2- invalid)");
        }

        [TestMethod]
        public void SuspendSystemTest()
        {
            _powerManager.SuspendSystem();
        }
    }
}
