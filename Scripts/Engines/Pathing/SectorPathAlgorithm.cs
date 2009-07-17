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
 * 	Technical Data and Computer Software clause at DFARS 252.227-7all013.
 *
 *	Angel Island UO Shard	Version 1.0
 *			Release A
 *			March 25, 2004
 */

/* Scripts/Engines/Pathing/SectorPathAlgorithm.cs
 * CHANGELOG
 *  11/14/06, Kit
 *      Add ( ) around bitshifts in for loops, for vc2005 comptability.
 *	7/03/06, Adam
 *		Rename Exception e to Exception ex to avoid compiler error.
 *  6/13/06 Taran Kain
 *		Cleaned up interface
 *		Cleaned up code, sanity checks etc
 *		Verified that SN network is exported to and read from the same location.
 *	6/13/06, Adam
 *		- remove requirement for TestCenter.Enabled. This conditional still allowed access to uninit
 *			data on production servers crashing them.
 *		- Wrap SameIsland with a try/catch as it was crashing on prod even though we had a null check
 *			for m_Nodes
 *  06/07/06, Kit
 *		Fixed null bug with SameIsland and SectorPath not being initialized which would crash server.
 *	06/02/06 Taran Kain
 *		Fixed bug in A* implementation
 *		Changed open list to use binary heap
 *		Added distance limit, island checks
 *	05/15/06 Taran Kain
 *		Public beta.
 *		Added TC checks to ensure none of this shit runs on Prod.
 */

using System;
using System.Collections;
using Server;
using Server.PathAlgorithms;
using Server.Targeting;
using System.IO;
using System.Drawing;
using Server.Scripts.Commands;

namespace Server.PathAlgorithms.Sector
{
	/// <summary>
	/// Summary description for SectorPathAlgorithm.
	/// </summary>
	public class SectorPathAlgorithm : PathAlgorithm
	{
		private class SectorNode
		{
			public Point3D Point;
			public int NumLinks;
			public SectorNode[] Links;
			public int[] Distances;
			public int Island;

			public void Serialize(GenericWriter writer)
			{
				writer.Write(Point);
				writer.Write(Island);
				writer.Write(NumLinks);
				for (int i = 0; i < NumLinks; i++)
				{
					writer.Write(Links[i].Point.X >> Map.SectorShift);
					writer.Write(Links[i].Point.Y >> Map.SectorShift);
					writer.Write(Distances[i]);
				}
			}

			public void Deserialize(GenericReader reader)
			{
				Point = reader.ReadPoint3D();
				Island = reader.ReadInt();
				NumLinks = reader.ReadInt();
				
				Links = new SectorNode[8];
				Distances = new int[8];

				for (int i = 0; i < NumLinks; i++)
				{
					int x = reader.ReadInt(), y = reader.ReadInt();
					Links[i] = m_Nodes[x, y];
					Distances[i] = reader.ReadInt();
				}
			}
		}

		private static SectorNode[,] m_Nodes;

		public static void Configure()
		{	
			EventSink.WorldLoad += new WorldLoadEventHandler(EventSink_WorldLoad);
		}
		
		public static void Initialize()
		{	
			Server.Commands.Register("BuildSectorNodeNetwork", AccessLevel.Administrator, new CommandEventHandler(BuildSectorNodeNetwork));
			Server.Commands.Register("ExportSectorNodeNetwork", AccessLevel.Administrator, new CommandEventHandler(ExportSectorNodeNetwork));
		}

