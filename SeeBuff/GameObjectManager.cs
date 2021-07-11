using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Dalamud;
using Dalamud.Game;
using Dalamud.Game.ClientState.Actors;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.Internal;
using Dalamud.Plugin;

namespace SeeBuff
{
	[StructLayout(LayoutKind.Explicit, Size = 0x27E0)]
	public unsafe struct GameObjectManager
	{
		public const int ListLength = 424;
		[FieldOffset(0x5)] public byte Active;
		[FieldOffset(0x10)] public GameObject* ObjectListStart; // size 424 * 8
		[FieldOffset(0xD50)] public GameObject* ObjectListFilteredStart;
		[FieldOffset(0x1A90)] public GameObject* ObjectList3Start;
		[FieldOffset(0x27D0)] public int ObjectListFilteredCount;
		[FieldOffset(0x27D4)] public int ObjectList3Count;

		// unsafe GameObject** List => (GameObject**)&ObjectListStart;
	}

	public static class GameObjectExtension
	{

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe GameObject* ToGameObject(this IntPtr i) => (GameObject*)i;
		public static unsafe T[] ReadArray<T>(IntPtr unmanagedArray, int length) where T : unmanaged
		{
			var managedArray = new T[length];

			for (int i = 0; i < length; i++)
			{
				// ReSharper disable once PossibleNullReferenceException
				managedArray[i] = ((T*)unmanagedArray)[i];
			}

			return managedArray;
		}
	}

	unsafe class GameObjects
	{
		class GameObjectAddressReslover : BaseAddressResolver
		{
			internal IntPtr GameObjectManagerPtr;
			protected override void Setup64Bit(SigScanner scanner)
			{
				GameObjectManagerPtr = scanner.GetStaticAddressFromSig("48 8D 35 ?? ?? ?? ?? 81 FA ?? ?? ?? ??");
				base.Setup64Bit(scanner);
			}
		}

		private GameObjectAddressReslover address = new GameObjectAddressReslover();
		public GameObjectManager* GameObjectManagerStruct;
		public IntPtr[] GameObjectList => GameObjectExtension.ReadArray<IntPtr>((IntPtr)GameObjectManagerStruct->ObjectListFilteredStart, GameObjectManagerStruct->ObjectListFilteredCount);
		private static GameObjects _instance;
		public static GameObjects GetInstance => _instance ??= new GameObjects();

		private GameObjects()
		{
			address.Setup(Plugin.pi.TargetModuleScanner);
			GameObjectManagerStruct = (GameObjectManager*)address.GameObjectManagerPtr;
		}



	}

	[StructLayout(LayoutKind.Explicit, Pack = 2)]
	public unsafe struct GameObject
	{
		public struct GameObjectVtbl
		{
			public delegate*<void*, void> Vf0;
			public delegate*<void*, void> Vf1;
			public delegate*<void*, uint> GetObjectId;
		}
		/// <summary>
		/// The actor's internal id.
		/// </summary>
		[FieldOffset(0)] public GameObjectVtbl* Vtbl;

		/// <summary>
		/// The actor name.
		/// </summary>
		[FieldOffset(ActorOffsets.Name)]
		private fixed byte _name[0x40];

		public string Name
		{
			get
			{
				fixed (byte* n = _name)
				{
					return ReadTerminatedString(n);
				}
			}
		}

		private static unsafe byte[] ReadTerminatedBytes(byte* ptr)
		{
			if (ptr == null)
			{
				return new byte[0];
			}

			var bytes = new List<byte>();
			while (*ptr != 0)
			{
				bytes.Add(*ptr);
				ptr += 1;
			}

			return bytes.ToArray();
		}

		internal static unsafe string ReadTerminatedString(byte* ptr)
		{
			return Encoding.UTF8.GetString(ReadTerminatedBytes(ptr));
		}


		/// <summary>
		/// The actor's internal id.
		/// </summary>
		[FieldOffset(ActorOffsets.ActorId)] private uint ID1;
		[FieldOffset(ActorOffsets.ActorId + 4)] private uint ID2;
		[FieldOffset(ActorOffsets.ActorId + 8)] private uint ID3;

		public uint Id
		{
			get
			{
				if (ID1 == 0xE0000000)
				{
					if (ID2 == 0 || ID3 - 200 < 44)
					{
						return ID3;
						//0x200000000 -- type masks
					}
					else
					{
						return ID2;
						//0x100000000 -- Type Mask
					}
				}

				return this.ID1;
			}
		}

