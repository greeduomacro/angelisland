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

/* Engines/Crafting/DefCartography.cs
 * CHANGELOG:
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using Server.Items;

namespace Server.Engines.Craft
{
	public class DefCartography : CraftSystem
	{
		public override SkillName MainSkill
		{
			get	{ return SkillName.Cartography; }
		}

		public override int GumpTitleNumber
		{
			get { return 1044008; } // <CENTER>CARTOGRAPHY MENU</CENTER>
		}

		public override double GetChanceAtMin( CraftItem item )
		{
			return 0.0; // 0%
		}

		private static CraftSystem m_CraftSystem;

		public static CraftSystem CraftSystem
		{
			get
			{
				if ( m_CraftSystem == null )
					m_CraftSystem = new DefCartography();

				return m_CraftSystem;
			}
		}

		private DefCartography() : base( 1, 1, 1.25 )// base( 1, 1, 3.0 )
		{
		}

		public override int CanCraft( Mobile from, BaseTool tool, Type itemType )
		{
			if ( tool.Deleted || tool.UsesRemaining < 0 )
				return 1044038; // You have worn out your tool!
			else if ( !BaseTool.CheckAccessible( tool, from ) )
				return 1044263; // The tool must be on your person to use.

			return 0;
		}

		public override void PlayCraftEffect( Mobile from )
		{
			from.PlaySound( 0x249 );
		}

		public override int PlayEndingEffect( Mobile from, bool failed, bool lostMaterial, bool toolBroken, int quality, bool makersMark, CraftItem item )
		{
			if ( toolBroken )
				from.SendLocalizedMessage( 1044038 ); // You have worn out your tool

			if ( failed )
			{
				if ( lostMaterial )
					return 1044043; // You failed to create the item, and some of your materials are lost.
				else
					return 1044157; // You failed to create the item, but no materials were lost.
			}
			else
			{
				if ( quality == 0 )
					return 502785; // You were barely able to make this item.  It's quality is below average.
				else if ( makersMark && quality == 2 )
					return 1044156; // You create an exceptional quality item and affix your maker's mark.
				else if ( quality == 2 )
					return 1044155; // You create an exceptional quality item.
				else				
					return 1044154; // You create the item.
			}
		}

		public override void InitCraftList()
		{
			AddCraft( typeof( LocalMap ), 1044448, 1015230, 10.0, 70.0, typeof( BlankMap ), 1044449, 1, 1044450 );
			AddCraft( typeof(  CityMap ), 1044448, 1015231, 25.0, 85.0, typeof( BlankMap ), 1044449, 1, 1044450 );
			AddCraft( typeof( SeaChart ), 1044448, 1015232, 35.0, 95.0, typeof( BlankMap ), 1044449, 1, 1044450 );
			AddCraft( typeof( WorldMap ), 1044448, 1015233, 39.5, 99.5, typeof( BlankMap ), 1044449, 1, 1044450 );
		}
	}
}