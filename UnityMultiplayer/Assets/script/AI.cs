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


/*
 * used by LanguageManager.cs
 */


public class AI : MonoBehaviour {

	public static int selectPiece(){

		for (int i=0; i<GameInfo.NUM_BLOCKS; ++i) {
			if(!GameInfo.blockSucceed[GameInfo.RandomList[i]]){
				return GameInfo.RandomList[i];
			}
		}

		return -1;
	}

}
