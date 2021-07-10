﻿using System.Runtime.InteropServices;

namespace FFXIVClientStructs.FFXIV.Component.GUI.ULD
{

    [StructLayout(LayoutKind.Explicit, Size = 0x94)]
    public unsafe struct ULDComponentDataJournalCanvas
    {
        [FieldOffset(0x00)] public ULDComponentDataBase Base;
        [FieldOffset(0x0C)] public fixed uint Nodes[32];
        [FieldOffset(0x8C)] public ushort ItemMargin;
        [FieldOffset(0x8E)] public ushort BasicMargin;
        [FieldOffset(0x90)] public ushort AnotherMargin;
        [FieldOffset(0x92)] public ushort Padding;
    }
}
