/*
 *	This program is the CONFIDENTIAL and PROPRIETARY property 
 *	of Tomasello Software LLC. Any unauthorized use, reproduction or
 *	transfer of this computer program is strictly prohibited.
 *
 *      Copyright (c ) 2004 Tomasello Software LLC.
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
/* /Scripts/Engines/IOBSystem/KinCityData.cs
 * CHANGELOG
 *	05/25/09, plasma
 *		- Vote annoucnments, made vote end when last beneficiary has voted.
 *		- Fixed bug that was duplicating the deassignment of slots
 *	04/27/09, plasma
 *		Added EmptyTreasury() 
 *	04/08/09, plasma
 *		Refactored some guard post slot allocation stuff; moved some methods into 
 *		the Beneficiary data class to address ambiguity, fixed some bugs with slots...
 *	04/06/09, plasma
 *		Removed "Kin" from guardoptions, already covered with "FactionOnly"
 *		Moved voting methods into here from the city manager as they are more data-centric
 *	01/31/09, plasma
 *		Added treasury and tax members, and a util fucntion or two
 *	01/14/09, plasma,
 *		Add some new methods to modify player's guard slots
 *	10/13/08: Plasma
 *		Fix logic bug in CanVote()
 *	05/15/08: Plasma
 *		Added HasVotingStageExpired prop
 *	04/09/08: Plasma
 *		Sorted out NPC flags
 *	01/08/08: Plasma
 *		Initial Version
 */

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Regions;
using Server.Mobiles;

namespace Server.Engines.IOBSystem
{

	#region City Data class

	/// <summary>
	/// Represents a city's status	
	/// </summary>
	public class KinCityData
	{

		/// <summary>
		/// Flags relating to which NPCs should spawn in a given town
		/// </summary>
		[Flags]
		public enum NPCFlags
		{
			None = 0x00000000,
			Bank = 0x00000001,
			Mages = 0x00000002,
			WeaponArmour = 0x00000004,
			EatDrink = 0x00000008,
			Animal = 0x00000010,
			Patrol = 0x00000020,
			Quest = 0x00000040,
			Gypsy = 0x00000080,
			Carpenter = 0x00000100,
			Healer = 0x00000200,
			Inn = 0x00000400,
			Smith = 0x00000800,
			Tailor = 0x00001000,
			TownCrier = 0x00002000,
			Misc = 0x00004000,
			Provisioner = 0x00008000,
			FightBroker = 0x00010000
		}

		/// <summary>
		/// Represents which guard choice is currently used in a given town
		/// </summary>
		public enum GuardOptions
		{
			None,
			LordBritish,
			FactionOnly,
			FactionAndReds,
			FactionAndRedsAndCrim,
			Everyone,
			RedsAndCrim,
			Crim
		}

		#region fields

		//Misc
		private KinFactionCities m_City = 0;													//city will also be the key in the dictionary
		private IOBAlignment m_ControlingKin;												//Who currently controls this city (none == golem controller king)
		private DateTime m_CaptureTime;															//When the relevant power vortex was destroyed or timed out
		private Mobile m_CityLeader;																//Who is in control of this city - NULL if in voting stage or golem owned
		private bool m_IsVotingStage = false;												//If this city is in the voting stage 
		private List<BeneficiaryData> m_BeneficiaryDataList =				//Beneficiary data, including leader
			new List<BeneficiaryData>();
		private KinSigil m_Sigil = null;														//The sigil item registered with this city 
		private int m_ControlPoints = 0; //Deperceated (unsed)
		private int m_ControlPointDelta = 0;
		private int m_UnassignedGuardSlots = 0;
		private int m_Treasury = 0;																	//Procceds from tax (in gold)
		private double m_TaxRate = 0.15;														//Tax rate for NPC shops

		//City Rules		
		private NPCFlags m_NPCFlags = (NPCFlags)0xFFFFFF;						//Flags indicating which NPCs to spawn / not spawn
		private GuardOptions m_GuardOption = GuardOptions.LordBritish;//Current guard setting
		private DateTime m_LastGuardChangeTime;											//When the last modification to guards was made (change freq determined in KinSettings)


		#endregion

		#region props

