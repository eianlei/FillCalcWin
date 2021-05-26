# FillCalcWin
FillCalcWin is a Windows desktop GUI tool for interactive technical scuba diving gas blending calculations.
![mainwin-shorturl](https://github.com/eianlei/FillCalcWin/blob/d4ede75a8f5155c7f0772d7a537b251c61158dc8/FillCalcWin_MainWindow.jpg?raw=true)

# download and install to Windows
Go to the release page to download the latest release.

https://github.com/eianlei/FillCalcWin/releases

# Features
- Free and open source
- Calculates Oxygen, Helium (and Nitrogen) gas blending instructions for 5 different fill methods
- Calculation can be done from a scuba tank containing any mix of O2/He to any wanted mix of O2/He
- Supported fill methods are:
  - fill just air
  - fill Nitrox using CFM
  - fill Trimix using CFM
  - partial pressure fill (He + O2 + air)     
  - fill Helium then top with Nitrox (CFM)
 - Calculate the cost of your fill
 - Copy results to clipboard or save to file
 - Calculates with ideal gas or Van der Waals gas laws

# Supported platforms
- FillCalcWin runs on all Windows platforms from Windows 7 SP2 and newer
- There is also a Python version FillCalc2.py, that will run on any platform such as MAC or Linux
- But FillCalcWin is really easy to install and use on Windows, because a precompiled binary and installer can be made easily. So almost anyone can install and use it. 
- The Python version is somewhat hard to install if you are not familiar with all the dev tools.
- Web and Mobile (Android, IOS) versions might be coming in future...

# Background
FillCalcWin is a rewrite of FillCalc2.py app from original Python/Qt5 implementation to C#/WPF.

It is a rewrite from:
https://github.com/eianlei/pydplan/blob/master/FillCalc2.py

See also the doc about the Python version:
**https://github.com/eianlei/pydplan/blob/master/doc/fillcalc2.md**

# Technical highlights
- The Python code has been converted to C#.
- The GUI framework has been changed from Qt5 to WPF (Windows Presentation Foundation)  
  - MVVM pattern is used 
  - the GUI part is coded in XAML
  - code behind is C#
  - .NET 4.8
- precompiled FillCalcWin installer is available at GitHub
https://github.com/eianlei/FillCalcWin/releases
- FillCalcWin contains embedded Help file, but you can also get PDF version from here: https://github.com/eianlei/FillCalcWin/blob/master/FillCalcWin/Resources/fillcalc2.pdf
- The code has been developed with Visual Studio 2019
  - you can clone the github repo to your VS with just few clicks



