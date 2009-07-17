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
/* ChangeLog:
 *  6/16/04, Old Salty
 *  	Fixed a RunUO bug so kindling can once again be taken from trees
 *	6/5/04, Pix
 *		Merged in 1.0RC0 code.
*/

using System;
using Server;
using Server.Targeting;
using Server.Items;
using Server.Engines.Harvest;
using Server.Mobiles;
using Server.Engines.Quests;
using Server.Engines.Quests.Hag;

namespace Server.Targets
{
	public class BladedItemTarget : Target
	{
		private Item m_Item;

		public BladedItemTarget( Item item ) : base( 2, false, TargetFlags.None )
		{
			m_Item = item;
		}

		protected override void OnTargetOutOfRange( Mobile from, object targeted )
		{
			if ( targeted is UnholyBone && from.InRange( ((UnholyBone)targeted), 12 ) )
				((UnholyBone)targeted).Carve( from, m_Item );
			else
				base.OnTargetOutOfRange (from, targeted);
		}

		protected override void OnTarget( Mobile from, object targeted )
		{
			if ( m_Item.Deleted )
				return;

			if ( targeted is ICarvable )
			{
				((ICarvable)targeted).Carve( from, m_Item );
			}
			else if ( targeted is SwampDragon && ((SwampDragon)targeted).HasBarding )
			{
				SwampDragon pet = (SwampDragon)targeted;

				if ( !pet.Controlled || pet.ControlMaster != from )
					from.SendLocalizedMessage( 1053022 ); // You cannot remove barding from a swamp dragon you do not own.
				else
					pet.HasBarding = false;
			}
			else if ( targeted is StaticTarget )
			{
				int itemID = ((StaticTarget)targeted).ItemID;

				if ( itemID == 0xD15 || itemID == 0xD16 ) // red mushroom
				{
					PlayerMobile player = from as PlayerMobile;

					if ( player != null )
					{
						QuestSystem qs = player.Quest;

						if ( qs is WitchApprenticeQuest )
						{
							FindIngredientObjective obj = qs.FindObjective( typeof( FindIngredientObjective ) ) as FindIngredientObjective;

							if ( obj != null && !obj.Completed && obj.Ingredient == Ingredient.RedMushrooms )
							{
								player.SendLocalizedMessage( 1055036 ); // You slice a red cap mushroom from its stem.
								obj.Complete();
							}
						}
					}
				}
			
				else
				{
					HarvestSystem system = Lumberjacking.System;
					HarvestDefinition def = Lumberjacking.System.Definition;

					int tileID;
					Map map;
					Point3D loc;
	
					if ( !system.GetHarvestDetails( from, m_Item, targeted, out tileID, out map, out loc ) )
					{
						from.SendLocalizedMessage( 500494 ); // You can't use a bladed item on that!
					}
					else if ( !def.Validate( tileID ) )
					{
						from.SendLocalizedMessage( 500494 ); // You can't use a bladed item on that!
					}
					else
					{
						HarvestBank bank = def.GetBank( map, loc.X, loc.Y );

						if ( bank == null )
							return;

						if ( bank.Current < 5 )
						{
							from.SendLocalizedMessage( 500493 ); // There's not enough wood here to harvest.
						}
						else
						{
							bank.Consume( def, 5 );

							Item item = new Kindling();

							if ( from.PlaceInBackpack( item ) )
							{
								from.SendLocalizedMessage( 500491 ); // You put some kindling into your backpack.
								from.SendLocalizedMessage( 500492 ); // An axe would probably get you more wood.
							}
							else
							{
								from.SendLocalizedMessage( 500490 ); // You can't place any kindling into your backpack!
		
								item.Delete();
							}
						}
					}
				}
			}
		}
	}
}
