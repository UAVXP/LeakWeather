using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Web;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms.DataVisualization.Charting;

using System.Net;

namespace LeakWeather
{
	public partial class Form1 : Form
	{
		public SettingsForm sf;
		public Form1()
		{
			InitializeComponent();
			Hide();
			sf = new SettingsForm(this);
		}

		public bool isShows = true;
		delegate void SetTextCallback(dynamic[] text, bool forChart);
		delegate void SetChartCallback(dynamic[] text);
		private Thread demoThread = null;
	//	Settings s = new Settings();
	//	Settings.settings Set = new Settings.settings();

		private void Set_Position()
		{
			System.Drawing.Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;
			this.Location = new Point(workingRectangle.Width - this.Size.Width, workingRectangle.Height - this.Size.Height);
		}

		private void Show( bool show )
		{
		/*	this.Visible = !this.Visible;
			if (!hide)
			{
				Set_Position();
				isShows = true;

				if(this.CanFocus)
					this.Focus();
				this.Activate();
			}
			else
				isShows = false;*/
		}

		private void SetVisible(bool vis)
		{
			if (vis == true)
			{
				Set_Position();

			//	this.Activate();
			//	if (/*this.CanFocus &&*/ !this.Focused)
			//	{
				//	this.Focus();
					this.TopMost = true;
			//		Console.WriteLine("Focus!");
			//	}
			}
			this.Visible = vis;
		}

		private void Form1_Deactivate(object sender, EventArgs e)
		{
		//	label1.Text = "DEACT";
			this.Visible = false;
		//	SetVisible( false );
		}

		private void Form1_Activated(object sender, EventArgs e)
		{
		//	label1.Text = "ACT";
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			notifyIcon1.Visible = false;
			notifyIcon1.Dispose();
			Environment.Exit(0);
		}

