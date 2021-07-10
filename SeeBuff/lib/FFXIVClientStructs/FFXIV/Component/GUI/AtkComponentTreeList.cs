﻿using System.Runtime.InteropServices;

namespace FFXIVClientStructs.FFXIV.Component.GUI
{
    // Component::GUI::AtkComponentTreeList
    //   Component::GUI::AtkComponentBase
    //     Component::GUI::AtkEventListener

    // size = 0x220
    // common CreateAtkComponent function 8B FA 33 DB E8 ? ? ? ? 
    // type ?
    [StructLayout(LayoutKind.Explicit, Size = 0x220)]
    public unsafe struct AtkComponentTreeList
    {
        [FieldOffset(0x0)] public AtkComponentList AtkComponentList;
    }
}
