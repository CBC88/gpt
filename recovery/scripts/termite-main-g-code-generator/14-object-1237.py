import re
import System
from System.Windows.Forms import MessageBox
from System.Net import WebClient

# Inputs from Grasshopper:
# trigger : bool (from button)
# currentversion : str in yymmdd format

a = None  # message for version status
fetched_version = None  # new output: date fetched from website

def parse_date_yymmdd(date_str):
    """Convert yymmdd string to tuple (yy, mm, dd) of ints for comparison."""
    if not date_str or len(date_str) != 6:
        return None
    try:
        yy = int(date_str[0:2])
        mm = int(date_str[2:4])
        dd = int(date_str[4:6])
        return (yy, mm, dd)
    except:
        return None

if bool(trigger):
    try:
        # Fetch webpage
        url = "https://iam.tugraz.at/research/shapelab/termite/"
        client = WebClient()
        client.Encoding = System.Text.Encoding.UTF8
        html = client.DownloadString(url)

        # Remove HTML tags to simplify search
        text_only = re.sub(r'<.*?>', ' ', html)

        # Search specifically for "Build " followed by 6-digit date
        date_match = re.search(r'Build\s+(\d{6})', text_only)
        if date_match:
            newversion_str = date_match.group(1)
            fetched_version = newversion_str  # set output
            parsed_new = parse_date_yymmdd(newversion_str)
            parsed_current = parse_date_yymmdd(currentversion)

            if parsed_current and parsed_new:
                if parsed_new > parsed_current:
                    msg = ("Update available (Build {0}).\n\n"
                           "Visit www.food4rhino.com/en/app/termite to download the latest version of Termite.").format(newversion_str)
                else:
                    msg = "You are using the latest Version (Build {0}) of Termite.".format(currentversion)
            else:
                msg = "Could not parse current or new version dates."
        else:
            msg = "No 'Build yymmdd' date found on the webpage."

    except Exception as e:
                msg = ("Termite was not able to check for updates.\n\n"
               "Check your internet connection or check manually on www.food4rhino.com/en/app/termite.")

    # Show message box
    MessageBox.Show(msg, "Termite Check For Updates")

    # Output
    a = msg
