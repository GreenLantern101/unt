using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Threading;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;


//connects agent AI to speech connection
public class NetworkConnection: MonoBehaviour
{
	//data
	private int sPort = 10011;

	//clients
	private TcpClient speechClient = new TcpClient();


	//location of google server for AI
	//private string Address = "129.59.79.186";
	private string Address = "129.59.79.171";


	public Hashtable speechHT;
	NetworkStream netStream;

	void Start()
	{
		//start speech connection
		
		Debug.Log("Connecting to speech started.");
		StartSpeechConnection();
		
		MainController.FSM.Fire(Trigger.startIntro);
	}

	void Update()
	{
		//ensure speechClient not null
		if (speechClient == null)
			return;
		if (speechClient.Connected && MainController.FSM.IsInState(PuzzleState.GAME)) {            

			if (speechClient.Available > 0) {
				Debug.Log("AAAAAAAAAAAAAAAHHHHHHHHHHHHHHHHHH");
				string speechDataString = receiveSpeechData();
				decodeSpeech(speechDataString);
				LanguageManager.takeAction(speechHT);
                

				//temporary shim
				//todo change based on parsing...
				//only use agent input if it is agent's turn
				if (MainController.isAgentActive)
					MainController._agentPlayer.startMoveBlock(6);

			}
		}
	}

	void ClientConnectLoop(TcpClient tcpClient, string ipAddress, int port)
	{
		//keep trying to connect to server, once per second
		while (!tcpClient.Connected) {
			// Connect to the server
			try {
				tcpClient.Connect(ipAddress, port);
			} catch (SocketException se) {
				Debug.Log("[ERROR] " + se.Message);
				Debug.Log("Failed to connect. Trying again.");
				Thread.Sleep(1000);
			}
		}

		// check that we've connected
		if (tcpClient.Connected) {
			Debug.Log("Connected to speech server at " + tcpClient.Client.RemoteEndPoint);

			// Get the message stream
			netStream = tcpClient.GetStream();
		}
	}

	// Use this to start the server for speech recognition
	void StartSpeechConnection()
	{            
		Thread client_conn = new Thread(new ThreadStart(() => ClientConnectLoop(speechClient, Address, sPort)));
		client_conn.Start();
	}

	string receiveSpeechData()
	{
		byte[] bytes = new byte[speechClient.ReceiveBufferSize];
		netStream.Read(bytes, 0, (int)speechClient.ReceiveBufferSize);
		string returndata = Encoding.UTF8.GetString(bytes);
		return returndata;
	}

	void decodeSpeech(string _str)
	{
		speechHT = new Hashtable();
		Debug.Log("Received data: " + _str);
		//each element is ended with ','
		string[] elements = _str.Split(',');
		for (int i = 0; i < elements.Length; ++i) {
			if (!elements[i].Contains(':'))
				continue;

			string[] Elem = elements[i].Split(':');

			if (Elem.Length != 2)
				continue;
			if (speechHT.ContainsKey(Elem[0])) {
				speechHT[Elem[0]] = Elem[1];
			} else {
				speechHT.Add(Elem[0], Elem[1]);
			}
		}
	}
}
