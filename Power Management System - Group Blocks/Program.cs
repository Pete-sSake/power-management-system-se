using Sandbox.Common.ObjectBuilders;
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
        public const string batteriesGroupName = "Dover Base - Batteries - Power";
        //Input the Hydrogen Engines' Group Name in between the quotation marks ("")
        public const string hydrogenEnginesGroupName = "Dover Base - Hydrogen Engines - Power";
        //Input the Hydrogen Reactors' Group Name in between the quotation marks ("")
        public const string reactorsGroupName = "Dover Base - Reactors - Power";

        /*OPTIONAL SETUP
        */
        //Replace the "0.20" with the decimal value of the percentage at which the Hydrogen Engines will turn ON
        public const float hydrogenEnginesToggleMinThreshold = 0.20f;
        //Replace the "0.35" with the decimal value of the percentage at which the Hydrogen Engines will turn OFF
        public const float hydrogenEnginesToggleMaxThreshold = 0.35f;
        //Replace the "0.15" with the decimal value of the percentage at which the Reactors will turn ON
        public const float reactorsToggleMinThreshold = 0.15f;
        //Replace the "0.25" with the decimal value of the percentage at which the Reactors will turn OFF
        public const float reactorsToggleMaxThreshold = 0.25f;

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
            List<IMyPowerProducer> hydrogenEngines = powerProducerListGenerator(hydrogenEnginesGroupName);
            //Detect Reactors
            List<IMyPowerProducer> reactors = powerProducerListGenerator(reactorsGroupName);

            //Ends Iteration If No Hydrogen Engines or Reactors are Detected
            if (hydrogenEngines.Count == 0 && reactors.Count == 0)
            {
                Echo("No Hydrogen Engines or Reactors were Detected");
                return;
            }

            //Prints Average Charge Level of Batteries
            float chargeLevel = chargeLevelCalculator(batteries);
            double charge = Math.Round(chargeLevel * 100, 2);
            Echo($"Battery Charge________{charge}%");

            if (hydrogenEngines.Count == 0)
            {
                Echo("No Hydrogen Engines Detected");
            } else
            {
                handlePowerProducers(chargeLevel, hydrogenEngines, hydrogenEnginesToggleMinThreshold,
                    hydrogenEnginesToggleMaxThreshold, "Hydrogen Engines_____");
            }
            
            if (reactors.Count == 0)
            {
                Echo("No Hydrogen Engines Detected");
            } else
            {
                handlePowerProducers(chargeLevel, reactors, reactorsToggleMinThreshold,
                    reactorsToggleMaxThreshold, "Reactors_____________");
            }
        }//End of Main method

        private void handlePowerProducers(float chargeLevel, List<IMyPowerProducer> powerProducers, float toggleMinThreshold,
                                          float toggleMaxThreshold, string messagePrefix)
        {
            if (chargeLevel < toggleMinThreshold)
            {
                foreach (IMyPowerProducer block in powerProducers)
                {
                    block.Enabled = true;
                }
            }
            else if (chargeLevel >= toggleMaxThreshold)
            {
                foreach (IMyPowerProducer block in powerProducers)
                {
                    block.Enabled = false;
                }
            }

            bool enabled = powerProducers.Any(p => p.Enabled);
            Echo($"{messagePrefix}" + (enabled ? "ON" : "OFF"));
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