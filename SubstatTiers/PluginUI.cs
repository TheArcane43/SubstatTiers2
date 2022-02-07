using ImGuiNET;
using System;
using System.Numerics;
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

            ImGui.SetNextWindowSize(new Vector2(320, 275), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(320, 275), new Vector2(float.MaxValue, float.MaxValue));
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

                ImGui.Text("Effects only consider traits and GCD buffs/traits.");
                Attributes a = Retrieval.GetDataFromGame();

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
                // physical/magical gcd split
                int[] physical = { 1, 2, 3, 4, 5, 19, 20, 21, 22, 23, 29, 30, 31, 32, 34, 37, 38, 39};

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
                string speedText;
                if (physical.Contains(a.JobId))
                {
                    calc.Speed = calc.SkillSpeed;
                    unitsSpeed = unitsSkillSpeed;
                    prevSpeed = prevSkillSpeed;
                    nextSpeed = nextSkillSpeed;
                    speedText = "Skill Speed";
                }
                else
                {
                    calc.Speed = calc.SpellSpeed;
                    unitsSpeed = unitsSpellSpeed;
                    prevSpeed = prevSpellSpeed;
                    nextSpeed = nextSpellSpeed;
                    speedText = "Spell Speed";
                }

                // Haste calculations
                int[,] HastePGL = { { 1, 5 }, { 20, 10 } };
                int[,] HasteMNK = { { 1, 5 }, { 20, 10 }, { 40, 15 }, { 76, 20 } };
                int[,] HasteBRD = { { 1, 0 }, { 40, 16 } };
                int[,] HasteWHM = { { 1, 0 }, { 30, 20 } };
                int[,] HasteBLM = { { 1, 0 }, { 52, 15 } };
                int[,] HasteNIN = { { 1, 0 }, { 45, 15 } };
                int[,] HasteSAM = { { 1, 0 }, { 18, 10 }, { 78, 13 } };
                int[,] HasteNone = { { 0, 0 } };
                int[,] HasteArray = a.JobId switch
                {
                    2 => HastePGL,
                    20 => HasteMNK,
                    23 => HasteBRD,
                    24 => HasteWHM,
                    25 => HasteBLM,
                    30 => HasteNIN,
                    34 => HasteSAM,
                    _ => HasteNone,
                };

                int hasteAmt = 0;
                for (int i = 0; i < HasteArray.Length / 2; i++)
                {
                    if (HasteArray[i, 0] <= calc.Level)
                    {
                        hasteAmt = HasteArray[i, 1];
                    }
                }
                Console.WriteLine(hasteAmt);

                string HasteName = a.JobId switch
                {
                    2 => "GL",
                    20 => "GL",
                    23 => "4 Paeon",
                    24 => "PoM",
                    25 => "LL",
                    30 => "Huton",
                    34 => "Fuka",
                    _ => "",
                };

                double gcd = Formulas.GCDFormula(unitsSpeed, 0);
                double gcdModified = Formulas.GCDFormula(unitsSpeed, hasteAmt);


                // ImGui.Text(calc.ToString());

                object[][] data = new object[9][];
                data[0] = new object[] { "Critical Rate", $"{unitsCritHit * 0.001 + 0.05:P1}", calc.CritHit, prevCritHit - calc.CritHit, (nextCritHit - calc.CritHit).ToString("+0") };
                data[1] = new object[] { "Crit Damage", $"{unitsCritHit * 0.001 + 0.40:P1}", "", "", "" };
                data[2] = new object[] { "Determination", $"{unitsDetermination * 0.001:P1}", calc.Determination, prevDetermination - calc.Determination, (nextDetermination - calc.Determination).ToString("+0")};
                data[3] = new object[] { "Direct Hit Rate", $"{unitsDirectHit * 0.001:P1}", calc.DirectHit, prevDirectHit - calc.DirectHit, (nextDirectHit - calc.DirectHit).ToString("+0") };
                data[4] = new object[] { speedText, $"{unitsSpeed * 0.001:P1}", calc.Speed, prevSpeed - calc.Speed, (nextSpeed - calc.Speed).ToString("+0") };
                data[5] = new object[] { "Tenacity", $"{unitsTenacity * 0.001:P1}", calc.Tenacity, prevTenacity - calc.Tenacity, (nextTenacity - calc.Tenacity).ToString("+0") };
                data[6] = new object[] { "Piety", $"{unitsPiety + 200} MP", calc.Piety, prevPiety - calc.Piety, (nextPiety - calc.Piety).ToString("+0") };
                data[7] = new object[] {$"GCD", $"{gcd:F2}", "",  "", "" };
                data[8] = new object[] {$"GCD ({HasteName})", $"{gcdModified:F2}", "", "", "" };

                ImGui.Spacing();

                ImGuiTableFlags flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders;

                if (ImGui.BeginTable("table1", 5, flags))
                {
                    ImGui.TableSetupColumn($"Level {calc.Level}",ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn($"Effect", ImGuiTableColumnFlags.WidthFixed, 60);
                    ImGui.TableSetupColumn($"Stat", ImGuiTableColumnFlags.WidthFixed, 50);
                    ImGui.TableSetupColumn($"Prev", ImGuiTableColumnFlags.WidthFixed, 40);
                    ImGui.TableSetupColumn($"Next", ImGuiTableColumnFlags.WidthFixed, 40);
                    ImGui.TableHeadersRow();

                    int MaxRows = 8 + ((hasteAmt>0)?1:0);

                    for (int row = 0; row < MaxRows; row++)
                    {
                        ImGui.TableNextRow();
                        for (int col = 0; col < 5; col++)
                        {
                            ImGui.TableSetColumnIndex(col);
                            ImGui.TextUnformatted(data[row][col].ToString());
                        }
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
