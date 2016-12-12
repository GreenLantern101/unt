
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Server
{
	// Listens for new incoming connections
	private TcpListener tcpListener;
		
	// Clients objects
	public Client client;
	//tcp client object of whatever is connecting to this server
	private TcpClient tcpClient_other = null;

	// Game stuff
	private Thread gameThread = null;

	// Other data
	public readonly string Name;
	public readonly int port_me;
	public bool Running { get; private set; }
	
	public GameController _currentGame;

	public Server()
	{
		Name = "SERVER_ONE";
		port_me = 32887;
		Running = false;
			
		// Create the listener, listening at any ip address
		tcpListener = new TcpListener(IPAddress.Any, port_me);
			
		//might need to use something like this to make public host
		//tcpListener = new TcpListener(IPAddress.Parse(Dns.GetHostName()), port_me);
	}
		
	// returns the private network IP address of server
	public static IPAddress GetLocalIPAddress()
	{
		var host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (var ip in host.AddressList) {
			if (ip.AddressFamily == AddressFamily.InterNetwork) {
				Debug.Log("IP: " + ip);
				return ip;
			}
		}
		throw new Exception("Local IP Address not found.");
	}

	public void Shutdown()
	{
		if (Running) {
			Running = false;
			Debug.Log("Shutting down server...");
		}
		// gracefully disconnect client...
		if (client != null)
			client.Disconnect();
	}
	public void Start(GameController game)
	{
		//------------------------------------------------ start server
		Debug.Log("============= Starting the " + Name + " server on port " + port_me + "."); 
			
		// Start running the server
		tcpListener.Start();
		Running = true;
		Debug.Log("Waiting for incoming connections...");
		
		Thread server_conn = new Thread(new ThreadStart(ServerConnectLoop));
		server_conn.Start();

		//------------------------------------------------- start client
		
		client = new Client();
		//connect game client...
		client.Connect();
	
		//server_conn.Join();
		
		//------------------------------------------------- run server & client
		
		this._currentGame = game;
		this.RunLoop();
	}
	void ServerConnectLoop()
	{
		while (!tcpListener.Pending()) {
			Thread.Sleep(100);
		}
		_handleNewConnection();
	}

	void RunLoop()
	{
		//Start a game for the first new connection
		
		//add networked player to game
		this._currentGame.AddPlayer(tcpClient_other);
		Debug.Log("A networked player has been added to the game.");
					
		//SYNC GAME AT BEGINNING immediately after connecting
		this._currentGame.SyncGame_command();
		
	
		while (Running) {
			//------------------------------------------------- client run cycle
			
			// Check for new packets
			client._handleIncomingPackets();
				
			//poll for local player input changes
			if (client.changed_local) {
				_currentGame.HandleInputAction(client.action_local);
				//reset flag
				client.changed_local = false;
			}
			//poll for remote player input changes
			if (client.changed_remote) {
				_currentGame.SyncGame_obey(client.action_remote);
				//reset flag
				client.changed_remote = false;
			}

			// Make sure that we didn't have a graceless disconnect
			if (IsDisconnected(this.client.tcpClient)
			     && !this.client._clientRequestedDisconnect) {
				Running = false;
				Debug.Log("Other server disconnected from us ungracefully.");
				Thread.Sleep(3000);
			}
			
				
			//--------------------------------------------------- Take a small nap
			Thread.Sleep(10);
		}

		//-------------------------------------------------------- server STOP

		// Disconnect any clients remaining
		if (tcpClient_other != null && tcpClient_other.Connected)
			DisconnectClient(tcpClient_other, "Other server is shutting down.");

		// Cleanup our resources
		tcpListener.Stop();

		// Info
		Debug.Log("The server has been shut down.");
			
		//-------------------------------------------------------- client STOP

		// Cleanup
		this.client._cleanupNetworkResources();
	}

	// Awaits for a new connection, sets it to networked client
	private void _handleNewConnection()
	{
		// Get the new client using a Future
		this.tcpClient_other = tcpListener.AcceptTcpClient();
		Debug.Log("New connection from " + tcpClient_other.Client.RemoteEndPoint);

		// Send a welcome message
		string msg = String.Format("Welcome to the \"{0}\" server.\n", Name);
		Packet.SendPacket(tcpClient_other.GetStream(), new Packet("message", msg));
	}

	// Checks if a client has disconnected ungracefully
	public static bool IsDisconnected(TcpClient client)
	{
		try {
			Socket s = client.Client;
			return s.Poll(10 * 1000, SelectMode.SelectRead) && (s.Available == 0);
		} catch (SocketException) {
			// If socket error, assume it's disconnected
			return true;
		}
	}
	// Gracefully disconnect a TcpClient
	public void DisconnectClient(TcpClient client, string message)
	{
		Debug.Log("Disconnecting the client from " + client.Client.RemoteEndPoint);

		// If no message set, use the default "Goodbye."
		if (message == "")
			message = "Goodbye.";

		// Send the "bye" message
		Packet.SendPacket(client.GetStream(), new Packet("bye", message));

		// Give the client some time to send and process the graceful disconnect
		Thread.Sleep(500);

		CleanupClient(client);
	}
		
	// cleans up resources for a TcpClient and closes it
	public void CleanupClient(TcpClient client)
	{
		client.GetStream().Close();     // Close network stream
		client.Close();                 // Close client
		//this.tcpClient_other = null;
	}

	// Will get a single packet from a TcpClient
	// Returns null if no data available or issue
	public Packet ReceivePacket(TcpClient client)
	{
		Packet packet = null;
		try {
			// First check there is data available
			if (client.Available == 0)
				return null;

			NetworkStream _msgStream = client.GetStream();
			packet = Packet.getPacketFromStream(_msgStream);

		} catch (Exception e) {
			// There was an issue in receiving
			Debug.Log("There was an issue receiving a packet from " + client.Client.RemoteEndPoint);
			Debug.Log("Reason: " + e.Message);
		}

		return packet;
	}

}
