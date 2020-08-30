using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace VanillaPowerExpanded
{
    public class GasPipeNet
    {

        public PipeMapComponent pipeNetManager;

        public bool hasPowerSource;

        public List<CompPipe> connectors = new List<CompPipe>();

        public List<CompPipe> transmitters = new List<CompPipe>();

        public List<CompPipeTrader> powerComps = new List<CompPipeTrader>();

        public List<CompPipeTank> batteryComps = new List<CompPipeTank>();

        private float debugLastCreatedEnergy;

        private float debugLastRawStoredEnergy;

        private float debugLastApparentStoredEnergy;

        private const int MaxRestartTryInterval = 200;

        private const int MinRestartTryInterval = 30;

        private const float RestartMinFraction = 0.05f;

        private const int ShutdownInterval = 20;

        private const float ShutdownMinFraction = 0.05f;

        private const float MinStoredEnergyToTurnOn = 5f;

        private static List<CompPipeTrader> partsWantingPowerOn = new List<CompPipeTrader>();

        private static List<CompPipeTrader> potentialShutdownParts = new List<CompPipeTrader>();

        private List<CompPipeTank> givingBats = new List<CompPipeTank>();

        private static List<CompPipeTank> batteriesShuffled = new List<CompPipeTank>();

        public GasPipeNet(IEnumerable<CompPipe> newTransmitters)
        {
            if (newTransmitters != null) {
                
                foreach (CompPipe compPipe in newTransmitters)
                {
                    if (compPipe != null) {
                        this.transmitters.Add(compPipe);
                        compPipe.transNet = this;
                        this.RegisterAllComponentsOf(compPipe.parent);
                        if (compPipe.connectChildren != null)
                        {
                            List<CompPipe> connectChildren = compPipe.connectChildren;
                            for (int i = 0; i < connectChildren.Count; i++)
                            {
                                this.RegisterConnector(connectChildren[i]);
                            }
                        }
                    }
                    
                }
                this.hasPowerSource = false;
                for (int j = 0; j < this.transmitters.Count; j++)
                {
                    if (this.IsPowerSource(this.transmitters[j]))
                    {
                        this.hasPowerSource = true;
                        break;
                    }
                }

            }
            
        }

        public Map Map
        {
            get
            {
                return this.pipeNetManager.map;
            }
        }

        public bool HasActivePowerSource
        {
            get
            {
                if (!this.hasPowerSource)
                {
                    return false;
                }
                for (int i = 0; i < this.transmitters.Count; i++)
                {
                    if (this.IsActivePowerSource(this.transmitters[i]))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private bool IsPowerSource(CompPipe cp)
        {
            return  cp is CompPipeTank || (cp is CompPipeTank && cp.Props.basePowerConsumption < 0f);
        }

        private bool IsActivePowerSource(CompPipe cp)
        {
            CompPipeTank compPowerBattery = cp as CompPipeTank;
            if (compPowerBattery != null && compPowerBattery.StoredEnergy > 0f)
            {
                return true;
            }
            CompPipeTrader compPowerTrader = cp as CompPipeTrader;
            return compPowerTrader != null && compPowerTrader.PowerOutput > 0f;
            
        }

        public void RegisterConnector(CompPipe b)
        {
            if (this.connectors.Contains(b))
            {
                Log.Error("PowerNet registered connector it already had: " + b, false);
                return;
            }
            this.connectors.Add(b);
            this.RegisterAllComponentsOf(b.parent);
        }

        public void DeregisterConnector(CompPipe b)
        {
            this.connectors.Remove(b);
            this.DeregisterAllComponentsOf(b.parent);
        }

        private void RegisterAllComponentsOf(ThingWithComps parentThing)
        {
            CompPipeTrader comp = parentThing.GetComp<CompPipeTrader>();
            if (comp != null)
            {
                if (this.powerComps.Contains(comp))
                {
                    //Log.Error("PowerNet adding powerComp " + comp + " which it already has.", false);
                }
                else
                {
                    this.powerComps.Add(comp);
                }
            }
            CompPipeTank comp2 = parentThing.GetComp<CompPipeTank>();
            if (comp2 != null)
            {
                if (this.batteryComps.Contains(comp2))
                {
                    //Log.Error("PowerNet adding batteryComp " + comp2 + " which it already has.", false);
                }
                else
                {
                    this.batteryComps.Add(comp2);
                }
            }
        }

        private void DeregisterAllComponentsOf(ThingWithComps parentThing)
        {
            CompPipeTrader comp = parentThing.GetComp<CompPipeTrader>();
            if (comp != null)
            {
                this.powerComps.Remove(comp);
            }
            CompPipeTank comp2 = parentThing.GetComp<CompPipeTank>();
            if (comp2 != null)
            {
                this.batteryComps.Remove(comp2);
            }
        }

        public float CurrentEnergyGainRate()
        {
            if (DebugSettings.unlimitedPower)
            {
                return 100000f;
            }
            float num = 0f;
            for (int i = 0; i < this.powerComps.Count; i++)
            {
                if (this.powerComps[i].PowerOn)
                {
                    num += this.powerComps[i].EnergyOutputPerTick;
                }
            }
            return num;
        }

        public float CurrentStoredEnergy()
        {
            float num = 0f;
            for (int i = 0; i < this.batteryComps.Count; i++)
            {
                num += this.batteryComps[i].StoredEnergy;
            }
            return num;
        }

        public void PowerNetTick()
        {
            float num = this.CurrentEnergyGainRate();
            float num2 = this.CurrentStoredEnergy();
            if (num2 + num >= -1E-07f)
            {
                float num3;
                if (this.batteryComps.Count > 0 && num2 >= 0.1f)
                {
                    num3 = num2 - 5f;
                }
                else
                {
                    num3 = num2;
                }
                
                if (num3 + num >= 0f)
                {
                    partsWantingPowerOn.Clear();
                    for (int i = 0; i < this.powerComps.Count; i++)
                    {
                        if (!this.powerComps[i].PowerOn && FlickUtility.WantsToBeOn(this.powerComps[i].parent) && !this.powerComps[i].parent.IsBrokenDown())
                        {
                            partsWantingPowerOn.Add(this.powerComps[i]);
                        }
                    }
                    if (partsWantingPowerOn.Count > 0)
                    {
                        int num4 = 200 / partsWantingPowerOn.Count;
                        if (num4 < 30)
                        {
                            num4 = 30;
                        }
                        if (Find.TickManager.TicksGame % num4 == 0)
                        {
                            int num5 = Mathf.Max(1, Mathf.RoundToInt((float)partsWantingPowerOn.Count * 0.05f));
                            for (int j = 0; j < num5; j++)
                            {
                                CompPipeTrader compPowerTrader = partsWantingPowerOn.RandomElement<CompPipeTrader>();
                                if (!compPowerTrader.PowerOn)
                                {
                                    if (num + num2 >= -(compPowerTrader.EnergyOutputPerTick + 1E-07f))
                                    {
                                        compPowerTrader.PowerOn = true;
                                        num += compPowerTrader.EnergyOutputPerTick;
                                    }
                                }
                            }
                        }
                    }
                }
                this.ChangeStoredEnergy(num);
            }
            else if (Find.TickManager.TicksGame % 20 == 0)
            {
                potentialShutdownParts.Clear();
                for (int k = 0; k < this.powerComps.Count; k++)
                {
                    if (this.powerComps[k].PowerOn && this.powerComps[k].EnergyOutputPerTick < 0f)
                    {
                        potentialShutdownParts.Add(this.powerComps[k]);
                    }
                }
                if (potentialShutdownParts.Count > 0)
                {
                    int num6 = Mathf.Max(1, Mathf.RoundToInt((float)potentialShutdownParts.Count * 0.05f));
                    for (int l = 0; l < num6; l++)
                    {
                        potentialShutdownParts.RandomElement<CompPipeTrader>().PowerOn = false;
                    }
                }
            }
        }

        private void ChangeStoredEnergy(float extra)
        {
            if (extra > 0f)
            {
                this.DistributeEnergyAmongBatteries(extra);
            }
            else
            {
                float num = -extra;
                this.givingBats.Clear();
                for (int i = 0; i < this.batteryComps.Count; i++)
                {
                    if (this.batteryComps[i].StoredEnergy > 1E-07f)
                    {
                        this.givingBats.Add(this.batteryComps[i]);
                    }
                }
                float a = num / (float)this.givingBats.Count;
                int num2 = 0;
                while (num > 1E-07f)
                {
                    for (int j = 0; j < this.givingBats.Count; j++)
                    {
                        float num3 = Mathf.Min(a, this.givingBats[j].StoredEnergy);
                        this.givingBats[j].DrawPower(num3);
                        num -= num3;
                        if (num < 1E-07f)
                        {
                            return;
                        }
                    }
                    num2++;
                    if (num2 > 10)
                    {
                        break;
                    }
                }
                if (num > 1E-07f)
                {
                    Log.Warning("Drew energy from a PowerNet that didn't have it.", false);
                }
            }
        }

        private void DistributeEnergyAmongBatteries(float energy)
        {
            if (energy <= 0f || !this.batteryComps.Any<CompPipeTank>())
            {
                return;
            }
            batteriesShuffled.Clear();
            batteriesShuffled.AddRange(this.batteryComps);
            batteriesShuffled.Shuffle<CompPipeTank>();
            int num = 0;
            for (; ; )
            {
                num++;
                if (num > 10000)
                {
                    break;
                }
                float num2 = float.MaxValue;
                for (int i = 0; i < batteriesShuffled.Count; i++)
                {
                    num2 = Mathf.Min(num2, batteriesShuffled[i].AmountCanAccept);
                }
                if (energy < num2 * (float)batteriesShuffled.Count)
                {
                    goto IL_129;
                }
                for (int j = batteriesShuffled.Count - 1; j >= 0; j--)
                {
                    float amountCanAccept = batteriesShuffled[j].AmountCanAccept;
                    bool flag = amountCanAccept <= 0f || amountCanAccept == num2;
                    if (num2 > 0f)
                    {
                        batteriesShuffled[j].AddEnergy(num2);
                        energy -= num2;
                    }
                    if (flag)
                    {
                        batteriesShuffled.RemoveAt(j);
                    }
                }
                if (energy < 0.0005f || !batteriesShuffled.Any<CompPipeTank>())
                {
                    goto IL_190;
                }
            }
            Log.Error("Too many iterations.", false);
            goto IL_19A;
            IL_129:
            float amount = energy / (float)batteriesShuffled.Count;
            for (int k = 0; k < batteriesShuffled.Count; k++)
            {
                batteriesShuffled[k].AddEnergy(amount);
            }
            energy = 0f;
            IL_190:
            IL_19A:
            batteriesShuffled.Clear();
        }

        public string DebugString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("POWERNET:");
            stringBuilder.AppendLine("  Created energy: " + this.debugLastCreatedEnergy);
            stringBuilder.AppendLine("  Raw stored energy: " + this.debugLastRawStoredEnergy);
            stringBuilder.AppendLine("  Apparent stored energy: " + this.debugLastApparentStoredEnergy);
            stringBuilder.AppendLine("  hasPowerSource: " + this.hasPowerSource);
            stringBuilder.AppendLine("  Connectors: ");
           /* foreach (CompPower compPower in this.connectors)
            {
                stringBuilder.AppendLine("      " + compPower.parent);
            }
            stringBuilder.AppendLine("  Transmitters: ");
            foreach (CompPower compPower2 in this.transmitters)
            {
                stringBuilder.AppendLine("      " + compPower2.parent);
            }
            stringBuilder.AppendLine("  powerComps: ");
            foreach (CompPowerTrader compPowerTrader in this.powerComps)
            {
                stringBuilder.AppendLine("      " + compPowerTrader.parent);
            }
            stringBuilder.AppendLine("  batteryComps: ");
            foreach (CompPowerBattery compPowerBattery in this.batteryComps)
            {
                stringBuilder.AppendLine("      " + compPowerBattery.parent);
            }*/
            return stringBuilder.ToString();
        }

       
    }
}

