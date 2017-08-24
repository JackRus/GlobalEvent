using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GlobalEvent.Models
{
	public class JackLib
	{
		// Copies the matching properties from one object to another.
		// Will override all matching properties (by Name)!!!
		// Dont' use when some properties shouln't be changed.  

		public static void SetValues(object from, object to)
		{
			foreach (var p in from.GetType().GetProperties())
			{
				if (to.GetType().GetProperty(p.Name) != null)
				{
					to.GetType().GetProperty(p.Name).GetSetMethod().Invoke(
						to, new[] { p.GetGetMethod().Invoke(from, null) }
					);
				}
			}
		}

		// FULL VERSION:
		// public static void SetValues(object from, object to)
		// {
		//     var toType = to.GetType();
		//     foreach (var prop in from.GetType().GetProperties())
		//     {
		//         if (toType.GetProperty(prop.Name) != null)
		//         {
		//             var propGetter = prop.GetGetMethod();
		//             var propSetter = toType.GetProperty(prop.Name).GetSetMethod();
		//             var valueToSet = propGetter.Invoke(from, null);
		//             propSetter.Invoke(to, new[] { valueToSet });
		//         }
		//     }
		// }
		//


		// Get the List<string> of Object's rpoperties names
		public static List<string> PropertiesAsString(object from)
		{
			return from.GetType().GetProperties().Select(x => x.Name).ToList();
		}

		// Get the List<PropertyInfo> of Object 
		public static List<PropertyInfo> PropertiesAsObject(object from)
		{
			return from.GetType().GetProperties().ToList();
		}


		// Get List<PropertyInfo> of Object
        // Returns object with 3 properties type string. 
		public static List<Property> PropertiesTypes(object from)
		{
			var list = new List<Property>();
            from.GetType().GetProperties().ToList().ForEach(
                x => list.Add(
                    new Property {
                        Name = x.Name,
                        Type = x.GetType().ToString(),
                        Value = x.GetValue(from, null).ToString()
                    }
                )
            );
            return list;
		}



		// Add Comparison 


		// 
	}

	public class Property
	{
		public string Name { get; set; }
		public string Value { get; set; }
		public string Type { get; set; }
	}
}