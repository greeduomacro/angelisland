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

/* Items/Facial/Beard.cs
 * ChangeLog:
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;

namespace Server.Items
{
	public abstract class Beard : Item
	{
		public static Beard CreateByID( int id, int hue )
		{
			switch ( id )
			{
				case 0x203E: return new LongBeard( hue );
				case 0x203F: return new ShortBeard( hue );
				case 0x2040: return new Goatee( hue );
				case 0x2041: return new Mustache( hue );
				case 0x204B: return new MediumShortBeard( hue );
				case 0x204C: return new MediumLongBeard( hue );
				case 0x204D: return new Vandyke( hue );
				default: return new GenericBeard( id, hue );
			}
		}

		public Beard( int itemID ) : this( itemID, 0 )
		{
		}

		public Beard( int itemID, int hue ) : base( itemID )
		{
			LootType = LootType.Blessed;
			Layer = Layer.FacialHair;
			Hue = hue;
		}

		public Beard( Serial serial ) : base( serial )
		{
		}

		public override bool DisplayLootType{ get{ return false; } }

		public override bool VerifyMove( Mobile from )
		{
			return ( from.AccessLevel >= AccessLevel.GameMaster );
		}

		public override DeathMoveResult OnParentDeath( Mobile parent )
		{
			Dupe( Amount );

			return DeathMoveResult.MoveToCorpse;
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			LootType = LootType.Blessed;

			int version = reader.ReadInt();
		}
	}

	public class GenericBeard : Beard
	{
		[Constructable]
		public GenericBeard( int itemID ) : this( itemID, 0 )
		{
		}

		[Constructable]
		public GenericBeard( int itemID, int hue ) : base( itemID, hue )
		{
		}

		public GenericBeard( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new GenericBeard( ItemID, Hue ), amount );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class LongBeard : Beard
	{
		[Constructable]
		public LongBeard() : this( 0 )
		{
		}

		[Constructable]
		public LongBeard( int hue ) : base( 0x203E, hue )
		{
		}

		public LongBeard( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new LongBeard(), amount );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class ShortBeard : Beard
	{
		[Constructable]
		public ShortBeard() : this( 0 )
		{
		}

		[Constructable]
		public ShortBeard( int hue ) : base( 0x203f, hue )
		{
		}

		public ShortBeard( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new ShortBeard(), amount );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class Goatee : Beard
	{
		[Constructable]
		public Goatee() : this( 0 )
		{
		}

		[Constructable]
		public Goatee( int hue ) : base( 0x2040, hue )
		{
		}

		public Goatee( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new Goatee(), amount );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class Mustache : Beard
	{
		[Constructable]
		public Mustache() : this( 0 )
		{
		}

		[Constructable]
		public Mustache( int hue ) : base( 0x2041, hue )
		{
		}

		public Mustache( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new Mustache(), amount );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class MediumShortBeard : Beard
	{
		[Constructable]
		public MediumShortBeard() : this( 0 )
		{
		}

		[Constructable]
		public MediumShortBeard( int hue ) : base( 0x204B, hue )
		{
		}

		public MediumShortBeard( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new MediumShortBeard(), amount );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class MediumLongBeard : Beard
	{
		[Constructable]
		public MediumLongBeard() : this( 0 )
		{
		}

		[Constructable]
		public MediumLongBeard( int hue ) : base( 0x204C, hue )
		{
		}

		public MediumLongBeard( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new MediumLongBeard(), amount );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class Vandyke : Beard
	{
		[Constructable]
		public Vandyke() : this( 0 )
		{
		}

		[Constructable]
		public Vandyke( int hue ) : base( 0x204D, hue )
		{
		}

		public Vandyke( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new Vandyke(), amount );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}