		private void Form1_VisibleChanged(object sender, EventArgs e)
		{
		/*	if (this.Visible)
			{
				Set_Position();

				if (this.CanFocus)
					this.Focus();
			//	this.Activate();
			}*/
		}
	/*	private string getKey(Dictionary<string, string> test, List<string> list, string key)
		{
			foreach (string k in list)
			{
				Console.WriteLine("{0}, {1}",
				k,
				test[k]);
			//	if( k == key )
			//		return key;
			//	MessageBox.Show(k);
				Dictionary<string, string> test2 = JSON.ParseJson(test[k]);
				List<string> list2 = new List<string>(test2.Keys);
				foreach (string s in list2)
				{
					Console.WriteLine("{0}", test2[s]);
				}
			}
			return "";
		}*/
	/*	private void SetText(string text)
		{
			this.label1.Text = text;
		}
		private void PicVisible(bool enable)
		{
			this.pictureBox1.Visible = enable;
		}*/
		dynamic[] all = new dynamic[5];
		dynamic[] chart = new dynamic[5];
		public void ThreadProcSafe( bool forChart )
		{
			if (!forChart)
			{
				//string query = "select * from weather.forecast where woeid in (select woeid from geo.places(1) where text=\"odesa\")";
				/*	string query = "select * from weather.forecast where woeid=929398";
					Uri uri = new Uri("https://query.yahooapis.com/v1/public/yql?q=" + query + "&format=json&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys");
					string response = JSON.webRequest(uri.AbsoluteUri);
					JObject rss = JObject.Parse(response);
					string temp = (string)rss["query"]["results"]["channel"]["title"];
					if (temp.IndexOf("Error") >= 0)
					{
						MessageBox.Show("Error! " + temp.IndexOf("Error"));
						Console.WriteLine(response);
					}
					temp = (string)rss["query"]["results"]["channel"]["item"]["condition"]["temp"];
					SetText(temp + "°" + (string)rss["query"]["results"]["channel"]["units"]["temperature"]);
				 */
				//	string query = "http://api.openweathermap.org/data/2.5/weather?q=Odessa,UA";
				int cityID = Settings.Set.cityID;
				if (cityID <= 0)
				{
					cityID = 698740; // Odessa by default
				}
				//&units=metric
				//&lang=ru
				//	string query = "http://api.openweathermap.org/data/2.5/weather?id=" + cityID + "&APPID=97becc18f03e970a7a00ff22637cffd3";
				string query = String.Format(@"http://api.openweathermap.org/data/2.5/weather?id={0}&APPID=97becc18f03e970a7a00ff22637cffd3", cityID);
				Uri uri = new Uri(query);
				string response = JSON.webRequest(uri.AbsoluteUri);
				if (response == "") // Server is down or something else
				{
					all[0] = 0.0;
					all[1] = "Server is down";
					all[2] = "";
					SetText(all, false);
					return;
				}
				JObject rss = JObject.Parse(@response);
				try
				{

					int errorCode = (int)rss["cod"];
					if (errorCode == 200)
					{
						all[0] = (double)rss["main"]["temp"];
						all[1] = (string)rss["name"];
						//	all[2] = (string)rss["weather"][0]["main"];
						all[2] = (string)rss["weather"][0]["description"];
						all[3] = (string)rss["weather"][0]["icon"];
					}
					else if (errorCode == 404)
					{
						all[0] = 0.0;
						all[1] = (string)rss["message"];
						all[2] = "NOTHING";
					}
				}
				catch (Exception ex)
				{
					throw ex;
				}

				SetText(all, false);
				Console.WriteLine("update ended {0} ({1})", all[0], DateTime.Now.ToLongTimeString());
			}
			else
			{
				UpdateChart();
			}
		}
		dynamic[,] temps = new dynamic[100, 100];
		private void UpdateChart()
		{
		//	try
			{
				int cityID = Settings.Set.cityID;
				if (cityID <= 0)
				{
					cityID = 698740; // Odessa by default
				}
				string query = String.Format(@"http://api.openweathermap.org/data/2.5/forecast/daily?id={0}&APPID=97becc18f03e970a7a00ff22637cffd3", cityID);
				Uri uri = new Uri(query);
				string response = JSON.webRequest(uri.AbsoluteUri);
				if (response == "") // Server is down or something else
				{
					chart[0] = 0.0;
					chart[1] = "Server is down";
					chart[2] = "";
					SetText(chart, true);
					return;
				}
				JObject rss = JObject.Parse(@response);
				int errorCode = (int)rss["cod"];
			//	MessageBox.Show(errorCode.ToString());
				if (errorCode == 200)
				{
					int count = (int)rss["cnt"];
					if (count > 0)
					{
						for (int i = 0; i < count; i++)
						{
							JToken cityList = rss["list"][i];
						//	temps[i, 0] = cityList["main"]["temp"];
							temps[i, 0] = cityList["temp"]["day"];
						//	temps[i, 1] = cityList["dt_txt"];
							temps[i, 1] = cityList["dt"];
						}
					}
					chart[0] = count;
				}
				else if (errorCode == 404)
				{
					chart[1] = "Error: 404";
					SetText(chart, true);
					return;
				}
			}
		/*	catch (Exception ex)
			{
				throw ex;
			//	Console.WriteLine(ex.Message);
			}*/
			SetText(chart, true);
		}
	//	private double kelvin = 0;
		private void SetText(dynamic[] text, bool forChart)
		{
			if (!forChart)
			{
				// InvokeRequired required compares the thread ID of the
				// calling thread to the thread ID of the creating thread.
				// If these threads are different, it returns true.
				if (this.pictureBox1.InvokeRequired)
				{
					SetTextCallback d = new SetTextCallback(SetText);
					this.Invoke(d, new object[] { all, false });
				}
				else
				{
					double kelvin = (all[0] != null) ? all[0] : 0.0;
					double temperature = 0.0;
					if (comboBox1.SelectedIndex == 0)
						temperature = (kelvin - 273.15); // Celsius
					else
						temperature = ((9.0 / 5.0) * (kelvin - 273) + 32); // Fahrenheit

					this.label1.Text = String.Format("{0:f0}{1}", temperature, comboBox1.Text);
					Console.WriteLine("Temp: {0}, {1}, {2} (kelvin: {3})",
						temperature,
						String.Format("{0:F1}", temperature),
						this.label1.Text,
						kelvin);


					label2.Text = all[1];
					string desc = "";
					switch ((string)all[2])
					{
						case "overcast clouds":
							desc = "пасмурные облака";
							break;
					}
					label3.Text = desc;



					if (all[0] != null)
					{
						this.pictureBox1.Visible = false;
						this.button1.Enabled = true;

						if (all[0] != 0.0 && all[3] != null)
							pictureBox2.ImageLocation = String.Format("http://openweathermap.org/img/w/{0}.png", all[3]);

					}
					else if (Settings.Set.isUpdateAfterLoad == 1)
					{
						this.pictureBox1.Visible = true;
						this.button1.Enabled = false;
					}
				}
			}
			else
			{
				if (this.chart1.InvokeRequired)
				{
					SetChartCallback d = new SetChartCallback(SetChart);
					this.Invoke(d, new object[] { all });
				}
				else
				{
					SetChart(text);
				}
			}
		}
		private void SetChart(dynamic[] ch)
		{
			int count = (chart[0] != null) ? chart[0] : 0;
			chart1.Annotations.Clear();
			chart1.Series[0].Points.Clear();
			for (int i = 0; i < count; i++)
			{
				try
				{
					//	DateTime dt = DateTime.Parse((string)temps[i, 1]); // For 3 hour forecast
					DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
					dt = dt.AddSeconds((double)temps[i, 1]).ToLocalTime();
					//	double kelvin = (temps[i, 0] != "") ? Convert.ToDouble(temps[i, 0]) : 0.0;
					double kelvin = (double)temps[i, 0];
					double temperature = 0.0;
					if (comboBox1.SelectedIndex == 0)
						temperature = (kelvin - 273.15); // Celsius
					else
						temperature = ((9.0 / 5.0) * (kelvin - 273) + 32); // Fahrenheit
					//	chart1.Series[0].Points.AddXY(dt.ToShortTimeString(), temperature + "°C");
					chart1.Series[0].Points.AddXY(dt.ToShortDateString(), temperature);
				//	Random rnd = new Random();
				//	int rndInt = rnd.Next(-10, 10);
				//	chart1.Series[0].Points.AddXY(dt.ToShortDateString(), rndInt);
					RectangleAnnotation annotation = new RectangleAnnotation();
					annotation.AnchorDataPoint = chart1.Series[0].Points[i];
					annotation.Text = String.Format("{0:f0}", temperature);
				//	annotation.Text = rndInt.ToString();
					annotation.ForeColor = Color.White;
					annotation.BackColor = Color.Gray;
					annotation.Font = new Font("Arial", 8);
					annotation.LineWidth = 0;
					annotation.ShadowColor = Color.DimGray;
					annotation.ShadowOffset = 2;

					chart1.Annotations.Add(annotation);
					Console.WriteLine("Added new point at ({0}, {1})", dt.ToShortTimeString(), temperature);
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
			if (chart[0] != null)
			{
				this.pictureBox1.Visible = false;
				this.button1.Enabled = true;
			}
			else
			{
				this.pictureBox1.Visible = true;
				this.button1.Enabled = false;
			}
		}

		public void updateWeather( bool forChart )
		{
			/// TODO: Fix the memory leak here!

		//	test();
		//	this.demoThread =
		//		new Thread(new ThreadStart(this.ThreadProcSafe));
			this.demoThread = new Thread(() => ThreadProcSafe(forChart));
			//	new Thread(new ParameterizedThreadStart(this.ThreadProcSafe));
			demoThread.Name = "UpdateGUIThread";
			this.demoThread.Start();
			pictureBox1.Visible = true;
			this.button1.Enabled = false;
		}
		private void button1_Click(object sender, EventArgs e)
		{
			updateWeather(isExtended);
		}


		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (/*kelvin != 0*/ all[0] != 0)
				SetText(all, false);
			Settings.Set.choosedUnits = (byte)comboBox1.SelectedIndex;
			Settings.s.WriteSettings(Settings.Set);
		}

		bool firstRun = true;
		Size oldSize;
		private void Form1_Shown(object sender, EventArgs e)
		{
			Console.WriteLine("shown");
			oldSize = this.Size; // Old: 296; 306
			this.Size = new Size(296, 79);
			

			Settings.Set = Settings.s.ReadSettings();
			Console.WriteLine("Is this first run? "+firstRun);
			if (Settings.Set.isUpdateAfterLoad == 1 || firstRun)
			{
				updateWeather(false);
				Console.WriteLine("LOAD TICK");
			}
			comboBox1.SelectedIndex = Settings.Set.choosedUnits;

			int delay = Settings.Set.updateTime, updateDelay = 1000;
			switch (delay)
			{
			//	case 0: { updateDelay = 60*1 * 1000; break; }
			//	case 1: { updateDelay = 60*2 * 1000; break; }
			//	case 3: { updateDelay = 60*5 * 1000; break; }
				case 0: { updateDelay = 60*10 * 1000; break; }
				case 1: { updateDelay = 60*30 * 1000; break; }
				case 2: { updateDelay = 60*60 * 1000; break; }
			}
			timer1.Interval = updateDelay;
			timer1.Start();

		//	SetVisible(false);
			Set_Position();
		}
		
		private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (sf != null)
			{
				sf.Show();
				if( !sf.Focused )
					sf.Focus();
			}
			else
				sf = new SettingsForm(this);
			System.Drawing.Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;
			sf.Location = new Point(workingRectangle.Width - sf.Size.Width, workingRectangle.Height - sf.Size.Height);
			sf.Show();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			updateWeather(false);
			Console.WriteLine("TIMER TICK");
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			if (!File.Exists(@"settings.dat"))
			{
				firstRun = true;
				try
				{
					FileStream fs = File.Create(@"settings.dat");
					fs.Flush();
					fs.Close();
				}
				catch (Exception ex)
				{
					MessageBox.Show("Cannot create settings file! Please check if you can write onto this directory.\n"+ex.Message);
					Environment.Exit(1);
				}
			}
			else
				firstRun = false;
		}

