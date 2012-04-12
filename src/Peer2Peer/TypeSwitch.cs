/* 
    Peer 2 Peer Library
    Copyright (C) 2011 - 2012 Leonid Gordo

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace Peer2Peer
{
	static class TypeSwitch
	{
		public class CaseInfo
		{
			public bool IsDefault { get; set; }
			public Type Target { get; set; }
			public Action<object> Action { get; set; }
		}

		public static void Do(object source, params CaseInfo[] cases)
		{
			var type = source.GetType();
			foreach (var entry in cases)
				if (entry.IsDefault || type == entry.Target)
				{
					entry.Action(source);
					break;
				}
		}

		public static CaseInfo Case<T>(Action action)
		{
			return new CaseInfo
			{
				Action = x => action(),
				Target = typeof(T)
			};
		}

		public static CaseInfo Case<T>(Action<T> action)
		{
			return new CaseInfo
			{
				Action = x => action((T)x),
				Target = typeof(T)
			};
		}

		public static CaseInfo Default(Action action)
		{
			return new CaseInfo
			{
				Action = x => action(),
				IsDefault = true
			};
		}
	}
}
