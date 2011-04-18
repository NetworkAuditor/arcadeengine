﻿#region Licence
//
//This file is part of ArcEngine.
//Copyright (C)2008-2011 Adrien Hémery ( iliak@mimicprod.net )
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
using System.Drawing;
using System.Text;
using System.Xml;
using ArcEngine;
using ArcEngine.Graphic;
using ArcEngine.Input;
using DungeonEye.Gui;

namespace DungeonEye.Script
{
	/// <summary>
	/// 
	/// </summary>
	public class AlcoveScript : ScriptBase
	{

		public AlcoveScript()
		{

		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override bool Run()
		{
			return base.Run();
		}




		#region IO

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		public override bool Load(XmlNode xml)
		{
			if (xml == null)
				return false;


			foreach (XmlNode node in xml)
			{
				switch (node.Name)
				{
					case "condition":
					{
						Condition = (AlcoveCondition)Enum.Parse(typeof(AlcoveCondition), node.InnerText);
					}
					break;

					case "consume":
					{
						ConsumeItem = true;
					}
					break;

					case "item":
					{
						ItemName = node.InnerText;
					}
					break;

					default:
					{
						Trace.WriteLine("[ScriptBase] Load() : Unknown node \"" + node.Name + "\" found.");
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

			writer.WriteElementString("condition", Condition.ToString());


			if (ConsumeItem)
				writer.WriteValue("consume");

			writer.WriteElementString("item", ItemName);

			return base.Save(writer);
		}

		#endregion



		#region Properties


		/// <summary>
		/// Condition
		/// </summary>
		public AlcoveCondition Condition
		{
			get;
			set;
		}


		/// <summary>
		/// 
		/// </summary>
		public bool ConsumeItem
		{
			get;
			set;
		}


		/// <summary>
		/// 
		/// </summary>
		public string ItemName
		{
			get;
			set;
		}

		#endregion
	}
}