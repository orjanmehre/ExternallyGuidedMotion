%% 
%{ 
This script is used to transform the position data of the disk from the 
workobjects coordinate system to the robot base cord.system. 
%}

%%
clc;
clear all; 

%% Choose plot
plotPosition = 0; 
plotVelocity = 0; 
plotAcceleration = 0; 
writeProcessedDataToFile = 0;
plotMeanPos = 0;
plotMeanVelocity = 0;
plotMeanAcceleration = 0;

%%
cx = 1;
cy = 2;
cz = 3; 

% x, y and z coordinates for origo in the new cord.system.
TransX = 83.663994925;
TransY = -716.879172118;
TransZ = 531.771925931;

% The rotation angles (same as in RS)
theta = -30; 
gamma = 150; % 180- angle of the ramp
tau = 0; 

%Open file
fileId = fopen('position.txt');

%Store file content in a cell
C = textscan(fileId, '%s %s %s %s %s %s %s');
fclose(fileId);

%Extract the position data and convert from string to double
for i = 1: 1: size(C{1,7},1)
    tempTime = C{1,1}{i,1}; 
    time(i) = str2double(strrep(tempTime, ',' , '.'));
end

for j = 2: 1: 4
    for i = 1: 1: size(C{1,7},1)
            tempSentSensorXYZ = C{1,j}{i,1};
            sentSensorXYZ(i,j-1) = str2double(strrep(tempSentSensorXYZ, ',' , '.')); 
    
    end
end

for j = 5: 1: 7 
    for i = 1: 1: size(C{1,7},1)
        tempRobotXYZ = C{1,j}{i,1};
        robotXYZ(i, j-4) = str2double(strrep(tempRobotXYZ, ',' , '.'));
    end
end

for i = 1 : 1 : size(C{1,7},1)
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

newXYZcord = newXYZcord(1:end, 1:3);

meanPosDisc = mean(newXYZcord,2);
meanPosRob = mean(robotXYZ,2);

if plotMeanPos == 1
    figure;
    plot(time,meanPosDisc,time,meanPosRob);
end

%% Plot position
if plotPosition == 1
    figure;
    plot(time, newXYZcord(1:size(newXYZcord,1),cx),'-r');hold on; 
    plot(time, robotXYZ(1:size(robotXYZ,1),cx),'-b'); hold on; 

    plot(time, newXYZcord(1:size(newXYZcord,1),cy),'-k'); hold on; 
    plot(time, robotXYZ(1:size(robotXYZ,1),cy),'-g');hold on;

    plot(time, newXYZcord(1:size(newXYZcord,1),cz),'-m'); hold on; 
    plot(time, robotXYZ(1:size(robotXYZ,1),cz),'-c');

    legend('Disc X position','Robot X position','Disc Y position','Robot Y position','Disc Z position','Robot Z position' )
    xlabel('Time [s]')
    ylabel('Position [mm]')
    grid on; 
end

%% Plot velocity
if plotVelocity == 1
    N = 6;
    windowSize = 8;
    translatedNewXYZCord = newXYZcord';
    translatedRobotXYZ = robotXYZ';
    
    posX = translatedNewXYZCord(1,:);
    posX = posX(1: N: length(posX));
    posRX = translatedRobotXYZ(1,:);
    posRX = posRX(1: N: length(posRX));
    
    timeNth = time(1: N: length(time)-1);
    
    b = (1/windowSize)*ones(1,windowSize);
    a = 1;

    velX = diff(posX);
    velRX = diff(posRX);
    
    velX = filter(b,a,velX);
    
    figure;
    plot(timeNth(10:end), velX(10:end),timeNth(10:end),velRX(10:end))
end


    %% Plot mean velocity
    if plotMeanVelocity == 0
        N = 6;
        windowSize = 8;
        transMeanPos = meanPosDisc';
        transMeanRob = meanPosRob';
        
        meanPosDisc = transMeanPos(1: N: length(transMeanPos));
        meanPosRob = transMeanRob(1: N: length(transMeanRob));
        
        timeNth = time(1: N: length(time)-1);
        
        meanVelDisc = diff(meanPosDisc);
        meanVelRob = diff(meanPosRob);
        
        b = (1/windowSize)*ones(1,windowSize);
        a = 1;

        meanVelDisc = filter(b,a,meanVelDisc);
        %meanVelRob = filter(b,a,meanVelRob);
        
        figure;
        plot(timeNth(10:end), meanVelDisc(10:end), timeNth(10:end), meanVelRob(10:end));
        
    end
    
    
    %% Plot acceleration
    if plotAcceleration == 1
        acelX = diff(velX);
        acelRX = diff(velRX);

        acelX = filter(b,a,acelX);
        acelRX = filter(b,a,acelRX);

        figure;
        plot(timeNth(20:end-1), acelX(20:end),timeNth(20:end-1),acelRX(20:end))
        legend('Disc acceleration', 'Robot acceleration');
    end
    
    %% Plot mean acceleration
    if plotMeanAcceleration == 0
        meanAccelDisc = diff(meanVelDisc);
        meanAccelRob = diff(meanVelRob);
        
        meanAccelDisc = filter(b,a,meanAccelDisc);
        meanAccelRob = filter(b,a,meanAccelRob);
        
        figure;
        plot(timeNth(10:end-1), meanAccelDisc(10:end), timeNth(10:end-1), meanAccelRob(10:end));
        
    end
    



 

%% Writing the processed data to txt file.
if writeProcessedDataToFile == 1
    fileIDW = fopen('Test\16.02.03 60deg Test5(P).txt','wt');
    fprintf(fileIDW, 'Time \t\t RobotX \t RobotY \t RobotZ \t DiscX \t\t DiscY \t\t DiscZ \n')
    for i = 1 :1 : size(robotXYZ,1)
            fprintf(fileIDW,'%#.6g \t',(time(:,i)),robotXYZ(i,:), newXYZcord(i,:));
            fprintf(fileIDW, '\n');
    end
end
fclose('all');


