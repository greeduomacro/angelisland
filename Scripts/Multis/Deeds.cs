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

/* Scripts/Multis/Deeds.cs
 * Changelog:
 *	10/19/06, Adam
 *		Tower placement: not necessarily an exploit, but lets track it
 *	10/16/06, Adam
 *		Add flag to disable tower placement
 *	7/22/06, weaver
 *		Modified OnPlacement() to ignore Siege tents and count regular tents
 *	5/2/06, weaver
 *		Made HouseDeed.OnPlacement() virtual and modified to ignore tents
 * 	5/1/06, weaver
 *		Added overloaded HouseDeed constructor to handle deeds which
 *		explicitly specify their deed id 
 */

using Server;
using System;
using System.Collections;
using Server.Multis;
using Server.Targeting;
using Server.Items;
using Server.Scripts.Commands;

namespace Server.Multis.Deeds
{
    public class HousePlacementTarget : MultiTarget
    {
        private HouseDeed m_Deed;

        public HousePlacementTarget(HouseDeed deed)
            : base(deed.MultiID, deed.Offset)
        {
            m_Deed = deed;
        }

        protected override void OnTarget(Mobile from, object o)
        {
            IPoint3D ip = o as IPoint3D;

            if (ip != null)
            {
                if (ip is Item)
                    ip = ((Item)ip).GetWorldTop();

                Point3D p = new Point3D(ip);

                Region reg = Region.Find(new Point3D(p), from.Map);

                if (from.AccessLevel >= AccessLevel.GameMaster || reg.AllowHousing(from, p))
                    m_Deed.OnPlacement(from, p);
                else if (reg is TreasureRegion)
                    from.SendLocalizedMessage(1043287); // The house could not be created here.  Either something is blocking the house, or the house would not be on valid terrain.
                else
                    from.SendLocalizedMessage(501265); // Housing can not be created in this area.
            }
        }
    }

    public abstract class HouseDeed : Item
    {
        private int m_MultiID;
        private Point3D m_Offset;

        [CommandProperty(AccessLevel.GameMaster)]
        public int MultiID
        {
            get
            {
                return m_MultiID;
            }
            set
            {
                m_MultiID = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D Offset
        {
            get
            {
                return m_Offset;
            }
            set
            {
                m_Offset = value;
            }
        }

        // wea: explicit multi and deed id specifying constructor
        public HouseDeed(int did, int id, Point3D offset)
            : base(did)
        {
            Weight = 1.0;
            LootType = LootType.Newbied;
            m_Offset = offset;
            m_MultiID = id;
        }

        public HouseDeed(int id, Point3D offset)
            : base(0x14F0)
        {
            Weight = 1.0;
            LootType = LootType.Newbied;
            m_MultiID = id & 0x3FFF;
            m_Offset = offset;
        }

        public HouseDeed(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1); // version
            writer.Write(m_Offset);
            writer.Write(m_MultiID);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    {
                        m_Offset = reader.ReadPoint3D();
                        goto case 0;
                    }
                case 0:
                    {
                        m_MultiID = reader.ReadInt();
                        break;
                    }
            }

            if (Weight == 0.0)
                Weight = 1.0;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
            else
            {
                from.SendLocalizedMessage(1010433); /* House placement cancellation could result in a
													   * 60 second delay in the return of your deed.
													   */

                from.Target = new HousePlacementTarget(this);
            }
        }

        public abstract BaseHouse GetHouse(Mobile owner);
        public abstract Rectangle2D[] Area { get; }

        public virtual void OnPlacement(Mobile from, Point3D p)
        {
            if (Deleted)
                return;

            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
                return;
            }

            if (from.AccessLevel < AccessLevel.GameMaster && BaseHouse.HasAccountHouse(from))
            {
                // wea: modified to ignore *siege* tents

                int i = 0;
                ArrayList ahouses = new ArrayList();
                ahouses = BaseHouse.GetAccountHouses(from);

                if (ahouses != null)
                {
                    foreach (object o in ahouses)
                        if (!(o is SiegeTent))
                            i++;

                    if (i > 0)
                    {
                        from.SendLocalizedMessage(501271); // You already own a house or a regular tent, you may not place another!
                        return;
                    }
                }
            }

            ArrayList toMove;
            Point3D center = new Point3D(p.X - m_Offset.X, p.Y - m_Offset.Y, p.Z - m_Offset.Z);
            HousePlacementResult res = HousePlacement.Check(from, m_MultiID, center, out toMove);

