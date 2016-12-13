import grovepi
import time
import sys

relay = int(sys.argv[1])

grovepi.pinMode(relay, 'OUTPUT')
grovepi.digitalWrite(relay, 0)
