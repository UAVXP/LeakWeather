using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Runtime.InteropServices;

namespace LeakWeather
{
	class Settings
	{
		public struct settings
		{
			//	public bool isUpdateAfterLoad;
			//	public int choosedUnits;
			//	public int updateTime;
			public byte isUpdateAfterLoad;
			public byte choosedUnits;
			public byte updateTime;
			public int cityID;
		};

		public static Settings s = new Settings();
		public static Settings.settings Set = new Settings.settings();
		public static string cityName = "";

		public Settings()
		{
			Set.isUpdateAfterLoad = 0;
			Set.choosedUnits = 0;
			Set.updateTime = 0;
			Set.cityID = 0;
		}

		public settings ReadSettings()
		{
			StructFile sf = new StructFile(@"settings.dat", typeof(settings));
			sf.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

			if (IsEmpty())
				WriteSettings(Settings.Set);

			//	settings Set = new settings();
			//	if (sf.GetNextStructureValue() != null)
			//		Set = (settings)sf.GetNextStructureValue();
			settings sets = (settings)sf.GetNextStructureValue();
			sf.Close();

			// Dirty hack, OMG
			string fname = "city.txt";
			if (!File.Exists(fname))
			{
				StreamWriter fr = File.AppendText(fname);
				fr.Close();
			}

			StreamReader fw = File.OpenText(fname);
			cityName = fw.ReadLine();
			fw.Close();

			return sets;
		}
		public bool WriteSettings(settings sets)
		{
			StructFile sf = new StructFile(@"settings.dat", typeof(settings));
			sf.Open(FileMode.Open, FileAccess.Write, FileShare.ReadWrite);

			sf.WriteStructure(sets);
			sf.Close();

			// Dirty hack, OMG
			File.WriteAllText(@"city.txt", cityName);
			Console.WriteLine("!!!!!!!WRITING SETTINGS!!!!!!!");
		//	fw.Write(cityName);
		//	fw.Close();

			return true;
		}

		public bool IsEmpty()
		{
			//	return File.Exists(@"settings.dat");
			FileStream fs = File.OpenRead(@"settings.dat");
			bool isEmpty = false;
			if (fs.Length <= 0)
				isEmpty = true;
			fs.Flush();
			fs.Close();
			return isEmpty;
		}
	}
}
