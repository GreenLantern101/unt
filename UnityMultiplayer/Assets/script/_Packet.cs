
using System;
using System.Text;
using System.Net.Sockets;
using Newtonsoft.Json;

public class Packet
{
	[JsonProperty("command")]
	public string Command { get; set; }

	[JsonProperty("message")]
	public string Message { get; set; }

	// Makes a packet
	public Packet(string command, string message)
	{
		Command = command;
		Message = message;
	}

	public override string ToString()
	{
		return string.Format(
			"[Packet:\n" +
			"  Command=`{0}`\n" +
			"  Message=`{1}`]",
			Command, Message);
	}
	string ToJson()
	{
		return JsonConvert.SerializeObject(this);
	}

	// Deserialize
	static Packet FromJson(string jsonData)
	{
		return JsonConvert.DeserializeObject<Packet>(jsonData);
	}
		
	#region SENDING
	public static void SendPacket(NetworkStream stream, Packet packet)
	{
		// encode content
		byte[] jsonbytes = Encoding.UTF8.GetBytes(packet.ToJson());
		// encode length of content (16 bit unsigned short)
		byte[] lengthbytes = BitConverter.GetBytes(Convert.ToUInt16(jsonbytes.Length));

		// Join the byte arrays
		byte[] packetbytes = new byte[lengthbytes.Length + jsonbytes.Length];
		lengthbytes.CopyTo(packetbytes, 0);
		jsonbytes.CopyTo(packetbytes, lengthbytes.Length);
		
		//write bytes to stream
		stream.Write(packetbytes, 0, packetbytes.Length);
	}
	#endregion
		
	#region RECEIVING
	public static Packet getPacketFromStream(NetworkStream _msgStream)
	{
		// First two bytes(16 bits) indicate size of the Packet
		byte[] lengthBuffer = new byte[2];
		_msgStream.Read(lengthBuffer, 0, 2);
		ushort packetByteSize = BitConverter.ToUInt16(lengthBuffer, 0);

		// Remaining bytes in the stream is packet content
		byte[] jsonBuffer = new byte[packetByteSize];
		_msgStream.Read(jsonBuffer, 0, jsonBuffer.Length);
			
		// Convert Json to packet
		string jsonString = Encoding.UTF8.GetString(jsonBuffer);
		return Packet.FromJson(jsonString);
	}
	#endregion
}
