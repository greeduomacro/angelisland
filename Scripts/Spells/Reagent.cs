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
using Server.Items;

namespace Server.Spells
{
	public class Reagent
	{
		private static Type[] m_Types = new Type[13]
			{
				typeof( BlackPearl ),
				typeof( Bloodmoss ),
				typeof( Garlic ),
				typeof( Ginseng ),
				typeof( MandrakeRoot ),
				typeof( Nightshade ),
				typeof( SulfurousAsh ),
				typeof( SpidersSilk ),
				typeof( BatWing ),
				typeof( GraveDust ),
				typeof( DaemonBlood ),
				typeof( NoxCrystal ),
				typeof( PigIron )
			};

		public Type[] Types
		{
			get{ return m_Types; }
		}

		public static Type BlackPearl
		{
			get{ return m_Types[0]; }
			set{ m_Types[0] = value; }
		}

		public static Type Bloodmoss
		{
			get{ return m_Types[1]; }
			set{ m_Types[1] = value; }
		}

		public static Type Garlic
		{
			get{ return m_Types[2]; }
			set{ m_Types[2] = value; }
		}

		public static Type Ginseng
		{
			get{ return m_Types[3]; }
			set{ m_Types[3] = value; }
		}

		public static Type MandrakeRoot
		{
			get{ return m_Types[4]; }
			set{ m_Types[4] = value; }
		}

		public static Type Nightshade
		{
			get{ return m_Types[5]; }
			set{ m_Types[5] = value; }
		}

		public static Type SulfurousAsh
		{
			get{ return m_Types[6]; }
			set{ m_Types[6] = value; }
		}

		public static Type SpidersSilk
		{
			get{ return m_Types[7]; }
			set{ m_Types[7] = value; }
		}

		public static Type BatWing
		{
			get{ return m_Types[8]; }
			set{ m_Types[8] = value; }
		}

		public static Type GraveDust
		{
			get{ return m_Types[9]; }
			set{ m_Types[9] = value; }
		}

		public static Type DaemonBlood
		{
			get{ return m_Types[10]; }
			set{ m_Types[10] = value; }
		}

		public static Type NoxCrystal
		{
			get{ return m_Types[11]; }
			set{ m_Types[11] = value; }
		}

		public static Type PigIron
		{
			get{ return m_Types[12]; }
			set{ m_Types[12] = value; }
		}
	}
} 
