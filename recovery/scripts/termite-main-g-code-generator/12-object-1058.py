import os

# Simulate static variables in the Grasshopper Python component
if 'wasTriggered' not in globals():
    wasTriggered = False
if 'resultReported' not in globals():
    resultReported = False
if 'writeSuccess' not in globals():
    writeSuccess = False

# When the trigger is activated
if trigger and not wasTriggered:
    wasTriggered = True
    resultReported = False
    writeSuccess = False

    try:
        # Use fixed folder: user's Desktop
        desktop_path = os.path.expanduser("~/Desktop")

        # Ensure fileName is valid
        if not fileName or not isinstance(fileName, str):
            raise ValueError("Invalid fileName input.")

        # Ensure fileContent is a list of strings
        if not isinstance(fileContent, list):
            raise ValueError("fileContent must be a list of strings.")

        # Build the full path
        full_path = os.path.join(desktop_path, fileName)
        print(" Writing to file:", full_path)

        with open(full_path, 'w') as f:
            f.write('\n'.join(map(str, fileContent)))  # Safely convert any non-strings

        print("✅ File successfully written.")
        writeSuccess = True

    except Exception as e:
        print("❗ Exception occurred:", str(e))
        writeSuccess = False

# Reset trigger state when released
if not trigger:
    wasTriggered = False

# Output A is True for one iteration after a successful write
if writeSuccess and not resultReported:
    A = True
    resultReported = True
else:
    A = False
