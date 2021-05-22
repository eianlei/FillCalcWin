# FillCalcWin
FillCalcWin is a Windows GUI tool for interactive technical scuba diving gas blending calculations.
![mainwin-shorturl](https://github.com/eianlei/FillCalcWin/blob/d4ede75a8f5155c7f0772d7a537b251c61158dc8/FillCalcWin_MainWindow.jpg?raw=true)

FillCalcWin is a rewrite of FillCalc2.py app from original Python/Qt5 implementation to C#/WPF.

It is a rewrite from:
https://github.com/eianlei/pydplan/blob/master/FillCalc2.py

See also the doc about the Python version:
**https://github.com/eianlei/pydplan/blob/master/doc/fillcalc2.md**

# highlights
- The Python code has been converted to C#.
- The GUI framework has been changed from Qt5 to WPF (Windows Presentation Foundation)  
  - MVVM pattern is used 
  - the GUI part is coded in XAML
  - code behind is C#
- precompiled FillCalcWin installer is available at GitHub
https://github.com/eianlei/FillCalcWin/releases
- FillCalcWin contains embedded Help file, but you can also read it from https://github.com/eianlei/FillCalcWin/blob/master/FillCalcWin/Resources/fillcalc2.rtf

# supported platforms
- FillCalcWin runs on all Windows platforms from Win 7 SP2 and newer
- Unlike the Python version FillCalc2.py, FillCalcWin will not run on MAC or Linux
- But FillCalcWin is really easy to install and use on Windows, because a precompiled binary and installed can be made easily


