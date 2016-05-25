%% 
%{ 
This script is used to transform the position data of the disk from the 
workobjects coordinate system to the robot base cord.system. 
Input for this script is position data in a tab seperated txt-file.
The order of the data in the txt-file should be: 
Time X-Disc Y-Disc Z-Disc X-Robot Y-Robot Z-Robot, seperated by tab. 
This script outputs plots of the position, velocity and accelereation of 
the disc and robot. 
%}

%%
clc;
clear; 
name = 'v100';

%% Choose plot
plotPosition                =	1; 
plotMeanPos                 =   1;
plotVelocity                =	1;
plotMeanVelocity            =   1;
plotAcceleration            =   1; 
plotMeanAcceleration        =   1;
writeProcessedDataToFile    =   0;

%%
cx = 1;
cy = 2;
cz = 3;
plotFrom = 1;

% x, y and z coordinates for origo in the new cord.system.
TransX = 550;
TransY = -479;
TransZ = 522;

% The rotation angles (same as in RS)
theta = 0; 
gamma = 160; % 180 minus angle of the ramp
tau = 0; 

%Open file
fileId = fopen('position.txt');

%Store file content in a cell
C = textscan(fileId, '%s %s %s %s %s %s %s %s');
fclose(fileId);

%% Extract the position data and convert from string to double
% Extract time
for i = 1: 1: size(C{1,7},1)
    tempTime = C{1,1}{i,1}; 
    time(i) = str2double(strrep(tempTime, ',' , '.'));
end

% Extract disc position 
for j = 2: 1: 4
    for i = 1: 1: size(C{1,7},1)
            tempSentSensorXYZ = C{1,j}{i,1};
            sentSensorXYZ(i,j-1) = str2double(strrep(tempSentSensorXYZ,...
                ',' , '.')); 
    end
end

% Extract robot position
for j = 5: 1: 7 
    for i = 1: 1: size(C{1,7},1)
        tempRobotXYZ = C{1,j}{i,1};
        robotXYZ(i, j-4) = str2double(strrep(tempRobotXYZ, ',' , '.'));
    end
end

for i = 1 : 1 : size(C{1,7},1)
    sentSensorXYZ(i, 4) = 1;
end

% Extract how far ahead the predictor i projecting in time
for i = 1: 1: size(C{1,8},1)
    tempPredTime = C{1,8}{i,1}; 
    predTime(i) = str2double(strrep(tempPredTime, ',' , '.'));
end

%% Transform the discs position data to the robots cord.system
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

%% Interpolate and filter the data
% Interpolate time
timei = time(1):1:time(end);

for i = 1: 1: length(predTime)
    timePred(i) = time(i) - predTime(i);
end

timePredi = timePred(1):1:timePred(end);

timePredi = timePredi(1:end-(length(timePredi) - length(timei)));

% Interpolate disc XYZ-cord
newXYZcordi = interp1(time,newXYZcord(1:end-(length(time)-length(time)),:)...
    ,timei,'pchip');

% Interpolate robot XYZ-cord
robotXYZi = interp1(time,robotXYZ(1:end-(length(time)-length(time)),:)...
    ,timei,'pchip');

% Smooth discs position data
for i = 1: 1: 3
    newXYZcordi(1:end,i) = smooth(newXYZcordi(1:end,i),100,'loess');
end

% Smooth robot position data
for i = 1: 1: 3
    robotXYZi(1:end,i) = smooth(robotXYZi(1:end,i),100,'loess');
end


for i = 1: 1: size(newXYZcordi,1)
    PositionX(i) = (newXYZcordi(1,1)- newXYZcordi(i,1)).^2;
end

%% Finding the distance which both the robot and disc have traveled. 

% Find mean distance for the disc
for i = 1: 1 : size(newXYZcordi,1)
        distanceDisc(i,1)= sqrt((newXYZcordi(1,1) - newXYZcordi(i,1)).^2 ...
        + (newXYZcordi(1,2) - newXYZcordi(i,2)).^2 ...
        + (newXYZcordi(1,3) - newXYZcordi(i,3)).^2);
end

