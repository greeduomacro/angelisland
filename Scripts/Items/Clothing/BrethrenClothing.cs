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

/* ./Scripts/Items/Clothing/BrethrenClothing.cs
 *	ChangeLog :
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 20 lines removed.
*/

/* /Scripts/Items/Clothing/BrethrenClothing.cs
 * Created 7/20/04 by mith
 *  2/13/05, Froste
 *      Commented out line in OrcishKinMask Deserialize that prevented natural colored masks from staying natural
 *  2/2/05, Darva
 *		Removed karma penalty from orcish iob.
 *  1/15/05, Adam
 *		Remove the forced setting of the Dyable bool in Deserialize (is handled in baseclothing.cs)
 *  1/15/05, Froste
 *      Set new Dyable bool to false in the creation code and Deserialize
 *	11/10/04, Adam
 *		for legacy objects, force the IOBAlignment = IOBAlignment.XXX in Deserialize
 *	7/23/04, mith
 *		made PirateHat un-dyeable.
 */

using System;

namespace Server.Items
{
	public class BloodDrenchedBandana : BaseHat
	{

		[Constructable]
		public BloodDrenchedBandana() : base( 0x1540, 0x66C )
		{
			Weight = 1.0;
			Name = "blood drenched bandana";
			IOBAlignment = IOBAlignment.Council;
            Dyable = false;
        }

		public BloodDrenchedBandana( Serial serial ) : base( serial )
		{
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

			// for legacy objects
			IOBAlignment = IOBAlignment.Council;
        }
	}

	public class PirateHat : BaseHat
	{

		[Constructable]
		public PirateHat() : base( 0x171B, 0x66C )
		{
			Weight = 1.0;
			Name = "a pirate hat";
			IOBAlignment = IOBAlignment.Pirate;
            Dyable = false;
        }

		public PirateHat( Serial serial ) : base( serial )
		{
		}

		/*public override bool Dye( Mobile from, DyeTub sender )
		{
			from.SendLocalizedMessage( sender.FailMessage );
			return false;
		}*/

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			// for legacy objects
			IOBAlignment = IOBAlignment.Pirate;
        }
	}

	public class OrcishKinMask : BaseHat
	{

		public override bool Dye( Mobile from, DyeTub sender )
		{
			from.SendLocalizedMessage( sender.FailMessage );
			return false;
		}

		[Constructable]
		public OrcishKinMask() : this( 0x8A4 )
		{
		}

		[Constructable]
		public OrcishKinMask( int hue ) : base( 0x141B, hue )
		{
			Weight = 2.0;
			Name = "a mask of orcish kin";
			IOBAlignment = IOBAlignment.Orcish;
            Dyable = false;
        }

		public override bool CanEquip( Mobile m )
		{
			if ( !base.CanEquip( m ) )
				return false;

			if ( m.BodyMod == 183 || m.BodyMod == 184 )
			{
				m.SendLocalizedMessage( 1061629 ); // You can't do that while wearing savage kin paint.
				return false;
			}

			return true;
		}

		public override void OnAdded( object parent )
		{
			base.OnAdded( parent );

		}

		public OrcishKinMask( Serial serial ) : base( serial )
		{
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

			//if (Hue != 0x8A4)
			//	Hue = 0x8A4;

			// for legacy objects
			IOBAlignment = IOBAlignment.Orcish;
        }
	}
	
	public class BrigandKinBandana : BaseHat
	{

		[Constructable]
		public BrigandKinBandana() : this( 0 )
		{
		}

		[Constructable]
		public BrigandKinBandana( int hue ) : base( 0x1540, hue )
		{
			Weight = 1.0;
			Name = "brigand kin bandana";
			IOBAlignment = IOBAlignment.Brigand;
            Dyable = false;
        }

		public BrigandKinBandana( Serial serial ) : base( serial )
		{
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

			// for legacy objects
			IOBAlignment = IOBAlignment.Brigand;
        }
	}

	public class BrigandKinBoots : BaseShoes
	{
		[Constructable]
		public BrigandKinBoots() : this( 0 )
		{
		}

		[Constructable]
		public BrigandKinBoots( int hue ) : base( 0x170B, hue )
		{
			Weight = 3.0;
			Name = "brigand kin boots";
			IOBAlignment = IOBAlignment.Brigand;
            Dyable = false;
        }

		public BrigandKinBoots( Serial serial ) : base( serial )
		{
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

			// for legacy objects
			IOBAlignment = IOBAlignment.Brigand;
        }
	}
}
