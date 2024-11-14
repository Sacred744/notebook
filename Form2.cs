using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.IO;

namespace 记事本
{
    public partial class Form2 : Form
    {
        
        private List<String> TabNames ;
        private JArray TabData ;
        private Form InputForm = new Form();
        private string userInput;
        private TextBox textBox = new TextBox();
        private Button button = new Button();

        public Form2()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.book;
            InitData();
            InitListbox();
            InitInputForm();
            
        }

        public void InitData()
        {
            TabData = JArray.Parse(Properties.Settings.Default["TabData"].ToString());
            TabNames = Properties.Settings.Default["TabNames"].ToString().Split(',').ToList();
        }

        public void InitListbox()
        {

        listBox1.Items.Clear();
            foreach (String name in TabNames)
            {
                listBox1.Items.Add(name.ToString());
            }

        }


        private void buttonDelete_Click(object sender, EventArgs e)
        {
            
            DialogResult result = MessageBox.Show("确定要执行删除操作吗？", "警告", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                TabNames.Remove(listBox1.SelectedItem.ToString());
                TabData.Remove(TabData[listBox1.SelectedIndex]);
                InitListbox();
            }
            else if (result == DialogResult.Cancel)
            {
                
            }

        }

        private void buttonChecked_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default["TabNames"] = string.Join(",", TabNames);
            Properties.Settings.Default["TabData"] = TabData.ToString();
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            InitData();
            InputForm.ShowDialog();
            if (!string.IsNullOrEmpty(userInput)) {
                TabNames.Add(userInput);
                TabData.Add("");
                InitListbox();
            }
            
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }



        private void InitInputForm() {

            InputForm.Icon = Properties.Resources.book;
            InputForm.Size=new Size(300, 150);
            // 创建一个文本框控件
            
            textBox.Location = new System.Drawing.Point(10, 10);
            textBox.Width = 200;
            InputForm.Controls.Add(textBox);

            // 创建一个按钮控件
            
            button.Text = "确定";
            button.Location = new System.Drawing.Point(120, 40);
            InputForm.Controls.Add(button);

            // 创建一个按钮控件
            Button buttonCancel = new Button();
            buttonCancel.Text = "取消";
            buttonCancel.Location = new System.Drawing.Point(10, 40);
            InputForm.Controls.Add(buttonCancel);

            textBox.KeyDown += InputForm_KeyDown;

            // 按钮点击事件处理程序
            button.Click += button_Click;

            InputForm.FormClosing += (sender, e) =>
            {
                textBox.Text = "";
               
            };

            buttonCancel.Click += (sender, e) =>
            {
                InputForm.Close();
                userInput = null;

            };
            InputForm.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    InputForm.Close();
                    userInput = null;
                }
            };

        }

        void button_Click(object sender,EventArgs e)
        {
            userInput = textBox.Text;
            if (!string.IsNullOrEmpty(userInput))
            {
                InputForm.Close();
            }
            else
            {
                MessageBox.Show("不得为空");
            }
        }


        void InputForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button_Click(sender, e);
            }
            else if (e.KeyCode == Keys.Escape) {

                InputForm.Close();
                userInput = null;

            }
        }



        public void ReturnInput(object sender, FormClosedEventArgs e) 
        {
            if (sender is Form)
            {
                InputForm.FormClosed += ReturnInput;
            }
        }

        private void buttonRename_Click(object sender, EventArgs e)
        {
            InitData();
            if (TabNames[listBox1.SelectedIndex] != null)
            {
                InputForm.ShowDialog();
                if (!string.IsNullOrEmpty(userInput))
                {
                    TabNames[listBox1.SelectedIndex] = userInput;
                }
            }
            else {

                MessageBox.Show("先选择需要修改的项");
            }
            InitListbox();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}
