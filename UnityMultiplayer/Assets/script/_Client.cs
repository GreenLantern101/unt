﻿
using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using UnityEngine;

//essentially a wrapper over TcpClient
public class Client
{
	// Connection objects
	public TcpClient tcpClient;
	private IPAddress ipAddress_other;
	private int Port;
	public bool _clientRequestedDisconnect = false;

	// Messaging
	private NetworkStream _msgStream = null;
		
	public Client()
	{
		tcpClient = new TcpClient(AddressFamily.InterNetwork);
			
		//ip address and port of opposing server should be put in "config.txt"
		string[] lines = File.ReadAllLines(Directory.GetCurrentDirectory() + "/Assets/script/config.txt");	
		ipAddress_other = IPAddress.Parse(lines[0]);
		Port = int.Parse(lines[1]);
		
		Debug.Log("============= Client made, will connect to " + ipAddress_other + " at port " + Port);
	}
	// Connects to the games server
	public void Connect()
	{
		Thread client_conn = new Thread(new ThreadStart(ClientConnectLoop));
        client_conn.Start();
        //Thread.Sleep(1);
        //client_conn.Join();
        
	}
	void ClientConnectLoop(){
		//keep trying to connect to server, once per second
		while (!tcpClient.Connected) {
			// Connect to the server
			try {
				tcpClient.Connect(ipAddress_other, Port);
			} catch (SocketException se) {
				Debug.Log("[ERROR]" + se.Message);
				Debug.Log("Failed to connect. Trying again.");
				Thread.Sleep(3000);
			}
		}
		
		// check that we've connected
		if (tcpClient.Connected) {
			Debug.Log("Connected to server at " + tcpClient.Client.RemoteEndPoint);

			// Get the message stream
			_msgStream = tcpClient.GetStream();
		}
	}

	// Requests a disconnect, will send "bye" message
	// This should only be called by the user
	public void Disconnect()
	{
		Debug.Log("Disconnecting...");
		_clientRequestedDisconnect = true;
		Packet.SendPacket(this._msgStream, new Packet("bye", ""));
	}
	public void _cleanupNetworkResources()
	{
		//Debug.Log("Cleaning up network resources...");
		if (_msgStream != null)
			_msgStream.Close();
		_msgStream = null;
		tcpClient.Close();
	}

	// Checks for new incoming messages and handles them
	// Handles one Packet at a time, even if more than one is in the memory stream
	public void _handleIncomingPackets()
	{
		Packet packet = new Packet("", "");
		// Handle incoming messages
		if (tcpClient.Available > 0) {
					
			packet = Packet.getPacketFromStream(_msgStream);

			switch (packet.Command) {
				case "bye":
					_handleBye(packet.Message);
					break;
				case "input":
					_handleInput(packet.Message);
					break;
				case "message":
					_handleMessage(packet.Message);
					break;
				case "sync":
					_handleSync(packet.Message);
					break;
				default:
					Debug.Log("Invalid packet command received.");
					break;
						
			}
		}
	}

	#region Command Handlers
	private void _handleBye(string message)
	{
		Debug.Log(message);
	}

	// Just Debug.Logs out a message sent from the server
	private void _handleMessage(string message)
	{
		Debug.Log(message);
	}
	private void _handleSync(string message)
	{
		//handle sync
		
		this.changed_remote = true;
		this.action_remote = message;
	}

	// Gets input from the user and sends it to the server
	private bool _handleInput(string message)
	{
		// Debug.Log the prompt and get a response to send
		Debug.Log(message);
			
		string responseMsg = Console.ReadLine();
			
		//NOTE: for some reason, calling synchronous game method here doesn't work...
		this.changed_local = true;
		this.action_local = responseMsg;
			
		// Send the response
		Packet resp = new Packet("input", responseMsg);
		Packet.SendPacket(this._msgStream, resp);
			
		return true;
	}
	public bool changed_local = false;
	public string action_local = "";
	public bool changed_remote = false;
	public string action_remote = "";
	#endregion
}
