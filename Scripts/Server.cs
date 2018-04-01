using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class ServerClient
{
    public int connectionId;
    public string playerName;
}

public class Server : MonoBehaviour
{

    private const int MAX_CONNECTION = 100;

    private int port = 5701;

    private int hostId;

    private int reliableChannel;
    private int unreliableChannel;

    private bool isStarted = false;
    private byte error;

    private List<ServerClient> clients = new List<ServerClient>();

    private void Start()
    {
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, port, null);

        isStarted = true;
    }

    private void Update()
    {
        if (!isStarted)
        {
            return;
        }
        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.ConnectEvent:    
                Debug.Log("Player " + connectionId + " has connected");
                OnConnection(connectionId);
                break;
            case NetworkEventType.DataEvent:       
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Receving from : " + connectionId + " : " +  msg);

                string[] splitData = msg.Split('|');

                switch (splitData[0])
                {
                    case "NAMEIS":
                        OnNameIs(connectionId, splitData[1]);

                        break;

                    case "CNN":
                        break;

                    case "DC":
                        break;

                    default:
                        break;

                }
                break;
            case NetworkEventType.DisconnectEvent: 
                Debug.Log("Player " + connectionId + " has disconnected");
                break;
        }
    }

    private void OnConnection(int cnnId)
    {
        //Add him to a list
        ServerClient c = new ServerClient();
        c.connectionId = cnnId;
        c.playerName = "TEMP";
        clients.Add(c);

        //When the player joins the server, tell him his ID


        //Request his name and send the name of all the other players 
        string msg = "ASKNAME|" + cnnId + "|";
        foreach(ServerClient sc in clients)
        {
            msg += sc.playerName + "%" + sc.connectionId + "|";
        }
   
        msg = msg.Trim('|');
        Send(msg, reliableChannel, cnnId);

    }
    private void Send(string message, int channelId, int cnnId)
    {
        List<ServerClient> c = new List<ServerClient>();
        c.Add(clients.Find(x => x.connectionId == cnnId));
        Send(message, channelId, c);
    }
    private void Send(string message, int channelId, List<ServerClient> c)
    {
        Debug.Log("Sending : " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        foreach(ServerClient sc in c)
        {
            NetworkTransport.Send(hostId, sc.connectionId, channelId, msg, sizeof(char) * message.Length, out error);
        }
    }
    private void OnNameIs(int cnnId, string playerName)
    {
        // Link the name to the connections Id
        clients.Find(x => x.connectionId == cnnId).playerName = playerName;

        // Tell everbody that a new player has connected 
        Send("CNN|" + playerName + "|" + cnnId, reliableChannel, clients);
    }

}
