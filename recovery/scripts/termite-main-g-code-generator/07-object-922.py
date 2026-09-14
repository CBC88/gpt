import Rhino
import System.Windows.Forms as Forms

# Inputs
# trigger: A boolean input to trigger the logic
# condition: A boolean input to define a condition
# text: A warning text to appear when triggered

# Output
# answer: A boolean output, defaults to False

if not 'answer' in globals():
    answer = False  # Initialize answer if not already defined

if trigger:
    if condition:
        # If trigger and condition are true, set answer to True
        answer = True
    else:
        # If trigger is true and condition is false, pop up a message box
        result = Forms.MessageBox.Show(
            text,
            "Termite Warning",
            Forms.MessageBoxButtons.YesNo,
            Forms.MessageBoxIcon.Question
        )
        
        if result == Forms.DialogResult.Yes:
            answer = True
        else:
            answer = False
else:
    # Default state when trigger is False
    answer = False
