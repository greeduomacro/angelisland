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

/* Items/Misc/PublicMoongate.cs
 * CHANGELOG:
 *	11/23/04, Darva
 *		Changed GetDetinationIndex so that a player won't wind up back at the location they just left.
 *	8/6/04, mith
 *		Modified the way we figure new location, based on steps between tram phase and fel phase.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 *	3/15/04, mith
 *		Removed moongate gump and replaced with code to get moonphase to determine destination.
 *		Used reference at Stratics for moongate messages based on destination.
 *		Reference for location based on moonphase: http://martin.brenner.de/ultima/uo/moongates.html
 *		Reference for description based on location: http://uo.stratics.com/content/basics/moongate/moongate.shtml
 *		Script location: Scripts/Items/Misc/PublicMoongate.cs
 */

using System;
using System.Collections;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Mobiles;

namespace Server.Items
{
	public class PublicMoongate : Item
	{
		[Constructable]
		public PublicMoongate() : base( 0xF6C )
		{
			Movable = false;
			Light = LightType.Circle300;
		}

		public PublicMoongate( Serial serial ) : base( serial )
		{
		}

		public override void OnSingleClick( Mobile from )
		{
			if ( !from.Player )
				return;

			int destIndex = GetDestinationIndex( from );

			// Create our message based on the location, Messages are hard-coded in
			// the public class PMList below.
			string NewMsg = PMList.Felucca.Entries[destIndex].Message;

			// Generate an overhead message that only this player can see, describing
			// The scene on the other side of the moongate.
			from.LocalOverheadMessage( MessageType.Emote, 0x3B2, false, NewMsg );
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !from.Player )
				return;

			if ( from.InRange( GetWorldLocation(), 1 ) )
				UseGate( from );
			else
				from.SendLocalizedMessage( 500446 ); // That is too far away.
		}

		public override bool OnMoveOver( Mobile m )
		{
			// Forced to modify this from original. Now that the gump has been removed,
			// this function was not working properly. Not entirely certain on how the
			// events are called, however the following code makes the moongates work.
			// Solution discovered thanks to Pulse.
			if ( !m.Player )
				return true;

			if (UseGate( m ) )
				return false;
			else
				return true;
		}

		public bool UseGate( Mobile m )
		{
			if ( m.Criminal )
			{
				m.SendLocalizedMessage( 1005561, "", 0x22 ); // Thou'rt a criminal and cannot escape so easily.
				return false;
			}
			else if ( Server.Spells.SpellHelper.CheckCombat( m ) )
			{
				m.SendLocalizedMessage( 1005564, "", 0x22 ); // Wouldst thou flee during the heat of battle??
				return false;
			}
			else if ( m.Spell != null )
			{
				m.SendLocalizedMessage( 1049616 ); // You are too busy to do that at the moment.
				return false;
			}
			else
			{
				int destIndex = GetDestinationIndex( m );

				Point3D NewLoc = PMList.Felucca.Entries[destIndex].Location;
				
				BaseCreature.TeleportPets( m, NewLoc, m.Map );
				
				m.Combatant = null;
				m.Warmode = false;
				m.Hidden = true;
				m.Location = NewLoc;

				Effects.PlaySound( m.Location, m.Map, 0x20E );
				return true;
			}
		}

