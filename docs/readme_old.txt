ZXMAK.NET Version 1.0.8.4 [28.04.2008]
.NET Crossplatform ZX Spectrum Emulator

====================Short help====================

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


To run emulator you should have the following libraries:
- .NET Framework 2.0 Runtime (Download it from Microsoft, not required for Windows Vista)
- fresh DirectX 9.0c		 (Download it from Microsoft)

Recommended PC:
CPU: 1.7-2 GHz or above
Video: 3D accelerated videocard
Audio: DirectX compatible soundcard


Emulator tested by ZEXALL z80 cpu test, it is 100% precise :)


Note: you can access to menu in fullscreen mode, 
it is possible by simply pressing <Alt>+<Ctrl> 
(to release capturing mouse) and then move mouse 
cursor to top of the screen. :)



Short History:
1.0.6.6 - multitherading, but unstable
1.0.6.7 - singletherading
			+ fix&refactor Tape,TapeLoader
			+ namespace&folders refacor
			+ full TapeForm
			+ full support TZX
			- tape sound
			+ Alt+Ctrl to release mouse
			+ disable kbdinput while Alt pressed
			+ fix hobeta load
			+ SNA loader check & other loaders messages (no exception)
			+ zip namespace = ZipLib
			+ debugger as classic singleton
			+ F2 - slow/F4 - fast
...
1.0.8.0 - strong refactoring
			+ crossplatform source infrastructure
			+ added simple logging & fatal error report window :)
			+ fullscreen without change resolution
			+ fix for StepOver operation in embedded debugger
			- some features (fps show, synchronous frame resampling, control speed) 
			  temporary removed (not implemented because too many source code rewriten)
1.0.8.1 - multiplatforms
			+ SDL platform added (thanks to Andrew Kurushin for porting) 
			+ XNA platforms added
			+ split for 3 exe: MDX, SDL and XNA platforms
1.0.8.2 - small fix
			+ MDX: target platform x86 (because MDX not supported by x64)
			+ disable texture filtering
1.0.8.3 - small fix
			+ fix port #fe unused bits
			+ fix mouse bug (artmouse.scl [disked and modified by Cobra])
			+ fix mouse sensitivity (for MDX,XNA,SDL)
			+ fix for SDL: if sound not available then run without sound (test for linux)
1.0.8.4: 
			+ CSW v2 tape loader added (Z-RLE compression)
			+ fix tape sound
			+ fix logger finalization
			+ SDL: scaling and fullscreen added
			+ engine load/save and sound refactoring
			+ fix mouse emulation
			+ TD0 disk deserializer added
			+ MDX: quick snapshot load added (F12)
			+ MDX: drag'n'drop fixed
			+ MDX: improved video synchronization for better smooth multicolor effect and scrolls
			  
Current TODO:
- Release complete DirectX platform
	- fps show
	- control speed
	- add FDI,TD0 serialization
	- implement Profi1024 clone
- Release OpenGL platform?

Please send bugreports to my email: zxmak@mail.ru

--------
Authors:

Engine - Alexander Makeev (c)2001-2008 :)
MDX, XNA platforms - Alexander Makeev zxmak@mail.ru
SDL platform - Andrew Kurushin 

