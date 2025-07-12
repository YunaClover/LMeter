using System;
using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;
using LMeter.Act;
using LMeter.Helpers;
using Newtonsoft.Json;

namespace LMeter.Config
{
    public class ActConfig : IConfigPage
    {
        [JsonIgnore]
        private const string _defaultSocketAddress = "ws://127.0.0.1:10501/ws";

        [JsonIgnore]
        private DateTime? LastCombatTime { get; set; }

        [JsonIgnore]
        private DateTime? LastReconnectAttempt { get; set; }

        [JsonIgnore]
        public bool Active { get; set; }

        public string Name => "ACT";
        public string ActSocketAddress;
        public int EncounterHistorySize = 15;
        public bool AutoReconnect = false;
        public int ReconnectDelay = 30;
        public bool ClearAct = false;
        public bool AutoEnd = false;
        public int AutoEndDelay = 3;
        public int ClientType = 0;
        public bool UseFFLogs = false;
        public bool DisableFFLogsOutsideDuty = true;

        public ActConfig()
        {
            this.ActSocketAddress = _defaultSocketAddress;
        }

        public IConfigPage GetDefault() => new ActConfig();

        public void DrawConfig(Vector2 size, float padX, float padY, bool border = true)
        {
            if (ImGui.BeginChild($"##{this.Name}", new Vector2(size.X, size.Y), border))
            {
                int currentClientType = this.ClientType;
                ImGui.Text("ACT客户端类别:");
                ImGui.RadioButton("WebSocket", ref this.ClientType, 0);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("如果您正在使用普通ACT程序，请使用此选项。");
                }

                ImGui.SameLine();
                ImGui.RadioButton("IINACT IPC", ref this.ClientType, 1);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("如果您正在使用 IINACT dalamud 插件，请使用此选项。");
                }

                if (currentClientType != this.ClientType)
                {
                    Singletons.Get<PluginManager>().ChangeClientType(this.ClientType);
                }

                Vector2 buttonSize = new(40, 0);
                ImGui.Text($"ACT 状态: {Singletons.Get<LogClient>().Status}");
                if (this.ClientType == 0)
                {
                    ImGui.InputTextWithHint("ACT Websocket 地址", $"Default: '{_defaultSocketAddress}'", ref this.ActSocketAddress, 64);
                }

                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Sync, () => Singletons.Get<LogClient>().Reset(), "重新连接", buttonSize);

                ImGui.SameLine();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 1f);
                ImGui.Text("刷新ACT连接");

                if (this.UseFFLogs)
                {
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.Checkbox("Disable FFLogs Calculations Outside Duty", ref this.DisableFFLogsOutsideDuty);
                }

                ImGui.NewLine();
                ImGui.PushItemWidth(30);
                ImGui.InputInt("保存战斗记录数", ref this.EncounterHistorySize, 0, 0);
                ImGui.PopItemWidth();

                ImGui.NewLine();
                ImGui.Checkbox("如果连接失败则自动尝试重新连接", ref this.AutoReconnect);
                if (this.AutoReconnect)
                {
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.PushItemWidth(30);
                    ImGui.InputInt("秒后重新尝试连接", ref this.ReconnectDelay, 0, 0);
                    ImGui.PopItemWidth();
                }


                ImGui.NewLine();
                ImGui.Checkbox("清理LMeter时也同时清理ACT数据", ref this.ClearAct);
                ImGui.Checkbox("强制 ACT 在战斗后结束战斗", ref this.AutoEnd);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("如果您使用此功能，建议禁用 ACT 命令提示音。\n" +
                                     "该选项可以在 ACT 的“选项”->“声音设置”下找到。");
                }

                if (this.AutoEnd)
                {
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.PushItemWidth(30);
                    ImGui.InputInt("Seconds delay after combat", ref this.AutoEndDelay, 0, 0);
                    ImGui.PopItemWidth();
                }

                ImGui.NewLine();
                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Stop, () => Singletons.Get<LogClient>().EndEncounter(), null, buttonSize);
                ImGui.SameLine();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 1f);
                ImGui.Text("强制结束战斗记录");

                DrawHelpers.DrawButton(string.Empty, FontAwesomeIcon.Trash, () => Singletons.Get<PluginManager>().Clear(), null, buttonSize);
                ImGui.SameLine();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 1f);
                ImGui.Text("清除LMeter数据");
            }

            ImGui.EndChild();
        }

        public void TryReconnect()
        {
            ConnectionStatus status = Singletons.Get<LogClient>().Status;
            if (this.LastReconnectAttempt.HasValue &&
                (status == ConnectionStatus.NotConnected || status == ConnectionStatus.ConnectionFailed))
            {
                if (this.AutoReconnect &&
                    this.LastReconnectAttempt < DateTime.UtcNow - TimeSpan.FromSeconds(this.ReconnectDelay))
                {
                    Singletons.Get<LogClient>().Reset();
                    this.LastReconnectAttempt = DateTime.UtcNow;
                }
            }
            else
            {
                this.LastReconnectAttempt = DateTime.UtcNow;
            }
        }

        public void TryEndEncounter()
        {
            if (Singletons.Get<LogClient>().Status == ConnectionStatus.Connected)
            {
                if (this.AutoEnd && CharacterState.IsInCombat())
                {
                    this.LastCombatTime = DateTime.UtcNow;
                }
                else if (this.LastCombatTime is not null &&
                         this.LastCombatTime < DateTime.UtcNow - TimeSpan.FromSeconds(this.AutoEndDelay))
                {
                    Singletons.Get<LogClient>().EndEncounter();
                    this.LastCombatTime = null;
                }
            }
        }
    }
}
