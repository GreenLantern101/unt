﻿
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
	private Client client;
	//tcp client object of whatever is connecting to this server
	private TcpClient tcpClient_other = null;
	
	//return whether other remote client has connected to this server
	public bool isOtherClientConnected {
		get {
			return (tcpClient_other != null);
		}
	}

	// Other data
	const string Name = "SERVER_ONE";
	const int port_me = 32890;
	public bool Running { get; private set; }

	public Server()
	{
		Running = false;
		// Create the listener, listening at any ip address
		tcpListener = new TcpListener(IPAddress.Any, port_me);
	}

	public void Start(GameController game)
	{
		//------------------------------------------------ start server
		Debug.Log("============= Starting the " + Name + " server on port " + port_me + "."); 
			
		//------------------ Start server
		tcpListener.Start();
		Debug.Log("Waiting for incoming connections...");
		Running = true;
		
		Thread server_conn = new Thread(new ThreadStart(ServerConnectLoop));
		//make server act as daemon thread
		server_conn.IsBackground = true;
		server_conn.Start();

		//------------------- start client
		client = new Client();
		//connect game client...
		client.Connect();
	}
	void ServerConnectLoop()
	{
		while (!tcpListener.Pending()) {
			Thread.Sleep(100);
		}
		_handleNewConnection();
		
		
		//Start a game for the first new connection
		
		//add networked player to game
		GameController.AddTcpClient(tcpClient_other);
		Debug.Log("A networked player has been added to the game.");
		
		//------------------------------------------------- run server & client
		while (!client.tcpClient.Connected) {
			//wait for client to connect
			Thread.Sleep(100);
		}
		RunLoop();
	}

	void RunLoop()
	{
		while (Running) {
			//------------------------------------------------- client run cycle
			
			// Check for new packets
			client._handleIncomingPackets();
				
			//poll for local player input changes
			if (client.changed_local) {
				GameController.HandleInputAction(client.action_local);
				//reset flag
				client.changed_local = false;
			}
			//poll for remote player input changes
			if (client.changed_remote) {
				GameController.SyncGame_obey(client.action_remote);
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
			
			Thread.Sleep(10);
		}
		
		Shutdown();
	}
	public void Shutdown()
	{
		
		Running = false;
		Debug.Log("Shutting down server...");
		
		//-------------------------------------------------------- client STOP
		// gracefully disconnect client...
		if (client != null)
			client.Disconnect();
		// Cleanup
		client.tcpClient.Close();
		Debug.Log("Client has been shut down.");
		
		//-------------------------------------------------------- server STOP
		// Disconnect any clients remaining
		if (tcpClient_other != null && tcpClient_other.Connected)
			DisconnectClient(tcpClient_other, "Other server is shutting down.");

		// Cleanup our resources
		tcpListener.Stop();

		// Info
		Debug.Log("The server has been shut down.");
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
	private static void DisconnectClient(TcpClient client, string message)
	{
		Debug.Log("Disconnecting the client from " + client.Client.RemoteEndPoint);

		// If no message set, use the default "Goodbye."
		if (message == "")
			message = "Goodbye.";

		// Send the "bye" message
		Packet.SendPacket(client.GetStream(), new Packet("bye", message));

		// Give the client some time to send and process the graceful disconnect
		Thread.Sleep(500);

		client.Close();
	}
		
	// Will get a single packet from a TcpClient
	// Returns null if no data available or issue
	private Packet ReceivePacket(TcpClient client)
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