% Find mean distance for the robot
for i = 1: 1 : size(robotXYZi,1)
        distanceRob(i,1) = sqrt((robotXYZi(1,1) - robotXYZi(i,1)).^2 + ...
            (robotXYZi(1,2) - robotXYZi(i,2)).^2 ...
            + (robotXYZi(1,3) - robotXYZi(i,3)).^2);       
end

%% Plot position in XYZ
if plotPosition == 1
    figure1 = figure;
    plot(timei(plotFrom:end), newXYZcordi(plotFrom:size(newXYZcordi,1),cx)...
        ,'-r');hold on; 
    plot(timePredi(plotFrom:end), robotXYZi(plotFrom:size(robotXYZi,1),cx)...
        ,'-b'); hold on; 

    plot(timei(plotFrom:end), newXYZcordi(plotFrom:size(newXYZcordi,1),cy)...
        ,'-k'); hold on; 
    plot(timePredi(plotFrom:end), robotXYZi(plotFrom:size(robotXYZi,1),cy)...
        ,'-g');hold on;

    plot(timei(plotFrom:end), newXYZcordi(plotFrom:size(newXYZcordi,1),cz)...
        ,'-m'); hold on; 
    plot(timePredi(plotFrom:end), robotXYZi(plotFrom:size(robotXYZi,1),cz),'-c');
    
    legend('Disc X position','Robot X position','Disc Y position',...
        'Robot Y position','Disc Z position','Robot Z position',...
        'Location','eastoutside')
    xlabel('Time [ms]')
    ylabel('Position [mm]')
    grid on;
    filename = ['Plot/positionXYZ',name,'.eps'];
    saveas(figure1, filename);
    filenamejpg = ['Plot/positionXYZJPG',name,'.jpg'];
    saveas(figure1, filenamejpg);
end

%% Plot mean position
if plotMeanPos == 1
    figure2 = figure;
    plot(timei(plotFrom:end),distanceDisc(plotFrom:end),'r',...
        timePredi(plotFrom:end), distanceRob(plotFrom:end),'b');
    grid on;
    legend('Distance disc', 'Distance robot','Location',...
        'northoutside','Orientation','horizontal');
    xlabel('Time [ms]')
    ylabel('Position [mm]')
    filename = ['Plot/distance',name,'.eps'];
    saveas(figure2, filename);
    filenamejpg = ['Plot/distanceJPG',name,'.jpg'];
    saveas(figure2, filenamejpg);
end

%% Plot velocity in XYZ
if plotVelocity == 1
    translatedNewXYZCord = newXYZcordi';
    translatedRobotXYZ = robotXYZi';
    
    posX = translatedNewXYZCord(1,:);
    posRX = translatedRobotXYZ(1,:);
    
    posY = translatedNewXYZCord(2,:);
    posRY = translatedRobotXYZ(2,:);
    
    posZ = translatedNewXYZCord(3,:);
    posRZ = translatedRobotXYZ(3,:);
    
    % Derivate position to get velocity
    velX = diff(posX);
    velRX = diff(posRX);
    velY = diff(posY);
    velRY = diff(posRY);
    velZ = diff(posZ);
    velRZ = diff(posRZ);
  
    figure3 = figure;
    plot(timei(plotFrom:end-1), velX(plotFrom:end),'r',...
        timei(plotFrom:end-1), velRX(plotFrom:end), 'b'); hold on;
    plot(timei(plotFrom:end-1), velY(plotFrom:end),'k',...
        timei(plotFrom:end-1), velRY(plotFrom:end), 'g'); hold on;
    plot(timei(plotFrom:end-1), velZ(plotFrom:end),'m',...
        timei(plotFrom:end-1), velRZ(plotFrom:end), 'c'); 
    legend('Disc X velocity','Robot X velocity','Disc Y velocity',...
        'Robot Y velocity','Disc Z velocity','Robot Z velocity',...
        'Location','eastoutside')
    xlabel('Time [ms]')
    ylabel('Velocity [mm/ms]')
    grid on;
    filename = ['Plot/velocityXYZ',name,'.eps'];
    saveas(figure3, filename);
    filenamejpg = ['Plot/velocityXYZJPG',name,'.jpg'];
    saveas(figure3, filenamejpg);