		public static void BuildSectorNodeNetwork(CommandEventArgs e)
		{
			if (m_Nodes == null)
			{
				try
				{
					Console.Write("Initializing SectorNodes...");
					DateTime dt = DateTime.Now;

					m_Nodes = new SectorNode[Map.Felucca.Width >> Map.SectorShift, Map.Felucca.Height >> Map.SectorShift];

					for (int y = 0; y < (Map.Felucca.Height >> Map.SectorShift); y++)
					{
						for (int x = 0; x < (Map.Felucca.Width >> Map.SectorShift); x++)
						{
							SectorNode sn = new SectorNode();
							sn.Point = Point3D.Zero;
							sn.Island = -1;
							sn.Links = new SectorNode[8];
							sn.Distances = new int[8];
							sn.NumLinks = 0;

							for (int sy = 0; sy < Map.SectorSize && sn.Point == Point3D.Zero; sy++)
							{
								for (int sx = 0; sx < Map.SectorSize && sn.Point == Point3D.Zero; sx++)
								{
									if (Map.Felucca.CanSpawnMobile((x << Map.SectorShift) + sx, (y << Map.SectorShift) + sy, 
										Map.Felucca.GetAverageZ((x << Map.SectorShift) + sx, (y << Map.SectorShift) + sy)))
									{
										sn.Point = new Point3D((x << Map.SectorShift) + sx, (y << Map.SectorShift) + sy, 
											Map.Felucca.GetAverageZ((x << Map.SectorShift) + sx, (y << Map.SectorShift) + sy));
									}
								}
							}

							m_Nodes[x, y] = sn;
						}
					}

					Console.WriteLine("done in {0} seconds.", (DateTime.Now - dt).TotalSeconds);
					Console.Write("Computing SectorNode network...");
					dt = DateTime.Now;

					Mobile m = new Server.Mobiles.WanderingHealer();
					MovementPath mp = null;
			
					for (int y = 0; y < (Map.Felucca.Height >> Map.SectorShift); y++)
					{
						for (int x = 0; x < (Map.Felucca.Width >> Map.SectorShift); x++)
						{
							if (m_Nodes[x, y].Point != Point3D.Zero)
							{
								m.MoveToWorld(m_Nodes[x, y].Point, Map.Felucca);

								if (x < (Map.Felucca.Width >> Map.SectorShift) - 1 && y > 0 && m_Nodes[x + 1, y - 1].Point != Point3D.Zero &&
									(mp = new MovementPath(m, m_Nodes[x + 1, y - 1].Point)).Success)
								{
									m_Nodes[x, y].Links[m_Nodes[x, y].NumLinks] = m_Nodes[x + 1, y - 1];
									m_Nodes[x, y].Distances[m_Nodes[x, y].NumLinks] = mp.Directions.Length;
									m_Nodes[x, y].NumLinks++;

									m_Nodes[x + 1, y - 1].Links[m_Nodes[x + 1, y - 1].NumLinks] = m_Nodes[x, y];
									m_Nodes[x + 1, y - 1].Distances[m_Nodes[x + 1, y - 1].NumLinks] = mp.Directions.Length;
									m_Nodes[x + 1, y - 1].NumLinks++;
								}
						
								if (x < (Map.Felucca.Width >> Map.SectorShift) - 1 && m_Nodes[x + 1, y].Point != Point3D.Zero &&
									(mp = new MovementPath(m, m_Nodes[x + 1, y].Point)).Success)
								{
									m_Nodes[x, y].Links[m_Nodes[x, y].NumLinks] = m_Nodes[x + 1, y];
									m_Nodes[x, y].Distances[m_Nodes[x, y].NumLinks] = mp.Directions.Length;
									m_Nodes[x, y].NumLinks++;

									m_Nodes[x + 1, y].Links[m_Nodes[x + 1, y].NumLinks] = m_Nodes[x, y];
									m_Nodes[x + 1, y].Distances[m_Nodes[x + 1, y].NumLinks] = mp.Directions.Length;
									m_Nodes[x + 1, y].NumLinks++;
								}

								if (x < (Map.Felucca.Width >> Map.SectorShift) - 1 && y < (Map.Felucca.Height >> Map.SectorShift) - 1 &&
									m_Nodes[x + 1, y + 1].Point != Point3D.Zero && (mp = new MovementPath(m, m_Nodes[x + 1, y + 1].Point)).Success)
								{
									m_Nodes[x, y].Links[m_Nodes[x, y].NumLinks] = m_Nodes[x + 1, y + 1];
									m_Nodes[x, y].Distances[m_Nodes[x, y].NumLinks] = mp.Directions.Length;
									m_Nodes[x, y].NumLinks++;

									m_Nodes[x + 1, y + 1].Links[m_Nodes[x + 1, y + 1].NumLinks] = m_Nodes[x, y];
									m_Nodes[x + 1, y + 1].Distances[m_Nodes[x + 1, y + 1].NumLinks] = mp.Directions.Length;
									m_Nodes[x + 1, y + 1].NumLinks++;
								}

								if (y < (Map.Felucca.Height >> Map.SectorShift) - 1 && m_Nodes[x, y + 1].Point != Point3D.Zero &&
									(mp = new MovementPath(m, m_Nodes[x, y + 1].Point)).Success)
								{
									m_Nodes[x, y].Links[m_Nodes[x, y].NumLinks] = m_Nodes[x, y + 1];
									m_Nodes[x, y].Distances[m_Nodes[x, y].NumLinks] = mp.Directions.Length;
									m_Nodes[x, y].NumLinks++;

									m_Nodes[x, y + 1].Links[m_Nodes[x, y + 1].NumLinks] = m_Nodes[x, y];
									m_Nodes[x, y + 1].Distances[m_Nodes[x, y + 1].NumLinks] = mp.Directions.Length;
									m_Nodes[x, y + 1].NumLinks++;
								}
							}
						}
					}

					m.Delete();

					Console.WriteLine("done in {0} seconds.", (DateTime.Now - dt).TotalSeconds);
					Console.Write("Finding islands...");
					dt = DateTime.Now;

					int nextIsland = 0;
					Queue open = new Queue();
					ArrayList closed = new ArrayList();

					for (int y = 0; y < (Map.Felucca.Height >> Map.SectorShift); y++)
					{
						for (int x = 0; x < (Map.Felucca.Width >> Map.SectorShift); x++)
						{
							if (m_Nodes[x, y].Point == Point3D.Zero)
								continue;

							if (m_Nodes[x, y].Island == -1)
							{
								int island = nextIsland++;

								// now use dijkstra-style flood fill to find all connected nodes
								open.Clear();
								closed.Clear();

								open.Enqueue(m_Nodes[x, y]);

								while (open.Count > 0)
								{
									SectorNode sn = (SectorNode)open.Dequeue();
									closed.Add(sn);

									sn.Island = island;

									for (int i = 0; i < sn.NumLinks; i++)
										if (!closed.Contains(sn.Links[i]) && !open.Contains(sn.Links[i]))
											open.Enqueue(sn.Links[i]);
								}
							}
						}
					}

					Console.WriteLine("done in {0} seconds.", (DateTime.Now - dt).TotalSeconds);
				}
				catch (Exception ex)
				{
					LogHelper.LogException(ex);
					Console.WriteLine("error!");
					Console.WriteLine(ex);
				}
			}
		}

