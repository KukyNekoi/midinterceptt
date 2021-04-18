using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using System.Text.Json;
using WebSocketSharp.Server;
using WebSocketSharp;
using NAudio.Midi;
using System.Collections.Generic;

namespace Miditerceptt
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            startServer();
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            stopServer();

        }
    }
}
