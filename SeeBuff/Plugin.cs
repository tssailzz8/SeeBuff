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
using System.Diagnostics;
using System.Net;
using Dalamud.Game.ClientState;
using Lumina.Excel.GeneratedSheets;
using QoLBar;
using seebuff;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Logging;

namespace SeeBuff
{
	public class Plugin : IDalamudPlugin
	{
		public string Name => "SeeBuff";


		public static DalamudPluginInterface pi;
		private Configuration configuration;
		public int count;
		public int count1;
		public enum StatusFlags : byte
		{
			None = 0,
			Hostile = 1 << 0,
			InCombat = 1 << 1,
			WeaponOut = 1 << 2,
			PartyMember = 1 << 4,
			AllianceMember = 1 << 5,
			Friend = 1 << 6,
			Casting = 1 << 7
		}

		private delegate bool TestDeto(IntPtr address);
		public readonly HashSet<ushort> 减伤 = new HashSet<ushort>
		{ 1178,//暗黑之夜
          1191,//铁壁
          746,//弃明投暗
          747,//暗影墙
          810,//行尸走肉
		  811,//死而不僵
		  735,//原初的直觉
          409,//死斗
          1209,//亲疏自行
          89,//复仇
          2227,//原初的勇猛
          87,//战栗
          728,
		  1856,//盾阵
          74,//预警
          82,//神圣领域
          1832,//伪装
          1834,//星云
          1840,//石之心
          1836,//超火流星
        };
		string commandName = "/SeeBuff";
		private char[] para = new char[]
{
			' '
};
		public Plugin(DalamudPluginInterface pluginInterface)
		{
			pi = pluginInterface;
			pluginInterface.Create<External>();
			var address = new PluginAddressResolver();
			address.Setup(External.SigScanner);
			XivApi.Initialize(pi, address);

			TextureDictionary = new TextureDictionary(true	, false);
			for (int i = 10000; i < 19999; i++)
			{
				TextureDictionary.LoadTexture(i);
			}
			buff1 = new BuffList(0, 0);
			buff1.zidian.Clear();

			PluginLog.Warning($"{TextureDictionary.Count.ToString()}");

			configuration = ((pi.GetPluginConfig() as Configuration) ?? new Configuration());
			this.configuration.Initialize(pi);
			External.Commands.AddHandler(this.commandName, new CommandInfo(new CommandInfo.HandlerDelegate(OnCommand))
			{
				HelpMessage = "显示自己上的buff"
			});
			configuration.ConfigUiVisible = false;
			pi.UiBuilder.Draw += BuildUI;
			pi.UiBuilder.OpenConfigUi += UiBuilder_OnOpenConfigUi;

		}

		private void UiBuilder_OnOpenConfigUi()
		{
			configuration.ConfigUiVisible = true;
		}

		private void OnCommand(string command, string args)
		{
			计数++;
			if (command == "/SeeBuff")
			{
				string[] array = args.Split(this.para);
				var a = array[0];
                switch (a)
                {
					case "ID":
					string[] array1 = array[1].Split(new char[]
				{
					','
				});
						ushort x;
						float y;
						ushort.TryParse(array1[0], out x);
						float.TryParse(array1[1], out y);
						buff1 = new BuffList(x, y);
						break;
					default:
						configuration.ConfigUiVisible = !configuration.ConfigUiVisible;
						break;
                }
    //            if (array[0] == "ID")
				//{
				//	string[] array1 = array[1].Split(new char[]
				//{
				//	','
				//});
				//	ushort x;
				//	float y;
				//	ushort.TryParse(array1[0], out x);
				//	float.TryParse(array1[1], out y);
				//	buff1 = new BuffList(x, y);
				//}
			}
		}

