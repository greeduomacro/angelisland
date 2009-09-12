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

/* Scripts/Context Menus/AddToSpellbookEntry.cs
 * CHANGELOG
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */


using System;
using Server.Items;
using Server.Targeting;

namespace Server.ContextMenus
{
	public class AddToSpellbookEntry : ContextMenuEntry
	{
		public AddToSpellbookEntry()
			: base(6144, 3)
		{
		}

		public override void OnClick()
		{
			if (Owner.From.CheckAlive() && Owner.Target is SpellScroll)
				Owner.From.Target = new InternalTarget((SpellScroll)Owner.Target);
		}

		private class InternalTarget : Target
		{
			private SpellScroll m_Scroll;

			public InternalTarget(SpellScroll scroll)
				: base(3, false, TargetFlags.None)
			{
				m_Scroll = scroll;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (targeted is Spellbook)
				{
					if (from.CheckAlive() && !m_Scroll.Deleted && m_Scroll.Movable && m_Scroll.Amount >= 1)
					{
						Spellbook book = (Spellbook)targeted;

						SpellbookType type = Spellbook.GetTypeForSpell(m_Scroll.SpellID);

						if (type != book.SpellbookType)
						{
						}
						else if (book.HasSpell(m_Scroll.SpellID))
						{
							from.SendLocalizedMessage(500179); // That spell is already present in that spellbook.
						}
						else
						{
							int val = m_Scroll.SpellID - book.BookOffset;

							if (val >= 0 && val < book.BookCount)
							{
								book.Content |= (ulong)1 << val;

								m_Scroll.Consume();

								from.Send(new Network.PlaySound(0x249, book.GetWorldLocation()));
							}
						}
					}
				}
			}
		}
	}
}