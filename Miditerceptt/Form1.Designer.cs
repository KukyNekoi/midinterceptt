
using System;

using System.Text.Json;
using WebSocketSharp.Server;
using WebSocketSharp;
using NAudio.Midi;
using System.Collections.Generic;

namespace Miditerceptt
{

    public class MidiThing : WebSocketBehavior
    {
        public static MidiThing MidiThingSingletonReference = null;
        public void Broadcast(string data)
        {
            Sessions.Broadcast(data);
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            Send("tuturu~~");
            if (MidiThingSingletonReference == null)
            {
                MidiThingSingletonReference = this;
            }
        }

    }
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        public static bool debugMode = false;
        static Queue<int> midiClockTimingQueue;
        static double currentBpm;
        static MidiIn device;
        static string messages = "";
        WebSocketServer wss;


        public void stopServer()
        {
            wss.Stop();
            device.Stop();
            addMessage("Server Stopped...");

            StopButton.Enabled = false;
            StartButton.Enabled = true;
            debugModeCheckbox.Enabled = true;
        }
        public void startServer()
        {

            device = new MidiIn(deviceSelectionCB.SelectedIndex);
            device.MessageReceived += midiInMessage;
            device.Start();
            addMessage("Midi interceptor running...");

            wss = new WebSocketServer(9999);
            wss.AddWebSocketService<MidiThing>("/midiTick");
            wss.Start();
            addMessage("Webserver socket online...");
            addMessage("Open index.html on OBS or a local browser to use it...");

            StatusLabel.Text = "Server status: Running";
            if (this.debugModeCheckbox.Checked)
            {
                debugMode = true;
            }
            else
            {
                debugMode = false;
            }
            StopButton.Enabled = true;
            StartButton.Enabled = false;
            debugModeCheckbox.Enabled = false;

        }

        private static void addMessage(string text)
        {
            //messages += String.Format("{0}\r\n", text);
            Console.WriteLine(text);
        }

        static void midiInMessage(object senderObject, MidiInMessageEventArgs args)
        {
            if (debugMode)
            {
                addMessage(String.Format("ev: {0} -- raw: {1} -- timestamp: {2}", args.MidiEvent, args.RawMessage, args.Timestamp));
            }

            if (args.MidiEvent.CommandCode == MidiCommandCode.TimingClock)
            {
                if (midiClockTimingQueue.Count > 23)
                {
                    int lastValue = midiClockTimingQueue.Dequeue();
                    // probably this shit is not useful at all
                    int currentElapsedTime = (args.Timestamp - lastValue); // time elapsed between one note
                    currentBpm = 60000.0 / currentElapsedTime;
                    midiClockTimingQueue.Clear();
                    if (debugMode)
                    {
                        addMessage(String.Format("Current time between notes: {0}msecs", currentElapsedTime));
                        addMessage("{\"bpm\": " + currentBpm + "}");
                        addMessage("tick!");
                    }
                    MidiEvent curEvent = args.MidiEvent;
                    MidiThing.MidiThingSingletonReference?.Broadcast(JsonSerializer.Serialize(new
                    {
                        type = curEvent.CommandCode.ToString(),
                        bpm = currentBpm
                    }));
                }
                midiClockTimingQueue.Enqueue(args.Timestamp);
            }

            else if (args.MidiEvent.CommandCode == MidiCommandCode.NoteOn || args.MidiEvent.CommandCode == MidiCommandCode.NoteOff)
            {
                NoteEvent curEvent = (NoteEvent)args.MidiEvent;
                MidiThing.MidiThingSingletonReference?.Broadcast(JsonSerializer.Serialize(new
                {
                    type = curEvent.CommandCode.ToString(),
                    noteName = curEvent.NoteNumber,
                    noteNumber = curEvent.NoteNumber,
                    velocity = curEvent.Velocity,
                    bpm = currentBpm
                }));
            }


        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            currentBpm = 0;
            midiClockTimingQueue = new Queue<int>();
            List<string> deviceList = new List<string>();

            for (int curDevice = 0; curDevice < MidiIn.NumberOfDevices; curDevice++)
            {
                Console.WriteLine(
                    String.Format("Device index {0} - Name: {1} - ID: {2}",
                        curDevice,
                        MidiIn.DeviceInfo(curDevice).ProductName,
                        MidiIn.DeviceInfo(curDevice).ProductId));
                deviceList.Add(String.Format("Device index {0} - Name: {1} - ID: {2}",
                        curDevice,
                        MidiIn.DeviceInfo(curDevice).ProductName,
                        MidiIn.DeviceInfo(curDevice).ProductId));
            }
            deviceSelectionCB.DataSource = deviceList;
            deviceSelectionCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;


        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.deviceSelectionCB = new System.Windows.Forms.ComboBox();
            this.StartButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.debugModeCheckbox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // deviceSelectionCB
            // 
            this.deviceSelectionCB.FormattingEnabled = true;
            this.deviceSelectionCB.Location = new System.Drawing.Point(12, 12);
            this.deviceSelectionCB.Name = "deviceSelectionCB";
            this.deviceSelectionCB.Size = new System.Drawing.Size(776, 33);
            this.deviceSelectionCB.TabIndex = 0;
            this.deviceSelectionCB.Text = "Midi device to hookup";
            this.deviceSelectionCB.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(12, 51);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(112, 34);
            this.StartButton.TabIndex = 1;
            this.StartButton.Text = "Start Server";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // StopButton
            // 
            this.StopButton.Enabled = false;
            this.StopButton.Location = new System.Drawing.Point(130, 51);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(112, 34);
            this.StopButton.TabIndex = 2;
            this.StopButton.Text = "Stop Server";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // StatusLabel
            // 
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.Location = new System.Drawing.Point(248, 56);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(190, 25);
            this.StatusLabel.TabIndex = 3;
            this.StatusLabel.Text = "Server status: Stopped";
            // 
            // debugModeCheckbox
            // 
            this.debugModeCheckbox.AutoSize = true;
            this.debugModeCheckbox.Location = new System.Drawing.Point(651, 55);
            this.debugModeCheckbox.Name = "debugModeCheckbox";
            this.debugModeCheckbox.Size = new System.Drawing.Size(137, 29);
            this.debugModeCheckbox.TabIndex = 5;
            this.debugModeCheckbox.Text = "debugMode";
            this.debugModeCheckbox.UseVisualStyleBackColor = true;
            this.debugModeCheckbox.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(805, 111);
            this.Controls.Add(this.debugModeCheckbox);
            this.Controls.Add(this.StatusLabel);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.deviceSelectionCB);
            this.Name = "Form1";
            this.Text = "Miditerceptt - by NeKoi";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox deviceSelectionCB;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.CheckBox debugModeCheckbox;
    }
}

