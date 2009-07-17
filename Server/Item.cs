/***************************************************************************
 *                               CREDITS
 *                         -------------------
 *                         : (C) 2004-2009 Luke Tomasello (AKA Adam Ant)
 *                         :   and the Angel Island Software Team
 *                         :   luke@tomasello.com
 *                         :   Official Documentation:
 *                         :   www.game-master.net, wiki.game-master.net
 *                         :   Official Source Code (SVN Repository):
 *                         :   http://game-master.net:8050/svn/angelisland
 *                         : 
 *                         : (C) May 1, 2002 The RunUO Software Team
 *                         :   info@runuo.com
 *
 *   Give credit where credit is due!
 *   Even though this is 'free software', you are encouraged to give
 *    credit to the individuals that spent many hundreds of hours
 *    developing this software.
 *   Many of the ideas you will find in this Angel Island version of 
 *   Ultima Online are unique and one-of-a-kind in the gaming industry! 
 *
 ***************************************************************************/

/***************************************************************************
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/

/* Server/Item.cs
 * CHANGELOG:
 *	5/12/09, Adam
 *		Remove the Obsolete property 'Newbied' because this property was being enumerated during Dupe operations and resetting the LootType value
 *		to Regular if the LootType didn't happen to be Newbied.
 *	1/7/09, Adam
 *		Recast LootType.Internal as LootType.Special. This is like blessed+Cursed (stealable, unlootable)
 *	1/4/09, Adam
 *		Add a version of IntMapStorage that preserves the proginal location of the item so that it gets restored to the correct place.
 *	12/23/08, Adam
 *		Add missing network and timer synchronization from RunUO 2.0
 *	10/16/08, Adam
 *		Add a new DropRate variable that holds the percent chance that this item drops.
 *		This approach was taken to avoid an external database that mapped items to percentages. 
 *		This system is optimized by NOT serializing this value unless it is something other than 100% (1.0) which is the case for virtually all items in the world. The only time this value is not 1.0 is for the small collection of  template  items that exist on the internal map.
 *	7/23/08, Adam
 *		Add new (non serialized) variable m_LastMap which is used post deletion as input to Map.FixColums()
 *	07/23/08, weaver
 *		Automated IPooledEnumerable optimizations. 1 loops updated.
 *	7/8/08, Adam
 *		Remove Default LogException().
 *		(We now use EventSink.InvokeLogException())
 *	3/18/08, Pix
 *		Fixed PublicOverheadMessage() the one that takes a string.  This was another
 *		packet acquire/release issue.
 *	3/14/08, Adam
 *		Make StackWith() check to make sure dropped.Name == Name.
 *			This prevents 'rares' from getting wiped when they are stacked (like "a christmas pork roast")
 *	2/16/08, Pix
 *		Added DoSpecialContainerUpdateForEquippingCorpses() function.
 *  1/8/08, Adam
 *		add WealthTracker enum (AuditType) and 'has been audited flag'
 *  12/6/07, Adam
 *      Add new SendMessage() function that sends a message to staff.
 *      Useful if setting a property fails for some reason.
 *  6/8/07, Adam
 *      - Add several new Freeze Dry helpers:
 *      public virtual void CancelFreezeTimer()
 *          Cancel any pending freeze timers for this object (See Basehouse.cs)
 *      public virtual bool ScheduleFreeze()
 *          create a FreezeDry timer if this container is not already freeze dried
 *      public virtual bool IsFreezeScheduled
 *          returns true if this container is scheduled for freeze drying
 *      - add a Default LogException() of items that simply prints the exception to the console.
 *  6/1/07, Adam
 *      Add new item.HideAttributes bool for suppressing display attributes
 *  5/5/07, Adam
 *      Change Rehydration to allow the rehydration of items that do not have a reserved serial number
 *          as long as the serial number they want is not currently in use.
 *      NOTE: This still means we have a bug where the reserved serials are getting out of step with the
 *      FD'ed items.
  *	3/13/07, weaver
 *		Added m_RareData property + SaveFlag to control conditional read/write of rare
 *		instancing data (CurIndex, LastIndex, StartIndex), along with appropriate
 *		accessors (visible only to AccessLevel.Owner).
 *  2/7/07, Adam
 *      Add IsFreezeDrying property to determine if the item is currently being freeze dried.
 *      You check this on your OnDelete() over ride to handle cleanup appropriately.
 *  2/5/07, Adam
 *      Cleanup ParentMobile property
 *  2/5/07, Adam
 *      Add new MoveItemToIntStorage, and RetrieveItemFromIntStorage functions
 *	10/18/06, Adam
 *		Remove notion of 'Fixed' non-decaying objects
 *	9/8/06, Adam
 *		Add PlayerQuest flag to mark this item as NO DELETE from the internal map.
 *		See the Heartbeat procedure for PlayerQuestCleanup
 *  08/05/06 Taran Kain
 *		Added a check in Item.Bounce() to make sure the container we're bouncing back to can hold us, otherwise we drop at the holder's feet.
 *		Note: Currently limited to secure containers ONLY. Should probably remove this restriction once we verify there's no scams/exploits here.
 *	7/4/06, Adam
 *		Give ToDelete AccessLevel.GameMaster
 *		Make ToDelete a new serialized flag
 *  06/29/06, Kit
 *		Added new flag IsTemplate, for identifying of items that are a spawner template. Added new accessor
 *		SpawnerTempItem()
 *	5/2/06, weaver
 *		Virtualized MoveToWorld( Point3D location, Map map ).
 *	3/2/06, Adam
 *		Add properties ParentMobile and ParentContainer to aid in locating items in realtime
 *	02/24/06 Taran Kain
 *		Added sanity checks to Rehydrate().
 *		Added function ReassignSerial(). DO NOT CALL THIS FUNCTION. Ask Taran Kain.
 *	2/10/06, Adam
 *		Add a new DeathMoveResult 'KeepInternal' and a new LootType 'Internal'
 *		This new lootType is held by the player in an invisible cache that is never seen
 *		or otherwise accessible to the player.
 *  01/18/06 Taran Kain
 *		Added .log file to .idx/.bin when dumping failed rehydrations, so we know wtf went wrong.
 *	01/05/06 - Pix
 *		Added bounceLocation check so that if the location of the parent or
 *		rootparent is out of range of the mobile, then we drop to mobile's feet instead.
 *	12/22/05 Taran Kain
 *		Made Rehydrate() call it's RootParent's UpdateTotals() function instead of just updating itself
 *	12/19/05 Taran Kain
 *		Removed SendRemovePacket and Update calls from FreezeDry
 *		Added Debug property
 *		Made IsFreezeDried a command property
 *	12/18/05 Taran Kain
 *		Added CanFreezeDry property and CheckFreezeDry() function to OO-ize FD logic handling.
 *  12/17/05 Taran Kain
 *		Extensive modifications and improvements to FreezeDry system
 *		Now freezedries to RAM instead of file - failed items are dumped to Logs/FailedRehydrate
 *		Made Items property always read-only - if you want to add or delete, use AddItem() and RemoveItem()
 *	12/15/05 Taran Kain
 *		Fixed error with WriteLines (of all the goddamn things to crash a server with)
 *		Now tracking items that fail to load and moving the data files to a Failed/ directory
 *		Changed method of item instantiation in Rehydrate()
 *	12/14/05 Taran Kain
 *		Moved FreezeDryEnabled to World
 *		Removed non-error WriteLines from FreezeDry/Rehydrate
 *		Cleaned up handling of containers with null Items collections
 *	12/13/05 Taran Kain
 *		Added FreezeDryEnabled flag, OnSave/OnLoad handlers
 *  12/12/05 TK
 *		Changed IsSecure and IsLockedDown FD checks to ignore if value == true
 *		Changed Thaw() to Rehydrate()
 *  12/12/05 Taran Kain
 *		Added support for FD'ing locked down containers (lockboxes)
 *		Added OnFreeze() and OnRehydrate() hooks, called at end of each method
 *		Changed IsSecure and IsLockedDown to be virtual, so that Container may override them to restart timers - any potential problems?
 *		Added testing properties FreezeDryPerf and RehydratePerf to track performance of methods
 *  12/11/05 Taran Kain
 *		Added FreezeDry() and Rehydrate() methods, and various supporting code to make sure everything's handled correctly.
 *	7/26/05, Adam
 *		Massive AOS cleanout
 *	02/28/05, Adam
 *		reuse the 'PayedInsurance' flag as 'Fixed'  (no new data)
 *		this new 'Fixed' flag keeps things from being deleted when attached 
 *		to spawners that respect the 'Fixed' flag.
 *		Decays() now tests: (Movable && Visible && !Fixed)
 *	02/25/05, Adam
 *		reuse the 'Insured' flag as 'PlayerCrafted' (no new data)
 *		also making the property visible
 *	02/21/05, Adam
 *		Add m_ToDelete property (not serialized)
 */

/***************************************************************************
 *                                  Item.cs
 *                            -------------------
 *   begin                : May 1, 2002
 *   copyright            : (C) The RunUO Software Team
 *   email                : info@runuo.com
 *
 *   $Id: Item.cs,v 1.66 2009/05/12 19:15:33 adam Exp $
 *   $Author: adam $
 *   $Date: 2009/05/12 19:15:33 $
 *
 *
 ***************************************************************************/

