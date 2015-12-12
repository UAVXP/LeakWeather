using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LeakWeather
{
	public partial class SettingsForm : Form
	{
		Form1 _parent = null;
		CityFind cf;
		public SettingsForm( Form1 parent )
		{
			InitializeComponent();
			_parent = parent;
			cf = new CityFind(_parent);
		}

	//	dynamic[] cities = new dynamic[2];

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			Settings.Set.isUpdateAfterLoad = (byte)((checkBox1.Checked) ? 1 : 0);
			Settings.s.WriteSettings(Settings.Set);
		}

		List<string[]> cities2 = new List<string[]>();
		public void UpdateCitiesList()
		{
		//	MessageBox.Show("UpdateCitiesList called!");
		//	try
			{
				Settings.Set = Settings.s.ReadSettings();
				checkBox1.Checked = (Settings.Set.isUpdateAfterLoad != 0) ? true : false;
				comboBox1.SelectedIndex = Settings.Set.updateTime;

				cities2.Clear();
				comboBox2.Items.Clear();
				
				cities2.Add(new string[] { "698740", "Odessa", "Одесса" });
				cities2.Add(new string[] { "703448", "Kiev", "Киев" });

				int n = cities2.Count;
				for (int i = 0; i < n; i++)
				{
					comboBox2.Items.Add(cities2[i][2]);
				}
				//	Console.WriteLine("{0}, {1}", Settings.Set.cityID.ToString(), comboBox2.FindString(Settings.Set.cityID.ToString()));
				string cityID = Settings.Set.cityID.ToString();
				//	string cityName = Settings.Set.getCityName().ToString();
				string cityName = Settings.cityName;
				bool found = false;
				for (int i = 0; i < n; i++)
				{
					if (cities2[i][0] == cityID /*|| cities[i][1] == cityName*/)
					{
						comboBox2.SelectedIndex = i;
						//	Console.WriteLine("Choosed id: " + i + "(" + cities[i][0] + ", " + cityID + ")");
						found = true;
						break;
					}
				}
				if (!found)
				{
					if (cityName == "" || cityID == "0")
					{
						Console.WriteLine("[SettingsForm]: First run, zero city");
						comboBox2.SelectedIndex = 0;
						return;
					}
					Console.WriteLine("[SettingsForm]: Added new city: {0} with ID: {1}", cityName, cityID);
					comboBox2.Items.Add(cityName);
					cities2.Add(new string[] { cityID, cityName, cityName });
					comboBox2.SelectedIndex = comboBox2.Items.Count - 1;
				}
			}
		/*	catch (Exception ex)
			{
				throw ex;
			}*/
		}
		private void SettingsForm_Load(object sender, EventArgs e)
		{
			UpdateCitiesList();
		}

		private void SettingsForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			_parent.Visible = true;
			_parent.updateWeather(false); // Need to make it with an argument of cityID
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			Settings.Set.updateTime = (byte)comboBox1.SelectedIndex;
			Settings.s.WriteSettings(Settings.Set);
		}

		private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
			//	Settings.Set.cityID = Convert.ToInt32(cities[comboBox2.SelectedIndex][0]);
				Settings.Set.cityID = Convert.ToInt32(cities2[comboBox2.SelectedIndex][0]);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		//	Console.WriteLine("Wrote: {0}, {1} - {2}", comboBox2.SelectedIndex, cities[comboBox2.SelectedIndex][0], Settings.Set.cityID);
			Settings.s.WriteSettings(Settings.Set);
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (cf != null)
			{
				cf.Show();
				if (!cf.Focused)
					cf.Focus();
			}
			else
				cf = new CityFind(_parent);
			cf.Show();
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
		//	base.OnFormClosing(e);
			if (e.CloseReason == CloseReason.WindowsShutDown) return;
			this.Visible = false;
			e.Cancel = true;
		}
	}
}