		private unsafe void BuildUI()
		{
			var actorTable = External.Objects;
			if (configuration.ConfigUiVisible)
			{

				if (ImGui.Begin("SeeBuff", ref configuration.ConfigUiVisible, ImGuiWindowFlags.AlwaysAutoResize))
				{
					if (ImGui.Checkbox("开启", ref configuration.Visible))
					{
						configuration.Save();
					}

					if (ImGui.SliderFloat("透明度", ref this.configuration.透明, 0, 1))
					{
						configuration.Save();
					}

					if (ImGui.SliderFloat("Y调整", ref this.configuration.y位置,250, -250))
					{
						configuration.Save();
					}
					if (ImGui.SliderFloat("图标大小", ref this.configuration.图标大小, 0, 3))
					{
						configuration.Save();
					}

					if (ImGui.InputInt("X调整", ref this.configuration.x位置))
					{
						configuration.Save();
					}
					if (ImGui.SliderFloat("背景图片X", ref this.configuration.背图片x, -100, 100))
					{
						configuration.Save();
					}

					if (ImGui.SliderFloat("背景图片Y", ref this.configuration.背图片y, -100, 100))
					{
						configuration.Save();
					}
					if (ImGui.Checkbox("显示自己buff", ref configuration.自己))
					{
						configuration.Save();
					}

					if (ImGui.Checkbox("仅在战斗中", ref configuration.战斗))
					{
						configuration.Save();
					}
					if (ImGui.Checkbox("显示t减伤", ref configuration.减伤))
					{
						configuration.Save();
					}
					if (ImGui.Checkbox("附加一个透明背景在图标位置", ref configuration.背景))
					{
						configuration.Save();
					}
                    if (ImGui.Checkbox("只显示小队成员的", ref configuration.party))
                    {
						configuration.Save();
					};
                    if (ImGui.Checkbox("显示具体忍术", ref configuration.renshu))
                    {
						configuration.Save();
					}
                    if (ImGui.Checkbox("添加字体描边", ref configuration.描边))
                    {
						configuration.Save();
                    }
                    if (ImGui.ColorEdit4("颜色", ref this.configuration.Value1, ImGuiColorEditFlags.NoInputs))
                    {
						configuration.Save();
					}
                    if (ImGui.ColorEdit4("描边字体颜色", ref this.configuration.背景字体, ImGuiColorEditFlags.NoInputs))
                    {
						configuration.Save();
					}
					

					

					var statusEnumerable = External.Data.GetExcelSheet<Status>();
					if (ImGui.BeginTable("buffids", 4, ImGuiTableFlags.BordersH|ImGuiTableFlags.SizingFixedSame))
					{
						ImGui.TableSetupColumn("ID");	
						ImGui.TableSetupColumn("图标");
						ImGui.TableSetupColumn("名称");
						ImGui.TableSetupColumn("删除");

						foreach (var buffid in configuration.userBuffIds)
						{
							ImGui.TableNextRow();
							var buff = statusEnumerable.GetRow(buffid);

							if (buff is null)
							{
								ImGui.TableNextColumn(); ImGui.TextUnformatted(buffid.ToString());
								ImGui.TableNextColumn();
								ImGui.TableNextColumn(); ImGui.TextUnformatted("未找到");
							}
							else
							{
								ImGui.TableNextColumn(); ImGui.TextUnformatted(buffid.ToString());
								var textureWrap = TextureDictionary[buff.Icon];
								ImGui.TableNextColumn();
								try
								{
									ImGui.Image(textureWrap.ImGuiHandle, new Vector2(25, 25), new Vector2(0.1f,0.1f),new Vector2(0.9f,0.9f));
								}
								catch (Exception e)
								{
									//
								}
								ImGui.TableNextColumn(); ImGui.TextUnformatted(buff.Name);
							}
							ImGui.TableNextColumn();
							if (Extensions.IconButton(FontAwesomeIcon.Trash, $"{buffid} delete"))
							{
								configuration.userBuffIds.Remove(buffid);
								break;
							}
						}

						ImGui.EndTable();
					}

					
					if (ImGui.InputText("new buff id", ref configuration.newuserBuffId, 255, ImGuiInputTextFlags.EnterReturnsTrue))
					{
						AddUserBuff();
					}
					ImGui.SameLine();
					if (Extensions.IconButton(FontAwesomeIcon.Plus, "add new buff button"))
					{
						AddUserBuff();
					}
					void AddUserBuff()
					{
						if (uint.TryParse(configuration.newuserBuffId, out var newbuffid))
						{
							try
							{
								configuration.userBuffIds.Add(newbuffid);
							}
							catch (Exception e)
							{
								//
							}
						}
					}

					ImGui.End();
				}
			}

			if (!configuration.Visible || actorTable == null)
			{
				return;
			}

			try
			{
				if (External.ClientState.LocalPlayer == null) return;
				var array = new Dictionary<int, XivApi.SafeNamePlateObject>();
				var addon = XivApi.GetSafeAddonNamePlate();
				for (int i = 0; i < 50; i++)
				{
					unsafe
					{
						var npObject = addon.GetNamePlateObject(i);
						if (npObject == null || !npObject.IsVisible)
							continue;

						var npInfo = npObject.NamePlateInfo;
						if (npInfo == null)
							continue;

						var actorID = npInfo.Data.ActorID;
						if (actorID == -1)
							continue;

						
						if (npObject.Data.NameplateKind != 3 && npObject.Data.NameplateKind != 0) continue;

						array.Add(actorID, npObject);
					}

				}
				foreach (var item in buff1.zidian)
				{
					if (item.Value.ToString() == DateTime.Now.ToString())
					{
						buff1.zidian.Remove(item.Key);
						break;
					}
				}
				var statusEnumerable = External.Data.GetExcelSheet<Status>();
				var getActionID = External.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>();
				var localPlayerActorId = External.ClientState.LocalPlayer.ObjectId;
				var localPlayerPet = External.Objects.FirstOrDefault(i => i is BattleNpc bnpc && bnpc.OwnerId == localPlayerActorId && bnpc.BattleNpcKind == BattleNpcSubKind.Pet);
				var hasmyeffect = External.Objects.Where(i => i.ObjectKind == ObjectKind.Player|| i.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.BattleNpc);
				var bdl = ImGui.GetBackgroundDrawList(ImGui.GetMainViewport());
				var b = (uint)ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, this.configuration.透明));
				//PluginLog.Log("b是{0}",b.ToString("x"));
				this.configuration.Value = new Vector4(this.configuration.Value1.X, this.configuration.Value1.Y, this.configuration.Value1.Z, this.configuration.透明);
				var 描边 = new Vector4(this.configuration.背景字体.X, this.configuration.背景字体.Y, this.configuration.背景字体.Z, 175);
				foreach (var actor in hasmyeffect)
				{
					if (External.Conditions[ConditionFlag.BetweenAreas])
					{
						continue;
					}

					if (!array.TryGetValue((int)actor.ObjectId, out var namePlateObject)) continue;
					var node = namePlateObject.Data.RootNode;

					var pos = new Vector2(node->AtkResNode.X, node->AtkResNode.Y);
					var level = External.ClientState.LocalPlayer.Level;

					if (!External.Conditions[ConditionFlag.InCombat] && this.configuration.战斗)
					{
						continue;
					}
					if (!this.configuration.自己 && actor.ObjectId == localPlayerActorId)
					{
						continue;
					}
					var StatusFlag = Marshal.ReadByte(actor.Address + 6560);
					var screenPosOriginal = new Vector2(pos.X - this.configuration.x位置, pos.Y - this.configuration.y位置);
					var new_actor = (BattleChara)actor;
					var effects = new_actor.StatusList
						.Where(i => (i.SourceID == localPlayerActorId || i.SourceID == localPlayerPet?.ObjectId || (configuration.减伤 && 减伤.Contains((ushort)i.StatusId))|| this.configuration.userBuffIds.Contains((uint)i.StatusId) || (i.StatusId == 496 && this.configuration.renshu) || buff1.zidian.ContainsKey((int)i.StatusId)))
						.Where(i => i.RemainingTime > 0 && i.RemainingTime < 40)
						.Select((effect, i) => (effect, i));
					count = effects.Count();
					screenPosOriginal += new Vector2((float)node->AtkResNode.Width / 2 - 6.25f - 12.5f * count, 0);
					if (this.configuration.背景 && count > 0)
					{

						bdl.AddRectFilled(new Vector2(screenPosOriginal.X - this.configuration.背图片x, screenPosOriginal.Y - 2 - this.configuration.背图片y), new Vector2(screenPosOriginal.X + 7 + 25 * count - this.configuration.背图片x, screenPosOriginal.Y + 45 - this.configuration.背图片y), 0x80000000, 3);
					}
					ImGui.PushFont(UiBuilder.DefaultFont);
					foreach (var effect in effects)
					{
						if (((!((StatusFlag & (byte)StatusFlags.PartyMember) > 0||actor.ObjectKind==ObjectKind.BattleNpc)) && configuration.party) && actor.ObjectId != localPlayerActorId)
						{
							continue;
						}
						if (effect.effect.StatusId == 497)
						{
							this.configuration.生杀 = true;
						}
						var status = effect.effect.GameData.Icon;
						var textureWrap = TextureDictionary[status];
						if (textureWrap == null) continue;
						//图片大小
						var texsize = new Vector2(textureWrap.Width, textureWrap.Height)*this.configuration.图标大小;
						var imagePos = screenPosOriginal + new Vector2(25 * effect.i, 0);
						var font = UiBuilder.DefaultFont;
						var screenPos1 = screenPosOriginal + new Vector2(texsize.X / 4, texsize.Y - 10)+ new Vector2(this.configuration.图标大小*45 * effect.i, 0);
						var screenPos2 = screenPosOriginal + new Vector2(texsize.X * 2 / 5 - 1, texsize.Y-10) + new Vector2(this.configuration.图标大小*45 * effect.i, 0);
						if (effect.effect.StatusId == 496 && this.configuration.renshu)
						{
							var 忍术种类 = effect.effect.StackCount;
							switch (忍术种类)
							{
								case 1:
								case 2:
								case 3:
									status_renshu = getActionID.GetRow(2265);//手里剑
									break;
								case 6:
								case 7:
									if (this.configuration.生杀)
										status_renshu = getActionID.GetRow(16491);
									else
										status_renshu = getActionID.GetRow(2266);//火遁
									break;
								case 9:
								case 11:
									status_renshu = getActionID.GetRow(2267);//雷遁
									break;
								case 13:
								case 14:
									if (this.configuration.生杀)
										status_renshu = getActionID.GetRow(16492);
									else
										status_renshu = getActionID.GetRow(2268);//冰遁
									break;
								case 27:
								case 30:
									status_renshu = getActionID.GetRow(2269);//风遁
									break;
								case 39:
								case 45:
									status_renshu = getActionID.GetRow(2270);//土遁
									break;
								case 54:
								case 57:
									status_renshu = getActionID.GetRow(2271);//水遁
									break;
								default:
									status_renshu = getActionID.GetRow(2272);
									break;
							}
							textureWrap_renshu = TextureDictionary[status_renshu.Icon];
							var texsize_renshu = new Vector2(45,45)*this.configuration.图标大小;
							var screenPos_renshu = screenPosOriginal + new Vector2(texsize_renshu.X / 5, 20);
							bdl.AddImage(textureWrap_renshu.ImGuiHandle,
							screenPosOriginal + new Vector2(this.configuration.图标大小*55 * effect.i, 5),
							screenPosOriginal + new Vector2(this.configuration.图标大小*55 * effect.i, 5) + texsize_renshu, Vector2.Zero, Vector2.One, b);
						}
						else
						{
							bdl.AddImage(textureWrap.ImGuiHandle,
								screenPosOriginal + new Vector2(this.configuration.图标大小*45 * effect.i, 0),
								screenPosOriginal + new Vector2(this.configuration.图标大小*45 * effect.i, 0) + texsize, Vector2.Zero, Vector2.One, b);
						}
						ImGui.Begin("##buffdrawlist",
							ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav |
							ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground);
						ImGui.SetWindowFontScale(1.5f);
						//PluginLog.Log(this.configuration.Value.ToString("x"));
						if (effect.effect.RemainingTime >= 9.5f)
						{
							if (this.configuration.描边)
							{
								bdl.AddText(font, 40 * this.configuration.图标大小, screenPos1 + new Vector2( - 1, -1),
(uint)ImGui.ColorConvertFloat4ToU32(描边),
effect.effect.RemainingTime.ToString("f0"));
								bdl.AddText(font, 40 * this.configuration.图标大小, screenPos1 + new Vector2( - 1, 1),
							(uint)ImGui.ColorConvertFloat4ToU32(描边),
							effect.effect.RemainingTime.ToString("f0"));
								bdl.AddText(font, 40 * this.configuration.图标大小, screenPos1 + new Vector2( 1, -1),
							(uint)ImGui.ColorConvertFloat4ToU32(描边),
							effect.effect.RemainingTime.ToString("f0"));
								bdl.AddText(font, 40 * this.configuration.图标大小, screenPos1 + new Vector2( 1, 1),
							(uint)ImGui.ColorConvertFloat4ToU32(描边),
							effect.effect.RemainingTime.ToString("f0"));
							}

							bdl.AddText(font,40 * this.configuration.图标大小, screenPos1,
						(uint)ImGui.ColorConvertFloat4ToU32(configuration.Value),
						effect.effect.RemainingTime.ToString("f0"));
						}
						else
						{
							if (this.configuration.描边)
							{
								bdl.AddText(font, 40 * this.configuration.图标大小, screenPos2 + new Vector2( - 1, -1),
						(uint)ImGui.ColorConvertFloat4ToU32(描边),
						effect.effect.RemainingTime.ToString("f0"));
								bdl.AddText(font, 40 * this.configuration.图标大小, screenPos2 + new Vector2( - 1, 1),
							(uint)ImGui.ColorConvertFloat4ToU32(描边),
							effect.effect.RemainingTime.ToString("f0"));
								bdl.AddText(font, 40 * this.configuration.图标大小, screenPos2 + new Vector2( + 1, -1),
							(uint)ImGui.ColorConvertFloat4ToU32(描边),
							effect.effect.RemainingTime.ToString("f0"));
								bdl.AddText(font, 40 * this.configuration.图标大小, screenPos2 + new Vector2( + 1, 1),
							(uint)ImGui.ColorConvertFloat4ToU32(描边),
							effect.effect.RemainingTime.ToString("f0"));
							}

							bdl.AddText(font,40 * this.configuration.图标大小, screenPos2 ,
						(uint)ImGui.ColorConvertFloat4ToU32(configuration.Value),
						effect.effect.RemainingTime.ToString("f0"));
						}
						ImGui.End();
						//ImGui.PopStyleColor();


					}

					ImGui.PopFont();
				}

