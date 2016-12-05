
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
		//each player initializes listener on their own IP, 
		//connects to the IP of the other player (hard-coded)
		IPAddress ipAddress_me;
		IPAddress ipAddress_other;

		TcpListener tcplistener;
		TcpClient tcpclient;
		
		//whoever connects to listener
		TcpClient connectedUser;
		NetworkStream stream;
		
		Game game;
		
		public MultiPlayerManager(RichTextBox console, Game game)
		{
			this.console = console;
			this.game = game;
			
			port = 3000;
			hostname = Dns.GetHostName();
			//more robust than hard-coding
			IPHostEntry ipHost = Dns.GetHostEntry(hostname);
			
			//IPv6 format
			//list of IP addresses that are associated with a host
			ipAddress_me = ipHost.AddressList[0];
			/*
			foreach (IPAddress addr in ipHost.AddressList) {
				println(addr.ToString());
			}
			*/
			
			//IPv4 format
			//ipAddress = IPAddress.Parse("129.59.122.21");
			
			//TEMPORARY
			ipAddress_other = ipAddress_me;
			
			StartReceiver();
			
			StartClient();
			

			
			println("");
			println("Multiplayer Manager initialized.");
			
		}
		void StartReceiver()
		{
			//initializing "receiver"
			tcplistener = new TcpListener(ipAddress_me, port);
			//start receiving messages
			tcplistener.Start();
			println("Tcp Listener started on IP " + ipAddress_me + ", listening on port " + port);
			
			//blocking, easier
			
			
			while(true)
			{
				if (tcplistener.Pending()) {
					//AcceptTcpClient is easier, but less flexibility, must be blocking synchronous
					connectedUser = tcplistener.AcceptTcpClient();
					break;
				}
				else
				{
					Thread.Sleep(100);
				}
			}
			
			
			// Create the listener socket
			/*
			Socket socket_listener = new Socket(
				//why AddressFamily.InterNetwork?
				                         AddressFamily.InterNetwork,
				                         SocketType.Stream, 
				                         ProtocolType.Tcp);
			socket_listener.Bind(new IPEndPoint(ipAddress_other, port));
// For use with localhost 127.0.0.1
			socket_listener.Listen(10);

// Setup a callback to be notified of connection requests
			socket_listener.BeginAccept(new AsyncCallback(app.OnConnectRequest), listener);
			*/
			
			
			
			//handle user added...
			//handleUserAdded();
		}
		void StartClient()
		{
			//initializing "sender" and connects to Host
			tcpclient = new TcpClient(new IPEndPoint(ipAddress_other, port));
			println("Tcp Client started, connecting to IP " + ipAddress_other + ", on port " + port);
		}
		public void handleUserAdded(Player other)
		{
			if (game.networkedPlayer == null) {
				//initialize data for networked player from local player
				game.networkedPlayer = other;
				println("Another player connected.");
			} else {
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
