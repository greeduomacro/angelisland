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
 *			March 27, 2007
 */

/* Scripts/Engines/IOBSystem/Attributes/FactionCityAttribute.cs
 * CHANGELOG:
 *	04/27/09 - Plasma,
 *		Initial creation
 */ 

using System;
using System.Collections.Generic;
using Server.Engines.IOBSystem;

namespace Server.Engines.IOBSystem.Attributes
{
	public class KinFactionCityAttribute : Attribute
	{

		private int m_GuardSlots = 0;

		public int GuardSlots
		{
			get { return m_GuardSlots; }
			set { m_GuardSlots = value; }
		}

		public KinFactionCityAttribute()
		{
			
		}
		
	}
}