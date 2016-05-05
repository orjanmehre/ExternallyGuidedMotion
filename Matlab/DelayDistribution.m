%%
%{
This script is used to plot the distribution of the time delays.
A .txt file with the time delays is used as input and a histogram
is the output.
%}

%%
clc;
clear all; 
name = 'ET2v4000';
fileId = fopen('executionTime.txt');

C = textscan(fileId, '%s');
fclose(fileId);

% Extract the time data

for i = 1: 1: size(C{1,1},1)
    tempTime = C{1,1}{i,1}; 
    time(i) = str2double(strrep(tempTime, ',' , '.'));
end

Maximum = max(time)
Minimum = min(time)

[N,X] = hist(time,unique(time));
figure1 = figure;
bar(X,N, 1)
xlim([Minimum-1 Maximum+1]);
xlabel('Delay[ms]')
ylabel('Number of occurences')
title(['Time delay, number of samples: ' num2str(size(time,2))])

filename = ['Plot/histogramDelay',name,'.eps'];
saveas(figure1, filename);

filenamejpg = ['Plot/histogramDelayJPG',name,'.jpg'];
saveas(figure1, filenamejpg);


% Storing the data in a cell array
C{1,2}(1,:) = N; % Occurences
C{1,2}(2,:) = X; % Delay


