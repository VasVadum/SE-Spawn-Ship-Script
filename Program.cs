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

            var eBat = (IMyBatteryBlock)GridTerminalSystem.GetBlockWithName("Emergency Battery");
            if (eBat == null) { Echo("Emergency battery not found. Please name a single battery 'Emergency battery'."); return; }

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
                eBat.ChargeMode = ChargeMode.Recharge;
                foreach (var battery in myBatteryBlocks) { battery.ChargeMode = ChargeMode.Auto; }
                List<IMyGasTank> myGasTanks = new List<IMyGasTank>();
                GridTerminalSystem.GetBlocksOfType(myGasTanks);
                if (myGasTanks != null) { foreach (var gasTank in myGasTanks) { gasTank.Enabled = true; } }
            } else if (chargeStatus) {
                if (EmBatteryPercent < 0.9 && percentStoredPower < 0.3) {
                    Echo("Vehicle is currently in recharge mode.");
                } else {
                    chargeStatus = false;
                }
            } else {
                if (percentStoredPower <= 0.05 && EmBatteryPercent <= 0.1) {
                    // Shutdown Everything Remaining (Survival Kit?)
                    foreach (var battery in myBatteryBlocks) { battery.ChargeMode = ChargeMode.Recharge; }
                    chargeStatus = true;
                    Echo("Emergency Mode: Full system shutdown.");
                } else if (percentStoredPower <= 0.05 && EmBatteryPercent <= 0.4) {
                    List<IMyBeacon> myBeacons = new List<IMyBeacon>();
                    GridTerminalSystem.GetBlocksOfType(myBeacons);
                    if (myBeacons != null) { foreach (var beacon in myBeacons) { beacon.Enabled = false; beacon.DisplayName = ("Spawn Rover"); } }
                    // /\ Not sure if this is right either. Needs to reset the display name of the beacon.
                    Echo("Emergency Mode: System backup power extremely low.");
                } else if (percentStoredPower <= 0.05 && EmBatteryPercent <= 0.7) {
                    // Disable everything except survival kit.
                    List<IMyBeacon> myBeacons = new List<IMyBeacon>();
                    GridTerminalSystem.GetBlocksOfType(myBeacons);
                    if (myBeacons != null) { foreach (var beacon in myBeacons) { beacon.Enabled = true; beacon.DisplayName = ("Spawn Rover: Emergency! Power: " + (EmBatteryPercent * 100) + "%"); } }
                    // /\ Not sure if this is right either. Needs to display the current percent of emergency power in the beacon name.
                    Echo("Emergency Mode: System backup power low.");
                } else if (percentStoredPower <= 0.05 && EmBatteryPercent > 0.7) { // Potential risk of all false here.
                    eBat.ChargeMode = ChargeMode.Auto;
                    foreach (var battery in myBatteryBlocks) { battery.ChargeMode = ChargeMode.Discharge; }
                    Echo("System power is extremely low, switching to Emergency battery.");
                } else if (percentStoredPower <= 0.25 && percentStoredPower > 0.05) {
                    List<IMyLightingBlock> myLights = new List<IMyLightingBlock>();
                    GridTerminalSystem.GetBlocksOfType(myLights);
                    if (myLights != null) { foreach (var light in myLights) { light.Enabled = false; } }
                    List<IMyTextPanel> myLCDs = new List<IMyTextPanel>();
                    GridTerminalSystem.GetBlocksOfType(myLCDs);
                    if (myLCDs != null) { foreach (var lcd in myLCDs) { lcd.Enabled = false; } }
                    List<IMySensorBlock> mySensors = new List<IMySensorBlock>();
                    GridTerminalSystem.GetBlocksOfType(mySensors);
                    if (mySensors != null) { foreach (var sensor in mySensors) { sensor.Enabled = false; } }
                    List<IMyGasTank> myGasTanks = new List<IMyGasTank>();
                    GridTerminalSystem.GetBlocksOfType(myGasTanks);
                    if (myGasTanks != null) { foreach (var gasTank in myGasTanks) { gasTank.Enabled = false; } }
                    if (cockpits != null) {foreach (var cockpit in cockpits) { cockpit.HandBrake = true; } } // Not happy with this.
                    eBat.ChargeMode = ChargeMode.Recharge;
                    Echo("System power is low.");
                } else if (percentStoredPower <= 0.5 && percentStoredPower > 0.25) {
                    List<IMyRadioAntenna> myAntennas = new List<IMyRadioAntenna>();
                    GridTerminalSystem.GetBlocksOfType(myAntennas);
                    if (myAntennas != null) { foreach (var antenna in myAntennas) { antenna.Enabled = false; } }
                    List<IMyOreDetector> myOreDetectors = new List<IMyOreDetector>();
                    GridTerminalSystem.GetBlocksOfType(myOreDetectors);
                    if (myOreDetectors != null) { foreach (var oreDetector in myOreDetectors) { oreDetector.Enabled = false; } }
                    // Shutdown Heat Exhcnagers (If they exist)
                    eBat.ChargeMode = ChargeMode.Recharge;
                    Echo("System power is reduced.");
                } else if (percentStoredPower <= 0.75 && percentStoredPower > 0.5) {
                    List<IMyGasGenerator> myGasGenerators = new List<IMyGasGenerator>();
                    GridTerminalSystem.GetBlocksOfType(myGasGenerators);
                    if (myGasGenerators != null) { foreach (var gasGen in myGasGenerators) { gasGen.Enabled = false; } }
                    List<IMyAirVent> myVents = new List<IMyAirVent>();
                    GridTerminalSystem.GetBlocksOfType(myVents);
                    if (myVents != null) { foreach (var vent in myVents) { vent.Enabled = false; } }
                    eBat.ChargeMode = ChargeMode.Recharge;
                    Echo("System power is nominal.");
                } else if (percentStoredPower >= 0.75) {
                    eBat.ChargeMode = ChargeMode.Recharge;
                    Echo("System power is great.");
                } else {
                    Echo("Something has gone wrong, I will wait for something to change.");
                }
            }
        }
    }
}