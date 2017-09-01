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
            if (from != null && to != null)
            {
                from.GetType().GetProperties().ToList().ForEach(p =>
                {
                    var toProperty = to.GetType().GetProperty(p.Name);
                    if (toProperty != null && toProperty.GetType() == p.GetType())
                    {
                        toProperty.GetSetMethod().Invoke(
                            to, new[] { p.GetGetMethod().Invoke(from, null) }
                        );
                    }
                });
            }
        }

		// Returns the List<string> of Object's rpoperties names
		public static List<string> PropertyAsString(object from)
		{
			if (from != null)
			{
				return from.GetType().GetProperties().Select(x => x.Name).ToList();
			}

			return null;
		}

		// Returns the List<PropertyInfo> of Object 
		public static List<PropertyInfo> PropertyAsObject(object from)
		{
			if (from != null)
			{
				return from.GetType().GetProperties().ToList();
			}

			return null;
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

		// supports JackLibCopyValues();
		public class Property
		{
			public string Name { get; set; }
			public string Value { get; set; }
			public string Type { get; set; }
		}

        public static void IfNull(params object[] objects)
        {
            int count = 1;
            // Lists all NULL objects 
            foreach(object obj in objects)
            {
				if (obj == null)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("===>");
					Console.WriteLine($"===> Object number \"{count}\" is NULL <===");
					Console.WriteLine("===>");
					Console.ResetColor();
				}
                count++;
            }

            count = 1;
            // Throws an Exception for the 1st null object
            foreach (object obj in objects)
			{
				if (obj == null)
				{
					throw new ArgumentNullException($"OBJECT {count.ToString()}");
				}
				count++;
			}
        }
    }
}