            switch (res)
            {
                case HousePlacementResult.Valid:
                    {
                        BaseHouse house = GetHouse(from);
                        house.MoveToWorld(center, from.Map);
                        Delete();

                        for (int i = 0; i < toMove.Count; ++i)
                        {
                            object o = toMove[i];

                            if (o is Mobile)
                                ((Mobile)o).Location = house.BanLocation;
                            else if (o is Item)
                                ((Item)o).Location = house.BanLocation;
                        }

                        break;
                    }
                case HousePlacementResult.BadItem:
                case HousePlacementResult.BadLand:
                case HousePlacementResult.BadStatic:
                case HousePlacementResult.BadRegionHidden:
                    {
                        from.SendLocalizedMessage(1043287); // The house could not be created here.  Either something is blocking the house, or the house would not be on valid terrain.
                        break;
                    }
                case HousePlacementResult.NoSurface:
                    {
                        from.SendMessage("The house could not be created here.  Part of the foundation would not be on any surface.");
                        break;
                    }
                case HousePlacementResult.BadRegion:
                    {
                        from.SendLocalizedMessage(501265); // Housing cannot be created in this area.
                        break;
                    }
                case HousePlacementResult.BadRegionTownship:
                    {
                        from.SendMessage("You are not authorized to build in this township.");
                        break;
                    }
            }
        }
    }

    public class StonePlasterHouseDeed : HouseDeed
    {
        [Constructable]
        public StonePlasterHouseDeed()
            : base(0x64, new Point3D(0, 4, 0))
        {
        }

        public StonePlasterHouseDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new SmallOldHouse(owner, 0x64);
        }

        public override int LabelNumber { get { return 1041211; } }
        public override Rectangle2D[] Area { get { return SmallOldHouse.AreaArray; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class FieldStoneHouseDeed : HouseDeed
    {
        [Constructable]
        public FieldStoneHouseDeed()
            : base(0x66, new Point3D(0, 4, 0))
        {
        }

        public FieldStoneHouseDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new SmallOldHouse(owner, 0x66);
        }

        public override int LabelNumber { get { return 1041212; } }
        public override Rectangle2D[] Area { get { return SmallOldHouse.AreaArray; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class SmallBrickHouseDeed : HouseDeed
    {
        [Constructable]
        public SmallBrickHouseDeed()
            : base(0x68, new Point3D(0, 4, 0))
        {
        }

        public SmallBrickHouseDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new SmallOldHouse(owner, 0x68);
        }

        public override int LabelNumber { get { return 1041213; } }
        public override Rectangle2D[] Area { get { return SmallOldHouse.AreaArray; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class WoodHouseDeed : HouseDeed
    {
        [Constructable]
        public WoodHouseDeed()
            : base(0x6A, new Point3D(0, 4, 0))
        {
        }

        public WoodHouseDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new SmallOldHouse(owner, 0x6A);
        }

        public override int LabelNumber { get { return 1041214; } }
        public override Rectangle2D[] Area { get { return SmallOldHouse.AreaArray; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class WoodPlasterHouseDeed : HouseDeed
    {
        [Constructable]
        public WoodPlasterHouseDeed()
            : base(0x6C, new Point3D(0, 4, 0))
        {
        }

        public WoodPlasterHouseDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new SmallOldHouse(owner, 0x6C);
        }

        public override int LabelNumber { get { return 1041215; } }
        public override Rectangle2D[] Area { get { return SmallOldHouse.AreaArray; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class ThatchedRoofCottageDeed : HouseDeed
    {
        [Constructable]
        public ThatchedRoofCottageDeed()
            : base(0x6E, new Point3D(0, 4, 0))
        {
        }

        public ThatchedRoofCottageDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new SmallOldHouse(owner, 0x6E);
        }

        public override int LabelNumber { get { return 1041216; } }
        public override Rectangle2D[] Area { get { return SmallOldHouse.AreaArray; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class BrickHouseDeed : HouseDeed
    {
        [Constructable]
        public BrickHouseDeed()
            : base(0x74, new Point3D(-1, 7, 0))
        {
        }

        public BrickHouseDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new GuildHouse(owner);
        }

        public override int LabelNumber { get { return 1041219; } }
        public override Rectangle2D[] Area { get { return GuildHouse.AreaArray; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class TwoStoryWoodPlasterHouseDeed : HouseDeed
    {
        [Constructable]
        public TwoStoryWoodPlasterHouseDeed()
            : base(0x76, new Point3D(-3, 7, 0))
        {
        }

        public TwoStoryWoodPlasterHouseDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new TwoStoryHouse(owner, 0x76);
        }

        public override int LabelNumber { get { return 1041220; } }
        public override Rectangle2D[] Area { get { return TwoStoryHouse.AreaArray; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class TwoStoryStonePlasterHouseDeed : HouseDeed
    {
        [Constructable]
        public TwoStoryStonePlasterHouseDeed()
            : base(0x78, new Point3D(-3, 7, 0))
        {
        }

        public TwoStoryStonePlasterHouseDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new TwoStoryHouse(owner, 0x78);
        }

        public override int LabelNumber { get { return 1041221; } }
        public override Rectangle2D[] Area { get { return TwoStoryHouse.AreaArray; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class TowerDeed : HouseDeed
    {
        [Constructable]
        public TowerDeed()
            : base(0x7A, new Point3D(0, 7, 0))
        {
        }

        public TowerDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new Tower(owner);
        }

        public override int LabelNumber { get { return 1041222; } }
        public override Rectangle2D[] Area { get { return Tower.AreaArray; } }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
                if (CoreAI.IsDynamicFeatureSet(CoreAI.FeatureBits.TowerAllowed) == false)
                {
                    from.SendMessage("Tower placement temporarily disabled");
                    return;
                }
                else
                {
                    // not necessarily an exploit, but lets track it
                    LogHelper.TrackIt(from, "Note: Tower placement attempt.", true);
                }

            base.OnDoubleClick(from);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class KeepDeed : HouseDeed
    {
        [Constructable]
        public KeepDeed()
            : base(0x7C, new Point3D(0, 11, 0))
        {
        }

        public KeepDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new Keep(owner);
        }

        public override int LabelNumber { get { return 1041223; } }
        public override Rectangle2D[] Area { get { return Keep.AreaArray; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class CastleDeed : HouseDeed
    {
        [Constructable]
        public CastleDeed()
            : base(0x7E, new Point3D(0, 16, 0))
        {
        }

        public CastleDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new Castle(owner);
        }

        public override int LabelNumber { get { return 1041224; } }
        public override Rectangle2D[] Area { get { return Castle.AreaArray; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class LargePatioDeed : HouseDeed
    {
        [Constructable]
        public LargePatioDeed()
            : base(0x8C, new Point3D(-4, 7, 0))
        {
        }

        public LargePatioDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new LargePatioHouse(owner);
        }

        public override int LabelNumber { get { return 1041231; } }
        public override Rectangle2D[] Area { get { return LargePatioHouse.AreaArray; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class LargeMarbleDeed : HouseDeed
    {
        [Constructable]
        public LargeMarbleDeed()
            : base(0x96, new Point3D(-4, 7, 0))
        {
        }

        public LargeMarbleDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new LargeMarbleHouse(owner);
        }

        public override int LabelNumber { get { return 1041236; } }
        public override Rectangle2D[] Area { get { return LargeMarbleHouse.AreaArray; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class SmallTowerDeed : HouseDeed
    {
        [Constructable]
        public SmallTowerDeed()
            : base(0x98, new Point3D(3, 4, 0))
        {
        }

        public SmallTowerDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new SmallTower(owner);
        }

        public override int LabelNumber { get { return 1041237; } }
        public override Rectangle2D[] Area { get { return SmallTower.AreaArray; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class LogCabinDeed : HouseDeed
    {
        [Constructable]
        public LogCabinDeed()
            : base(0x9A, new Point3D(1, 6, 0))
        {
        }

        public LogCabinDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new LogCabin(owner);
        }

        public override int LabelNumber { get { return 1041238; } }
        public override Rectangle2D[] Area { get { return LogCabin.AreaArray; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class SandstonePatioDeed : HouseDeed
    {
        [Constructable]
        public SandstonePatioDeed()
            : base(0x9C, new Point3D(-1, 4, 0))
        {
        }

        public SandstonePatioDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new SandStonePatio(owner);
        }

        public override int LabelNumber { get { return 1041239; } }
        public override Rectangle2D[] Area { get { return SandStonePatio.AreaArray; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class VillaDeed : HouseDeed
    {
        [Constructable]
        public VillaDeed()
            : base(0x9E, new Point3D(3, 6, 0))
        {
        }

        public VillaDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new TwoStoryVilla(owner);
        }

        public override int LabelNumber { get { return 1041240; } }
        public override Rectangle2D[] Area { get { return TwoStoryVilla.AreaArray; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class StoneWorkshopDeed : HouseDeed
    {
        [Constructable]
        public StoneWorkshopDeed()
            : base(0xA0, new Point3D(-1, 4, 0))
        {
        }

        public StoneWorkshopDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new SmallShop(owner, 0xA0);
        }

        public override int LabelNumber { get { return 1041241; } }
        public override Rectangle2D[] Area { get { return SmallShop.AreaArray2; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class MarbleWorkshopDeed : HouseDeed
    {
        [Constructable]
        public MarbleWorkshopDeed()
            : base(0xA2, new Point3D(-1, 4, 0))
        {
        }

        public MarbleWorkshopDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseHouse GetHouse(Mobile owner)
        {
            return new SmallShop(owner, 0xA2);
        }

        public override int LabelNumber { get { return 1041242; } }
        public override Rectangle2D[] Area { get { return SmallShop.AreaArray1; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}
