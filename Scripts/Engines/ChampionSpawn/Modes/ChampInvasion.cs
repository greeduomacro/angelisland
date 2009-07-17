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

/* Scripts/Engines/ChampionSpawn/Modes/ChampInvasion.cs
 *	ChangeLog:
 *	10/28/2006, plasma
 *		Initial creation
 * 
 **/
using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.ChampionSpawn
{	
	// This is the town ivasion champion spawn, automated by the AES
	public class ChampInvasion : ChampEngine
	{
		// Members
		private TownInvasionAES m_Monitor;				// external AES spawn monitor
		
		// props
		public TownInvasionAES Monitor					// and a prop for it
		{
			get{ return m_Monitor; }
			set{ m_Monitor = value; }
		}

		[Constructable]
		public ChampInvasion() : base()
		{					
			// pick a random champ
			PickChamp();
			// switch off  gfx and restart timer
			Graphics = false;				
			m_bRestart = false;
		}
		
		protected override void  AdvanceLevel()
		{
			// has champ just been completed?
			if( IsFinalLevel )
			{
				// tell AES that the champ is over
				if ( m_Monitor != null && m_Monitor.Deleted == false )
					m_Monitor.ChampComplete = true;
			}
			base.AdvanceLevel();
		}
		public ChampInvasion( Serial serial ) : base( serial )
		{
		}

		// #region serialize

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize (writer);

			writer.Write( (int) 0 );
			writer.Write( m_Monitor );  //AES 			
		}
		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					m_Monitor = reader.ReadItem() as TownInvasionAES;
					break;
				}
			}
		}
		// #endregion

		public void PickChamp()
		{
			// Currently the invasions randomly pick one of the 5 main big skull giving champs
			switch( Utility.Random(5) )
			{
				case 0:
				{
					SpawnType =  ChampLevelData.SpawnTypes.Abyss ; 
					break;
				}
				case 1:
				{
					SpawnType =  ChampLevelData.SpawnTypes.Arachnid ; 
					break;
				}
				case 2:
				{
					SpawnType =  ChampLevelData.SpawnTypes.ColdBlood ; 
					break;
				}
				case 3:
				{
					SpawnType =  ChampLevelData.SpawnTypes.UnholyTerror; 
					break;
				}
				case 4:
				{
					SpawnType =  ChampLevelData.SpawnTypes.VerminHorde; 
					break;
				}								
			}
		}

		public override void OnSingleClick(Mobile from)
		{
			if( from.AccessLevel >= AccessLevel.GameMaster )
			{			
				// this is a gm, allow normal text from base and champ indicator
				LabelTo( from, "Invasion Champ" );
				base.OnSingleClick(from);
			}
		}
	}
}