		private void notifyIcon1_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				SetVisible(!this.Visible);
			//	this.Visible = !this.Visible;
				/*	if( !this.Visible )
						SetVisible(true);
					else
						SetVisible(false);*/
			/*	if (this.Visible)
				{
				//	this.Activate();
					if (this.CanFocus)
						this.Focus();
				}*/
			}
			else if (e.Button == System.Windows.Forms.MouseButtons.Right)
				contextMenuStrip1.Show(MousePosition);
		}
		private bool isExtended = false;
		private void button2_Click(object sender, EventArgs e)
		{
			// Do I need anim?
			Size temp = oldSize;
			oldSize = this.Size;
			this.Size = temp;
			Set_Position();
			isExtended = !isExtended;

			if (isExtended)
			{
				Console.WriteLine("Window extended");

			/*	double x, y, A = 0, B = 10, h = 0.2;
				for (x = A; x <= B + 0.1 * h; x += h)
				{
					y=x*x*x*Math.Pow(Math.Cos(x),2)/Math.Pow(2,x);
					chart1.Series[0].Points.AddXY(x, y);
				}*/
			/*	chart1.Series[0].Points.AddXY("00:00", 1);
				chart1.Series[0].Points.AddXY("03:00", 2);
				chart1.Series[0].Points.AddXY("06:00", 2);
				chart1.Series[0].Points.AddXY("09:00", 2);
				chart1.Series[0].Points.AddXY("12:00", 3);
				chart1.Series[0].Points.AddXY("15:00", 2);
				chart1.Series[0].Points.AddXY("18:00", 2);
				chart1.Series[0].Points.AddXY("21:00", 2);*/
				updateWeather(true);
			}
		}
	/*	private void ThreadProcSafe()
		{
			this.SetText("This text was set safely.");
		}*/
	}
}
