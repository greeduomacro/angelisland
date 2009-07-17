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

/* Scripts\Items\Special\Holiday\ValentinesDayRose.cs
 * ChangeLog:
 * 12/18/06 Adam
 *		Initial Creation 	
 */

using System;
using System.Text.RegularExpressions;
using Server;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Targeting;
using Server.Prompts;

namespace Server.Items
{
	public class ValentinesDayRose : Item // Create the item class which is derived from the base item class
	{
		private bool m_personalized = false;
		public bool Personalized
		{
			get { return m_personalized; }
			set { m_personalized = value; }
		}

		[Constructable]
		public ValentinesDayRose()
			// ugly, but an easy way to code 'you get the cheap red roses way more often than the neat-o purple ones'
			: base(Utility.RandomList(9035, 9036, 9037, 6377, 6378, 6377, 6378, 6377, 6378, 6377, 6378, 6377, 6378, 6377, 6378))
		{
			Name = "a rose";
            Weight = 1.0;
			Hue = 0;
			LootType = LootType.Newbied;
		}

		public ValentinesDayRose( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			// Make sure deed is in pack
			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042001);
				return;
			}

			// Create target and call it
			if (Personalized == true)
				from.SendMessage("That rose has already been inscribed.");
			else
			{
				from.SendMessage("What dost thou wish to inscribe?");
				from.Prompt = new RenamePrompt(from, this);
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
			writer.Write(m_personalized);
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
			m_personalized = reader.ReadBool();
		}

		private class RenamePrompt : Prompt
		{
			private Mobile m_from;
			ValentinesDayRose m_rose;

			public RenamePrompt(Mobile from, ValentinesDayRose rose)
			{
				m_from = from;
				m_rose = rose;
			}

			public override void OnResponse(Mobile from, string text)
			{
				char[] exceptions = new char[] { ' ', '-', '.', '\'', ':', ',' };
				Regex InvalidPatt = new Regex("[^-a-zA-Z0-9':, ]");
				if (InvalidPatt.IsMatch(text))
				{
					// Invalid chars
					from.SendMessage("You may only use numbers, letters, apostrophes, hyphens, colons, commas, and spaces in the inscription.");

				}
				else if (!NameVerification.Validate(text, 2, 32, true, true, true, 4, exceptions, NameVerification.BuildList(true, true, false, true)))
				{
					// Invalid for some other reason
					from.SendMessage("That inscription is not allowed here.");
				}
				else
				{
					m_rose.Name = text;
					m_rose.Personalized = true;
					from.SendMessage("Thou hast successfully inscribed thy rose.");
				}
			}
		}
	}
}


