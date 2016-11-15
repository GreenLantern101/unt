using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

public class NetworkConnection: MonoBehaviour
{
	//data
	private int sPort;
	private int pPort;
	private TcpClient speechClient;
	private string Adress = "129.59.79.186";
	//private string Adress = "10.110.10.208";
	public Hashtable speechHT;
	NetworkStream netStream;

	void Start() {
		//start speech connection
	    StartSpeechConnection();
		print ("Connected to speech finished");
		netStream = speechClient.GetStream ();
		MainController.FSM.Fire (Trigger.startIntro);
		//start player connection
//		pPort = 3000;
//		pData = new PlayerData();
//		StartPlayerServer ();
//		print ("Connection to player finished");
	}

	void Update(){
		if (speechClient.Connected && MainController.FSM.IsInState(PuzzleState.GAME)) {            

				if(speechClient.Available>0){
					string speechDataString = receiveSpeechData();
					print ("the received data is: " + speechDataString);
					decodeSpeech(speechDataString);
					LanguageManager.takeAction(speechHT);
				}
			}
		}

	// Use this to start the server for speech recognition
	void StartSpeechConnection()
	{            
		sPort = 10010;
		speechClient = new TcpClient ();
		speechClient.Connect (Adress, sPort);
	}

	string receiveSpeechData(){
		byte[] bytes = new byte[speechClient.ReceiveBufferSize];
		netStream.Read (bytes, 0, (int)speechClient.ReceiveBufferSize);
		string returndata = Encoding.UTF8.GetString (bytes);
		return returndata;
	}

	void decodeSpeech(string _str){
		speechHT = new Hashtable ();
		print ("received data: " + _str);
		//each element is ended with ','
		string[] elements = _str.Split(',');
		for (int i=0; i<elements.Length; ++i) {
			if(!elements[i].Contains(':'))
				continue;

			string[] Elem = elements[i].Split(':');

			if(Elem.Length != 2)
				continue;
			if(speechHT.ContainsKey(Elem[0])){
				speechHT[Elem[0]] = Elem[1];
			}else{
				speechHT.Add (Elem [0], Elem [1]);
			}
		}
	}
}
