using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Collections; 
using System.Threading;


/*
 * Saves audio clip
 */ 


public static class SavWav {
	
	const int HEADER_SIZE = 44;

	public static Thread fileThread;
	public static bool newFile = true;

	public static string fileName;
	public static AudioClip[] clip = new AudioClip[2];
	private static int clipBufferSelector = 0;
	public static int lengthInSample;
	public static string filePath;
	public static float[] trimedSamples;
	public static List<float> samplesList;
	public static float lengthOfBlank;

	private static int hz;
	private static int channels;
	private static int currClipSamples;
	private static int newSamples;



	


	public static void save(){


		if(newFile){

			if(!Directory.Exists(filePath))
				Directory.CreateDirectory(Path.GetDirectoryName(filePath));

			CreateEmptyFile(filePath);
			hz = clip[0].frequency;
			channels = clip[0].channels;

			//newFile = false;
		}




		currClipSamples = lengthInSample;


		trimedSamples = TrimSilence (clip[clipBufferSelector], lengthInSample);
		clipBufferSelector = ++clipBufferSelector % 2;


		fileThread = new Thread(new ThreadStart(locSave));

		fileThread.Start ();
	
	}


	
	public static void locSave() {


		try{
			using (FileStream currFile = new FileStream(filePath, FileMode.Open)) {

				float[] locTrimedSamples = trimedSamples;



				currFile.Seek(0, SeekOrigin.End);

				WriteBlank(currFile);
			


				ConvertAndWrite(currFile, locTrimedSamples);
				WriteHeader(currFile);

		}
		}catch(Exception e){
			Debug.Log("Exception!: " + e.ToString());
		}

	}




	static void CreateEmptyFile(string filepath) {
		using (var fileStream = new FileStream(filepath, FileMode.Create)) {
			byte emptyByte = new byte ();
		
			for (int i = 0; i < HEADER_SIZE; i++) { //preparing the header
				fileStream.WriteByte (emptyByte);
			}
		}
	}



	public static float[] TrimSilence(AudioClip locClip, int locLength){

		float[] samples = new float[locClip.samples];
		
		locClip.GetData(samples, 0);

		if (locLength >= locClip.samples) {
			return samples;
		}


		samplesList = new List<float>(samples);
		samplesList.RemoveRange(locLength, samplesList.Count - locLength);		// Remove this line to test



		return samplesList.ToArray ();

	}



	static void WriteBlank(FileStream fileStream){


		Byte[] blankData = new Byte[(int)(lengthOfBlank * hz) * 2];
		fileStream.Write(blankData, 0, blankData.Length);


	}


	static void ConvertAndWrite(FileStream fileStream, float[] samples) {

		
		Int16[] intData = new Int16[samples.Length];

		
		Byte[] bytesData = new Byte[samples.Length * 2];

		
		int rescaleFactor = 32767; //to convert float to Int16
		
		for (int i = 0; i<samples.Length; i++) {


			intData[i] = (short) (samples[i] * rescaleFactor);


			Byte[] byteArr = new Byte[2];
			byteArr = BitConverter.GetBytes(intData[i]);
			byteArr.CopyTo(bytesData, i * 2);
		}
		
		fileStream.Write(bytesData, 0, bytesData.Length);

	}





	

	static void WriteHeader(FileStream fileStream) {

		//newSamples += (currClipSamples + (int)(lengthOfBlank * hz));
		newSamples = currClipSamples;
		
		fileStream.Seek(0, SeekOrigin.Begin);
		
		Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
		fileStream.Write(riff, 0, 4);
		
		Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
		fileStream.Write(chunkSize, 0, 4);
		
		Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
		fileStream.Write(wave, 0, 4);
		
		Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
		fileStream.Write(fmt, 0, 4);
		
		Byte[] subChunk1 = BitConverter.GetBytes(16);
		fileStream.Write(subChunk1, 0, 4);
		

		UInt16 one = 1;
		
		Byte[] audioFormat = BitConverter.GetBytes(one);
		fileStream.Write(audioFormat, 0, 2);
		
		Byte[] numChannels = BitConverter.GetBytes(channels);
		fileStream.Write(numChannels, 0, 2);
		
		Byte[] sampleRate = BitConverter.GetBytes(hz);
		fileStream.Write(sampleRate, 0, 4);
		
		Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2);
		fileStream.Write(byteRate, 0, 4);
		
		UInt16 blockAlign = (ushort) (channels * 2);
		fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);
		
		UInt16 bps = 16;
		Byte[] bitsPerSample = BitConverter.GetBytes(bps);
		fileStream.Write(bitsPerSample, 0, 2);
		
		Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
		fileStream.Write(datastring, 0, 4);
		
		Byte[] subChunk2 = BitConverter.GetBytes(newSamples * channels * 2);
		fileStream.Write(subChunk2, 0, 4);

	}
}