using System.Numerics;
using System.Text.Json.Serialization;
using Dalamud.Bindings.ImGui;
using LMeter.Helpers;

namespace LMeter.Config
{
    public class BarConfig : IConfigPage
    {
        [JsonIgnore]
        public bool Active { get; set; }
        public string Name => "伤害数据";

        private static readonly string[] _jobIconStyleOptions = ["类型1", "类型2"];

        public int BarHeightType = 0;
        public int BarCount = 8;
        public int BarGaps = 1;
        public float BarHeight = 25;

        public bool ShowJobIcon = true;
        public int JobIconSizeType = 0;
        public Vector2 JobIconSize = new(25, 25);
        public int JobIconStyle = 0;
        public Vector2 JobIconOffset = new(0, 0);
        public ConfigColor JobIconBackgroundColor = new(0, 0, 0, 0);

        public bool ThousandsSeparators = true;

        public bool UseJobColor = true;
        public ConfigColor BarColor = new(.3f, .3f, .3f, 1f);

        public bool ShowRankText = false;
        public string RankTextFormat = "[rank].";
        public DrawAnchor RankTextAlign = DrawAnchor.Right;
        public Vector2 RankTextOffset = new(0, 0);
        public bool RankTextJobColor = false;
        public ConfigColor RankTextColor = new(1, 1, 1, 1);
        public bool RankTextShowOutline = true;
        public ConfigColor RankTextOutlineColor = new(0, 0, 0, 0.5f);
        public string RankTextFontKey = FontsManager.DalamudFontKey;
        public int RankTextFontId = 0;
        public bool AlwaysShowSelf = false;

        public string LeftTextFormat = "[name]";
        public Vector2 LeftTextOffset = new(0, 0);
        public bool LeftTextJobColor = false;
        public ConfigColor BarNameColor = new(1, 1, 1, 1);
        public bool BarNameShowOutline = true;
        public ConfigColor BarNameOutlineColor = new(0, 0, 0, 0.5f);
        public string BarNameFontKey = FontsManager.DalamudFontKey;
        public int BarNameFontId = 0;
        public bool UseCharacterName = false;

        public string RightTextFormat = "[damagetotal:k.1]  ([dps:k.1], [damagepct])";
        public Vector2 RightTextOffset = new(0, 0);
        public bool RightTextJobColor = false;
        public ConfigColor BarDataColor = new(1, 1, 1, 1);
        public bool BarDataShowOutline = true;
        public ConfigColor BarDataOutlineColor = new(0, 0, 0, 0.5f);
        public string BarDataFontKey = FontsManager.DalamudFontKey;
        public int BarDataFontId = 0;

        public bool ShowColumnHeader;
        public float ColumnHeaderHeight = 25;
        public ConfigColor ColumnHeaderColor = new(0, 0, 0, 0.5f);
        public bool UseColumnFont = true;
        public ConfigColor ColumnHeaderTextColor = new(1, 1, 1, 1);
        public bool ColumnHeaderShowOutline = true;
        public ConfigColor ColumnHeaderOutlineColor = new(0, 0, 0, 0.5f);
        public Vector2 ColumnHeaderOffset = new();
        public string ColumnHeaderFontKey = FontsManager.DalamudFontKey;
        public int ColumnHeaderFontId = 0;

        public float BarFillHeight = 1f;
        public ConfigColor BarBackgroundColor = new(.3f, .3f, .3f, 1f);
        public int BarFillDirection = 0;

        public bool UseCustomColorForSelf;
        public ConfigColor CustomColorForSelf = new(218f / 255f, 157f / 255f, 46f / 255f, 1f);

        public RoundingOptions TopBarRounding = new(false, 10f, RoundingFlag.Top);
        public RoundingOptions MiddleBarRounding = new(false, 10f, RoundingFlag.All);
        public RoundingOptions BottomBarRounding = new(false, 10f, RoundingFlag.BottomLeft);

        public IConfigPage GetDefault()
        {
            BarConfig defaultConfig = new()
            {
                BarNameFontKey = FontsManager.DefaultSmallFontKey,
                BarNameFontId = FontsManager.GetFontIndex(FontsManager.DefaultSmallFontKey),

                BarDataFontKey = FontsManager.DefaultSmallFontKey,
                BarDataFontId = FontsManager.GetFontIndex(FontsManager.DefaultSmallFontKey),

                RankTextFontKey = FontsManager.DefaultSmallFontKey,
                RankTextFontId = FontsManager.GetFontIndex(FontsManager.DefaultSmallFontKey),
            };

            return defaultConfig;
        }

