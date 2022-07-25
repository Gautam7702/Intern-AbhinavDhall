import cv2
from matplotlib.pyplot import draw
import mediapipe as mp
import time
import numpy as np
class FaceMeshDetector():
    def __init__(self,static_image_mode = False,maxFaces =2,refine_landmarks=False,minDetectionCon = 0.5,minTrackCon= 0.5):
        self.static_image_mode=static_image_mode
        self.refine_landmarks = refine_landmarks
        self.maxFaces = maxFaces
        self.minDetectionCon = minDetectionCon
        self.minTrackCon = minTrackCon
        self.mpDraw = mp.solutions.drawing_utils # help to draw on the face
        self.mpFaceMesh = mp.solutions.face_mesh 
        self.faceMesh = self.mpFaceMesh.FaceMesh(self.static_image_mode, self.maxFaces,self.refine_landmarks,self.minDetectionCon,self.minTrackCon) #a faceMesh is created
        self.drawSpec = self.mpDraw.DrawingSpec(thickness=1,circle_radius=2)
        

    def findFaceMesh(self,img,draw =True):
        imgRGB  = cv2.cvtColor(img,cv2.COLOR_BGR2RGB)
        results = self.faceMesh.process(imgRGB)
        faces =  []
        if results.multi_face_landmarks:
            for faceLms in results.multi_face_landmarks:
                if draw:
                    self.mpDraw.draw_landmarks(img,faceLms,self.mpFaceMesh.FACEMESH_CONTOURS,self.drawSpec,self.drawSpec)
                faces.append(faceLms.landmark)
        return img,faces
    
    def getLipContour(self,faceLms):
        LIP_HULL_IDS = [61, 185, 39, 37, 267, 269, 409, 291, 375, 321, 405, 314, 17, 84, 181, 91, 146] # calculated using convex hull algorithm 
        LIP_HULL_CORDINATES = list()
        for id in LIP_HULL_IDS:
            lm = faceLms[id]
            LIP_HULL_CORDINATES.append([lm.x,lm.y])
        return LIP_HULL_CORDINATES
    
    def getLeftEyebrow(self,faceLms):
        LE_HULL_IDS = [285, 295, 282, 283, 276]
        LE_HULL_CORDINATES = list()
        for id in LE_HULL_IDS:
            lm = faceLms[id]
            LE_HULL_CORDINATES.append([lm.x,lm.y])
        return LE_HULL_CORDINATES

    def getRightEyebrow(self,faceLms):
        RE_HULL_IDS = [70, 63, 105, 66, 107]
        RE_HULL_CORDINATES = list()
        for id in RE_HULL_IDS:
            lm = faceLms[id]
            RE_HULL_CORDINATES.append([lm.x,lm.y])
        return RE_HULL_CORDINATES
    
    def headPoseDetection(self,faceLms,img):
        FACE_KEY_POINTS = [33,263,1,61,291,199]
        FACE_3D = []
        FACE_2D = []
        ih,iw,ic = img.shape
        for idx,lm in enumerate(faceLms):
            if idx == 33 or idx == 263 or idx == 1 or idx == 61 or idx == 291 or idx == 199:
                x,y = int(lm.x*iw),int(lm.y*ih)
                FACE_2D.append([x,y])
                FACE_3D.append([x,y,lm.z])
        FACE_2D = np.array(FACE_2D,dtype = np.float64)
        FACE_3D = np.array(FACE_3D,dtype = np.float64)
        focal_length = 1*iw
        cam_matrix  =np.array([[focal_length,0,ih/2],
                                [0,focal_length,iw/2],
                                [0,0,1]])
        dist_matrix = np.zeros((4,1),dtype = np.float64)
        sucess,rot_vec,trans_vec = cv2.solvePnP(FACE_3D,FACE_2D,cam_matrix,dist_matrix)
        rmat,jac = cv2.Rodrigues(rot_vec)
        angles,mtxR,mtxQ,Qx,Qy,Qz = cv2.RQDecomp3x3(rmat)
        x = round(angles[0],6)*360
        y = round(angles[1],6)*360
        z = round(angles[2],6)*360
        return x,y

def main():
    cap = cv2.VideoCapture(0)
    pTime = 0
    detector = FaceMeshDetector(refine_landmarks=True)
    while True:
        sucess,img = cap.read()
        img,faces = detector.findFaceMesh(img)
        LIP_HULL_CORDINATES = []
        if len(faces)>0:
            detector.getLipContour(faces[0])
        if sucess == False:
            break
        cTime = time.time()
        fps = 1/(cTime-pTime)
        pTime = cTime
        cv2.imshow("Image",img)
        if cv2.waitKey(100) & 0xFF == ord('q'):
            break

if __name__ == "__main__":
    main()