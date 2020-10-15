# avr-identify
A tool that will help you identify what code is running on your Arduino


## ArchiveTool
Put platform.local.txt next to platform.txt (in /hardware/arduino/avr) 
and put archive_metadata.cmd in /tools/archive.

On each build, it will create a metadata file with the name of the project, the contents of the sketch folder, the git remote url and the latest commit.
This, and the hex file that is uploaded to the device, is put in the archive folder.

Later, the avr-identify tool can be used to find what source code was used for the code that is running on a device.

## To identify a device
To identify a device, you start by getting the binary from it. On a normal Arduino you will probably use avrdude.
For an Arduino Uno the command is something like:

```arduino-ide\arduino-1.8.12\hardware\tools\avr/bin/avrdude -CC:\Toys\arduino-ide\arduino-1.8.12\hardware\tools\avr/etc/avrdude.conf -v -patmega328p -carduino -PCOM6 -b115200 -D -Uflash:r:unknown.hex:i```

Then you compare the file unknown.hex to the files in the archive. The structure of a hex file can be different for the same data, and the file you just read will be bigger than the original (the size of the flash), that's where avr-identify comes in.

```
avr-identify <file> <archive folder>"
```

This command will tell you which of the files in the archive folder that is the same binary as <file>.
If you have been using ArchiveTool, you will also have the metadata file helping you to identify the build and latest commit behind that binary.
Hopefully, all you have to do is to check your version control and you will be right on track.
  
