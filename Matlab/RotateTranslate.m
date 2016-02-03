%% 
% This script is used to transform the position data of the disk from the 
% workobjects coordinate system to the robot base cord.system. 
%%
clear all; 

% x, y and z coordinates for origo in the new cord.system.
TransX = 83.663994925;
TransY = -716.879172118;
TransZ = 531.771925931;

% The rotation angles (same as in RS)
theta = -30; 
gamma = 150;
tau = 0; 

%Open file
fileId = fopen('position.txt');

%Store file content in a cell
C = textscan(fileId, '%s %s %s %s %s %s %s');
fclose(fileId);

%Extract the position data and convert from string to double
for i = 1: 1: size(C{1,1},1)
    tempTime = C{1,1}{i,1}; 
    time(i) = str2double(strrep(tempTime, ',' , '.'));
end

for j = 2: 1: 4
    for i = 1: 1: size(C{1,2},1)
            tempSentSensorXYZ = C{1,j}{i,1};
            sentSensorXYZ(i,j-1) = str2double(strrep(tempSentSensorXYZ, ',' , '.')); 
    
    end
end

for j = 5: 1: 7 
    for i = 1: 1: size(C{1,j},1)
        tempRobotXYZ = C{1,j}{i,1};
        robotXYZ(i, j-4) = str2double(strrep(tempRobotXYZ, ',' , '.'));
    end
end

for i = 1 : 1 : size(C{1,2},1)
    sentSensorXYZ(i, 4) = 1;
end

%Rotation matrices
rotZ = [cosd(theta), -sind(theta), 0, 0 ; sind(theta), cosd(theta), 0, 0 ; 0, 0, 1, 0 ; 0, 0, 0, 1];
rotY = [cosd(gamma), 0, sind(gamma), 0 ; 0, 1, 0, 0 ; -sind(gamma), 0, cosd(gamma), 0 ; 0, 0, 0, 1];
rotX = [1, 0, 0, 0 ; 0, cosd(tau), -sind(tau), 0 ; 0, sind(tau), cosd(tau), 0 ; 0, 0, 0, 1];


% Translation matrix
translation = [1, 0, 0, TransX ; 0, 1, 0, TransY ; 0, 0, 1, TransZ ; 0,0,0,1];

% Calculate the rotation matrix of the system
rotZYX = (-rotZ)*(-rotY)*(rotX);

% Transformation
wobjToBase = translation * rotZYX;

% Store the values in a new matrix
for i = 1: 1 : size(sentSensorXYZ,1)
    
        tempNewXYZCord = wobjToBase*(sentSensorXYZ(i,1:4))';
        newXYZcord(i, 1:4) = tempNewXYZCord';
   
end







