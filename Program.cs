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
        // Help was also given by the users of Isy's discord. Isy and JyeGuru.
        // Everything after this line, goes into the programming block.

        public Program() // Script Initialization
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100; // 1=1.6ms, 10=166ms, 100=1667ms (1.6s)
        }

        // Function given to me by Isy
        void ChangeState<T>(bool state) where T : class {
            var list = new List<T>();
            GridTerminalSystem.GetBlocksOfType(list);

            foreach (var item in list) {
                (item as IMyFunctionalBlock).Enabled = state;
            }
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
                ChangeState<IMyGasTank>(true);
                ChangeState<IMyTimerBlock>(true);
            } else if (chargeStatus) {
                if (EmBatteryPercent < 0.9 && percentStoredPower < 0.3) {
                    Echo("Vehicle is currently in recharge mode.");
                } else {
                    chargeStatus = false;
                }
            } else {
                if (percentStoredPower <= 0.05 && EmBatteryPercent <= 0.1) {
                    ChangeState<IMyAssembler>(false);
                    foreach (var battery in myBatteryBlocks) { battery.ChargeMode = ChargeMode.Recharge; }
                    chargeStatus = true;
                    Echo("Emergency Mode: Full system shutdown.");
                } else if (percentStoredPower <= 0.05 && EmBatteryPercent <= 0.4) {
                    ChangeState<IMyTimerBlock>(false);
                    List<IMyBeacon> myBeacons = new List<IMyBeacon>();
                    GridTerminalSystem.GetBlocksOfType(myBeacons);
                    if (myBeacons != null) { foreach (var beacon in myBeacons) { beacon.Enabled = false; beacon.HudText = "Spawn Rover"; } }
                    Echo("Emergency Mode: System backup power extremely low.");
                } else if (percentStoredPower <= 0.05 && EmBatteryPercent <= 0.7) {
                    List<IMyBeacon> myBeacons = new List<IMyBeacon>();
                    GridTerminalSystem.GetBlocksOfType(myBeacons);
                    if (myBeacons != null) { foreach (var beacon in myBeacons) { beacon.Enabled = true; beacon.HudText = "Spawn Rover: Emergency! Power: " + (EmBatteryPercent * 100) + "%"; } }
                    Echo("Emergency Mode: System backup power low.");
                } else if (percentStoredPower <= 0.05 && EmBatteryPercent > 0.7) { // Potential risk of all false here.
                    eBat.ChargeMode = ChargeMode.Auto;
                    foreach (var battery in myBatteryBlocks) { battery.ChargeMode = ChargeMode.Discharge; }
                    Echo("System power is extremely low, switching to Emergency battery.");
                } else if (percentStoredPower <= 0.25 && percentStoredPower > 0.05) {
                    ChangeState<IMyLightingBlock>(false);
                    ChangeState<IMyTextPanel>(false);
                    ChangeState<IMySensorBlock>(false);
                    ChangeState<IMyGasTank>(false);
                    if (cockpits != null) { cockpits[0].HandBrake = true; }
                    eBat.ChargeMode = ChargeMode.Recharge;
                    ChangeState<IMyAssembler>(true);
                    Echo("System power is low.");
                } else if (percentStoredPower <= 0.5 && percentStoredPower > 0.25) {
                    ChangeState<IMyRadioAntenna>(false);
                    ChangeState<IMyLaserAntenna>(false);
                    ChangeState<IMyOreDetector>(false);
                    eBat.ChargeMode = ChargeMode.Recharge;
                    ChangeState<IMyAssembler>(true);
                    Echo("System power is reduced.");
                } else if (percentStoredPower <= 0.75 && percentStoredPower > 0.5) {
                    ChangeState<IMyGasGenerator>(false);
                    ChangeState<IMyAirVent>(false);
                    eBat.ChargeMode = ChargeMode.Recharge;
                    ChangeState<IMyAssembler>(true);
                    Echo("System power is nominal.");
                } else if (percentStoredPower >= 0.75) {
                    eBat.ChargeMode = ChargeMode.Recharge;
                    ChangeState<IMyAssembler>(true);
                    Echo("System power is great.");
                } else {
                    Echo("Something has gone wrong, I will wait for something to change.");
                }
            }
        }
    }
}