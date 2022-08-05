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

            bool chargeStatus = false;
            float currentStoredPowerAll = 0f;
            float maxStoredPowerAll = 0f;
            float EmBatteryPercent = 0f;

            foreach (var battery in myBatteryBlocks) {
                currentStoredPowerAll += battery.CurrentStoredPower;
                maxStoredPowerAll += battery.MaxStoredPower;
            }
            float percentStoredPower = currentStoredPowerAll / maxStoredPowerAll;

            List<IMyShipController> cockpits = new List<IMyShipController>();
            GridTerminalSystem.GetBlocksOfType(cockpits);
            if (cockpits == null) { Echo("There are no cockpits!"); return; }
            bool underControl = false;
            foreach (var cockpit in cockpits) {
                if (cockpit.IsUnderControl) {
                    underControl = true;
                    break;
                }
            }
            if (underControl) {
                Echo("Vehicle is currently under control.");
                // Set e.battery to recharge
                // Set batteries to auto
                // Set gas tanks on
            } else if (chargeStatus) {
                if (EmBatteryPercent < 0.9 && percentStoredPower < 0.3) {
                    Echo("Vehicle is currently in recharge mode.");
                } else {
                    chargeStatus = false;
                }
            } else {
                if (percentStoredPower <= 0.05 && EmBatteryPercent <= 0.1) {
                    // Shutdown Everything.
                    // Enable batteries recharge.
                    chargeStatus = true;
                    Echo("Emergency Mode: Full system shutdown.");
                } else if (percentStoredPower <= 0.05 && EmBatteryPercent <= 0.4) {
                    // Turn Beacon Off
                    // Reset Beacon Name
                    Echo("Emergency Mode: System backup power extremely low.");
                } else if (percentStoredPower <= 0.05 && EmBatteryPercent <= 0.7) {
                    // Disable everything except survival kit.
                    // Set Beacon Name to Emergency Mode.
                    Echo("Emergency Mode: System backup power low.");
                } else if (percentStoredPower <= 0.05 && EmBatteryPercent > 0.7) { // Potential risk of all false here.
                    // Enable Emergency Batteries.
                    // Set Batteries to Discharge.
                    Echo("System power is extremely low, switching to Emergency battery.");
                } else if (percentStoredPower <= 0.25 && percentStoredPower > 0.05) {
                    // Shutdown Lights
                    // Shutdown LCDs
                    // Shutdown Sensors
                    // Shutdown Gas tanks
                    // Park if not parked
                    // Set e.bat to Recharge
                    Echo("System power is low.");
                } else if (percentStoredPower <= 0.5 && percentStoredPower > 0.25) {
                    // Shutdown Antennas
                    // Shutdown Heat Exhcnagers (If they exist)
                    // Shutdown Ore Detectors
                    // Set e.bat to Recharge
                    Echo("System power is reduced.");
                } else if (percentStoredPower <= 0.75 && percentStoredPower > 0.5) {
                    // Shutdown Oxygen Generators
                    // Shutdown Vents
                    // Set e.bat to Recharge
                    Echo("System power is nominal.");
                } else if (percentStoredPower >= 0.75) {
                    // Set e.bat to Recharge
                    Echo("System power is great.");
                } else {
                    Echo("Something has gone wrong, I will wait for something to change.");
                }
            }
        }
    }
}

// "[1] Emergency Battery" (Will rename if script works)
// Group Names: Batteries, Exterior Lights, Floodlights, Hydrogen Tanks, Wheels