		/// <summary>
		/// Gets the treasury.
		/// </summary>
		/// <value>The treasury.</value>
		public int Treasury
		{ get { return m_Treasury; } }

		/// <summary>
		/// Gets or sets the tax rate.
		/// </summary>
		/// <value>The tax rate.</value>
		public double TaxRate
		{
			get { return m_TaxRate; }
			set { m_TaxRate = value; }
		}

		/// <summary>
		/// Gets the city.
		/// </summary>
		/// <value>The city.</value>
		public KinFactionCities City
		{ get { return m_City; } }

		/// <summary>
		/// Gets or sets the controling kin.
		/// </summary>
		/// <value>The controling kin.</value>
		public IOBAlignment ControlingKin
		{
			get { return m_ControlingKin; }
			set { m_ControlingKin = value; }
		}

		/// <summary>
		/// Gets or sets the capture time.
		/// </summary>
		/// <value>The capture time.</value>
		public DateTime CaptureTime
		{
			get { return m_CaptureTime; }
			set { m_CaptureTime = value; }
		}

		/// <summary>
		/// Gets or sets the city leader.
		/// </summary>
		/// <value>The city leader.</value>
		public Mobile CityLeader
		{
			get { return m_CityLeader; }
			set { m_CityLeader = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is voting stage.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is voting stage; otherwise, <c>false</c>.
		/// </value>
		public bool IsVotingStage
		{
			get { return m_IsVotingStage; }
			set { m_IsVotingStage = value; }
		}

		/// <summary>
		/// Gets the activity delta.
		/// </summary>
		/// <value>The activity delta.</value>
		public int ActivityDelta
		{
			get
			{
				return m_ControlPointDelta;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance can change guards.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance can change guards; otherwise, <c>false</c>.
		/// </value>
		public bool CanChangeGuards
		{
			get
			{
				if (DateTime.Now <= LastGuardChangeTime + TimeSpan.FromHours(KinSystemSettings.GuardChangeTimeHours))
					return false;
				else
					return true;
			}
		}

		/// <summary>
		/// Gets the NPC current flags.
		/// </summary>
		/// <value>The NPC current flags.</value>
		public long NPCCurrentFlags
		{
			get { return (long)m_NPCFlags; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance's voting stage has expired.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance has voting stage expired; otherwise, <c>false</c>.
		/// </value>
		public bool HasVotingStageExpired
		{
			get
			{
				if (!IsVotingStage) return false;

				if ((DateTime.Now - m_CaptureTime > TimeSpan.FromHours(24)))
					return true;

				bool allVoted = true;
				foreach (BeneficiaryData bd in BeneficiaryDataList)
				{
					if (bd.HasVoted == false)
					{
						allVoted = false;
						break;
					}
				}

				return allVoted;
			}
		}

		/// <summary>
		/// Gets or sets the guard option.
		/// </summary>
		/// <value>The guard option.</value>
		public GuardOptions GuardOption
		{
			get { return m_GuardOption; }
			set
			{
				m_GuardOption = value;
			}
		}

		/// <summary>
		/// Gets or sets the unassigned guard post slots.
		/// </summary>
		/// <value>The unassigned guard post slots.</value>
		public int UnassignedGuardPostSlots
		{
			get { return m_UnassignedGuardSlots; }
			set
			{
				if (value < 0)
					m_UnassignedGuardSlots = 0;
				else
					m_UnassignedGuardSlots = value;
			}
		}

		/// <summary>
		/// Gets the beneficiary data list.
		/// </summary>
		/// <value>The beneficiary data list.</value>
		public List<BeneficiaryData> BeneficiaryDataList
		{ get { return m_BeneficiaryDataList; } }

		/// <summary>
		/// Gets or sets the sigil.
		/// </summary>
		/// <value>The sigil.</value>
		public KinSigil Sigil
		{
			get { return m_Sigil; }
			set { m_Sigil = value; }
		}

		/// <summary>
		/// Gets or sets the last guard change time.
		/// </summary>
		/// <value>The last guard change time.</value>
		public DateTime LastGuardChangeTime
		{
			get { return m_LastGuardChangeTime; }
			set { m_LastGuardChangeTime = value; }
		}

		#endregion

		#region methods

		#region Activity

		/// <summary>
		/// Processes the activity delta.
		/// </summary>
		/// <param name="delta">The delta.</param>
		public void ProcessActivityDelta(int delta)
		{
			m_ControlPointDelta += delta;
			if (m_ControlPointDelta > 100) m_ControlPointDelta = 100;
			if (m_ControlPointDelta < -100) m_ControlPointDelta = -100;
		}

		/// <summary>
		/// Clears the activity delta.
		/// </summary>
		public void ClearActivityDelta()
		{
			m_ControlPointDelta = 0;
		}

		#endregion

		#region Treasury

		/// <summary>
		/// Adds to treasury.
		/// </summary>
		/// <param name="goldAmount">The gold amount.</param>
		public void AddToTreasury(int goldAmount)
		{
			m_Treasury += goldAmount;
		}

		/// <summary>
		/// Sets the treasury to 0
		/// </summary>
		public void EmptyTreasury()
		{
			m_Treasury = 0;
		}

		/// <summary>
		/// Distributes the treasury amongst the beneficiaries.
		/// </summary>
		public void DistributeTreasury()
		{
			//Work out how much each player will get (rounded DOWN!)
			if (BeneficiaryDataList.Count == 0) return;

			try	//Just in case - anything to do with creating gold is a bit scary.
			{
				int amountEach = Convert.ToInt32(Math.Floor(((Decimal)m_Treasury / BeneficiaryDataList.Count)));
				if (amountEach > 0)
				{
					foreach (KinCityData.BeneficiaryData bd in BeneficiaryDataList)
					{
						try
						{
							if (bd != null)
							{
								int totalAmount = amountEach;
								while (totalAmount > 0)
								{
									int currentAmount = 0;
									if (totalAmount > 60000)
									{
										currentAmount = 60000;
										totalAmount -= 60000;
									}
									else
									{
										currentAmount = totalAmount;
										totalAmount = 0;
									}
									if (currentAmount > 0)
									{
										Gold gold = new Gold(currentAmount);
										bd.Pm.BankBox.AddItem(gold);
									}
								}
							}
						}
						catch (Exception)
						{

							//TODO: Logs !
						}
					}
				}
			}
			catch
			{

				//TODO: Some logging
			}
			finally
			{
				//Make sure whatever happens the treasury is culled!
				m_Treasury = 0;
			}

		}

		#endregion

		#region voting stuff

		/// <summary>
		/// Increases the vote count for a given mobile
		/// </summary>
		/// <param name="city"></param>
		/// <param name="voteFor"></param>
		/// <param name="voteFrom"></param>
		/// <returns></returns>
		public bool CastVote(PlayerMobile voteFor, PlayerMobile voteFrom)
		{
			//woo sanity
			if (voteFor == null || voteFrom == null)
				return false;

			//Check this city is in its voting stage
			if (!IsVotingStage)
				return false;

			//Mobile can't vote for itself
			if (voteFrom == voteFor)
			{
				voteFrom.SendMessage("You may not vote for yourself.");
				return false;
			}

			//double check the mobile can still vote 
			if (!CanVote(voteFrom))
			{
				voteFrom.SendMessage("You may vote only once.");
				return false;
			}

			//lastly make sure the mobile to be voted for is eligible  
			if (!IsBeneficiary(voteFor))
			{
				//This should be impossible
				voteFrom.SendMessage("That person is not a candidate for town control.");
				return false;
			}

			//Increment vote count
			KinCityData.BeneficiaryData v = GetBeneficiary(voteFor);
			if (v == null)
			{
				//Should be impossible
				voteFrom.SendMessage("That person could not be indentified as a beneificary");
			}
			else
			{
				v.Votes++;
			}

			v = GetBeneficiary(voteFrom);
			if (v == null)
			{
				//Should be impossible
				voteFrom.SendMessage("You could not be found in the list of valid beneficiaries.");
			}
			else
			{
				v.HasVoted = true;
			}

			voteFrom.SendMessage("Your vote has been counted successfully.");
			
			//plasma: if everyone has voted, let the stage end early
			bool end = true;
			foreach (BeneficiaryData bd in m_BeneficiaryDataList)
			{
				if (!bd.HasVoted)
				{
					end = false;
					break;
				}
			}

			if (end)
			{
				ProcessVotes();
			}

			return true;
		}

		/// <summary>
		/// Processes the vote count, designates the city leader and switches off the voting stage
		/// </summary>
		/// <param name="data"></param>
		public void ProcessVotes()
		{
			List<PlayerMobile> winners = new List<PlayerMobile>();
			PlayerMobile winner = null;
			//calculate who wins the vote for this city!
			//grab city data			
			if (BeneficiaryDataList.Count == 0)
			{
				//TODO:  might want to log this case?
				//Turn town over to the golem controllers
				KinCityManager.TransferOwnership(this.City, IOBAlignment.None, null);
				return;
			}
			//Call sort
			BeneficiaryDataList.Sort(new KinCityData.BeneficiaryDataComparer(KinCityData.BeneficiaryDataComparer.SortFields.Votes, true));

			//Find highest vote
			int winningVote = BeneficiaryDataList[0].Votes;

			//Get the winner(s)
			foreach (KinCityData.BeneficiaryData v in BeneficiaryDataList)
				if (v.Votes == winningVote)
					winners.Add(v.Pm);

			if (winners.Count == 0)
			{
				//This should be impossible
				// turn city over to golem controllers
				KinCityManager.TransferOwnership(this.City, IOBAlignment.None, null);
			}
			else
			{
				//Pick a random mobile from the winners list to be leader
				winner = winners[Utility.Random(0, winners.Count - 1)];
			}

			//more sanity
			if (winner == null)
			{
				//TODO:  something here heh.  should be massively impossible
			}

			//Establish winner as the leader of this city and declare voting over
			IsVotingStage = false;
			CityLeader = winner;

			foreach (BeneficiaryData bd in BeneficiaryDataList)
			{
				bd.Pm.SendMessage("The voting stage for the City of {0} has ended.  {1} has been selected as the City Leader.", m_City.ToString(), winner.Name);
			}
		}


		#endregion

		#region util/ info

		/// <summary>
		/// Gets the beneficiary.
		/// </summary>
		/// <param name="pm">The pm.</param>
		/// <returns></returns>
		public BeneficiaryData GetBeneficiary(PlayerMobile pm)
		{
			if (pm == null) return null;
			BeneficiaryData data = null;
			try
			{
				data = BeneficiaryDataList.Find(delegate(BeneficiaryData bd) { return bd.Pm == pm; });
			}
			catch
			{
			}
			return data;
		}

		/// <summary>
		/// Gets the beneficiary.
		/// </summary>
		/// <param name="pmName">Name of the pm.</param>
		/// <returns></returns>
		public BeneficiaryData GetBeneficiary(string pmName)
		{
			BeneficiaryData data = null;
			try
			{
				data = BeneficiaryDataList.Find(delegate(BeneficiaryData bd) { return bd.Pm.Name == pmName; });
			}
			catch
			{
			}
			return data;
		}

		/// <summary>
		/// Checks this mobile has not already voted
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public bool CanVote(PlayerMobile m)
		{
			foreach (BeneficiaryData data in m_BeneficiaryDataList)
				if (data.Pm == m && data.HasVoted)
					return false;
			return true;
		}

		/// <summary>
		/// Checks if this mobile appears in the beneficiary list
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public bool IsBeneficiary(PlayerMobile m)
		{
			foreach (BeneficiaryData v in m_BeneficiaryDataList)
				if (v.Pm == m)
					return true;
			return false;
		}

		//flag rubbish hither

		/// <summary>
		/// Gets the NPC flag.
		/// </summary>
		/// <param name="flag">The flag.</param>
		/// <returns></returns>
		public bool GetNPCFlag(NPCFlags flag)
		{
			return ((m_NPCFlags & flag) != 0);
		}

		/// <summary>
		/// Sets the NPC flag.
		/// </summary>
		/// <param name="flag">The flag.</param>
		/// <param name="value">if set to <c>true</c> [value].</param>
		public void SetNPCFlag(NPCFlags flag, bool value)
		{
			if (value)
				m_NPCFlags |= flag;
			else
				m_NPCFlags &= ~flag;
		}

		/// <summary>
		/// Toggles the NPC flag.
		/// </summary>
		/// <param name="flag">The flag.</param>
		public void ToggleNPCFlag(NPCFlags flag)
		{
			if (GetNPCFlag(flag))
				SetNPCFlag(flag, false);
			else
				SetNPCFlag(flag, true);
		}

		/// <summary>
		/// Sets all NPC flags.
		/// </summary>
		public void SetAllNPCFlags()
		{
			m_NPCFlags = (NPCFlags)0xFFFFFF;
		}

		/// <summary>
		/// Clears the NPC Flags.
		/// </summary>
		public void ClearNPCFLags()
		{
			m_NPCFlags = 0;
		}

		#endregion

		#region guards

		/// <summary>
		/// Assigns or removes usable slots from the city pool to the beneficiary
		/// </summary>
		/// <param name="pm">The pm.</param>
		/// <param name="amount">The amount.</param>
		public void ModifyGuardSlots(PlayerMobile pm, int amount)
		{
			KinCityData.BeneficiaryData bd = GetBeneficiary(pm);
			if (bd == null || amount == 0) return;
			if (amount > 0)
			{
				//Simply add to unassigned slots
				bd.UnassignedGuardSlots += amount;
				m_UnassignedGuardSlots -= amount;
			}
			else
			{
				amount = Math.Abs(amount);
				//We can only remove slots if there are some to remove
				if (bd.UnassignedGuardSlots >= amount)
				{
					bd.UnassignedGuardSlots -= amount;
					m_UnassignedGuardSlots += amount;
				}
			}
		}

		/// <summary>
		/// Clears all city guard posts
		/// </summary>
		public void ClearAllGuardPosts()
		{
			//NOTE: the spawner deals with removing active mobiles on delete!
			foreach (BeneficiaryData data in m_BeneficiaryDataList)
			{				
				for (int i = data.GuardPosts.Count - 1; i >= 0; --i) 
				{
					data.GuardPosts[i].Delete(); //this call will deal with slotsas well
				}
				data.GuardPosts.Clear();
			}
		}

		#endregion

		/// <summary>
		/// Saves the specified writer.
		/// </summary>
		/// <param name="writer">The writer.</param>
		public void Save(GenericWriter writer)
		{
			writer.Write((int)2);//version
			//pla: v2
			writer.Write(m_Treasury);
			writer.Write(m_TaxRate);
			//pla: v1
			writer.Write(m_UnassignedGuardSlots);
			//pla: v0
			writer.Write((int)m_City);						//City
			writer.Write((int)m_ControlingKin);		//Kin
			writer.WriteDeltaTime(m_CaptureTime);
			writer.Write(m_CityLeader);
			writer.Write(m_IsVotingStage);
			writer.Write(m_Sigil);
			writer.Write(m_ControlPoints);
			writer.Write(m_ControlPointDelta);
			writer.Write((int)m_NPCFlags);
			writer.Write((int)m_GuardOption);
			writer.WriteDeltaTime(m_LastGuardChangeTime);

			writer.Write(m_BeneficiaryDataList.Count);
			foreach (BeneficiaryData data in m_BeneficiaryDataList)
				data.Serialize(writer);

		}


		//blank ctor
		public KinCityData()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KinCityData"/> class.
		/// </summary>
		/// <param name="city">The city.</param>
		public KinCityData(KinFactionCities city)
		{
			this.m_City = city;
			this.m_CaptureTime = DateTime.Now;
			this.m_ControlingKin = IOBAlignment.None;
			this.m_GuardOption = GuardOptions.LordBritish;
			this.SetAllNPCFlags();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KinCityData"/> class.
		/// </summary>
		/// <param name="reader">The reader.</param>
		public KinCityData(BinaryFileReader reader)
		{
			int version = reader.ReadInt();

			switch (version)
			{
				case 2:
					{
						m_Treasury = reader.ReadInt();
						m_TaxRate = reader.ReadDouble();
						goto case 1;
					}
				case 1:
					{
						m_UnassignedGuardSlots = reader.ReadInt();
						goto case 0;
					}
				case 0:
					{
						m_City = (KinFactionCities)reader.ReadInt();
						m_ControlingKin = (IOBAlignment)reader.ReadInt();
						m_CaptureTime = reader.ReadDeltaTime();
						m_CityLeader = (PlayerMobile)reader.ReadMobile();
						m_IsVotingStage = reader.ReadBool();
						m_Sigil = (KinSigil)reader.ReadItem();
						m_ControlPoints = reader.ReadInt();
						m_ControlPointDelta = reader.ReadInt();
						m_NPCFlags = (NPCFlags)reader.ReadInt();
						m_GuardOption = (GuardOptions)reader.ReadInt();
						m_LastGuardChangeTime = reader.ReadDeltaTime();

						int length = reader.ReadInt();

						if (length > 0)
							for (int i = 0; i < length; ++i)
								m_BeneficiaryDataList.Add(new BeneficiaryData(reader));

						break;
					}
			}
		}

		#endregion

		#region Beneficiary data

		/// <summary>
		/// Represents beneficiary data, including vote data, guard posts and slots
		/// </summary>
		public class BeneficiaryData
		{
			private PlayerMobile m_Pm = null;
			private int m_Votes = 0;
			private bool m_HasVoted = false;
			private List<KinGuardPost> m_GuardPosts = new List<KinGuardPost>();
			private int m_UnassignedGuardSlots = 0;

			/// <summary>
			/// Gets or sets the pm.
			/// </summary>
			/// <value>The pm.</value>
			public PlayerMobile Pm
			{
				get { return m_Pm; }
				set { m_Pm = value; }
			}

			/// <summary>
			/// Gets or sets the votes.
			/// </summary>
			/// <value>The votes.</value>
			public int Votes
			{
				get { return m_Votes; }
				set { m_Votes = value; }
			}

			/// <summary>
			/// Gets or sets a value indicating whether this instance has voted.
			/// </summary>
			/// <value><c>true</c> if this instance has voted; otherwise, <c>false</c>.</value>
			public bool HasVoted
			{
				get { return m_HasVoted; }
				set { m_HasVoted = value; }
			}

			/// <summary>
			/// Gets or sets the guard slots remaining.
			/// </summary>
			/// <value>The guard slots remaining.</value>
			public int UnassignedGuardSlots
			{
				get { return m_UnassignedGuardSlots; }
				set
				{
					if (value < 0)
						m_UnassignedGuardSlots = 0;
					else
						m_UnassignedGuardSlots = value;
				}
			}

			/// <summary>
			/// Gets the guard posts.
			/// </summary>
			/// <value>The guard posts.</value>
			public List<KinGuardPost> GuardPosts
			{
				get { return m_GuardPosts; }
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="BeneficiaryData"/> class.
			/// </summary>
			/// <param name="pm">The pm.</param>
			/// <param name="votes">The votes.</param>
			public BeneficiaryData(PlayerMobile pm, int votes)
			{
				m_Pm = pm;
				m_Votes = votes;
			}


			#region guards

			/// <summary>
			/// Modifies a PLAYER's slots within the beneficiary data - not the overall city's slots!
			/// </summary>
			/// <param name="pm"></param>
			/// <param name="amount"></param>
			public void ModifyGuardSlots(int amount)
			{
				//la sanity
				if (Pm == null || amount == 0) return;

				if (amount > 0)
				{
					RemoveGuardSlots(amount);
				}
				else
				{
					AssignGuardSlots(Math.Abs(amount));
				}
			}

			/// <summary>
			/// Modifies a PLAYER's slots within the beneficiary data - not the overall city's slots!
			/// </summary>
			/// <param name="pm"></param>
			/// <param name="amount"></param>
			/// <returns></returns>
			private bool RemoveGuardSlots(int amount)
			{
				//woooo sanity
				if (Pm == null || Pm.Deleted || amount <= 0) return false;

				//Commit changes
				UnassignedGuardSlots += amount;

				return true;
			}

			/// <summary>
			/// Modifies a PLAYER's slots within the beneficiary data - not the overall city's slots!
			/// </summary>
			/// <param name="pm"></param>
			/// <param name="amount"></param>
			/// <returns></returns>
			private bool AssignGuardSlots(int amount)
			{
				//woooo sanity
				if (Pm == null || Pm.Deleted || amount <= 0) return false;

				//If this is an add then make sure there are avaliable slots to assign
				if (amount > 0 && UnassignedGuardSlots < amount) return false;

				//Commit changes
				UnassignedGuardSlots -= amount;

				return true;
			}

			/// <summary>
			/// Attempts to register a guard post to this city
			/// </summary>
			/// <param name="gp"></param>
			/// <param name="pm"></param>
			/// <returns></returns>
			public bool RegisterGuardPost(KinGuardPost gp)
			{
				//wooooo sanity!
				if (gp == null || Pm == null || gp.Deleted || Pm.Deleted) return false;

				//check the player has enough free guard slots to register this sort of guard
				int guardCost = (int)gp.CostType;
				if (guardCost <= UnassignedGuardSlots)
				{
					//all good,	 register this guard post and decrement slots as required
					GuardPosts.Add(gp);
					AssignGuardSlots(guardCost);
				}
				else
				{
					return false;
				}
				return true;
			}

			/// <summary>
			/// Attempts to unregister a guard post to this city
			/// </summary>
			/// <param name="gp"></param>
			/// <param name="pm"></param>
			/// <returns></returns>
			public bool UnRegisterGuardPost(KinGuardPost gp)
			{
				//wooooo sanity!
				if (gp == null || Pm == null || gp.Deleted || Pm.Deleted) return false;
				//Check the guard post is registred with this char
				if (!GuardPosts.Contains(gp)) return false;
				int guardCost = (int)GuardPosts[GuardPosts.IndexOf(gp)].CostType;
				//Remove guard post and increment slots
				GuardPosts.Remove(gp);
				RemoveGuardSlots(guardCost);
				return true;
			}
			#endregion

			/// <summary>
			/// Serial ctor
			/// </summary>
			/// <param name="reader"></param>
			public BeneficiaryData(GenericReader reader)
			{
				int version = reader.ReadInt();

				switch (version)
				{
					case 1:
						{
							m_UnassignedGuardSlots = reader.ReadInt();
							m_GuardPosts = reader.ReadItemList<KinGuardPost>();
							goto case 0;
						}
					case 0:
						{
							m_Pm = (PlayerMobile)reader.ReadMobile();
							m_Votes = reader.ReadInt();
							m_HasVoted = reader.ReadBool();
							break;
						}
				}
			}

			/// <summary>
			/// Serializes the specified writer.
			/// </summary>
			/// <param name="writer">The writer.</param>
			public void Serialize(GenericWriter writer)
			{
				writer.Write(1); //version

				//pla: v1
				writer.Write(m_UnassignedGuardSlots);
				writer.WriteItemList<KinGuardPost>(m_GuardPosts);
				//pla: v0
				writer.Write(m_Pm);
				writer.Write(m_Votes);
				writer.Write(m_HasVoted);
			}

		}

		/// <summary>
		/// Allows BeneficiaryData to be sorted
		/// </summary>
		public class BeneficiaryDataComparer : IComparer<KinCityData.BeneficiaryData>
		{
			/// <summary>
			/// 
			/// </summary>
			public enum SortFields
			{
				/// <summary>
				/// 
				/// </summary>
				Name,
				/// <summary>
				/// 
				/// </summary>
				Votes
			}

			private SortFields m_SortByField = SortFields.Name;
			private bool m_Descending = false;

			/// <summary>
			/// Initializes a new instance of the <see cref="BeneficiaryDataComparer"/> class.
			/// </summary>
			/// <param name="sortByField">The sort by field.</param>
			/// <param name="descending">if set to <c>true</c> [descending].</param>
			public BeneficiaryDataComparer(SortFields sortByField, bool descending)
			{
				m_SortByField = sortByField;
				m_Descending = descending;
			}

			/// <summary>
			/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
			/// </summary>
			/// <param name="x">The first object to compare.</param>
			/// <param name="y">The second object to compare.</param>
			/// <returns>
			/// </returns>
			public int Compare(KinCityData.BeneficiaryData x, KinCityData.BeneficiaryData y)
			{
				//Sort desc by whatever field was selected
				switch (m_SortByField)
				{
					case SortFields.Name:
						{
							if (m_Descending)
								return -x.Votes.CompareTo(y.Pm.Name);
							else
								return x.Votes.CompareTo(y.Pm.Name);
						}
					case SortFields.Votes:
						{
							if (m_Descending)
								return -x.Votes.CompareTo(y.Votes);
							else
								return x.Votes.CompareTo(y.Votes);
						}
				}
				return 0;
			}
		}

		#endregion

	}

	#endregion

}
