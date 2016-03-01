%% 
%{ 
This script is used to transform the position data of the disk from the 
workobjects coordinate system to the robot base cord.system. 
%}

%%
clc;
clear all; 
name = '50Deg';

%% Choose plot
plotPosition = 1; 
plotVelocity = 0; 
plotAcceleration = 0; 
plotMeanPos = 1;
plotMeanVelocity = 0;
plotMeanAcceleration = 0;
writeProcessedDataToFile = 0;

%%
cx = 1;
cy = 2;
cz = 3;
plotFrom = 50;

% x, y and z coordinates for origo in the new cord.system.
TransX = 83.0;
TransY = -716.0;
TransZ = 700.0;

% The rotation angles (same as in RS)
theta = -30; 
gamma = 130; % 180- angle of the ramp
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

% Smooth data
filt = fir1(30, 0.01);
newXYZcord = filter(filt,1,newXYZcord);
robotXYZ = filter(filt,1,robotXYZ);

% Find the mean between the position in XYZ -direction
meanPosDisc = mean(newXYZcord,2);
meanPosRob = mean(robotXYZ,2);

%% Plot position in XYZ
if plotPosition == 1
    figure1 = figure;
    plot(time(plotFrom:end), newXYZcord(plotFrom:size(newXYZcord,1),cx),'-r');hold on; 
    plot(time(plotFrom:end), robotXYZ(plotFrom:size(robotXYZ,1),cx),'-b'); hold on; 

    plot(time(plotFrom:end), newXYZcord(plotFrom:size(newXYZcord,1),cy),'-k'); hold on; 
    plot(time(plotFrom:end), robotXYZ(plotFrom:size(robotXYZ,1),cy),'-g');hold on;

    plot(time(plotFrom:end), newXYZcord(plotFrom:size(newXYZcord,1),cz),'-m'); hold on; 
    plot(time(plotFrom:end), robotXYZ(plotFrom:size(robotXYZ,1),cz),'-c');
    
    legend('Disc X position','Robot X position','Disc Y position',...
        'Robot Y position','Disc Z position','Robot Z position',...
        'Location','northoutside')
    legend('boxoff')
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
    plot(time(50:end),meanPosDisc(50:end),'b',time(50:end),...
        meanPosRob(50:end),'r');
    grid on;
    legend('Mean position disc', 'Mean position robot','Location',...
        'northoutside','Orientation','horizontal');
    legend('boxoff');
    xlabel('Time [ms]')
    ylabel('Position [mm]')
    filename = ['Plot/meanPosition',name,'.eps'];
    saveas(figure2, filename);
    filenamejpg = ['Plot/meanPositionJPG',name,'.jpg'];
    saveas(figure2, filenamejpg);
end

%% Plot velocity in XYZ
if plotVelocity == 1
    translatedNewXYZCord = newXYZcord';
    translatedRobotXYZ = robotXYZ';
    
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
    
    velX = filter(filt,1,velX);
    velY = filter(filt,1,velY);
    velZ = filter(filt,1,velZ);
    velRX = filter(filt,1,velRX);
    velRY = filter(filt,1,velRY);
    velRZ = filter(filt,1,velRZ);
    
    figure3 = figure;
    plot(time(50:end-1), velX(50:end),'r', time(50:end-1),...
        velRX(50:end), 'b'); hold on;
    plot(time(50:end-1), velY(50:end),'k', time(50:end-1),...
        velRY(50:end), 'g'); hold on;
    plot(time(50:end-1), velZ(50:end),'m', time(50:end-1),...
        velRZ(50:end), 'c'); 
    legend('Disc X velocity','Robot X velocity','Disc Y velocity',...
        'Robot Y velocity','Disc Z velocity','Robot Z velocity',...
        'Location','eastoutside')
    grid on;
    filename = ['Plot/velocityXYZ',name,'.eps'];
    saveas(figure3, filename);
end


    %% Plot mean velocity
    if plotMeanVelocity == 1
        transMeanPos = meanPosDisc';
        transMeanRob = meanPosRob';
      
        meanVelDisc = diff(meanPosDisc);
        meanVelRob = diff(meanPosRob);
        
        figure4 = figure;
        plot(time(50:end-1), meanVelDisc(50:end), time(50:end-1),...
            meanVelRob(50:end)); 
        grid on; 
        legend('Mean speed disc', 'Mean speed robot''Location',...
            'northoutside','Orientation','horizontal');
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
        
        acelX = filter(filt,1,acelX);
        acelY = filter(filt,1,acelY);
        acelZ = filter(filt,1,acelZ);
        acelRX = filter(filt,1,acelRX);
        acelRY = filter(filt,1,acelRY);
        acelRZ = filter(filt,1,acelRZ);
        
        figure5 = figure;
        plot(time(100:end-2), acelX(100:end),'r', time(100:end-2),...
            acelRX(100:end),'b'); hold on;
        plot(time(100:end-2), acelY(100:end),'k', time(100:end-2),...
            acelRY(100:end),'g'); hold on;
        plot(time(100:end-2), acelZ(100:end),'m' ,time(100:end-2),...
            acelRZ(100:end),'c'); 
        legend('Disc X acceleration','Robot X acceleration',...
            'Disc Y acceleration', 'Robot Y acceleration',...
            'Disc Z acceleration','Robot Z acceleration',...
            'Location','eastoutside');
        grid on; 
        filename = ['Plot/accelerationXYZ',name,'.eps'];
        saveas(figure5, filename);
    end
    
    %% Plot mean acceleration
    if plotMeanAcceleration == 1
        meanAccelDisc = diff(meanVelDisc);
        meanAccelRob = diff(meanVelRob);
        
        meanAccelDisc = filter(filt,1,meanAccelDisc);
        meanAccelRob = filter(filt,1,meanAccelRob);

        figure6 = figure;
        plot(time(100:end-2), meanAccelDisc(100:end),time(100:end-2),...
            meanAccelRob(100:end));
        grid on; 
        legend('Mean acceleration disc', 'Mean acceleration robot',...
            'Location','northoutside','Orientation','horizontal');
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


