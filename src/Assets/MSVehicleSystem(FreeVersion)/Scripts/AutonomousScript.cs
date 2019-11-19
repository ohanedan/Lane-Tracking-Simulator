using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net; 
using System.Net.Sockets; 
using System.Text; 
using System.Threading;
using System.IO;

[Serializable]
public class ControlPackage
{
    public int vertical = -1;
    public int horizontal = -1;
    public bool pause;
    public bool handbrake;
    public bool reset;
}

public class AutonomousScript : MonoBehaviour
{
    private static AutonomousScript instance = null;
    public float _verticalVal = 0;
    public float verticalVal
    {
        get
        {
            return _verticalVal;
        }
        set
        {
            if(value > 255) value = 255.0f;
            if(value < 0) value = 0.0f;
            _verticalVal = value;
        }
    }
    public float _horizontalVal = 0;
    public float horizontalVal
    {
        get
        {
            return _horizontalVal;
        }
        set
        {
            if(value > 254) value = 254.0f;
            _horizontalVal = value - 127;
        }
    }
    public bool pause = false;
    public bool handbrake = false;
    public bool reset = false;

    private TcpListener tcpListener; 
	private Thread tcpListenerThread;  	
	private TcpClient connectedTcpClient;

    void Start()
    {
        if( instance == null )
        {
            instance = this;
        }
        else if( this != instance )
        {
            Destroy( gameObject );
            return;
        }
        DontDestroyOnLoad(this);
        tcpListenerThread = new Thread (new ThreadStart(ListenForIncommingRequests)); 		
		tcpListenerThread.IsBackground = true; 		
		tcpListenerThread.Start(); 
    }
    
    private void ListenForIncommingRequests () { 		
		try { 						
			tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 6161); 			
			tcpListener.Start();          			
			while (true) { 				
				using (connectedTcpClient = tcpListener.AcceptTcpClient()) {			
					using (StreamReader reader = new StreamReader(connectedTcpClient.GetStream(), Encoding.UTF8)) { 						
						string line;
                        while((line = reader.ReadLine()) != null) {
                            try
                            {
                                ControlPackage package = JsonUtility.FromJson<ControlPackage>(line);
                                if(package.vertical != -1)
                                {
                                    verticalVal = package.vertical;
                                }
                                if(package.horizontal != -1)
                                {
                                    horizontalVal = package.horizontal;
                                }
                                pause = package.pause;
                                handbrake = package.handbrake;
                                reset = package.reset;
                                Debug.Log("Socket: " + line);
                            }
                            catch
                            {
                                Debug.LogWarning("SocketWarning: JSON Parse error.");
                            }
                        }		
					} 				
				} 			
			} 		
		} 		
		catch (SocketException socketException) { 			
			Debug.LogError("SocketException: " + socketException.ToString()); 		
		}     
	}  	

	private void SendMessage(string serverMessage) { 		
		if (connectedTcpClient == null) {             
			return;         
		}  		
		
		try { 					
			NetworkStream stream = connectedTcpClient.GetStream(); 			
			if (stream.CanWrite) {		            
				byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(serverMessage);        
				stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);       
			}       
		} 		
		catch (SocketException socketException) {             
			Debug.LogError("SocketException: " + socketException);         
		} 	
	} 
}
