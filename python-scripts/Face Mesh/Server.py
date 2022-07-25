from http import client
from turtle import shape
from winreg import REG_BINARY
import faceMeshModule
import cv2
import time
import socket
import json

server_socket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)
IP_ADDRESS = "127.0.0.1"
PORT  = 9999
server_socket.bind((IP_ADDRESS,PORT))
print('UDP server up and listening')
cap = cv2.VideoCapture(0)
detector = faceMeshModule.FaceMeshDetector(refine_landmarks=True)
ptime = 0
while True:
    sucess,img = cap.read()
    img,faces = detector.findFaceMesh(img=img,draw = True)
    LIP_HULL_CORDINATES = list()
    ROTATION_CORDINATES = list()
    if len(faces) > 0:
        LIP_HULL_CORDINATES = detector.getLipContour(faces[0]) 
        LE_COORDINATES  = detector.getLeftEyebrow(faces[0])
        RE_COORDINATES  = detector.getRightEyebrow(faces[0])
        x,y = detector.headPoseDetection(faces[0],img)
        ROTATION_CORDINATES = [[round(x,6)  ,round(y,6)/10]] #HEAD ROTATION
    DATA_TO_SEND = LIP_HULL_CORDINATES + LE_COORDINATES + RE_COORDINATES + ROTATION_CORDINATES
    ih,iw,ic = img.shape
    minm_x = iw+1
    maxm_x = 0
    if len(faces) >0:
        for points in faces[0]:
            x,y = points.x*iw,points.y*ih
            minm_x = min(x,minm_x)
            maxm_x = max(x,maxm_x)
    
    sp = (int(minm_x),0)
    ep  =(int(minm_x),ih)
    sp1 = (int(maxm_x),0)
    ep1 = (int(maxm_x),ih)
    cv2.line(img,sp,ep,(0,255,0),1)
    cv2.line(img,sp1,ep1,(0,255,0),1)
    server_socket.sendto(json.dumps(DATA_TO_SEND).encode(),("127.0.0.1",9990))
    ctime = time.time()
    fps = 1/(ctime-ptime)
    ptime = ctime
    cv2.putText(img,f"FPS = {int(fps)}",(20,70),cv2.FONT_HERSHEY_PLAIN,3,(0,255,0),3)
    cv2.imshow("image",img)
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

        
