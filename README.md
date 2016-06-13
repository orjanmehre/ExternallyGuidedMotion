# ExternallyGuidedMotion

This program combined with a program written i RAPID will be used to pick up a ball rolling down a ramp using a ABB robotic arm. 
To detect the ball a camera is used, so this program detect and predict the trajectory of the ball. 
The position data of the ball is sent over to the RAPID program using UDPuc packets. 

## Matlab folder

In the Matlab folder there is a script that transform the discs position from the workobjects coordninate system to the coordinate system of the robot base.

## Robotstudio folder
Here are the program files for the program in Robotstudio. Just extract the "pack-and go" file to unpack the full program.

## Cognex folder
.job file for the program made in In-Sight Explorer. This is the vision program for the Cognex 5400 smart camera

