using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
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
        public bool LogConnectionErrors = true;

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
                ImGui.Text("ACT 客户端类型:");
                ImGui.RadioButton("WebSocket", ref this.ClientType, 0);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(
                        "如果你使用的是标准独立版 ACT，请选择此项。"
                    );
                }

                ImGui.SameLine();
                ImGui.RadioButton("IINACT IPC", ref this.ClientType, 1);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("如果你使用的是 IINACT Dalamud 插件，请选择此项。");
                }

                if (currentClientType != this.ClientType)
                {
                    Singletons.Get<PluginManager>().ChangeClientType(this.ClientType);
                }

                Vector2 buttonSize = new(40, 0);
                ImGui.Text($"ACT状态: {Singletons.Get<LogClient>().Status}");
                if (this.ClientType == 0)
                {
                    ImGui.InputTextWithHint(
                        "ACT Websocket地址",
                        $"Default: '{_defaultSocketAddress}'",
                        ref this.ActSocketAddress,
                        64
                    );
                }

                DrawHelpers.DrawButton(
                    string.Empty,
                    FontAwesomeIcon.Sync,
                    () => Singletons.Get<LogClient>().Reset(),
                    "重新连接",
                    buttonSize
                );

                ImGui.SameLine();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 1f);
                ImGui.Text("重新连接 ACT");

                if (this.UseFFLogs)
                {
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.Checkbox("Disable FFLogs Calculations Outside Duty", ref this.DisableFFLogsOutsideDuty);
                }

                ImGui.NewLine();
                ImGui.PushItemWidth(30);
                ImGui.InputInt("保存的战斗记录数量", ref this.EncounterHistorySize, 0, 0);
                ImGui.PopItemWidth();

                ImGui.NewLine();
                ImGui.Checkbox("连接失败时自动重连", ref this.AutoReconnect);
                if (this.AutoReconnect)
                {
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.PushItemWidth(30);
                    ImGui.InputInt("重连间隔秒数", ref this.ReconnectDelay, 0, 0);
                    ImGui.PopItemWidth();
                }

                ImGui.Checkbox("Log connection errors", ref this.LogConnectionErrors);

                ImGui.NewLine();
                ImGui.Checkbox("清除 LMeter 时同时清除 ACT", ref this.ClearAct);
                ImGui.Checkbox("战斗结束后强制 ACT 结束战斗", ref this.AutoEnd);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(
                        "如果使用此功能，建议关闭 ACT 的命令音效。\n"
                            + "可在 ACT 的 Options -> Sound Settings 中设置。"
                    );
                }

                if (this.AutoEnd)
                {
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.PushItemWidth(30);
                    ImGui.InputInt("战斗结束后延迟秒数", ref this.AutoEndDelay, 0, 0);
                    ImGui.PopItemWidth();
                }

                ImGui.NewLine();
                DrawHelpers.DrawButton(
                    string.Empty,
                    FontAwesomeIcon.Stop,
                    () => Singletons.Get<LogClient>().EndEncounter(),
                    null,
                    buttonSize
                );
                ImGui.SameLine();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 1f);
                ImGui.Text("强制结束战斗");

                DrawHelpers.DrawButton(
                    string.Empty,
                    FontAwesomeIcon.Trash,
                    () => Singletons.Get<PluginManager>().Clear(),
                    null,
                    buttonSize
                );
                ImGui.SameLine();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 1f);
                ImGui.Text("清除 LMeter");
            }

            ImGui.EndChild();
        }

        public void TryReconnect()
        {
            ConnectionStatus status = Singletons.Get<LogClient>().Status;
            if (
                this.LastReconnectAttempt.HasValue
                && (status == ConnectionStatus.NotConnected || status == ConnectionStatus.ConnectionFailed)
            )
            {
                if (
                    this.AutoReconnect
                    && this.LastReconnectAttempt < DateTime.UtcNow - TimeSpan.FromSeconds(this.ReconnectDelay)
                )
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
                else if (
                    this.LastCombatTime is not null
                    && this.LastCombatTime < DateTime.UtcNow - TimeSpan.FromSeconds(this.AutoEndDelay)
                )
                {
                    Singletons.Get<LogClient>().EndEncounter();
                    this.LastCombatTime = null;
                }
            }
        }
    }
}
