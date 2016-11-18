﻿
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TCP_test
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		
		Game game;
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			game = new Game(richTextBox1);
		}
	}
}