		private static void EventSink_WorldLoad()
		{
			try
			{
				Console.Write("Loading SectorNodes...");
				DateTime dt = DateTime.Now;
				using (FileStream fs = new FileStream("Data/SectorPathData.dat", FileMode.Open))
				{
					using (BinaryReader br = new BinaryReader(fs))
					{
						BinaryFileReader reader = new BinaryFileReader(br);

						if (reader.ReadInt() != (Map.Felucca.Width >> Map.SectorShift))
							throw new Exception("SectorNode data has different width than current map.");
						if (reader.ReadInt() != (Map.Felucca.Height >> Map.SectorShift))
							throw new Exception("SectorNode data has different height than current map.");

						m_Nodes = new SectorNode[Map.Felucca.Width >> Map.SectorShift, Map.Felucca.Height >> Map.SectorShift];

						for (int y = 0; y < (Map.Felucca.Height >> Map.SectorShift); y++)
						{
							for (int x = 0; x < (Map.Felucca.Width >> Map.SectorShift); x++)
							{
								m_Nodes[x, y] = new SectorNode();
							}
						}

						for (int y = 0; y < (Map.Felucca.Height >> Map.SectorShift); y++)
						{
							for (int x = 0; x < (Map.Felucca.Width >> Map.SectorShift); x++)
							{
								m_Nodes[x, y].Deserialize(reader);
							}
						}

						reader.Close();
					}
				}
				Console.WriteLine("done in {0}ms.", (DateTime.Now - dt).TotalMilliseconds);
			}
			catch (Exception e)
			{
				LogHelper.LogException(e);
				Console.WriteLine("error:");
				Console.WriteLine(e.Message);
				Console.WriteLine("SectorNode data must be recomputed.");
				m_Nodes = null;
			}
		}

