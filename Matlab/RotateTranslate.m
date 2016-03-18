%% 
%{ 
This script is used to transform the position data of the disk from the 
workobjects coordinate system to the robot base cord.system. 
%}

%%
clc;
clear all; 
name = 'v100';

%% Choose plot
plotPosition                =	1; 
plotMeanPos                 =   1;
plotVelocity                =	1;
plotMeanVelocity            =   1;
plotAcceleration            =   1; 
plotMeanAcceleration        =   1;
writeProcessedDataToFile    =   1;

%%
cx = 1;
cy = 2;
cz = 3;
plotFrom = 1;

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
% filt = fir1(30, 0.01);
% newXYZcord = filter(filt,1,newXYZcord);
% robotXYZ = filter(filt,1,robotXYZ);

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
    plot(time(1:end),meanPosDisc(1:end),'r',time(1:end),...
        meanPosRob(1:end),'b');
    grid on;
    legend('Mean position disc', 'Mean position robot','Location',...
        'northoutside','Orientation','horizontal');
    xlabel('Time [ms]')
    ylabel('Position [mm]')
    filename = ['Plot/meanPosition',name,'.eps'];
    saveas(figure2, filename);
    filenamejpg = ['Plot/meanPositionJPG',name,'.jpg'];
    saveas(figure2, filenamejpg);
end

%% Plot velocity in XYZ
if plotVelocity == 1
    N = 1; 
    wts = 1/N*ones(N,1);
    translatedNewXYZCord = newXYZcord';
    translatedRobotXYZ = robotXYZ';
    
    posX = translatedNewXYZCord(1,:);
    posRX = translatedRobotXYZ(1,:);
    
    posY = translatedNewXYZCord(2,:);
    posRY = translatedRobotXYZ(2,:);
    
    posZ = translatedNewXYZCord(3,:);
    posRZ = translatedRobotXYZ(3,:);
    
    % Derivate position to get velocity
    velX = diff(posX, N);
    velRX = diff(posRX, N);
    velY = diff(posY, N);
    velRY = diff(posRY, N);
    velZ = diff(posZ, N);
    velRZ = diff(posRZ, N);
  
    figure3 = figure;
    plot(time(1:end-N), velX(1:end),'r', time(1:end-N),...
        velRX(1:end), 'b'); hold on;
    plot(time(1:end-N), velY(1:end),'k', time(1:end-N),...
        velRY(1:end), 'g'); hold on;
    plot(time(1:end-N), velZ(1:end),'m', time(1:end-N),...
        velRZ(1:end), 'c'); 
    legend('Disc X velocity','Robot X velocity','Disc Y velocity',...
        'Robot Y velocity','Disc Z velocity','Robot Z velocity',...
        'Location','eastoutside')
    grid on;
    filename = ['Plot/velocityXYZ',name,'.eps'];
    saveas(figure3, filename);
    filenamejpg = ['Plot/velocityXYZJPG',name,'.jpg'];
    saveas(figure3, filenamejpg);
end


    %% Plot mean velocity
    if plotMeanVelocity == 1
        transMeanPos = meanPosDisc';
        transMeanRob = meanPosRob';
      
        meanVelDisc = diff(meanPosDisc, N);
        meanVelRob = diff(meanPosRob, N);
        
        figure4 = figure;
        plot(time(1:end-N), meanVelDisc(1:end),'r', time(1:end-N),...
            meanVelRob(1:end),'b'); 
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
        plot(time(1:end-2), acelX(1:end),'r', time(1:end-2),...
            acelRX(1:end),'b'); hold on;
        plot(time(1:end-2), acelY(1:end),'k', time(1:end-2),...
            acelRY(1:end),'g'); hold on;
        plot(time(1:end-2), acelZ(1:end),'m' ,time(1:end-2),...
            acelRZ(1:end),'c'); 
        legend('Disc X acceleration','Robot X acceleration',...
            'Disc Y acceleration', 'Robot Y acceleration',...
            'Disc Z acceleration','Robot Z acceleration',...
            'Location','eastoutside');
        grid on; 
        filename = ['Plot/accelerationXYZ',name,'.eps'];
        saveas(figure5, filename);
        filenamejpg = ['Plot/accelerationXYZJPG',name,'.jpg'];
        saveas(figure5, filenamejpg);
    end
    
    %% Plot mean acceleration
    if plotMeanAcceleration == 1
        meanAccelDisc = diff(meanVelDisc);
        meanAccelRob = diff(meanVelRob);
        
        figure6 = figure;
        plot(time(1:end-2), meanAccelDisc(1:end),'r',time(1:end-2),...
            meanAccelRob(1:end),'b');
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


