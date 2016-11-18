
using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace TCP_test
{
	/// <summary>
	/// manages multiplayer
	/// </summary>
	public class MultiPlayerManager
	{
		//display purposes
		RichTextBox console;
		
		string hostname;
		int port;
		//NOTE: ip address may need to be changed for each computer
		IPAddress ipAddress;

		TcpListener tcplistener;
		TcpClient tcpclient;
		
		Game game;
		
		public MultiPlayerManager(RichTextBox console, Game game)
		{
			this.console = console;
			this.game = game;
			
			//common vars for both sender & receiver
			port = 3000;
			hostname = Dns.GetHostName();	
			
			
			//more robust than hard-coding
			IPHostEntry ipHost = Dns.GetHostEntry(hostname);
			
			//returns IPv6 address format
			ipAddress = ipHost.AddressList[0];
			
			//IPv4 format
			//ipAddress = IPAddress.Parse("129.59.122.21");
			
			
			//initializing "receiver"
			tcplistener = new TcpListener(ipAddress, port);
			//start receiving messages
			tcplistener.Start();
			println("Tcp Listener initialized...");
			println("PORT: " + port);
			println("IP address: " + ipAddress);
			
			println("");
			
			//initializing "sender" and connects to Host
			tcpclient = new TcpClient(hostname, port);
			println("Tcp Client initialized...");
			println("PORT: " + port);
			println("Hostname: " + hostname);
			
			
			println("");
			
			println("Multiplayer Manager initialized.");
			
		}
		public void handleUserAdded(Player other)
		{
			if (game.networkedPlayer == null) {
				//initialize data for networked player from local player
				game.networkedPlayer = other;
				println("Another player connected.");
			}
			else
			{
				println("A player has already been connected.");
			}
		}
		/// <summary>
		/// called every time this LocalPlayer takes an action...
		/// AND called on every game Update loop???
		/// </summary>
		public void sendMessage(Message msg)
		{

		}
		/// <summary>
		/// occurs whenever message is recieved
		/// </summary>
		public void receiveMessage(Message msg)
		{
		
		}
		
		public void closeConnection()
		{
			tcplistener.Stop();
			tcpclient.Close();
		}
		
		
		void println(string s)
		{
			console.AppendText("\n" + s);
		}
	}
}