		private static void ExportSectorNodeNetwork(CommandEventArgs e)
		{
			try
			{
				Console.Write("Saving SectorNodes...");
				DateTime dt = DateTime.Now;
				if (!Directory.Exists("Data"))
					Directory.CreateDirectory("Data");

				using (FileStream fs = new FileStream("Data/SectorPathData.dat", FileMode.Create))
				{
					BinaryFileWriter writer = new BinaryFileWriter(fs, false);

					writer.Write(Map.Felucca.Width >> Map.SectorShift);
					writer.Write(Map.Felucca.Height >> Map.SectorShift);

					for (int y = 0; y < (Map.Felucca.Height >> Map.SectorShift); y++)
					{
						for (int x = 0; x < (Map.Felucca.Width >> Map.SectorShift); x++)
						{
							m_Nodes[x, y].Serialize(writer);
						}
					}

					writer.Close();
				}
				Console.WriteLine("done in {0}ms.", (DateTime.Now - dt).TotalMilliseconds);
			}
			catch (Exception ex)
			{
				LogHelper.LogException(ex);
				Console.WriteLine("error:");
				Console.WriteLine(ex);
			}
		}

		public override bool CheckCondition(Mobile m, Map map, Point3D start, Point3D goal)
		{
			return SameIsland(start, goal);
		}

		public static bool SameIsland(Point3D start, Point3D goal)
		{
			try
			{
				if (m_Nodes == null)
					return false;

				return m_Nodes[start.X >> Map.SectorShift, start.Y >> Map.SectorShift].Island == m_Nodes[goal.X >> Map.SectorShift, goal.Y >> Map.SectorShift].Island;
			}
			catch
			{
				Console.WriteLine("WARNING: SectorNode network corrupted.");
				return false;
			}
		}

		private class SearchNode
		{
			public SectorNode SectorNode;
			public SearchNode Parent;
			public int G;
			public double Score;

			public SearchNode(SectorNode sn, SearchNode parent, int g, Point3D goal)
			{
				SectorNode = sn;
				Parent = parent;
				G = g;
				Score = g + Heuristic(goal);
			}

			public override bool Equals(object obj)
			{
				SearchNode sn = obj as SearchNode;
				if (sn == null)
					return false;

				return SectorNode == sn.SectorNode;
			}

			public override int GetHashCode()
			{
				return SectorNode.GetHashCode();
			}

			private double Heuristic(Point3D goal)
			{
				return Math.Abs(SectorNode.Point.X - goal.X) + Math.Abs(SectorNode.Point.Y - goal.Y);
			}
		}

		public override Direction[] Find(Mobile m, Map map, Point3D start, Point3D goal)
		{
			return null;
		}

		private class SearchNodeHeap
		{
			private SearchNode[] m_Heap;
			private int m_Count;

			public int Count
			{
				get
				{
					return m_Count;
				}
			}
    
			public SearchNodeHeap()
			{
				m_Heap = new SearchNode[16];
				m_Count = 0;
			}

			public void Add(SearchNode sn)
			{
				if (sn == null)
					return;

				int index = m_Count++, parent = (index - 1) / 2;
				SearchNode t;

				m_Heap[index] = sn;
    
				while (index > 0 && m_Heap[index].Score < m_Heap[parent].Score)
				{
					t = m_Heap[index];
					m_Heap[index] = m_Heap[parent];
					m_Heap[parent] = t;

					index = parent;
					parent = (index - 1) / 2;
				}

				if (m_Count == m_Heap.Length)
				{
					SearchNode[] resize = new SearchNode[m_Count * 2];
					Array.Copy(m_Heap, 0, resize, 0, m_Count);
					m_Heap = resize;
				}
			}

