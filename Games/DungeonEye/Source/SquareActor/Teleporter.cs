﻿#region Licence
//
//This file is part of ArcEngine.
//Copyright (C)2008-2010 Adrien Hémery ( iliak@mimicprod.net )
//
//ArcEngine is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//any later version.
//
//ArcEngine is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
//
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml;
using ArcEngine.Graphic;

namespace DungeonEye
{
	/// <summary>
	/// Teleporter object
	/// </summary>
	public class Teleporter : SquareActor
	{

		/// <summary>
		/// 
		/// </summary>
		public Teleporter(Square block) : base(block)
		{
			if (block == null)
				throw new ArgumentNullException("block");

			AcceptItems = true;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("Teleporter (target " + Target + ")");

			return sb.ToString();
		}

		#region I/O


		/// <summary>
		/// 
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		public override bool Load(XmlNode xml)
		{
			if (xml == null)
				return false;
			
			if (xml.Name.ToLower() != "teleporter")
				return false;

			foreach (XmlNode node in xml)
			{
				switch (node.Name.ToLower())
				{
					case "target":
					{
						Target = new DungeonLocation(Square.Location);
						Target.Load(node);
					}
					break;
				}

			}

			return true;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		/// <returns></returns>
		public override bool Save(XmlWriter writer)
		{
			if (writer == null)
				return false;
			

			writer.WriteStartElement("teleporter");
			Target.Save("target", writer);
			writer.WriteEndElement();

			return true;
		}



		#endregion


		#region Script

		/// <summary>
		/// 
		/// </summary>
		/// <param name="team"></param>
		/// <returns></returns>
		public override bool OnTeamEnter(Team team)
		{
			if (team == null)
				return false;

			if (Target == null)
				return false;

			team.Teleport(Target);

			return true;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="monster"></param>
		/// <returns></returns>
		public override bool OnMonsterEnter(Monster monster)
		{
			if (monster == null)
				return false;

			if (Target == null)
				return false;

			monster.Teleport(Target);

			return true;
		}


		#endregion


		#region Properties


		/// <summary>
		/// 
		/// </summary>
		public override bool IsBlocking
		{
			get
			{
				return false;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		public override bool CanPassThrough
		{
			get
			{
				return true;
			}
		}
	
		/// <summary>
		/// Target of the stair
		/// </summary>
		public DungeonLocation Target
		{
			get;
			set;
		}


		/// <summary>
		/// Name of the sound to play
		/// </summary>
		public string SoundName
		{
			get;
			set;
		}


		/// <summary>
		/// Silent teleporter
		/// </summary>
		public bool UseSound
		{
			get;
			set;
		}


		/// <summary>
		/// Scope
		/// </summary>
		public TeleporterScope Scope
		{
			get;
			set;
		}

		#endregion


	}


	/// <summary>
	/// Affected object by a teleporter
	/// </summary>
	[Flags]
	public enum TeleporterScope
	{
		/// <summary>
		/// Everything
		/// </summary>
		Everything,

		/// <summary>
		/// Only items
		/// </summary>
		Items,

		/// <summary>
		/// Only monsters
		/// </summary>
		Monsters,

		/// <summary>
		/// Only the team
		/// </summary>
		Team,

	}
}