
invoke -entry convey_new

task -slotname udpuc -slotid 138 -pri 100 -vwopt 0x1c -stcks 20000 \
-entp udpuc_main -auto -noreg

task -slotname gsictrlts -slotid 119 -pri 3 -vwopt 0x1c -stcks 8000 \
-entp gsictrlts_main -auto

