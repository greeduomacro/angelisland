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

using System;
using Server;
using Server.Targeting;

namespace Server.Engines.PartySystem
{
	public class AddPartyTarget : Target
	{
		public AddPartyTarget( Mobile from ) : base( 8, false, TargetFlags.None )
		{
			from.SendLocalizedMessage( 1005454 ); // Who would you like to add to your party?
		}

		protected override void OnTarget( Mobile from, object o )
		{
			if ( o is Mobile )
			{
				Mobile m = (Mobile)o;
				Party p = Party.Get( from );
				Party mp = Party.Get( m );

				if ( from == m )
					from.SendLocalizedMessage( 1005439 ); // You cannot add yourself to a party.
				else if ( p != null && p.Leader != from )
					from.SendLocalizedMessage( 1005453 ); // You may only add members to the party if you are the leader.
				else if ( m.Party is Mobile )
					return;
				else if ( p != null && (p.Members.Count + p.Candidates.Count) >= Party.Capacity )
					from.SendLocalizedMessage( 1008095 ); // You may only have 10 in your party (this includes candidates).
				else if ( !m.Player && m.Body.IsHuman )
					m.SayTo( from, 1005443 ); // Nay, I would rather stay here and watch a nail rust.
				else if ( !m.Player )
					from.SendLocalizedMessage( 1005444 ); // The creature ignores your offer.
				else if ( mp != null && mp == p )
					from.SendLocalizedMessage( 1005440 ); // This person is already in your party!
				else if ( mp != null )
					from.SendLocalizedMessage( 1005441 ); // This person is already in a party!
				else
					Party.Invite( from, m );
			}
			else
			{
				from.SendLocalizedMessage( 1005442 ); // You may only add living things to your party!
			}
		}
	}
} 