/***************************************************************************
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Server.Network;
using Server.Items;
using Server.ContextMenus;
using Server.Targeting;

namespace Server
{
	/// <summary>
	/// Enumeration of item layer values.
	/// </summary>
	public enum Layer : byte
	{
		/// <summary>
		/// Invalid layer.
		/// </summary>
		Invalid		 = 0x00,
		/// <summary>
		/// First valid layer. Equivalent to <c>Layer.OneHanded</c>.
		/// </summary>
		FirstValid   = 0x01,
		/// <summary>
		/// One handed weapon.
		/// </summary>
		OneHanded	 = 0x01,
		/// <summary>
		/// Two handed weapon or shield.
		/// </summary>
		TwoHanded	 = 0x02,
		/// <summary>
		/// Shoes.
		/// </summary>
		Shoes		 = 0x03,
		/// <summary>
		/// Pants.
		/// </summary>
		Pants		 = 0x04,
		/// <summary>
		/// Shirts.
		/// </summary>
		Shirt		 = 0x05,
		/// <summary>
		/// Helmets, hats, and masks.
		/// </summary>
		Helm		 = 0x06,
		/// <summary>
		/// Gloves.
		/// </summary>
		Gloves		 = 0x07,
		/// <summary>
		/// Rings.
		/// </summary>
		Ring		 = 0x08,
		/// <summary>
		/// Talismans.
		/// </summary>
		Talisman	 = 0x09,
		/// <summary>
		/// Gorgets and necklaces.
		/// </summary>
		Neck		 = 0x0A,
		/// <summary>
		/// Hair.
		/// </summary>
		Hair		 = 0x0B,
		/// <summary>
		/// Half aprons.
		/// </summary>
		Waist		 = 0x0C,
		/// <summary>
		/// Torso, inner layer.
		/// </summary>
		InnerTorso	 = 0x0D,
		/// <summary>
		/// Bracelets.
		/// </summary>
		Bracelet	 = 0x0E,
		/// <summary>
		/// Unused.
		/// </summary>
		Unused_xF	 = 0x0F,
		/// <summary>
		/// Beards and mustaches.
		/// </summary>
		FacialHair	 = 0x10,
		/// <summary>
		/// Torso, outer layer.
		/// </summary>
		MiddleTorso	 = 0x11,
		/// <summary>
		/// Earings.
		/// </summary>
		Earrings	 = 0x12,
		/// <summary>
		/// Arms and sleeves.
		/// </summary>
		Arms		 = 0x13,
		/// <summary>
		/// Cloaks.
		/// </summary>
		Cloak		 = 0x14,
		/// <summary>
		/// Backpacks.
		/// </summary>
		Backpack	 = 0x15,
		/// <summary>
		/// Torso, outer layer.
		/// </summary>
		OuterTorso	 = 0x16,
		/// <summary>
		/// Leggings, outer layer.
		/// </summary>
		OuterLegs	 = 0x17,
		/// <summary>
		/// Leggings, inner layer.
		/// </summary>
		InnerLegs	 = 0x18,
		/// <summary>
		/// Last valid non-internal layer. Equivalent to <c>Layer.InnerLegs</c>.
		/// </summary>
		LastUserValid= 0x18,
		/// <summary>
		/// Mount item layer.
		/// </summary>
		Mount		 = 0x19,
		/// <summary>
		/// Vendor 'buy pack' layer.
		/// </summary>
		ShopBuy		 = 0x1A,
		/// <summary>
		/// Vendor 'resale pack' layer.
		/// </summary>
		ShopResale	 = 0x1B,
		/// <summary>
		/// Vendor 'sell pack' layer.
		/// </summary>
		ShopSell	 = 0x1C,
		/// <summary>
		/// Bank box layer.
		/// </summary>
		Bank		 = 0x1D,
		/// <summary>
		/// Last valid layer. Equivalent to <c>Layer.Bank</c>.
		/// </summary>
		LastValid    = 0x1D
	}

	/// <summary>
	/// Internal flags used to signal how the item should be updated and resent to nearby clients.
	/// </summary>
	[Flags]
	public enum ItemDelta
	{
		/// <summary>
		/// Nothing.
		/// </summary>
		None		= 0x00000000,
		/// <summary>
		/// Resend the item.
		/// </summary>
		Update		= 0x00000001,
		/// <summary>
		/// Resend the item only if it is equiped.
		/// </summary>
		EquipOnly	= 0x00000002,
		/// <summary>
		/// Resend the item's properties.
		/// </summary>
		Properties	= 0x00000004
	}

	/// <summary>
	/// Enumeration containing possible ways to handle item ownership on death.
	/// </summary>
	public enum DeathMoveResult
	{
		/// <summary>
		/// The item should be placed onto the corpse.
		/// </summary>
		MoveToCorpse,
		/// <summary>
		/// The item should remain equiped.
		/// </summary>
		RemainEquiped,
		/// <summary>
		/// The item should be placed into the owners backpack.
		/// </summary>
		MoveToBackpack,
	}

	/// <summary>
	/// Enumeration containing all possible light types. These are only applicable to light source items, like lanterns, candles, braziers, etc.
	/// </summary>
	public enum LightType
	{
		/// <summary>
		/// Window shape, arched, ray shining east.
		/// </summary>
		ArchedWindowEast,
		/// <summary>
		/// Medium circular shape.
		/// </summary>
		Circle225,
		/// <summary>
		/// Small circular shape.
		/// </summary>
		Circle150,
		/// <summary>
		/// Door shape, shining south.
		/// </summary>
		DoorSouth,
		/// <summary>
		/// Door shape, shining east.
		/// </summary>
		DoorEast,
		/// <summary>
		/// Large semicircular shape (180 degrees), north wall.
		/// </summary>
		NorthBig,
		/// <summary>
		/// Large pie shape (90 degrees), north-east corner.
		/// </summary>
		NorthEastBig,
		/// <summary>
		/// Large semicircular shape (180 degrees), east wall.
		/// </summary>
		EastBig,
		/// <summary>
		/// Large semicircular shape (180 degrees), west wall.
		/// </summary>
		WestBig,
		/// <summary>
		/// Large pie shape (90 degrees), south-west corner.
		/// </summary>
		SouthWestBig,
		/// <summary>
		/// Large semicircular shape (180 degrees), south wall.
		/// </summary>
		SouthBig,
		/// <summary>
		/// Medium semicircular shape (180 degrees), north wall.
		/// </summary>
		NorthSmall,
		/// <summary>
		/// Medium pie shape (90 degrees), north-east corner.
		/// </summary>
		NorthEastSmall,
		/// <summary>
		/// Medium semicircular shape (180 degrees), east wall.
		/// </summary>
		EastSmall,
		/// <summary>
		/// Medium semicircular shape (180 degrees), west wall.
		/// </summary>
		WestSmall,
		/// <summary>
		/// Medium semicircular shape (180 degrees), south wall.
		/// </summary>
		SouthSmall,
		/// <summary>
		/// Shaped like a wall decoration, north wall.
		/// </summary>
		DecorationNorth,
		/// <summary>
		/// Shaped like a wall decoration, north-east corner.
		/// </summary>
		DecorationNorthEast,
		/// <summary>
		/// Small semicircular shape (180 degrees), east wall.
		/// </summary>
		EastTiny,
		/// <summary>
		/// Shaped like a wall decoration, west wall.
		/// </summary>
		DecorationWest,
		/// <summary>
		/// Shaped like a wall decoration, south-west corner.
		/// </summary>
		DecorationSouthWest,
		/// <summary>
		/// Small semicircular shape (180 degrees), south wall.
		/// </summary>
		SouthTiny,
		/// <summary>
		/// Window shape, rectangular, no ray, shining south.
		/// </summary>
		RectWindowSouthNoRay,
		/// <summary>
		/// Window shape, rectangular, no ray, shining east.
		/// </summary>
		RectWindowEastNoRay,
		/// <summary>
		/// Window shape, rectangular, ray shining south.
		/// </summary>
		RectWindowSouth,
		/// <summary>
		/// Window shape, rectangular, ray shining east.
		/// </summary>
		RectWindowEast,
		/// <summary>
		/// Window shape, arched, no ray, shining south.
		/// </summary>
		ArchedWindowSouthNoRay,
		/// <summary>
		/// Window shape, arched, no ray, shining east.
		/// </summary>
		ArchedWindowEastNoRay,
		/// <summary>
		/// Window shape, arched, ray shining south.
		/// </summary>
		ArchedWindowSouth,
		/// <summary>
		/// Large circular shape.
		/// </summary>
		Circle300,
		/// <summary>
		/// Large pie shape (90 degrees), north-west corner.
		/// </summary>
		NorthWestBig,
		/// <summary>
		/// Negative light. Medium pie shape (90 degrees), south-east corner.
		/// </summary>
		DarkSouthEast,
		/// <summary>
		/// Negative light. Medium semicircular shape (180 degrees), south wall.
		/// </summary>
		DarkSouth,
		/// <summary>
		/// Negative light. Medium pie shape (90 degrees), north-west corner.
		/// </summary>
		DarkNorthWest,
		/// <summary>
		/// Negative light. Medium pie shape (90 degrees), south-east corner. Equivalent to <c>LightType.SouthEast</c>.
		/// </summary>
		DarkSouthEast2,
		/// <summary>
		/// Negative light. Medium circular shape (180 degrees), east wall.
		/// </summary>
		DarkEast,
		/// <summary>
		/// Negative light. Large circular shape.
		/// </summary>
		DarkCircle300,
		/// <summary>
		/// Opened door shape, shining south.
		/// </summary>
		DoorOpenSouth,
		/// <summary>
		/// Opened door shape, shining east.
		/// </summary>
		DoorOpenEast,
		/// <summary>
		/// Window shape, square, ray shining east.
		/// </summary>
		SquareWindowEast,
		/// <summary>
		/// Window shape, square, no ray, shining east.
		/// </summary>
		SquareWindowEastNoRay,
		/// <summary>
		/// Window shape, square, ray shining south.
		/// </summary>
		SquareWindowSouth,
		/// <summary>
		/// Window shape, square, no ray, shining south.
		/// </summary>
		SquareWindowSouthNoRay,
		/// <summary>
		/// Empty.
		/// </summary>
		Empty,
		/// <summary>
		/// Window shape, skinny, no ray, shining south.
		/// </summary>
		SkinnyWindowSouthNoRay,
		/// <summary>
		/// Window shape, skinny, ray shining east.
		/// </summary>
		SkinnyWindowEast,
		/// <summary>
		/// Window shape, skinny, no ray, shining east.
		/// </summary>
		SkinnyWindowEastNoRay,
		/// <summary>
		/// Shaped like a hole, shining south.
		/// </summary>
		HoleSouth,
		/// <summary>
		/// Shaped like a hole, shining south.
		/// </summary>
		HoleEast,
		/// <summary>
		/// Large circular shape with a moongate graphic embeded.
		/// </summary>
		Moongate,
		/// <summary>
		/// Unknown usage. Many rows of slightly angled lines.
		/// </summary>
		Strips,
		/// <summary>
		/// Shaped like a small hole, shining south.
		/// </summary>
		SmallHoleSouth,
		/// <summary>
		/// Shaped like a small hole, shining east.
		/// </summary>
		SmallHoleEast,
		/// <summary>
		/// Large semicircular shape (180 degrees), north wall. Identical graphic as <c>LightType.NorthBig</c>, but slightly different positioning.
		/// </summary>
		NorthBig2,
		/// <summary>
		/// Large semicircular shape (180 degrees), west wall. Identical graphic as <c>LightType.WestBig</c>, but slightly different positioning.
		/// </summary>
		WestBig2,
		/// <summary>
		/// Large pie shape (90 degrees), north-west corner. Equivalent to <c>LightType.NorthWestBig</c>.
		/// </summary>
		NorthWestBig2
	}

	/// <summary>
	/// Enumeration of an item's loot and steal state.
	/// </summary>
	public enum LootType : byte
	{
		/// <summary>
		/// Stealable. Lootable.
		/// </summary>
		Regular = 0,
		/// <summary>
		/// Unstealable. Unlootable, unless owned by a murderer.
		/// </summary>
		Newbied = 1,
		/// <summary>
		/// Unstealable. Unlootable, always.
		/// </summary>
		Blessed = 2,
		/// <summary>
		/// Stealable. Lootable, always.
		/// </summary>
		Cursed  = 3,
		/// <summary>
		/// Stealable. UnLootable, always.
		/// </summary>
		Special  = 4,
	}

    public enum AuditType
    {
        GoldLifted,             // looting a fallen monster, etc
        GoldDropBackpack,       //
        GoldDropBank,
        CheckDropBackpack,
        CheckDropBank,
    }

	public class BounceInfo
	{
		public Map m_Map;
		public Point3D m_Location, m_WorldLoc;
		public object m_Parent;

		public BounceInfo( Item item )
		{
			m_Map = item.Map;
			m_Location = item.Location;
			m_WorldLoc = item.GetWorldLocation();
			m_Parent = item.Parent;
		}

		private BounceInfo( Map map, Point3D loc, Point3D worldLoc, object parent )
		{
			m_Map = map;
			m_Location = loc;
			m_WorldLoc = worldLoc;
			m_Parent = parent;
		}

		public static BounceInfo Deserialize( GenericReader reader )
		{
			if ( reader.ReadBool() )
			{
				Map map = reader.ReadMap();
				Point3D loc = reader.ReadPoint3D();
				Point3D worldLoc = reader.ReadPoint3D();

				object parent;

				Serial serial = reader.ReadInt();

				if ( serial.IsItem )
					parent = World.FindItem( serial );
				else if ( serial.IsMobile )
					parent = World.FindMobile( serial );
				else
					parent = null;

				return new BounceInfo( map, loc, worldLoc, parent );
			}
			else
			{
				return null;
			}
		}

		public static void Serialize( BounceInfo info, GenericWriter writer )
		{
			if ( info == null )
			{
				writer.Write( false );
			}
			else
			{
				writer.Write( true );

				writer.Write( info.m_Map );
				writer.Write( info.m_Location );
				writer.Write( info.m_WorldLoc );

				if ( info.m_Parent is Mobile )
					writer.Write( (Mobile) info.m_Parent );
				else if ( info.m_Parent is Item )
					writer.Write( (Item) info.m_Parent );
				else
					writer.Write( (Serial) 0 );
			}
		}
	}

	public class Item : IPoint3D, IEntity, IHued
	{
        public int CompareTo(IEntity other)
        {
            if (other == null)
                return -1;

            return m_Serial.CompareTo(other.Serial);
        }

        public int CompareTo(Item other)
        {
            return this.CompareTo((IEntity)other);
        }

        public int CompareTo(object other)
        {
            if (other == null || other is IEntity)
                return this.CompareTo((IEntity)other);

            throw new ArgumentException();
        }


		public static readonly EmptyArrayList EmptyItems = new EmptyArrayList();

		private Serial m_Serial;
		private Point3D m_Location;
		private int m_ItemID;
		private int m_Hue;
		private int m_Amount;
		private Layer m_Layer;
		private string m_Name;
		private object m_Parent; // Mobile, Item, or null=World
		private ArrayList m_Items;
		private double m_Weight;
		private int m_TotalItems;
		private int m_TotalWeight;
		private int m_TotalGold;
		private Map m_Map;
		private LootType m_LootType;
		private DateTime m_LastMovedTime;	// 
		private DateTime m_LastAccessed;	// for freezedry stuff, not serialized
		private Direction m_Direction;

		private BounceInfo m_Bounce;

		private ItemDelta m_DeltaFlags;
		private ImplFlag m_Flags;

		#region Packet caches
		private Packet m_WorldPacket;
		private Packet m_RemovePacket;

		private Packet m_OPLPacket;
		private ObjectPropertyList m_PropertyList;
		#endregion

		private byte[] m_SerializedContentsBin;
		private byte[] m_SerializedContentsIdx;

        // allow items to relay status back to the GM.
        public virtual void SendMessage(string text)
        {
            IPooledEnumerable eable = this.GetMobilesInRange(10);
            foreach (Mobile m in eable)
                if (m.AccessLevel >= AccessLevel.GameMaster)
                    m.SendMessage(text);
            eable.Free();
        }

		private double m_DropRate = 1.0;	// 100% drop rate by default. Not serialized if 100%

		[CommandProperty(AccessLevel.GameMaster)]
		public double DropRate
		{
			get
			{
				return m_DropRate;
			}

			set
			{
				m_DropRate = value;
			}
		}

		// wea: 13/Mar/2007 Added new m_RareData property + various
		// accessors to shift the int for its data
		private UInt32 m_RareData;

		[CommandProperty(AccessLevel.Owner)]
		public UInt32 RareData
		{
			get
			{
				return m_RareData;
			}

			set
			{
				m_RareData = value;
			}
		}
				
		[CommandProperty(AccessLevel.GameMaster)]
		public UInt32 RareCurIndex
		{ 
			get 
			{ 
				return (m_RareData > 0 ? m_RareData & 0xFF : 0); 
			} 
		}
		
		[CommandProperty(AccessLevel.GameMaster)]
		public UInt32 RareStartIndex 
		{ 
			get 
			{ 
				return (m_RareData > 0 ?(m_RareData>>8) & 0xFF : 0);
			} 
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public UInt32 RareLastIndex 
		{ 
			get 
			{ 
				return (m_RareData>>16) & 0xFF;
			} 
		}

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Audited
        {
            get { return GetFlag(ImplFlag.Audited); }
            set { SetFlag(ImplFlag.Audited, value); InvalidateProperties(); }
        }

		[CommandProperty( AccessLevel.GameMaster )]
		public bool ToDelete
		{
			get{ return GetFlag( ImplFlag.ToDelete ); }
			set{ SetFlag( ImplFlag.ToDelete, value ); InvalidateProperties(); }
		}

		private int m_TempFlags, m_SavedFlags;

		public int TempFlags
		{
			get{ return m_TempFlags; }
			set{ m_TempFlags = value; }
		}

		public int SavedFlags
		{
			get{ return m_SavedFlags; }
			set{ m_SavedFlags = value; }
		}

		[CommandProperty(AccessLevel.Administrator)]
		public bool Debug
		{
			get
			{
				return GetFlag(ImplFlag.Debug);
			}
			set
			{
				SetFlag(ImplFlag.Debug, value);
			}
		}

		[CommandProperty(AccessLevel.Administrator)]
		public Container ParentContainer
		{
			get
			{   // do not go up to the root
                return m_Parent as Container;
			}
		}

		[CommandProperty(AccessLevel.Administrator)]
		public Mobile ParentMobile
		{
			get
			{
				return RootParent as Mobile;
			}
		}

		private Mobile m_HeldBy;

		/// <summary>
		/// The <see cref="Mobile" /> who is currently <see cref="Mobile.Holding">holding</see> this item.
		/// </summary>
		public Mobile HeldBy
		{
			get{ return m_HeldBy; }
			set{ m_HeldBy = value; }
		}

		[Flags]
		private enum ImplFlag
		{
			None			    = 0x00000000,
			Visible			    = 0x00000001,
			Movable			    = 0x00000002,
			Deleted			    = 0x00000004,
			Stackable		    = 0x00000008,
			InQueue			    = 0x00000010,
			PlayerCrafted	    = 0x00000020,	// Adam: Is this Player crafted?
            IsIntMapStorage     = 0x00000040,	// Adam: Is Internal Map Storage?
			FreezeDried		    = 0x00000080,	 
			Debug			    = 0x00000100,
IsTemplate = 0x00000200,	// is this a template item (used in spawners) (THIS SHOULD BE CONVERTED TO IsIntMapStorage)
			ToDelete		    = 0x00000400,	// is this thing marked to delete?
			PlayerQuest		    = 0x00000800,	// is this thing a PlayerQuest item?
            HideAttributes      = 0x00001000,	// suppress all weapon/armor attributes?
            Audited             = 0x00002000,	// Adam: Wealth Tracker system use ONLY
		}

		private void SetFlag( ImplFlag flag, bool value )
		{
			if ( value )
				m_Flags |= flag;
			else
				m_Flags &= ~flag;
		}

		private bool GetFlag( ImplFlag flag )
		{
			return ( (m_Flags & flag) != 0 );
		}

		public BounceInfo GetBounce()
		{
			return m_Bounce;
		}

		public void RecordBounce()
		{
			m_Bounce = new BounceInfo( this );
		}

		public void ClearBounce()
		{
			BounceInfo bounce = m_Bounce;

			if ( bounce != null )
			{
				m_Bounce = null;

				if ( bounce.m_Parent is Item )
				{
					Item parent = (Item) bounce.m_Parent;

					if ( !parent.Deleted )
						parent.OnItemBounceCleared( this );
				}
				else if ( bounce.m_Parent is Mobile )
				{
					Mobile parent = (Mobile) bounce.m_Parent;

					if ( !parent.Deleted )
						parent.OnItemBounceCleared( this );
				}
			}
		}

		/// <summary>
		/// Overridable. Virtual event invoked when a client, <paramref name="from" />, invokes a 'help request' for the Item. Seemingly no longer functional in newer clients.
		/// </summary>
		public virtual void OnHelpRequest( Mobile from )
		{
		}

		/// <summary>
		/// Overridable. Method checked to see if the item can be traded.
		/// </summary>
		/// <returns>True if the trade is allowed, false if not.</returns>
		public virtual bool AllowSecureTrade( Mobile from, Mobile to, Mobile newOwner, bool accepted )
		{
			return true;
		}

		/// <summary>
		/// Overridable. Virtual event invoked when a trade has completed, either successfully or not.
		/// </summary>
		public virtual void OnSecureTrade( Mobile from, Mobile to, Mobile newOwner, bool accepted )
		{
		}

		/// <summary>
		/// Overridable. Method checked to see if the elemental resistances of this Item conflict with another Item on the <see cref="Mobile" />.
		/// </summary>
		/// <returns>
		/// <list type="table">
		/// <item>
		/// <term>True</term>
		/// <description>There is a confliction. The elemental resistance bonuses of this Item should not be applied to the <see cref="Mobile" /></description>
		/// </item>
		/// <item>
		/// <term>False</term>
		/// <description>There is no confliction. The bonuses should be applied.</description>
		/// </item>
		/// </list>
		/// </returns>
		public virtual bool CheckPropertyConfliction( Mobile m )
		{
			return false;
		}

		/// <summary>
		/// Overridable. Sends the <see cref="PropertyList">object property list</see> to <paramref name="from" />.
		/// </summary>
		public virtual void SendPropertiesTo( Mobile from )
		{
			from.Send( PropertyList );
		}

		/// <summary>
		/// Overridable. Adds the name of this item to the given <see cref="ObjectPropertyList" />. This method should be overriden if the item requires a complex naming format.
		/// </summary>
		public virtual void AddNameProperty( ObjectPropertyList list )
		{
			if ( m_Name == null )
			{
				if ( m_Amount <= 1 )
					list.Add( LabelNumber );
				else
					list.Add( 1050039, "{0}\t#{1}", m_Amount, LabelNumber ); // ~1_NUMBER~ ~2_ITEMNAME~
			}
			else
			{
				if ( m_Amount <= 1 )
					list.Add( m_Name );
				else
					list.Add( 1050039, "{0}\t{1}", m_Amount, Name ); // ~1_NUMBER~ ~2_ITEMNAME~
			}
		}

		/// <summary>
		/// Overridable. Adds the loot type of this item to the given <see cref="ObjectPropertyList" />. By default, this will be either 'blessed', 'cursed', or 'insured'.
		/// </summary>
		public virtual void AddLootTypeProperty( ObjectPropertyList list )
		{
			if ( m_LootType == LootType.Blessed )
				list.Add( 1038021 ); // blessed
			else if ( m_LootType == LootType.Cursed )
				list.Add( 1049643 ); // cursed
			//else if ( Insured )
			//list.Add( 1061682 ); // <b>insured</b>
		}
		/*
				/// <summary>
				/// Overridable. Adds any elemental resistances of this item to the given <see cref="ObjectPropertyList" />.
				/// </summary>
				public virtual void AddResistanceProperties( ObjectPropertyList list )
				{
					int v = PhysicalResistance;

					if ( v != 0 )
						list.Add( 1060448, v.ToString() ); // physical resist ~1_val~%

					v = FireResistance;

					if ( v != 0 )
						list.Add( 1060447, v.ToString() ); // fire resist ~1_val~%

					v = ColdResistance;

					if ( v != 0 )
						list.Add( 1060445, v.ToString() ); // cold resist ~1_val~%

					v = PoisonResistance;

					if ( v != 0 )
						list.Add( 1060449, v.ToString() ); // poison resist ~1_val~%

					v = EnergyResistance;

					if ( v != 0 )
						list.Add( 1060446, v.ToString() ); // energy resist ~1_val~%
				}
		*/
		/// <summary>
		/// Overridable. Adds header properties. By default, this invokes <see cref="AddNameProperty" />, <see cref="AddBlessedForProperty" /> (if applicable), and <see cref="AddLootTypeProperty" /> (if <see cref="DisplayLootType" />).
		/// </summary>
		public virtual void AddNameProperties( ObjectPropertyList list )
		{
			AddNameProperty( list );

			if ( IsSecure )
				AddSecureProperty( list );
			else if ( IsLockedDown )
				AddLockedDownProperty( list );

			if ( m_BlessedFor != null && !m_BlessedFor.Deleted )
				AddBlessedForProperty( list, m_BlessedFor );

			if ( DisplayLootType )
				AddLootTypeProperty( list );

			AppendChildNameProperties( list );
		}

		/// <summary>
		/// Overridable. Adds the "Locked Down & Secure" property to the given <see cref="ObjectPropertyList" />.
		/// </summary>
		public virtual void AddSecureProperty( ObjectPropertyList list )
		{
			list.Add( 501644 ); // locked down & secure
		}

		/// <summary>
		/// Overridable. Adds the "Locked Down" property to the given <see cref="ObjectPropertyList" />.
		/// </summary>
		public virtual void AddLockedDownProperty( ObjectPropertyList list )
		{
			list.Add( 501643 ); // locked down
		}

		/// <summary>
		/// Overridable. Adds the "Blessed for ~1_NAME~" property to the given <see cref="ObjectPropertyList" />.
		/// </summary>
		public virtual void AddBlessedForProperty( ObjectPropertyList list, Mobile m )
		{
			list.Add( 1062203, "{0}", m.Name ); // Blessed for ~1_NAME~
		}

		/// <summary>
		/// Overridable. Fills an <see cref="ObjectPropertyList" /> with everything applicable. By default, this invokes <see cref="AddNameProperties" />, then <see cref="Item.GetChildProperties">Item.GetChildProperties</see> or <see cref="Mobile.GetChildProperties">Mobile.GetChildProperties</see>. This method should be overriden to add any custom properties.
		/// </summary>
		public virtual void GetProperties( ObjectPropertyList list )
		{
			AddNameProperties( list );
		}

		/// <summary>
		/// Overridable. Event invoked when a child (<paramref name="item" />) is building it's <see cref="ObjectPropertyList" />. Recursively calls <see cref="Item.GetChildProperties">Item.GetChildProperties</see> or <see cref="Mobile.GetChildProperties">Mobile.GetChildProperties</see>.
		/// </summary>
		public virtual void GetChildProperties( ObjectPropertyList list, Item item )
		{
			if ( m_Parent is Item )
				((Item)m_Parent).GetChildProperties( list, item );
			else if ( m_Parent is Mobile )
				((Mobile)m_Parent).GetChildProperties( list, item );
		}

		/// <summary>
		/// Overridable. Event invoked when a child (<paramref name="item" />) is building it's Name <see cref="ObjectPropertyList" />. Recursively calls <see cref="Item.GetChildProperties">Item.GetChildNameProperties</see> or <see cref="Mobile.GetChildProperties">Mobile.GetChildNameProperties</see>.
		/// </summary>
		public virtual void GetChildNameProperties( ObjectPropertyList list, Item item )
		{
			if ( m_Parent is Item )
				((Item)m_Parent).GetChildNameProperties( list, item );
			else if ( m_Parent is Mobile )
				((Mobile)m_Parent).GetChildNameProperties( list, item );
		}

		public void Bounce( Mobile from )
		{
			if ( m_Parent is Item )
				((Item)m_Parent).RemoveItem( this );
			else if ( m_Parent is Mobile )
				((Mobile)m_Parent).RemoveItem( this );

			m_Parent = null;

			if ( m_Bounce != null )
			{
				object parent = m_Bounce.m_Parent;

				if ( parent is Item && !((Item)parent).Deleted )
				{
					Item p = (Item)parent;
					object root = p.RootParent;

					//Pixie: 01/05/2006
					// Added bounceLocation check so that if the location of the parent or
					// rootparent is out of range of the mobile, then we drop to mobile's feet instead.
					Point3D bounceLocation = p.Location;
					if( root != null )
					{
						if( root is Mobile )
						{
							bounceLocation = ((Mobile)root).Location;
						}
						else if( root is Item )
						{
							bounceLocation = ((Item)root).Location;
						}
					}

					bool bounceLocationInRange = Utility.InRange( from.Location, bounceLocation, 3 );

					if ( bounceLocationInRange
						&& p.IsAccessibleTo( from ) 
						&& ( !(root is Mobile) || ((Mobile)root).CheckNonlocalDrop( from, this, p ) )
						// note to taran kain: probably should remove secure restriction here in the future!
						&& ( !(p is Container) || (!((Container)p).IsSecure || (((Container)p).IsSecure && ((Container)p).CheckHold(from, this, false))) )
					   )
					{
						Location = m_Bounce.m_Location;
						p.AddItem( this );
					}
					else
					{
						MoveToWorld( from.Location, from.Map );
					}
				}
				else if ( parent is Mobile && !((Mobile)parent).Deleted )
				{
					if ( !((Mobile)parent).EquipItem( this ) )
						MoveToWorld( m_Bounce.m_WorldLoc, m_Bounce.m_Map );
				}
				else
				{
					MoveToWorld( m_Bounce.m_WorldLoc, m_Bounce.m_Map );
				}
			}
			else
			{
				MoveToWorld( from.Location, from.Map );
			}

			ClearBounce();
		}

		/// <summary>
		/// Overridable. Method checked to see if this item may be equiped while casting a spell. By default, this returns false. It is overriden on spellbook and spell channeling weapons or shields.
		/// </summary>
		/// <returns>True if it may, false if not.</returns>
		/// <example>
		/// <code>
		///	public override bool AllowEquipedCast( Mobile from )
		///	{
		///		if ( from.Int &gt;= 100 )
		///			return true;
		///		
		///		return base.AllowEquipedCast( from );
		/// }</code>
		/// 
		/// When placed in an Item script, the item may be cast when equiped if the <paramref name="from" /> has 100 or more intelligence. Otherwise, it will drop to their backpack.
		/// </example>
		public virtual bool AllowEquipedCast( Mobile from )
		{
			return false;
		}

		public virtual bool CheckConflictingLayer( Mobile m, Item item, Layer layer )
		{
			return ( m_Layer == layer );
		}

		public virtual bool CanEquip( Mobile m )
		{
			return ( m_Layer != Layer.Invalid && m.FindItemOnLayer( m_Layer ) == null );
		}

		public virtual void GetChildContextMenuEntries( Mobile from, ArrayList list, Item item )
		{
			if ( m_Parent is Item )
				((Item)m_Parent).GetChildContextMenuEntries( from, list, item );
			else if ( m_Parent is Mobile )
				((Mobile)m_Parent).GetChildContextMenuEntries( from, list, item );
		}

		public virtual void GetContextMenuEntries( Mobile from, ArrayList list )
		{
			if ( m_Parent is Item )
				((Item)m_Parent).GetChildContextMenuEntries( from, list, this );
			else if ( m_Parent is Mobile )
				((Mobile)m_Parent).GetChildContextMenuEntries( from, list, this );
		}

		public virtual bool VerifyMove( Mobile from )
		{
			return Movable;
		}

		public virtual DeathMoveResult OnParentDeath( Mobile parent )
		{
			if ( !Movable )
				return DeathMoveResult.RemainEquiped;
			else if ( parent.KeepsItemsOnDeath )
				return DeathMoveResult.MoveToBackpack;
			else if ( CheckBlessed( parent ) )
				return DeathMoveResult.MoveToBackpack;
			else if ( CheckNewbied() && parent.Kills < 5 )
				return DeathMoveResult.MoveToBackpack;
			else if (this.LootType == LootType.Special)
				return DeathMoveResult.MoveToBackpack;
			else
				return DeathMoveResult.MoveToCorpse;
		}

		public virtual DeathMoveResult OnInventoryDeath( Mobile parent )
		{
			if ( !Movable )
				return DeathMoveResult.MoveToBackpack;
			else if ( parent.KeepsItemsOnDeath )
				return DeathMoveResult.MoveToBackpack;
			else if ( CheckBlessed( parent ) )
				return DeathMoveResult.MoveToBackpack;
			else if ( CheckNewbied() && parent.Kills < 5 )
				return DeathMoveResult.MoveToBackpack;
			else if (this.LootType == LootType.Special)
				return DeathMoveResult.MoveToBackpack;
			else
				return DeathMoveResult.MoveToCorpse;
		}

		/// <summary>
		/// Moves the Item to <paramref name="location" />. The Item does not change maps.
		/// </summary>
		public virtual void MoveToWorld( Point3D location )
		{
			MoveToWorld( location, m_Map );
		}

		public void LabelTo( Mobile to, int number )
		{
			to.Send( new MessageLocalized( m_Serial, m_ItemID, MessageType.Label, 0x3B2, 3, number, "", "" ) );
		}

		public void LabelTo( Mobile to, int number, string args )
		{
			to.Send( new MessageLocalized( m_Serial, m_ItemID, MessageType.Label, 0x3B2, 3, number, "", args ) );
		}

		public void LabelTo( Mobile to, string text )
		{
			to.Send( new UnicodeMessage( m_Serial, m_ItemID, MessageType.Label, 0x3B2, 3, "ENU", "", text ) );
		}

		public void LabelTo( Mobile to, string format, params object[] args )
		{
			LabelTo( to, String.Format( format, args ) );
		}

		public void LabelToAffix( Mobile to, int number, AffixType type, string affix )
		{
			to.Send( new MessageLocalizedAffix( m_Serial, m_ItemID, MessageType.Label, 0x3B2, 3, number, "", type, affix, "" ) );
		}

		public void LabelToAffix( Mobile to, int number, AffixType type, string affix, string args )
		{
			to.Send( new MessageLocalizedAffix( m_Serial, m_ItemID, MessageType.Label, 0x3B2, 3, number, "", type, affix, args ) );
		}

		public virtual void LabelLootTypeTo( Mobile to )
		{
			if ( m_LootType == LootType.Blessed )
				LabelTo( to, 1041362 ); // (blessed)
			else if ( m_LootType == LootType.Cursed )
				LabelTo( to, "(cursed)" );
		}

		public bool AtWorldPoint( int x, int y )
		{
			return ( m_Parent == null && m_Location.m_X == x && m_Location.m_Y == y );
		}

		public bool AtPoint( int x, int y )
		{
			return ( m_Location.m_X == x && m_Location.m_Y == y );
		}

		/// <summary>
		/// Moves the Item to a given <paramref name="location" /> and <paramref name="map" />.
		/// </summary>
		public virtual void MoveToWorld( Point3D location, Map map )		// wea: virtualised
		{
			if ( Deleted )
				return;

			Point3D oldLocation = GetWorldLocation();
			Point3D oldRealLocation = m_Location;

			SetLastMoved();

			if ( Parent is Mobile )
				((Mobile)Parent).RemoveItem( this );
			else if ( Parent is Item )
				((Item)Parent).RemoveItem( this );

			if ( m_Map != map )
			{
				Map old = m_Map;

				if ( m_Map != null )
				{
					m_Map.OnLeave( this );

					if ( oldLocation.m_X != 0 )
					{
						Packet remPacket = null;

						IPooledEnumerable eable = m_Map.GetClientsInRange( oldLocation, GetMaxUpdateRange() );

						foreach ( NetState state in eable )
						{
							Mobile m = state.Mobile;

							if ( m.InRange( oldLocation, GetUpdateRange( m ) ) )
							{
								if ( remPacket == null )
									remPacket = this.RemovePacket;

								state.Send( remPacket );
							}
						}

						eable.Free();
					}
				}

				m_Location = location;
				this.OnLocationChange( oldRealLocation );

				Packet.Release( ref m_WorldPacket );

                if (m_Items != null)
                {
                    for (int i = 0; i < m_Items.Count; ++i)
                        ((Item)m_Items[i]).Map = map;
                }

				m_Map = map;

				if ( m_Map != null )
					m_Map.OnEnter( this );

				OnMapChange();

				if ( m_Map != null )
				{
					IPooledEnumerable eable = m_Map.GetClientsInRange( m_Location, GetMaxUpdateRange() );

					foreach ( NetState state in eable )
					{
						Mobile m = state.Mobile;

						if ( m.CanSee( this ) && m.InRange( m_Location, GetUpdateRange( m ) ) )
							SendInfoTo( state );
					}

					eable.Free();
				}

				RemDelta( ItemDelta.Update );

				if ( old == null || old == Map.Internal )
					InvalidateProperties();
			}
			else if ( m_Map != null )
			{
				IPooledEnumerable eable;

				if ( oldLocation.m_X != 0 )
				{
					Packet removeThis = null;

					eable = m_Map.GetClientsInRange( oldLocation, GetMaxUpdateRange() );

					foreach ( NetState state in eable )
					{
						Mobile m = state.Mobile;

						if ( !m.InRange( location, GetUpdateRange( m ) ) )
						{
							if ( removeThis == null )
								removeThis = this.RemovePacket;

							state.Send( removeThis );
						}
					}

					eable.Free();
				}

				Point3D oldInternalLocation = m_Location;

				m_Location = location;
				this.OnLocationChange( oldRealLocation );

				Packet.Release( ref m_WorldPacket );

				eable = m_Map.GetClientsInRange( m_Location, GetMaxUpdateRange() );

				foreach ( NetState state in eable )
				{
					Mobile m = state.Mobile;

					if ( m.CanSee( this ) && m.InRange( m_Location, GetUpdateRange( m ) ) )
						SendInfoTo( state );
				}

				eable.Free();

				m_Map.OnMove( oldInternalLocation, this );

				RemDelta( ItemDelta.Update );
			}
			else
			{
				Map = map;
				Location = location;
			}
		}

		Point3D IEntity.Location
		{
			get
			{
				return m_Location;
			}
		}

		/// <summary>
		/// Has the item been deleted?
		/// </summary>
		public bool Deleted{ get{ return GetFlag( ImplFlag.Deleted ); } }

		[CommandProperty( AccessLevel.GameMaster )]
		public LootType LootType
		{
			get
			{
				return m_LootType;
			}
			set
			{
				if ( m_LootType != value )
				{
					m_LootType = value;

					if ( DisplayLootType )
						InvalidateProperties();
				}
			}
		}

		private static TimeSpan m_DDT = TimeSpan.FromHours( 1.0 );

		public static TimeSpan DefaultDecayTime{ get{ return m_DDT; } set{ m_DDT = value; } }

		[CommandProperty( AccessLevel.GameMaster )]
		public virtual TimeSpan DecayTime
		{
			get
			{
				return m_DDT;
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public virtual bool Decays
		{
			get
			{
				return (Movable && Visible);
			}
		}

		public virtual bool OnDecay()
		{
			return ( Decays && Parent == null && Map != Map.Internal && Region.Find( Location, Map ).OnDecay( this ) );
		}

		public void SetLastMoved()
		{
			m_LastMovedTime = DateTime.Now;
		}

		public DateTime LastMoved
		{
			get
			{
				return m_LastMovedTime;
			}
			set
			{
				m_LastMovedTime = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public DateTime LastAccessed
		{
			get
			{
				return m_LastAccessed;
			}
			set
			{
				m_LastAccessed = value;
			}
		}

		public virtual bool StackWith( Mobile from, Item dropped )
		{
			return StackWith( from, dropped, true );
		}

		public virtual bool StackWith( Mobile from, Item dropped, bool playSound )
		{
			if (Stackable && dropped.Stackable && dropped.GetType() == GetType() && dropped.ItemID == ItemID && dropped.Name == Name && dropped.Hue == Hue && (dropped.Amount + Amount) <= 60000)
			{
				if ( m_LootType != dropped.m_LootType )
					m_LootType = LootType.Regular;

				Amount += dropped.Amount;
				dropped.Delete();

				if ( playSound )
				{
					int soundID = GetDropSound();

					if ( soundID == -1 )
						soundID = 0x42;

					from.SendSound( soundID, GetWorldLocation() );
				}

				return true;
			}

			return false;
		}

		public virtual bool OnDragDrop( Mobile from, Item dropped )
		{
			if ( Parent is Container )
				return ((Container)Parent).OnStackAttempt( from, this, dropped );

			return StackWith( from, dropped );
		}

		public Rectangle2D GetGraphicBounds()
		{
			int itemID = m_ItemID;
			bool doubled = m_Amount > 1;

			if ( itemID >= 0xEEA && itemID <= 0xEF2 ) // Are we coins?
			{
				int coinBase = (itemID - 0xEEA) / 3;
				coinBase *= 3;
				coinBase += 0xEEA;

				doubled = false;

				if ( m_Amount <= 1 )
				{
					// A single coin
					itemID = coinBase;
				}
				else if ( m_Amount <= 5 )
				{
					// A stack of coins
					itemID = coinBase + 1;
				}
				else // m_Amount > 5
				{
					// A pile of coins
					itemID = coinBase + 2;
				}
			}

			Rectangle2D bounds = ItemBounds.Table[itemID & 0x3FFF];

			if ( doubled )
			{
				bounds.Set( bounds.X, bounds.Y, bounds.Width + 5, bounds.Height + 5 );
			}

			return bounds;
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Stackable
		{
			get{ return GetFlag( ImplFlag.Stackable ); }
			set{ SetFlag( ImplFlag.Stackable, value ); }
		}

		public Packet RemovePacket
		{
			get
			{
				if ( m_RemovePacket == null )
				{
					m_RemovePacket = new RemoveItem( this );
					m_RemovePacket.SetStatic();
				}

				return m_RemovePacket;
			}
		}

		public Packet OPLPacket
		{
			get
			{
				if ( m_OPLPacket == null )
				{
					m_OPLPacket = new OPLInfo( PropertyList );
					m_OPLPacket.SetStatic();
				}

				return m_OPLPacket;
			}
		}

		public ObjectPropertyList PropertyList
		{
			get
			{
				if ( m_PropertyList == null )
				{
					m_PropertyList = new ObjectPropertyList( this );

					GetProperties( m_PropertyList );
					AppendChildProperties( m_PropertyList );

					m_PropertyList.Terminate();
					m_PropertyList.SetStatic();
				}

				return m_PropertyList;
			}
		}

		public virtual void AppendChildProperties( ObjectPropertyList list )
		{
			if ( m_Parent is Item )
				((Item)m_Parent).GetChildProperties( list, this );
			else if ( m_Parent is Mobile )
				((Mobile)m_Parent).GetChildProperties( list, this );
		}

		public virtual void AppendChildNameProperties( ObjectPropertyList list )
		{
			if ( m_Parent is Item )
				((Item)m_Parent).GetChildNameProperties( list, this );
			else if ( m_Parent is Mobile )
				((Mobile)m_Parent).GetChildNameProperties( list, this );
		}

		public void ClearProperties()
		{
			Packet.Release( ref m_PropertyList );
			Packet.Release( ref m_OPLPacket );
		}

		public void InvalidateProperties()
		{
			if ( !Core.AOS )
				return;

			if ( m_Map != null && m_Map != Map.Internal && !World.Loading )
			{
				ObjectPropertyList oldList = m_PropertyList;
				m_PropertyList = null;
				ObjectPropertyList newList = PropertyList;

				if ( oldList == null || oldList.Hash != newList.Hash )
				{
					Packet.Release( ref m_OPLPacket );
					Delta( ItemDelta.Properties );
				}
			}
			else
			{
				ClearProperties();
			}
		}

		public Packet WorldPacket
		{
			get
			{
				// This needs to be invalidated when any of the following changes:
				//  - ItemID
				//  - Amount
				//  - Location
				//  - Hue
				//  - Packet Flags
				//  - Direction

				if ( m_WorldPacket == null )
				{
					m_WorldPacket = new WorldItem( this );
					m_WorldPacket.SetStatic();
				}

				return m_WorldPacket;
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Visible
		{
			get{ return GetFlag( ImplFlag.Visible ); }
			set
			{
				if ( GetFlag( ImplFlag.Visible ) != value )
				{
					SetFlag( ImplFlag.Visible, value );
					Packet.Release( ref m_WorldPacket );

					if ( m_Map != null )
					{
						Packet removeThis = null;
						Point3D worldLoc = GetWorldLocation();

						IPooledEnumerable eable = m_Map.GetClientsInRange( worldLoc, GetMaxUpdateRange() );

						foreach ( NetState state in eable )
						{
							Mobile m = state.Mobile;

							if ( !m.CanSee( this ) && m.InRange( worldLoc, GetUpdateRange( m ) ) )
							{
								if ( removeThis == null )
									removeThis = this.RemovePacket;

								state.Send( removeThis );
							}
						}

						eable.Free();
					}

					Delta( ItemDelta.Update );
				}
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Movable
		{
			get{ return GetFlag( ImplFlag.Movable ); }
			set
			{
				if ( GetFlag( ImplFlag.Movable ) != value )
				{
					SetFlag( ImplFlag.Movable, value );
					Packet.Release( ref m_WorldPacket );
					Delta( ItemDelta.Update );
				}
			}
		}

		public virtual bool ForceShowProperties{ get{ return false; } }

		public virtual int GetPacketFlags()
		{
			int flags = 0;

			if ( !Visible )
				flags |= 0x80;

			if ( Movable || ForceShowProperties )
				flags |= 0x20;

			return flags;
		}

		public virtual bool OnMoveOff( Mobile m )
		{
			return true;
		}

		public virtual bool OnMoveOver( Mobile m )
		{
			return true;
		}

		public virtual bool HandlesOnMovement{ get{ return false; } }

		public virtual void OnMovement( Mobile m, Point3D oldLocation )
		{
		}

		public void Internalize()
		{
			MoveToWorld( Point3D.Zero, Map.Internal );
		}

		public virtual void OnMapChange()
		{
		}

		public virtual void OnRemoved( object parent )
		{
		}

		public virtual void OnAdded( object parent )
		{
		}
		
		[CommandProperty( AccessLevel.Counselor, AccessLevel.GameMaster )]
		public Map Map
		{
			get
			{
				return m_Map;
			}
			set
			{
				if ( m_Map != value )
				{
					Map old = m_Map;

					if ( m_Map != null && m_Parent == null )
					{
						m_Map.OnLeave( this );
							SendRemovePacket();
					}

					//					if ( m_Items == null )
					//						throw new Exception( String.Format( "Items array is null--are you calling the serialization constructor? Type={0}", GetType() ) );

					if ( m_Items != null )
					{
						for ( int i = 0; i < m_Items.Count; ++i )
							((Item)m_Items[i]).Map = value;
					}

					m_Map = value;

					if ( m_Map != null && m_Parent == null )
						m_Map.OnEnter( this );

					Delta( ItemDelta.Update );

					this.OnMapChange();

					if ( old == null || old == Map.Internal )
						InvalidateProperties();
				}
			}
		}

		[Flags]
		private enum SaveFlag
		{
			None			= 0x00000000,
			Direction		= 0x00000001,
			Bounce			= 0x00000002,
			LootType		= 0x00000004,
			LocationFull	= 0x00000008,
			ItemID			= 0x00000010,
			Hue				= 0x00000020,
			Amount			= 0x00000040,
			Layer			= 0x00000080,
			Name			= 0x00000100,
			Parent			= 0x00000200,
			Items			= 0x00000400,
			WeightNot1or0	= 0x00000800,
			Map				= 0x00001000,
			Visible			= 0x00002000,
			Movable			= 0x00004000,
			Stackable		= 0x00008000,
			WeightIs0		= 0x00010000,
			LocationSByteZ	= 0x00020000,
			LocationShortXY = 0x00040000,
			LocationByteXY	= 0x00080000,
			ImplFlags		= 0x00100000,
			InsuredFor		= 0x00200000,
			BlessedFor		= 0x00400000,
			HeldBy			= 0x00800000,
			IntWeight		= 0x01000000,
			SavedFlags		= 0x02000000,
			FreezeDried		= 0x04000000,
			RareData        = 0x08000000,	// wea: 13/Mar/2007 To handle rares generated by rare factory
			DropRate        = 0x10000000	// adam: custom drop rate? Used by the at least the Spawner / Loot Pack system
		}

		private static void SetSaveFlag( ref SaveFlag flags, SaveFlag toSet, bool setIf )
		{
			if ( setIf )
				flags |= toSet;
		}

		private static bool GetSaveFlag( SaveFlag flags, SaveFlag toGet )
		{
			return ( (flags & toGet) != 0 );
		}

		public virtual void Serialize( GenericWriter writer )
		{
			writer.Write( 10 ); // version

			SaveFlag flags = SaveFlag.None;

			int x = m_Location.m_X, y = m_Location.m_Y, z = m_Location.m_Z;

			if ( x != 0 || y != 0 || z != 0 )
			{
				if ( x >= short.MinValue && x <= short.MaxValue && y >= short.MinValue && y <= short.MaxValue && z >= sbyte.MinValue && z <= sbyte.MaxValue )
				{
					if ( x != 0 || y != 0 )
					{
						if ( x >= byte.MinValue && x <= byte.MaxValue && y >= byte.MinValue && y <= byte.MaxValue )
							flags |= SaveFlag.LocationByteXY;
						else
							flags |= SaveFlag.LocationShortXY;
					}

					if ( z != 0 )
						flags |= SaveFlag.LocationSByteZ;
				}
				else
				{
					flags |= SaveFlag.LocationFull;
				}
			}

			if ( m_Direction != Direction.North ) flags |= SaveFlag.Direction;
			if ( m_Bounce != null ) flags |= SaveFlag.Bounce;
			if ( m_LootType != LootType.Regular ) flags |= SaveFlag.LootType;
			if ( m_ItemID != 0 ) flags |= SaveFlag.ItemID;
			if ( m_Hue != 0 ) flags |= SaveFlag.Hue;
			if ( m_Amount != 1 ) flags |= SaveFlag.Amount;
			if ( m_Layer != Layer.Invalid ) flags |= SaveFlag.Layer;
			if ( m_Name != null ) flags |= SaveFlag.Name;
			if ( m_Parent != null ) flags |= SaveFlag.Parent;
			if ( m_Items != null && m_Items.Count > 0 ) flags |= SaveFlag.Items;
			if ( m_Map != Map.Internal ) flags |= SaveFlag.Map;
			//if ( m_InsuredFor != null && !m_InsuredFor.Deleted ) flags |= SaveFlag.InsuredFor;
			if ( m_BlessedFor != null && !m_BlessedFor.Deleted ) flags |= SaveFlag.BlessedFor;
			if ( m_HeldBy != null && !m_HeldBy.Deleted ) flags |= SaveFlag.HeldBy;
			if ( m_SavedFlags != 0 ) flags |= SaveFlag.SavedFlags;
			if (m_RareData != 0) flags |= SaveFlag.RareData;		//wea: 13/Mar/2007 Rare Factory
			if (m_DropRate != 1.0) flags |= SaveFlag.DropRate;		//Adam: Spawner Loot Packs

			if ( m_Weight == 0.0 )
			{
				flags |= SaveFlag.WeightIs0;
			}
			else if ( m_Weight != 1.0 )
			{
				if ( m_Weight == (int)m_Weight )
					flags |= SaveFlag.IntWeight;
				else
					flags |= SaveFlag.WeightNot1or0;
			}

			// see if anything here is set
            ImplFlag implFlags = (m_Flags & (ImplFlag.Visible | ImplFlag.Movable | ImplFlag.Stackable | ImplFlag.PlayerCrafted | ImplFlag.IsIntMapStorage | ImplFlag.FreezeDried | ImplFlag.Debug | ImplFlag.IsTemplate | ImplFlag.ToDelete | ImplFlag.PlayerQuest | ImplFlag.HideAttributes | ImplFlag.Audited));

			if ( implFlags != (ImplFlag.Visible | ImplFlag.Movable) )
				flags |= SaveFlag.ImplFlags;

			if (IsFreezeDried)
				flags |= SaveFlag.FreezeDried;

			// must be the first thing written 
			writer.WriteInt32( (int) flags );

			// version 10: Adam
			// Spawner Loot Packs
			if (GetSaveFlag(flags, SaveFlag.DropRate))
				writer.Write(m_DropRate);
			
			//
			// older versions follow
			//

			/* begin last moved time optimization */
			long ticks = m_LastMovedTime.Ticks;
			long now = DateTime.Now.Ticks;

			TimeSpan d;

			try{ d = new TimeSpan( ticks-now ); }
			catch{ if ( ticks < now ) d = TimeSpan.MaxValue; else d = TimeSpan.MaxValue; }

			double minutes = -d.TotalMinutes;

			if ( minutes < int.MinValue )
				minutes = int.MinValue;
			else if ( minutes > int.MaxValue )
				minutes = int.MaxValue;

			writer.WriteEncodedInt( (int) minutes );
			/* end */

			if ( GetSaveFlag( flags, SaveFlag.Direction ) )
				writer.Write( (byte) m_Direction );

			if ( GetSaveFlag( flags, SaveFlag.Bounce ) )
				BounceInfo.Serialize( m_Bounce, writer );

			if ( GetSaveFlag( flags, SaveFlag.LootType ) )
				writer.Write( (byte) m_LootType );

			if ( GetSaveFlag( flags, SaveFlag.LocationFull ) )
			{
				writer.WriteEncodedInt( x );
				writer.WriteEncodedInt( y );
				writer.WriteEncodedInt( z );
			}
			else
			{
				if ( GetSaveFlag( flags, SaveFlag.LocationByteXY ) )
				{
					writer.Write( (byte) x );
					writer.Write( (byte) y );
				}
				else if ( GetSaveFlag( flags, SaveFlag.LocationShortXY ) )
				{
					writer.Write( (short) x );
					writer.Write( (short) y );
				}

				if ( GetSaveFlag( flags, SaveFlag.LocationSByteZ ) )
					writer.Write( (sbyte) z );
			}

			if ( GetSaveFlag( flags, SaveFlag.ItemID ) )
				writer.WriteEncodedInt( (int) m_ItemID );

			if ( GetSaveFlag( flags, SaveFlag.Hue ) )
				writer.WriteEncodedInt( (int) m_Hue );

			if ( GetSaveFlag( flags, SaveFlag.Amount ) )
				writer.WriteEncodedInt( (int) m_Amount );

			if ( GetSaveFlag( flags, SaveFlag.Layer ) )
				writer.Write( (byte) m_Layer );

			if ( GetSaveFlag( flags, SaveFlag.Name ) )
				writer.Write( (string) m_Name );

			if ( GetSaveFlag( flags, SaveFlag.Parent ) )
			{
				if ( m_Parent is Mobile && !((Mobile)m_Parent).Deleted )
                    writer.WriteInt32(((Mobile)m_Parent).Serial);
				else if ( m_Parent is Item && !((Item)m_Parent).Deleted )
                    writer.WriteInt32(((Item)m_Parent).Serial);
				else
                    writer.WriteInt32((int)Serial.MinusOne);
			}

			if ( GetSaveFlag( flags, SaveFlag.Items ) )
				writer.WriteItemList( m_Items, false );

			if ( GetSaveFlag( flags, SaveFlag.IntWeight ) )
				writer.WriteEncodedInt( (int) m_Weight );
			else if ( GetSaveFlag( flags, SaveFlag.WeightNot1or0 ) )
				writer.Write( (double) m_Weight );

			if ( GetSaveFlag( flags, SaveFlag.Map ) )
				writer.Write( (Map) m_Map );

			if ( GetSaveFlag( flags, SaveFlag.ImplFlags ) )
				writer.WriteEncodedInt( (int) implFlags );

			// not to be confused with ImplFlag.FreezeDried
			if ( GetSaveFlag( flags, SaveFlag.FreezeDried ) )
			{
				writer.WriteInt32(TotalWeight);
                writer.WriteInt32(TotalItems);
                writer.WriteInt32(TotalGold);

				writer.Write((int)m_SerializedContentsIdx.Length);
				for (int i = 0; i < m_SerializedContentsIdx.Length; i++)
					writer.Write(m_SerializedContentsIdx[i]);
				writer.Write((int)m_SerializedContentsBin.Length);
				for (int i = 0; i < m_SerializedContentsBin.Length; i++)
					writer.Write(m_SerializedContentsBin[i]);
			}

			if ( GetSaveFlag( flags, SaveFlag.InsuredFor ) )
				writer.Write( (Mobile)null );

			if ( GetSaveFlag( flags, SaveFlag.BlessedFor ) )
				writer.Write( m_BlessedFor );

			if ( GetSaveFlag( flags, SaveFlag.HeldBy ) )
				writer.Write( m_HeldBy );

			if ( GetSaveFlag( flags, SaveFlag.SavedFlags ) )
				writer.WriteEncodedInt( m_SavedFlags );

			//wea: 13/Mar/2007 Rare Factory
			if (GetSaveFlag(flags, SaveFlag.RareData))
				writer.Write(m_RareData);
		}

		public virtual void Deserialize(GenericReader reader)
		{
			int version = reader.ReadInt();

			SetLastMoved();
			
			// must always read this first
			SaveFlag flags = 0;
			if (version >= 5)
				flags = (SaveFlag)reader.ReadInt32();

			switch (version)
			{
				case 10:
					{	// get the per item custom drop rate
						if (GetSaveFlag(flags, SaveFlag.DropRate))
							m_DropRate = reader.ReadDouble();
					}
					goto case 9;
				case 9:
					goto case 8;
				case 8:
					goto case 7;// change is at bottom of file after ImplFlags are read
				case 7:
					goto case 6;
				case 6:
					{
						if (version < 7)
						{
							LastMoved = reader.ReadDeltaTime();
						}
						else
						{
							int minutes = reader.ReadEncodedInt();

							try { LastMoved = DateTime.Now - TimeSpan.FromMinutes(minutes); }
							catch { LastMoved = DateTime.Now; }
						}

						if (GetSaveFlag(flags, SaveFlag.Direction))
							m_Direction = (Direction)reader.ReadByte();

						if (GetSaveFlag(flags, SaveFlag.Bounce))
							m_Bounce = BounceInfo.Deserialize(reader);

						if (GetSaveFlag(flags, SaveFlag.LootType))
							m_LootType = (LootType)reader.ReadByte();

						int x = 0, y = 0, z = 0;

						if (GetSaveFlag(flags, SaveFlag.LocationFull))
						{
							x = reader.ReadEncodedInt();
							y = reader.ReadEncodedInt();
							z = reader.ReadEncodedInt();
						}
						else
						{
							if (GetSaveFlag(flags, SaveFlag.LocationByteXY))
							{
								x = reader.ReadByte();
								y = reader.ReadByte();
							}
							else if (GetSaveFlag(flags, SaveFlag.LocationShortXY))
							{
								x = reader.ReadShort();
								y = reader.ReadShort();
							}

							if (GetSaveFlag(flags, SaveFlag.LocationSByteZ))
								z = reader.ReadSByte();
						}

						m_Location = new Point3D(x, y, z);

						if (GetSaveFlag(flags, SaveFlag.ItemID))
							m_ItemID = reader.ReadEncodedInt();

						if (GetSaveFlag(flags, SaveFlag.Hue))
							m_Hue = reader.ReadEncodedInt();

						if (GetSaveFlag(flags, SaveFlag.Amount))
							m_Amount = reader.ReadEncodedInt();
						else
							m_Amount = 1;

						if (GetSaveFlag(flags, SaveFlag.Layer))
							m_Layer = (Layer)reader.ReadByte();

						if (GetSaveFlag(flags, SaveFlag.Name))
							m_Name = reader.ReadString();

						if (GetSaveFlag(flags, SaveFlag.Parent))
						{
							Serial parent = reader.ReadInt32();

							if (parent.IsMobile)
								m_Parent = World.FindMobile(parent);
							else if (parent.IsItem)
								m_Parent = World.FindItem(parent);
							else
								m_Parent = null;

							if (m_Parent == null && (parent.IsMobile || parent.IsItem))
								Delete();
						}

						if (GetSaveFlag(flags, SaveFlag.Items))
							m_Items = reader.ReadItemList();
						//else
						//	m_Items = new ArrayList( 1 );

						if (GetSaveFlag(flags, SaveFlag.IntWeight))
							m_Weight = reader.ReadEncodedInt();
						else if (GetSaveFlag(flags, SaveFlag.WeightNot1or0))
							m_Weight = reader.ReadDouble();
						else if (GetSaveFlag(flags, SaveFlag.WeightIs0))
							m_Weight = 0.0;
						else
							m_Weight = 1.0;

						if (GetSaveFlag(flags, SaveFlag.Map))
							m_Map = reader.ReadMap();
						else
							m_Map = Map.Internal;

						if (GetSaveFlag(flags, SaveFlag.Visible))
							SetFlag(ImplFlag.Visible, reader.ReadBool());
						else
							SetFlag(ImplFlag.Visible, true);

						if (GetSaveFlag(flags, SaveFlag.Movable))
							SetFlag(ImplFlag.Movable, reader.ReadBool());
						else
							SetFlag(ImplFlag.Movable, true);

						if (GetSaveFlag(flags, SaveFlag.Stackable))
							SetFlag(ImplFlag.Stackable, reader.ReadBool());

						if (GetSaveFlag(flags, SaveFlag.ImplFlags))
						{
							m_Flags = (ImplFlag)reader.ReadEncodedInt();
						}

						// don't confuse ImplFlag.FreezeDried with SaveFlag.FreezeDried
						// we check different flags because of a version quirk - ask Taran
						if (GetFlag(ImplFlag.FreezeDried))
						{
							TotalWeight = reader.ReadInt32();
							TotalItems = reader.ReadInt32();
							TotalGold = reader.ReadInt32();
						}

						if (GetSaveFlag(flags, SaveFlag.FreezeDried))
						{
							int count = reader.ReadInt();
							m_SerializedContentsIdx = new byte[count];
							for (int i = 0; i < count; i++)
								m_SerializedContentsIdx[i] = reader.ReadByte();
							count = reader.ReadInt();
							m_SerializedContentsBin = new byte[count];
							for (int i = 0; i < count; i++)
								m_SerializedContentsBin[i] = reader.ReadByte();
						}

						if (GetSaveFlag(flags, SaveFlag.InsuredFor))
							/*m_InsuredFor = */
							reader.ReadMobile();

						if (GetSaveFlag(flags, SaveFlag.BlessedFor))
							m_BlessedFor = reader.ReadMobile();

						if (GetSaveFlag(flags, SaveFlag.HeldBy))
							m_HeldBy = reader.ReadMobile();

						if (GetSaveFlag(flags, SaveFlag.SavedFlags))
							m_SavedFlags = reader.ReadEncodedInt();

						//wea: 13/Mar/2007 Rare Factory
						if (GetSaveFlag(flags, SaveFlag.RareData))
							m_RareData = (UInt32)reader.ReadInt();

						if (m_Map != null && m_Parent == null)
							m_Map.OnEnter(this);

						break;
					}
				case 5:
					{
						//SaveFlag flags = (SaveFlag)reader.ReadInt();

						LastMoved = reader.ReadDeltaTime();

						if (GetSaveFlag(flags, SaveFlag.Direction))
							m_Direction = (Direction)reader.ReadByte();

						if (GetSaveFlag(flags, SaveFlag.Bounce))
							m_Bounce = BounceInfo.Deserialize(reader);

						if (GetSaveFlag(flags, SaveFlag.LootType))
							m_LootType = (LootType)reader.ReadByte();

						if (GetSaveFlag(flags, SaveFlag.LocationFull))
							m_Location = reader.ReadPoint3D();

						if (GetSaveFlag(flags, SaveFlag.ItemID))
							m_ItemID = reader.ReadInt();

						if (GetSaveFlag(flags, SaveFlag.Hue))
							m_Hue = reader.ReadInt();

						if (GetSaveFlag(flags, SaveFlag.Amount))
							m_Amount = reader.ReadInt();
						else
							m_Amount = 1;

						if (GetSaveFlag(flags, SaveFlag.Layer))
							m_Layer = (Layer)reader.ReadByte();

						if (GetSaveFlag(flags, SaveFlag.Name))
							m_Name = reader.ReadString();

						if (GetSaveFlag(flags, SaveFlag.Parent))
						{
							Serial parent = reader.ReadInt();

							if (parent.IsMobile)
								m_Parent = World.FindMobile(parent);
							else if (parent.IsItem)
								m_Parent = World.FindItem(parent);
							else
								m_Parent = null;

							if (m_Parent == null && (parent.IsMobile || parent.IsItem))
								Delete();
						}

						if (GetSaveFlag(flags, SaveFlag.Items))
							m_Items = reader.ReadItemList();
						//else
						//	m_Items = new ArrayList( 1 );

						if (GetSaveFlag(flags, SaveFlag.IntWeight))
							m_Weight = reader.ReadEncodedInt();
						else if (GetSaveFlag(flags, SaveFlag.WeightNot1or0))
							m_Weight = reader.ReadDouble();
						else if (GetSaveFlag(flags, SaveFlag.WeightIs0))
							m_Weight = 0.0;
						else
							m_Weight = 1.0;

						if (GetSaveFlag(flags, SaveFlag.Map))
							m_Map = reader.ReadMap();
						else
							m_Map = Map.Internal;

						if (GetSaveFlag(flags, SaveFlag.Visible))
							SetFlag(ImplFlag.Visible, reader.ReadBool());
						else
							SetFlag(ImplFlag.Visible, true);

						if (GetSaveFlag(flags, SaveFlag.Movable))
							SetFlag(ImplFlag.Movable, reader.ReadBool());
						else
							SetFlag(ImplFlag.Movable, true);

						if (GetSaveFlag(flags, SaveFlag.Stackable))
							SetFlag(ImplFlag.Stackable, reader.ReadBool());

						if (m_Map != null && m_Parent == null)
							m_Map.OnEnter(this);


						break;
					}
				case 4: // Just removed variables
				case 3:
					{
						m_Direction = (Direction)reader.ReadInt();

						goto case 2;
					}
				case 2:
					{
						m_Bounce = BounceInfo.Deserialize(reader);
						LastMoved = reader.ReadDeltaTime();

						goto case 1;
					}
				case 1:
					{
						m_LootType = (LootType)reader.ReadByte();//m_Newbied = reader.ReadBool();

						goto case 0;
					}
				case 0:
					{
						m_Location = reader.ReadPoint3D();
						m_ItemID = reader.ReadInt();
						m_Hue = reader.ReadInt();
						m_Amount = reader.ReadInt();
						m_Layer = (Layer)reader.ReadByte();
						m_Name = reader.ReadString();

						Serial parent = reader.ReadInt();

						if (parent.IsMobile)
							m_Parent = World.FindMobile(parent);
						else if (parent.IsItem)
							m_Parent = World.FindItem(parent);
						else
							m_Parent = null;

						if (m_Parent == null && (parent.IsMobile || parent.IsItem))
							Delete();

						int count = reader.ReadInt();

						if (count > 0)
						{
							m_Items = new ArrayList(count);

							for (int i = 0; i < count; ++i)
							{
								Item item = reader.ReadItem();

								if (item != null)
									m_Items.Add(item);
							}
						}

						m_Weight = reader.ReadDouble();

						if (version <= 3)
						{
							/*m_TotalItems =*/
							reader.ReadInt();
							/*m_TotalWeight =*/
							reader.ReadInt();
							/*m_TotalGold =*/
							reader.ReadInt();
						}

						m_Map = reader.ReadMap();
						SetFlag(ImplFlag.Visible, reader.ReadBool());
						SetFlag(ImplFlag.Movable, reader.ReadBool());

						if (version <= 3)
							/*m_Deleted =*/
							reader.ReadBool();

						Stackable = reader.ReadBool();

						if (m_Map != null && m_Parent == null)
							m_Map.OnEnter(this);

						break;
					}
			}

			if (m_HeldBy != null)
				Timer.DelayCall(TimeSpan.Zero, new TimerCallback(FixHolding_Sandbox));
		}

		public IPooledEnumerable GetObjectsInRange( int range )
		{
			Map map = m_Map;

			if ( map == null )
				return Map.NullEnumerable.Instance;

			if ( m_Parent == null )
				return map.GetObjectsInRange( m_Location, range );

			return map.GetObjectsInRange( GetWorldLocation(), range );
		}

		public IPooledEnumerable GetItemsInRange( int range )
		{
			Map map = m_Map;

			if ( map == null )
				return Map.NullEnumerable.Instance;

			if ( m_Parent == null )
				return map.GetItemsInRange( m_Location, range );

			return map.GetItemsInRange( GetWorldLocation(), range );
		}

		public IPooledEnumerable GetMobilesInRange( int range )
		{
			Map map = m_Map;

			if ( map == null )
				return Map.NullEnumerable.Instance;

			if ( m_Parent == null )
				return map.GetMobilesInRange( m_Location, range );

			return map.GetMobilesInRange( GetWorldLocation(), range );
		}

		public IPooledEnumerable GetClientsInRange( int range )
		{
			Map map = m_Map;

			if ( map == null )
				return Map.NullEnumerable.Instance;

			if ( m_Parent == null )
				return map.GetClientsInRange( m_Location, range );

			return map.GetClientsInRange( GetWorldLocation(), range );
		}

		private static int m_LockedDownFlag;
		private static int m_SecureFlag;

		public static int LockedDownFlag
		{
			get{ return m_LockedDownFlag; }
			set{ m_LockedDownFlag = value; }
		}

		public static int SecureFlag
		{
			get{ return m_SecureFlag; }
			set{ m_SecureFlag = value; }
		}

		public virtual bool IsLockedDown
		{
			get{ return GetTempFlag( m_LockedDownFlag ); }
			set
			{ 
				if (!value)
					CheckRehydrate();

				SetTempFlag( m_LockedDownFlag, value ); 
				InvalidateProperties(); 
			}
		}

		public virtual bool IsSecure
		{
			get{ return GetTempFlag( m_SecureFlag ); }
			set
			{
				if (!value)
					CheckRehydrate();

				SetTempFlag( m_SecureFlag, value );
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public bool IsFreezeDried
		{
			get
			{
				return GetFlag(ImplFlag.FreezeDried);
			}
		}

		public bool GetTempFlag( int flag )
		{
			return ( (m_TempFlags & flag) != 0 );
		}

		public void SetTempFlag( int flag, bool value )
		{
			if ( value )
				m_TempFlags |= flag;
			else
				m_TempFlags &= ~flag;
		}

		public bool GetSavedFlag( int flag )
		{
			return ( (m_SavedFlags & flag) != 0 );
		}

		public void SetSavedFlag( int flag, bool value )
		{
			if ( value )
				m_SavedFlags |= flag;
			else
				m_SavedFlags &= ~flag;
		}

		private void FixHolding_Sandbox()
		{
			Mobile heldBy = m_HeldBy;

			if ( heldBy != null )
			{
				if ( m_Bounce != null )
				{
					Bounce( heldBy );
				}
				else
				{
					heldBy.Holding = null;
					heldBy.AddToBackpack( this );
					ClearBounce();
				}
			}
		}

		public virtual int GetMaxUpdateRange()
		{
			return 18;
		}

		public virtual int GetUpdateRange( Mobile m )
		{
			return 18;
		}

        public virtual void SendInfoTo(NetState state)
        {
            SendInfoTo(state, ObjectPropertyList.Enabled);
        }

        public virtual void SendInfoTo(NetState state, bool sendOplPacket)
        {
            state.Send(GetWorldPacketFor(state));

            if (sendOplPacket)
            {
                state.Send(OPLPacket);
            }
        }

		protected virtual Packet GetWorldPacketFor( NetState state ) {
			return this.WorldPacket;
		}

		public void SetTotalGold( int value )
		{
			m_TotalGold = value;
		}

		public void SetTotalItems( int value )
		{
			m_TotalItems = value;
		}

		public void SetTotalWeight( int value )
		{
			m_TotalWeight = value;
		}

		public virtual bool IsVirtualItem{ get{ return false; } }

		public virtual void UpdateTotals()
		{
			if (GetFlag(ImplFlag.FreezeDried))
				return;

			m_LastAccessed = DateTime.Now;

			m_TotalGold = 0;
			m_TotalItems = 0;
			m_TotalWeight = 0;

			if ( m_Items == null )
				return;

			for ( int i = 0; i < m_Items.Count; ++i )
			{
				Item item = (Item)m_Items[i];

				item.UpdateTotals();

				m_TotalGold += item.TotalGold;
				m_TotalItems += item.TotalItems;// + item.Items.Count;
				m_TotalWeight += item.TotalWeight + item.PileWeight;

				if ( item.IsVirtualItem )
					--m_TotalItems;
			}

			//if ( this is Gold )
			//	m_TotalGold += m_Amount;

			m_TotalItems += m_Items.Count;
		}

		public virtual int LabelNumber
		{
			get
			{
				return 1020000 + (m_ItemID & 0x3FFF);
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int TotalGold
		{
			get
			{
				return m_TotalGold;
			}
			set
			{
				if ( m_TotalGold != value )
				{
					if ( m_Parent is Item )
					{
						Item parent = (Item)m_Parent;

						parent.TotalGold = (parent.TotalGold - m_TotalGold) + value;
					}
					else if ( m_Parent is Mobile && !(this is BankBox) )
					{
						Mobile parent = (Mobile)m_Parent;

						parent.TotalGold = (parent.TotalGold - m_TotalGold) + value;
					}

					m_TotalGold = value;
				}
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int TotalItems
		{
			get
			{
				return m_TotalItems;
			}
			set
			{
				if ( m_TotalItems != value )
				{
					if ( m_Parent is Item )
					{
						Item parent = (Item)m_Parent;

						parent.TotalItems = (parent.TotalItems - m_TotalItems) + value;
						parent.InvalidateProperties();
					}

					m_TotalItems = value;
					InvalidateProperties();
				}
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int TotalWeight
		{
			get
			{
				return m_TotalWeight;
			}
			set
			{
				if ( m_TotalWeight != value )
				{
					if ( m_Parent is Item )
					{
						Item parent = (Item)m_Parent;

						parent.TotalWeight = (parent.TotalWeight - m_TotalWeight) + value;
						parent.InvalidateProperties();
					}
					else if ( m_Parent is Mobile && !(this is BankBox) )
					{
						Mobile parent = (Mobile)m_Parent;

						parent.TotalWeight = (parent.TotalWeight - m_TotalWeight) + value;
					}

					m_TotalWeight = value;
					InvalidateProperties();
				}
			}
		}

		[CommandProperty( AccessLevel.Counselor, AccessLevel.GameMaster )]
		public double Weight
		{
			get
			{
				return m_Weight;
			}
			set
			{
				if ( m_Weight != value )
				{
					int oldPileWeight = PileWeight;

					m_Weight = value;

					if ( m_Parent is Item )
					{
						Item parent = (Item)m_Parent;

						parent.TotalWeight = (parent.TotalWeight - oldPileWeight) + PileWeight;
						parent.InvalidateProperties();
					}
					else if ( m_Parent is Mobile && !(this is BankBox) )
					{
						Mobile parent = (Mobile)m_Parent;

						parent.TotalWeight = (parent.TotalWeight - oldPileWeight) + PileWeight;
					}

					InvalidateProperties();
				}
			}
		}

		[CommandProperty( AccessLevel.Counselor, AccessLevel.GameMaster )]
		public int PileWeight
		{
			get
			{
				return (int)Math.Ceiling( m_Weight * m_Amount );
			}
		}

		public virtual int HuedItemID
		{
			get
			{
				return ( m_ItemID & 0x3FFF );
			}
		}

		[Hue, CommandProperty( AccessLevel.GameMaster )]
		public virtual int Hue
		{
			get
			{
				return m_Hue;
			}
			set
			{
				if ( m_Hue != value )
				{
					m_Hue = value;
					Packet.Release( ref m_WorldPacket );

					Delta( ItemDelta.Update );
				}
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public virtual Layer Layer
		{
			get
			{
				return m_Layer;
			}
			set
			{
				if ( m_Layer != value )
				{
					m_Layer = value;

					Delta( ItemDelta.EquipOnly );
				}
			}
		}

		public ArrayList Items
		{
			get
			{
				if (!World.Saving)
					CheckRehydrate();

				if ( m_Items == null )
					return EmptyItems;

				return ArrayList.ReadOnly(m_Items); // if people want to add or delete items they need to use the proper functions
			}
		}

		public object RootParent
		{
			get
			{
				object p = m_Parent;

				while ( p is Item )
				{
					Item item = (Item)p;

					if ( item.m_Parent == null )
					{
						break;
					}
					else
					{
						p = item.m_Parent;
					}
				}

				return p;
			}
		}

		public virtual void AddItem( Item item ) // FreezeDried needs modifications
		{
			if ( item == null || item.Deleted || item.m_Parent == this )
			{
				return;
			}
			if ( item == this )
			{
				Console.WriteLine( "Warning: Adding item to itself: [0x{0:X} {1}].AddItem( [0x{2:X} {3}] )", this.Serial.Value, this.GetType().Name, item.Serial.Value, item.GetType().Name );
				Console.WriteLine( new System.Diagnostics.StackTrace() );
				return;
			}
			if ( IsChildOf( item ) )
			{
				Console.WriteLine( "Warning: Adding parent item to child: [0x{0:X} {1}].AddItem( [0x{2:X} {3}] )", this.Serial.Value, this.GetType().Name, item.Serial.Value, item.GetType().Name );
				Console.WriteLine( new System.Diagnostics.StackTrace() );
				return;
			}
			
			CheckRehydrate();

			if ( item.m_Parent is Mobile )
			{
				((Mobile)item.m_Parent).RemoveItem( item );
			}
			else if ( item.m_Parent is Item )
			{
				((Item)item.m_Parent).RemoveItem( item );
			}
			else
			{
				item.SendRemovePacket();
			}

			item.Parent = this;
			item.Map = m_Map;

			if ( m_Items == null )
				m_Items = new ArrayList( 4 );

			int oldCount = m_Items.Count;
			m_Items.Add( item );

			TotalItems = (TotalItems - oldCount) + m_Items.Count + item.TotalItems - (item.IsVirtualItem ? 1 : 0);
			TotalWeight += item.TotalWeight + item.PileWeight;
			TotalGold += item.TotalGold;

			item.Delta( ItemDelta.Update );

			item.OnAdded( this );
			OnItemAdded( item );
		}

		private static ArrayList m_DeltaQueue = new ArrayList();// Queue m_DeltaQueue = new Queue();

		public void Delta( ItemDelta flags )
		{
			if ( m_Map == null || m_Map == Map.Internal )
				return;

			m_DeltaFlags |= flags;

			if ( !GetFlag( ImplFlag.InQueue ) )
			{
				SetFlag( ImplFlag.InQueue, true );

				m_DeltaQueue.Add( this );
			}

			Core.Set();
		}

		public void RemDelta( ItemDelta flags )
		{
			m_DeltaFlags &= ~flags;

			if ( GetFlag( ImplFlag.InQueue ) && m_DeltaFlags == ItemDelta.None )
			{
				SetFlag( ImplFlag.InQueue, false );

				m_DeltaQueue.Remove( this );
			}
		}

		public void ProcessDelta()
		{
			ItemDelta flags = m_DeltaFlags;

			SetFlag( ImplFlag.InQueue, false );
			m_DeltaFlags = ItemDelta.None;

			Map map = m_Map;

			if ( map != null && !Deleted )
			{
				bool sendOPLUpdate = ObjectPropertyList.Enabled && (flags & ItemDelta.Properties) != 0;

				Container contParent = m_Parent as Container;

				if ( contParent != null && !contParent.IsPublicContainer )
				{
					if ( (flags & ItemDelta.Update) != 0 )
					{
						Point3D worldLoc = GetWorldLocation();

						Mobile rootParent = contParent.RootParent as Mobile;
						Mobile tradeRecip = null;

						if ( rootParent != null )
						{
							NetState ns = rootParent.NetState;

							if ( ns != null )
							{
								if ( rootParent.CanSee( this ) && rootParent.InRange( worldLoc, GetUpdateRange( rootParent ) ) )
								{
									if ( ns.IsPost6017 )
										ns.Send( new ContainerContentUpdate6017( this ) );
									else
										ns.Send( new ContainerContentUpdate( this ) );

									if ( ObjectPropertyList.Enabled )
										ns.Send( OPLPacket );
								}
							}
						}

						SecureTradeContainer stc = this.GetSecureTradeCont();

						if ( stc != null )
						{
							SecureTrade st = stc.Trade;

							if ( st != null )
							{
								Mobile test = st.From.Mobile;

								if ( test != null && test != rootParent )
									tradeRecip = test;

								test = st.To.Mobile;

								if ( test != null && test != rootParent )
									tradeRecip = test;

								if ( tradeRecip != null )
								{
									NetState ns = tradeRecip.NetState;

									if ( ns != null )
									{
                                        if (tradeRecip.CanSee(this) && tradeRecip.InRange(worldLoc, GetUpdateRange(tradeRecip)))
                                        {
                                            if (ns.IsPost6017)
                                                ns.Send(new ContainerContentUpdate6017(this));
                                            else
                                                ns.Send(new ContainerContentUpdate(this));

                                            if (ObjectPropertyList.Enabled)
                                                ns.Send(OPLPacket);
                                        }
									}
								}
							}
						}

						ArrayList openers = contParent.Openers;

						if ( openers != null )
						{
							for ( int i = 0; i < openers.Count; ++i )
							{
								Mobile mob = (Mobile)openers[i];

								int range = GetUpdateRange( mob );

								if ( mob.Map != map || !mob.InRange( worldLoc, range ) )
								{
									openers.RemoveAt( i-- );
								}
								else
								{
									if ( mob == rootParent || mob == tradeRecip )
										continue;

									NetState ns = mob.NetState;

									if ( ns != null )
									{
                                        if (mob.CanSee(this))
                                        {
                                            if (ns.IsPost6017)
                                                ns.Send(new ContainerContentUpdate6017(this));
                                            else
                                                ns.Send(new ContainerContentUpdate(this));

                                            if (ObjectPropertyList.Enabled)
                                                ns.Send(OPLPacket);
                                        }
									}
								}
							}

							if ( openers.Count == 0 )
								contParent.Openers = null;
						}
						return;
					}
				}

				if ( (flags & ItemDelta.Update) != 0 )
				{
					Packet p = null;
					Point3D worldLoc = GetWorldLocation();

					IPooledEnumerable eable = map.GetClientsInRange( worldLoc, GetMaxUpdateRange() );

					foreach ( NetState state in eable ) {
						Mobile m = state.Mobile;

						if ( m.CanSee( this ) && m.InRange( worldLoc, GetUpdateRange( m ) ) ) {
							if ( m_Parent == null ) {
								SendInfoTo( state, ObjectPropertyList.Enabled );
							} else {
								if ( p == null ) {
									if ( m_Parent is Item ) {
                                        if (state.IsPost6017)
											state.Send( new ContainerContentUpdate6017( this ) );
                                        else
											state.Send( new ContainerContentUpdate( this ) );
									} else if ( m_Parent is Mobile ) {
                                        p = new EquipUpdate(this);
										p.Acquire();
										state.Send( p );
								}
								} else {
								state.Send( p );
								}

								if ( ObjectPropertyList.Enabled ) {
									state.Send( OPLPacket );
							}
						}
					}
					}

					if ( p != null )
						Packet.Release( p );

					eable.Free();
					sendOPLUpdate = false;
				}
				else if ( (flags & ItemDelta.EquipOnly ) != 0 )
				{
					if ( m_Parent is Mobile )
					{
						Packet p = null;
						Point3D worldLoc = GetWorldLocation();

						IPooledEnumerable eable = map.GetClientsInRange( worldLoc, GetMaxUpdateRange() );

						foreach ( NetState state in eable )
						{
							Mobile m = state.Mobile;

							if ( m.CanSee( this ) && m.InRange( worldLoc, GetUpdateRange( m ) ) )
							{
								//if ( sendOPLUpdate )
								//	state.Send( RemovePacket );

								if ( p == null )
									p = Packet.Acquire( new EquipUpdate( this ) );

								state.Send( p );

								if ( ObjectPropertyList.Enabled )
									state.Send( OPLPacket );
							}
						}

						Packet.Release( p );

						eable.Free();
						sendOPLUpdate = false;
					}
				}

				if ( sendOPLUpdate )
				{
					Point3D worldLoc = GetWorldLocation();
					IPooledEnumerable eable = map.GetClientsInRange( worldLoc, GetMaxUpdateRange() );

					foreach ( NetState state in eable )
					{
						Mobile m = state.Mobile;

						if ( m.CanSee( this ) && m.InRange( worldLoc, GetUpdateRange( m ) ) )
							state.Send( OPLPacket );
					}

					eable.Free();
				}
			}
		}

		public static void ProcessDeltaQueue()
		{
			int count = m_DeltaQueue.Count;

			for ( int i = 0; i < m_DeltaQueue.Count; ++i )
			{
				((Item)m_DeltaQueue[i]).ProcessDelta();

				if ( i >= count )
					break;
			}

			if ( m_DeltaQueue.Count > 0 )
				m_DeltaQueue.Clear();
		}

		public virtual void OnDelete()
		{
		}

		public virtual void OnParentDeleted( object parent )
		{
			this.Delete();
		}

		public virtual void FreeCache()
		{
			Packet.Release( ref m_RemovePacket );
			Packet.Release( ref m_WorldPacket );
			Packet.Release( ref m_OPLPacket );
			Packet.Release( ref m_PropertyList );
		}

		public virtual void Delete() // needs FreezeDried modifications
		{
			if ( Deleted )
				return;
			
			CheckRehydrate();

			if ( !World.OnDelete( this ) )
				return;

			OnDelete();

			if ( m_Items != null )
			{
				for ( int i = m_Items.Count - 1; i >= 0; --i )
				{
					if ( i < m_Items.Count )
						((Item)m_Items[i]).OnParentDeleted( this );
				}
			}

			SendRemovePacket();

			SetFlag( ImplFlag.Deleted, true );

			if ( Parent is Mobile )
				((Mobile)Parent).RemoveItem( this );
			else if ( Parent is Item )
				((Item)Parent).RemoveItem( this );

			ClearBounce();

			Map wasMap = m_Map;
			object wasParent = Parent;
			Point3D wasLocation = Location;

			if ( m_Map != null )
			{
				m_Map.OnLeave( this );
				m_Map = null;
			}

			World.RemoveItem( this );

			OnAfterDelete();

			if (wasMap != null && wasMap != Map.Internal && wasParent == null)
				wasMap.FixColumn(wasLocation.X, wasLocation.Y);

			m_RemovePacket = null;
			m_WorldPacket = null;
			m_OPLPacket = null;
			m_PropertyList = null;
		}

		public void PublicOverheadMessage( MessageType type, int hue, bool ascii, string text )
		{
			if ( m_Map != null )
			{
				Packet p = null;
				Point3D worldLoc = GetWorldLocation();

				IPooledEnumerable eable = m_Map.GetClientsInRange( worldLoc, GetMaxUpdateRange() );

				foreach ( NetState state in eable )
				{
					Mobile m = state.Mobile;

					if ( m.CanSee( this ) && m.InRange( worldLoc, GetUpdateRange( m ) ) )
					{
						if ( p == null )
						{
							if ( ascii )
								p = new AsciiMessage( m_Serial, m_ItemID, type, hue, 3, m_Name, text );
							else
								p = new UnicodeMessage( m_Serial, m_ItemID, type, hue, 3, "ENU", m_Name, text );

							p.Acquire();
						}

						state.Send( p );
					}
				}

				Packet.Release(p);

				eable.Free();
			}
		}

		public void PublicOverheadMessage( MessageType type, int hue, int number )
		{
			PublicOverheadMessage( type, hue, number, "" );
		}

		public void PublicOverheadMessage( MessageType type, int hue, int number, string args )
		{
			if ( m_Map != null )
			{
				Packet p = null;
				Point3D worldLoc = GetWorldLocation();

				IPooledEnumerable eable = m_Map.GetClientsInRange( worldLoc, GetMaxUpdateRange() );

				foreach ( NetState state in eable )
				{
					Mobile m = state.Mobile;

					if ( m.CanSee( this ) && m.InRange( worldLoc, GetUpdateRange( m ) ) )
					{
						if ( p == null )
							p = Packet.Acquire( new MessageLocalized( m_Serial, m_ItemID, type, hue, 3, number, this.Name, args ) );

						state.Send( p );
					}
				}

				Packet.Release( p );

				eable.Free();
			}
		}

		public virtual void OnAfterDelete()
		{
		}

		public virtual void RemoveItem( Item item ) // needs FreezeDried modifications
		{
			CheckRehydrate();

			if ( m_Items != null && m_Items.Contains( item ) )
			{
				item.SendRemovePacket();

				int oldCount = m_Items.Count;

				m_Items.Remove( item );

				TotalItems = (TotalItems - oldCount) + m_Items.Count - item.TotalItems + (item.IsVirtualItem ? 1 : 0);
				TotalWeight -= item.TotalWeight + item.PileWeight;
				TotalGold -= item.TotalGold;

				item.Parent = null;

				item.OnRemoved( this );
				OnItemRemoved( item );
			}
		}

		public virtual Item Dupe( Item item, int amount )
		{
			item.Visible = Visible;
			item.Movable = Movable;
			item.LootType = LootType;
			item.Direction = Direction;
			item.Hue = Hue;
			item.ItemID = ItemID;
			item.Location = Location;
			item.Layer = Layer;
			item.Name = Name;
			item.Weight = Weight;
			item.Amount = amount;
			item.Map = Map;

			if ( Parent is Mobile )
			{
				((Mobile)Parent).AddItem( item );
			}
			else if ( Parent is Item )
			{
				((Item)Parent).AddItem( item );
			}

			item.Delta( ItemDelta.Update );

			return item;
		}

		public virtual bool OnDragLift( Mobile from )
		{
			return true;
		}

		public virtual bool OnEquip( Mobile from )
		{
			return true;
		}

		public virtual void OnBeforeSpawn( Point3D location, Map m )
		{
		}

		public virtual void OnAfterSpawn()
		{
		}

		public virtual Item Dupe( int amount )
		{
			return Dupe( new Item(), amount );
		}
		/*
				public virtual int PhysicalResistance{ get{ return 0; } }
				public virtual int FireResistance{ get{ return 0; } }
				public virtual int ColdResistance{ get{ return 0; } }
				public virtual int PoisonResistance{ get{ return 0; } }
				public virtual int EnergyResistance{ get{ return 0; } }
		*/
		[CommandProperty( AccessLevel.Counselor )]
		public Serial Serial
		{
			get
			{
				return m_Serial;
			}
		}

		public virtual void OnLocationChange( Point3D oldLocation )
		{
		}

		//PIX: This was put in so that the contents of a corpse (which is the
		// ONLY place this should be called) is refreshed for newer (> 6.0.something)
		// clients - if this isn't done, then people coming in range of a corpse (other
		// than when it's freshly created) won't see what's equipped on the corpse.
		public void DoSpecialContainerUpdateForEquippingCorpses()
		{
			if (m_Map != null)
			{
				if (m_Parent is Item)
				{
					Packet.Release(ref m_WorldPacket);

					Delta(ItemDelta.Update);
				}
			}
		}

		[CommandProperty( AccessLevel.Counselor, AccessLevel.GameMaster )]
		public virtual Point3D Location
		{
			get
			{
				return m_Location;
			}
			set
			{
				Point3D oldLocation = m_Location;

				if ( oldLocation != value )
				{
					if ( m_Map != null )
					{
						if ( m_Parent == null )
						{
							IPooledEnumerable eable;

							if ( m_Location.m_X != 0 )
							{
								Packet removeThis = null;

								eable = m_Map.GetClientsInRange( oldLocation, GetMaxUpdateRange() );

								foreach ( NetState state in eable )
								{
									Mobile m = state.Mobile;

									if ( !m.InRange( value, GetUpdateRange( m ) ) )
									{
										if ( removeThis == null )
											removeThis = this.RemovePacket;

										state.Send( removeThis );
									}
								}

								eable.Free();
							}

							m_Location = value;
							Packet.Release( ref m_WorldPacket );

							SetLastMoved();

							eable = m_Map.GetClientsInRange( m_Location, GetMaxUpdateRange() );

							foreach ( NetState state in eable )
							{
								Mobile m = state.Mobile;

								if ( m.CanSee( this ) && m.InRange( m_Location, GetUpdateRange( m ) ) )
									SendInfoTo( state );
							}

							eable.Free();

							RemDelta( ItemDelta.Update );
						}
						else if ( m_Parent is Item )
						{
							m_Location = value;
							Packet.Release( ref m_WorldPacket );

							Delta( ItemDelta.Update );
						}
						else
						{
							m_Location = value;
							Packet.Release( ref m_WorldPacket );
						}

						if ( m_Parent == null )
							m_Map.OnMove( oldLocation, this );
					}
					else
					{
						m_Location = value;
						Packet.Release( ref m_WorldPacket );
					}

					this.OnLocationChange( oldLocation );
				}
			}
		}

		[CommandProperty( AccessLevel.Counselor, AccessLevel.GameMaster )]
		public virtual int X
		{
			get{ return m_Location.m_X; }
			set{ Location = new Point3D( value, m_Location.m_Y, m_Location.m_Z ); }
		}

		[CommandProperty( AccessLevel.Counselor, AccessLevel.GameMaster )]
		public virtual int Y
		{
			get{ return m_Location.m_Y; }
			set{ Location = new Point3D( m_Location.m_X, value, m_Location.m_Z ); }
		}

		[CommandProperty( AccessLevel.Counselor, AccessLevel.GameMaster )]
		public virtual int Z
		{
			get{ return m_Location.m_Z; }
			set{ Location = new Point3D( m_Location.m_X, m_Location.m_Y, value ); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public virtual int ItemID
		{
			get
			{
				return m_ItemID;
			}
			set
			{
				if ( m_ItemID != value )
				{
					m_ItemID = value;
					Packet.Release( ref m_WorldPacket );

					InvalidateProperties();
					Delta( ItemDelta.Update );
				}
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public virtual string Name
		{
			get
			{
				return m_Name;
			}
			set
			{
				m_Name = value;
				InvalidateProperties();
			}
		}

		public virtual object Parent
		{
			get
			{
				return m_Parent;
			}
			set
			{
				if ( m_Parent == value )
					return;

				object oldParent = m_Parent;

				m_Parent = value;

				if ( m_Map != null )
				{
					if ( oldParent != null && m_Parent == null )
						m_Map.OnEnter( this );
					else if ( m_Parent != null )
						m_Map.OnLeave( this );
				}
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public LightType Light
		{
			get
			{
				return (LightType)m_Direction;
			}
			set
			{
				if ( (LightType)m_Direction != value )
				{
					m_Direction = (Direction)value;
					Packet.Release( ref m_WorldPacket );

					Delta( ItemDelta.Update );
				}
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public Direction Direction
		{
			get
			{
				return m_Direction;
			}
			set
			{
				if ( m_Direction != value )
				{
					m_Direction = value;
					Packet.Release( ref m_WorldPacket );

					Delta( ItemDelta.Update );
				}
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int Amount
		{
			get
			{
				return m_Amount;
			}
			set
			{
				int oldValue = m_Amount;

				if ( oldValue != value )
				{
					int oldPileWeight = PileWeight;

					m_Amount = value;
					Packet.Release( ref m_WorldPacket );

					if ( m_Parent is Item )
					{
						Item parent = (Item)m_Parent;

						parent.TotalWeight = (parent.TotalWeight - oldPileWeight) + PileWeight;
					}
					else if ( m_Parent is Mobile && !(this is BankBox) )
					{
						Mobile parent = (Mobile)m_Parent;

						parent.TotalWeight = (parent.TotalWeight - oldPileWeight) + PileWeight;
					}

					OnAmountChange( oldValue );

					Delta( ItemDelta.Update );

					if ( oldValue > 1 || value > 1 )
						InvalidateProperties();

					if ( !Stackable && m_Amount > 1 )
						Console.WriteLine( "Warning: 0x{0:X}: Amount changed for non-stackable item '{2}'. ({1})", m_Serial.Value, m_Amount, GetType().Name );
				}
			}
		}

		protected virtual void OnAmountChange( int oldValue )
		{
		}

		public virtual bool HandlesOnSpeech{ get{ return false; } }

		public virtual void OnSpeech( SpeechEventArgs e )
		{
		}

		public virtual bool OnDroppedToMobile( Mobile from, Mobile target )
		{
			return true;
		}

		public virtual bool DropToMobile( Mobile from, Mobile target, Point3D p )
		{
			if ( Deleted || from.Deleted || target.Deleted || from.Map != target.Map || from.Map == null || target.Map == null )
				return false;
			else if ( from.AccessLevel < AccessLevel.GameMaster && !from.InRange( target.Location, 2 ) )
				return false;
			else if ( !from.CanSee( target ) || !from.InLOS( target ) )
				return false;
			else if ( !from.OnDroppedItemToMobile( this, target ) )
				return false;
			else if ( !OnDroppedToMobile( from, target ) )
				return false;
			else if ( !target.OnDragDrop( from, this ) )
				return false;
			else
				return true;
		}

		public virtual bool OnDroppedInto( Mobile from, Container target, Point3D p )
		{
			if ( !from.OnDroppedItemInto( this, target, p ) )
				return false;

			return target.OnDragDropInto( from, this, p );
		}

		public virtual bool OnDroppedOnto( Mobile from, Item target )
		{
			if ( Deleted || from.Deleted || target.Deleted || from.Map != target.Map || from.Map == null || target.Map == null )
				return false;
			else if ( from.AccessLevel < AccessLevel.GameMaster && !from.InRange( target.GetWorldLocation(), 2 ) )
				return false;
			else if ( !from.CanSee( target ) || !from.InLOS( target ) )
				return false;
			else if ( !target.IsAccessibleTo( from ) )
				return false;
			else if ( !from.OnDroppedItemOnto( this, target ) )
				return false;
			else
				return target.OnDragDrop( from, this );
		}

		public virtual bool DropToItem( Mobile from, Item target, Point3D p )
		{
			if ( Deleted || from.Deleted || target.Deleted || from.Map != target.Map || from.Map == null || target.Map == null )
				return false;

			object root = target.RootParent;

			if ( from.AccessLevel < AccessLevel.GameMaster && !from.InRange( target.GetWorldLocation(), 2 ) )
				return false;
			else if ( !from.CanSee( target ) || !from.InLOS( target ) )
				return false;
			else if ( !target.IsAccessibleTo( from ) )
				return false;
			else if ( root is Mobile && !((Mobile)root).CheckNonlocalDrop( from, this, target ) )
				return false;
			else if ( !from.OnDroppedItemToItem( this, target, p ) )
				return false;
			else if ( target is Container && p.m_X != -1 && p.m_Y != -1 )
				return OnDroppedInto( from, (Container)target, p );
			else
				return OnDroppedOnto( from, target );
		}

		public virtual bool OnDroppedToWorld( Mobile from, Point3D p )
		{
			return true;
		}

		public virtual int GetLiftSound( Mobile from )
		{
			return 0x57;
		}

		private static int m_OpenSlots;

		public virtual bool DropToWorld( Mobile from, Point3D p )
		{
			if ( Deleted || from.Deleted || from.Map == null )
				return false;
			else if ( !from.InRange( p, 2 ) )
				return false;

			Map map = from.Map;

			if ( map == null )
				return false;

			int x = p.m_X, y = p.m_Y;
			int z = int.MinValue;

			int maxZ = from.Z + 16;

			Tile landTile = map.Tiles.GetLandTile( x, y );
			TileFlag landFlags = TileData.LandTable[landTile.ID & 0x3FFF].Flags;

			int landZ = 0, landAvg = 0, landTop = 0;
			map.GetAverageZ( x, y, ref landZ, ref landAvg, ref landTop );

			if ( !landTile.Ignored && (landFlags & TileFlag.Impassable) == 0 )
			{
				if ( landAvg <= maxZ )
					z = landAvg;
			}

			Tile[] tiles = map.Tiles.GetStaticTiles( x, y, true );

			for ( int i = 0; i < tiles.Length; ++i )
			{
				Tile tile = tiles[i];
				ItemData id = TileData.ItemTable[tile.ID & 0x3FFF];

				if ( !id.Surface )
					continue;

				int top = tile.Z + id.CalcHeight;

				if ( top > maxZ || top < z )
					continue;

				z = top;
			}

			ArrayList items = new ArrayList();

			IPooledEnumerable eable = map.GetItemsInRange( p, 0 );

			foreach ( Item item in eable )
			{
				if ( item.ItemID >= 0x4000 )
					continue;

				items.Add( item );

				ItemData id = item.ItemData;

				if ( !id.Surface )
					continue;

				int top = item.Z + id.CalcHeight;

				if ( top > maxZ || top < z )
					continue;

				z = top;
			}

			eable.Free();

			if ( z == int.MinValue )
				return false;

			if ( z > maxZ )
				return false;

			m_OpenSlots = (1<<20)-1;

			int surfaceZ = z;

			for ( int i = 0; i < tiles.Length; ++i )
			{
				Tile tile = tiles[i];
				ItemData id = TileData.ItemTable[tile.ID & 0x3FFF];

				int checkZ = tile.Z;
				int checkTop = checkZ + id.CalcHeight;

				if ( checkTop == checkZ && !id.Surface )
					++checkTop;

				int zStart = checkZ - z;
				int zEnd = checkTop - z;

				if ( zStart >= 20 || zEnd < 0 )
					continue;

				if ( zStart < 0 )
					zStart = 0;

				if ( zEnd > 19 )
					zEnd = 19;

				int bitCount = zEnd-zStart;

				m_OpenSlots &= ~(((1<<bitCount)-1)<<zStart);
			}

			for ( int i = 0; i < items.Count; ++i )
			{
				Item item = (Item)items[i];
				ItemData id = item.ItemData;

				int checkZ = item.Z;
				int checkTop = checkZ + id.CalcHeight;

				if ( checkTop == checkZ && !id.Surface )
					++checkTop;

				int zStart = checkZ - z;
				int zEnd = checkTop - z;

				if ( zStart >= 20 || zEnd < 0 )
					continue;

				if ( zStart < 0 )
					zStart = 0;

				if ( zEnd > 19 )
					zEnd = 19;

				int bitCount = zEnd-zStart;

				m_OpenSlots &= ~(((1<<bitCount)-1)<<zStart);
			}

			int height = ItemData.Height;

			if ( height == 0 )
				++height;

			if ( height > 30 )
				height = 30;

			int match = (1<<height)-1;
			bool okay = false;

			for ( int i = 0; i < 20; ++i )
			{
				if ( (i+height) > 20 )
					match >>= 1;

				okay = ((m_OpenSlots>>i)&match) == match;

				if ( okay )
				{
					z += i;
					break;
				}
			}

			if ( !okay )
				return false;

			height = ItemData.Height;

			if ( height == 0 )
				++height;

			if ( landAvg > z && (z + height) > landZ )
				return false;
			else if ( (landFlags & TileFlag.Impassable) != 0 && landAvg > surfaceZ && (z + height) > landZ )
				return false;

			for ( int i = 0; i < tiles.Length; ++i )
			{
				Tile tile = tiles[i];
				ItemData id = TileData.ItemTable[tile.ID & 0x3FFF];

				int checkZ = tile.Z;
				int checkTop = checkZ + id.CalcHeight;

				if ( checkTop > z && (z + height) > checkZ )
					return false;
				else if ( (id.Surface || id.Impassable) && checkTop > surfaceZ && (z + height) > checkZ )
					return false;
			}

			for ( int i = 0; i < items.Count; ++i )
			{
				Item item = (Item)items[i];
				ItemData id = item.ItemData;

				int checkZ = item.Z;
				int checkTop = checkZ + id.CalcHeight;

				if ( (item.Z + id.CalcHeight) > z && (z + height) > item.Z )
					return false;
			}

			p = new Point3D( x, y, z );

			if ( !from.InLOS( new Point3D( x, y, z + 1 ) ) )
				return false;
			else if ( !from.OnDroppedItemToWorld( this, p ) )
				return false;
			else if ( !OnDroppedToWorld( from, p ) )
				return false;

			int soundID = GetDropSound();

			MoveToWorld( p, from.Map );

			from.SendSound( soundID == -1 ? 0x42 : soundID, GetWorldLocation() );

			return true;
		}

		public void SendRemovePacket()
		{
			if ( !Deleted && m_Map != null )
			{
				Packet p = null;
				Point3D worldLoc = GetWorldLocation();

				IPooledEnumerable eable = m_Map.GetClientsInRange( worldLoc, GetMaxUpdateRange() );

				foreach ( NetState state in eable )
				{
					Mobile m = state.Mobile;

					if ( m.InRange( worldLoc, GetUpdateRange( m ) ) )
					{
						if ( p == null )
							p = this.RemovePacket;

						state.Send( p );
					}
				}

				eable.Free();
			}
		}

		public virtual int GetDropSound()
		{
			return -1;
		}

		public Point3D GetWorldLocation()
		{
			object root = RootParent;

			if ( root == null )
				return m_Location;
			else
				return ((IEntity)root).Location;

			//return root == null ? m_Location : new Point3D( (IPoint3D) root );
		}

		public virtual bool BlocksFit{ get{ return false; } }

		public Point3D GetSurfaceTop()
		{
			object root = RootParent;

			if ( root == null )
				return new Point3D( m_Location.m_X, m_Location.m_Y, m_Location.m_Z + (ItemData.Surface ? ItemData.CalcHeight : 0) );
			else
				return ((IEntity)root).Location;
		}

		public Point3D GetWorldTop()
		{
			object root = RootParent;

			if ( root == null )
				return new Point3D( m_Location.m_X, m_Location.m_Y, m_Location.m_Z + ItemData.CalcHeight );
			else
				return ((IEntity)root).Location;
		}

		public void SendLocalizedMessageTo( Mobile to, int number )
		{
			if ( Deleted || !to.CanSee( this ) )
				return;

			to.Send( new MessageLocalized( Serial, ItemID, MessageType.Regular, 0x3B2, 3, number, "", "" ) );
		}

		public void SendLocalizedMessageTo( Mobile to, int number, string args )
		{
			if ( Deleted || !to.CanSee( this ) )
				return;

			to.Send( new MessageLocalized( Serial, ItemID, MessageType.Regular, 0x3B2, 3, number, "", args ) );
		}

		public void SendLocalizedMessageTo( Mobile to, int number, AffixType affixType, string affix, string args )
		{
			if ( Deleted || !to.CanSee( this ) )
				return;

			to.Send( new MessageLocalizedAffix( Serial, ItemID, MessageType.Regular, 0x3B2, 3, number, "", affixType, affix, args ) );
		}

		public virtual void OnDoubleClick( Mobile from )
		{
		}

		public virtual void OnDoubleClickOutOfRange( Mobile from )
		{
		}

		public virtual void OnDoubleClickCantSee( Mobile from )
		{
		}

		public virtual void OnDoubleClickDead( Mobile from )
		{
			from.LocalOverheadMessage( MessageType.Regular, 0x3B2, 1019048 ); // I am dead and cannot do that.
		}

		public virtual void OnDoubleClickNotAccessible( Mobile from )
		{
			from.SendLocalizedMessage( 500447 ); // That is not accessible.
		}

		public virtual void OnDoubleClickSecureTrade( Mobile from )
		{
			from.SendLocalizedMessage( 500447 ); // That is not accessible.
		}

		public virtual void OnSnoop( Mobile from )
		{
		}

		public bool InSecureTrade
		{
			get
			{
				return ( GetSecureTradeCont() != null );
			}
		}

		public SecureTradeContainer GetSecureTradeCont()
		{
			object p = this;

			while ( p is Item )
			{
				if ( p is SecureTradeContainer )
					return (SecureTradeContainer)p;

				p = ((Item)p).m_Parent;
			}

			return null;
		}

		public virtual void OnItemAdded( Item item )
		{
			if ( m_Parent is Item )
				((Item)m_Parent).OnSubItemAdded( item );
			else if ( m_Parent is Mobile )
				((Mobile)m_Parent).OnSubItemAdded( item );
		}

		public virtual void OnItemRemoved( Item item )
		{
			if ( m_Parent is Item )
				((Item)m_Parent).OnSubItemRemoved( item );
			else if ( m_Parent is Mobile )
				((Mobile)m_Parent).OnSubItemRemoved( item );
		}

		public virtual void OnSubItemAdded( Item item )
		{
			if ( m_Parent is Item )
				((Item)m_Parent).OnSubItemAdded( item );
			else if ( m_Parent is Mobile )
				((Mobile)m_Parent).OnSubItemAdded( item );
		}

		public virtual void OnSubItemRemoved( Item item )
		{
			if ( m_Parent is Item )
				((Item)m_Parent).OnSubItemRemoved( item );
			else if ( m_Parent is Mobile )
				((Mobile)m_Parent).OnSubItemRemoved( item );
		}

		public virtual void OnItemBounceCleared( Item item )
		{
			if ( m_Parent is Item )
				((Item)m_Parent).OnSubItemBounceCleared( item );
			else if ( m_Parent is Mobile )
				((Mobile)m_Parent).OnSubItemBounceCleared( item );
		}

		public virtual void OnSubItemBounceCleared( Item item )
		{
			if ( m_Parent is Item )
				((Item)m_Parent).OnSubItemBounceCleared( item );
			else if ( m_Parent is Mobile )
				((Mobile)m_Parent).OnSubItemBounceCleared( item );
		}

		public virtual bool CheckTarget( Mobile from, Server.Targeting.Target targ, object targeted )
		{
			if ( m_Parent is Item )
				return ((Item)m_Parent).CheckTarget( from, targ, targeted );
			else if ( m_Parent is Mobile )
				return ((Mobile)m_Parent).CheckTarget( from, targ, targeted );

			return true;
		}

		public virtual bool IsAccessibleTo( Mobile check )
		{
			if ( m_Parent is Item )
				return ((Item)m_Parent).IsAccessibleTo( check );

			Region reg = Region.Find( GetWorldLocation(), m_Map );

			return reg.CheckAccessibility( this, check );

			/*SecureTradeContainer cont = GetSecureTradeCont();

			if ( cont != null && !cont.IsChildOf( check ) )
				return false;

			return true;*/
		}

		public bool IsChildOf( object o )
		{
			return IsChildOf( o, false );
		}

		public bool IsChildOf( object o, bool allowNull )
		{
			object p = m_Parent;

			if ( (p == null || o == null) && !allowNull )
				return false;

			if ( p == o )
				return true;

			while ( p is Item )
			{
				Item item = (Item)p;

				if ( item.m_Parent == null )
				{
					break;
				}
				else
				{
					p = item.m_Parent;

					if ( p == o )
						return true;
				}
			}

			return false;
		}

		public ItemData ItemData
		{
			get
			{
				return TileData.ItemTable[m_ItemID & 0x3FFF];
			}
		}

		public virtual void OnItemUsed( Mobile from, Item item )
		{
			if ( m_Parent is Item )
				((Item)m_Parent).OnItemUsed( from, item );
			else if ( m_Parent is Mobile )
				((Mobile)m_Parent).OnItemUsed( from, item );
		}

		public virtual bool CheckItemUse( Mobile from, Item item )
		{
			if ( m_Parent is Item )
				return ((Item)m_Parent).CheckItemUse( from, item );
			else if ( m_Parent is Mobile )
				return ((Mobile)m_Parent).CheckItemUse( from, item );
			else
				return true;
		}

		public virtual void OnItemLifted( Mobile from, Item item )
		{
			if ( m_Parent is Item )
				((Item)m_Parent).OnItemLifted( from, item );
			else if ( m_Parent is Mobile )
				((Mobile)m_Parent).OnItemLifted( from, item );
		}

//		public virtual bool CheckLift( Mobile from, Item item )
//		{
//			if ( m_Parent is Item )
//				return ((Item)m_Parent).CheckLift( from, item );
//			else if ( m_Parent is Mobile )
//				return ((Mobile)m_Parent).CheckLift( from, item );
//			else
//				return true;
//		}

		public bool CheckLift( Mobile from )
		{
			LRReason reject = LRReason.Inspecific;

			return CheckLift( from, this, ref reject );
		}

		public virtual bool CheckLift( Mobile from, Item item, ref LRReason reject )
		{
			if ( m_Parent is Item )
				return ((Item)m_Parent).CheckLift( from, item, ref reject );
			else if ( m_Parent is Mobile )
				return ((Mobile)m_Parent).CheckLift( from, item, ref reject );
			else
				return true;
		}


		public virtual bool CanTarget{ get{ return true; } }
		public virtual bool DisplayLootType{ get{ return true; } }

		public virtual void OnSingleClickContained( Mobile from, Item item )
		{
			if ( m_Parent is Item )
				((Item)m_Parent).OnSingleClickContained( from, item );
		}

		public virtual void OnAosSingleClick( Mobile from )
		{
			ObjectPropertyList opl = this.PropertyList;

			if ( opl.Header > 0 )
				from.Send( new MessageLocalized( m_Serial, m_ItemID, MessageType.Label, 0x3B2, 3, opl.Header, m_Name, opl.HeaderArgs ) );
		}

		public virtual void OnSingleClick( Mobile from )
		{
			if ( Deleted || !from.CanSee( this ) )
				return;

			if ( DisplayLootType )
				LabelLootTypeTo( from );

			NetState ns = from.NetState;

			if ( ns != null )
			{
				if ( m_Name == null )
				{
					if ( m_Amount <= 1 )
						ns.Send( new MessageLocalized( m_Serial, m_ItemID, MessageType.Label, 0x3B2, 3, LabelNumber, "", "" ) );
					else
						ns.Send( new MessageLocalizedAffix( m_Serial, m_ItemID, MessageType.Label, 0x3B2, 3, LabelNumber, "", AffixType.Append, String.Format( " : {0}", m_Amount ), "" ) );
				}
				else
				{
					ns.Send( new UnicodeMessage( m_Serial, m_ItemID, MessageType.Label, 0x3B2, 3, "ENU", "", m_Name + (m_Amount > 1 ? " : " + m_Amount : "") ) );
				}
			}
		}

		private static bool m_ScissorCopyLootType;

		public static bool ScissorCopyLootType
		{
			get{ return m_ScissorCopyLootType; }
			set{ m_ScissorCopyLootType = value; }
		}

		public virtual void ScissorHelper( Mobile from, Item newItem, int amountPerOldItem )
		{
			ScissorHelper( from, newItem, amountPerOldItem, true );
		}

		public virtual void ScissorHelper( Mobile from, Item newItem, int amountPerOldItem, bool carryHue )
		{
			int amount = Amount;

			if ( amount > (60000 / amountPerOldItem) ) // let's not go over 60000
				amount = (60000 / amountPerOldItem);

			Amount -= amount;

			int ourHue = Hue;
			Map thisMap = this.Map;
			object thisParent = this.m_Parent;
			Point3D worldLoc = this.GetWorldLocation();
			LootType type = this.LootType;

			if ( Amount == 0 )
				Delete();

			newItem.Amount = amount * amountPerOldItem;

			if ( carryHue )
				newItem.Hue = ourHue;

			if ( m_ScissorCopyLootType )
				newItem.LootType = type;

			if ( !(thisParent is Container) || !((Container)thisParent).TryDropItem( from, newItem, false ) )
				newItem.MoveToWorld( worldLoc, thisMap );
		}

		public virtual void Consume()
		{
			Consume( 1 );
		}

		public virtual void Consume( int amount )
		{
			this.Amount -= amount;

			if ( this.Amount <= 0 )
				this.Delete();
		}

		private Mobile m_BlessedFor;

		[CommandProperty( AccessLevel.Counselor, AccessLevel.GameMaster )]
		public bool PlayerCrafted
		{
			get{ return GetFlag( ImplFlag.PlayerCrafted ); }
			set{ SetFlag( ImplFlag.PlayerCrafted, value ); InvalidateProperties(); }
		}

		public bool SpawnerTempItem
		{
			get{ return GetFlag( ImplFlag.IsTemplate ); }
			set{ SetFlag( ImplFlag.IsTemplate, value ); InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.Counselor, AccessLevel.GameMaster )]
		public bool IsIntMapStorage
		{
			get{ return GetFlag( ImplFlag.IsIntMapStorage ); }
			set{ SetFlag( ImplFlag.IsIntMapStorage, value ); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool PlayerQuest
		{
			get{ return GetFlag( ImplFlag.PlayerQuest ); }
			set{ SetFlag( ImplFlag.PlayerQuest, value ); }
		}

        [CommandProperty( AccessLevel.GameMaster )]
        public bool HideAttributes
		{
            get { return GetFlag(ImplFlag.HideAttributes); }
            set { SetFlag(ImplFlag.HideAttributes, value); }
		}

		public Mobile BlessedFor
		{
			get{ return m_BlessedFor; }
			set{ m_BlessedFor = value; InvalidateProperties(); }
		}

		public virtual bool CheckBlessed( object obj )
		{
			return CheckBlessed( obj as Mobile );
		}

		public virtual bool CheckBlessed( Mobile m )
		{
			if ( m_LootType == LootType.Blessed /*|| (Mobile.InsuranceEnabled && Insured)*/ )
				return true;

			return ( m != null && m == m_BlessedFor );
		}

		public virtual bool CheckNewbied()
		{
			return ( m_LootType == LootType.Newbied );
		}

		public virtual bool IsStandardLoot()
		{
			//if ( Mobile.InsuranceEnabled && Insured )
			//return false;

			if ( m_BlessedFor != null )
				return false;

			return ( m_LootType == LootType.Regular );
		}

		public override string ToString()
		{
			return String.Format( "0x{0:X} \"{1}\"", m_Serial.Value, GetType().Name );
		}

		internal int m_TypeRef;

		public Item()
		{
			m_Serial = Serial.NewItem;

			//m_Items = new ArrayList( 1 );
			Visible = true;
			Movable = true;
			Amount = 1;
			m_Map = Map.Internal;

			SetLastMoved();
			m_LastAccessed = DateTime.Now;

			World.AddItem( this );

			Type ourType = this.GetType();
			m_TypeRef = World.m_ItemTypes.IndexOf( ourType );

			if ( m_TypeRef == -1 )
				m_TypeRef = World.m_ItemTypes.Add( ourType );
		}

		[Constructable]
		public Item( int itemID ) : this()
		{
			m_ItemID = itemID;
		}

		public Item( Serial serial )
		{
			m_Serial = serial;

			Type ourType = this.GetType();
			m_TypeRef = World.m_ItemTypes.IndexOf( ourType );

			if ( m_TypeRef == -1 )
				m_TypeRef = World.m_ItemTypes.Add( ourType );

			m_LastAccessed = DateTime.Now;
		}

		/// <summary>
		/// Do not call this function. Ask Taran Kain about it.
		/// </summary>
		public Serial ReassignSerial(Serial s)
		{
			CheckRehydrate();

			SendRemovePacket();
			World.RemoveItem(this);
			m_Serial = s;
			World.AddItem(this);
			m_WorldPacket = null;
			Delta(ItemDelta.Update);

			return m_Serial;
		}

		public virtual void OnSectorActivate()
		{
		}

		public virtual void OnSectorDeactivate()
		{
		}

		public bool MoveItemToIntStorage()
		{
			return MoveItemToIntStorage(false);
		}

        public bool MoveItemToIntStorage(bool PreserveLocation)
        {
            if (Map == null)
                return false;

            IsIntMapStorage = true;
			if (PreserveLocation == true)
				MoveToWorld(Location, Map.Internal);
			else
				Internalize();
            return true;
        }

        public bool RetrieveItemFromIntStorage(Point3D p, Map m)
        {
            if (Deleted == true || p == Point3D.Zero)
                return false;

            IsIntMapStorage = false;
            MoveToWorld(p, m);
            return true;
        }

        // Moves the item to the mobiles backpack
        public bool RetrieveItemFromIntStorage(Mobile m)
        {
            if (Deleted == true || m == null || m.Deleted == true || m.Map == Map.Internal)
                return false;

            IsIntMapStorage = false;
            m.AddToBackpack(this);
            return true;
        }

        // Moves the item to whatever container is passed to it, this should ignore item count/weight limit restrictions and simply place the item into the container. 
        public bool RetrieveItemFromIntStorage(Container c)
        {
            if (Deleted == true || c == null || c.Deleted == true || c.Map == Map.Internal)
                return false;

            IsIntMapStorage = false;
            c.DropItem(this);
            return true;
        }

		public ArrayList GetDeepItems()
		{
			// don't need to call CheckRehydrate since we're using the Items property for our logic - already calls
			ArrayList items = new ArrayList(Items);
			foreach (Item i in Items)
			{
				items.AddRange(i.GetDeepItems());
			}

			return items;
        }

        #region FreezeDry
        public bool FreezeDry()
		{
			if (!World.FreezeDryEnabled)
				return false;

			if (!CanFreezeDry)
			{
				Console.WriteLine("Warning: Tried to freeze dry a non-freezable item: {0}", this);
				Console.WriteLine(new System.Diagnostics.StackTrace());
				return false;
			}

			if (GetFlag(ImplFlag.FreezeDried))
			{
				Console.WriteLine("Warning: Tried to freeze dry an already-frozen item: {0}", this);
				Console.WriteLine(new System.Diagnostics.StackTrace());
				return false;
			}

			if (m_SerializedContentsBin != null || m_SerializedContentsIdx != null)
			{
				Console.WriteLine("Warning: Tried to freezedry an item with that already has serialized data: {0}", this);
				Console.WriteLine(new System.Diagnostics.StackTrace());
				return false;
			}

			MemoryStream idxms = new MemoryStream();
			MemoryStream binms = new MemoryStream();
			// it's called BinaryFileWriter, but it accepts any stream
			GenericWriter idx = new BinaryFileWriter(idxms, true);
			GenericWriter bin = new BinaryFileWriter(binms, true);

			int totalweight = TotalWeight, totalitems = TotalItems, totalgold = TotalGold;

			if (m_Items == null)
				idx.Write((int)-1);
			else
			{
				ArrayList items = GetDeepItems();
				idx.Write( (int) items.Count );
				foreach (Item item in items)
				{
					long start = bin.Position;

					idx.Write( item.GetType().FullName ); // <--- DIFFERENT FROM WORLD SAVE FORMAT!
					idx.Write( (int)item.Serial );
					idx.Write( (long) start );

					item.Serialize( bin );

					idx.Write( (int) (bin.Position - start) );
				}

				foreach (Item item in items)
				{
					World.ReserveSerial(item.Serial);
					item.Delete();
				}
			}

			idx.Close();
			bin.Close();

			m_SerializedContentsIdx = idxms.ToArray();
			m_SerializedContentsBin = binms.ToArray();

			SetFlag(ImplFlag.FreezeDried, true);
			TotalWeight = totalweight;
			TotalItems = totalitems;
			TotalGold = totalgold;

			m_Items = null; // frees about 9kb memory
			FreeCache(); // frees even more

			OnFreezeDry();

			if (Debug)
				Console.WriteLine("Freezedried {0}", this);

			return true;
        }

        // call in your OnDelete to see if you are getting Freeze Dried
        public bool IsFreezeDrying
        {
            get { return World.IsReserved(this.Serial); }
        }

		public virtual void OnFreezeDry()
		{
		}

		public virtual void OnRehydrate()
		{
		}

        public virtual void CancelFreezeTimer()
        {
        }

        public virtual bool ScheduleFreeze()
        {
            return false;
        }

        public virtual bool IsFreezeScheduled
        {
            get { return false; }
        }

		public bool Rehydrate()
		{
			if (!CanFreezeDry)
			{
				Console.WriteLine("Warning: Tried to rehydrate dry a non-freezable item: {0}", this);
				Console.WriteLine(new System.Diagnostics.StackTrace());
				return false;
			}
			
			if (!GetFlag(ImplFlag.FreezeDried))
			{
				Console.WriteLine("Warning: Tried to rehydrate a non-freezedried item: {0}", this);
				Console.WriteLine(new System.Diagnostics.StackTrace());
				return false;
			}

			if (m_SerializedContentsBin == null || m_SerializedContentsIdx == null)
			{
				Console.WriteLine("Warning: Tried to rehydrate an item with no serialized data: {0}", this);
				Console.WriteLine(new System.Diagnostics.StackTrace());
				return false;
			}

			if (World.Saving)
			{
				Console.WriteLine("Warning: Attempted to rehydrate item {0} during a world save!", this);
				Console.WriteLine(new System.Diagnostics.StackTrace());
				return false;
			}

			GenericReader bin = new BinaryFileReader(new BinaryReader(new MemoryStream(m_SerializedContentsBin)));
			GenericReader idx = new BinaryFileReader(new BinaryReader(new MemoryStream(m_SerializedContentsIdx)));

			SetFlag(ImplFlag.FreezeDried, false); // set it here, no fatal errors from here on out and AddItem checks it
			TotalItems = 0;
			TotalWeight = 0;
			TotalGold = 0;

			bool faileditem = false;
			StringBuilder errlog = new StringBuilder();

			int count = idx.ReadInt();
			if (count == -1)
				m_Items = null;
			else
			{
				ArrayList items = new ArrayList(count);
				m_Items = new ArrayList(count); // set so it won't double unnecessarily

				Type[] ctortypes = new Type[] { typeof(Serial) };
				object[] ctorargs = new object[1];
				
				for (int i = 0; i < count; i++)
				{
					string type = idx.ReadString();
					Serial serial = (Serial)idx.ReadInt();
					long position = idx.ReadLong();
					int length = idx.ReadInt();

					Type t = ScriptCompiler.FindTypeByFullName(type);
					if (t == null)
					{
						Console.WriteLine("Warning: Tried to load nonexistent type {0} when rehydrating container {1}. Ignoring item.", type, this);
						errlog.AppendFormat("Warning: Tried to load nonexistent type {0} when rehydrating container {1}. Ignoring item.\r\n\r\n", type, this);
						faileditem = true;
						continue;
					}

					ConstructorInfo ctor = t.GetConstructor(ctortypes);
					if (ctor == null)
					{
						Console.WriteLine("Warning: Tried to load type {0} which has no serialization constructor when rehydrating container {1}. Ignoring item.", type, this);
						errlog.AppendFormat("Warning: Tried to load type {0} which has no serialization constructor when rehydrating container {1}. Ignoring item.\r\n\r\n", type, this);
						faileditem = true;
						continue;
					}

					Item item = null;
					try
					{
                        if (World.FindItem(serial) != null)
                        {
                            Console.WriteLine("Warning: Serial number being rehydrated already exists in world, patching: {0}", serial);

                            if (World.IsReserved(serial) == true)   // free the old one
                                World.FreeSerial(serial);

                            serial = Serial.NewItem;                // create a new one

                            Console.WriteLine("Warning: Serial in use, issuing a new one: {0}", serial);
                            World.ReserveSerial(serial);            // reserve a new one

                            // throw new Exception(String.Format("Serial number being rehydrated already exists in world: {0}", serial));
                        }
                        else if (!World.IsReserved(serial))
                        {
                            Console.WriteLine("Warning: Serial number being rehydrated is not reserved, patching: {0}", serial);
                            Console.WriteLine("Warning: Serial Not being used, reusing: {0}", serial);
                            
                            // reserve it now
                            World.ReserveSerial(serial);
                            
                            // throw new Exception(String.Format("Serial number being rehydrated is not reserved (shouldn't be FD'ed!): {0}", serial));
                        }
						ctorargs[0] = serial;
						item = (Item)(ctor.Invoke(ctorargs));
					}
					catch (Exception e)
					{
						Console.WriteLine("An exception occurred while trying to invoke {0}'s serialization constructor.", t.FullName);
						Console.WriteLine(e.ToString());
						errlog.AppendFormat("An exception occurred while trying to invoke {0}'s serialization constructor.\r\n", t.FullName);
						errlog.Append(e.ToString());
						errlog.AppendFormat("\r\n\r\n");
						faileditem = true;
					}

					if (item != null)
					{
						World.FreeSerial(serial);

						World.AddItem(item);
						items.Add(new object[]{item, position, length});
					}
				}

				for (int i = 0; i < items.Count; i++)
				{
					object[] entry = (object[])items[i];
					Item item = entry[0] as Item;
					long position = (long)entry[1];
					int length = (int)entry[2];

					if (item != null)
					{
						bin.Seek(position, SeekOrigin.Begin);

						try
						{
							item.Deserialize(bin);
							item.Map = Map;

							// items will set their parent automatically, and containers will load their contents
							// however items in the first level will load their parent (this), but this won't add them automatically
							if (item.Parent == this) 
							{
								item.Parent = null;
								AddItem(item);
							}

							item.ClearProperties();

							if (bin.Position != (position + length))
								throw new Exception(String.Format("Bad serialize on {0}", item.GetType().FullName));
						}
						catch (Exception e)
						{
							Console.WriteLine("Caught exception while deserializing {0} for container {1}:", item.GetType().FullName, this);
							Console.WriteLine(e.ToString());
							Console.WriteLine("Deleting item.");
							item.Delete();

							errlog.AppendFormat("Caught exception while deserializing {0} for container {1}:\r\n", item.GetType().FullName, this);
							errlog.Append(e.ToString());
							errlog.Append("\r\nDeleting item.\r\n\r\n");
							faileditem = true;
						}
					}
				}
			}

			idx.Close();
			bin.Close();

			if (faileditem)
			{
				try
				{
					string failedpath = "Logs/FailedRehydrate/";
					if (!Directory.Exists(failedpath))
						Directory.CreateDirectory(failedpath);

					using (FileStream fs = new FileStream(Path.Combine(failedpath, String.Format("{0} {1}.idx", Serial, DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss"))), FileMode.Create, FileAccess.Write))
					{
						fs.Write(m_SerializedContentsIdx, 0, m_SerializedContentsIdx.Length);
						fs.Close();
					}
					using (FileStream fs = new FileStream(Path.Combine(failedpath, String.Format("{0} {1}.bin", Serial, DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss"))), FileMode.Create, FileAccess.Write))
					{
						fs.Write(m_SerializedContentsBin, 0, m_SerializedContentsBin.Length);
						fs.Close();
					}
					using (StreamWriter sw = new StreamWriter(Path.Combine(failedpath, String.Format("{0} {1}.log", Serial, DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss")))))
					{
						sw.WriteLine("Error log for container {0}", this);
						if (this.Parent is Mobile)
							sw.WriteLine("Parent is Mobile: {0}", Parent);
						else
							sw.WriteLine("Location: {0}", Location);
						sw.WriteLine();
						sw.Write(errlog.ToString());
						sw.Close();
					}
				}
				catch (Exception e)
				{
					Console.WriteLine("Failed to dump data for failed rehydration.");
					Console.WriteLine("Exception: {0}", e.Message);
				}
			}
			
			m_SerializedContentsIdx = null;
			m_SerializedContentsBin = null;

			if (RootParent is Mobile)
				((Mobile)RootParent).UpdateTotals();
			else if (RootParent is Item)
				((Item)RootParent).UpdateTotals();
			else
				UpdateTotals();

			OnRehydrate();

			if (Debug)
				Console.WriteLine("Rehydrated {0}", this);

			return true;
		}

		public void CheckRehydrate()
		{
			m_LastAccessed = DateTime.Now;
			if (GetFlag(ImplFlag.FreezeDried))
				Rehydrate();
		}

		public virtual bool CanFreezeDry
		{
			get
			{
				return false;
			}
		}

		public virtual bool CheckFreezeDry()
		{
			return false;
		}
    }
    #endregion

    public class EmptyArrayList : ArrayList
	{
		public override bool IsReadOnly{ get{ return true; } }
		public override bool IsFixedSize{ get{ return true; } }

		private void OnPopulate()
		{
			Console.WriteLine( "Warning: Attempted to populate a static empty ArrayList" );
			Console.WriteLine( new System.Diagnostics.StackTrace() );
		}

		public override int Add( object value )
		{
			OnPopulate();
			return -1;
		}

		public override void AddRange( ICollection c )
		{
			OnPopulate();
		}

		public override void InsertRange( int index, ICollection c )
		{
			OnPopulate();
		}

		public override void Insert( int index, object value )
		{
			OnPopulate();
		}

		public override void SetRange( int index, ICollection c )
		{
			OnPopulate();
		}
	}
}
