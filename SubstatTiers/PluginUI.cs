using ImGuiNET;
using System;
using System.Numerics;
using System.Collections.Generic;
using Dalamud;
using Dalamud.Logging;
using System.Linq;

namespace SubstatTiers
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private Configuration configuration;

        private ImGuiScene.TextureWrap goatImage;

        // Have an instance of the AttributeData ready at all times
        private AttributeData? attributeData = null;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }

        // passing in the image here just for simplicity
        public PluginUI(Configuration configuration, ImGuiScene.TextureWrap goatImage)
        {
            this.configuration = configuration;
            this.goatImage = goatImage;
        }

        public void Dispose()
        {
            this.goatImage.Dispose();
        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawMainWindow();
            DrawSettingsWindow();

        }

        public unsafe void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(310, 333), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(310, 333), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Substat Tiers", ref this.visible, ImGuiWindowFlags.None))
            {

                if (ImGui.Button("Show Settings"))
                {
                    SettingsVisible = true;
                }

                //ImGui.Spacing();

                //ImGui.Text("Have a goat:");
                //ImGui.Indent(55);
                //ImGui.Image(this.goatImage.ImGuiHandle, new Vector2(this.goatImage.Width, this.goatImage.Height));
                //ImGui.Unindent(55);
                
                this.attributeData = new();
                
                if (attributeData is null || !attributeData.IsLoaded)
                {
                    ImGui.Text("Unable to obtain character info.");
                    ImGui.End();
                    return;
                }
                if (attributeData.CriticalHit < 20 || attributeData.SkillSpeed < 20)
                {
                    ImGui.Text("Substat Tiers does not work in this area.");
                    ImGui.End();
                    return;
                }
                if (attributeData.IsHandLand())
                {
                    ImGui.Text("Substats do not apply for your current class/job.");
                    ImGui.End();
                    return;
                }
                Calculations calc = new(attributeData);


                // Main table -------------------------------------------------

                ImGui.Text("Effects only consider traits, GCD buffs/traits, and any active stat buffs. (e.g. food)");
                ImGui.Text("Substats unrelated to this class/job are excluded.");

                int unitsCritHit = calc.GetUnits(StatConstants.SubstatType.Crit);
                int unitsDetermination = calc.GetUnits(StatConstants.SubstatType.Det);
                int unitsDirectHit = calc.GetUnits(StatConstants.SubstatType.Direct);
                int unitsSkillSpeed = calc.GetUnits(StatConstants.SubstatType.SkSpd);
                int unitsSpellSpeed = calc.GetUnits(StatConstants.SubstatType.SpSpd);
                int unitsTenacity = calc.GetUnits(StatConstants.SubstatType.Ten);
                int unitsPiety = calc.GetUnits(StatConstants.SubstatType.Piety);

                int prevCritHit = calc.GetStatsFromUnits(StatConstants.SubstatType.Crit, unitsCritHit);
                int prevDetermination = calc.GetStatsFromUnits(StatConstants.SubstatType.Det, unitsDetermination);
                int prevDirectHit = calc.GetStatsFromUnits(StatConstants.SubstatType.Direct, unitsDirectHit);
                int prevSkillSpeed = calc.GetStatsFromUnits(StatConstants.SubstatType.SkSpd, unitsSkillSpeed);
                int prevSpellSpeed = calc.GetStatsFromUnits(StatConstants.SubstatType.SpSpd, unitsSpellSpeed);
                int prevTenacity = calc.GetStatsFromUnits(StatConstants.SubstatType.Ten, unitsTenacity);
                int prevPiety = calc.GetStatsFromUnits(StatConstants.SubstatType.Piety, unitsPiety);
                int prevGCDBase = calc.GetStatsFromUnits(calc.Data.SpeedType, calc.GetSpeedUnitsOfGCDbase());
                int prevGCDModified = calc.GetStatsFromUnits(calc.Data.SpeedType, calc.GetSpeedUnitsOfGCDmodified());

                int nextCritHit = calc.GetStatsFromUnits(StatConstants.SubstatType.Crit, unitsCritHit + 1);
                int nextDetermination = calc.GetStatsFromUnits(StatConstants.SubstatType.Det, unitsDetermination + 1);
                int nextDirectHit = calc.GetStatsFromUnits(StatConstants.SubstatType.Direct, unitsDirectHit + 1);
                int nextSkillSpeed = calc.GetStatsFromUnits(StatConstants.SubstatType.SkSpd, unitsSkillSpeed + 1);
                int nextSpellSpeed = calc.GetStatsFromUnits(StatConstants.SubstatType.SpSpd, unitsSpellSpeed + 1);
                int nextTenacity = calc.GetStatsFromUnits(StatConstants.SubstatType.Ten, unitsTenacity + 1);
                int nextPiety = calc.GetStatsFromUnits(StatConstants.SubstatType.Piety, unitsPiety + 1);
                int nextGCDBase = calc.GetStatsFromUnits(calc.Data.SpeedType, calc.GetSpeedUnitsOfNextGCDbase());
                int nextGCDModified = calc.GetStatsFromUnits(calc.Data.SpeedType, calc.GetSpeedUnitsOfNextGCDmodified());

                // Defense units for effects only
                int unitsDefense = calc.GetUnits(StatConstants.SubstatType.Defense);
                int unitsMagicDefense = calc.GetUnits(StatConstants.SubstatType.MagicDefense);

                // List of stats/tiers
                List<VisibleInfo> statList = new();
                statList.Add(new VisibleInfo(StatConstants.SubstatType.Crit.VisibleName(), calc.Data.CriticalHit, calc.Data.CriticalHit - prevCritHit, nextCritHit - calc.Data.CriticalHit));
                statList.Add(new VisibleInfo(StatConstants.SubstatType.Det.VisibleName(), calc.Data.Determination, calc.Data.Determination - prevDetermination, nextDetermination - calc.Data.Determination));
                statList.Add(new VisibleInfo(StatConstants.SubstatType.Direct.VisibleName(), calc.Data.DirectHit, calc.Data.DirectHit - prevDirectHit, nextDirectHit - calc.Data.DirectHit));
                if (attributeData.UsesAttackPower())
                {
                    statList.Add(new VisibleInfo(StatConstants.SubstatType.SkSpd.VisibleName(), calc.Data.SkillSpeed, calc.Data.SkillSpeed - prevSkillSpeed, nextSkillSpeed - calc.Data.SkillSpeed));
                }
                else
                {
                    statList.Add(new VisibleInfo(StatConstants.SubstatType.SpSpd.VisibleName(), calc.Data.SpellSpeed, calc.Data.SpellSpeed - prevSpellSpeed, nextSpellSpeed - calc.Data.SpellSpeed));
                }
                if (attributeData.IsTank())
                {
                    statList.Add(new VisibleInfo(StatConstants.SubstatType.Ten.VisibleName(), calc.Data.Tenacity, calc.Data.Tenacity - prevTenacity, nextTenacity - calc.Data.Tenacity));
                }
                if (attributeData.IsHealer())
                {
                    statList.Add(new VisibleInfo(StatConstants.SubstatType.Piety.VisibleName(), calc.Data.Piety, calc.Data.Piety - prevPiety, nextPiety - calc.Data.Piety));
                }

                statList.Add(new VisibleInfo("GCD(Base)", calc.GetGCDbase(), calc.Speed - prevGCDBase, nextGCDBase - calc.Speed));

                if (attributeData.HasteAmount() > 0)
                {
                    statList.Add(new VisibleInfo("GCD +", calc.GetGCDmodified(), calc.Speed - prevGCDModified, nextGCDModified - calc.Speed));
                }

                // List of effects
                List<VisibleEffect> effects = new();
                effects.Add(new VisibleEffect("Critical Rate", $"{unitsCritHit * 0.001 + 0.05:P1}", "The frequency of critical hits"));
                effects.Add(new VisibleEffect("Critical Damage", $"+{unitsCritHit * 0.001 + 0.40:P1}", "Damage bonus when you hit a critical hit"));
                effects.Add(new VisibleEffect("Determination", $"+{unitsDetermination * 0.001:P1}", "Overall increase in outgoing damage and healing"));
                effects.Add(new VisibleEffect("Direct Hit Rate", $"{unitsDirectHit * 0.001:P1}", "The frequency of direct hits"));
                if (attributeData.UsesAttackPower())
                {
                    effects.Add(new VisibleEffect("DoT Bonus", $"+{unitsSkillSpeed * 0.001:P1}", "Damage bonus on damage over time effects"));
                }
                else
                {
                    effects.Add(new VisibleEffect("DoT Bonus", $"+{unitsSpellSpeed * 0.001:P1}", "Damage bonus on damage over time effects"));
                }
                if (attributeData.IsTank())
                {
                    effects.Add(new VisibleEffect("Tenacity Bonus", $"+{unitsTenacity * 0.001:P1}", "Extra damage, mitigation, and outgoing healing as a tank"));
                }
                if (attributeData.IsHealer())
                {
                    effects.Add(new VisibleEffect("MP Regen per tick", $"{unitsPiety + 200} MP", "MP recovery every 3 seconds"));
                }
                effects.Add(new VisibleEffect("GCD (Base)", $"{calc.GetGCDbase():F2}", "Recast time for most actions with a base of 2.50 seconds"));
                if (attributeData.HasteAmount() > 0)
                {
                    effects.Add(new VisibleEffect($"GCD ({attributeData.HasteName()})", $"{calc.GetGCDmodified():F2}", "Recast time when under the given effect"));
                }
                effects.Add(new VisibleEffect("Defense", $"{unitsDefense}%", "Physical Damage Mitigation due to Defense stat"));
                effects.Add(new VisibleEffect("Magic Defense", $"{unitsMagicDefense}%", "Magical Damage Mitigation due to Magic Defense stat"));

                // List of materia tiers
                List<VisibleMateria> materiaTiers = new();
                materiaTiers.Add(new VisibleMateria(calc, StatConstants.SubstatType.Crit));
                materiaTiers.Add(new VisibleMateria(calc, StatConstants.SubstatType.Det));
                materiaTiers.Add(new VisibleMateria(calc, StatConstants.SubstatType.Direct));
                if (attributeData.UsesAttackPower())
                {
                    materiaTiers.Add(new VisibleMateria(calc, StatConstants.SubstatType.SkSpd));
                }
                else
                {
                    materiaTiers.Add(new VisibleMateria(calc, StatConstants.SubstatType.SpSpd));
                }
                if (attributeData.IsTank())
                {
                    materiaTiers.Add(new VisibleMateria(calc, StatConstants.SubstatType.Ten));
                }
                if (attributeData.IsHealer())
                {
                    materiaTiers.Add(new VisibleMateria(calc, StatConstants.SubstatType.Piety));
                }
                materiaTiers.Add(new VisibleMateria(calc, StatConstants.SubstatType.GCDbase));
                if (attributeData.HasteAmount() > 0)
                {
                    materiaTiers.Add(new VisibleMateria(calc, StatConstants.SubstatType.GCDmodified));
                }

                // List of damage potencies
                List<VisibleDamage> damageNums = new();
                damageNums.Add(new VisibleDamage("Normal Damage", calc.DamageFormula(false, false)));
                if (configuration.ShowVerboseDamage)
                {
                    damageNums.Add(new VisibleDamage("Damage on Critical Hits", calc.DamageFormula(true, false)));
                    damageNums.Add(new VisibleDamage("Damage on Direct Hits", calc.DamageFormula(false, true)));
                    damageNums.Add(new VisibleDamage("Damage on Critical Direct Hits", calc.DamageFormula(true, true)));
                }
                damageNums.Add(new VisibleDamage("Average Damage per 100 potency", calc.DamageAverage()));
                // Damage over time row
                if (configuration.ShowVerboseDamage)
                {
                    damageNums.Add(new VisibleDamage("Damage Over Time Average", calc.DamageOverTimeAverage()));
                }

                ImGui.Spacing();

                ImGuiTableFlags flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.NoHostExtendX;

                // Beginning of layout - grid(1)
                // Stat Table -------------------------------------------------
                if (ImGui.BeginTable("tableStats", 4, flags))
                {
                    ImGui.TableSetupColumn($"{attributeData.GetJobTL()} Lv{calc.Data.Level}", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn($"Stat", ImGuiTableColumnFlags.WidthFixed, 50);
                    ImGui.TableSetupColumn($"Over", ImGuiTableColumnFlags.WidthFixed, 40);
                    ImGui.TableSetupColumn($"Next", ImGuiTableColumnFlags.WidthFixed, 40);
                    ImGui.TableHeadersRow();

                    // Stat Table
                    foreach (var row in statList)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.TextUnformatted(row.Name);
                        if (ImGui.IsItemHovered())
                        {
                            if (row.Name == "GCD +")
                            {
                                ImGui.SetTooltip("GCD including job-specific speed boosts");
                            }
                        }
                        ImGui.TableSetColumnIndex(1);
                        ImGui.TextUnformatted(row.Stat);
                        ImGui.TableSetColumnIndex(2);
                        ImGui.TextUnformatted(row.Prev);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("Amount of stat wasted for this tier");
                        }
                        ImGui.TableSetColumnIndex(3);
                        ImGui.TextUnformatted(row.Next);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("Amount of stat required to reach the next tier");
                        }
                    }

                    ImGui.EndTable();

                }

                

                // Materia tiers table ----------------------------------------
                if (configuration.ShowMateriaTiers)
                {
                    // This table to the right of previous table if horizontal or grid(2) layout
                    if (configuration.LayoutType == 0 || configuration.LayoutType == 1)
                    {
                        ImGui.SameLine();
                    }
                    else // This table below previous table if vertical layout
                    {
                        ImGui.Spacing();
                    }

                    // Materia table setup
                    if (ImGui.BeginTable("tableMateria", 5, flags))
                    {
                        int[] titles = VisibleMateria.MateriaTiersAt(attributeData.Level);
                        ImGui.TableSetupColumn("Materia:", ImGuiTableColumnFlags.WidthFixed, 100);
                        ImGui.TableSetupColumn($"+ {titles[0]}", ImGuiTableColumnFlags.WidthFixed, 30);
                        ImGui.TableSetupColumn($"+ {titles[1]}", ImGuiTableColumnFlags.WidthFixed, 30);
                        ImGui.TableSetupColumn($"+ {titles[2]}", ImGuiTableColumnFlags.WidthFixed, 30);
                        ImGui.TableSetupColumn($"+ {titles[3]}", ImGuiTableColumnFlags.WidthFixed, 30);
                        ImGui.TableHeadersRow();

                        // materia table
                        foreach (var row in materiaTiers)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            ImGui.TextUnformatted(row.EffectName);
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip(row.EffectTooltip);
                            }
                            for (int i = 0; i < row.EffectTiers.Length; i++)
                            {
                                ImGui.TableSetColumnIndex(1 + i);
                                ImGui.TextUnformatted(row.EffectTiers[i]);
                            }
                        }
                        ImGui.EndTable();
                    }
                }

                
                // Stat Effects Table -----------------------------------------

                if (configuration.ShowSubstatEffects)
                {

                    // This table to the right of previous table if horizontal layout or grid layout with previous table missing
                    if (configuration.LayoutType == 0 || (configuration.LayoutType == 1 && !configuration.ShowMateriaTiers))
                    {
                        ImGui.SameLine();
                    }
                    else // This table below previous table if vertical or grid layout
                    {
                        ImGui.Spacing();
                    }

                    // Effect Table Setup
                    if (ImGui.BeginTable("tableEffects", 2, flags))
                    {
                        ImGui.TableSetupColumn($"Stat", ImGuiTableColumnFlags.WidthFixed, 150);
                        ImGui.TableSetupColumn($"Effect", ImGuiTableColumnFlags.WidthFixed, 50);
                        ImGui.TableHeadersRow();

                        // Effect Table
                        foreach (var row in effects)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            ImGui.TextUnformatted(row.EffectName);
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip(row.EffectTooltip);
                            }
                            ImGui.TableSetColumnIndex(1);
                            ImGui.TextUnformatted(row.EffectAmount);
                        }

                        ImGui.EndTable();

                    }
                } // end stat effect table


                if (configuration.ShowDamagePotency)
                {

                    // This table to the right of previous table if horizontal or grid layout with all tables
                    if (configuration.LayoutType == 0 || (configuration.LayoutType == 1 && configuration.ShowSubstatEffects && configuration.ShowMateriaTiers))
                    {
                        ImGui.SameLine();
                    }
                    else // This table below previous table if vertical layout or grid layout with missing table(s)
                    {
                        ImGui.Spacing();
                    }

                    // Blue mage's effective Magic Damage on weapon is some function of Intelligence (formula unknown)
                    if (calc.Data.JobId == JobThreeLetter.BLU)
                    {
                        ImGui.Text("Blue Mage's damage potency numbers are not supported."); // Sorry!
                    }
                    else if (calc.Data.IsSynced && !calc.Data.HasAccurateWeaponDamage) // Cannot determine synced weapon damage
                    {
                        ImGui.Text("Damage potency numbers cannot be calculated for synced content."); 
                    }
                    else
                    {
                        // Potency Table setup
                        if (ImGui.BeginTable("tablePotency", 2, flags))
                        {
                            ImGui.TableSetupColumn("Per 100 potency", ImGuiTableColumnFlags.WidthFixed);
                            ImGui.TableSetupColumn("Amount", ImGuiTableColumnFlags.WidthFixed);
                            ImGui.TableHeadersRow();
                            foreach (var row in damageNums)
                            {
                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(0);
                                ImGui.TextUnformatted(row.DamageName);
                                ImGui.TableSetColumnIndex(1);
                                ImGui.TextUnformatted(row.DamageNumber);
                            }
                        }
                        ImGui.EndTable();
                    }

                }
                
            }
            ImGui.End();
        }

        public void DrawSettingsWindow()
        {
            
            if (!SettingsVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(275, 220), ImGuiCond.Always);
            if (ImGui.Begin("Substat Tiers Settings", ref this.settingsVisible,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (ImGui.Button("Show Substat Tier Window"))
                {
                    this.Visible = true;
                }


                // can't ref a property, so use a local copy
                var configValue = this.configuration.ShowSubstatEffects;
                if (ImGui.Checkbox("Show Substat Effect table", ref configValue))
                {
                    this.configuration.ShowSubstatEffects = configValue;
                    // can save immediately on change, if you don't want to provide a "Save and Close" button
                    this.configuration.Save();
                }

                
                var configValue2 = this.configuration.ShowMateriaTiers;
                if (ImGui.Checkbox("Show Materia Tier table", ref configValue2))
                {
                    this.configuration.ShowMateriaTiers = configValue2;
                    this.configuration.Save();
                }

                var configValue3 = this.configuration.ShowDamagePotency;
                if (ImGui.Checkbox("Show Damage Potency", ref configValue3))
                {
                    this.configuration.ShowDamagePotency = configValue3;
                    this.configuration.Save();
                }
                ImGui.Indent(25);
                var configValue5 = this.configuration.ShowVerboseDamage;
                if (configuration.ShowDamagePotency)
                {
                    if (ImGui.Checkbox("Verbose Damage output", ref configValue5))
                    {
                        this.configuration.ShowVerboseDamage = configValue5;
                        this.configuration.Save();
                    }
                }
                ImGui.Unindent(25);

                var configValue4 = this.configuration.LayoutType;
                string[] layouts = { "Horizontal", "Grid", "Vertical" };
                ImGui.SetNextItemWidth(100);
                if (ImGui.BeginCombo("Layout", layouts[configValue4]))
                {
                    for (var i = 0; i < layouts.Length; i++)
                    {
                        bool is_selected = configValue4 == i;
                        if (ImGui.Selectable(layouts[i], is_selected))
                        {
                            configValue4 = i;
                            this.configuration.LayoutType = configValue4;
                            this.configuration.Save();
                        }
                        // initial selection
                        if (is_selected) ImGui.SetItemDefaultFocus();
                    }
                    ImGui.EndCombo();
                }

            }
            ImGui.End();
            
        }
    }
}
