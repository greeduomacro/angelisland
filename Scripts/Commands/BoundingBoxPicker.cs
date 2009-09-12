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

using System;
using Server;
using Server.Targeting;

namespace Server
{
	public delegate void BoundingBoxCallback(Mobile from, Map map, Point3D start, Point3D end, object state);

	public class BoundingBoxPicker
	{
		public static void Begin(Mobile from, BoundingBoxCallback callback, object state)
		{
			from.SendMessage("Target the first location of the bounding box.");
			from.Target = new PickTarget(callback, state);
		}

		private class PickTarget : Target
		{
			private Point3D m_Store;
			private bool m_First;
			private Map m_Map;
			private BoundingBoxCallback m_Callback;
			private object m_State;

			public PickTarget(BoundingBoxCallback callback, object state)
				: this(Point3D.Zero, true, null, callback, state)
			{
			}

			public PickTarget(Point3D store, bool first, Map map, BoundingBoxCallback callback, object state)
				: base(-1, true, TargetFlags.None)
			{
				m_Store = store;
				m_First = first;
				m_Map = map;
				m_Callback = callback;
				m_State = state;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				IPoint3D p = targeted as IPoint3D;

				if (p == null)
					return;
				else if (p is Item)
					p = ((Item)p).GetWorldTop();

				if (m_First)
				{
					from.SendMessage("Target another location to complete the bounding box.");
					from.Target = new PickTarget(new Point3D(p), false, from.Map, m_Callback, m_State);
				}
				else if (from.Map != m_Map)
				{
					from.SendMessage("Both locations must reside on the same map.");
				}
				else if (m_Map != null && m_Map != Map.Internal && m_Callback != null)
				{
					Point3D start = m_Store;
					Point3D end = new Point3D(p);

					Utility.FixPoints(ref start, ref end);

					m_Callback(from, m_Map, start, end, m_State);
				}
			}
		}
	}
}
