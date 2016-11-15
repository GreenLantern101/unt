using UnityEngine;  
using System.Collections;  
using System;
using System.Threading;

[RequireComponent (typeof (AudioSource))]  

/*
 * Captures microphone input, saves recordings using SavWav.cs
 */ 


public class SingleMicrophoneCapture : MonoBehaviour   
{  
	//A boolean that flags whether there's a connected microphone  
	private bool micConnected = false;  
	
	//The maximum and minimum available recording frequencies  
	private static int minFreq;  
	private static int maxFreq;  
	private static int length;
	private static string fileName = null;

	private static float blankTimmer;
	private static bool firstTimeRecording = true;
	
	//A handle to the attached AudioSource  
	//public static AudioSource goAudioSource;

	public static AudioClip[] clipBuffer = new AudioClip[2];
	public static int bufferSelector = 0;
	//public static int currBuffer = 0;

	
	//Use this for initialization  
	void Start()   
	{  
		print ("Microphone.devices.Length is " + Microphone.devices.Length);
		//Check if there is at least one microphone connected  
		if(Microphone.devices.Length <= 0)  
		{  
			//Throw a warning message at the console if there isn't  
			Debug.LogWarning("Microphone not connected!");  
		}  
		else //At least one microphone is present  
		{  
			//Set 'micConnected' to true  
			micConnected = true;  
			
			//Get the default microphone recording capabilities  
			Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);  
			
			//According to the documentation, if minFreq and maxFreq are zero, the microphone supports any frequency...  

			if(minFreq == 0 && maxFreq == 0)  
			{   
				maxFreq = 44100;  
			}  
			
			//Get the attached AudioSource component  
			//goAudioSource = this.GetComponent<AudioSource>();  
		}  
	}  
	
	void OnGUI()   
	{  
		//If there is a microphone  
		if(micConnected)  
		{  
			//If the audio from any microphone isn't being captured  
//			if(!Microphone.IsRecording(null))  
//			{  
//				//Case the 'Record' button gets pressed  
//				if(GUI.Button(new Rect(Screen.width/2-100, Screen.height/2-25, 200, 50), "Record"))  
//				{  
//
//					startRecording();
//				}  
//			}  
//			else //Recording is in progress  
//			{  
//				//Case the 'Stop and Play' button gets pressed  
//				if(GUI.Button(new Rect(Screen.width/2-100, Screen.height/2-25, 200, 50), "Stop"))  
//				{  
//					//length = Microphone.GetPosition(null);
//					stopRecording();
//				}  
//
//				//timmer += Time.deltaTime;
//				GUI.Label(new Rect(Screen.width/2-100, Screen.height/2+25, 200, 50), "Recording in progress...");  
//			}
		}  
		else // No microphone  
		{  
			//Print a red "Microphone not connected!" message at the center of the screen  
			GUI.contentColor = Color.red;  
			GUI.Label(new Rect(Screen.width/2-100, Screen.height/2-25, 200, 50), "Microphone not connected!");  
		}  
		
	} 



	public static void startRecording(){
		startRecording(string.Empty);

	}


	public static void startRecording(string newName){

		if(firstTimeRecording){
			if (newName != null)
				fileName = newName;

			string tempStr = fileName == null ? "DefaultName" : fileName;
			tempStr = "Audio_" + tempStr + "-" + DateTime.Now.ToString(@"M-d-hh-mm-ss");
			if (!tempStr.ToLower().EndsWith(".wav")) {
				tempStr += ".wav";
			}
			
			string appPath = Application.dataPath;
			appPath = appPath.LastIndexOf ('/') == -1 ? appPath : appPath.Substring (0, appPath.LastIndexOf ('/'));
			
			SavWav.filePath = appPath + "/RecordedAudio/" + tempStr;



			//firstTimeRecording = false;
			
			if(minFreq == 0 && maxFreq == 0)  
			{   
				maxFreq = 44100;  
			}  
		}
		clipBuffer[bufferSelector] = Microphone.Start(null, true, 600, maxFreq);  
		bufferSelector = (bufferSelector + 1) % 2;
		//Debug.Log ("Start: bufferSelecter: " + bufferSelecter);

		//SavWav.lengthOfBlank = (blankTimmer != 0) ? (Time.realtimeSinceStartup - blankTimmer) : 0;
		SavWav.lengthOfBlank = 0;

		//print ("Is Recording: " + Microphone.IsRecording(null));
	}

	public static void stopRecording(){
		if(SavWav.fileThread != null){
			//print ("Thread Stat 1: " + SavWav.fileThread.ThreadState.ToString());
			SavWav.fileThread.Join ();
			//print ("Thread Stat 2: " + SavWav.fileThread.ThreadState.ToString());
		}

		length = Microphone.GetPosition(null);

		Microphone.End(null); //Stop the audio recording  
		
		//SavWav.clip = (AudioClip)Instantiate(goAudioSource.clip);
		SavWav.clip[Mathf.Abs(bufferSelector - 1)] = clipBuffer[Mathf.Abs(bufferSelector - 1)];





		//goAudioSource.clip = SavWav.clip;
		//Debug.Log ("Stop: bufferSelecter: " + bufferSelecter);


		//string tempStr = fileName == null ? "DefaultName" : fileName;
		//print ("tmpStr: " + tempStr);
		//SavWav.fileName = "Audio_" + tempStr + DateTime.Now.ToString(@"M-d-hh-mm-ss");
		SavWav.lengthInSample = length;
		//SavWav.minute = timmer / 60;
		
		//Thread fileOpThread = new Thread (new ThreadStart (SavWav.newSave));
		//fileOpThread.Start ();
		SavWav.save();
		//SavWav.Save("audio", goAudioSource.clip);		//record 19s data
		//goAudioSource.Play();
		
		blankTimmer = Time.realtimeSinceStartup;
	}

	void OnApplicationQuit() {
		if(SavWav.fileThread != null)
			SavWav.fileThread.Join ();
	}







}  