using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


public class Face_Script : MonoBehaviour
{
    // Start is called before the first frame updat
    Thread receiveThread;
    UdpClient client;
    Transform cheeckL;
    Transform cheeckR;
    Transform sideLipL,sideLipR,lipUpL,lipUpR,lipDownL,lipDownR,lipDown,lipUp,browHL,browML,browLL,browHR,browMR,browLR;
    Transform jaw;
    Transform head;
    [SerializeField]
    float eulerAngX;
    [SerializeField]
    float eulerAngY;
    [SerializeField]
    float eulerAngZ;
 
    
    float[][] cur_pos_lip;
    float[][] cur_pos_rr;
    float[][] prev_pos_rr;
    float[][] cur_pos_ll;
    float[][] prev_pos_ll;
    float[][] prev_pos_lip;
    float[] cur_rotation;
    float[] prev_rotation;

    bool move;
    // float lerpDuration  = 0.5f;
    bool rotate;
    bool ready;
    bool cur_ready;
    int port;
    int scalefactor;
    
    private void normalize(float[][] arr)
        {
            float minmx = 0,minmy = 0;
            for(int i=0;i<arr.Length;i++)
                {
                    minmx += arr[i][0];
                    minmy += arr[i][1];
                }
            minmx = minmx / arr.Length;
            minmy = minmy / arr.Length;
            for(int i=0;i<arr.Length;i++)   
            {
                arr[i][0] -= minmx;
                arr[i][1] -= minmy;
            }
        }
    void Start()
    {
        port = 9990;
        cur_ready = false;
        ready = false;
        cur_pos_lip = new float[17][];
        prev_pos_lip = new float[17][];
        cur_pos_rr = new float[5][];
        prev_pos_rr = new float[5][];
        cur_pos_ll = new float[5][];
        prev_pos_ll = new float[5][];
        cur_rotation = new float[2];
        prev_rotation = new float[2];
        for(int i=0;i<17;i++)
            {
                cur_pos_lip[i] = new float[2];
                prev_pos_lip[i] = new float[2];
            }
        for(int i=0;i<5;i++)    
            {
                cur_pos_ll[i] = new float[2];
                cur_pos_rr[i] = new float[2];
                prev_pos_ll[i] = new float[2];
                prev_pos_rr[i] = new float[2];
            }
        
        head  = GameObject.Find("head").GetComponent<Transform>();
        cheeckL = GameObject.Find("Cheeck.L").GetComponent<Transform>();
        cheeckR = GameObject.Find("Cheeck.R").GetComponent<Transform>();
        jaw = GameObject.Find("jaw").GetComponent<Transform>();
        sideLipL = GameObject.Find("SideLip.L").GetComponent<Transform>();
        sideLipR  = GameObject.Find("SideLip.R").GetComponent<Transform>();
        lipUpL = GameObject.Find("LipUp.L").GetComponent<Transform>();
        lipUpR = GameObject.Find("LipUp.R").GetComponent<Transform>();
        lipDownL = GameObject.Find("LipDown.L").GetComponent<Transform>();
        lipDownR =  GameObject.Find("LipDown.R").GetComponent<Transform>();
        lipDownR =  GameObject.Find("LipDown.R").GetComponent<Transform>();
        lipDown  = GameObject.Find("LipDown").GetComponent<Transform>();
        lipUp =  GameObject.Find("LipUp").GetComponent<Transform>();
        browHL = GameObject.Find("BrowHigh.L").GetComponent<Transform>();
        browHR = GameObject.Find("BrowHigh.R").GetComponent<Transform>();
        browLL = GameObject.Find("BrowLow.L").GetComponent<Transform>();
        browLR = GameObject.Find("BrowLow.R").GetComponent<Transform>();
        browML = GameObject.Find("BrowMed.L").GetComponent<Transform>();
        browMR = GameObject.Find("BrowMed.R").GetComponent<Transform>();
        print(head.eulerAngles);
        // head.eulerAngles = new Vector3(304,175,0);
        // head.Rotate(new Vector3(0,-50,0),Space.World);
        // print(head.eulerAngles);
        InitUDP();
    }

