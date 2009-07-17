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
	public class HarvestVein
	{
		private double m_VeinChance;
		private double m_ChanceToFallback;
		private HarvestResource m_PrimaryResource;
		private HarvestResource m_FallbackResource;

		public double VeinChance{ get{ return m_VeinChance; } set{ m_VeinChance = value; } }
		public double ChanceToFallback{ get{ return m_ChanceToFallback; } set{ m_ChanceToFallback = value; } }
		public HarvestResource PrimaryResource{ get{ return m_PrimaryResource; } set{ m_PrimaryResource = value; } }
		public HarvestResource FallbackResource{ get{ return m_FallbackResource; } set{ m_FallbackResource = value; } }

		public HarvestVein( double veinChance, double chanceToFallback, HarvestResource primaryResource, HarvestResource fallbackResource )
		{
			m_VeinChance = veinChance;
			m_ChanceToFallback = chanceToFallback;
			m_PrimaryResource = primaryResource;
			m_FallbackResource = fallbackResource;
		}
	}
}