from grovepi import *

dht_1 = 7
dht_2 = 8
dht_type = 0

try:
    temp1, hum1 = dht(dht_1, dht_type)
    temp2, hum2 = dht(dht_2, dht_type)

    print('%.2f|%.2f' % (temp1, hum1))
    print('%.2f|%.2f' % (temp2, hum2))
except:
    pass

