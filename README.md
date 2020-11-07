# ZXMAK.NET.Legacy
.NET Cross-platform ZX Spectrum Emulator. Based on ZXMAK.NET Version 1.0.8.4 [28.04.2008] by Alexander Makeev

## Short help

The most important keyboard shortcuts: 
<Alt>+<Enter> = switch between windowed/fullscreen mode 
<Alt>+<Ctrl>  = release captured mouse (click in window to start mouse capturing) 

<Left Shift>  = <Caps Shift> 
<Right Shift> = <Symbol Shift> 
<F3>          = reset spectrum 
<F7>          = rewind tape 
<F8>	      = play/pause tape 
<F5>          = stop spectrum 
<F9>          =	start spectrum 

Command line options: 

<filename>	open spectrum image from file[s] 
/MODEL:		select spectrum clone (short /M:) 
/AA			enable videofilter 
/AA-		disable videofilter 
/F			fullscreen mode 
/D			start without run 
/W			start with write protected disk drives 
--HELP		video mode help (SDL specific, short /? or -H) 
/VM			select video mode (SDL specific) 

Tested PC: 
CPU: 1.7-2 GHz or above 
Video: 3D accelerated videocard 
Audio: DirectX compatible soundcard 

Emulator tested by ZEXALL z80 cpu test, it is 100% precise :) 

Note: you can access to menu in fullscreen mode, 
it is possible by simply pressing <Alt>+<Ctrl> 
(to release capturing mouse) and then move mouse 
cursor to top of the screen. :) 

Current TODO: 
- Release complete DirectX platform 
	- fps show 
	- control speed 
	- add FDI,TD0 serialization 
	- implement Profi1024 clone 
- Release OpenGL platform? 

## Original Authors:

Engine - Alexander Makeev (c)2001-2008 :) 
MDX, XNA platforms - Alexander Makeev zxmak@mail.ru 
SDL platform - Andrew Kurushin 
