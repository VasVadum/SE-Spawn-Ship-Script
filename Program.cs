using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // Helpful Information:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        // Everything after this line, goes into the programming block.

        public Program() // Script Initialization
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100; // 1=1.6ms, 10=166ms, 100=1667ms (1.6s)
        }

        public void Main(string argument, UpdateType updateSource)
        {
            List<IMyBatteryBlock> myBatteryBlocks = new List<IMyBatteryBlock>();
            IMyBlockGroup StandardBatteries = GridTerminalSystem.GetBlockGroupWithName("Batteries");
            if (StandardBatteries == null) { Echo("Battery group not found!"); return; }
            StandardBatteries.GetBlocksOfType(myBatteryBlocks);

            var vehicleState = 0;
            float currentStoredPowerAll = 0f;
            float maxStoredPowerAll = 0f;
            float EmBatteryPercent = 0f;

            foreach (var battery in myBatteryBlocks) {
                currentStoredPowerAll += battery.CurrentStoredPower;
                maxStoredPowerAll += battery.MaxStoredPower;
            }
            float percentStoredPower = currentStoredPowerAll / maxStoredPowerAll;

            // Plans: Invert the below code, section it off into vehicle state checks.
            
            if (percentStoredPower >= 0.75 && vehicleState == 0) {
                // Set E.Battery to Recharge.
            } else if (percentStoredPower < 0.75 && percentStoredPower >= 0.5 && vehicleState == 0) {
                // Shut down Oxygen Generators & Vents
                // Set E.Battery to Recharge.
            } else if (percentStoredPower <= 0.5 && percentStoredPower >= 0.25 && vehicleState == 0) {
                // Shut down Antennas, Heat Exchanger, Ore Detectors
                // Set E.Battery to Recharge.
            } else if (percentStoredPower <= 0.25 && percentStoredPower >= 0.05 && vehicleState == 0) {
                // Shut down lights, LCDs, Sensors, gas tanks.
                // Park if not already parked.
                // Set E.Battery to Recharge.
            } else if (percentStoredPower <= 0.05 && EmBatteryPercent >= 0.7 && vehicleState == 0) {
                // Enable emergency batteries, set batteries to discharge.
            } else if (percentStoredPower <= 0.05 && EmBatteryPercent <= 0.7 && vehicleState == 0) {
                // Disable everything except Survival Kit
            } else if (percentStoredPower <= 0.05 && EmBatteryPercent <= 0.4 && vehicleState == 0) {
                // Enable Beacon with Custom Name
            } else if (percentStoredPower <= 0.05 && EmBatteryPercent <= 0.1 && vehicleState == 0) {
                // Shut down -everything-.
                // Enable e.battery recharge and battery recharge.
                // Vehicle state 1.
            } else if (EmBatteryPercent > 0.9 && percentStoredPower <= 0.3 && vehicleState == 1) {
                // Set vehicle state 0
            } else {
                Echo("Either someone is driving, or I'm confused.");
            }
        }
    }
}


// Block Group: Batteries
// This group contains 4 batteries.
// "[1] Emergency Battery" (Will rename if script works)
// Beacon blinks faster with more power. Add battery percentage to name with up/down arrow for charge/discharge.
// Group Names: Exterior Lights, Floodlights, Hydrogen Tanks, Wheels
// Script should be disabled while vehicle is piloted. Setting back to the default state when not piloted.

/*
List<IMyShipController> cockpits = new List<IMyShipController>();
GridTerminalSystem.GetBlocksOfType(cockpits);

bool underControl = false;

foreach (var cockpit in cockpits)
{
    if (cockpit.IsUnderControl)
    {
        underControl = true;
        break;
    }
}
*/