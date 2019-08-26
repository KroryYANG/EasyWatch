using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;


namespace EasyWatch
{
    public partial class Form1 : Form
    {
        SerialPort serialPort = new SerialPort();
        private delegate void SetTextCallback(string text);

        public Form1()
        {
            
            InitializeComponent();
        }

        //在给textBox1.text赋值的地方调用以下方法即可
        private void SetText(string text)
        {
            // InvokeRequired需要比较调用线程ID和创建线程ID
            // 如果它们不相同则返回true
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox1.AppendText(text);
            }
        }

        //选择串口
        private void comboBox1_Click(object sender, EventArgs e)
        {
            if(serialPort.IsOpen)
            {
                MessageBox.Show("请先关闭当前串口");
                return;
            }
            comboBox1.Items.Clear();
            string[] ports = SerialPort.GetPortNames();

            foreach (string port in ports)
            {
                comboBox1.Items.Add(port);
            }


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox2.Text = comboBox2.Items[3].ToString(); //波特率
            comboBox3.Text = comboBox3.Items[3].ToString(); //数据位
            comboBox4.Text = comboBox4.Items[0].ToString(); //校验位
            comboBox5.Text = comboBox5.Items[0].ToString(); //停止位


            serialPort.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);   //注册串口接收事件，给事件的委托添加处理方法            
        }

        //串口接收事件处理方法
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs args)
        {

            if (radioButton2.Checked)
            {
                byte[] data = new byte[serialPort.BytesToRead];
                serialPort.Read(data, 0, serialPort.BytesToRead);
                if (checkBox1.Checked)
                {
                    SetText(DateTime.Now.ToString("[HH:mm:ss] "));  //显示时间
                }
                for (int i = 0; i < data.Length; i++)
                {
                    string str = Convert.ToString(data[i], 16).ToUpper();   //把byte以16进制的形式转化为字符串，并大写
                    SetText(str.Length == 1 ? "0" + str : str);   //补0
                    SetText(" ");
                }
            }
            else if (radioButton1.Checked)
            {
                char[] data = new char[serialPort.BytesToRead];
                serialPort.Read(data, 0, serialPort.BytesToRead);
                if (checkBox1.Checked)
                {
                    SetText(DateTime.Now.ToString("[HH:mm:ss] "));  //显示时间
                }
                string str = new string(data);
                SetText(str);          
            }

            SetText("\n");  //换行
        }

        //打开/关闭串口
        private void button1_Click(object sender, EventArgs e)
        {
        
                if (button1.Text == "打开串口")
                {
                    if (serialPort.IsOpen == false)
                    {
                        if(comboBox1.Text == "")
                        {
                        MessageBox.Show("请选择串口");
                            return;
                        }
                        serialPort.PortName = comboBox1.Text;                        
                        serialPort.BaudRate = Convert.ToInt32(comboBox2.Text);
                        serialPort.DataBits = Convert.ToInt32(comboBox3.Text);
                        switch (comboBox4.Text)
                        {
                            case "None":
                                serialPort.Parity = Parity.None;
                                break;
                            case "Even":
                                serialPort.Parity = Parity.Even;
                                break;
                            case "Odd":
                                serialPort.Parity = Parity.Odd;
                                break;
                        }
                        switch (comboBox5.Text)
                        {
                            case "1":
                                serialPort.StopBits = StopBits.One;
                                break;
                            case "2":
                                serialPort.StopBits = StopBits.Two;
                                break;
                            case "1.5":
                                serialPort.StopBits = StopBits.OnePointFive;
                                break;
                        }

                        serialPort.ReadBufferSize = 10 * 1024;

                    try
                    {
                        serialPort.Open();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }

                    button1.Text = "关闭串口";
                    label3.Text = "开启";


                    }
                }
                else
                {
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();
                    }
                    button1.Text = "打开串口";
                    label3.Text = "关闭";
                }
         
        }


        //串口发送
        private void button2_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen == false)
            {
                MessageBox.Show("串口未开启");
                return;
            }
            if (radioButton2.Checked) //Hex发送
            {
                string str = textBox2.Text;
                string[] strs = str.Split(' ');
                byte[] data = new byte[strs.Length];
                for (int i = 0; i < strs.Length; i++)
                {
                    data[i] = Convert.ToByte(strs[i].Substring(0, 2), 16);
                }
                if (checkBox2.Checked)  //显示发送
                {

                    if (checkBox1.Checked) { SetText(DateTime.Now.ToString("[HH:mm:ss] ") + str.ToUpper() + '\n'); }    //显示时间
                    else { SetText(str.ToUpper() + '\n'); } //不显示时间
                }
                try
                {
                    serialPort.Write(data, 0, strs.Length); //写数据到串口}
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
            else if (radioButton1.Checked) //ASCII发送
            {
                string str = textBox2.Text;

                if (checkBox2.Checked)  //显示发送
                {
                    if (checkBox1.Checked) { SetText(DateTime.Now.ToString("[HH:mm:ss] ") + str + '\n'); }    //显示时间
                    else { SetText(str + '\n'); } //不显示时间
                }
                try
                {
                    serialPort.Write(str); //写数据到串口
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }


            }


        }
        //窗口关闭时关闭串口并释放资源
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            serialPort.Close();
            this.Dispose();
        }

        //清屏
        private void button3_Click(object sender, EventArgs e)
        {
            this.textBox1.Clear();
        }
    }
}


