﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DungeonEye.Script.Actions;

namespace DungeonEye.Forms
{
	/// <summary>
	/// 
	/// </summary>
	public partial class ScriptTeleportControl : ScriptActionControlBase
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="script"></param>
		public ScriptTeleportControl(ScriptTeleport script, Dungeon dungeon)
		{
			InitializeComponent();


			if (script != null)
				Action = script;
			else
				Action = new ScriptTeleport();

			TargetBox.Dungeon = dungeon;
			TargetBox.SetTarget(((ScriptTeleport)Action).Target);

			TargetBox.TargetChanged +=new TargetControl.TargetChangedEventHandler(TargetBox_TargetChanged);
			
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="target"></param>
		void TargetBox_TargetChanged(object sender, DungeonLocation target)
		{
			((ScriptTeleport) Action).Target = target;
		}


		#region Events



		#endregion



		#region Properties

		/// <summary>
		/// Target location
		/// </summary>
		public DungeonLocation Target
		{
			get;
			private set;
		}

		#endregion

	}
}