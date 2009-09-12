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

/* Scripts/Engines/AI/AI/VendorAI.cs
 * CHANGELOG
 *	1/1/09, Adam
 *		Add new Serialization model for creature AI.
 *		BaseCreature will already be serializing this data when I finish the upgrade, so all you need to do is add your data to serialize. 
 *		Make sure to use the SaveFlags to optimize saves by not writing stuff that is non default.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */


using System;
using System.Collections;
using Server.Targeting;
using Server.Network;

//
// This is a first simple AI
//
//

namespace Server.Mobiles
{
	public class VendorAI : BaseAI
	{
		public VendorAI(BaseCreature m)
			: base(m)
		{
		}

		public override bool DoActionWander()
		{
			m_Mobile.DebugSay("I'm fine");

			if (m_Mobile.Combatant != null)
			{
				if (m_Mobile.Debug)
					m_Mobile.DebugSay("{0} is attacking me", m_Mobile.Combatant.Name);

				m_Mobile.Say(Utility.RandomList(1005305, 501603));

				Action = ActionType.Flee;
			}
			else
			{
				if (m_Mobile.FocusMob != null)
				{
					if (m_Mobile.Debug)
						m_Mobile.DebugSay("{0} has talked to me", m_Mobile.FocusMob.Name);

					Action = ActionType.Interact;
				}
				else
				{
					m_Mobile.Warmode = false;

					base.DoActionWander();
				}
			}

			return true;
		}

		public override bool DoActionInteract()
		{
			Mobile customer = m_Mobile.FocusMob;

			if (m_Mobile.Combatant != null)
			{
				if (m_Mobile.Debug)
					m_Mobile.DebugSay("{0} is attacking me", m_Mobile.Combatant.Name);

				m_Mobile.Say(Utility.RandomList(1005305, 501603));

				Action = ActionType.Flee;

				return true;
			}

			if (customer == null || customer.Deleted || customer.Map != m_Mobile.Map)
			{
				m_Mobile.DebugSay("My customer have disapeared");
				m_Mobile.FocusMob = null;

				Action = ActionType.Wander;
			}
			else
			{
				if (customer.InRange(m_Mobile, m_Mobile.RangeFight))
				{
					if (m_Mobile.Debug)
						m_Mobile.DebugSay("I am with {0}", customer.Name);

					m_Mobile.Direction = m_Mobile.GetDirectionTo(customer);
				}
				else
				{
					if (m_Mobile.Debug)
						m_Mobile.DebugSay("{0} is gone", customer.Name);

					m_Mobile.FocusMob = null;

					Action = ActionType.Wander;
				}
			}

			return true;
		}

		public override bool DoActionGuard()
		{
			m_Mobile.FocusMob = m_Mobile.Combatant;
			return base.DoActionGuard();
		}

		public override bool HandlesOnSpeech(Mobile from)
		{
			if (from.InRange(m_Mobile, 4))
				return true;

			return base.HandlesOnSpeech(from);
		}

		// Temporary 
		public override void OnSpeech(SpeechEventArgs e)
		{
			base.OnSpeech(e);

			Mobile from = e.Mobile;

			if (m_Mobile is BaseVendor && from.InRange(m_Mobile, 4) && !e.Handled)
			{
				if (e.HasKeyword(0x14D)) // *vendor sell*
				{
					e.Handled = true;

					((BaseVendor)m_Mobile).VendorSell(from);
					m_Mobile.FocusMob = from;
				}
				else if (e.HasKeyword(0x3C))
				{
					e.Handled = true;

					((BaseVendor)m_Mobile).VendorBuy(from);
					m_Mobile.FocusMob = from;
				}
				else if (WasNamed(e.Speech))
				{
					e.Handled = true;

					if (e.HasKeyword(0x177)) // *sell*
						((BaseVendor)m_Mobile).VendorSell(from);
					else if (e.HasKeyword(0x171)) // *buy*
						((BaseVendor)m_Mobile).VendorBuy(from);

					m_Mobile.FocusMob = from;
				}
			}
		}

		#region Serialize
		private SaveFlags m_flags;

		[Flags]
		private enum SaveFlags
		{	// 0x00 - 0x800 reserved for version
			unused = 0x1000
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			int version = 1;								// current version (up to 4095)
			m_flags = m_flags | (SaveFlags)version;			// save the version and flags
			writer.Write((int)m_flags);

			// add your version specific stuffs here.
			// Make sure to use the SaveFlags for conditional Serialization
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			m_flags = (SaveFlags)reader.ReadInt();				// grab the version an flags
			int version = (int)(m_flags & (SaveFlags)0xFFF);	// maskout the version

			// add your version specific stuffs here.
			// Make sure to use the SaveFlags for conditional Serialization
			switch (version)
			{
				default: break;
			}

		}
		#endregion Serialize
	}
}