# WMA/JPG to MP3 Converter

**WMA/JPG to MP3 Converter** is a simple tool that can take on a music library that consists of *Windows Media Audio* (**.WMA** - See: https://en.wikipedia.org/wiki/Windows_Media_Audio) files and converts them into *MPEG-1 or MPEG-2 Audio Layer III* (**.MP3** - See: https://en.wikipedia.org/wiki/MP3)

What makes this tool stand out from other converters is the fact that it will **scan the folders in which the .WMA-files reside for the largest available .JPG-image and embeds it into the .MP3 as the album art.**

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

The media player in my car is capable of displaying the album art of a given track it is playing as long as it is embedded into the file itself. Unfortunately, the .WMA file format does not support this and my music collection consists of countless .WMA-files that were 'ripped' from CD's that I have purchased throughout the years. This tool exactly fits my niche.

## Credits

#### NAudio:

https://github.com/naudio/NAudio

#### NAudio.Lame:

https://github.com/Corey-M/NAudio.Lame

#### NAudio.Wma:

https://github.com/naudio/NAudio.Wma

## License

This project uses the MIT license. See 'license.txt' for more info.
