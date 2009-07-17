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

namespace Server.Engines.Harvest
{
	public class HarvestResource
	{
		private Type[] m_Types;
		private double m_ReqSkill, m_MinSkill, m_MaxSkill;
		private object m_SuccessMessage;

		public Type[] Types{ get{ return m_Types; } set{ m_Types = value; } }
		public double ReqSkill{ get{ return m_ReqSkill; } set{ m_ReqSkill = value; } }
		public double MinSkill{ get{ return m_MinSkill; } set{ m_MinSkill = value; } }
		public double MaxSkill{ get{ return m_MaxSkill; } set{ m_MaxSkill = value; } }
		public object SuccessMessage{ get{ return m_SuccessMessage; } }

		public void SendSuccessTo( Mobile m )
		{
			if ( m_SuccessMessage is int )
				m.SendLocalizedMessage( (int)m_SuccessMessage );
			else if ( m_SuccessMessage is string )
				m.SendMessage( (string)m_SuccessMessage );
		}

		public HarvestResource( double reqSkill, double minSkill, double maxSkill, object message, params Type[] types )
		{
			m_ReqSkill = reqSkill;
			m_MinSkill = minSkill;
			m_MaxSkill = maxSkill;
			m_Types = types;
			m_SuccessMessage = message;
		}
	}
} 
