﻿using System;
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
			string searchTerm;
			IEnumerable<Contacts> matchingContacts;
			IEnumerable<Outlets> matchingOutlets;

			try
			{

				Console.WriteLine("{0} Contacts read..", contacts.Count);
				Console.WriteLine("{0} Outlets read..", outlets.Count);

				//1)	MediaOutlets that contain the matching word in the Name should be returned
				searchTerm = "ed";
				Console.WriteLine("\r\n-- MediaOutlets that contain the matching word in the Name [{0}]", searchTerm);
				matchingOutlets = outlets.Where(x => x.name.ToLower().Contains(searchTerm.ToLower()));
				sendToUI(matchingOutlets);

				//2)	Contacts that contain the matching word in their profile should be returned
				searchTerm = "Downtown";
				Console.WriteLine("\r\n\r\n-- Contacts that contain the matching word in their profile [{0}]", searchTerm);
				matchingContacts = contacts.Where(x => x.profile.ToLower().Contains(searchTerm.ToLower()));
				sendToUI(matchingContacts);

				//3)	Contacts that match on Last Name exactly should be returned
				searchTerm = "Edwards";
				Console.WriteLine("\r\n\r\n-- Contacts that match on Last Name exactly [{0}]", searchTerm);
				matchingContacts = contacts.Where(x => String.Equals(x.lastName, searchTerm, StringComparison.CurrentCultureIgnoreCase));
				sendToUI(matchingContacts);

				//4)	Contacts that match on Title exactly should be returned
				searchTerm = "Program Director";
				Console.WriteLine("\r\n\r\n-- Contacts that match on Title exactly [{0}]", searchTerm);
				matchingContacts = contacts.Where(x => String.Equals(x.title, searchTerm, StringComparison.CurrentCultureIgnoreCase));
				sendToUI(matchingContacts);

				//5)	The count of both outlets and contacts should be returned
				Console.WriteLine("\r\n\r\n-- The count of both outlets and contacts");
				List<Counter> totalsList = new List<Counter>();
				totalsList.Add(new Counter() { name = "totalContacts", totalRecords = contacts.Count });
				totalsList.Add(new Counter() { name = "totalOutlets", totalRecords = outlets.Count });
				sendToUI(totalsList);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unexpected error in application..\r\n[{0}]", ex.Message);
			}


		}

		#region UI

		private static void sendToUI(object T)
		{
			Console.WriteLine(JsonConvert.SerializeObject(T).Replace("},", "},\r\n"));
			Console.ReadKey();
		}

		#endregion

		#region DAO
		private static List<Contacts> _contacts;

		private static List<Contacts> contacts
		{
			get
			{
				if (_contacts != null) return _contacts; // todo: improbe caching
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
				if (_outlets != null) return _outlets; // todo: improbe caching
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
	}

	public class Counter
	{
		public string name { get; set; }
		public int totalRecords { get; set; }
	}

	#endregion
}