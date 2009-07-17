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

/* Scripts/Gumps/Guilds/RecruitTarget.cs
 * Changelog:
 *	1/13/09, Adam
 *		Allow CertificateOfIdentity to be used as a valid player proxy
 *	4/28/06, Pix
 *		Changes for Kin alignment by guild.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using Server;
using Server.Guilds;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Gumps
{
	public class GuildRecruitTarget : Target
	{
		private Mobile m_Mobile;
		private Guild m_Guild;

		public GuildRecruitTarget( Mobile m, Guild guild ) : base( 10, false, TargetFlags.None )
		{
			m_Mobile = m;
			m_Guild = guild;
		}

		protected override void OnTarget( Mobile from, object targeted )
		{
			if ( GuildGump.BadMember( m_Mobile, m_Guild ) )
				return;

			if (targeted is Mobile || targeted is Items.CertificateOfIdentity)
			{
				Mobile m = targeted as Mobile;
				PlayerMobile pm = targeted as PlayerMobile;
				Items.CertificateOfIdentity coi = null;

				if (targeted is Items.CertificateOfIdentity)
				{
					coi = targeted as Items.CertificateOfIdentity;
					if (coi.Mobile != null && coi.Mobile.Deleted == false)
					{	// reassign
						m = coi.Mobile as Mobile;
						pm = coi.Mobile as PlayerMobile;
					}
				}

				if (coi != null && (coi.Mobile == null || coi.Mobile.Deleted))
				{
					from.SendMessage("That identity certificate does not represent a player.");
				}
				else if (!m.Player )
				{
					m_Mobile.SendLocalizedMessage( 501161 ); // You may only recruit players into the guild.
				}
				else if ( !m.Alive )
				{
					m_Mobile.SendLocalizedMessage( 501162 ); // Only the living may be recruited.
				}
				else if ( m_Guild.IsMember( m ) )
				{
					m_Mobile.SendLocalizedMessage( 501163 ); // They are already a guildmember!
				}
					//Pix: IOBAlignment check
				else if( pm != null  && !( pm.IOBAlignment == IOBAlignment.None || pm.IOBAlignment == m_Guild.IOBAlignment ) )
				{
					m_Mobile.SendMessage( "Only non-aligned or same-aligned can be recruited." );
				}
				else if ( m_Guild.Candidates.Contains( m ) )
				{
					m_Mobile.SendLocalizedMessage( 501164 ); // They are already a candidate.
				}
				else if ( m_Guild.Accepted.Contains( m ) )
				{
					m_Mobile.SendLocalizedMessage( 501165 ); // They have already been accepted for membership, and merely need to use the Guildstone to gain full membership.
				}
				else if ( m.Guild != null )
				{
					m_Mobile.SendLocalizedMessage( 501166 ); // You can only recruit candidates who are not already in a guild.
				}
				else if ( m_Mobile.AccessLevel >= AccessLevel.GameMaster || m_Guild.Leader == m_Mobile )
				{
					m_Guild.Accepted.Add( m );
				}
				else
				{
					m_Guild.Candidates.Add( m );
				}
			}
		}

		protected override void OnTargetFinish( Mobile from )
		{
			if ( GuildGump.BadMember( m_Mobile, m_Guild ) )
				return;

			GuildGump.EnsureClosed( m_Mobile );
			m_Mobile.SendGump( new GuildGump( m_Mobile, m_Guild ) );
		}
	}
}