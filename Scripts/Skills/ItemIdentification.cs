/*
 *	This program is the CONFIDENTIAL and PROPRIETARY property
 *	of Tomasello Software LLC. Any unauthorized use, reproduction or
 *	transfer of this computer program is strictly prohibited.
 *
 *      Copyright (c) 2004 Tomasello Software LLC.
 *	This is an unpublished work, and is subject to limited distribution and
 *	restricted disclosure only. ALL RIGHTS RESERVED.
 *
 *			RESTRICTED RIGHTS LEGEND
 *	Use, duplication, or disclosure by the Government is subject to
 *	restrictions set forth in subparagraph (c)(1)(ii) of the Rights in
 * 	Technical Data and Computer Software clause at DFARS 252.227-7013.
 *
 *	Angel Island UO Shard	Version 1.0
 *			Release A
 *			March 25, 2004
 */

/* Scripts/Skills/ItemIdentification.cs
 * ChangeLog:
 *  4/17/07, Adam
 *      Add back support for EnchantedScrolls 
 *	3/13/07, weaver
 *		- Centralised identification functionality
 *		- Added RareData based rarity report on successful ID.
 *	11/4/05, weaver
 *		Added check and report on whether item identified is player crafted.
 *	8/11/05, weaver
 *		Added scale for chance to ID enchanted scroll, replacing fixed requirement.
 *	7/27/05, weaver
 *		Added call to skill check function for EnchantedItem types to ensure skill gains
 *		properly.
 *	7/14/05, Kit
 *		Changed Item id for enchateditem type to display propertys automatically after decoded.
 *	7/13/05, weaver
 *		Added check for EnchantedItem type on target of skill use. Reformatted
 *		this changelog too.
 *	05/11/04, Pulse
 *		Added "is BaseJewel" and "is BaseClothing" conditions to the OnTarget method to
 *		implement identifying of magic jewelry and clothes.
 */

using System;
using Server;
using Server.Targeting;

namespace Server.Items
{
	public class ItemIdentification
	{
		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.ItemID].Callback = new SkillUseCallback( OnUse );
		}

		public static TimeSpan OnUse( Mobile from )
		{
			from.SendLocalizedMessage( 500343 ); // What do you wish to appraise and identify?
			from.Target = new InternalTarget();

			return TimeSpan.FromSeconds( 1.0 );
		}

		// wea: 14/Mar/2007 Added rarity check
		public static bool IdentifyItem( Mobile from, object o )
		{
			if (!(o is Item))
				return false;

			Item itm = (Item)o;

			if (o is BaseWeapon)
				((BaseWeapon)o).Identified = true;
			else if (o is BaseArmor)
				((BaseArmor)o).Identified = true;
			else if (o is BaseJewel)
				((BaseJewel)o).Identified = true;
			else if (o is BaseClothing)
				((BaseClothing)o).Identified = true;
            else if (o is EnchantedScroll )
 				((EnchantedScroll)o).Identified = true;


			string idstr = "You determine that : ";

			if (itm.PlayerCrafted)
				idstr += "the item was crafted";
			else
				idstr += "the item is of an unknown origin";

			if (!Core.AOS)
				itm.OnSingleClick(from);

			if (itm.RareData > 0)
			{
				idstr += string.Format(" and is number {0} of a collection of {1}",
									   itm.RareCurIndex,
									  (itm.RareLastIndex - itm.RareStartIndex) + 1);
			}

			if (idstr != "")
			{
				from.SendMessage(idstr + ".");
			}

			return true;
		}
		

		private class InternalTarget : Target
		{
			public InternalTarget() :  base ( 8, false, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if( o is EnchantedItem )
				{
					EnchantedItem EItem = (EnchantedItem) o;
					double level = EItem.MagicLevel;

					int min = 0;

					if( level <= 4.0 && level >= 3.5)
						min = 30;						// Require 30 item ID
					else if( level > 4.0 )
						min = 60; 						// Require 60 item ID

					// Offset skill max value in checkskill to provide significant
					// difficulty of harder scrolls - at most, 115 with level 5

					if( from.CheckSkill( SkillName.ItemID, min, 100 + (min / 4) ) )
					{
						EItem.Identified = true;

						if ( !Core.AOS )
							((Item)o).OnSingleClick( from );
					}
					else
						from.SendLocalizedMessage( 500353 ); // You are not certain...

				}
				else if ( o is Item )
				{
					if ( from.CheckTargetSkill( SkillName.ItemID, o, 0, 100 ) )
						IdentifyItem(from, o);
					else
						from.SendLocalizedMessage( 500353 ); // You are not certain...
				}
				else if ( o is Mobile )
					((Mobile)o).OnSingleClick( from );
				else
					from.SendLocalizedMessage( 500353 ); // You are not certain...
			}
		}
	}
}