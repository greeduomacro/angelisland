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

/* Items/Deeds/StorageTaxCredits.cs
 * ChangeLog:
 *	05/5/07, Adam
 *      first time checkin
 */

using System;
using Server.Network;
using Server.Prompts;
using Server.Items;
using Server.Targeting;
using Server.Multis;        // HouseSign

namespace Server.Items
{
	public class StorageTaxCredits : Item // Create the item class which is derived from the base item class
	{
        private ushort m_Credits;
        public ushort Credits
        {
            get
            {
                return m_Credits;
            }
        }

		[Constructable]
		public StorageTaxCredits() : base( 0x14F0 )
		{
			Weight = 1.0;
            Name = "tax credits: storage";
            LootType = LootType.Regular;

            // 30 credits: Cost is 1K each and decays at 1 per day
            m_Credits = 30*24; 
		}

		public StorageTaxCredits( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteInt32( (int) 0 ); // version

            writer.Write( m_Credits );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt32();

            m_Credits = reader.ReadUShort();
		}

		public override bool DisplayLootType{ get{ return false; } }

		public override void OnDoubleClick( Mobile from )
		{
            if (from.Backpack == null || !IsChildOf(from.Backpack)) // Make sure its in their pack
			{
				 from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
			else
			{
                from.SendMessage("Please target the house sign of the house to apply credits to.");
				from.Target = new StorageTaxCreditsTarget( this ); // Call our target
			 }
		}	
	}

    public class StorageTaxCreditsTarget : Target
    {
        private StorageTaxCredits m_Deed;

        public StorageTaxCreditsTarget(StorageTaxCredits deed)
            : base(1, false, TargetFlags.None)
        {
            m_Deed = deed;
        }

        protected override void OnTarget(Mobile from, object target) // Override the protected OnTarget() for our feature
        {
            if (target is HouseSign && (target as HouseSign).Owner != null)
            {
                HouseSign sign = target as HouseSign;

                if (sign.Owner.IsFriend(from) == false)
                {
                    from.SendLocalizedMessage(502094); // You must be in your house to do this.
                    return;
                }

                if (sign.Owner.CanAddStorageCredits(m_Deed.Credits) == false)
                {
                    from.SendMessage("That house cannot hold more credits.");
                    return;
                }

                sign.Owner.StorageTaxCredits += (uint)m_Deed.Credits;
                from.SendMessage("Your total storage credits are {0}.", sign.Owner.StorageTaxCredits);
                m_Deed.Delete(); // Delete the deed                
            }
            else
            {
                from.SendMessage("That is not a house sign.");
            }
        }
    }
}


