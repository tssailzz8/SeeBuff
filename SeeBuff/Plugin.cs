using Dalamud.Configuration;
using Dalamud.Game.Command;
using Dalamud.Interface;
using Dalamud.Plugin;
using ImGuiNET;
using Num = System.Numerics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ImGuiScene;
using Dalamud.Game.ClientState.Actors.Types;
using System.Diagnostics;
using System.Net;
using Dalamud.Game.ClientState;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using QoLBar;

namespace SeeBuff
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "SeeBuff";


        public static DalamudPluginInterface pi;
        private Configuration configuration;
        private DalamudPluginInterface pluginInterface;

        private delegate bool TestDeto(IntPtr address);

        string commandName = "/SeeBuff";

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            pi = pluginInterface;

            TextureDictionary = new TextureDictionary(false, false);
            for (int i = 10000; i < 19999; i++)
            {
                TextureDictionary.LoadTexture(i);
            }

            PluginLog.Warning($"{TextureDictionary.Count.ToString()}");

            configuration = ((pi.GetPluginConfig() as Configuration) ?? new Configuration());
            this.configuration.Initialize(pi);
            pi.CommandManager.AddHandler(this.commandName, new CommandInfo(new CommandInfo.HandlerDelegate(OnCommand))
            {
                HelpMessage = "显示自己上的buff"
            });
            configuration.ConfigUiVisible = false;
            pi.UiBuilder.OnBuildUi += new RawDX11Scene.BuildUIDelegate(this.BuildUI);
            pi.UiBuilder.OnOpenConfigUi += UiBuilder_OnOpenConfigUi;

            var address = new PluginAddressResolver();
            address.Setup(pi.TargetModuleScanner);
            XivApi.Initialize(pi, address);
        }

        private void UiBuilder_OnOpenConfigUi(object sender, EventArgs e)
        {
            configuration.ConfigUiVisible = true;
        }

        private void OnCommand(string command, string args)
        {
            configuration.ConfigUiVisible = true;
        }

        private unsafe void BuildUI()
        {
            var actorTable = pi.ClientState.Actors;
            if (configuration.ConfigUiVisible)
            {
                bool configVisible = this.configuration.ConfigUiVisible;
                if (configVisible != this.configuration.ConfigUiVisible)
                {
                    this.configuration.ConfigUiVisible = configVisible;
                    this.configuration.Save();
                }

                if (ImGui.Begin("SeeBuff", ref configuration.ConfigUiVisible, ImGuiWindowFlags.AlwaysAutoResize))
                {
                    if (ImGui.Checkbox("开启", ref configuration.Visible))
                    {
                    }
                    //if (ImGui.SliderFloat("透明度", ref this.configuration.透明, 0, 1))
                    //{
                    //	this.configuration.透明度 = (uint)(0xfefffff0 * this.configuration.透明);

                    //}

                    if (ImGui.SliderFloat("透明度", ref this.configuration.透明, 0, 1))
                    {
                    }

                    if (ImGui.SliderFloat("Z调整(与摄像机远近有关)", ref this.configuration.z位置, -20, 20))
                    {
                    }

                    if (ImGui.InputInt("X调整", ref this.configuration.x位置))
                    {
                    }

                    if (ImGui.Checkbox("显示自己buff", ref configuration.自己))
                    {
                    }

                    if (ImGui.Checkbox("仅在战斗中", ref configuration.战斗))
                    {
                    }

                    configuration.Save();
                    //PluginLog.Log(this.configuration.透明度.ToString("X"));
                    ImGui.End();
                }
            }

            if (!configuration.Visible || actorTable == null)
            {
                return;
            }

            try
            {
                if (pi.ClientState.LocalPlayer == null) return;
                var array = new Dictionary<int, IntPtr>();
                var addon = XivApi.GetSafeAddonNamePlate();
                for (int i = 0; i < 50; i++)
                {
                    unsafe
                    {
                        var npObject = addon.GetNamePlateObject(i);
                        if (npObject == null || *(byte*) (npObject.Pointer + 0x60) != 0)
                            continue;

                        var npInfo = npObject.NamePlateInfo;
                        if (npInfo == null)
                            continue;

                        var actorID = npInfo.Data.ActorID;
                        if (actorID == -1)
                            continue;

                        //if (npInfo.Name != "") PluginLog.Error(i+" "+npInfo.Name+npObject.Pointer.ToString("X"));

                        if (*(byte*) (npObject.Pointer + 0x5C) != 0) continue;

                        array.Add(actorID, (IntPtr) (*(long*) npObject.Pointer));
                    }
                }


                var statusEnumerable = pi.Data.GetExcelSheet<Status>();
                var localPlayerActorId = pi.ClientState.LocalPlayer.ActorId;
                var hasmyeffect = pi.ClientState.Actors
                    .Where(i => i.StatusEffects.Any(j => j.OwnerId == localPlayerActorId))
                    .Where(i => i.ObjectKind == Dalamud.Game.ClientState.Actors.ObjectKind.Player);
                var bdl = ImGui.GetBackgroundDrawList(ImGui.GetMainViewport());
                var b = (uint) ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, this.configuration.透明));
                //PluginLog.Log("b是{0}",b.ToString("x"));

                foreach (var actor in hasmyeffect)
                {
                    if (!array.TryGetValue(actor.ActorId, out var ptr)) continue;
                    var node = (AtkComponentNode*) ptr;

                    var pos = new Vector2(node->AtkResNode.X, node->AtkResNode.Y);


                    if (!pi.ClientState.Condition[ConditionFlag.InCombat] && this.configuration.战斗)
                    {
                        continue;
                    }

                    if (!this.configuration.自己 && pi.ClientState.Actors[0].ActorId == localPlayerActorId)
                    {
                        continue;
                    }

                    //var b = pluginInterface.Framework.Gui.WorldToScreen(new SharpDX.Vector3(actor.Position.X, actor.Position.Z - 10, actor.Position.Y), out SharpDX.Vector2 pos);
                    var screenPosO = new Vector2(pos.X - this.configuration.x位置, pos.Y);

                    var effects = actor.StatusEffects.Where(i => i.OwnerId == localPlayerActorId)
                        .Select((effect, i) => (effect.EffectId, i));
                    foreach (var effect in effects)
                    {
                        var status = statusEnumerable.GetRow((uint) effect.EffectId);
                        var textureWrap = TextureDictionary[status.Icon];
                        var texsize = new Vector2(textureWrap.Width, textureWrap.Height);
                        bdl.AddImage(textureWrap.ImGuiHandle,
                            screenPosO + new Vector2(25 * effect.i, 0),
                            screenPosO + new Vector2(25 * effect.i, 0) + texsize, Vector2.Zero, Vector2.One, b);
                    }
                }

                array.Clear();
            }

            catch (Exception e)
            {
                PluginLog.Error(e.ToString());
            }
        }


        private void DoIt(string command, string arguments)
        {
            if (command == "/SeeBuff")
            {
                configuration.ConfigUiVisible = !configuration.ConfigUiVisible;
            }
        }


        public void Dispose()
        {
            pi.UiBuilder.OnBuildUi -= new RawDX11Scene.BuildUIDelegate(this.BuildUI);
            pi.UiBuilder.OnOpenConfigUi -= UiBuilder_OnOpenConfigUi;
            pi.CommandManager.RemoveHandler(commandName);
            TextureDictionary.Dispose();
        }

        //public TextureDictionary TextureDictionary
        //{
        //	get
        //	{
        //		if (UseHRIcons == true)
        //		{
        //			return textureDictionaryHR;
        //		}
        //		else
        //		{
        //			return textureDictionaryLR;
        //		}
        //	}
        //}
        //public static TextureDictionary TextureDictionary => textureDictionaryLR;
        //public static readonly TextureDictionary textureDictionaryHR = new TextureDictionary(true, false);
        public TextureDictionary TextureDictionary;
        public bool UseHRIcons = false;
        public static Vector2 iconFrameUV0 = new Vector2(1f / 426f, 141f / 426f);
        public static Vector2 iconFrameUV1 = new Vector2(47f / 426f, 187f / 426f);
    }

    [Serializable]
    class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool Visible = false;
        public bool Debug = false;
        public bool ConfigUiVisible;
        public float z位置;
        public float 透明;

        [NonSerialized] public DalamudPluginInterface pluginInterface;
        internal int x位置;
        internal bool 战斗;
        internal bool 自己;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            pluginInterface?.SavePluginConfig(this);
        }
    }
}