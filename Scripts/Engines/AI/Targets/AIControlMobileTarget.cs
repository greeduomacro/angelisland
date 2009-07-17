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
using Server.Mobiles;
using System.Collections;

namespace Server.Targets
{
	public class AIControlMobileTarget : Target
	{
		private ArrayList m_List;
		private OrderType m_Order;

		public OrderType Order
		{
			get
			{
				return m_Order;
			}
		}

		public AIControlMobileTarget( BaseAI ai, OrderType order ) : base( -1, false, TargetFlags.None )
		{
			m_List = new ArrayList();
			m_Order = order;

			AddAI( ai );
		}

		public void AddAI( BaseAI ai )
		{
			if ( !m_List.Contains( ai ) )
				m_List.Add( ai );
		}

		protected override void OnTarget( Mobile from, object o )
		{
			if ( o is Mobile )
			{
				for ( int i = 0; i < m_List.Count; ++i )
					((BaseAI)m_List[i]).EndPickTarget( from, (Mobile)o, m_Order );
			}
		}
	}
} 
