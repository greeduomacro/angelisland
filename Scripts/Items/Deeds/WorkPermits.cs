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

/* Items/Deeds/WorkPermits.cs
 * ChangeLog:
 *	4/2/08, Adam
 *      first time checkin
 */

using System;
using Server.Network;
using Server.Prompts;
using Server.Items;
using Server.Targeting;
using Server.Multis;        // HouseSign
using Server.Scripts.Commands;			// log helper

namespace Server.Items
{
	public abstract class BaseWorkPermit : Item 
	{
		public BaseWorkPermit() : base( 0x14F0 )
		{
			Weight = 1.0;
            LootType = LootType.Regular;
		}

        public BaseWorkPermit(Serial serial)
            : base(serial)
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteInt32( (int) 0 );       // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt32();
            switch (version)
            {
                case 0:
                {
                    break;
                }
            }
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
                from.SendMessage("Please target the house sign of the house to apply permit to.");
                from.Target = new WorkPermitTarget(this); // Call our target
			 }
		}	
	}
        
    public class WorkPermitTarget : Target
    {
        private BaseWorkPermit m_Deed;

        public WorkPermitTarget(BaseWorkPermit deed)
            : base(1, false, TargetFlags.None)
        {
            m_Deed = deed;
        }

        bool UpgradeCheck(Mobile from, BaseHouse house)
        {
            // see if the upgrade is really an *upgrade*
            //  handle the special case where the user has added taxable lockbox storage below
			if (house.MaximumBarkeepCount >= 255)
            {
				from.SendMessage("Fire regulations prohibit allowing any more barkeeps to work at this residence.");
                return false;
            }

            return true;
        }

        protected override void OnTarget(Mobile from, object target) // Override the protected OnTarget() for our feature
        {
            if (target is HouseSign && (target as HouseSign).Owner != null)
            {
                HouseSign sign = target as HouseSign;
                LogHelper Logger = null;

                try
                {
                    if (sign.Owner.IsFriend(from) == false)
                    {
                        from.SendLocalizedMessage(502094); // You must be in your house to do this.
                        return;
                    }
                    else if (UpgradeCheck(from, (target as HouseSign).Owner) == false)
                    {
                        // filters out any oddball cases and askes the user to correct it
                    }
                    else
                    {
                        BaseHouse house = (target as HouseSign).Owner;
                        Logger = new LogHelper("WorkPermit.log", false);
						Logger.Log(LogType.Item, house, String.Format("WorkPermit applied: {0}", m_Deed.ToString()));
						house.MaximumBarkeepCount++;
						from.SendMessage(String.Format("Permit Accepted. You may now employ up to {0} barkeepers.", house.MaximumBarkeepCount));
                        m_Deed.Delete();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.LogException(ex);
                }
                finally
                {
                    if (Logger != null)
                        Logger.Finish();
                }
            }
            else
            {
                from.SendMessage("That is not a house sign.");
            }
        }
    }

    public class BarkeepWorkPermit : BaseWorkPermit
    {
        [Constructable]
        public BarkeepWorkPermit()
        {
            Name = "work permit for a barkeep";
        }

        public BarkeepWorkPermit(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteInt32((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt32();
        }

    }
}