		/// <summary>
		/// The actor's data id.
		/// </summary>
		[FieldOffset(ActorOffsets.DataId)] public uint NpcBase;

		/// <summary>
		/// The actor's owner id. This is useful for pets, summons, and the like.
		/// </summary>
		[FieldOffset(ActorOffsets.OwnerId)] public int OwnerId;

		/// <summary>
		/// The type or kind of actor.
		/// </summary>
		[FieldOffset(ActorOffsets.ObjectKind)] public ObjectKind ObjectKind;

		/// <summary>
		/// The sub-type or sub-kind of actor.
		/// </summary>
		[FieldOffset(ActorOffsets.SubKind)] public byte SubKind;

		///// <summary>
		///// Whether the actor is friendly.
		///// </summary>
		//[FieldOffset(ActorOffsets.IsFriendly)] public bool IsFriendly;

		/// <summary>
		/// The horizontal distance in game units from the player.
		/// </summary>
		[FieldOffset(ActorOffsets.YalmDistanceFromPlayerX)]
		public byte YalmDistanceFromPlayerX;

		/// <summary>
		/// The player target status.
		/// </summary>
		/// <remarks>
		/// This is some kind of enum.
		/// </remarks>
		[FieldOffset(ActorOffsets.PlayerTargetStatus)]
		public byte PlayerTargetStatus;

		/// <summary>
		/// The vertical distance in game units from the player.
		/// </summary>
		[FieldOffset(ActorOffsets.YalmDistanceFromPlayerY)]
		public byte YalmDistanceFromPlayerY;

		/// <summary>
		/// The Vector3 location of the actor.
		/// </summary>
		[FieldOffset(ActorOffsets.Position)] public SharpDX.Vector3 Location;
		public Vector2 Location2D => new Vector2(X, Z);

		[FieldOffset(ActorOffsets.Position)] public float X;
		[FieldOffset(ActorOffsets.Position + 4)] public float Y;
		[FieldOffset(ActorOffsets.Position + 8)] public float Z;

		/// <summary>
		/// The rotation of the actor.
		/// </summary>
		/// <remarks>
		/// The rotation is around the vertical axis (yaw), from -pi to pi radians.
		/// </remarks>
		[FieldOffset(ActorOffsets.Rotation)] public float Rotation;

		/// <summary>
		/// The hitbox radius of the actor.
		/// </summary>
		[FieldOffset(ActorOffsets.HitboxRadius)] public float HitboxRadius;

		/// <summary>
		/// The current HP of the actor.
		/// </summary>
		[FieldOffset(ActorOffsets.CurrentHp)] public int CurrentHp;

		/// <summary>
		/// The max HP of the actor.
		/// </summary>
		[FieldOffset(ActorOffsets.MaxHp)] public int MaxHp;

		/// <summary>
		/// The current MP of the actor.
		/// </summary>
		[FieldOffset(ActorOffsets.CurrentMp)] public int CurrentMp;

		/// <summary>
		/// The max MP of the actor.
		/// </summary>
		[FieldOffset(ActorOffsets.MaxMp)] public short MaxMp;

		/// <summary>
		/// The current GP of the actor.
		/// </summary>
		[FieldOffset(ActorOffsets.CurrentGp)] public short CurrentGp;

		/// <summary>
		/// The max GP of the actor.
		/// </summary>
		[FieldOffset(ActorOffsets.MaxGp)] public short MaxGp;

		/// <summary>
		/// The current CP of the actor.
		/// </summary>
		[FieldOffset(ActorOffsets.CurrentCp)] public short CurrentCp;

		/// <summary>
		/// The max CP of the actor.
		/// </summary>
		[FieldOffset(ActorOffsets.MaxCp)] public short MaxCp;

		/// <summary>
		/// The class-job of the actor.
		/// </summary>
		[FieldOffset(ActorOffsets.ClassJob)] public byte ClassJob;

		/// <summary>
		/// The level of the actor.
		/// </summary>
		[FieldOffset(ActorOffsets.Level)] public byte Level;

		[FieldOffset(0x1980)]
		public StatusFlags StatusFlags;

		[FieldOffset(ActorOffsets.UIStatusEffects)]
		public StatusEffect StatusEffectStart;
	}

	[Flags]
	public enum StatusFlags : byte
	{
		None = 0,
		Hostile = 1,
		InCombat = 2,
		WeaponOut = 4,
		SubWeaponOut = 8,
		PartyMember = 16,
		AllianceMember = 32,
		Friend = 64,
		IsCasting = 128,
	}
}
