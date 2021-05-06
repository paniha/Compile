using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace Compile
{

    public partial class Form1 : Form
    {

        List<Operation> Ops = new List<Operation>();
        public void GetOps()
        {
            ConsoleTextBox.Text += "Reading OP list from ops.json..." + Environment.NewLine;
            string store = "";
            if (File.Exists("ops.json"))
            {
                try
                {
                    store = File.ReadAllText("ops.json");
                    Ops = JsonConvert.DeserializeObject<List<Operation>>(store);

                    ConsoleTextBox.Text += "Done! " + Ops.Count + " OPs loaded." + Environment.NewLine;
                }
                catch (Newtonsoft.Json.JsonReaderException e)
                {
                    ConsoleTextBox.Text += "ERROR in ops.json: " + e.Message + Environment.NewLine;
                }
            }
            else
            {
                ConsoleTextBox.Text += "ERROR: ops.json not found" + Environment.NewLine;
            }
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Shown(Object sender, EventArgs e)
        {
           OutputTypeList.SelectedItem = "Binary";
           GetOps();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((string)OutputTypeList.SelectedItem == "Binary")
                CScheckBox.Enabled = true;
            else
                CScheckBox.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            OutputBox.Clear();
            Operation op = new Operation();
            string compiled = "";
            string[] lineIn = new string[4];
            string[] lineInTemp;
            string lineOut = "";
            int lineNum = 0;
            int hexLine = 0;
            foreach (string element in InputBox.Lines)
            {
                for (int i = 0; i < 4; i++) { lineIn[i] = "0000"; }
                lineInTemp = element.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                for(int i = 0; i < lineInTemp.Length; i++) { lineInTemp[i].Trim(); }
                for (int i = 0; i < lineInTemp.Length; i++) { lineIn[i] = lineInTemp[i]; }
                try
                {
                    op = Ops.Find(x => x.mnemonic == lineIn[0]);
                    if(op == null)
                    {
                        ConsoleTextBox.Text += "ERROR on line " + lineNum + ": operation \"" + lineIn[0] + "\" not recognized" + Environment.NewLine;
                        return;
                    }
                    lineOut = op.OPCode;
                    if (op.parameters.Length == lineInTemp.Length - 1)
                    {
                        if (op.parameters.Contains('D'))
                            if (lineIn[1][0] == 'D' && lineIn[1][1] - '0' <= 7)
                                lineOut += Convert.ToString(lineIn[1][1] - '0', 2).PadLeft(3, '0');
                            else
                            {
                                ConsoleTextBox.Text += "ERROR on line " + lineNum + ": invalid address for \"D\"" + Environment.NewLine;
                                return;
                            }
                        else
                            lineOut += "000";

                        if (op.parameters.Contains('A'))
                            if (lineIn[1][0] == 'A' && lineIn[1][1] - '0' <= 7)
                                lineOut += Convert.ToString(lineIn[1][1] - '0', 2).PadLeft(3, '0');
                            else if((lineIn[2][0] == 'A' && lineIn[2][1] - '0' <= 7))
                                lineOut += Convert.ToString(lineIn[2][1] - '0', 2).PadLeft(3, '0');
                            else
                            {
                                ConsoleTextBox.Text += "ERROR on line " + lineNum + ": invalid address for \"A\""  + Environment.NewLine;
                                return;
                            }
                        else
                            lineOut += "000";

                        if (op.parameters.Contains('B'))
                            if (lineIn[1][0] == 'B' && lineIn[1][1] - '0' <= 7)
                                lineOut += Convert.ToString(lineIn[1][1] - '0', 2).PadLeft(3, '0');
                            else if ((lineIn[2][0] == 'B' && lineIn[2][1] - '0' <= 7))
                                lineOut += Convert.ToString(lineIn[2][1] - '0', 2).PadLeft(3, '0');
                            else if ((lineIn[3][0] == 'B' && lineIn[3][1] - '0' <= 7))
                                        lineOut += Convert.ToString(lineIn[3][1] - '0', 2).PadLeft(3, '0');  
                            else
                            {
                                ConsoleTextBox.Text += "ERROR on line " + lineNum + ": invalid address for \"B\"" + Environment.NewLine;
                                return;
                            }
                        else
                            lineOut += "000";

                    }
                    else
                    {
                        ConsoleTextBox.Text += "ERROR on line " + lineNum +": invalid number of parameters" + Environment.NewLine;
                        return;
                    }
                }
                catch(ArgumentNullException)
                {
                    ConsoleTextBox.Text += "ERROR on line " + lineNum + ": operation \"" + lineIn[0] + "\" not recognized" + Environment.NewLine;
                    return;
                }
                if((string)OutputTypeList.SelectedItem == "Binary")
                {
                if (CScheckBox.Checked  && InputBox.Lines.Length > lineNum + 1)
                    lineOut += ",";
                    //if (MLcheckBox.Checked && InputBox.Lines.Length > lineNum + 1)
                    compiled += lineOut + Environment.NewLine;
                    //else
                    
                    //compiled += lineOut;
                }
                else
                {
                    compiled = compiled.Insert(hexLine, Convert.ToInt32(lineOut, 2).ToString("X").PadLeft(4, '0'));
                    if (lineNum == 15)
                    {
                        compiled += Environment.NewLine;
                        hexLine += 65;
                        lineNum = 0;
                    }
                }
                lineNum++;
            }
            stopwatch.Stop();
            OutputBox.Text = compiled;
            ConsoleTextBox.AppendText("Program compiled successfully in  " + stopwatch.ElapsedMilliseconds + "ms." + Environment.NewLine);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            LineBox.Text = "";
            int n = 0;
            while(InputBox.Lines.Length + 1 > LineBox.Lines.Length)
                LineBox.Text +=("" + n++ + Environment.NewLine);   
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "txt files (*.txt)|*.txt|Assembly Source (*.asm*)|*.asm*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var fileStream = openFileDialog.OpenFile();

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        InputBox.Text = reader.ReadToEnd();
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Stream myStream;
            using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
            {
                saveFileDialog1.Filter = "txt files (*.txt)|*.txt|Assembly Source (*.asm*)|*.asm*";
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {

                    if ((myStream = saveFileDialog1.OpenFile()) != null)
                    {
                        using (StreamWriter writer = new StreamWriter(myStream))
                        {
                             writer.Write(InputBox.Text);
                        }
                        ConsoleTextBox.Text += "Source saved to file." + Environment.NewLine;
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Stream myStream;
            using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
            {
                saveFileDialog1.Filter = "txt files (*.txt)|*.txt|Assembly Source (*.asm*)|*.asm*";
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {

                    if ((myStream = saveFileDialog1.OpenFile()) != null)
                    {
                        using (StreamWriter writer = new StreamWriter(myStream))
                        {
                            writer.Write(OutputBox.Text);
                        }
                        ConsoleTextBox.Text += "Output saved to file." + Environment.NewLine;
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(OutputBox.Text);
            ConsoleTextBox.Text += "Output copied to clipboard." + Environment.NewLine;
        }
    }
    public class Operation
    {
        public string mnemonic;
        public string OPCode;
        public string parameters;
    }
}
