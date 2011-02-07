﻿using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace DungeonEye.EventScript
{
	public class ScriptDisableTarget : IScriptAction
	{

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool Run()
		{


			return true;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		public bool Load(XmlNode xml)
		{
			return true;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		/// <returns></returns>
		public bool Save(XmlWriter writer)
		{
			return true;
		}




		#region Properties


		/// <summary>
		/// 
		/// </summary>
		public string Name
		{
			get
			{
				return "Disable Target";
			}
		}

		#endregion
	}
}