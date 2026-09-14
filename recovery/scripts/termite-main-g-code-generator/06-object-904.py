from datetime import datetime

# Initialize output values
year = None
month = None
day = None
hour = None
minute = None
second = None

# Check if the button is pressed
if trigger:
    now = datetime.now()

    # Zero-padded string outputs
    year   = "{:04d}".format(now.year)
    month  = "{:02d}".format(now.month)
    day    = "{:02d}".format(now.day)
    hour   = "{:02d}".format(now.hour)
    minute = "{:02d}".format(now.minute)
    second = "{:02d}".format(now.second)

# Assign to outputs
a = year
b = month
c = day
d = hour
e = minute
f = second
