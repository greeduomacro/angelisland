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

/* Items/Deeds/NameChangeDeed.cs
 * ChangeLog:
 *	8/26/07, Adam
 *		Double check that a name change is in your backpack when it is applied, this stops
 *		Players from getting the rename prompt and handing the deed to a friend.
 *	2/23/05, erlein
 *		Changed output format of log entry to include date/time of change.
 *  2/15/05, erlein
 *		Altered so is automatic.. added prompt which includes warning,
 *		regular expression to check for weird chars, NameVerification
 *		validate check and log entry to track name changes ("logs/namechange.log").
 *  8/27/04, Adam
 *		Add message when double clicked.
 */

using System;
using System.IO;
using System.Text.RegularExpressions;
using Server.Network;
using Server.Prompts;
using Server.Items;
using Server.Misc;

namespace Server.Items
{
	public class NameChangeDeed : Item
	{
		[Constructable]
		public NameChangeDeed() : base( 0x14F0 )
		{
			base.Weight = 1.0;
			base.Name = "a name change deed";
		}

		public NameChangeDeed( Serial serial ) : base( serial )
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
			// Make sure is in pack
			if(!IsChildOf(from.Backpack)) {
				from.SendLocalizedMessage(1042001);  // Must be in pack to use!!
				return;
			}

			// Do namechange
			from.SendMessage("Please choose your new name. There will be no refunds for a poorly selected name.");
			from.Prompt = new RenamePrompt( from, this );

		}

		private class RenamePrompt : Prompt
		{
			private Mobile m_from;
			private NameChangeDeed m_ncdeed;

			public RenamePrompt( Mobile from, NameChangeDeed ncdeed )
			{
				m_from   = from;
				m_ncdeed = ncdeed;
			}

			public override void OnResponse ( Mobile from, string text )
			{

				// Pattern match for invalid characters
				Regex InvalidPatt = new Regex("[^-a-zA-Z0-9' ]");

				if (m_ncdeed == null || m_ncdeed.Deleted == true)
				{
					// error
					from.SendLocalizedMessage(1042001);  // Must be in pack to use!!
				}
				// Make sure is in pack (still)
				else if (!m_ncdeed.IsChildOf(from.Backpack))
				{
					from.SendLocalizedMessage(1042001);  // Must be in pack to use!!
				}
				else if(InvalidPatt.IsMatch(text)) {

					// Invalid chars
					from.SendMessage("You may only use numbers, letters, apostrophes, hyphens and spaces in your name.");

				} else if(!NameVerification.Validate( text, 2, 16, true, true, true, 1, NameVerification.SpaceDashPeriodQuote )) {

					// Invalid for some other reason
					from.SendMessage("That name is not allowed here.");

				} else {

					// Log change
					try {

						StreamWriter LogFile = new StreamWriter( "logs/namechange.log", true );
						LogFile.WriteLine("{0}: {1},{2},{3}", DateTime.Now, from.Account, from.Name, text);
						LogFile.Close();

					} catch {
					}

					// Make the change
					from.Name = text;
					from.SendMessage("You have successfully changed your name to {0}.", text);

					// Destroy deed
					m_ncdeed.Delete();
				}

			}

		}

	}

}