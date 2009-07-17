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

/* Scripts\Items\Special\Holiday\Snowman.cs
 * Changelog:
 *	12/11/05, Adam
 *		Changed LootType.Blessed to LootType.Regular
 */

using System;
using Server.Items;
using Server.Network;

namespace Server.Items
{
	[Flipable( 0x2328, 0x2329 )]
	public class Snowman : Item, IDyable
	{
		public static string GetRandomTitle()
		{
			// All hail OSI staff
			string[] titles = new string[]
				{
					/*  1 */ "Backflash",
					/*  2 */ "Carbon",
					/*  3 */ "Colbalistic",
					/*  4 */ "Comforl",
					/*  5 */ "Coppacchia",
					/*  6 */ "Cyrus",
					/*  7 */ "DannyB",
					/*  8 */ "DJSoul",
					/*  9 */ "DraconisRex",
					/* 10 */ "Earia",
					/* 11 */ "Foster",
					/* 12 */ "Gonzo",
					/* 13 */ "Haan",
					/* 14 */ "Halona",
					/* 15 */ "Hugo",
					/* 16 */ "Hyacinth",
					/* 17 */ "Imirian",
					/* 18 */ "Jinsol",
					/* 19 */ "Liciatia",
					/* 20 */ "Loewen",
					/* 21 */ "Loke",
					/* 22 */ "Magnus",
					/* 23 */ "Maleki",
					/* 24 */ "Morpheus",
					/* 25 */ "Obberron",
					/* 26 */ "Odee",
					/* 27 */ "Orbeus",
					/* 28 */ "Pax",
					/* 29 */ "Phields",
					/* 30 */ "Pigpen",
					/* 31 */ "Platinum",
					/* 32 */ "Polpol",
					/* 33 */ "Prume",
					/* 34 */ "Quinnly",
					/* 35 */ "Ragnarok",
					/* 36 */ "Rend",
					/* 37 */ "Roland",
					/* 38 */ "RyanM",
					/* 39 */ "Screach",
					/* 40 */ "Seraph",
					/* 41 */ "Silvani",
					/* 42 */ "Sherbear",
					/* 43 */ "SkyWalker",
					/* 44 */ "Snark",
					/* 45 */ "Sowl",
					/* 46 */ "Spada",
					/* 47 */ "Starblade",
					/* 48 */ "Tenacious",
					/* 49 */ "Tnez",
					/* 50 */ "Wasia",
					/* 51 */ "Zilo",
					/* 52 */ "Zippy",
					/* 53 */ "Zoer"
				};

			if ( titles.Length > 0 )
				return titles[Utility.Random( titles.Length )];

			return null;
		}

		private string m_Title;

		[CommandProperty( AccessLevel.GameMaster )]
		public string Title
		{
			get{ return m_Title; }
			set{ m_Title = value; InvalidateProperties(); }
		}

		[Constructable]
		public Snowman() : this( Utility.RandomDyedHue(), GetRandomTitle() )
		{
		}

		[Constructable]
		public Snowman( int hue ) : this( hue, GetRandomTitle() )
		{
		}

		[Constructable]
		public Snowman( string title ) : this( Utility.RandomDyedHue(), title )
		{
		}

		[Constructable]
		public Snowman( int hue, string title ) : base( Utility.Random( 0x2328, 2 ) )
		{
			Weight = 10.0;
			Hue = hue;
			//Adam: Unbless these
			LootType = LootType.Regular;

			m_Title = title;
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			if ( m_Title != null )
				list.Add( 1062841, m_Title ); // ~1_NAME~ the Snowman
		}

		public bool Dye( Mobile from, DyeTub sender )
		{
			if ( Deleted )
				return false;

			Hue = sender.DyedHue;

			return true;
		}

		public Snowman( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 ); // version

			writer.Write( (string) m_Title );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 1:
				{
					m_Title = reader.ReadString();
					break;
				}
			}
		}
	}
}