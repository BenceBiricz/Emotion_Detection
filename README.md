# Emotion_Detection

The task is to detect and recognise the emotions of the human face. Facial expressions are a type of non-verbal communication used by people to give feedback about how a person is feeling. Nowadays, facial recognition is used in many areas of everyday life.
The elements used in this exercise:
- Visual Studio 2019 .NET
- Emgu.CV 4.1.1.3497
- haarcascade_frontalface_alt_tree.xml - for face detection
- SVM - Support Vector Machine
- Yale Face Database

#Input image scanning
In order to start the image processing procedure, it is necessary that the program can read any image of your choice. Using the Visual Studio Windows Forms project development environment makes it easy to visualize the image and manage the changes made to it. The input can be easily read using OpenFileDialog, which allows the image to be selected and accessed from anywhere on the computer. The search can be limited by setting the appropriate filters (JPG, BMP, PNG).
In the solution, images with GIF extension from the database were read and tested.
Face detection
To determine the position of the face, I used the Haar-Cascade function of Emgu.CV, which detects the face in the image using the haarcascade_frontalface_alt_tree.xml classifier file and draws a square around it. The DetectMultiScale function performs the detection of the face Figure 1.
Set parameters:
- bgrImage - the scanned image
- scaleFactor = 1.4 (specifies how much the image size is reduced at each image scale)
- minNeighbors =1 (specifies how many neighbours each candidate rectangle should have to keep it)

#Scan database
Similar to the image scan, you can use the FolderBrowserDialog to scan the entire image database folder. Also in the scanned images the parts outside the face are omitted. A FaceData helper class stores the images in a list. The names of the images indicate which emotion is included in the image, this will be exploited during testing.
The name structure of the images is: subject011.happy - where a number before the dot (so in this case 1) defines the emotion.
Classification of emotions:
- 1 - happy
- 2 - normal
- 3 - sad
- 4 - sleepy
- 5 - surprised
Preparing the database
Using a Helper class, I prepare the database for machine learning and the images can be split up for learning and validation.

#HOG
Various features need to be extracted from the face in order to allow further classification. These features were determined using the HOG function in Emgu.CV. This technique counts the occurrences of color transition shifting in localized parts of the image. It is computed on a dense grid of uniformly spaced cells and applies overlapping local contrast normalization to increase accuracy.

#Classification of emotions
I classified the facial emotions using Support Vector Machine. SVMs are a group of related supervised learning methods used for classification and regression. When the input data is viewed as two sets of vectors in an n-dimensional space. The SVM creates a separating hyperplane in this space that maximizes the margin between the two data sets.

![Screenshot 2023-08-26 184148](https://github.com/BenceBiricz/Emotion_Detection/assets/71565433/66313b70-9200-4b04-ae2f-d64a41ef14e6)

![Screenshot 2023-08-26 184227](https://github.com/BenceBiricz/Emotion_Detection/assets/71565433/37b308c7-3b91-4d80-bb5c-5ccae09a5a6a)

