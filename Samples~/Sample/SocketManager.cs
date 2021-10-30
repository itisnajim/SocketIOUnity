using System;
using System.Collections.Generic;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

using Debug = System.Diagnostics.Debug;


public class SocketManager : MonoBehaviour
{
    public SocketIOUnity socket;

    public InputField EventNameTxt;
    public InputField DataTxt;
    public Text ReceivedText;  

    public GameObject objectToSpin;

    // Start is called before the first frame update
    void Start()
    {
        //TODO: check the Uri if Valid.
        var uri = new Uri("http://192.168.1.107:11100");
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
                {
                    {"token", "UNITY" }
                }
            ,
            EIO = 4
            ,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        socket.JsonSerializer = new NewtonsoftJsonSerializer();

        ///// reserved socketio events
        socket.OnConnected += (sender, e) =>
        {
            Debug.Print("socket.OnConnected");
        };
        socket.OnPing += (sender, e) =>
        {
            Debug.Print("Ping");
        };
        socket.OnPong += (sender, e) =>
        {
            Debug.Print("Pong: " + e.TotalMilliseconds);
        };
        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Print("disconnect: " + e);
        };
        socket.OnReconnectAttempt += (sender, e) =>
        {
            Debug.Print($"{DateTime.Now} Reconnecting: attempt = {e}");
        };
        ////

        Debug.Print("Connecting...");
        socket.Connect();

        socket.OnUnityThread("spin", (data) =>
        {
            rotateAngle = 0;
        });

        ReceivedText.text = "";
        socket.OnAnyInUnityThread((name, response) =>
        {
            ReceivedText.text += "Received On " + name + " : " + response.GetValue().GetRawText() + "\n";
        });
    }

    public void EmitTest()
    {
        string eventName = EventNameTxt.text.Trim().Length < 1 ? "hello" : EventNameTxt.text;
        string txt = DataTxt.text;
        if (!IsJSON(txt))
        {
            socket.Emit(eventName, txt);
        }
        else
        {
            socket.EmitStringAsJSON(eventName, txt);
        }
    }

    public static bool IsJSON(string str)
    {
        if (string.IsNullOrWhiteSpace(str)) { return false; }
        str = str.Trim();
        if ((str.StartsWith("{") && str.EndsWith("}")) || //For object
            (str.StartsWith("[") && str.EndsWith("]"))) //For array
        {
            try
            {
                var obj = JToken.Parse(str);
                return true;
            }catch (Exception ex) //some other exception
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public void EmitSpin()
    {
        socket.Emit("spin");
    }

    public void EmitClass()
    {
        TestClass testClass = new TestClass(new string[] { "foo", "bar", "baz", "qux" });
        TestClass2 testClass2 = new TestClass2("lorem ipsum");
        socket.Emit("class", testClass2);
    }

    // our test class
    [System.Serializable]
    class TestClass
    {
        public string[] arr;

        public TestClass(string[] arr)
        {
            this.arr = arr;
        }
    }

    [System.Serializable]
    class TestClass2
    {
        public string text;

        public TestClass2(string text)
        {
            this.text = text;
        }
    }
    //


    float rotateAngle = 45;
    readonly float MaxRotateAngle = 45;
    void Update()
    {
        if(rotateAngle < MaxRotateAngle)
        {
            rotateAngle++;
            objectToSpin.transform.Rotate(0, 1, 0);
        }
    }
}