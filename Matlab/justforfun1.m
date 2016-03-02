
 
k = 1;
i = 1;
   justforfun=meanPosDisc(1:28:length(meanPosDisc));
   time2 = time(1:28: length(time));

translatedJustforfun = justforfun';
posX1 = translatedJustforfun(1,:);

velX1 = diff(posX1);

figure;
plot(time2(1:end-1),velX1);