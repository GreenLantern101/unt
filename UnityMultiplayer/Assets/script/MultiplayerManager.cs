using System;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.IO;

public class MultiplayerManager
{
	string hostname = Dns.GetHostName();
	int port = 3000;
	TcpClient tcpclient = new TcpClient(hostname, port);

	//NOTE: ip address may need to be changed for each computer
	IPAddress ipAddress = new IPAddress.Parse("129.59.122.21");

	/*
	IPHostEntry ipHost = Dns.GetHostEntry ();
	IPAddress ipAddress = ipHost.AddressList[0];
	*/

	TcpListener tcplistener = new TcpListener(IPAddress, port);
	//initialize this in MainController?
	public MultiplayerManager ()
	{
		//connection network code
	}
	public void handleUserAdded(LocalPlayer player)
	{
		if (MainController._networkedPlayer == null) {
			MainController._networkedPlayer = new NetworkedPlayer();
			//initialize data for networked player from local player
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
}