    private void InitUDP()
	{
		print ("UDP Initialized");
		receiveThread = new Thread (new ThreadStart(ReceiveData)); //1 
		receiveThread.IsBackground = true; //2
		receiveThread.Start (); //3
	}


    private void ReceiveData()
	{
		client = new UdpClient (port); //1
		while (true) //2
		{
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port); //3
				byte[] data = client.Receive(ref anyIP); //4
				string text = Encoding.UTF8.GetString(data); //5
                float[][] data_recv;
                data_recv = JsonConvert.DeserializeObject<float[][]>(text);
                for(int i=0;i<17;i++)   
                {
                    prev_pos_lip[i][0] = cur_pos_lip[i][0];
                    prev_pos_lip[i][1] = cur_pos_lip[i][1];
                }
                for(int i =0;i<5;i++)
                    {
                        prev_pos_ll[i][0] = cur_pos_ll[i][0];
                        prev_pos_ll[i][1] = cur_pos_ll[i][1];
                        prev_pos_rr[i][0] = cur_pos_rr[i][0];
                        prev_pos_rr[i][1] = cur_pos_rr[i][1];
                    }
                for(int i=0;i<17;i++)   
                    {
                        cur_pos_lip[i][0] = data_recv[i][0];
                        cur_pos_lip[i][1] = data_recv[i][1];
                    }
                for(int i=0;i<5;i++)
                    {
                    
                        cur_pos_ll[i][0] = data_recv[i+17][0];
                        cur_pos_ll[i][1] = data_recv[i+17][1];         
                        cur_pos_rr[i][0] = data_recv[22+i][0];
                        cur_pos_rr[i][1] = data_recv[22+i][1];           
                    }
                prev_rotation[0] = cur_rotation[0];
                prev_rotation[1] = cur_rotation[1];
                cur_rotation[0] = data_recv[27][0];
                cur_rotation[0] = ((float)-9)*cur_rotation[0]+(float)191;
                cur_rotation[1] = data_recv[27][1];
                cur_rotation[1] = ((float)2.3)*cur_rotation[1]+(float)210;
                // cur_rotation[1] = 
                // print(cur_rotation[0]);
                normalize(cur_pos_lip);
                normalize(cur_pos_ll);
                normalize(cur_pos_rr);
                if(cur_ready ==  true)
                    ready = true;
                cur_ready = true;
		}
	}
    // Update is called once per frame
    // IEnumerator Rotate(Transform BONE,Quaternion angle,float lerpDuration){
    //     rotate = true;
    //     float timeElapsed = 0;
    //     Quaternion startRotation = BONE.rotation;
    //     Quaternion targetRotation = BONE.rotation * angle;
    //     while(timeElapsed < lerpDuration)
    //         {
    //              BONE.rotation = Quaternion.Slerp(startRotation, targetRotation, timeElapsed / lerpDuration);
    //              timeElapsed += Time.deltaTime;
    //              yield return null;
    //         }
    //     BONE.rotation = targetRotation;
    //     targetRotation = startRotation;
    //     startRotation =  BONE.rotation;
    //     timeElapsed = 0;
    //     while(timeElapsed < lerpDuration)
    //         {
    //              BONE.rotation = Quaternion.Slerp(startRotation, targetRotation, timeElapsed / lerpDuration);
    //              timeElapsed += Time.deltaTime;
    //              yield return null;
    //         }
    //     rotate = false;  
    // }
        private void Rotate(Transform BONE,Quaternion angle,float lerpDuration){
        rotate = true;
        Quaternion startRotation = BONE.rotation;
        Quaternion targetRotation = BONE.rotation * angle;
        BONE.rotation  = targetRotation;
    }


    // IEnumerator Move(Transform BONE,Vector3 position,float Duration){
    //     float time = 0;
    //     Vector3 startPosition = BONE.position;
    //     Vector3 targetPosition = startPosition + position;
    //     while( time  < Duration )
    //         {
    //             BONE.position = Vector3.Lerp(startPosition, targetPosition, time / Duration);
    //             time += Time.deltaTime;
    //             yield return null;
    //         }
    //     BONE.position = targetPosition;
    //     targetPosition = startPosition;
    //     startPosition = BONE.position;
    //     time = 0;
    //     while( time  <Duration)
    //         {
    //             BONE.position = Vector3.Lerp(startPosition, targetPosition, time / Duration);
    //             time += Time.deltaTime;
    //             yield return null;
    //         }
        
    // }
    private void Move(Transform BONE,Vector3 position,float Duration){
        Vector3 startPosition = BONE.position;
        Vector3 targetPosition = startPosition + position;
        BONE.position = targetPosition;
    }

    void Update()
    {
        

    }

   void FixedUpdate() {
        
        eulerAngX = transform.localEulerAngles.x;
        eulerAngY = transform.localEulerAngles.y;
        eulerAngZ = transform.localEulerAngles.z;

        bool browMovement = true;
        bool headMovement = true;
        bool lipMovement = true;
        if(ready&&lipMovement){

            if(cur_pos_lip.Length > 0 && prev_pos_lip.Length > 0)
                {
                    Vector3 sideLipLC = new Vector3((cur_pos_lip[7][0]-prev_pos_lip[7][0])*(-3),(cur_pos_lip[7][1]-prev_pos_lip[7][1])*-1,0),
                    sideLipRC = new Vector3((cur_pos_lip[0][0]-prev_pos_lip[0][0])*(-3),(cur_pos_lip[0][1]-prev_pos_lip[0][1])*-1,0),
                    lipUpLC = new Vector3((cur_pos_lip[5][0]-prev_pos_lip[5][0])*-1,(cur_pos_lip[5][1]-prev_pos_lip[5][1])*-1,0),
                    lipUpRC = new Vector3((cur_pos_lip[2][0]-prev_pos_lip[2][0])*-1,(cur_pos_lip[2][1]-prev_pos_lip[2][1])*-1,0),
                    lipUpC = new Vector3((cur_pos_lip[4][0]-prev_pos_lip[4][0])*-1,(cur_pos_lip[4][1]-prev_pos_lip[4][1])*-1,0),
                    lipDownLC = new Vector3((cur_pos_lip[9][0]-prev_pos_lip[9][0])*-1,(cur_pos_lip[9][1]-prev_pos_lip[9][1])*-1,0),
                    lipDownRC = new Vector3((cur_pos_lip[15][0]-prev_pos_lip[15][0])*-1,(cur_pos_lip[15][1]-prev_pos_lip[15][1])*-1,0),
                    lipDownC =new Vector3((cur_pos_lip[12][0]-prev_pos_lip[12][0])*-1,(cur_pos_lip[12][1]-prev_pos_lip[12][1])*-1,0),
                    // jawC = new Vector3((cur_pos_lip[13][0]-prev_pos_lip[13][0])*scalefactor,(cur_pos_lip[13][1]-prev_pos_lip[13][1])*scalefactor,0),
                    cheeckLC = new Vector3((cur_pos_lip[7][0]-prev_pos_lip[7][0])*-2,(cur_pos_lip[7][1]-prev_pos_lip[7][1])*-2,0),
                    cheeckRC = new Vector3((cur_pos_lip[0][0]-prev_pos_lip[0][0])*-2,(cur_pos_lip[0][1]-prev_pos_lip[0][1])*-2,0);
                    Rotate(jaw,Quaternion.Euler((cur_pos_lip[13][1]-prev_pos_lip[13][1])*100,0,0),Time.deltaTime);
                    Move(sideLipL,sideLipLC,Time.deltaTime);
                    // print(sideLipRC);
                    Move(sideLipR,sideLipRC,Time.deltaTime);
                    Move(cheeckL,cheeckLC,Time.deltaTime);
                    Move(cheeckR,cheeckRC,Time.deltaTime);
                    Move(lipUpL,lipUpLC,Time.deltaTime);
                    Move(lipUpR,lipUpRC,Time.deltaTime);
                    Move(lipUp,lipUpC,Time.deltaTime);
                    Move(lipDown,lipDownC,Time.deltaTime);
                    Move(lipDownR,lipDownRC,Time.deltaTime);
                    Move(lipDownL,lipDownLC,Time.deltaTime);                
                }
       
        }
        if(ready&&browMovement){
         if(cur_pos_ll.Length>0) 
                {
                    Vector3 browLLC = new Vector3(cur_pos_ll[4][0]-prev_pos_ll[4][0],(cur_pos_ll[4][1]-prev_pos_ll[4][1])*(15),0);
                    Vector3 browMLC = new Vector3(cur_pos_ll[2][0]-prev_pos_ll[2][0],(cur_pos_ll[2][1]-prev_pos_ll[2][1])*(-15),0);
                    Vector3 browHLC = new Vector3(cur_pos_ll[0][0]-prev_pos_ll[0][0],(cur_pos_ll[0][1]-prev_pos_ll[0][1])*(-15),0);
                    Move(browHL,browHLC,Time.deltaTime);
                    Move(browLL,browLLC,Time.deltaTime);
                    Move(browML,browMLC,Time.deltaTime); 
                }
            if(cur_pos_rr.Length>0&&browMovement) 
                {
                    Vector3 browLRC = new Vector3(cur_pos_rr[0][0]-prev_pos_rr[0][0],(cur_pos_rr[0][1]-prev_pos_rr[0][1])*(15),0);
                    Vector3 browMRC = new Vector3(cur_pos_rr[2][0]-prev_pos_rr[2][0],(cur_pos_rr[2][1]-prev_pos_rr[2][1])*(-15),0);
                    Vector3 browHRC = new Vector3(cur_pos_rr[4][0]-prev_pos_rr[4][0],(cur_pos_rr[4][1]-prev_pos_rr[4][1])*(-15),0);
                    Move(browHR,browHRC,Time.deltaTime);
                    Move(browLR,browLRC,Time.deltaTime);
                    Move(browMR,browMRC,Time.deltaTime); 
                }
        }
    
    // print(ready);
    if(ready&&headMovement)
        {
            // print(head.localEulerAngles);
            // print(prev_rotation[0]-cur_rotation[0]);
            
                // head.localEulerAngles  = new Vector3(head.localEulerAngles[1],head.localEulerAngles[2]);
            // if(Math.Abs(head.localEulerAngles[1]-cur_rotation[1]) > 2&&Math.Abs(head.localEulerAngles[0]-cur_rotation[0]) > 10)
            //     head.localEulerAngles  = new Vector3(-1*cur_rotation[0]/10,cur_rotation[1],head.localEulerAngles[2]);
            // else if(Math.Abs(head.localEulerAngles[1]-cur_rotation[1]) > 2)
            //     {
            //         head.localEulerAngles  = new Vector3(head.localEulerAngles[0],cur_rotation[1],head.localEulerAngles[2]);
            //     }
            // else if(Math.Abs(head.localEulerAngles[0]-cur_rotation[0]) > 10)
            //     {
            //         head.localEulerAngles  = new Vector3(-1*cur_rotation[0]/10,head.localEulerAngles[1],head.localEulerAngles[2]);
            //     }
            // print(head.rotation.m_LocalRotation);
            print(cur_rotation[1]);
            if(Math.Abs(prev_rotation[0]-cur_rotation[0]) > 5)
                {
                    head.Rotate((cur_rotation[0]-prev_rotation[0])/10,0,0,Space.World);

                }
            if(Math.Abs(cur_rotation[1]-prev_rotation[1]) > 3)  
                {
                        head.Rotate(-1*(cur_rotation[0]-prev_rotation[0])/20,(cur_rotation[1]-prev_rotation[1])/2,0,Space.World);
                }
            
         
       
            
        }




    }
}
