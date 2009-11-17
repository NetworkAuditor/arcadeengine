﻿#region Licence
//
//This file is part of ArcEngine.
//Copyright (C)2008-2009 Adrien Hémery ( iliak@mimicprod.net )
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

using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ArcEngine.Graphic
{
	/// <summary>
	/// An efficient way to render a batch of geometry
	/// </summary>
	/// http://songho.ca/opengl/gl_vbo.html
	public class Batch : IDisposable
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public Batch()
		{
			BufferID = new int[3];

			if (Display.Capabilities.HasVBO)
			{
				GL.GenBuffers(3, BufferID);
			}
			else
			{
			}


			VertexBuffer = new List<Point>();
			TextureBuffer = new List<Point>();
			ColorBuffer = new List<int>();
		}


		/// <summary>
		/// Destructor
		/// </summary>
		~Batch()
		{
			Dispose(false);
		}

		/// <summary>
		/// Clear the batch
		/// </summary>
		public void Clear()
		{
			VertexBuffer.Clear();
			ColorBuffer.Clear();
			TextureBuffer.Clear();
		}




		/// <summary>
		/// Apply updates
		/// </summary>
		public void Apply()
		{
			if (!Display.Capabilities.HasVBO)
				return;

			try
			{
				// Update Vertex buffer
				GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID[0]);
				GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(VertexBuffer.Count * sizeof(int) * 2), VertexBuffer.ToArray(), BufferUsageHint.StaticDraw);

				// Update Texture buffer
				GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID[1]);
				GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(TextureBuffer.Count * sizeof(int) * 2), TextureBuffer.ToArray(), BufferUsageHint.StaticDraw);

				// Update Color buffer
				GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID[2]);
				GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(ColorBuffer.Count * sizeof(int)), ColorBuffer.ToArray(), BufferUsageHint.StaticDraw);

			}
			catch (Exception e)
			{
				bool er = GL.IsBuffer(BufferID[0]);
				Trace.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
			}


		}


		/// <summary>
		/// Adds a rectangle
		/// </summary>
		/// <param name="rect">Rectangle on the screen</param>
		/// <param name="color">Drawing color</param>
		/// <param name="tex">Texture coordinate</param>
		public void AddRectangle(Rectangle rect, Color color, Rectangle tex)
		{
			AddPoint(rect.Location, color, tex.Location);
			AddPoint(new Point(rect.Right, rect.Top), color, new Point(tex.Right, tex.Top));
			AddPoint(new Point(rect.Right, rect.Bottom), color, new Point(tex.Right, tex.Bottom));
			AddPoint(new Point(rect.X, rect.Bottom), color, new Point(tex.X, tex.Bottom));
		}


		/// <summary>
		/// Adds a point
		/// </summary>
		/// <param name="point">Location on the screen</param>
		/// <param name="color">Color of the point</param>
		/// <param name="texture">Texture coordinate</param>
		public void AddPoint(Point point, Color color, Point texture)
		{
			VertexBuffer.Add(point);
			TextureBuffer.Add(texture);

			ColorBuffer.Add((color.A << 24) + (color.B << 16) + (color.G << 8) + (color.R));
		}


		/// <summary>
		/// Adds a point
		/// </summary>
		/// <param name="point">Location on the screen</param>
		/// <param name="color">Color of the point</param>
		public void AddPoint(Point point, Color color)
		{
			AddPoint(point, color, Point.Empty);
		}


		/// <summary>
		/// Adds a line
		/// </summary>
		/// <param name="from">Start point</param>
		/// <param name="to">Ending point</param>
		/// <param name="color">Color of the line</param>
		public void AddLine(Point from, Point to, Color color)
		{
			AddPoint(from, color);
			AddPoint(to, color);
		}



		#region Disposing

		/// <summary>
		/// 
		/// </summary>
		bool disposed;



		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the
		// runtime from inside the finalizer and you should not reference
		// other objects. Only unmanaged resources can be disposed.
		private void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if (!this.disposed)
			{
				if (Display.Capabilities.HasVBO)
				{
					//GL.DeleteBuffers(3, BufferID);
				}
				BufferID[0] = -1;
				BufferID[1] = -1;
				BufferID[2] = -1;

				// Note disposing has been done.
				disposed = true;
			}
		}


		#endregion


		#region Properties


		/// <summary>
		/// Gets the size of the buffer
		/// </summary>
		public int Size
		{
			get
			{
				return VertexBuffer.Count;
			}
		}


		/// <summary>
		/// Buffer ID
		/// </summary>
		/// <remarks>Buffer 0 => Vertex
		/// Buffer 1 => Textures
		/// Buffer 2 => Color</remarks>
		internal int[] BufferID
		{
			get;
			private set;
		}

		
		/// <summary>
		/// Vertex buffer
		/// </summary>
		internal List<Point> VertexBuffer;
		
		/// <summary>
		/// Texture buffer
		/// </summary>
		internal List<Point> TextureBuffer;

		/// <summary>
		/// Color buffer
		/// </summary>
		internal List<int> ColorBuffer;

		#endregion

	}
}
