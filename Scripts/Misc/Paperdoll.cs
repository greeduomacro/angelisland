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

/* Scripts/Misc/Paperdoll.cs
 * ChangeLog
 *  02/15/05, Pixie
 *		CHANGED FOR RUNUO 1.0.0 MERGE.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Collections;
using Server;
using Server.Network;

namespace Server.Misc
{
	public class Paperdoll
	{
		public static void Initialize()
		{
			EventSink.PaperdollRequest += new PaperdollRequestEventHandler( EventSink_PaperdollRequest );
		}

		public static void EventSink_PaperdollRequest( PaperdollRequestEventArgs e )
		{
			Mobile beholder = e.Beholder;
			Mobile beheld = e.Beheld;

			//beholder.Send( new DisplayPaperdoll( beheld, Titles.ComputeTitle( beholder, beheld ) ) );
			beholder.Send( new DisplayPaperdoll( beheld, Titles.ComputeTitle( beholder, beheld ), beheld.AllowEquipFrom( beholder ) ) );

			if ( ObjectPropertyList.Enabled )
			{
				ArrayList items = beheld.Items;

				for ( int i = 0; i < items.Count; ++i )
					beholder.Send( ((Item)items[i]).OPLPacket );

				// NOTE: OSI sends MobileUpdate when opening your own paperdoll.
				// It has a very bad rubber-banding affect. What positive affects does it have?
			}
		}
	}
}