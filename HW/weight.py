import RPi.GPIO as GPIO
import sys
from hx711 import HX711

dt = int(sys.argv[1])
scl = int(sys.argv[2])

GPIO.setwarnings(False)

hx = HX711(dt, scl)
hx.set_reading_format("LSB", "MSB")
hx.set_reference_unit(90)
hx.reset()
hx.tare()

print(hx.get_weight(3))
