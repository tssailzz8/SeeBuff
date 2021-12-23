using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Interface;
using ImGuiNET;

namespace SeeBuff
{
	static class Extensions
	{
		internal static bool IconButton(FontAwesomeIcon icon, string id, Vector2 size = default)
		{
			ImGui.PushFont(UiBuilder.IconFont);
			bool ret;
			if (size != default)
			{
				ret = ImGui.Button($"{icon.ToIconString()}##{id}", size);
			}
			else
			{
				ret = ImGui.Button($"{icon.ToIconString()}##{id}");
			}
			ImGui.PopFont();
			return ret;
		}

	}
}