		private int GetDestinationIndex( Mobile m )
		{
			// Get the MoonPhase as an int for referencing array values in PMList
			int TramPhase = (int)Clock.GetMoonPhase( Map.Trammel, m.X, m.Y );
			int FelPhase = (int)Clock.GetMoonPhase( Map.Felucca, m.X, m.Y );

			// Calculate the number of steps between the moon phases.
			// Default Steps to 1 so that we always move at least one step (when phases are the same)
			int Steps = 1;
			if ( FelPhase > TramPhase )
				Steps = FelPhase - TramPhase;
			else if ( TramPhase > FelPhase )
				Steps = (8 - TramPhase) + FelPhase;
				
			// Figure out our current location
			// Then we can use that to calculate which gate we need to move to, based on the Steps.
			int CurLoc = 0;
			for ( int i = 0; i < PMList.Felucca.Entries.Length; i++ )
				if ( PMList.Felucca.Entries[i].Location == this.Location )
					CurLoc = i;
				
			// Calculate the destination index, based on our current location and the number of steps to move.
			int destIndex = 0;
			if ( CurLoc + Steps > 7 )
				destIndex = (CurLoc + Steps) - 8;
			else
				destIndex = CurLoc + Steps;

			if (CurLoc == destIndex)
				destIndex ++;
			if (CurLoc > 7)
				destIndex -= destIndex;
			return destIndex;
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

		public static void Initialize()
		{
			Server.Commands.Register( "MoonGen", AccessLevel.Administrator, new CommandEventHandler( MoonGen_OnCommand ) );
		}

		[Usage( "MoonGen" )]
		[Description( "Generates public moongates. Removes all old moongates." )]
		public static void MoonGen_OnCommand( CommandEventArgs e )
		{
			DeleteAll();

			int count = 0;
			
			// removed code to add moongates in Trammel, Ilshenar, and Malas
			// Operating under the impression these area will be removed
			// The code to generate the gates in those lands in the PMList class
			// has also been removed.
			// When "[moongen" is run by an admin in the client, all moongates are deleted
			// only the ones listed below are re-created.
			count += MoonGen( PMList.Felucca );
			
			World.Broadcast( 0x35, true, "{0} moongates generated.", count );
		}

		private static void DeleteAll()
		{
			ArrayList list = new ArrayList();

			foreach ( Item item in World.Items.Values )
			{
				if ( item is PublicMoongate )
					list.Add( item );
			}

			foreach ( Item item in list )
				item.Delete();

			if ( list.Count > 0 )
				World.Broadcast( 0x35, true, "{0} moongates removed.", list.Count );
		}

		private static int MoonGen( PMList list )
		{
			foreach ( PMEntry entry in list.Entries )
			{
				Item item = new PublicMoongate();

				item.MoveToWorld( entry.Location, list.Map );

				if ( entry.Number == 1060642 ) // Umbra
					item.Hue = 0x497;
			}

			return list.Entries.Length;
		}
	}

	public class PMEntry
	{
		private Point3D m_Location;
		private int m_Number;
		private string m_Message;

		public Point3D Location
		{
			get
			{
				return m_Location;
			}
		}

		public int Number
		{
			get
			{
				return m_Number;
			}
		}

		public string Message
		{
			get
			{
				return m_Message;
			}
		}

		public PMEntry( Point3D loc, int number, string message )
		{
			m_Location = loc;
			m_Number = number;
			m_Message = message;
		}
	}

	public class PMList
	{
		private int m_Number, m_SelNumber;
		private Map m_Map;
		private PMEntry[] m_Entries;

		public int Number
		{
			get
			{
				return m_Number;
			}
		}

		public int SelNumber
		{
			get
			{
				return m_SelNumber;
			}
		}

		public Map Map
		{
			get
			{
				return m_Map;
			}
		}

		public PMEntry[] Entries
		{
			get
			{
				return m_Entries;
			}
		}


		public PMList( int number, int selNumber, Map map, PMEntry[] entries )
		{
			m_Number = number;
			m_SelNumber = selNumber;
			m_Map = map;
			m_Entries = entries;
		}
		
		// Removed map declarations for Trammel, Ilshenar, and Malas.
		// Since the gates don't actually take you there, they are pretty much superfluous.
		public static readonly PMList Felucca =
			new PMList( 1012001, 1012013, Map.Felucca, new PMEntry[]
				{
					new PMEntry( new Point3D( 4467, 1283, 5 ), 1012003, "Through the moongate you see a small escarpment to the south and a large city to the North." ), // Moonglow
					new PMEntry( new Point3D( 1336, 1997, 5 ), 1012004, "Through the moongate you see a road to the east and mountains in the distance to the west." ), // Britain
					new PMEntry( new Point3D( 1499, 3771, 5 ), 1012005, "Through the moongate you see a vast body of water to the east while to the west a city can be seen nearby." ), // Jhelom
					new PMEntry( new Point3D(  771,  752, 5 ), 1012006, "Through the moongate you see deep forest on all sides." ), // Yew
					new PMEntry( new Point3D( 2701,  692, 5 ), 1012007, "Through the moongate you can just make out a road to the southwest and a river to the north." ), // Minoc
					new PMEntry( new Point3D( 1828, 2948,-20), 1012008, "Through the moongate you see a large sandstone city standing on a far bank of the river to the north." ), // Trinsic
					new PMEntry( new Point3D(  643, 2067, 5 ), 1012009, "Through the moongate you see a small city to the south, while a vast ocean lies in all other directions." ), // Skara Brae
					new PMEntry( new Point3D( 3563, 2139, 34), 1012010, "Through the moongate you see what appears to be a small peninsula covered in lush foliage." ), // Magincia
					
			// There's only 8 phases of the moon, and Bucc's has not traditionally been an option.
			// Documentation does not refer to a public gate in Bucs either, so this has been omitted.
			// new PMEntry( new Point3D( 2711, 2234, 0 ), 1019001 )  // Buccaneer's Den
		} );
		// Removed lists of available locations, which were only really used by the gump anyway
		// and since the gump is gone, technically we don't really need this list either
		// but just in case it's needed elsewhere, it's still here.
		public static readonly PMList[] RedLists = new PMList[]{ Felucca };
	}
	// removed code to intitialize and display gump, as well as actions to perform. Code has been
	// moved to the UseGate function above. All checks for criminal, performing an action, combat flags
	// is still in UseGate function.
}

