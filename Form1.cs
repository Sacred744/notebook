using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.IO;
using System.Xml;



namespace 记事本
{
    public partial class Form1 : Form
    {
        private ContextMenuStrip contextMenuStrip;

        //private JArray TabData = JArray.Parse("[{Data:\"1\"},{Data:\"2\"},{Data:\"3\"}]")

        //private List<String> TabNames = new List<string>{"1","2","3" };
        private JArray TabData ;
        private List<String> TabNames ;
        private bool StartUpStates ;
        private bool ShowTopStates ;

        private ToolStripMenuItem StartUpMenu;
        private ToolStripMenuItem ShowTopMenu;
        Form2 form2 = new Form2();

        public Form1()
        {
            // Properties.Settings.Default["TabNames"] = "1,2,3";
            // Properties.Settings.Default.Save();
            this.Icon = Properties.Resources.book;
            InitConfig();
            InitData();
            InitializeComponent();
            InitTab();
            InitTimer();
            InitMenu();
        }

        public void InitTimer() {
            var timer = new Timer();
            timer.Interval = 500;
            timer.Tick += SaveDataTimer;
            timer.Start();
        }


        public void InitData() {
            TabData = JArray.Parse(Properties.Settings.Default["TabData"].ToString());
            TabNames = Properties.Settings.Default["TabNames"].ToString().Split(',').ToList();
        }

        public void InitTab() {

            tabControl1.TabPages.Clear();

            for (int i = 0; i < TabNames.Count; i++)
            {
                TabPage tabPage = new TabPage(TabNames[i].ToString());
                TextBox textBox = new TextBox();
                textBox.Multiline = true;
                textBox.Dock = DockStyle.Fill; // 填充整个父控件的空间
                textBox.ScrollBars = ScrollBars.Vertical;
                textBox.Font = new Font("宋体", 12, FontStyle.Regular);
                try
                {
                    textBox.Text = TabData[i]["Data"].ToString();
                }catch
                {
                    textBox.Text = "";
                }
                

                tabPage.Controls.Add(textBox);
                tabControl1.TabPages.Add(tabPage);
            }

        }

        public void SaveDataTimer(object sender, EventArgs e)
        {
            SaveData();
        }

        public void SaveData()
        {
            TabData.Clear();
            for (int i = 0; i < tabControl1.TabPages.Count; i++)
            {
                TabPage tabPage1 = tabControl1.TabPages[i];
                TextBox textBox1 = tabPage1.Controls.OfType<TextBox>().FirstOrDefault();
                string text1 = textBox1.Text;
                TabData.Add(MakeToken(text1));
            }
            Properties.Settings.Default["TabData"] = TabData.ToString();
            Properties.Settings.Default.Save();
            
        }

        public JToken MakeToken(string text) {
        JToken token = JToken.Parse("{Data:\"\"}");
            token["Data"]= text;
            return token;
        }
        private void InitMenu()
        {
            // 创建右键菜单
            contextMenuStrip = new ContextMenuStrip();

            // 添加菜单项
            ToolStripMenuItem MangeMenu = new ToolStripMenuItem("标签页管理");

             StartUpMenu = new ToolStripMenuItem("开机自启");
             ShowTopMenu = new ToolStripMenuItem("置顶显示");
  
            contextMenuStrip.Items.Add(MangeMenu);
            contextMenuStrip.Items.Add(StartUpMenu);
            contextMenuStrip.Items.Add(ShowTopMenu);

            if (StartUpStates)
            {
                StartUpMenu.Checked = true;
            }else
            {
                StartUpMenu.Checked = false;
            }

            if (ShowTopStates)
            {
                ShowTopMenu.Checked = true;
            }
            else
            {
                ShowTopMenu.Checked = false;
            }
            ShowTop(ShowTopStates);


            MangeMenu.Click += MangeMenu_Click;
            StartUpMenu.Click += StartUpMenu_Click;
            ShowTopMenu.Click += ShowTopMenu_Click;
            // 注册右键菜单事件处理程序
            this.ContextMenuStrip = contextMenuStrip;
            this.MouseDown += MainForm_MouseDown;
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            // 在鼠标右键按下时显示右键菜单
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip.Show(this, e.Location);
            }
        }
        private void MangeMenu_Click(object sender, EventArgs e) {
            SaveData();
            form2.FormClosed += Form2_FormClosed;
            ShowTop(false);
            form2.ShowDialog();
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 确保 sender 是 Form2 类型
            if (sender is Form2)
            {
                // 调用 Form1 的 InitTab 方法
                InitTab();
                ShowTop(ShowTopStates);
            }
        }

        private void StartUpMenu_Click(object sender, EventArgs e)
        {
            string appName = "记事本"; // 替换为你的应用程序名称
            string appPath = Application.ExecutablePath; // 获取应用程序的路径

            // 检查程序是否设置为开机自启
           bool isStartup = IsStartupEnabled(appName);

            if (isStartup)
            {
                // 如果已设置为开机自启，则取消设置
                RemoveFromStartup(appName);
                //MessageBox.Show("程序已取消开机自启。");
                Properties.Settings.Default["StartUp"] = false;
                Properties.Settings.Default.Save();
                StartUpMenu.Checked = false;
            }
            else
            {
                // 如果未设置为开机自启，则添加设置
                AddToStartup(appName, appPath);
                //MessageBox.Show("程序已设置为开机自启。");
                Properties.Settings.Default["StartUp"] = true;
                Properties.Settings.Default.Save();
                StartUpMenu.Checked = true;
            }
            

        }
        private void ShowTopMenu_Click(object sender, EventArgs e)
        {
            ShowTopMenu.Checked = !ShowTopMenu.Checked;
            ShowTopStates = !ShowTopStates;
            if (ShowTopMenu.Checked) {
                ShowTop(ShowTopStates);
                Properties.Settings.Default["ShowTop"] = true;
                Properties.Settings.Default.Save();
            }
            else {
                ShowTop(ShowTopStates);
                Properties.Settings.Default["ShowTop"] = false;
                Properties.Settings.Default.Save();

            }
            
        }

        private void ShowTop(bool states)
        {
            this.TopMost = states;

        }

        private bool IsStartupEnabled(string appName)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"))
            {
                return key != null && key.GetValue(appName) != null;
            }
        }

        // 添加程序到开机自启
        private void AddToStartup(string appName, string appPath)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                key.SetValue(appName, appPath);
            }
        }

        // 从开机自启中移除程序
        private void RemoveFromStartup(string appName)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (key != null)
                {
                    key.DeleteValue(appName, false);
                }
            }
        }

        private void InitConfig()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default["TabNames"].ToString()))
            {
                // 设置项尚未初始化，进行初始化操作
                Properties.Settings.Default["TabName"] = "新标签";
                Properties.Settings.Default["TabData"] = "[{Data:\"\"}]";
                Properties.Settings.Default["StartUp"] = false;
                Properties.Settings.Default["ShowTop"] = false;
                // 保存更改
                Properties.Settings.Default.Save();
         TabData = JArray.Parse(Properties.Settings.Default["TabData"].ToString());
         TabNames = Properties.Settings.Default["TabNames"].ToString().Split(',').ToList();
         StartUpStates = (bool)Properties.Settings.Default["StartUp"];
         ShowTopStates = (bool)Properties.Settings.Default["ShowTop"];
    }
        }
    }
}

