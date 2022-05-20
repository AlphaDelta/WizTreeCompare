
# WizTreeCompare
Compare two WizTree CSV snapshots into a differential CSV that can be loaded back into WizTree

# Requirements
 [.NET Desktop Runtime >=6.0.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

# Usage

## GUI
**Step 1.** Run WizTreeCompare.exe (with no arguments)

**Step 2.** Select a 'Past CSV' and 'Future CSV', where the Past CSV *must* be older than the Future CSV

![WizTreeCompare with the Past CSV and Future CSV files selected](https://i.imgur.com/9XHdmHR.png)

**Step 3.** Click the `Compare...` button, name your output CSV, then wait. You will know when the process is complete when this message box appears:

![Success dialog](https://i.imgur.com/XSQ1CBm.png)

**Step 4.** Load the CSV output into WizTree, enjoy

![WizTree with the differential log loaded](https://i.imgur.com/AIwjwMU.png)

## CLI
**Usage:**
```
WizTreeCompare <past csv> <future csv> <output csv>
```
**Example:**
```
WizTreeCompare "C:\past.csv" "C:\future.csv" "C:\output.csv"
```