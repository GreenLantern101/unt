using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;


//connects agent AI to speech connection
public class NetworkConnection: MonoBehaviour
{
	//data
	private int sPort;
	private int pPort;
	private TcpClient speechClient;

	private TcpClient serverClient;

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
		Debug.Log("Connected to speech finished.");
		netStream = speechClient.GetStream();
		
		MainController.FSM.Fire(Trigger.startIntro);
		//start player connection
		pPort = 10011;
//		pData = new PlayerData();
		StartPlayerServer();
		Debug.Log("Connection to player finished");
	}

	void Update()
	{
		//ensure speechClient not null
		if (speechClient == null)
			return;
		if (speechClient.Connected && MainController.FSM.IsInState(PuzzleState.GAME)) {            

			if (speechClient.Available > 0) {
				string speechDataString = receiveSpeechData();
				print("the received data is: " + speechDataString);
				decodeSpeech(speechDataString);
				LanguageManager.takeAction(speechHT);
			}
		}
	}

	void StartPlayerServer()
	{
		pPort = 10011;
		serverClient = new TcpClient();
		serverClient.Connect(Address, pPort);

	}

	// Use this to start the server for speech recognition
	void StartSpeechConnection()
	{            
		sPort = 10010;
		speechClient = new TcpClient();
		speechClient.Connect(Address, sPort);
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
		print("received data: " + _str);
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
