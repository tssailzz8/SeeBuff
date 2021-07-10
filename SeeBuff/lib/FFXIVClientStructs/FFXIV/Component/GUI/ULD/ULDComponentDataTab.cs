﻿using System.Runtime.InteropServices;

namespace FFXIVClientStructs.FFXIV.Component.GUI.ULD
{

    [StructLayout(LayoutKind.Explicit, Size = 0x24)]
    public unsafe struct ULDComponentDataTab
    {
        [FieldOffset(0x00)] public ULDComponentDataBase Base;
        [FieldOffset(0x0C)] public fixed uint Nodes[4];
        [FieldOffset(0x1C)] public uint TextId;
        [FieldOffset(0x20)] public uint GroupId;
    }
}
