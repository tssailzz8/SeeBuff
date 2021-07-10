﻿using System.Runtime.InteropServices;

namespace FFXIVClientStructs.FFXIV.Component.GUI.ULD
{
    [StructLayout(LayoutKind.Explicit, Size = 0x10)]
    public unsafe struct ULDPartsList
    {
        [FieldOffset(0x0)] public uint Id;
        [FieldOffset(0x4)] public uint PartCount;
        [FieldOffset(0x8)] public ULDPart* Parts; // array with size PartCount
    }
}
