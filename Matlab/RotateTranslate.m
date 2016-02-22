%% 
%{ 
This script is used to transform the position data of the disk from the 
workobjects coordinate system to the robot base cord.system. 
%}

%%
clc;
clear all; 
name = 'v500';

%% Choose plot
plotPosition = 1; 
plotVelocity = 1; 
plotAcceleration = 1; 
plotMeanPos = 1;
plotMeanVelocity = 1;
plotMeanAcceleration = 1;
writeProcessedDataToFile = 1;

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
            sentSensorXYZ(i,j-1) = str2double(strrep(tempSentSensorXYZ,...
                ',' , '.')); 
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
rotZ = [cosd(theta), -sind(theta), 0, 0 ; sind(theta), cosd(theta), 0,0;...
    0, 0, 1, 0 ; 0, 0, 0, 1];
rotY = [cosd(gamma), 0, sind(gamma), 0 ; 0, 1, 0, 0 ;...
    -sind(gamma), 0, cosd(gamma), 0 ; 0, 0, 0, 1];
rotX = [1, 0, 0, 0 ; 0, cosd(tau), -sind(tau), 0 ;...
    0, sind(tau), cosd(tau), 0 ; 0, 0, 0, 1];

% Translation matrix
translation = [1, 0, 0, TransX ; 0, 1, 0, TransY ; 0, 0, 1, TransZ...
    ; 0,0,0,1];

% Calculate the rotation matrix of the system
rotZYX = (-rotZ)*(-rotY)*(rotX);

% Transformation
wobjToBase = translation * rotZYX;

% Store the values in a new matrix
for i = 1: 1 : size(sentSensorXYZ,1)
        tempNewXYZCord = wobjToBase*(sentSensorXYZ(i,1:4))';
        newXYZcord(i, 1:4) = tempNewXYZCord';
end

% Remove the once from column 4
newXYZcord = newXYZcord(1:end, 1:3);

% Find the mean between the position in XYZ -direction
meanPosDisc = mean(newXYZcord,2);
meanPosRob = mean(robotXYZ,2);

%% Plot position in XYZ
if plotPosition == 1
    figure1 = figure;
    plot(time, newXYZcord(1:size(newXYZcord,1),cx),'-r');hold on; 
    plot(time, robotXYZ(1:size(robotXYZ,1),cx),'-b'); hold on; 

    plot(time, newXYZcord(1:size(newXYZcord,1),cy),'-k'); hold on; 
    plot(time, robotXYZ(1:size(robotXYZ,1),cy),'-g');hold on;

    plot(time, newXYZcord(1:size(newXYZcord,1),cz),'-m'); hold on; 
    plot(time, robotXYZ(1:size(robotXYZ,1),cz),'-c');

    legend('Disc X position','Robot X position','Disc Y position',...
        'Robot Y position','Disc Z position','Robot Z position' )
    xlabel('Time [s]')
    ylabel('Position [mm]')
    grid on;
    filename = ['Plot/positionXYZ',name,'.eps'];
    saveas(figure1, filename);
end

%% Plot mean position
if plotMeanPos == 1
    figure2 = figure;
    plot(time,meanPosDisc,'r',time,meanPosRob,'b');
    grid on;
    legend('Mean position disc', 'Mean position robot');
    filename = ['Plot/meanPosition',name,'.eps'];
    saveas(figure2, filename);
end

