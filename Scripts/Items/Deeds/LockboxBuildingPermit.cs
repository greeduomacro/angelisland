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

/* Items/Deeds/LockboxBuildingPermit.cs
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
	public class LockboxBuildingPermit : Item // Create the item class which is derived from the base item class
	{
		[Constructable]
		public LockboxBuildingPermit() : base( 0x14F0 )
		{
			Weight = 1.0;
            Name = "building permit: lockbox";
			LootType = LootType.Regular;
		}

		public LockboxBuildingPermit( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteInt32( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt32();
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
                from.SendMessage("Please target the house sign of the house to build on.");
				from.Target = new LockboxBuildingPermitTarget( this ); // Call our target
			 }
		}	
	}

    public class LockboxBuildingPermitTarget : Target
    {
        private LockboxBuildingPermit m_Deed;

        public LockboxBuildingPermitTarget(LockboxBuildingPermit deed)
            : base(1, false, TargetFlags.None)
        {
            m_Deed = deed;
        }

        protected override void OnTarget(Mobile from, object target) // Override the protected OnTarget() for our feature
        {
            if (target is HouseSign && (target as HouseSign).Owner != null)
            {
                HouseSign sign = target as HouseSign;

                if (sign.Owner.IsOwner(from) == false)
                {
                    from.SendLocalizedMessage(1010587); // You are not a co-owner of this house.
                    return;
                }

                if (sign.Owner.CanAddLockbox == false)
                {
                    from.SendMessage("That house cannot hold more lockboxes.");
                    return;
                }

                // 5 free credits: decays at 1 per day
                ushort freeCredits = 5*24;
                if (sign.Owner.CanAddStorageCredits(freeCredits) == true)
                    sign.Owner.StorageTaxCredits += freeCredits;    

                // add the lockbox
                sign.Owner.MaxLockBoxes++;
                from.SendMessage("This house now allows {0} lockboxes.", sign.Owner.MaxLockBoxes);
                m_Deed.Delete(); // Delete the deed                
            }
            else
            {
                from.SendMessage("That is not a house sign.");
            }
        }
    }
}


