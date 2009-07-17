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

namespace Server.Items
{
	public class RandomWand
	{
		public static BaseWand CreateWand()
		{
			return CreateRandomWand();
		}

		public static BaseWand CreateRandomWand( )
		{
			switch ( Utility.Random( 11 ) )
			{
				default:
				case  0: return new ClumsyWand();
				case  1: return new FeebleWand();
				case  2: return new FireballWand();
				case  3: return new GreaterHealWand();
				case  4: return new HarmWand();
				case  5: return new HealWand();
				case  6: return new IDWand();
				case  7: return new LightningWand();
				case  8: return new MagicArrowWand();
				case  9: return new ManaDrainWand();
				case 10: return new WeaknessWand();
			}
		}
	}
}