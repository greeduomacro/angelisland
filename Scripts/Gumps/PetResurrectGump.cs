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

/*
 * Scripts/Gumps/PetRessurectGump.cs
 * CHANGE LOG
 *	10/7/04 - Pixie
 *		Added warning message to pet owner for when the pet will take
 *		skill loss on resurrection.
// 5/25/2004 - Pulse
//		Changed OnResponse() to reduce a ressurected pet's skills by 10%
//		Prior to this change the skills were reduced .1  if the owner resd 
//		and .2 if resd by someone else.
*/

using System;
using Server;
using Server.Mobiles;
using Server.Network;
using Server.Gumps;

namespace Server.Gumps
{
	public class PetResurrectGump : Gump
	{
		private BaseCreature m_Pet;

		public PetResurrectGump( Mobile from, BaseCreature pet ) : base( 50, 50 )
		{
			from.CloseGump( typeof( PetResurrectGump ) );

			bool bStatLoss = false;
			if( pet.BondedDeadPetStatLossTime > DateTime.Now )
			{
				bStatLoss = true;
			}

			m_Pet = pet;

			AddPage( 0 );

			AddBackground( 10, 10, 265, bStatLoss ? 250 : 140, 0x242C );

			AddItem( 205, 40, 0x4 );
			AddItem( 227, 40, 0x5 );

			AddItem( 180, 78, 0xCAE );
			AddItem( 195, 90, 0xCAD );
			AddItem( 218, 95, 0xCB0 );

			AddHtmlLocalized( 30, 30, 150, 75, 1049665, false, false ); // <div align=center>Wilt thou sanctify the resurrection of:</div>
			AddHtml( 30, 70, 150, 25, String.Format( "<div align=CENTER>{0}</div>", pet.Name ), true, false );

			if( bStatLoss )
			{
				string statlossmessage = "Your pet lacks the ability to return to the living at this time without suffering skill loss.";
				AddHtml( 30, 105, 150, 105, String.Format( "<div align=CENTER>{0}</div>", statlossmessage), true, false);
			}

			AddButton( 40, bStatLoss ? 215 : 105, 0x81A, 0x81B, 0x1, GumpButtonType.Reply, 0 ); // Okay
			AddButton( 110, bStatLoss ? 215 : 105, 0x819, 0x818, 0x2, GumpButtonType.Reply, 0 ); // Cancel
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			if ( m_Pet.Deleted || !m_Pet.IsBonded || !m_Pet.IsDeadPet )
				return;

			Mobile from = state.Mobile;

			if ( info.ButtonID == 1 )
			{
                if (m_Pet.Map == null || !m_Pet.Map.CanFit(m_Pet.Location, 16, CanFitFlags.requireSurface))
				{
					from.SendLocalizedMessage( 503256 ); // You fail to resurrect the creature.
					return;
				}

				m_Pet.PlaySound( 0x214 );
				m_Pet.FixedEffect( 0x376A, 10, 16 );
				m_Pet.ResurrectPet();

//				double decreaseAmount;

//				if( from == m_Pet.ControlMaster )
//					decreaseAmount = 0.1;
//				else
//					decreaseAmount = 0.2;

//				for ( int i = 0; i < m_Pet.Skills.Length; ++i )	//Decrease all skills on pet.
//					m_Pet.Skills[i].Base -= decreaseAmount;

				if( m_Pet.BondedDeadPetStatLossTime > DateTime.Now )
				{
					// Reduce all skills on pet by 10%
					for ( int i = 0; i < m_Pet.Skills.Length; ++i )	
						m_Pet.Skills[i].Base -= (m_Pet.Skills[i].Base * .1);
				}
			}

		}
	}
}