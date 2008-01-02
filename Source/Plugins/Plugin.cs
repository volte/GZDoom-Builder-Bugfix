
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
using System.IO;
using System.Reflection;
using CodeImp.DoomBuilder.Controls;

#endregion

namespace CodeImp.DoomBuilder.Plugins
{
	internal class Plugin
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// The plugin assembly
		private Assembly asm;
		
		// Unique name used to refer to this assembly
		private string name;
		
		// Disposing
		private bool isdisposed = false;

		#endregion

		#region ================== Properties

		public Assembly Assembly { get { return asm; } }
		public string Name { get { return name; } }
		public bool IsDisposed { get { return isdisposed; } }

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public Plugin(string filename)
		{
			// Initialize
			name = Path.GetFileNameWithoutExtension(filename);
			General.WriteLogLine("Loading plugin '" + name + "' from '" + Path.GetFileName(filename) + "'...");
			
			// Load assembly
			asm = Assembly.LoadFile(filename);
			
			// Load actions
			General.Actions.LoadActions(asm);
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}

		// Disposer
		public void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Clean up
				asm = null;
				
				// Done
				isdisposed = true;
			}
		}

		#endregion

		#region ================== Methods

		// This creates a stream to read a resource or returns null when not found
		public Stream FindResource(string resourcename)
		{
			string[] resnames;
			
			// Find a resource
			resnames = asm.GetManifestResourceNames();
			foreach(string rn in resnames)
			{
				// Found it?
				if(rn.EndsWith(resourcename, StringComparison.InvariantCultureIgnoreCase))
				{
					// Get a stream from the resource
					return asm.GetManifestResourceStream(rn);
				}
			}

			// Nothing found
			return null;
		}
		
		// This finds all class types that inherits from the given type
		public Type[] FindClasses(Type t)
		{
			List<Type> found = new List<Type>();
			Type[] types;
			
			// Get all exported types
			types = asm.GetExportedTypes();
			foreach(Type it in types)
			{
				// Compare types
				if(t.IsAssignableFrom(it)) found.Add(it);
			}

			// Return list
			return found.ToArray();
		}

		// This finds a single class type that inherits from the given type
		// Returns null when no valid type was found
		public Type FindSingleClass(Type t)
		{
			Type[] types = FindClasses(t);
			if(types.Length > 0) return types[0]; else return null;
		}
		
		// This creates an instance of a class
		public T CreateObject<T>(Type t, params object[] args)
		{
			return CreateObjectA<T>(t, args);
		}

		// This creates an instance of a class
		public T CreateObjectA<T>(Type t, object[] args)
		{
			try
			{
				// Create instance
				return (T)asm.CreateInstance(t.FullName, false, BindingFlags.Default, null, args, CultureInfo.CurrentCulture, new object[0]);
			}
			catch(TargetInvocationException e)
			{
				// Error!
				General.WriteLogLine("ERROR: Failed to create class instance '" + t.Name + "' from plugin '" + name + "'!");
				General.WriteLogLine(e.InnerException.GetType().Name + ": " + e.InnerException.Message);
				return default(T);
			}
			catch(Exception e)
			{
				// Error!
				General.WriteLogLine("ERROR: Failed to create class instance '" + t.Name + "' from plugin '" + name + "'!");
				General.WriteLogLine(e.GetType().Name + ": " + e.Message);
				return default(T);
			}
		}

		#endregion
	}
}
