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

/* Scripts/Gumps/Guilds/GuildmasterGump.cs
 * ChangeLog:
 *	07/21/08, Pix
 *		Removed check on moving guildstone if it's in a township.
 *  01/04/08, Pix
 *      Added delay to Guild War Ring toggling.
 *  12/21/07, Pix
 *      Added no counting flag for guilds.
 *  12/6/07, Adam
 *      Change image ID on one of the disabled options.
 *  12/5/07, Adam
 *      Add support for 'peaceful' guilds. I.e., disable war etc. menus
 *	4/28/06, Pix
 *		Changes for Kin alignment by guild.
 *  12/14/05, Kit
 *		Added Ally stuff
 *	3/9/05, mith
 *		Linked "Move this guildstone" option to Guildstone.OnPrepareMove() to re-deed stone.
 */
using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Guilds;
using Server.Network;

namespace Server.Gumps
{
	public class GuildmasterGump : Gump
	{
		private Mobile m_Mobile;
		private Guild m_Guild;

		public GuildmasterGump( Mobile from, Guild guild ) : base( 20, 30 )
		{
			m_Mobile = from;
			m_Guild = guild;

			Dragable = false;

			AddPage( 0 );
			AddBackground( 0, 0, 550, 420, 5054 );
			AddBackground( 10, 10, 530, 400, 3000 );

			AddHtmlLocalized( 20, 15, 510, 35, 1011121, false, false ); // <center>GUILDMASTER FUNCTIONS</center>

			AddButton( 20, 40, 4005, 4007, 2, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 55, 40, 470, 30, 1011107, false, false ); // Set the guild name.

			AddButton( 20, 70, 4005, 4007, 3, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 55, 70, 470, 30, 1011109, false, false ); // Set the guild's abbreviation.

            string strGuildType = "Change guild type/kin: Currently ";
            switch (m_Guild.Type)
            {
                case GuildType.Regular:
                    strGuildType += "Standard, ";
                    break;
                case GuildType.Order:
                    strGuildType += "Order, ";
                    break;
                case GuildType.Chaos:
                    strGuildType += "Chaos, ";
                    break;
            }
            if (m_Guild.IOBAlignment == IOBAlignment.None)
            {
                strGuildType += "Unaligned";
            }
            else
            {
                strGuildType += "Aligned with " + Engines.IOBSystem.IOBSystem.GetIOBName(m_Guild.IOBAlignment);
            }

            if (guild.Peaceful == false)
            {
                AddButton(20, 100, 4005, 4007, 4, GumpButtonType.Reply, 0);
                AddHtml(55, 100, 470, 30, strGuildType, false, false);
            }
            else
            {
                AddImage(20, 100, 4020);
                AddHtml(55, 100, 470, 30, strGuildType, false, false);
            }

			AddButton( 20, 130, 4005, 4007, 5, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 55, 130, 470, 30, 1011112, false, false ); // Set the guild's charter.

			AddButton( 20, 160, 4005, 4007, 6, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 55, 160, 470, 30, 1011113, false, false ); // Dismiss a member.

            if (guild.Peaceful == false)
            {
                AddButton(20, 190, 4005, 4007, 7, GumpButtonType.Reply, 0);
                AddHtmlLocalized(55, 190, 470, 30, 1011114, false, false); // Go to the WAR menu.
            }
            else
            {
                AddImage(20, 190, 4020);
                AddHtmlLocalized(55, 190, 470, 30, 1011114, false, false); // Go to the WAR menu.
            }

            if (DateTime.Now >= m_Guild.GuildWarRingChangeTime.AddMinutes(CoreAI.GWRChangeDelayMinutes))
            {
                if (m_Guild.GuildWarRing)
                {
                    if (guild.Peaceful == false)
                    {
                        AddButton(250, 190, 4005, 4007, 13, GumpButtonType.Reply, 0);
                        AddHtml(285, 190, 470, 30, "Leave the Guild War Ring", false, false);
                    }
                    else
                    {
                        AddImage(250, 190, 4020);
                        AddHtml(285, 190, 470, 30, "Leave the Guild War Ring", false, false);
                    }

                }
                else
                {
                    if (guild.Peaceful == false)
                    {
                        AddButton(250, 190, 4005, 4007, 13, GumpButtonType.Reply, 0);
                        AddHtml(285, 190, 470, 30, "Join the Guild War Ring", false, false);
                    }
                    else
                    {
                        AddImage(250, 190, 4020);
                        AddHtml(285, 190, 470, 30, "Join the Guild War Ring", false, false);
                    }
                }
            }
            else
            {
                string strGWRMessage = string.Format("Guild War Ring change not yet possible.",
                    CoreAI.GWRChangeDelayMinutes);
                AddImage(250, 190, 4020);
                AddHtml(285, 190, 470, 30, strGWRMessage, false, false);
            }

            if (m_Guild.IsNoCountingGuild)
            {
                AddButton(250, 220, 4005, 4007, 14, GumpButtonType.Reply, 0);
                AddHtml(285, 220, 470, 30, "Allow Murder Reporting", false, false);
            }
            else
            {
                AddButton(250, 220, 4005, 4007, 14, GumpButtonType.Reply, 0);
                AddHtml(285, 220, 470, 30, "Restrict Murder Reporting", false, false);
            }

            if (guild.Peaceful == false)
            {
                AddButton(20, 220, 4005, 4007, 12, GumpButtonType.Reply, 0);
                AddHtml(55, 220, 470, 30, "Go to the Ally menu", false, false); // Go to the ALLY menu.
            }
            else
            {
                AddImage(20, 220, 4020);
                AddHtml(55, 220, 470, 30, "Go to the Ally menu", false, false); // Go to the ALLY menu.
            }

			if ( m_Guild.Candidates.Count > 0 )
			{
				AddButton( 20, 250, 4005, 4007, 8, GumpButtonType.Reply, 0 );
				AddHtmlLocalized( 55, 250, 470, 30, 1013056, false, false ); // Administer the list of candidates
			}
			else
			{
				AddImage( 20, 250, 4020 );
				AddHtmlLocalized( 55, 250, 470, 30, 1013031, false, false ); // There are currently no candidates for membership.
			}

			AddButton( 20, 280, 4005, 4007, 9, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 55, 280, 470, 30, 1011117, false, false ); // Set the guildmaster's title.

			AddButton( 20, 310, 4005, 4007, 10, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 55, 310, 470, 30, 1011118, false, false ); // Grant a title to another member.

			AddButton( 20, 340, 4005, 4007, 11, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 55, 340, 470, 30, 1011119, false, false ); // Move this guildstone.

			AddButton( 20, 380, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 55, 380, 245, 30, 1011120, false, false ); // Return to the main menu.

			AddButton( 300, 380, 4005, 4007, 0, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 335, 380, 100, 30, 1011441, false, false ); // EXIT
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			if ( GuildGump.BadLeader( m_Mobile, m_Guild ) )
				return;

			switch ( info.ButtonID )
			{
				case 1: // Main menu
				{
					GuildGump.EnsureClosed( m_Mobile );
					m_Mobile.SendGump( new GuildGump( m_Mobile, m_Guild ) );

					break;
				}
				case 2: // Set guild name
				{
					m_Mobile.SendLocalizedMessage( 1013060 ); // Enter new guild name (40 characters max):
					m_Mobile.Prompt = new GuildNamePrompt( m_Mobile, m_Guild );

					break;
				}
				case 3: // Set guild abbreviation
				{
					m_Mobile.SendLocalizedMessage( 1013061 ); // Enter new guild abbreviation (3 characters max):
					m_Mobile.Prompt = new GuildAbbrvPrompt( m_Mobile, m_Guild );

					break;
				}
				case 4: // Change guild type
				{
					GuildGump.EnsureClosed( m_Mobile );
					m_Mobile.SendGump( new GuildChangeTypeGump( m_Mobile, m_Guild ) );

					break;
				}
				case 5: // Set charter
				{
					m_Mobile.SendLocalizedMessage( 1013071 ); // Enter the new guild charter (50 characters max):
					m_Mobile.Prompt = new GuildCharterPrompt( m_Mobile, m_Guild );

					break;
				}
				case 6: // Dismiss member
				{
					GuildGump.EnsureClosed( m_Mobile );
					m_Mobile.SendGump( new GuildDismissGump( m_Mobile, m_Guild ) );

					break;
				}
				case 7: // War menu
				{
					GuildGump.EnsureClosed( m_Mobile );
					m_Mobile.SendGump( new GuildWarAdminGump( m_Mobile, m_Guild ) );

					break;
				}
				case 8: // Administer candidates
				{
					GuildGump.EnsureClosed( m_Mobile );
					m_Mobile.SendGump( new GuildAdminCandidatesGump( m_Mobile, m_Guild ) );

					break;
				}
				case 9: // Set guildmaster's title
				{
					m_Mobile.SendLocalizedMessage( 1013073 ); // Enter new guildmaster title (20 characters max):
					m_Mobile.Prompt = new GuildTitlePrompt( m_Mobile, m_Mobile, m_Guild );

					break;
				}
				case 10: // Grant title
				{
					GuildGump.EnsureClosed( m_Mobile );
					m_Mobile.SendGump( new GrantGuildTitleGump( m_Mobile, m_Guild ) );

					break;
				}
				case 11: // Move guildstone
				{
					if (m_Guild.Guildstone != null)
					{
						if (m_Guild.Guildstone is Items.Guildstone)
						{
							((Items.Guildstone)m_Guild.Guildstone).OnPrepareMove(m_Mobile);
						}
					}

					// GuildGump.EnsureClosed( m_Mobile );
					// m_Mobile.SendGump( new GuildmasterGump( m_Mobile, m_Guild ) );

					break;
				}
				case 12: // Ally Menu
				{
					GuildGump.EnsureClosed( m_Mobile );
					m_Mobile.SendGump( new GuildAllianceAdminGump( m_Mobile, m_Guild ) );

					break;
				}
				case 13: //Guild War Ring toggle
				{
                    if (DateTime.Now >= m_Guild.GuildWarRingChangeTime.AddMinutes(CoreAI.GWRChangeDelayMinutes))
                    {
                        m_Guild.GuildWarRing = !m_Guild.GuildWarRing;
                        m_Guild.GuildWarRingChangeTime = DateTime.Now;

                        if (m_Guild.GuildWarRing)
                        {
                            m_Guild.GuildMessage("Your guild has joined the Guild War Ring.  All the guilds in the ring are now your enemies.");
                        }
                        else
                        {
                            m_Guild.GuildMessage("Your guild has left the Guild War Ring.");
                        }
                    }
                    else
                    {
                        m_Mobile.SendMessage("You cannot change the Guild War Ring feature yet.");
                    }

					m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
					break;
				}
                case 14: //Murder Reporting
                {
                    m_Guild.IsNoCountingGuild = !m_Guild.IsNoCountingGuild;

                    if (m_Guild.IsNoCountingGuild)
                    {
                        m_Guild.GuildMessage("Your guild now restricts murder reporting.");
                    }
                    else
                    {
                        m_Guild.GuildMessage("Your guild now allows murder reporting.");
                    }

                    m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
                    break;
                }
			}
		}
	}
}