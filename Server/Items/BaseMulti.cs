/***************************************************************************
 *                                BaseMulti.cs
 *                            -------------------
 *   begin                : May 1, 2002
 *   copyright            : (C) The RunUO Software Team
 *   email                : info@runuo.com
 *
 *   $Id: BaseMulti.cs,v 1.4 2008/05/15 20:24:36 adam Exp $
 *   $Author: adam $
 *   $Date: 2008/05/15 20:24:36 $
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

/* Server/Items/BaseMulti.cs
 * CHANGELOG:
 *  3/25/07, Adam
 *      Add FindAll() function to locate all multis at a given point.
 */

using System;
using System.Collections;

namespace Server.Items
{
	public class BaseMulti : Item
	{
		[Constructable]
		public BaseMulti(int itemID)
			: base(itemID)
		{
			Movable = false;
		}

		public BaseMulti(Serial serial)
			: base(serial)
		{
		}

		public virtual void RefreshComponents()
		{
			if (Parent != null)
				return;

			Map map = Map;

			if (map != null)
			{
				map.OnLeave(this);
				map.OnEnter(this);
			}
		}

		public override int LabelNumber
		{
			get
			{
				MultiComponentList mcl = this.Components;

				if (mcl.List.Length > 0)
					return 1020000 + (mcl.List[0].m_ItemID & 0x3FFF);

				return base.LabelNumber;
			}
		}

		public override int GetMaxUpdateRange()
		{
			return 22;
		}

		public override int GetUpdateRange(Mobile m)
		{
			return 22;
		}

		public virtual MultiComponentList Components
		{
			get
			{
				return MultiData.GetComponents(ItemID);
			}
		}

		public virtual bool Contains(Point2D p)
		{
			return Contains(p.m_X, p.m_Y);
		}

		public virtual bool Contains(Point3D p)
		{
			return Contains(p.m_X, p.m_Y);
		}

		public virtual bool Contains(IPoint3D p)
		{
			return Contains(p.X, p.Y);
		}

		public virtual bool Contains(int x, int y)
		{
			MultiComponentList mcl = this.Components;

			x -= this.X + mcl.Min.m_X;
			y -= this.Y + mcl.Min.m_Y;

			return x >= 0
				&& x < mcl.Width
				&& y >= 0
				&& y < mcl.Height
				&& mcl.Tiles[x][y].Length > 0;
		}

		public bool Contains(Mobile m)
		{
			if (m.Map == this.Map)
				return Contains(m.X, m.Y);
			else
				return false;
		}

		public bool Contains(Item item)
		{
			if (item.Map == this.Map)
				return Contains(item.X, item.Y);
			else
				return false;
		}

		public virtual bool Inside(int x, int y)
		{
			MultiComponentList mcl = this.Components;

			x -= this.X + mcl.Min.m_X;
			y -= this.Y + mcl.Min.m_Y;

			return x >= 0
				&& x < mcl.Width
				&& y >= 0
				&& y < mcl.Height;
		}

		public static BaseMulti Find(Point3D p, Map map)
		{
			if (map == null)
				return null;

			Sector sector = map.GetSector(p);

			foreach (BaseMulti mult in sector.Multis.Values)
			{
				if (mult == null)
					continue;

				if (mult.Contains(p) || mult.Inside(p.X, p.Y))
					return mult;
			}

			return null;
		}

		public static ArrayList FindAll(Point3D p, Map map)
		{
			ArrayList all = new ArrayList();

			if (map == null)
				return all;

			Sector sector = map.GetSector(p);

			foreach (BaseMulti mult in sector.Multis.Values)
			{
				if (mult == null)
					continue;

				if (mult.Contains(p) || mult.Inside(p.X, p.Y))
					all.Add(mult);
			}

			return all;
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
}