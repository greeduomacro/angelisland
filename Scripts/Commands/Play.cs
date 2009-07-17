/*
 *	This program is the CONFIDENTIAL and PROPRIETARY property
 *	of Tomasello Software LLC. Any unauthorized use, reproduction or
 *	transfer of this computer program is strictly prohibited.
 *
 *      Copyright (c) 2004 Tomasello Software LLC.
 *	This is an unpublished work, and is subject to limited distribution and
 *	restricted disclosure only. ALL RIGHTS RESERVED.
 *
 *			RESTRICTED RIGHTS LEGEND
 *	Use, duplication, or disclosure by the Government is subject to
 *	restrictions set forth in subparagraph (c)(1)(ii) of the Rights in
 * 	Technical Data and Computer Software clause at DFARS 252.227-7013.
 *
 *	Angel Island UO Shard	Version 1.0
 *			Release A
 *			March 25, 2004
 */

/* Scripts/Commands/Play.cs
 * Changelog
 *	3/22/08, Adam
 *		Add checks to OnPickedInstrument
 *			Disallow percussion instruments
 *			Make sure it's in your backpack
 *			Convert all the specific Catch types to generic catches and use LogHelper to log them
 *  09/03/06, Rhiannon
 *		Fixed bug in StopMusic_OnCommand().
 *  08/20/06, Rhiannon
 *		Added emote when player starts playing
 *		Added detection of repeated notes
 *	07/30/06, Rhiannon
 *		Initial creation.
 */
using System;
using Server;
using System.Collections;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Targeting;
using System.Text.RegularExpressions;

namespace Server.Scripts.Commands
{
	public class Play
	{
		public static void Initialize()
		{
			Server.Commands.Register( "Play", CoreAI.PlayAccessLevel, new CommandEventHandler( Play_OnCommand ) );
			Server.Commands.Register( "StopMusic", AccessLevel.Player, new CommandEventHandler( StopMusic_OnCommand ) );
			Server.Commands.Register( "FilterMusic", AccessLevel.Player, new CommandEventHandler( FilterMusic_OnCommand ) );
		}

		[Usage( "Play note|pause [note|pause]" )]
		[Description( "Plays a note or a series of notes and pauses." )]
		public static void Play_OnCommand( CommandEventArgs e )
		{
			PlayerMobile pm = (PlayerMobile)e.Mobile;
			Queue PlayList = new Queue();
			Object LastItem = null;
			string[] Notes = { "cl", "csl", "d", "ds", "e", "f", "fs", "g", "gs", "a", "as",
								 "b", "c", "cs", "dh", "dsh", "eh", "fh", "fsh", "gh", "gsh", 
								 "ah", "ash", "bh", "ch"};
			Regex ValidPause = new Regex(@"^((1\.0)|(0\.[0-9]))$");
			int NumOfNotes = 0;
			int MaxQueueSize = 64;
			double MinMusicSkill = 80.0;

			// Allows dynamic control through the CoreManagementConsole.
			if (e.Mobile.AccessLevel < CoreAI.PlayAccessLevel )
			{
				e.Mobile.SendMessage("Playing music is currently disabled.");
				return;
			}

			if ( e.Arguments.Length == 0 )
			{
				Usage(pm);
				return;
			}

			// If the player's Musicianship is too low, don't let them play at all.
			if ( e.Mobile.Skills[SkillName.Musicianship].Value < MinMusicSkill )
			{
				e.Mobile.SendMessage("You do not have enough skill to play a tune.");
				return;
			}

			// If there are too many notes in the queue, make the player pause and try again.
			if ( pm.PlayList != null && pm.PlayList.Count + e.Arguments.Length > MaxQueueSize )
			{
				e.Mobile.SendMessage("Your fingers hurt from so much playing. You must rest a moment before playing another note.");
				return;
			}

			// If there are some leftover notes in the playlist but we're starting a new tune,
			// clear the playlist.
			if ( !pm.Playing && pm.PlayList != null ) 
				pm.PlayList.Clear();

			for ( int i = 0; i < e.Length; ++i )
			{	
				string item = e.Arguments[i].ToLower();
				bool Queued = false;

				for ( int j = 0; j < Notes.Length; ++j )
				{
					if ( item == Notes[j] ) // If the argument is a note, add it directly to the queue.
					{
						// Detect repeated notes
						if ( PlayList.Count > 0 && LastItem is String && ((String)LastItem).ToLower() == item )
							e.Mobile.SendMessage( "Warning: Repeated note detected. Some notes may not play. Insert a 0.3 pause between repeated notes." );
						PlayList.Enqueue(item);
						LastItem = item;
						NumOfNotes++;
						Queued = true;
						break;
					}
				}

				if ( Queued ) continue;
				
				if ( ValidPause.IsMatch( item ) ) // Otherwise, check if it is a valid pause value.
				{
					double d = 0.0;

					try 
					{
						d = System.Convert.ToDouble(item);
//						Console.WriteLine(
//							"The argument has been converted to a double: {0}", d);
					}
					catch (Exception ex)
					{
						Scripts.Commands.LogHelper.LogException(ex);
					}

					PlayList.Enqueue(d); // If so, add it to the queue as a double.
					LastItem = item;
					continue;
				}
				else
				{
					Usage(pm);
					return;
				}
			}

			if ( NumOfNotes == 0 ) // If the list is all pauses, do nothing. 
			{
				PlayList.Clear();
				return;
			}

			// Append the new playlist to the player's existing playlist (or make a new one).
			if ( pm.PlayList == null ) pm.PlayList = new Queue();

			foreach ( Object obj in PlayList )
				pm.PlayList.Enqueue( obj );

			PlayList.Clear();

			// Make sure an instrument is selected.
			BaseInstrument.PickInstrument( pm, new InstrumentPickedCallback( OnPickedInstrument ) );

		}

