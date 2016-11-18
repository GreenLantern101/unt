
using System;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

//used by MultiplayerManager
public class Message
{
	public Message()
	{

	}
}


/*
 * Types of data needed to be sent:
 * 
 * bool readyFlag; //sent on beginning of each game only
 * 
 * int activePiece;
 * Vector3 curOrientation;
 * Vector3 curPosition;
 * float activeTimer;
 * 
 * 
 */ 
