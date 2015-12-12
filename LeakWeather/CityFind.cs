using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace LeakWeather
{
	public partial class CityFind : Form
	{
		Form1 _parent = null;
		public CityFind( Form1 parent )
		{
			InitializeComponent();
			_parent = parent;
		}

		delegate void SetTextCallback(dynamic[] text);
		private Thread demoThread = null;
		dynamic[] all = new dynamic[5];
		dynamic[,] cities = new dynamic[100,100];

		public void ThreadProcSafe()
		{
			all[0] = -1;
			string city = textBox1.Text;
			string query = String.Format(@"http://api.openweathermap.org/data/2.5/find?q={0}&mode=json&type=like&APPID=97becc18f03e970a7a00ff22637cffd3", city);
			Uri uri = new Uri(query);
			string response = JSON.webRequest(uri.AbsoluteUri);
			if (response == "")
			{
				all[1] = "Server is down"; // Find a way to display an errors
				SetText(all);
				return;
			}
			JObject rss = JObject.Parse(@response);
			try
			{
				int errorCode = (int)rss["cod"];
			//	MessageBox.Show(errorCode.ToString());
				if (errorCode == 200)
				{
					int count = (int)rss["count"];
					if (count > 0)
					{
						for (int i = 0; i < count; i++)
						{
							JToken cityList = rss["list"][i];
							cities[i, 0] = (string)cityList["name"];
							cities[i, 1] = (string)cityList["sys"]["country"];
							cities[i, 2] = (int)cityList["id"];
						}
					}
					all[0] = count;
				}
				else if (errorCode == 404)
				{
					all[1] = "Error: 404";
					SetText(all);
					return;
				}
			}
			catch (Exception ex)
			{
			//	throw ex;
				Console.WriteLine(ex.Message);
			}
			SetText(all);
		}
		private void SetText(dynamic[] text)
		{
			if (this.textBox1.InvokeRequired)
			{
				SetTextCallback d = new SetTextCallback(SetText);
				this.Invoke(d, new object[] { all });
			}
			else
			{
			/*	if (all[1] != null)
				{
					textBox1.Text = all[1];
					textBox1.SelectAll();
					all[1] = null;
				}*/
				int count = (all[0] != null) ? all[0] : 0;
				dataGridView1.Rows.Clear();
				if (count > 0)
				{
				//	if( count != 1 )
				//		dataGridView1.Rows.Add(count - 1);
					for (int i = 0; i < count; i++)
					{
						dataGridView1.Rows.Add();
						for (int j = 0; j < 3; j++)
							dataGridView1[j, i].Value = cities[i, j];
					}
				}

				if (all[0] != null)
				{
					this.button1.Enabled = true;
					this.pictureBox1.Visible = false;
				}
			}
		}

		private void startFind()
		{
			this.demoThread = new Thread(new ThreadStart(this.ThreadProcSafe));
			demoThread.Name = "UpdateCityFind";
			this.demoThread.Start();

			this.button1.Enabled = false;
			this.pictureBox1.Visible = true;
		}

		private void textBox1_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				startFind();
		}

		private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
		{
		//	MessageBox.Show(e.RowIndex.ToString());
			List<DataGridViewRow> lst = dataGridView1.Rows.Cast<DataGridViewRow>().ToList();
		//	try
			{
				object ID = lst[e.RowIndex].Cells[2].Value;
				string CityName = lst[e.RowIndex].Cells[0].Value.ToString();
				Console.WriteLine("string CityName = {0}", CityName);
				Settings.Set.cityID = Convert.ToInt32(ID);
				Settings.cityName = CityName;
			}
		/*	catch (Exception ex)
			{
				throw ex;
			}*/
			Console.WriteLine("Test: {0}, {1} - {2}", e.RowIndex, lst[e.RowIndex].Cells[2].Value, Settings.Set.cityID);
			Settings.s.WriteSettings(Settings.Set);

			_parent.Visible = true;
			_parent.updateWeather(false); // Need to make it with an argument of cityID
			// Calling update on settings form
			_parent.sf.UpdateCitiesList();
			_parent.sf.Hide();
			this.Hide();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			startFind();
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			startFind();
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