end

    %% Plot mean velocity
    if plotMeanVelocity == 1
%         meanVelDisc = sqrt((velX).^2+(velY).^2+(velZ).^2);
%         meanVelRob = sqrt((velRX).^2+(velRY).^2+(velRZ).^2);

meanVelDisc = diff(distanceDisc);
meanVelRob = diff(distanceRob);

        
        figure4 = figure;
        plot(timei(plotFrom:end-1), meanVelDisc(plotFrom:end),...
            timei(plotFrom:end-1), meanVelRob(plotFrom:end)); 
        xlabel('Time [ms]')
        ylabel('Velocity [mm/ms]')
        grid on; 
        legend('Mean speed disc', 'Mean speed robot','Location',...
            'northoutside','Orientation','horizontal');
        filename = ['Plot/meanVelocity',name,'.eps'];
        saveas(figure4, filename);
        filenamejpg = ['Plot/meanVelocityJPG',name,'.jpg'];
        saveas(figure4, filenamejpg);
    end
    
    %% Plot acceleration in XYZ
    if plotAcceleration == 1
        acelX = diff(velX);
        acelRX = diff(velRX);
        acelY = diff(velY);
        acelRY = diff(velRY);
        acelZ = diff(velY);
        acelRZ = diff(velZ);
        
        figure5 = figure;
        plot(timei(plotFrom:end-2), acelX(plotFrom:end),'r',...
            timei(plotFrom:end-2), acelRX(plotFrom:end),'b'); hold on;
        plot(timei(plotFrom:end-2), acelY(plotFrom:end),'k',...
            timei(plotFrom:end-2), acelRY(plotFrom:end),'g'); hold on;
        plot(timei(plotFrom:end-2), acelZ(plotFrom:end),'m' ,...
            timei(plotFrom:end-2), acelRZ(plotFrom:end),'c'); 

        legend('Disc X acceleration','Robot X acceleration',...
            'Disc Y acceleration', 'Robot Y acceleration',...
            'Disc Z acceleration','Robot Z acceleration',...
            'Location','eastoutside');
        xlabel('Time [ms]')
        ylabel('Acceleration [mm/ms^2]')
        grid on; 
        filename = ['Plot/accelerationXYZ',name,'.eps'];
        saveas(figure5, filename);
        filenamejpg = ['Plot/accelerationXYZJPG',name,'.jpg'];
        saveas(figure5, filenamejpg);
    end
    
    %% Plot mean acceleration
    if plotMeanAcceleration == 1
%         meanAccelDisc = sqrt((acelX).^2 + (acelY).^2 + (acelZ).^2);
%         meanAccelRob = sqrt((acelRX).^2 + (acelRY).^2 + (acelRZ).^2);

         meanAccelDisc = diff(meanVelDisc); 
         meanAccelRob = diff(meanVelRob);

        
        figure6 = figure;
        plot(timei(plotFrom:end-2), meanAccelDisc(plotFrom:end),...
            timei(plotFrom:end-2), meanAccelRob(plotFrom:end));
        xlabel('Time [ms]')
        ylabel('Acceleration [mm/ms^2]')
        grid on; 
        legend('Mean acceleration disc', 'Mean acceleration robot',...
            'Location','northoutside','Orientation','horizontal');
        filename = ['Plot/meanAcceleration',name,'.eps'];
        saveas(figure6, filename);
        filenamejpg = ['Plot/meanAccelerationJPG',name,'.jpg'];
        saveas(figure6, filenamejpg);
    end
    
%% Writing the processed data to txt file.
if writeProcessedDataToFile == 1
    filename = ['Plot/ProcessedData/processedData',name,'.txt'];
    fileIDW = fopen(filename,'wt');
    fprintf(fileIDW, 'Time\tRobotX\tRobotY\tRobotZ\tDiscX\tDiscY\tDiscZ\n')
    for i = 1 :1 : size(robotXYZ,1)
            fprintf(fileIDW,'%#.6g \t',(time(:,i)),robotXYZ(i,:),...
                newXYZcord(i,:));
            fprintf(fileIDW, '\n');
    end
end
fclose('all');


