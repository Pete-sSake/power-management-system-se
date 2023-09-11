﻿using Sandbox.Common.ObjectBuilders;
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
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        /*Power Management System
        
        This Script will toggle Hydrogen Engines and Reactors on and off based on Batteries average Charge Level
        */

        /*REQUIRED SETUP:
        */

        //Input the Batteries' Group Name in between the quotation marks ("")
        public string batteriesGroupName = "Dover Base - Batteries - Power";
        //Input the Hydrogen Engines' Group Name in between the quotation marks ("")
        public string hydrogenEnginesGroupName = "Dover Base - Hydrogen Engines - Power";
        //Input the Hydrogen Reactors' Group Name in between the quotation marks ("")
        public string reactorsGroupName = "Dover Base - Reactors - Power";

        /*OPTIONAL SETUP
        */
        //Replace the "0.20" with the decimal value of the percentage at which the Hydrogen Engines will turn ON
        public float hydrogenEnginesToggleMinThreshold = 0.20f;
        //Replace the "0.35" with the decimal value of the percentage at which the Hydrogen Engines will turn OFF
        public float hydrogenEnginesToggleMaxThreshold = 0.35f;
        //Replace the "0.15" with the decimal value of the percentage at which the Reactors will turn ON
        public float reactorsToggleMinThreshold = 0.15f;
        //Replace the "0.25" with the decimal value of the percentage at which the Reactors will turn OFF
        public float reactorsToggleMaxThreshold = 0.25f;

        /*DO NOT MODIFY BELOW THIS LINE
        //
        //
        */ 

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            //Detect Batteries
            List<IMyBatteryBlock> batteries = batteriesListGenerator();
            if (batteries.Count == 0)
            {
                Echo("No Batteries Detected");
                return;
            }

            //Detect Hydrogen Engines
            bool hydrogenEnginesExist = true;
            List<IMyPowerProducer> hydrogenEngines = powerProducerListGenerator(hydrogenEnginesGroupName);
            if (hydrogenEngines.Count == 0)
            {
                hydrogenEnginesExist = false;
                Echo("No Hydrogen Engines Detected");
            }

            //Detect Reactors
            bool reactorsExist = true;
            List<IMyPowerProducer> reactors = powerProducerListGenerator(reactorsGroupName);
            if (reactors.Count == 0)
            {
                reactorsExist = false;
                Echo("No Hydrogen Engines Detected");
            }

            //Ends Iteration If No Hydrogen Engines or Reactors are Detected
            if (!hydrogenEnginesExist && !reactorsExist)
            {
                Echo("No Hydrogen Engines or Reactors are Detected");
                return;
            }

            //Prints Average Charge Level of Batteries
            float chargeLevel = chargeLevelCalculator(batteries);
            double charge = Math.Round(chargeLevel * 100, 2);
            Echo("Battery Charge:   " +  charge + "%");

            //Turns Hydrogen Engines and Reactors ON or OFF based on batteries Charge Level
            powerProducersToggle(chargeLevel, hydrogenEnginesExist, reactorsExist, hydrogenEngines, reactors);

        }//End of Main method

        //Turns Hydrogen Engines and Reactors ON or OFF base on batteries Charge Level
        public void powerProducersToggle(float chargeLevel, bool hydrogenEnginesExist, bool reactorsExist,
                                         List<IMyPowerProducer> hydrogenEngines, List<IMyPowerProducer> reactors)
        {
            if (hydrogenEnginesExist)
            {
                if (chargeLevel < hydrogenEnginesToggleMinThreshold)
                {
                    foreach (IMyPowerProducer block in hydrogenEngines)
                    {
                        block.Enabled = true;
                    }
                }
                else if (chargeLevel >= hydrogenEnginesToggleMaxThreshold)
                {
                    foreach (IMyPowerProducer block in hydrogenEngines)
                    {
                        block.Enabled = false;
                    }
                }
                {
                    bool enabled = false;
                    foreach (IMyPowerProducer block in hydrogenEngines)
                    {
                        if (block.Enabled == true)
                        {
                            enabled = true;
                        }
                    }
                    if (enabled)
                    {
                        Echo("Hydrogen Engines:   ON");
                    }
                    else
                    {
                        Echo("Hydrogen Engines:   OFF");
                    }
                }
            }
            if (reactorsExist)
            {
                if (chargeLevel < reactorsToggleMinThreshold)
                {
                    foreach (IMyPowerProducer block in reactors)
                    {
                        block.Enabled = true;
                    }
                }
                else if (chargeLevel >= reactorsToggleMaxThreshold)
                {
                    foreach (IMyPowerProducer block in reactors)
                    {
                        block.Enabled = false;
                    }
                }
                {
                    bool enabled = false;
                    foreach (IMyPowerProducer block in reactors)
                    {
                        if (block.Enabled == true)
                        {
                            enabled = true;
                        }
                    }
                    if (enabled)
                    {
                        Echo("Reactors:   ON");
                    }
                    else
                    {
                        Echo("Reactors:   OFF");
                    }
                }
            }
        }

        //Calculates Charge Level of all functional batteries as Current Charge divided by Max Charge
        public float chargeLevelCalculator(List<IMyBatteryBlock> batteries)
        {
            float chargeSum = 0;
            float maxChargeSum = 0;
            foreach (IMyBatteryBlock batterie in batteries)
            {
                chargeSum += batterie.CurrentStoredPower;
                maxChargeSum += batterie.MaxStoredPower;
            }

            return chargeSum / maxChargeSum;
        }                
                
        //Creates Lists of Batteries. Adds only if the Battery is at least functional
        public List<IMyBatteryBlock> batteriesListGenerator()
        {
            IMyBlockGroup batteriesGroup = GridTerminalSystem.GetBlockGroupWithName(batteriesGroupName) as IMyBlockGroup;
            List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
            batteriesGroup.GetBlocksOfType(batteries, bat => bat.IsFunctional);

            return batteries;
        }

        //Creates List of Power Producer Blocks. Adds only if the Power Producer is at least functional
        public List<IMyPowerProducer> powerProducerListGenerator(string powerProducersGroupName)
        {
            IMyBlockGroup powerProducersGroup = GridTerminalSystem.GetBlockGroupWithName(powerProducersGroupName) as IMyBlockGroup;
            List<IMyPowerProducer> powerProducers = new List<IMyPowerProducer>();
            powerProducersGroup.GetBlocksOfType(powerProducers, powP => powP.IsFunctional);

            return powerProducers;
        }
    }
}