			public SearchNode Pop()
			{
				if (m_Count == 0)
					return null;

				SearchNode ret = m_Heap[0];

				m_Heap[0] = m_Heap[m_Count - 1];
				m_Heap[m_Count - 1] = null;
				m_Count--;

				PercolateDown(0);

				return ret;
			}

			public void Clear()
			{
				for (int i = 0; i < m_Heap.Length; i++)
					m_Heap[i] = null;
			}

			public bool ContainsMatch(SearchNode sn)
			{
				return IndexOf(sn) != -1;
			}

			private int IndexOf(SearchNode sn)
			{
				for (int i = 0; i < m_Count; i++)
					if (m_Heap[i].SectorNode == sn.SectorNode)
						return i;

				return -1;
			}

			public void Adjust(SearchNode sn)
			{
				int index = IndexOf(sn);
				if (index == -1)
					return;

				m_Heap[index] = sn;

				int parent = index;
				do
				{
					index = parent;
					PercolateDown(index);
					parent = (index - 1) / 2;
				} while (parent >= 0 && m_Heap[parent].Score > m_Heap[index].Score);
			}

			private void PercolateDown(int index)
			{
				int child = 2 * index + 1;
				SearchNode t;
				while (child < m_Count) // while we've got at least one child
				{
					// if there's a second child AND it's smaller than the first
					if (child + 1 < m_Count && m_Heap[child + 1].Score < m_Heap[child].Score)
						child += 1; // use the second child

					if (m_Heap[child].Score < m_Heap[index].Score)
					{
						t = m_Heap[child];
						m_Heap[child] = m_Heap[index];
						m_Heap[index] = t;

						index = child;
						child += child + 1; // child = index * 2 + 1, aka child = child + child + 1
					}
					else
						break; // no switch means we're done
				}
			}

			public SearchNode FindMatch(SearchNode sn)
			{
				int index = IndexOf(sn);
				if (index == -1)
					return null;

				return m_Heap[index];
			}
		}

		public static Point3D[] FindWaypoints(Mobile m, Map map, Point3D start, Point3D goal)
		{
			DateTime s = DateTime.Now;
			if (m_Nodes == null)
				return null; // sanity check

			if (!SameIsland(start, goal))
				return null;

			if (!Utility.InRange(start, goal, 512))
				return null;

			SearchNodeHeap open = new SearchNodeHeap();
			ArrayList closed = new ArrayList();

			SectorNode goalnode = m_Nodes[goal.X >> Map.SectorShift, goal.Y >> Map.SectorShift];

			open.Add(new SearchNode(m_Nodes[start.X >> Map.SectorShift, start.Y >> Map.SectorShift], null, 0, goal));

			while (open.Count > 0)
			{
				SearchNode curnode = open.Pop();
				closed.Add(curnode);

				if (curnode.SectorNode == goalnode)
					break;

				for (int i = 0; i < curnode.SectorNode.NumLinks; i++)
				{
					SearchNode newnode = new SearchNode(curnode.SectorNode.Links[i], curnode, curnode.G + curnode.SectorNode.Distances[i], goal);
					
					if (closed.Contains(newnode))
						continue;

					SearchNode existing = open.FindMatch(newnode);
					if (existing == null)
						open.Add(newnode);
					else if (newnode.G < existing.G)
						open.Adjust(newnode);
					// else skip
				}
			}

			SearchNode sn = (SearchNode)closed[closed.Count - 1];
			if (sn.SectorNode == goalnode)
			{
				Stack nodes = new Stack();
				while (sn != null)
				{
					closed.Remove(sn);
					nodes.Push(sn);
					sn = sn.Parent;
				}

				Point3D[] points = new Point3D[nodes.Count + 1];
				for (int i = 0; i < points.Length - 1; i++)
					points[i] = ((SearchNode)nodes.Pop()).SectorNode.Point;
				points[points.Length - 1] = goal;

				return points;
			}

			return null;
		}
	}
}
