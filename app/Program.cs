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
	internal class Program
	{
		private static void Main(string[] args)
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
						+ "  2. Contact by word in Profile\r\n"
						+ "  3. Contact by exact Last Name\r\n"
						+ "  4. Contact by exact Title\r\n"
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

							//2)	Contact that contain the matching word in their profile should be returned
						case ConsoleKey.D2:
						case ConsoleKey.NumPad2:
							searchTerm = requestString("Please enter Contact search term");
							sendToUI(getContactsByProfileTerm(searchTerm), true);
							break;

							//3)	Contact that match on Last Name exactly should be returned
						case ConsoleKey.D3:
						case ConsoleKey.NumPad3:
							searchTerm = requestString("Please enter Contact Last Name");
							sendToUI(getContactsByExactLastName(searchTerm), false);
							break;


							//4)	Contact that match on Title exactly should be returned
						case ConsoleKey.D4:
						case ConsoleKey.NumPad4:
							searchTerm = requestString("Please enter Contact Title");
							sendToUI(getContactsByExactTitle(searchTerm), false);
							break;

							//5)	The count of both outlets and contacts should be returned
						case ConsoleKey.D5:
						case ConsoleKey.NumPad5:
							Console.CursorTop--;
							Console.WriteLine("\r   \r\n\r\nData count:");
							Console.WriteLine("  {0} MediaOutlets\r\n  {1} contacts ", outlets.Count, contacts.Count);

							// inner count
							List<Contact> titleList = contacts.GroupBy(x => x.title).Select(x => x.First()).ToList();
							foreach (var contact in titleList.OrderBy(x => x.title))
								Console.WriteLine("\t{0} {1}", getContactsByExactTitle(contact.title).Count, contact.title);
							waitForKey();
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
				waitForKey();
			}
		}

		#region UI

		private static void sendToUI(List<Outlet> outletList)
		{
			Console.WriteLine("{0} outlet{1} returned", outletList.Count, (outletList.Count != 1 ? "s" : ""));
			foreach (Outlet outlet in outletList)
			{
				Console.WriteLine("\r\n\t» {0}", outlet);
				if (outlet.Contacts.Count > 0)
				{
					foreach  (Contact contact in outlet.Contacts)
					{
						Console.WriteLine("\t\t· {0}", contact);
					}
				}
			}

			// Console.WriteLine(JsonHelper.FormatJson(JsonConvert.SerializeObject(outlets)));
			waitForKey();
		}

		private static void sendToUI(List<Contact> contactList, bool includeProfile)
		{
			Console.WriteLine("{0} contact{1} matching", contactList.Count, (contactList.Count!=1?"s":""));
			foreach (Contact contact in contactList)
			{
				if(includeProfile)
					Console.WriteLine("\r\n\t· {0}\r\n\tprofile: {1}", contact, contact.profile);
				else
					Console.WriteLine("\t· {0} [from {1}]", contact, outlets.FirstOrDefault(x => x.id == contact.outletId));
			}
			waitForKey();
		}

		private static void waitForKey()
		{
			Console.CursorVisible = false;
			Console.WriteLine("\r\n\t press any key to continue...");
			Console.ReadKey();
			Console.CursorVisible = true;
		}


		private static string requestString(string prompt)
		{
			while (true)
			{
				Console.Write("\r{0}: ", prompt);
				string str = Console.ReadLine();
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

		#region Contact
		private static List<Contact> _contacts;

		public static List<Contact> contacts
		{
			get
			{
				if (_contacts != null) return _contacts; // todo: improve caching
				try
				{
					using (var sr = new StreamReader("Contacts.json"))
					{
						_contacts = JsonConvert.DeserializeObject<List<Contact>>(sr.ReadToEnd());
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

		private static List<Contact> getContactsByProfileTerm(string searchTerm)
		{
			return contacts.Where(x => x.profile.ToLower().Contains(searchTerm.ToLower())).ToList();
		}

		private static List<Contact> getContactsByExactLastName(string searchTerm)
		{
			return contacts.Where(x => String.Equals(x.lastName, searchTerm, StringComparison.CurrentCultureIgnoreCase)).ToList();
		}

		private static List<Contact> getContactsByExactTitle(string searchTerm)
		{
			return contacts.Where(x => String.Equals(x.title, searchTerm, StringComparison.CurrentCultureIgnoreCase)).ToList();
		}
		
		#endregion

		#region MediaOutlets
		private static List<Outlet> _outlets;

		private static List<Outlet> outlets
		{
			get
			{
				if (_outlets != null) return _outlets; // todo: improve caching
				try
				{
					using (var sr = new StreamReader("Outlets.json"))
					{
						_outlets = JsonConvert.DeserializeObject<List<Outlet>>(sr.ReadToEnd());
					}
				}
				catch (Exception ex)
				{
					throw new ApplicationException("Error: " + ex.Message);
				}

				if (_outlets == null) throw new ApplicationException("Error reading Contact.json");
				return _outlets;
			}
		}

		private static List<Outlet> getMediaOutletsByTerm(string searchTerm)
		{
			return outlets.Where(x => x.name.ToLower().Contains(searchTerm.ToLower())).ToList();
		}
		#endregion

		#endregion

	}

	// *IMPORTANT NOTICE: To populate the POCO objects we should use a ORM such as Entity Framework / NHibernate or a OC such as RavenDB.. 
	// In this particular case, I decided to use a simple Linq query in the getter to join both classes to keep this simple (without an external database).

	#region Model

	public class Contact
	{
		public int id { get; set; }
		public int outletId { get; set; }
		public string firstName { get; set; }
		public string lastName { get; set; }
		public string title { get; set; }
		public string profile { get; set; }

		public override string ToString()
		{
			return string.Format("{0} {1} - {2}", this.firstName, this.lastName, this.title);
		}
	}

	public class Outlet
	{
		public int id { get; set; }
		public string name { get; set; }

		public List<Contact> Contacts
		{
			get { return Program.contacts.Where(x => x.outletId == this.id).ToList(); }
			// yes, this should be handled by the real ORM in the Service/Data layer
		}

		public override string ToString()
		{
			//return string.Format("{0} [id: {1}]", this.name, this.id);
			return this.name;
		}
	}

	public class Counter
	{
		public string objectName { get; set; }
		public int totalItems { get; set; }
	}

	#endregion

	#region Json beautifier (downloaded from http://stackoverflow.com/questions/4580397/json-formatter-in-c)

	internal class JsonHelper
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

	internal static class Extensions
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
