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

            ImGui.SetNextWindowSize(new Vector2(300, 300), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(300, 300), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Substat Tiers", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                //ImGui.Text($"The random config bool is {this.configuration.SomePropertyToBeSavedAndWithADefault}");

                //if (ImGui.Button("Show Settings"))
                //{
                //    SettingsVisible = true;
                //}

                //ImGui.Spacing();

                //ImGui.Text("Have a goat:");
                //ImGui.Indent(55);
                //ImGui.Image(this.goatImage.ImGuiHandle, new Vector2(this.goatImage.Width, this.goatImage.Height));
                //ImGui.Unindent(55);

                Attributes a = Retrieval.GetDataFromGame();
                if (a.CriticalHit < 20 || a.SkillSpeed < 20)
                {
                    ImGui.Text("Substat Tiers does not work in PvP.\r\nNo substats available.");
                    ImGui.End();
                    return;
                }
                Calculations calc = new()
                {
                    Level = a.Level,
                    CritHit = a.CriticalHit,
                    Determination = a.Determination,
                    DirectHit = a.DirectHit,
                    SkillSpeed = a.SkillSpeed,
                    SpellSpeed = a.SpellSpeed,
                    Tenacity = a.Tenacity,
                    Piety = a.Piety,
                };

                // TODO: change this to get info from game
                int jobId = a.JobId;
                Job.ClassJobData jobData;
                if (Job.JobData.ContainsKey(jobId))
                {
                    jobData = Job.JobData[jobId];
                }
                else
                {
                    ImGui.Text("Substats do not apply for your current class/job.");
                    return;
                }

                ImGui.Text("Effects only consider traits and GCD buffs/traits.");
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

                int nextCritHit = calc.GetStatsFromUnits(StatConstants.SubstatType.Crit, unitsCritHit + 1);
                int nextDetermination = calc.GetStatsFromUnits(StatConstants.SubstatType.Det, unitsDetermination + 1);
                int nextDirectHit = calc.GetStatsFromUnits(StatConstants.SubstatType.Direct, unitsDirectHit + 1);
                int nextSkillSpeed = calc.GetStatsFromUnits(StatConstants.SubstatType.SkSpd, unitsSkillSpeed + 1);
                int nextSpellSpeed = calc.GetStatsFromUnits(StatConstants.SubstatType.SpSpd, unitsSpellSpeed + 1);
                int nextTenacity = calc.GetStatsFromUnits(StatConstants.SubstatType.Ten, unitsTenacity + 1);
                int nextPiety = calc.GetStatsFromUnits(StatConstants.SubstatType.Piety, unitsPiety + 1);

                int unitsSpeed, prevSpeed, nextSpeed;
                if (jobData.IsPhysical)
                {
                    calc.Speed = calc.SkillSpeed;
                    unitsSpeed = unitsSkillSpeed;
                    prevSpeed = prevSkillSpeed;
                    nextSpeed = nextSkillSpeed;
                }
                else
                {
                    calc.Speed = calc.SpellSpeed;
                    unitsSpeed = unitsSpellSpeed;
                    prevSpeed = prevSpellSpeed;
                    nextSpeed = nextSpellSpeed;
                }

                int[,] HasteArray = jobData.HasteValues;

                int hasteAmt = 0;
                for (int i = 0; i < HasteArray.Length / 2; i++)
                {
                    if (HasteArray[i, 0] <= calc.Level)
                    {
                        hasteAmt = HasteArray[i, 1];
                    }
                }

                double gcd = Formulas.GCDFormula(unitsSpeed, 0);
                double gcdModified = Formulas.GCDFormula(unitsSpeed, hasteAmt);

                // List of stats/tiers
                List<VisibleInfo> statList = new();
                statList.Add(new VisibleInfo("Critical Hit", calc.CritHit, prevCritHit - calc.CritHit, nextCritHit - calc.CritHit));
                statList.Add(new VisibleInfo("Determination", calc.Determination, prevDetermination - calc.Determination, nextDetermination - calc.Determination));
                statList.Add(new VisibleInfo("Direct Hit Rate", calc.DirectHit, prevDirectHit - calc.DirectHit, nextDirectHit - calc.DirectHit));
                if (jobData.IsPhysical)
                {
                    statList.Add(new VisibleInfo("Skill Speed", calc.SkillSpeed, prevSkillSpeed - calc.SkillSpeed, nextSkillSpeed - calc.SkillSpeed));
                }
                else
                {
                    statList.Add(new VisibleInfo("Spell Speed", calc.SpellSpeed, prevSpellSpeed - calc.SpellSpeed, nextSpellSpeed - calc.SpellSpeed));
                }
                if (jobData.RoleStat == Job.RoleStats.Tenacity)
                {
                    statList.Add(new VisibleInfo("Tenacity", calc.Tenacity, prevTenacity - calc.Tenacity, nextTenacity - calc.Tenacity));
                }
                if (jobData.RoleStat == Job.RoleStats.Piety)
                {
                    statList.Add(new VisibleInfo("Piety", calc.Piety, prevPiety - calc.Piety, nextPiety - calc.Piety));
                }

                // List of effects
                List<VisibleEffect> effects = new();
                effects.Add(new VisibleEffect("Critical Rate", $"{unitsCritHit * 0.001 + 0.05:P1}"));
                effects.Add(new VisibleEffect("Critical Damage", $"+{unitsCritHit * 0.001 + 0.40:P1}"));
                effects.Add(new VisibleEffect("Determination", $"+{unitsDetermination * 0.001:P1}"));
                effects.Add(new VisibleEffect("Direct Hit Rate", $"{unitsDirectHit * 0.001:P1}"));
                effects.Add(new VisibleEffect("DoT Bonus", $"+{unitsSpeed * 0.001:P1}"));
                if (jobData.RoleStat == Job.RoleStats.Tenacity)
                {
                    effects.Add(new VisibleEffect("Bonus Damage/Mitigation", $"+{unitsTenacity * 0.001:P1}"));
                }
                if (jobData.RoleStat == Job.RoleStats.Piety)
                {
                    effects.Add(new VisibleEffect("MP Regen per tick", $"{unitsPiety + 200} MP"));
                }
                effects.Add(new VisibleEffect("GCD (Base)", $"{gcd:F2}"));
                if (jobData.HasHaste)
                {
                    effects.Add(new VisibleEffect($"GCD ({jobData.HasteName})", $"{gcdModified:F2}"));
                }
                

                ImGui.Spacing();

                ImGuiTableFlags flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders;

                // Stat Table Setup
                if (ImGui.BeginTable("tableStats", 4, flags))
                {
                    ImGui.TableSetupColumn($"{jobData.JobThreeLetter} Lv{calc.Level}",ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn($"Stat", ImGuiTableColumnFlags.WidthFixed, 50);
                    ImGui.TableSetupColumn($"Prev", ImGuiTableColumnFlags.WidthFixed, 40);
                    ImGui.TableSetupColumn($"Next", ImGuiTableColumnFlags.WidthFixed, 40);
                    ImGui.TableHeadersRow();

                    // Stat Table
                    foreach (var row in statList)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.TextUnformatted(row.Name);
                        ImGui.TableSetColumnIndex(1);
                        ImGui.TextUnformatted(row.Stat);
                        ImGui.TableSetColumnIndex(2);
                        ImGui.TextUnformatted(row.Prev);
                        ImGui.TableSetColumnIndex(3);
                        ImGui.TextUnformatted(row.Next);
                    }

                    // old
                    /*
                    for (int row = 0; row < MaxRows; row++)
                    {
                        ImGui.TableNextRow();
                        for (int col = 0; col < 5; col++)
                        {
                            ImGui.TableSetColumnIndex(col);
                            ImGui.TextUnformatted(data[row][col].ToString());
                        }
                    }
                    */
                    ImGui.EndTable();

                }
                ImGui.Spacing();

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
                        ImGui.TableSetColumnIndex(1);
                        ImGui.TextUnformatted(row.EffectAmount);
                    }

                    ImGui.EndTable();

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

            ImGui.SetNextWindowSize(new Vector2(232, 75), ImGuiCond.Always);
            if (ImGui.Begin("Settings Window", ref this.settingsVisible,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                // can't ref a property, so use a local copy
                var configValue = this.configuration.SomePropertyToBeSavedAndWithADefault;
                if (ImGui.Checkbox("Button That Does Nothing", ref configValue))
                {
                    this.configuration.SomePropertyToBeSavedAndWithADefault = configValue;
                    // can save immediately on change, if you don't want to provide a "Save and Close" button
                    this.configuration.Save();
                }
            }
            ImGui.End();
        }
    }
}
