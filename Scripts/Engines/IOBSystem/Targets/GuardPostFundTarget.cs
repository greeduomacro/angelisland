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

/* Engines/IOBSystem/Targets/GuardPost.cs
 * CHANGELOG:
 *	12/16/08, plasma
 *		Initial creation
 */

using System;
using System.Collections.Generic;
using System.Text;
using Server.Targeting;
using Server.Engines.IOBSystem;
using Server.Items;

namespace Server.Engines.IOBSystem.Targets
{
	public class KinGuardPostFundTarget : Target
	{		
		private KinGuardPost _guardPost;

		public KinGuardPostFundTarget(KinGuardPost guardPost) : base(4, false, TargetFlags.None)
		{
			_guardPost = guardPost;
		}

		public KinGuardPostFundTarget() : base(10, false, TargetFlags.None)
		{
		}
		
		protected override void OnTarget(Mobile from, object targeted)
		{
			if( targeted == null || !(targeted  is Item) )
				return;

			if (_guardPost == null)
			{
				//Guard post stage
				if (!(targeted is KinGuardPost))
				{
					from.SendMessage("You may only fund guard posts");
					return;
				}

				//Grab guardpost
				KinGuardPost gp = targeted as KinGuardPost;
				if (gp == null) return;

				//Verify owner
				KinCityData data = KinCityManager.GetCityData(gp.City);
				if( data == null )
				{
					from.SendMessage("That guard post does not appear to be a valid part of a faction city");
					return;
				}
				
				//Owner or city leader can fund
				if (gp.Owner != from && gp.Owner != data.CityLeader )
				{
					from.SendMessage("That guard post does not belong to you");
					return;
				}

				from.SendMessage("Select the silver you wish to fund it with");
				
				//Issue new target
				from.Target = new KinGuardPostFundTarget(gp);
			}
			else
			{
				Silver silver = targeted as Silver;
			  //Silver stage
				if (silver == null)
				{
					from.SendMessage("You may only fund the guard post with silver");
					return;
				}
				if (!from.Backpack.Items.Contains(targeted))
				{
					from.SendMessage("The silver must be in your backpack");
				}
				if (from.GetDistanceToSqrt(_guardPost.Location) > 3)
				{
					from.SendMessage("You are not close enough to the guard post");
					return;
				}
				
				//Verify owner
				KinCityData data = KinCityManager.GetCityData(_guardPost.City);
				if (data == null)
				{
					from.SendMessage("That guard post does not appear to be a valid part of a faction city");
					return;
				}

				//check again that the guard post exists and they are the owner
				if (_guardPost.Deleted || (_guardPost.Owner != from && _guardPost.Owner != data.CityLeader))
				{
					from.SendMessage("The guard post no longer or exists or you are no longer the rightful owner");
					return;
				}
				
				int amount = silver.Amount;
				
				if (amount <= 0)
				{
					//should be impossible
					from.SendMessage("Your guard post was not successfully funded");
					return;
				}

				//Fund guardpost
				silver.Delete();
				_guardPost.Silver += amount;
				//if( !_guardPost.Running ) _guardPost.Running = true;
				from.SendMessage("Your guard post was successfully funded with {0} silver",amount);
			}
		}

	}
}
