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

namespace Server.Spells.First
{
	public class CreateFoodSpell : Spell
	{
		private static SpellInfo m_Info = new SpellInfo(
				"Create Food", "In Mani Ylem",
				SpellCircle.First,
				224,
				9011,
				Reagent.Garlic,
				Reagent.Ginseng,
				Reagent.MandrakeRoot
			);

		public CreateFoodSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		private static FoodInfo[] m_Food = new FoodInfo[]
			{
				new FoodInfo( typeof( Grapes ), "a grape bunch" ),
				new FoodInfo( typeof( Ham ), "a ham" ),
				new FoodInfo( typeof( CheeseWedge ), "a wedge of cheese" ),
				new FoodInfo( typeof( Muffins ), "muffins" ),
				new FoodInfo( typeof( FishSteak ), "a fish steak" ),
				new FoodInfo( typeof( Ribs ), "cut of ribs" ),
				new FoodInfo( typeof( CookedBird ), "a cooked bird" ),
				new FoodInfo( typeof( Sausage ), "sausage" ),
				new FoodInfo( typeof( Apple ), "an apple" ),
				new FoodInfo( typeof( Peach ), "a peach" )
			};

		public override void OnCast()
		{
			if ( CheckSequence() )
			{
				FoodInfo foodInfo = m_Food[Utility.Random( m_Food.Length )];
				Item food = foodInfo.Create();

				if ( food != null )
				{
					Caster.AddToBackpack( food );

					// You magically create food in your backpack:
					Caster.SendLocalizedMessage( 1042695, true, " " + foodInfo.Name );

					Caster.FixedParticles( 0, 10, 5, 2003, EffectLayer.RightHand );
					Caster.PlaySound( 0x1E2 );
				}
			}

			FinishSequence();
		}
	}

	public class FoodInfo
	{
		private Type m_Type;
		private string m_Name;

		public Type Type{ get{ return m_Type; } set{ m_Type = value; } }
		public string Name{ get{ return m_Name; } set{ m_Name = value; } }

		public FoodInfo( Type type, string name )
		{
			m_Type = type;
			m_Name = name;
		}

		public Item Create()
		{
			Item item;

			try
			{
				item = (Item)Activator.CreateInstance( m_Type );
			}
			catch
			{
				item = null;
			}

			return item;
		}
	}
} 