        public void DrawConfig(Vector2 size, float padX, float padY, bool border = true)
        {
            if (ImGui.BeginChild($"##{this.Name}", new Vector2(size.X, size.Y), border))
            {
                ImGui.Text("伤害数据类型");
                ImGui.RadioButton("固定条数", ref this.BarHeightType, 0);
                ImGui.SameLine();
                ImGui.RadioButton("固定高度", ref this.BarHeightType, 1);

                if (this.BarHeightType == 0)
                {
                    ImGui.DragInt("显示条数", ref this.BarCount, 1, 1, 48);
                }
                else if (this.BarHeightType == 1)
                {
                    ImGui.DragFloat("条形高度", ref this.BarHeight, .1f, 1, 100);
                }

                ImGui.DragInt("条形间距", ref this.BarGaps, 1, 0, 20);

                ImGui.NewLine();
                ImGui.DragFloat("填充高度（占条形高度百分比）", ref this.BarFillHeight, .1f, 0, 1f);
                ImGui.Combo("填充方向", ref this.BarFillDirection, ["向上", "向下"], 2);
                DrawHelpers.DrawColorSelector("数据条背景色", this.BarBackgroundColor);

                ImGui.NewLine();
                ImGui.Checkbox("显示职业图标", ref this.ShowJobIcon);
                if (this.ShowJobIcon)
                {
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.SameLine();
                    ImGui.RadioButton("自动大小", ref this.JobIconSizeType, 0);
                    ImGui.SameLine();
                    ImGui.RadioButton("手动大小", ref this.JobIconSizeType, 1);

                    if (this.JobIconSizeType == 1)
                    {
                        DrawHelpers.DrawNestIndicator(1);
                        ImGui.DragFloat2("大小##JobIconSize", ref this.JobIconSize);
                    }

                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.DragFloat2("职业图标偏移", ref this.JobIconOffset);

                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.Combo(
                        "职业图标样式",
                        ref this.JobIconStyle,
                        _jobIconStyleOptions,
                        _jobIconStyleOptions.Length
                    );
                    DrawHelpers.DrawNestIndicator(1);
                    DrawHelpers.DrawColorSelector("背景颜色##JobIcon", this.JobIconBackgroundColor);
                }

                ImGui.NewLine();
                ImGui.Checkbox("显示表头栏", ref this.ShowColumnHeader);
                if (this.ShowColumnHeader)
                {
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.DragFloat("表头高度", ref this.ColumnHeaderHeight);

                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.DragFloat2("文字偏移", ref this.ColumnHeaderOffset);

                    DrawHelpers.DrawNestIndicator(1);
                    DrawHelpers.DrawColorSelector("表头背景色", this.ColumnHeaderColor);

                    DrawHelpers.DrawNestIndicator(1);
                    DrawHelpers.DrawColorSelector("表头文字颜色", this.ColumnHeaderTextColor);

                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.Checkbox("使用表头字体", ref this.UseColumnFont);
                    if (!this.UseColumnFont)
                    {
                        DrawHelpers.DrawNestIndicator(2);
                        DrawHelpers.DrawFontSelector(
                            "字体##Column",
                            ref this.ColumnHeaderFontKey,
                            ref this.ColumnHeaderFontId
                        );
                    }

                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.Checkbox("显示描边", ref this.ColumnHeaderShowOutline);
                    if (this.ColumnHeaderShowOutline)
                    {
                        DrawHelpers.DrawNestIndicator(2);
                        DrawHelpers.DrawColorSelector("表头描边颜色", this.ColumnHeaderOutlineColor);
                    }
                }

                ImGui.NewLine();
                ImGui.Checkbox("伤害数据使用职业颜色", ref this.UseJobColor);
                if (!this.UseJobColor)
                {
                    DrawHelpers.DrawNestIndicator(1);
                    DrawHelpers.DrawColorSelector("条形颜色", this.BarColor);
                }

                ImGui.Checkbox("个人数据使用自定义颜色", ref this.UseCustomColorForSelf);
                if (this.UseCustomColorForSelf)
                {
                    DrawHelpers.DrawNestIndicator(1);
                    DrawHelpers.DrawColorSelector("颜色", this.CustomColorForSelf);
                }

                ImGui.Checkbox("使用角色名替代 'YOU'", ref this.UseCharacterName);
                if (this.BarHeightType == 0)
                {
                    ImGui.Checkbox("始终显示自己伤害数据", ref this.AlwaysShowSelf);
                }

                ImGui.NewLine();
                DrawHelpers.DrawRoundingOptions("顶部圆角", 0, this.TopBarRounding);
                DrawHelpers.DrawRoundingOptions("中部圆角", 0, this.MiddleBarRounding);
                DrawHelpers.DrawRoundingOptions("底部圆角", 0, this.BottomBarRounding);
            }

            ImGui.EndChild();
        }
    }
}
