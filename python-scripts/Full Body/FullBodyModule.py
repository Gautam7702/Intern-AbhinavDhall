from asyncio.windows_events import NULL
import cv2
from matplotlib.pyplot import draw
import mediapipe as mp
import time
import numpy as np
mpPose = mp.solutions.pose
class FullBodyModule():
    def __init__(self,static_image_mode = False,maxFaces =2,refine_landmarks=False,minDetectionCon = 0.5,minTrackCon= 0.5):
        self.static_image_mode=static_image_mode
        self.refine_landmarks = refine_landmarks
        self.maxFaces = maxFaces
        self.minDetectionCon = minDetectionCon
        self.minTrackCon = minTrackCon
        self.mpDraw = mp.solutions.drawing_utils # help to draw on the face
        self.mpFaceMesh = mp.solutions.face_mesh 
        self.mpPose = mp.solutions.pose
        self.PosePoints = self.mpPose.Pose(min_detection_confidence = self.minDetectionCon , min_tracking_confidence = self.minTrackCon)
        self.faceMesh = self.mpFaceMesh.FaceMesh(self.static_image_mode, self.maxFaces,self.refine_landmarks,self.minDetectionCon,self.minTrackCon) #a faceMesh is created
        self.drawSpec = self.mpDraw.DrawingSpec(thickness=1,circle_radius=2)
        
    
    def findPosePoints(self,img,draw= True):
        imgRGB  = cv2.cvtColor(img,cv2.COLOR_BGR2RGB)
        results = self.PosePoints.process(imgRGB)
        if draw:
            self.mpDraw.draw_landmarks(img,results.pose_landmarks,self.mpPose.POSE_CONNECTIONS,self.mpDraw.DrawingSpec(color = (245,117,66),thickness = 2,circle_radius = 2),
            self.mpDraw.DrawingSpec(color = (245,66,280),thickness = 2,circle_radius = 2))
        try:
            landmarks = results.pose_landmarks.landmark
        except: 
            return img,NULL
        return img,landmarks
    def calc_angle(self,landmarks,pos1,pos2,pos3,):
        if pos1.visibility < 0.5 or pos2.visibility < 0.5 or pos3.visibility < 0.5:
            return -1,-1,-1
        a = [pos1.x,pos1.y,pos1.z]
        b = [pos2.x,pos2.y,pos2.z]
        c = [pos3.x,pos3.y,pos3.z]
        a = np.array(a)
        b = np.array(b)
        c = np.array(c)
        radian_xy = np.arctan2(c[1]-b[1],c[0]-b[0])  - np.arctan2(a[1]-b[1],a[0]-b[0])
        angle_xy = np.abs(radian_xy*180/np.pi)
        if angle_xy > 180:
            angle_xy = 360-angle_xy
        radian_xz = np.arctan2(c[2]-b[2],c[0]-b[0])  - np.arctan2(a[2]-b[2],a[0]-b[0])
        angle_xz = np.abs(radian_xz*180/np.pi)
        if angle_xz > 180:
            angle_xz = 360-angle_xz
        radian_zy = np.arctan2(c[1]-b[1],c[2]-b[2])  - np.arctan2(a[1]-b[1],a[2]-b[2])
        angle_zy = np.abs(radian_zy*180/np.pi)
        if angle_zy > 180:
            angle_zy = 360-angle_zy
        return angle_xy,angle_xz,angle_zy

    def left_shoulder_angle(self,landmarks):
        left_hip = landmarks[self.mpPose.PoseLandmark.LEFT_HIP.value]
        left_shoudlder = landmarks[self.mpPose.PoseLandmark.LEFT_SHOULDER.value]
        left_elbow = landmarks[self.mpPose.PoseLandmark.LEFT_ELBOW.value]
        return self.calc_angle(landmarks,left_hip,left_shoudlder,left_elbow)

    def right_shoulder_angle(self,landmarks):
        right_hip = landmarks[self.mpPose.PoseLandmark.RIGHT_HIP.value]
        right_shoudlder = landmarks[self.mpPose.PoseLandmark.RIGHT_SHOULDER.value]
        right_elbow = landmarks[self.mpPose.PoseLandmark.RIGHT_ELBOW.value]
        return self.calc_angle(landmarks,right_hip,right_shoudlder,right_elbow)
    
    def left_elbow_angle(self,landmarks):
        left_shoudlder = landmarks[self.mpPose.PoseLandmark.LEFT_SHOULDER.value]
        left_elbow = landmarks[self.mpPose.PoseLandmark.LEFT_ELBOW.value]
        left_wrist =  landmarks[self.mpPose.PoseLandmark.LEFT_WRIST.value]
        return self.calc_angle(landmarks,left_shoudlder,left_elbow,left_wrist)
    
    def right_elbow_angle(self,landmarks):
        right_shoudlder = landmarks[self.mpPose.PoseLandmark.RIGHT_SHOULDER.value]
        right_elbow = landmarks[self.mpPose.PoseLandmark.RIGHT_ELBOW.value]
        right_wrist = landmarks[self.mpPose.PoseLandmark.RIGHT_WRIST.value]
        return self.calc_angle(landmarks,right_shoudlder,right_elbow,right_wrist)
       
def main():
    cap = cv2.VideoCapture(0)
    pTime = 0
    detector = FullBodyModule(refine_landmarks=True)
    while True:
        sucess,img = cap.read()
        cv2.flip(img,1,img)
        img,pose_landmarks = detector.findPosePoints(img)
        left_shoudlder = pose_landmarks[mpPose.PoseLandmark.LEFT_SHOULDER.value]
        left_elbow = pose_landmarks[mpPose.PoseLandmark.LEFT_ELBOW.value]
        right_shoudlder = pose_landmarks[mpPose.PoseLandmark.RIGHT_SHOULDER.value]
        right_elbow = pose_landmarks[mpPose.PoseLandmark.RIGHT_ELBOW.value]
        angle = detector.left_shoulder_angle(pose_landmarks)
        # cv2.putText(img,str(int(left_angle_sew_xz)),tuple(np.multiply([left_elbow[0],left_elbow[1]],[710,480]).astype(int)),cv2.FONT_HERSHEY_PLAIN,1,(255,255,255),2,cv2.LINE_AA) 
        cv2.putText(img,f"{int(angle[0])}",tuple(np.multiply([left_shoudlder.x,left_shoudlder.y],[710,480]).astype(int)),cv2.FONT_HERSHEY_PLAIN,1,(255,255,255),2,cv2.LINE_AA)
        angle = detector.right_shoulder_angle(pose_landmarks)
        cv2.putText(img,f"{int(angle[0])}",tuple(np.multiply([right_shoudlder.x,right_shoudlder.y],[710,480]).astype(int)),cv2.FONT_HERSHEY_PLAIN,1,(255,255,255),2,cv2.LINE_AA)
        angle = detector.left_elbow_angle(pose_landmarks)
        cv2.putText(img,f"{int(angle[0])}",tuple(np.multiply([left_elbow.x,left_elbow.y],[710,480]).astype(int)),cv2.FONT_HERSHEY_PLAIN,1,(255,255,255),2,cv2.LINE_AA)
        angle = detector.right_elbow_angle(pose_landmarks)
        cv2.putText(img,f"{int(angle[0])}",tuple(np.multiply([right_elbow.x,right_elbow.y],[710,480]).astype(int)),cv2.FONT_HERSHEY_PLAIN,1,(255,255,255),2,cv2.LINE_AA)
        cv2.imshow("Image",img)
        if cv2.waitKey(100) & 0xFF == ord('q'):
            break

if __name__ == "__main__":
    main()