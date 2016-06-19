# ExternallyGuidedMotion
This solution can be used to pick up a disc sliding down a ramp using an industrial ABB robotic arm with EGM implemented.
As a sensor a Cognex In-Sight 5400 smart camera was used. This camera sent the discs position over to the program written in C#. 
All tests were run on an IRB140 industrial robotic arm. 
 
## Matlab folder
In the Matlab folder there is a script that transform the disc's position from the workobject's coordninate system to the coordinate system of the robot's base.

## Robotstudio folder
Here are the program files for the program in Robotstudio. Just extract the "pack-and go" file to unpack the full program.

## Cognex folder
Contains the program written for the Cognex smart camera to detect the disc.

