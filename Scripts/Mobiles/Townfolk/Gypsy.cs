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

/* /Scripts/Mobiles/Townfolk/Gypsy.cs
 *
 * ChangeLog
 *  07/02/06, Kit
 *		InitOutFit/Body overrides
 *	5/26/04, Pixie
 *		Changed to use the new CollectBounty() call in BountyKeeper.
 *	5/23/04 created by smerX
 *
 */

using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Network;
using Server.ContextMenus;
using EDI = Server.Mobiles;
using Server.BountySystem;
using Server.Scripts.Commands;

namespace Server.Mobiles
{
	public class Gypsy : BaseCreature
	{

		[Constructable]
		public Gypsy() : base( AIType.AI_Melee, FightMode.Aggressor, 22, 1, 0.2, 1.0 )
		{
			InitBody();
			InitOutfit();
			Title = "the Gypsy";
		}

		public override void InitBody()
		{
			SetStr( 90, 100 );
			SetDex( 90, 100 );
			SetInt( 15, 25 );

			Hue = Utility.RandomSkinHue();

			if ( Female = Utility.RandomBool() )
			{
				Body = 401;
				Name = NameList.RandomName( "female" );
			}
			else
			{
				Body = 400;
				Name = NameList.RandomName( "male" );
			}
		}

		public override void InitOutfit()
		{
			WipeLayers();
			AddItem( new Shirt( Utility.RandomNeutralHue() ) );
			AddItem( new ShortPants( Utility.RandomNeutralHue() ) );
			AddItem( new Boots( Utility.RandomNeutralHue() ) );

			switch ( Utility.Random( 4 ) )
			{
				case 0: AddItem( new ShortHair( Utility.RandomHairHue() ) ); break;
				case 1: AddItem( new TwoPigTails( Utility.RandomHairHue() ) ); break;
				case 2: AddItem( new ReceedingHair( Utility.RandomHairHue() ) ); break;
				case 3: AddItem( new KrisnaHair( Utility.RandomHairHue() ) ); break;
			}
			
			switch ( Utility.Random( 4 ) )
			{
				case 0: AddItem( new Drums() ); break;
				case 1: AddItem( new Harp() ); break;
				case 2: AddItem( new Lute() ); break;
				case 3: AddItem( new Tambourine() ); break;
			}
			
			PackGold( 26 );
		}
		
		public override bool OnDragDrop( Mobile from, Item dropped )
		{
			bool bReturn = false;
			try
			{
				if( dropped is Head )
				{
					int result = 0;
					int goldGiven = 0;
					bReturn = BountyKeeper.CollectBounty( (Head)dropped, from, this, ref goldGiven, ref result );
					switch(result)
					{
						case -2:
							Say("Pft.. I don't want that.");
							break;
						case -3:
							Say("Haha, yeah right..");
							Say("I'll take that head, you just run along now.");
							break;
						case 1: //good, gold given
							Say( string.Format("I thank you for the business.  Here's the reward of {0} gold!", goldGiven) );
							break;
						default:
							if( bReturn )
							{
								Say("I'll take that.");
							}
							else
							{
								Say("I don't want that.");
							}
							break;
					}
				}
			}
			catch( Exception e )
			{
				LogHelper.LogException(e);
				System.Console.WriteLine("Error (nonfatal) in Gypsy.OnDragDrop(): " + e.Message);
				System.Console.WriteLine(e.StackTrace);
			}
			return bReturn;
		}
		
		public Gypsy( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}		
	}
}