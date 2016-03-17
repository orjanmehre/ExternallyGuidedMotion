% 
%  
% k = 1;
% i = 1;
%    justforfun=meanPosDisc(1:56:length(meanPosDisc));
%    jff = meanPosRob(1:56:length(meanPosRob));
%    time2 = time(1:56: length(time));
% 
% translatedJustforfun = justforfun';
% translatedjff = jff';
% posX1 = translatedJustforfun(1,:);
% robX1 = translatedjff(1,:);
% 
% velX1 = diff(posX1);
% velRX = diff(robX1);
% 
% figure;
% plot(time2(1:end-1),velX1, time2(1:end-1), velRX);

% k = 1; 
% jump = 0;
% for i = 2: 1 : length(meanPosDisc)
%     if meanPosDisc(i-1) ~= meanPosDisc(i)
%         tempmeanPosDisc(k) = meanPosDisc(i);
%         k = k+1;
%     else
%         jump = jump +1;
%     end
% end

