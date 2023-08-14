using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.IO.Ports;
using System.Management;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Data.SqlClient;
using System.Reflection.Emit;
using System.Diagnostics;

namespace Shark_Clone
{
    public partial class Form1 : Form
    {
        private bool mouseDown;
        private Point lastLocation;

        public Form1()
        {
            InitializeComponent();

            // Wire up the mouse events for panel1 to move the form
            this.panel1.MouseDown += Panel1_MouseDown;
            this.panel1.MouseMove += Panel1_MouseMove;
            this.panel1.MouseUp += Panel1_MouseUp;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // Add any painting logic for panel1 here if required
        }

        // Mouse event handlers for panel1
        private void Panel1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastLocation = e.Location;
        }

        private void Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X,
                    (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }

        private void Panel1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void LoadPortNames()
        {
            List<string> stringList = new List<string>();

            // Fetch details about modems using WMI
            ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_POTSModem");

            // Create a dictionary to store port names and associated device names
            Dictionary<string, string> portDetails = new Dictionary<string, string>();

            // Iterate through each modem object and extract port and device names
            foreach (ManagementObject modem in managementObjectSearcher.Get())
            {
                if (modem["AttachedTo"] != null && modem["Name"] != null)
                {
                    string portName = modem["AttachedTo"].ToString();
                    string deviceName = modem["Name"].ToString();

                    portDetails[portName] = deviceName;
                }
            }

            // Clear the comboBox items
            this.comboBox1.Items.Clear();

            // Iterate through the available COM ports
            foreach (string portName in SerialPort.GetPortNames())
            {
                // Check if we have device details for the port, if so, show both port and device name, else just the port name
                string comboBoxItem = portDetails.ContainsKey(portName) ? $"{portName} - {portDetails[portName]}" : portName;
                stringList.Add(comboBoxItem);
            }

            // Sort and add items to comboBox
            stringList.Sort();
            this.comboBox1.Items.AddRange(stringList.ToArray());

            // Set the default selected index
            if (this.comboBox1.Items.Count > 0)
            {
                this.comboBox1.SelectedIndex = 0;
            }
        }

        private delegate void SetTextCallback(string string_0, Color A_1);
        internal void InsertText(string string_0, Color A_1)
        {
            if (this.richTextBox1.InvokeRequired)
            {
                this.Invoke((Delegate)new Form1.SetTextCallback(this.InsertText), (object)string_0, (object)A_1);
            }
            else
            {
                this.richTextBox1.SelectionColor = A_1;
                this.richTextBox1.AppendText(string_0);
                this.richTextBox1.SelectionStart = this.richTextBox1.TextLength;
                this.richTextBox1.SelectionLength = string_0.Length;
                this.richTextBox1.ScrollToCaret();
            }
        }

        public string IMEI = "";

        public string SN = "";

        public string MODEL = "";

        public static string adb(string cmd)
        {
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = Environment.CurrentDirectory + "/adb/adb.exe",
                    Arguments = cmd,
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            return process.StandardOutput.ReadToEnd();
        }// Start ADB server

        public bool adbdevice()
        {
            bool flag = false;
            if (Form1.adb("devices").Contains("\tdevice"))
                flag = true;
            return flag;
        } // Check if device is connected in ADB mode

