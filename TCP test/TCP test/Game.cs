
using System;
using System.Windows.Forms;

namespace TCP_test
{
	/// <summary>
	/// Description of Game.
	/// </summary>
	public class Game
	{
		MultiPlayerManager manager;
		RichTextBox console;
		public Player localPlayer;
		public Player networkedPlayer;
		public Game(RichTextBox console)
		{
			this.console = console;
			manager = new MultiPlayerManager(console, this);
			
			localPlayer = new Player(3f, 25f);
			networkedPlayer = null;
			
			println("Game initialized");
			println("");
		}
		
		void println(string s)
		{
			console.AppendText("\n" + s);
		}
	}
}