/*
using System;
using System.Collections;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Mobiles;

namespace Server.Items
{
	public class PublicMoongate : Item
	{
		[Constructable]
		public PublicMoongate() : base( 0xF6C )
		{
			Movable = false;
			Light = LightType.Circle300;
		}

		public PublicMoongate( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !from.Player )
				return;

			if ( from.InRange( GetWorldLocation(), 1 ) )
				UseGate( from );
			else
				from.SendLocalizedMessage( 500446 ); // That is too far away.
		}

		public override bool OnMoveOver( Mobile m )
		{
			return !m.Player || UseGate( m );
		}

		public bool UseGate( Mobile m )
		{
			if ( m.Criminal )
			{
				m.SendLocalizedMessage( 1005561, "", 0x22 ); // Thou'rt a criminal and cannot escape so easily.
				return false;
			}
			else if ( Server.Spells.SpellHelper.CheckCombat( m ) )
			{
				m.SendLocalizedMessage( 1005564, "", 0x22 ); // Wouldst thou flee during the heat of battle??
				return false;
			}
			else if ( m.Spell != null )
			{
				m.SendLocalizedMessage( 1049616 ); // You are too busy to do that at the moment.
				return false;
			}
			else
			{
				m.CloseGump( typeof( MoongateGump ) );
				m.SendGump( new MoongateGump( m, this ) );

				Effects.PlaySound( m.Location, m.Map, 0x20E );
				return true;
			}
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

		public static void Initialize()
		{
			Server.Commands.Register( "MoonGen", AccessLevel.Administrator, new CommandEventHandler( MoonGen_OnCommand ) );
		}

		[Usage( "MoonGen" )]
		[Description( "Generates public moongates. Removes all old moongates." )]
		public static void MoonGen_OnCommand( CommandEventArgs e )
		{
			DeleteAll();

			int count = 0;

			count += MoonGen( PMList.Trammel );
			count += MoonGen( PMList.Felucca );
			count += MoonGen( PMList.Ilshenar );
			count += MoonGen( PMList.Malas );

			World.Broadcast( 0x35, true, "{0} moongates generated.", count );
		}

		private static void DeleteAll()
		{
			ArrayList list = new ArrayList();

			foreach ( Item item in World.Items.Values )
			{
				if ( item is PublicMoongate )
					list.Add( item );
			}

			foreach ( Item item in list )
				item.Delete();

			if ( list.Count > 0 )
				World.Broadcast( 0x35, true, "{0} moongates removed.", list.Count );
		}

		private static int MoonGen( PMList list )
		{
			foreach ( PMEntry entry in list.Entries )
			{
				Item item = new PublicMoongate();

				item.MoveToWorld( entry.Location, list.Map );

				if ( entry.Number == 1060642 ) // Umbra
					item.Hue = 0x497;
			}

			return list.Entries.Length;
		}
	}

	public class PMEntry
	{
		private Point3D m_Location;
		private int m_Number;

		public Point3D Location
		{
			get
			{
				return m_Location;
			}
		}

		public int Number
		{
			get
			{
				return m_Number;
			}
		}

		public PMEntry( Point3D loc, int number )
		{
			m_Location = loc;
			m_Number = number;
		}
	}

	public class PMList
	{
		private int m_Number, m_SelNumber;
		private Map m_Map;
		private PMEntry[] m_Entries;

		public int Number
		{
			get
			{
				return m_Number;
			}
		}

		public int SelNumber
		{
			get
			{
				return m_SelNumber;
			}
		}

		public Map Map
		{
			get
			{
				return m_Map;
			}
		}

		public PMEntry[] Entries
		{
			get
			{
				return m_Entries;
			}
		}

		public PMList( int number, int selNumber, Map map, PMEntry[] entries )
		{
			m_Number = number;
			m_SelNumber = selNumber;
			m_Map = map;
			m_Entries = entries;
		}

		public static readonly PMList Trammel =
			new PMList( 1012000, 1012012, Map.Trammel, new PMEntry[]
				{
					new PMEntry( new Point3D( 4467, 1283, 5 ), 1012003 ), // Moonglow
					new PMEntry( new Point3D( 1336, 1997, 5 ), 1012004 ), // Britain
					new PMEntry( new Point3D( 1499, 3771, 5 ), 1012005 ), // Jhelom
					new PMEntry( new Point3D(  771,  752, 5 ), 1012006 ), // Yew
					new PMEntry( new Point3D( 2701,  692, 5 ), 1012007 ), // Minoc
					new PMEntry( new Point3D( 1828, 2948,-20), 1012008 ), // Trinsic
					new PMEntry( new Point3D(  643, 2067, 5 ), 1012009 ), // Skara Brae
					new PMEntry( new Point3D( 3563, 2139, 34), 1012010 ), // Magincia
					new PMEntry( new Point3D( 3763, 2771, 50), 1046259 )  // Haven
				} );

		public static readonly PMList Felucca =
			new PMList( 1012001, 1012013, Map.Felucca, new PMEntry[]
				{
					new PMEntry( new Point3D( 4467, 1283, 5 ), 1012003 ), // Moonglow
					new PMEntry( new Point3D( 1336, 1997, 5 ), 1012004 ), // Britain
					new PMEntry( new Point3D( 1499, 3771, 5 ), 1012005 ), // Jhelom
					new PMEntry( new Point3D(  771,  752, 5 ), 1012006 ), // Yew
					new PMEntry( new Point3D( 2701,  692, 5 ), 1012007 ), // Minoc
					new PMEntry( new Point3D( 1828, 2948,-20), 1012008 ), // Trinsic
					new PMEntry( new Point3D(  643, 2067, 5 ), 1012009 ), // Skara Brae
					new PMEntry( new Point3D( 3563, 2139, 34), 1012010 ), // Magincia
					new PMEntry( new Point3D( 2711, 2234, 0 ), 1019001 )  // Buccaneer's Den
				} );

		public static readonly PMList Ilshenar =
			new PMList( 1012002, 1012014, Map.Ilshenar, new PMEntry[]
				{
					new PMEntry( new Point3D( 1215,  467, -13 ), 1012015 ), // Compassion
					new PMEntry( new Point3D(  722, 1366, -60 ), 1012016 ), // Honesty
					new PMEntry( new Point3D(  744,  724, -28 ), 1012017 ), // Honor
					new PMEntry( new Point3D(  281, 1016,   0 ), 1012018 ), // Humility
					new PMEntry( new Point3D(  987, 1011, -32 ), 1012019 ), // Justice
					new PMEntry( new Point3D( 1174, 1286, -30 ), 1012020 ), // Sacrifice
					new PMEntry( new Point3D( 1532, 1340, - 3 ), 1012021 ), // Spirituality
					new PMEntry( new Point3D(  528,  216, -45 ), 1012022 ), // Valor
					new PMEntry( new Point3D( 1721,  218,  96 ), 1019000 )  // Chaos
				} );

		public static readonly PMList Malas =
			new PMList( 1060643, 1062039, Map.Malas, new PMEntry[]
				{
					new PMEntry( new Point3D( 1015,  527, -65 ), 1060641 ), // Luna
					new PMEntry( new Point3D( 1997, 1386, -85 ), 1060642 )  // Umbra
				} );

		public static readonly PMList[] UORLists = new PMList[]{ Trammel, Felucca };
		public static readonly PMList[] LBRLists = new PMList[]{ Trammel, Felucca, Ilshenar };
		public static readonly PMList[] AOSLists = new PMList[]{ Trammel, Felucca, Ilshenar, Malas };
		public static readonly PMList[] RedLists = new PMList[]{ Felucca };
	}

	public class MoongateGump : Gump
	{
		private Mobile m_Mobile;
		private Item m_Moongate;
		private PMList[] m_Lists;

		public MoongateGump( Mobile mobile, Item moongate ) : base( 100, 100 )
		{
			m_Mobile = mobile;
			m_Moongate = moongate;

			PMList[] checkLists;

			if ( mobile.Player )
			{
				if ( mobile.Kills >= 5 )
				{
					checkLists = PMList.RedLists;
				}
				else
				{
					int flags = mobile.NetState == null ? 0 : mobile.NetState.Flags;

					if ( Core.AOS && (flags & 0x8) != 0 )
						checkLists = PMList.AOSLists;
					else if ( (flags & 0x4) != 0 )
						checkLists = PMList.LBRLists;
					else
						checkLists = PMList.UORLists;
				}
			}
			else
			{
				checkLists = PMList.AOSLists;
			}

			m_Lists = new PMList[checkLists.Length];

			for ( int i = 0; i < m_Lists.Length; ++i )
				m_Lists[i] = checkLists[i];

			for ( int i = 0; i < m_Lists.Length; ++i )
			{
				if ( m_Lists[i].Map == mobile.Map )
				{
					PMList temp = m_Lists[i];

					m_Lists[i] = m_Lists[0];
					m_Lists[0] = temp;

					break;
				}
			}

			AddPage( 0 );

			AddBackground( 0, 0, 380, 280, 5054 );

			AddButton( 10, 210, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 45, 210, 140, 25, 1011036, false, false ); // OKAY

			AddButton( 10, 235, 4005, 4007, 0, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 45, 235, 140, 25, 1011012, false, false ); // CANCEL

			AddHtmlLocalized( 5, 5, 200, 20, 1012011, false, false ); // Pick your destination:

			for ( int i = 0; i < checkLists.Length; ++i )
			{
				AddButton( 10, 35 + (i * 25), 2117, 2118, 0, GumpButtonType.Page, Array.IndexOf( m_Lists, checkLists[i] ) + 1 );
				AddHtmlLocalized( 30, 35 + (i * 25), 150, 20, checkLists[i].Number, false, false );
			}

			for ( int i = 0; i < m_Lists.Length; ++i )
				RenderPage( i, Array.IndexOf( checkLists, m_Lists[i] ) );
		}

		private void RenderPage( int index, int offset )
		{
			PMList list = m_Lists[index];

			AddPage( index + 1 );

			AddButton( 10, 35 + (offset * 25), 2117, 2118, 0, GumpButtonType.Page, index + 1 );
			AddHtmlLocalized( 30, 35 + (offset * 25), 150, 20, list.SelNumber, false, false );

			PMEntry[] entries = list.Entries;

			for ( int i = 0; i < entries.Length; ++i )
			{
				AddRadio( 200, 35 + (i * 25), 210, 211, false, (index * 100) + i );
				AddHtmlLocalized( 225, 35 + (i * 25), 150, 20, entries[i].Number, false, false );
			}
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			if ( info.ButtonID == 0 ) // Cancel
				return;
			else if ( m_Mobile.Deleted || m_Moongate.Deleted || m_Mobile.Map == null )
				return;

			int[] switches = info.Switches;

			if ( switches.Length == 0 )
				return;

			int switchID = switches[0];
			int listIndex = switchID / 100;
			int listEntry = switchID % 100;

			if ( listIndex < 0 || listIndex >= m_Lists.Length )
				return;

			PMList list = m_Lists[listIndex];

			if ( listEntry < 0 || listEntry >= list.Entries.Length )
				return;

			PMEntry entry = list.Entries[listEntry];

			if ( !m_Mobile.InRange( m_Moongate.GetWorldLocation(), 1 ) || m_Mobile.Map != m_Moongate.Map )
			{
				m_Mobile.SendLocalizedMessage( 1019002 ); // You are too far away to use the gate.
			}
			else if ( m_Mobile.Player && m_Mobile.Kills >= 5 && list.Map != Map.Felucca )
			{
				m_Mobile.SendLocalizedMessage( 1019004 ); // You are not allowed to travel there.
			}
			else if ( m_Mobile.Criminal )
			{
				m_Mobile.SendLocalizedMessage( 1005561, "", 0x22 ); // Thou'rt a criminal and cannot escape so easily.
			}
			else if ( Server.Spells.SpellHelper.CheckCombat( m_Mobile ) )
			{
				m_Mobile.SendLocalizedMessage( 1005564, "", 0x22 ); // Wouldst thou flee during the heat of battle??
			}
			else if ( m_Mobile.Spell != null )
			{
				m_Mobile.SendLocalizedMessage( 1049616 ); // You are too busy to do that at the moment.
			}
			else if ( m_Mobile.Map == list.Map && m_Mobile.InRange( entry.Location, 1 ) )
			{
				m_Mobile.SendLocalizedMessage( 1019003 ); // You are already there.
			}
			else
			{
				BaseCreature.TeleportPets( m_Mobile, entry.Location, list.Map );

				m_Mobile.Combatant = null;
				m_Mobile.Warmode = false;
				m_Mobile.Hidden = true;

				m_Mobile.MoveToWorld( entry.Location, list.Map );

				Effects.PlaySound( entry.Location, list.Map, 0x1FE );
			}
		}
	}
}
*/
