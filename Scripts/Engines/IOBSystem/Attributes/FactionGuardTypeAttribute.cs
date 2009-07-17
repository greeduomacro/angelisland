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

/* Scripts/Engines/IOBSystem/Attributes/FactionGuardTypeAttribute.cs
 * CHANGELOG:
 *	01/14/09 - Plasma,
 *		Initial creation
 */ 

using System;
using System.Collections.Generic;
using Server.Engines.IOBSystem;

namespace Server.Engines.IOBSystem.Attributes
{
	public class KinFactionGuardTypeAttribute : Attribute
	{
		public enum PermissionType
		{
			Allow,
			Deny
		}

		private KinFactionGuardCostTypes m_Type = KinFactionGuardCostTypes.MediumCost;
		private List<IOBAlignment> m_EligibleKin = new List<IOBAlignment>();

		private int m_CustomSlotCost = 0;
		private int m_CustomHireCost = 0;
		private int m_CustomMaintCost = 0;

		public int CustomSlotCost
		{
			get { return m_CustomSlotCost; }
			set { m_CustomSlotCost = value; }
		}

		public int CustomHireCost
		{
			get { return m_CustomHireCost; }
			set { m_CustomHireCost = value; }
		}

		public int CustomMaintCost
		{
			get { return m_CustomMaintCost; }
			set { m_CustomMaintCost = value; }
		}

		public KinFactionGuardCostTypes GuardCostType
		{
			get { return m_Type; }
		}

		public List<IOBAlignment> EligibleKin
		{
			get { return m_EligibleKin; }
		}

		public KinFactionGuardTypeAttribute() { }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type">Guard cost type</param>
		public KinFactionGuardTypeAttribute( KinFactionGuardCostTypes type) : this ( type, PermissionType.Allow, null )
		{
			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="kin">Which kins to apply the permission to.  All kin have access by default. By supplying any Allow permissions will automatically restrict all other kins, and vice-versa</param>
		public KinFactionGuardTypeAttribute(PermissionType permission, IOBAlignment[] kin) : this( KinFactionGuardCostTypes.MediumCost, permission, kin)
		{

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type">Guard cost type</param>
		/// <param name="permission">Whether to restrict or deny access</param>
		/// <param name="eligibleKin">Which kins to apply the permission to.  All kin have access by default.  By supplying any Allow permissions will automatically restrict all other kins, and vice-versa</param>
		public KinFactionGuardTypeAttribute(KinFactionGuardCostTypes type, PermissionType permission, IOBAlignment[] eligibleKin )
		{
			m_Type = type;
			if (eligibleKin != null)
			{
				if (permission == PermissionType.Allow)
				{
					m_EligibleKin.Clear();
					m_EligibleKin.AddRange(eligibleKin);
				}
				else
				{		
					
					//allow everyone but these.
					foreach( string current in Enum.GetNames( typeof(IOBAlignment) ))
					{
						bool found = false;
						foreach( IOBAlignment iob in eligibleKin )
						{
							if( current.Equals(iob.ToString()))
							{
								found = true;
								break;
							}
						}
						if (!found)
						{
							//Add this to the list
							m_EligibleKin.Add((IOBAlignment)Enum.Parse(typeof(IOBAlignment), current));
						}
						
					}
				}

			}
			
		}

		
	}
}