		public static void OnPickedInstrument( Mobile from, BaseInstrument instrument )
		{
			PlayerMobile pm = (PlayerMobile)from;

			if (!instrument.IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042001); //This must be in your backpack
			}
			else if (instrument is Drums || instrument is Tambourine || instrument is TambourineTassel )
			{
				from.SendMessage("You cannot play a tune on that instrument.");
				BaseInstrument.SetInstrument(from, null);
			}
			else if ( !pm.Playing ) // If this is a new tune, create a new timer and start it.
			{
				from.Emote("*plays a tune*"); // Player emotes to indicate they are playing
				pm.Playing = true;
				PlayTimer pt = new PlayTimer( pm );
				pt.Start();
			}
		}

		public static void Usage(Mobile to)
		{
			to.SendMessage("Usage: [play note|pause [note|pause] ...");
		}

		public class PlayTimer : Timer
		{
			private PlayerMobile m_Player;
			public DateTime m_PauseTime;

			
			public PlayTimer( PlayerMobile pm ) : base( TimeSpan.FromSeconds( 0.0 ), TimeSpan.FromSeconds( 0.1 ), 0 )
			{
				m_Player = pm;
				Priority = TimerPriority.FiftyMS;
				m_PauseTime = DateTime.Now;
			}

			protected override void OnTick()
			{
				if(DateTime.Now < m_PauseTime)
				{
//					Console.WriteLine("Waiting pause time");
					return;
				}

				if ( m_Player.PlayList.Count == 0 ) // If the tune is done, stop the timer.
				{
					m_Player.Playing = false;
					Stop();
					return;
				}
				else
				{
					try 
					{
//						Console.WriteLine(DateTime.Now.TimeOfDay);
						object obj = m_Player.PlayList.Dequeue();

						// If the first item in the queue is a string, make sure a string 
						// instrument is selected, and play the note.
						if (obj.GetType() == (typeof(string)) )
						{
							BaseInstrument instrument = BaseInstrument.GetInstrument( m_Player );

							// Unfortunately, there are no note files for percussion instruments.
							if (instrument is Drums || instrument is Tambourine || instrument is TambourineTassel )
							{
								m_Player.SendMessage("You cannot play a tune on that instrument.");
								m_Player.PlayList = null;
								m_Player.Playing = false;
								Stop();
							}
							else if (instrument == null)
							{
								m_Player.SendMessage("Something has happened to your instrument.");
								m_Player.PlayList = null;
								m_Player.Playing = false;
								Stop();
							}
							else
							{
								Music.PlayNote( m_Player, (string)obj, instrument );
//								Console.WriteLine("Playing Music");
							}
						}
						else // If the first item is a double, treat it as a pause.
						{
							double pause = (double)obj;
//							Console.WriteLine(pause);
							m_PauseTime = DateTime.Now + TimeSpan.FromSeconds(pause);
//							Console.WriteLine(m_PauseTime);
//							Console.WriteLine(DateTime.Now.TimeOfDay);
							return;
						}
					}
					catch (Exception ex)
					{
						Scripts.Commands.LogHelper.LogException(ex);
					}
				}
			}
		}

		[Usage( "[StopMusic" )]
		[Description( "Stops a current melody." )]
		public static void StopMusic_OnCommand( CommandEventArgs e )
		{
			PlayerMobile pm = (PlayerMobile)e.Mobile;
			if ( pm.PlayList == null ) pm.SendMessage("You are not playing anything.");
			else
			{
				pm.PlayList.Clear();
				pm.Playing = false;
				pm.SendMessage("Music stopped.");
			}
		}

		[Usage( "[FilterMusic" )]
		[Description( "Toggles the ability to hear music" )]
		public static void FilterMusic_OnCommand( CommandEventArgs e )
		{
			PlayerMobile pm = (PlayerMobile)e.Mobile;
			bool filter = pm.FilterMusic;
			if ( !filter ) pm.SendMessage("You are now filtering music.");
			else pm.SendMessage("You are no longer filtering music.");
			pm.FilterMusic = !filter;
		}
	}

}
