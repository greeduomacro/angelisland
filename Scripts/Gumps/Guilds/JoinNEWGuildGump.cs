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

/* Scripts/Gumps/Guilds/JoinNEWGuildGump.cs
 * ChangeLog
 *  1/5/07, Adam
 *      Obsolete while we auto-add new players
 *  12/6/07, Adam
 *      First time check in.
 *      New gump to auto add players to the New Guild (a peaceful guild)
 */

using System;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Guilds;
using Server.Scripts.Commands;			// log helper

namespace Server.Gumps
{
	/*public class JoinNEWGuildGump : Gump
	{
		private static Guildstone m_Stone;

		public JoinNEWGuildGump( Mobile from ) : base( 50, 50 )
		{
			AddPage( 0 );

			AddBackground( 10, 10, 190, 140, 0x242C );

            AddHtml(30, 30, 150, 75, String.Format("<div align=CENTER>{0}</div>", "Would you like to join the guild for new players?"), false, false);

			AddButton( 40, 105, 0x81A, 0x81B, 0x1, GumpButtonType.Reply, 0 ); // Okay
			AddButton( 110, 105, 0x819, 0x818, 0x2, GumpButtonType.Reply, 0 ); // Cancel
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			Mobile from = state.Mobile;

            if (info.ButtonID == 1)
            {
                Guildstone stone = FindGuild("new");
                if (stone != null && stone.Guild != null)
                {   // log it
                    LogHelper logger = new LogHelper("PlayerAddedToNEWGuild.log", false, true);
                    logger.Log(LogType.Mobile, from);
                    logger.Finish();
                    // do it
                    stone.Guild.AddMember(from);
                    from.DisplayGuildTitle = true;
                    DateTime tx = DateTime.Now.AddDays(14);
                    string title = String.Format("{0}/{1}", tx.Day, tx.Month);
                    from.GuildTitle = title;
                    from.GuildFealty = stone.Guild.Leader;
                    stone.Guild.GuildMessage(String.Format("{0} has just joined {1}.", from.Name, stone.Guild.Abbreviation == null ? "your guild" : stone.Guild.Abbreviation));
                }
                else
                    from.SendMessage("We're sorry, but the new player guild is temporarily unavailable.");
            }
		}

        private Guildstone FindGuild(string abv)
        {
            // cache the stone
            if (m_Stone != null && m_Stone.Deleted == false)
                return m_Stone;

            Guild guild = null;
            string name = abv.ToLower();
            foreach (Item n in World.Items.Values)
            {
                if (n is Guildstone && n != null)
                {
                    if (((Guildstone)n).Guild != null)
                        guild = ((Guildstone)n).Guild;

                    if (guild.Abbreviation != null && guild.Peaceful == true && guild.Abbreviation.ToLower() == name)
                    {   // cache the guildstone
                        m_Stone = (Guildstone)guild.Guildstone;
                        return m_Stone;
                    }
                }
            }

            return null;
        }
	}
    */
}