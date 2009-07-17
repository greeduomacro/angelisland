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

/* Items/Deeds/VendorRenegotiationContract.cs
 * ChangeLog:
 *  1/15/00, Adam
 *		Initial Creation
 *		Convert a Player Vendor from (modified)OSI fees to a commission model
 */

using System;
using Server.Network;
using Server.Prompts;
using Server.Items;
using Server.Targeting;
using Server.Mobiles;
using System.Text.RegularExpressions;
using Server.Misc;

namespace Server.Items
{
	public class VendorRenegotiationContractTarget : Target // Create our targeting class (which we derive from the base target class)
	{
		private VendorRenegotiationContract m_Deed;
               
		public VendorRenegotiationContractTarget(VendorRenegotiationContract deed) : base( 1, false, TargetFlags.None )
		{
			m_Deed = deed;
		}

		protected override void OnTarget( Mobile from, object target ) 
		{
                
			if ( target is PlayerVendor )
			{
				PlayerVendor vendor = (PlayerVendor)target;
				if (vendor.IsOwner(from))
				{
					if (vendor.PricingModel == PricingModel.Commission)
					{
						from.SendMessage("This vendor is already working on commission."); 
					}
					else
					{
						vendor.PricingModel = PricingModel.Commission;
						vendor.SayTo(from, String.Format("I shall now work for a minimum wage plus a {0}% comission.", ((int)(vendor.Commission * 100)).ToString()));
						m_Deed.Delete();
					}
				}

				else
				{
					vendor.SayTo(from, "I do not work for thee! Only my master may renegotiate my contract.");
				}
			}
			else
			{
				from.SendMessage("Thou canst only renegotiate the contracts of thy own servants."); 
			}
		}
		
	}

            
	public class VendorRenegotiationContract : Item // Create the item class which is derived from the base item class
	{
		[Constructable]
		public VendorRenegotiationContract() : base( 0x14F0 )
		{
			Weight = 1.0;
			Name = "a vendor renegotiation contract";
		}

		public VendorRenegotiationContract(Serial serial) : base( serial )
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
		}
           
		public override void OnDoubleClick( Mobile from )
		{	
			// Make sure deed is in pack
			if(!IsChildOf(from.Backpack)) 
			{
				from.SendLocalizedMessage(1042001);
				return;
			}
							        
			// Create target and call it
			from.SendMessage("Whose contract dost thou wish to renegotiate?");
			from.Target = new VendorRenegotiationContractTarget(this);
		}

	}

}

