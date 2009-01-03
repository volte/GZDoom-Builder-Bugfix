
#region ================== Copyright (c) 2007 Pascal vd Heiden

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion

#region ================== Namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.Config;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	public abstract class BaseClassicMode : ClassicMode
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Properties

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public BaseClassicMode()
		{
			// Initialize

			// We have no destructor
			GC.SuppressFinalize(this);
		}

		// Disposer
		public override void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Clean up

				// Done
				base.Dispose();
			}
		}

		#endregion

		#region ================== Methods

		// This occurs when the user presses Copy. All selected geometry must be marked for copying!
		public override bool OnCopyBegin()
		{
			General.Map.Map.MarkAllSelectedGeometry(true, false);

			// Return true when anything is selected so that the copy continues
			// We only have to check vertices for the geometry, because without selected
			// vertices, no complete structure can exist.
			return (General.Map.Map.GetMarkedVertices(true).Count > 0) ||
				   (General.Map.Map.GetMarkedThings(true).Count > 0);
		}

		// This is called when something is pasted.
		public override void OnPasteEnd()
		{
			General.Map.Map.ClearAllSelected();
			General.Map.Map.SelectMarkedGeometry(true, true);

			// Switch to EditSelectionMode
			EditSelectionMode editmode = new EditSelectionMode();
			editmode.Pasting = true;
			General.Editing.ChangeMode(editmode);
		}

		#endregion

		#region ================== Actions

		[BeginAction("placevisualstart")]
		public void PlaceVisualStartThing()
		{
			bool onefound = false;
			
			// Not during volatile mode
			if(this.Attributes.Volatile) return;
			
			// Mouse must be inside window
			if(!mouseinside) return;
			
			// Go for all things
			List<Thing> things = new List<Thing>(General.Map.Map.Things);
			foreach(Thing t in things)
			{
				if(t.Type == General.Map.Config.Start3DModeThingType)
				{
					if(!onefound)
					{
						// Move this thing
						t.Move(mousemappos);
						onefound = true;
					}
					else
					{
						// One was already found and moved, delete this one
						t.Dispose();
					}
				}
			}
			
			// No thing found?
			if(!onefound)
			{
				// Make a new one
				Thing t = General.Map.Map.CreateThing();
				t.Type = General.Map.Config.Start3DModeThingType;
				t.Move(mousemappos);
				t.UpdateConfiguration();
				General.Map.ThingsFilter.Update();
			}

			// Redraw display to show changes
			General.Interface.RedrawDisplay();
		}

		#endregion
	}
}
