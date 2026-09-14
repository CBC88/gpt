# Input: x (string or list of strings)
# Output: a (converted string or list)

def convert_text(s):
    if not isinstance(s, str):
        return s

    replacements = {
        u"Ä": "AE",
        u"Ö": "OE",
        u"Ü": "UE",
        u"ä": "ae",
        u"ö": "oe",
        u"ü": "ue",
        u"ß": "ss"
    }

    for k, v in replacements.items():
        s = s.replace(k, v)

    return s

# Handle single string or list
if isinstance(x, list):
    a = [convert_text(item) for item in x]
else:
    a = convert_text(x)
