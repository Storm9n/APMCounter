# APMCounter

## Use
APMCounter is a small WPF overlay that displays your current **Actions Per Minute (APM)**.

### What counts as an action
- Every keyboard **key down** event
- Mouse button presses: **left**, **right**, **middle**, and **X1/X2** (mouse buttons 4/5)

### How APM is calculated
- The number shown is a rolling total over the last **60 seconds**.

### How to use it
1. Run the application.
2. A small borderless window appears showing a single number (your current APM).
3. Click and drag the window (left mouse button) to reposition it.
4. Close it with **Alt+F4**, via the taskbar window menu, or by ending the process in Task Manager.

## Install

1. Download executable from releases
2. Run executable

## Build

### Requirements
- Windows
- .NET Framework
- Visual Studio with the ".NET desktop development" workload

### Steps
1. Open the solution in Visual Studio.
2. Restore NuGet packages (if prompted).
3. Build the solution.
4. Run the generated executable from the build output.