				array.Clear();
				this.configuration.生杀 = false;
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
			pi.UiBuilder.Draw -= BuildUI;
			pi.UiBuilder.OpenConfigUi -= UiBuilder_OnOpenConfigUi;
			External.Commands.RemoveHandler(commandName);
			TextureDictionary.Dispose();
			XivApi.DisposeInstance();
			pi.Dispose();
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
		public Lumina.Excel.GeneratedSheets.Action status_renshu;
		public BuffList buff1;
		public TextureWrap textureWrap_renshu;
        private int 计数;
    }

	[Serializable]
	class Configuration : IPluginConfiguration
	{
		public int Version { get; set; } = 0;
		public bool Visible = false;
		public bool Debug = false;
		public bool ConfigUiVisible;
		public float y位置;
		public float 图标大小=1;
		public float 透明 = 1f;
		public int x位置;
		public bool 战斗;
		public bool 自己;
		public Vector4 背景字体;
		public Vector4 Value;
		public Vector4 Value1;
		public bool 减伤;
		public HashSet<uint> userBuffIds = new HashSet<uint>();
		public string newuserBuffId = "";
		public bool 背景;
		public float 背图片x;
		public float 背图片y;
		public bool party;
		public bool renshu;
		public bool 生杀;
		public bool 描边;
		[NonSerialized] public DalamudPluginInterface pluginInterface;


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