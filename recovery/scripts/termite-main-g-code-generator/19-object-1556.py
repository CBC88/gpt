# x: list of strings representing floats
# a: output list of numbers

# Pre-allocate list for speed
count = len(x)
result = [0.0] * count

# Fast loop conversion (avoids Python overhead of list comprehension)
for i in range(count):
    result[i] = float(x[i])

a = result