%% Plot velocity in XYZ
if plotVelocity == 1
    N = 6;
    windowSize = 8;
    translatedNewXYZCord = newXYZcord';
    translatedRobotXYZ = robotXYZ';
    
    posX = translatedNewXYZCord(1,:);
    posX = posX(1: N: length(posX));
    posRX = translatedRobotXYZ(1,:);
    posRX = posRX(1: N: length(posRX));
    
    posY = translatedNewXYZCord(2,:);
    posY = posY(1: N: length(posY));
    posRY = translatedRobotXYZ(2,:);
    posRY = posRY(1: N: length(posRY));
    
    posZ = translatedNewXYZCord(3,:);
    posZ = posZ(1: N: length(posZ));
    posRZ = translatedRobotXYZ(3,:);
    posRZ = posRZ(1: N: length(posRZ));
    
    timeNth = time(1: N: length(time)-1);
    
    b = (1/windowSize)*ones(1,windowSize);
    a = 1;

    velX = diff(posX);
    velRX = diff(posRX);
    
    velY = diff(posY);
    velRY = diff(posRY);
    
    velZ = diff(posZ);
    velRZ = diff(posRZ);
    
    velX = filter(b,a,velX);
    velY = filter(b,a,velY);
    velZ = filter(b,a,velZ);
    
    figure3 = figure;
    plot(timeNth(10:end), velX(10:end),'r', timeNth(10:end),...
        velRX(10:end), 'b'); hold on;
    plot(timeNth(10:end), velY(10:end),'k', timeNth(10:end),...
        velRY(10:end), 'g'); hold on;
    plot(timeNth(10:end), velZ(10:end),'m', timeNth(10:end),...
        velRZ(10:end), 'c'); 
    legend('Disc X velocity','Robot X velocity','Disc Y velocity',...
        'Robot Y velocity','Disc Z velocity','Robot Z velocity' )
    grid on;
    filename = ['Plot/velocityXYZ',name,'.eps'];
    saveas(figure3, filename);
end


    %% Plot mean velocity
    if plotMeanVelocity == 1
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
        
        figure4 = figure;
        plot(timeNth(10:end), meanVelDisc(10:end), timeNth(10:end),...
            meanVelRob(10:end)); 
        grid on; 
        legend('Mean speed disc', 'Mean speed robot');
        filename = ['Plot/meanVelocity',name,'.eps'];
        saveas(figure4, filename);
    end
    
    
    %% Plot acceleration in XYZ
    if plotAcceleration == 1
        acelX = diff(velX);
        acelRX = diff(velRX);
        acelY = diff(velY);
        acelRY = diff(velRY);
        acelZ = diff(velY);
        acelRZ = diff(velZ);

        acelX = filter(b,a,acelX);
        acelRX = filter(b,a,acelRX);
        acelY = filter(b,a,acelY);
        acelRY = filter(b,a,acelRY);
        acelZ = filter(b,a,acelZ);
        acelRZ = filter(b,a,acelRZ);
        
        figure5 = figure;
        plot(timeNth(20:end-1), acelX(20:end),'r', timeNth(20:end-1),...
            acelRX(20:end),'b'); hold on;
        plot(timeNth(20:end-1), acelY(20:end),'k', timeNth(20:end-1),...
            acelRY(20:end),'g'); hold on;
        plot(timeNth(20:end-1), acelZ(20:end),'m' ,timeNth(20:end-1),...
            acelRZ(20:end),'c'); 
        legend('Disc X acceleration','Robot X acceleration',...
            'Disc Y acceleration', 'Robot Y acceleration',...
            'Disc Z acceleration','Robot Z acceleration' );
        grid on; 
        filename = ['Plot/accelerationXYZ',name,'.eps'];
        saveas(figure5, filename);
    end
    
    %% Plot mean acceleration
    if plotMeanAcceleration == 1
        meanAccelDisc = diff(meanVelDisc);
        meanAccelRob = diff(meanVelRob);
        
        meanAccelDisc = filter(b,a,meanAccelDisc);
        meanAccelRob = filter(b,a,meanAccelRob);
        
        figure6 = figure;
        plot(timeNth(10:end-1), meanAccelDisc(10:end),timeNth(10:end-1),...
            meanAccelRob(10:end));
        grid on; 
        legend('Mean acceleration disc', 'Mean acceleration robot');
        filename = ['Plot/meanAcceleration',name,'.eps'];
        saveas(figure6, filename);
    end
    

%% Writing the processed data to txt file.
if writeProcessedDataToFile == 1
    filename = ['Plot/processedData',name,'.txt'];
    fileIDW = fopen(filename,'wt');
    fprintf(fileIDW, 'Time\tRobotX\tRobotY\tRobotZ\tDiscX\tDiscY\tDiscZ\n')
    for i = 1 :1 : size(robotXYZ,1)
            fprintf(fileIDW,'%#.6g \t',(time(:,i)),robotXYZ(i,:),...
                newXYZcord(i,:));
            fprintf(fileIDW, '\n');
    end
end
fclose('all');


