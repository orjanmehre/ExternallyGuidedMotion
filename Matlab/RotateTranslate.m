%% 
% This script is used to rotate and translate the position data from 
% the robot. The position from the sensor is related to the wobj
% but the position data from the robot is related to the base.
%%
clear all; 

% x, y and z coordinates for origo in the new cord.system.
vx = 83.663994925;
vy = -716.879172118;
vz = 531.771925931;

degreesToRad = pi/180;

theta = 135*degreesToRad;
gamma = 45 *degreesToRad; 
tau = 0; 

%Open file
fileId = fopen('position.txt');

%Stor file content in a cell
C = textscan(fileId, '%s %s %s %s %s %s');
fclose(fileId);

%Extract the position data and convert from string to double
for i = 1: 1: size(C{1,1},1)
    tempTime = C{1,1}{i,1}; 
    time(i) = str2double(tempTime);
end

for j = 2: 1: 3
    for i = 1: 1: size(C{1,2},1)
        tempSentSensorXY = C{1,j}{i,1};
        sentSensorXY(i,j-1) = str2double(tempSentSensorXY);
    end
end

for j = 4: 1: 6 
    for i = 1: 1: size(C{1,j},1)
        tempRobotXYZ = C{1,j}{i,1};
        robotXYZ(i, j-3) = str2double(tempRobotXYZ);
    end
end

%Rotation matrices
rotZ = [cos(theta), -sin(theta), 0: sin(theta), cos(theta), 0: 0, 0, 1];
rotY = [cos(gamma), 0, sin(gamma): 0, 1, 0: -sin(gamma), 0, cos(gamma)];
rotX = [1, 0, 0: 0, cos(tau), -sin(tau): 0, sin(tau), cos(tau)];


%translation matrix
translation = [1, 0, 0, vx : 0, 1, 0, vy : 0, 0, 1, vz : 0,0,0,1];



