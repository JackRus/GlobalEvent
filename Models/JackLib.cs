using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GlobalEvent.Models
{
	/***********************************
	
			MY LITTLE HELPERS
	
	***********************************/
	
	
	public class JackLib
	{
		// Copies the matching properties from one object to another.
		// Will override all matching properties (by Name)!!!
		// Dont' use when some properties shouln't be changed.  
		// Properties have to have GETTER AND SETTER.

		// TODO: include exepted properties (string, split(","))
		
		public static void CopyValues(object from, object to)
		{
            from.GetType().GetProperties().ToList().ForEach(p => 
			{
				if (to.GetType().GetProperty(p.Name) != null)
				{
					to.GetType().GetProperty(p.Name).GetSetMethod().Invoke(
						to, new[] { p.GetGetMethod().Invoke(from, null) }
					);
				}
			});
		}

		// VERSION with vars:
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

		// Returns the List<string> of Object's rpoperties names
		public static List<string> PropertyAsString(object from)
		{
			return from.GetType().GetProperties().Select(x => x.Name).ToList();
		}

		// Returns the List<PropertyInfo> of Object 
		public static List<PropertyInfo> PropertyAsObject(object from)
		{
			return from.GetType().GetProperties().ToList();
		}


		// Returns List<PropertyInfo> of Object
        // Returns object with 3 properties type string. 
        // NTV stands for NAME TYPE VALUE.
		public static List<Property> PropertyNTV(object obj)
		{
            if (obj != null)
            {
                var list = new List<Property>();
				obj.GetType().GetProperties().ToList().ForEach( x => {
                    list.Add( new Property {
                        Name = x.Name,
                        Type = x.PropertyType.Name, 
                        Value = x.GetValue(obj, null) == null ? "null" : x.GetValue(obj, null).ToString()
                    });
                });
				return list;
            }
			return null;
		}
		// Add Comparison 

		// public static void CheckNull(object one)
		// {
		// 	if (one == null)
		// 	{
		// 		throw new ArgumentNullException(nameof(one));
		// 	}
		// }

		// public static void CheckNull(object one, object two)
		// {
		// 	if (one == null)
		// 	{
		// 		throw new ArgumentNullException(nameof(one));
		// 	}

		// 	if (two == null)
		// 	{
		// 		throw new ArgumentNullException(nameof(two));
		// 	}
		// }

		// supports JackLibCopyValues();
		public class Property
		{
			public string Name { get; set; }
			public string Value { get; set; }
			public string Type { get; set; }
		}
	}
}