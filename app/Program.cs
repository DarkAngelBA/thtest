using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace app
{
	class Program
	{
		static void Main(string[] args)
		{
		    try
			{
			    string searchTerm;
			    bool mainLoop = true;
                while (mainLoop)
                {

                    Console.Clear();
                    Console.WriteLine("-- TH Test Application --\r\n");

                    //-- Main Menu --
                    Console.WriteLine(
                    "Please select an option:\r\n" 
                    + "  1. MediaOutlets by word\r\n"
                    + "  2. Contacts by word\r\n"
                    + "  3. Contacts by exact Last Name\r\n"
                    + "  4. Contacts by exact Title\r\n"
                    + "  5. Count of data\r\n"
                    + "  6. Exit app\r\n"
                    );

                    Console.CursorVisible = false;
                    var key = Console.ReadKey().Key;
                    Console.CursorVisible = true;

                    //-- Answers
                    switch (key)
                    {
                        //1)	MediaOutlets that contain the matching word in the Name should be returned
                        case ConsoleKey.D1:
                        case ConsoleKey.NumPad1:
                            searchTerm = requestString("Please enter MediaOutlets search term");
                            sendToUI(getMediaOutletsByTerm(searchTerm));
                            break;

                        //2)	Contacts that contain the matching word in their profile should be returned
                        case ConsoleKey.D2:
                        case ConsoleKey.NumPad2:
                            searchTerm = requestString("Please enter Contacts search term");
                            sendToUI(getContactsByProfileTerm(searchTerm));
                            break;

                        //3)	Contacts that match on Last Name exactly should be returned
                        case ConsoleKey.D3:
                        case ConsoleKey.NumPad3:
                            searchTerm = requestString("Please enter Contact Last Name");
                            sendToUI(getContactsByExactLastName(searchTerm));
                            break;

                        
                        //4)	Contacts that match on Title exactly should be returned
                        case ConsoleKey.D4:
                        case ConsoleKey.NumPad4:
                            searchTerm = requestString("Please enter Contact Title");
                            sendToUI(getContactsByExactTitle(searchTerm));
                            break;

                        //5)	The count of both outlets and contacts should be returned
                        case ConsoleKey.D5:
                        case ConsoleKey.NumPad5:
                            var totalsList = new List<Counter>
				            {
					            new Counter() {objectName = "Contacts", totalItems = contacts.Count},
					            new Counter() {objectName = "Outlets", totalItems = outlets.Count}
				            };
                            sendToUI(totalsList);
                            break;

                        // exit..
                        case ConsoleKey.D6:
                        case ConsoleKey.NumPad6:
                        case ConsoleKey.Escape:
                            mainLoop = false;
                            break;

                        default:
                            continue;
                    }

                }

                Console.WriteLine("\r\n\r\nThank you for evaluating this app");
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unexpected error in application..\r\n[{0}]", ex.Message);
			}


		}

        #region bussinessRules
        private static List<Outlets> getMediaOutletsByTerm(string searchTerm)
        {
            return outlets.Where(x => x.name.ToLower().Contains(searchTerm.ToLower())).ToList();
        }

	    private static List<Contacts> getContactsByProfileTerm(string searchTerm)
	    {
	        return contacts.Where(x => x.profile.ToLower().Contains(searchTerm.ToLower())).ToList();
	    }

        private static List<Contacts> getContactsByExactLastName(string searchTerm)
        {
            return contacts.Where(x => String.Equals(x.lastName, searchTerm, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }

        private static List<Contacts> getContactsByExactTitle(string searchTerm)
        {
            return contacts.Where(x => String.Equals(x.title, searchTerm, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }

	    private static List<Contacts> getContactsByOutletName(string searchTerm)
	    {
	        var outlet = outlets.FirstOrDefault(o => o.name.ToLower().Contains(searchTerm.ToLower()));
	        return outlet == null ? 
                null : contacts.Where(x => x.outletId == outlet.id).ToList();
	    }
	    #endregion

        #region UI

        private static void sendToUI(object T)
		{
            Console.WriteLine(JsonHelper.FormatJson(JsonConvert.SerializeObject(T)));
            Console.WriteLine("\r\n\t press any key to continue...");
            Console.CursorVisible = false;
            Console.ReadKey();
            Console.CursorVisible = true;
        }

        private static string requestString(string prompt) {
            string str; 
            while (true)
            {
                Console.Write("\r{0}: ",prompt);
                str = Console.ReadLine();
                if (!string.IsNullOrEmpty(str))
                {
                    Console.WriteLine();
                    return str;
                }
                Console.Write("\tstring shouldn't be empty..");
                Console.CursorTop--;
            }

        }

		#endregion

		#region DAO
		private static List<Contacts> _contacts;

		public static List<Contacts> contacts
		{
			get
			{
				if (_contacts != null) return _contacts; // todo: improve caching
				try
				{
					using (var sr = new StreamReader("Contacts.json"))
					{
						_contacts = JsonConvert.DeserializeObject<List<Contacts>>(sr.ReadToEnd());
					}
				}
				catch (Exception ex)
				{
					throw new ApplicationException("Error: " + ex.Message);
				}

				if (_contacts == null) throw new ApplicationException("Error reading Contacts.json");
				return _contacts;
			}
		}

		private static List<Outlets> _outlets;

		private static List<Outlets> outlets
		{
			get
			{
				if (_outlets != null) return _outlets; // todo: improve caching
				try
				{
					using (var sr = new StreamReader("Outlets.json"))
					{
						_outlets = JsonConvert.DeserializeObject<List<Outlets>>(sr.ReadToEnd());
					}
				}
				catch (Exception ex)
				{
					throw new ApplicationException("Error: " + ex.Message);
				}

				if (_outlets == null) throw new ApplicationException("Error reading Contacts.json");
				return _outlets;
			}
		}

		#endregion

	}

	#region Model

    // *IMPORTANT NOTICE: To populate the POCO objects we should use a ORM such as Entity Framework / NHibernate or a OC such as RavenDB.. 
    // In this particular case, I decided to use a simple Linq query in the getter to join both classes to keep this simple (without an external database).

	public class Contacts
	{
		public int id { get; set; }
		public int outletId { get; set; }
		public string firstName { get; set; }
		public string lastName { get; set; }
		public string title { get; set; }
		public string profile { get; set; }
		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}

	public class Outlets
	{
		public int id { get; set; }
		public string name { get; set; }

	    public List<Contacts> contactsList
	    {
            get { return Program.contacts.Where(x => x.outletId == this.id).ToList(); } // yes, this should be handled by the real ORM.. POCO classes must be clean.
	    }
	}

	public class Counter
	{
		public string objectName { get; set; }
		public int totalItems { get; set; }
	}

	#endregion

	#region Json beautifier (downloaded from http://stackoverflow.com/questions/4580397/json-formatter-in-c)
	class JsonHelper
	{
		private const string INDENT_STRING = "    ";
		public static string FormatJson(string str)
		{
			var indent = 0;
			var quoted = false;
			var sb = new StringBuilder();
			for (var i = 0; i < str.Length; i++)
			{
				var ch = str[i];
				switch (ch)
				{
					case '{':
					case '[':
						sb.Append(ch);
						if (!quoted)
						{
							sb.AppendLine();
							Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
						}
						break;
					case '}':
					case ']':
						if (!quoted)
						{
							sb.AppendLine();
							Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));
						}
						sb.Append(ch);
						break;
					case '"':
						sb.Append(ch);
						bool escaped = false;
						var index = i;
						while (index > 0 && str[--index] == '\\')
							escaped = !escaped;
						if (!escaped)
							quoted = !quoted;
						break;
					case ',':
						sb.Append(ch);
						if (!quoted)
						{
							sb.AppendLine();
							Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
						}
						break;
					case ':':
						sb.Append(ch);
						if (!quoted)
							sb.Append(" ");
						break;
					default:
						sb.Append(ch);
						break;
				}
			}
			return sb.ToString();
		}
	}

	static class Extensions
	{
		public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
		{
			foreach (var i in ie)
			{
				action(i);
			}
		}
	}
	#endregion
}