        private void rjButton7_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            LoadPortNames();
        }

        private void rjButton1_Click(object sender, EventArgs e) // Read info
        {
           // Thread.Sleep(1500);
            richTextBox1.Clear();
            if (ModemSerialPort.IsOpen)
            {
                ModemSerialPort.Close();
            }

            string selectedText1 = comboBox1.Texts;
            string comPort1 = null;

            // Extract COM port using a more specific pattern to prevent mistakes
            Match match1 = Regex.Match(selectedText1, @"(COM\d+)");
            if (match1.Success)
            {
                comPort1 = match1.Groups[1].Value;

            }

            if (!string.IsNullOrEmpty(comboBox1.Texts))
            {
                this.InsertText("- Samsung FRP Tool (", Color.White);
                this.InsertText("V1.0", Color.Aqua);
                this.InsertText(")\n", Color.White);
                this.InsertText("- Operation : [", Color.White);
                this.InsertText("Read Information", Color.Aqua);
                this.InsertText("]\n", Color.White);
                this.InsertText("- Selected ComPort : [", Color.White);
                this.InsertText(comPort1, Color.Aqua);
                this.InsertText("]\n", Color.White);
                this.InsertText("- Connecting : [", Color.White);
                this.InsertText("OK", Color.Lime);
                this.InsertText("]\n", Color.White);
                this.InsertText("- Reading info : [", Color.White);
                this.InsertText("OK", Color.Lime);
                this.InsertText("]\n", Color.White);
                try
                {
                    string selectedText = comboBox1.Texts;
                    string comPort = null;

                    // Extract COM port using a more specific pattern to prevent mistakes
                    Match match = Regex.Match(selectedText, @"(COM\d+)");
                    if (match.Success)
                    {
                        comPort = match.Groups[1].Value;
                        
                    }

                    if (string.IsNullOrEmpty(comPort))
                    {
                        this.InsertText("Unable to determine COM Port from selection.", Color.Red);
                        return;
                    }

                    ModemSerialPort.PortName = comPort;
                   // this.InsertText("\rConnecting Phone On Port : ", Color.Black);
                    ModemSerialPort.Open();
                   // this.InsertText(ModemSerialPort.PortName + "...", Color.Black);
                    Thread.Sleep(500);
                   // this.InsertText(" OK\r\n\n", Color.Green);
                    ModemSerialPort.WriteLine("AT+DEVCONINFO\r");
                    Thread.Sleep(6500);

                    // ... Rest of your code ...
                    string str1 = ModemSerialPort.ReadExisting();
                    string str2 = str1.Split('(', ')')[1];
                    string str3 = str1.Split('(', ')')[5];
                    string str4 = str1.Split('(', ')')[17];
                    string str5 = str1.Split('(', ')')[19];
                    string str6 = str1.Split('(', ')')[27];
                    string str7 = Regex.Match(str1, "PRD\\((.+?)\\)").Groups[1].Value;
                    string str8 = Regex.Match(str1, "UN\\((.+?)\\)").Groups[1].Value;

                    string[] strArray = str3.Split('/');

                    this.IMEI = Regex.Match(str1, "IMEI\\((.+?)\\)").Groups[1].Value;
                    this.SN = Regex.Match(str1, "SN\\((.+?)\\)").Groups[1].Value;
                    this.MODEL = Regex.Match(str1, "MN\\((.+?)\\)").Groups[1].Value;

                    // this.InsertText(IMEI + " " + SN + " " + MODEL + "\n", Color.Black);


                    this.InsertText("- Model: ", Color.White);
                    this.InsertText(MODEL + "\r\n", Color.Aqua);
                    //this.MODEL = str2;
                    this.InsertText("- Baseband: ", Color.White);
                    //string[] strArray = str3.Split('/');
                    // + "/" + strArray[1] + "/" + strArray[2] + "/" + strArray[3] 
                    this.InsertText(strArray[0] + "\r\n", Color.Aqua);
                    this.InsertText("- Software: ", Color.White);
                    this.InsertText(strArray[1] + "\r\n", Color.Aqua);
                    this.InsertText("- Sales Code: ", Color.White);
                    this.InsertText(str7 + "\r\n", Color.Aqua);
                    this.InsertText("- SN: ", Color.White);
                    this.InsertText(SN + "\r\n", Color.Aqua);
                    // this.SN = str4;
                    this.InsertText("- IMEI: ", Color.White);
                    this.InsertText(IMEI + "\r\n", Color.Aqua);

                    
                    this.InsertText("- UN: ", Color.White);
                    this.InsertText(str8 + "\r\n", Color.Aqua);
                    // this.IMEI = str5;
                    this.InsertText("- Checking device lock :", Color.White);
                    Thread.Sleep(2500);



                    ModemSerialPort.WriteLine("AT+REACTIVE=1,0,0\r");
                    Thread.Sleep(2500);
                    string input = ModemSerialPort.ReadExisting();
                    string[] strArra = new string[4]


            {
          "UNLOCK",
          "LOCK",
          "TRIGGERED",
          "TRIGGER"
            };
                    string str = new Regex("REACTIVE:1,(.*)\\r\\n").Match(input).Groups[1].Value;
                    if (input != null && input.Trim() != string.Empty && input.Length >= 4 && !input.Contains("ERROR"))
                    {
                        string f_lock = str.Trim();

                        if (f_lock == "TRIGGER" || f_lock == "TRIGGERED" )
                        {
                            this.InsertText(" " + f_lock, Color.Red);
                        }

                        if (f_lock == "UNLOCK" || f_lock == "LOCK")
                        {
                            this.InsertText(" " + f_lock, Color.Aqua);

                        }
                    }


                    else if (input.Contains("ERROR"))
                    {
                        string f_lock = "FAIL N/A";
                        this.InsertText(f_lock, Color.Red);
                    }


                }
                catch (Exception ex)
                {
                    this.InsertText($"Error: {ex.Message}", Color.Red);
                    if (ModemSerialPort.IsOpen)
                    {
                        ModemSerialPort.Close();
                    }
                }
            }
            else
            {
                this.InsertText("\nNo device selected. Please select a device and try again.", Color.Red);
            }
        }

        private void rjButton2_Click(object sender, EventArgs e) // Enable ADB
        {
            if (ModemSerialPort.IsOpen)
            {
                ModemSerialPort.Close();
            }
            // Thread.Sleep(1500);
            richTextBox1.Clear();

            string selectedText1 = comboBox1.Texts;
            string comPort1 = null;

            // Extract COM port using a more specific pattern to prevent mistakes
            Match match1 = Regex.Match(selectedText1, @"(COM\d+)");
            if (match1.Success)
            {
                comPort1 = match1.Groups[1].Value;

            }

            if (!string.IsNullOrEmpty(comboBox1.Texts))
            {
                this.InsertText("- Samsung FRP Tool (", Color.White);
                this.InsertText("V1.0", Color.Aqua);
                this.InsertText(")\n", Color.White);
                this.InsertText("- Operation : [", Color.White);
                this.InsertText("Enable Debug", Color.Aqua);
                this.InsertText("]\n", Color.White);
                this.InsertText("- Selected ComPort : [", Color.White);
                this.InsertText(comPort1, Color.Aqua);
                this.InsertText("]\n", Color.White);
                this.InsertText("- Connecting : [", Color.White);
                this.InsertText("OK", Color.Lime);
                this.InsertText("]\n", Color.White);
                this.InsertText("- Setting debug state : ", Color.White);
                // this.InsertText("OK", Color.Lime);
                // this.InsertText("]\n", Color.White);
                ModemSerialPort.PortName = comPort1;
                if (!ModemSerialPort.IsOpen)
                {

                    ModemSerialPort.Open();
                   // this.InsertText("\nEnabling ADB...", Color.Black);
                    ModemSerialPort.Write("AT+DUMPCTRL=1,0\r\n");
                    Thread.Sleep(1000);
                    ModemSerialPort.Write("AT+DEBUGLVC=0,5\r\n");
                    Thread.Sleep(1000);
                    if (!ModemSerialPort.IsOpen)
                    {
                        ModemSerialPort.Open();
                    }
                    ModemSerialPort.Write("AT+SWATD=0\r\n");
                    Thread.Sleep(1000);
                    ModemSerialPort.Write("AT+ACTIVATE=0,0,0\r\n");
                    Thread.Sleep(1000);
                    ModemSerialPort.Write("AT+SWATD=1\r\n");
                    Thread.Sleep(1000);
                    ModemSerialPort.Write("AT+DEBUGLVC=0,5\r\n");
                    this.InsertText("[\n", Color.White);
                    this.InsertText("OK", Color.Lime);
                    this.InsertText("]\n", Color.White);
                    Thread.Sleep(1000);
                    ModemSerialPort.Close();

                    adbdevice();
                    Thread.Sleep(8000);
                }
            }
            else
            {
                this.InsertText("\nNo device selected. Please select a device and try again.", Color.Red);
            }

        }

        private void rjButton4_Click(object sender, EventArgs e)
        {

            richTextBox1.Clear();

            if (ModemSerialPort.IsOpen)
            {
                ModemSerialPort.Close();
            }
            string selectedText1 = comboBox1.Texts;
            string comPort1 = null;
            // Extract COM port using a more specific pattern to prevent mistakes
            Match match1 = Regex.Match(selectedText1, @"(COM\d+)");
            if (match1.Success)
            {
                comPort1 = match1.Groups[1].Value;

            }

            if (string.IsNullOrEmpty(comPort1))
            {
                this.InsertText("Unable to determine COM Port from selection.", Color.Red);
                return;
            }
            this.InsertText("- Samsung FRP Tool (", Color.White);
            this.InsertText("V1.0", Color.Aqua);
            this.InsertText(")\n", Color.White);
            this.InsertText("- Operation : [", Color.White);
            this.InsertText("Reboot Normal", Color.Aqua);
            this.InsertText("]\n", Color.White);
            this.InsertText("- Selected ComPort : [", Color.White);
            this.InsertText(comPort1, Color.Aqua);
            this.InsertText("]\n", Color.White);
            this.InsertText("- Connecting : [", Color.White);
            this.InsertText("OK", Color.Lime);
            this.InsertText("]\n", Color.White);
            this.InsertText("- Rebooting : ", Color.White);
            Thread.Sleep(2500);
            ModemSerialPort.PortName = comPort1;
            ModemSerialPort.Open();
            ModemSerialPort.WriteLine("AT+CFUN=1,1\r");
            ModemSerialPort.Close();

            this.InsertText("[", Color.White);
            this.InsertText("OK", Color.Lime);
            this.InsertText("]\n", Color.White);


        }

        private void rjButton6_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();

            if (ModemSerialPort.IsOpen)
            {
                ModemSerialPort.Close();
            }
            string selectedText1 = comboBox1.Texts;
            string comPort1 = null;
            // Extract COM port using a more specific pattern to prevent mistakes
            Match match1 = Regex.Match(selectedText1, @"(COM\d+)");
            if (match1.Success)
            {
                comPort1 = match1.Groups[1].Value;

            }

            if (string.IsNullOrEmpty(comPort1))
            {
                this.InsertText("Unable to determine COM Port from selection.", Color.Red);
                return;
            }
            this.InsertText("- Samsung FRP Tool (", Color.White);
            this.InsertText("V1.0", Color.Aqua);
            this.InsertText(")\n", Color.White);
            this.InsertText("- Operation : [", Color.White);
            this.InsertText("Factory Reset", Color.Aqua);
            this.InsertText("]\n", Color.White);
            this.InsertText("- Selected ComPort : [", Color.White);
            this.InsertText(comPort1, Color.Aqua);
            this.InsertText("]\n", Color.White);
            this.InsertText("- Connecting : [", Color.White);
            this.InsertText("OK", Color.Lime);
            this.InsertText("]\n", Color.White);
            this.InsertText("- Reseting : ", Color.White);
            Thread.Sleep(2500);
            ModemSerialPort.PortName = comPort1;
            ModemSerialPort.Open();
            
              
          
            ModemSerialPort.Write("AT+FACTORST=0,0\r\n");
            ModemSerialPort.Close();
            this.InsertText("[", Color.White);
            this.InsertText("OK", Color.Lime);
            this.InsertText("]\n", Color.White);
        }

        private void rjButton3_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();

            this.InsertText("- Samsung FRP Tool (", Color.White);
            this.InsertText("V1.0", Color.Aqua);
            this.InsertText(")\n", Color.White);
            this.InsertText("- Operation : [", Color.White);
            this.InsertText("Reset FRP", Color.Aqua);
            this.InsertText("]\n", Color.White);
            this.InsertText("- Selected Device : [", Color.White);
            this.InsertText("ADB Device", Color.Aqua);
            this.InsertText("]\n", Color.White);
            this.InsertText("- Connecting : [", Color.White);
            this.InsertText("OK", Color.Lime);
            this.InsertText("]\n", Color.White);
            this.InsertText("- Reseting FRP lock : ", Color.White);
            //this.InsertText("Checking if device is connected...", Color.Black);
            Thread.Sleep(4000);
            if (this.adbdevice())
            {
                //this.InsertText("OK", Color.Green);
                //this.InsertText("\nReading device information...", Color.Black);
                //this.InsertText("OK", Color.Green);
                //this.InsertText("\nBrand: ", Color.Black);
                //Thread.Sleep(1500);
                //this.InsertText(Form1.adb("shell getprop ro.product.brand"), Color.DarkOrange);
                //Thread.Sleep(1500);
                //this.InsertText("Model: ", Color.Black);
                //Thread.Sleep(1500);
                //this.InsertText(Form1.adb("shell getprop ro.product.model"), Color.DarkOrange);
                //Thread.Sleep(1500);
                //this.InsertText("Android Version: ", Color.Black);
                //Thread.Sleep(1500);
                //this.InsertText(Form1.adb("shell getprop ro.build.version.release"), Color.DarkOrange);
                //Thread.Sleep(1500);
                //this.InsertText("Sale Code: ", Color.Black);
                //Thread.Sleep(1500);
                //this.InsertText(Form1.adb("shell getprop persist.audio.sales_code"), Color.DarkOrange);
                //Thread.Sleep(1500);
                //this.InsertText("Country: ", Color.Black);
                //Thread.Sleep(1500);
                //this.InsertText(Form1.adb("shell getprop ro.csc.country_code"), Color.DarkOrange);
                //Thread.Sleep(1500);
                //this.InsertText("RIL Version: ", Color.Black);
                //Thread.Sleep(1500);
                //this.InsertText(Form1.adb("shell getprop gsm.version.ril-impl"), Color.DarkOrange);
                //Thread.Sleep(1500);
                //this.InsertText("Software Version: ", Color.Black);
                //Thread.Sleep(1500);
                //this.InsertText(Form1.adb("shell getprop ril.sw_ver"), Color.DarkOrange);
                //Thread.Sleep(1500);
                //this.InsertText("Bootloader Version: ", Color.Black);
                //Thread.Sleep(1500);
                //this.InsertText(Form1.adb("shell getprop ro.bootloader"), Color.DarkOrange);
                //Thread.Sleep(1500);

                //this.InsertText("Device Identifier: ", Color.Black);
                //Thread.Sleep(1500);
                //this.InsertText(Form1.adb("shell getprop ro.boot.em.did"), Color.DarkOrange);
                //this.InsertText("Build Number: ", Color.Black);
                //Thread.Sleep(1500);
                //this.InsertText(Form1.adb("shell getprop ro.build.display.id"), Color.DarkOrange);
                //Thread.Sleep(1500);
                //this.InsertText("Product Board: ", Color.Black);
                //Thread.Sleep(1500);
                //this.InsertText(Form1.adb("shell getprop ro.product.board"), Color.DarkOrange);
                //Thread.Sleep(1500);
                //this.InsertText("Security Patch: ", Color.Black);
                //Thread.Sleep(1500);
                //this.InsertText(Form1.adb("shell getprop ro.build.version.security_patch"), Color.DarkOrange);
                //Thread.Sleep(1500);
                //this.InsertText("Software Released Date: ", Color.Black);
                //Thread.Sleep(1500);
                //this.InsertText(Form1.adb("shell getprop ro.build.date"), Color.DarkOrange);
                //Thread.Sleep(1500);

                //this.InsertText("SIM Operator: ", Color.Black);
                //Thread.Sleep(1500);
                //this.InsertText(Form1.adb("shell getprop gsm.sim.operator.alpha"), Color.DarkOrange);
                //Thread.Sleep(1500);

                //this.InsertText("SIM Operator Country: ", Color.Black);
                //Thread.Sleep(1500);
                //this.InsertText(Form1.adb("shell getprop gsm.sim.operator.iso-country"), Color.DarkOrange);
                //Thread.Sleep(1500);

                //this.InsertText("Checking Frp Lock...", Color.Black);
                //Thread.Sleep(2000);
                //this.InsertText("OK", Color.Green);
                //Thread.Sleep(1500);

                //this.InsertText("\nUnlocking Frp...", Color.Black);
                //Thread.Sleep(5000);

                Form1.adb("am start -n com.google.Android.gsf.login/");
                Thread.Sleep(1500);
                Form1.adb("shell am start -n com.google.Android.gsf.login.LoginActivity");
                Thread.Sleep(1500);
                Form1.adb("shell content insert --uri content://settings/secure --bind name:s:user_setup_complete --bind value:s:1");
                this.InsertText("OK", Color.Lime);
                Thread.Sleep(1500);
                 Form1.adb("reboot");

                


            }
            else
            {

                this.InsertText(" No device detected!", Color.Red);
                this.InsertText("\n", Color.Red);
                this.InsertText("\nCannot find any devices connected in ADB Mode.", Color.OrangeRed);
                this.InsertText("\nRepeat Enable ADB operation.", Color.DarkRed);

            }

        }
    }
    }


