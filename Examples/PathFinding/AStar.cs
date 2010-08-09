﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;


// http://www.policyalmanac.org/games/aStarTutorial.htm
// http://www.siteduzero.com/tutoriel-3-34333-le-pathfinding-avec-a.html
// http://www.3dbuzz.com/vbforum/showthread.php?176912-Wolf-s-A*-pathfinding-tutorial.
// http://wiki.gamegardens.com/Path_Finding_Tutorial
//
//
//

namespace ArcEngine.Examples.PathFinding
{
	/// <summary>
	/// 
	/// </summary>
	public class AStar
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="size">Size of the grid</param>
		public AStar(Size size)
		{
			OpenQueue = new PriorityQueue<PathNode>();

			GridSize = size;

			Nodes = new PathNode[GridSize.Width * GridSize.Height];
			for(int y = 0; y < size.Height; y++)
				for (int x = 0 ; x < size.Width ; x++)
				{
					Nodes[y * GridSize.Width + x] = new PathNode(new Point(x, y));
				}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		public List<PathNode> FindPath(Point start, Point end)
		{

			Clear();
			
			// Add root node
			PathNode startnode = GetNode(start);
			startnode.Parent = null;
			startnode.IsOpen = false;
			OpenQueue.Push(startnode);
			

			


			PathNode dest = GetNode(end);

			int MovementCost = 10;



			while (OpenQueue.Count > 0)
			{
				// No path...
				if (OpenQueue.Count == 0)
				{
					OpenQueue.Clear();
					return null;
				}


				// Get first node
				PathNode parent = OpenQueue.Pop();
			
				// Destination reached ?
				if (parent == dest)
				{
					return GetPath(parent);
				}

				PathNode node = null;

				// Top
				node = GetNode(parent.Location.X, parent.Location.Y - 1);
				if (node != null && node.IsOpen && node.IsWalkable)
				{
					node.Parent = parent;
					node.G = parent.G + MovementCost;
					node.H = GetHeuristic(node.Location, end);
					node.IsOpen = false;
					OpenQueue.Push(node);
				}

				// Right
				node = GetNode(parent.Location.X + 1, parent.Location.Y);
				if (node != null && node.IsOpen && node.IsWalkable)
				{
					node.Parent = parent;
					node.G = parent.G + MovementCost;
					node.H = GetHeuristic(node.Location, end);
					node.IsOpen = false;
					OpenQueue.Push(node);
				}


				// Bottom
				node = GetNode(parent.Location.X, parent.Location.Y + 1);
				if (node != null && node.IsOpen && node.IsWalkable)
				{
					node.Parent = parent;
					node.G = parent.G + MovementCost;
					node.H = GetHeuristic(node.Location, end);
					node.IsOpen = false;
					OpenQueue.Push(node);
				}

				// Left
				node = GetNode(parent.Location.X - 1, parent.Location.Y);
				if (node != null && node.IsOpen && node.IsWalkable)
				{
					node.Parent = parent;
					node.G = parent.G + MovementCost;
					node.H = GetHeuristic(node.Location, end);
					node.IsOpen = false;
					OpenQueue.Push(node);
				}


			}

			return null;
		}


		/// <summary>
		/// Clear
		/// </summary>
		public void Clear()
		{

			// Clear the queue
			OpenQueue.Clear();


			// Clear nodes
			foreach (PathNode node in Nodes)
			{
				node.Clear();
			}



		}


		/// <summary>
		/// Get the past
		/// </summary>
		/// <returns></returns>
		List<PathNode> GetPath(PathNode node)
		{
			List<PathNode> path = new List<PathNode>();

			while (node.Parent != null)
			{
				path.Add(node);

				node = node.Parent;
			}


			return path;
		}

		/// <summary>
		/// Gets heuristic value
		/// </summary>
		/// <param name="start">Current position</param>
		/// <param name="destination">Destination</param>
		/// <returns></returns>
		int GetHeuristic(Point start, Point destination)
		{
			// Manhattan distance
			return Math.Abs(start.X - destination.X) + Math.Abs(start.Y - destination.Y);
		}


		/// <summary>
		/// Gets a node
		/// </summary>
		/// <param name="location">Location</param>
		/// <returns></returns>
		public PathNode GetNode(Point location)
		{
			return GetNode(location.X, location.Y);
		}


		/// <summary>
		/// Gets a node
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <returns></returns>
		public PathNode GetNode(int x, int y)
		{
			if (x >= GridSize.Width || y >= GridSize.Height ||
				x < 0 || y < 0)
				return null;

			return Nodes[y * GridSize.Width + x];
		}

		#region Properties

		/// <summary>
		/// 
		/// </summary>
		PriorityQueue<PathNode> OpenQueue;


		/// <summary>
		/// Size of the grid
		/// </summary>
		public Size GridSize
		{
			get;
			private set;
		}

		/// <summary>
		/// 
		/// </summary>
		Rectangle Rectangle
		{
			get
			{
				return new Rectangle(Point.Empty, GridSize);
			}
		}

		/// <summary>
		/// Nodes
		/// </summary>
		PathNode[] Nodes;

		#endregion
	}
}
