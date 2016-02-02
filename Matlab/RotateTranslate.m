%% This script is used to rotate and translate the position data from 
% the robot. The position from the sensor is related to the wobj
% but the position data from the robot is related to the base.
%%

vx = 83.663994925;
vy = -716.879172118;
vz = 531.771925931;

%Open file
fileId = fopen('position.txt');

%Stor file content in a cell
C = textscan(fileId, '%s %s %s %s %s %s');
fclose(fileId);

%Extract the position data and convert from string to double
for i = 1: 1: size(C{1,1},1)
    midTime = C{1,1}{i,1}; 
    time(i) = str2double(midTime);
end

for i = 1: 1: size(C{1,2},1)
    midSentX = C{1,2}{i,1};
    sentX(i) = str2double(midSentX);
end

for i = 1: 1: size(C{1,3},1)
    midSentY = C{1,3}{i,1};
    sentY(i) = str2double(midSentY);
end

for i = 1: 1: size(C{1,4},1)
    midRobotX = C{1,4}{i,1};
    robotX(i) = str2double(midRobotX);
end

for i = 1: 1: size(C{1,5},1)
    midRobotY = C{1,5}{i,1};
    robotY(i) = str2double(midRobotY);
end

for i = 1: 1: size(C{1,6},1)
    midRobotZ = C{1,6}{i,1};
    robotZ(i) = str2double(midRobotZ);
end


%Rotation matrices
rotZ = [cos(theta), -sin(theta), 0: sin(theta), cos(theta), 0: 0, 0, 1];
rotY = [cos(gamma), 0, sin(gamma): 0, 1, 0: -sin(gamma), 0, cos(gamma)];
rotX = [1, 0, 0: 0, cos(tau), -sin(tau): 0, sin(tau), cos(tau)];


%translation matrix
translation = [1, 0, 0, vx : 0, 1, 0, vy : 0, 0, 1, vz : 0,0,0,1];

