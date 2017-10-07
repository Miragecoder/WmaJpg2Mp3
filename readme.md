# WMA/JPG to MP3 Converter

[![Build status](https://ci.appveyor.com/api/projects/status/8gfr42e1k1ypv3nx/branch/master?svg=true)](https://ci.appveyor.com/project/Miragecoder/wmajpg2mp3/branch/master)

**WMA/JPG to MP3 Converter** is a simple tool that can take on a music library that consists of *Windows Media Audio* (.WMA) files and converts them into *MPEG-1 or MPEG-2 Audio Layer III* (.MP3), whilst embedding .JPG image files as album art.

You may want to use this if you are operating a media device that is capable of displaying album art for .MP3-files, but not for .WMA-files; which happens to be the case with some modern car radios.

![A breakdown of the problem and its solution](/images/collage.png?raw=true "A breakdown of the problem and its solution")

## Installation

1. Clone the repository
2. Compile the **WmaJpg2Mp3**-project
3. Run the **WmaJpg2Mp3**-project

## Usage

1. Run the **WmaJpg2Mp3**-executable;
2. Set the **'Source folder'** to the root of your .WMA-file based media collection; 
3. Set the **'Destination folder'** to the root of the path where the converted .MP3-files will be copied to.

## Contributing

Feel free to chime in.

## History

The media player in my car is capable of displaying the album art of a given track it is playing as long as it is embedded into the file itself. Unfortunately, the .WMA file format does not support this and my music collection consists of countless .WMA-files that were 'ripped' from CD's that I have purchased throughout the years. Ripping hundreds of CD's is a very tedious process and I wasn't looking forward to repeating the cycle for a different file format; hence this tool.

## Credits

#### NAudio:

https://github.com/naudio/NAudio

#### NAudio.Lame:

https://github.com/Corey-M/NAudio.Lame

#### NAudio.Wma:

https://github.com/naudio/NAudio.Wma

## License

This project uses the MIT license. See 'license.txt' for more info.
