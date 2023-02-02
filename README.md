# MPD client Balboa

## Description

        Balboa is a Windows client for MPD (Music Player Daemon http://www.musicpd.org", designed for use preferably on a tablets.
        Balboa provides comfortable playng control options, playlist management, display status
        and statistic information, searching tracks in music collection. While playing tracks, Balboa
        showes album art images from music collection folders. 
        It is only one MPD client for Windows RT 8.1.

## Instalation
		Download file Balboa_4.4.0.20_AnyCPU.appxbundle and install it.
    
## Privacy Policy
### Settings
            
        All Balboa setting parameters are stored locally. The app does not collect any information
        and do not share, sale or store any private information like your position, played track 
        and so on.
            
### Internet connection
        
        Balboa does not use internet connection.

### Local area network
        
        Your local area network connection is only used to communicate with MPD server specified
        by you and download album arts from music collection folder to the computer running Balboa.
        To display album arts Balboa need access to folder with your music collection.
        You may provide this access by setting parameter ‘Path to music collection’ on ‘Settings’ page.
            

      
## Balboa Configuration 
        The Balboa configuration is performed on page "Settings".  
            
### Mandatory parameters 
- Server - name or ip-address of the server on which the MPD is running (if Balboa runs
        on the same computer as the MPD then you can specify localhost as the server name or
        address 127.0.0.1)
        - Port - by Default value is equal to 6600 (the same as the default value MPD).
        - View update interval - value is measured in milliseconds and cannot be less than 100.
### Not mandatory settings (used to display album arts)
 - Path to music collection - 

    To display album arts, it is necessary to provide Balboa access to album arts image files
    and specify a place where to search for album art files. 
    Thus, to display album artwork, you need to provide shared access over the network
    to a directory with music files and album art on the computer that is running MPD (for example,
    using  Samba ).
    At Balboa Settings page set parameter "Path to music collection" so that it points to the
    directory with music files and album covers images.	
    Parameter "Path to music collection" must point to the same physical location of files as
    the parameter music_directory in the file mpd.conf.
            
#### For example
        MPD runs on a server that is running Ubuntu 14.04. Server name is SAFAGA 
        Balboa runs on a tablet running Windows RT 8.1.
        Music files with album art on the SAFAGA are located in /mnt/storage/audio/collection
        At mpd.conf file on server
        music_directory "/mnt/storage/audio/collection"
        Using SAMBA the /mnt/storage/audio/collection directory is shared under the name 
        "audiocollection".
        So, to view the contents of the directory /mnt/storage/audio/collection on tablet 
        with Windows, it is necessary at Windows Explorer address bar input address \\SAFAGA\audiocollection.
        This address must be entered in the parameter "Path to music collection" on the Balboa 
        "Settings" page.
            
        Album cover file name - list of file names which Balboa will conside as for album art images.
        The names are separated by the symbol ";". Balboa will search for image files in the order in which they are listed.
                
        Display pictures for folders - if this the parameter is enabled, then on the File system tab,
        in the list of directories, the album covers will be displayed before the catalog name. 
            
## Known issues and solutions
            
### Album art does not displayed
                   
    Parameter Balboa "Path to music collection" and parameter MPD "music _ directory" points to the different elements in the directory tree on server that is running MPD. 
                   
    Change parameter "Path to music collection".
                        
## History of changes
        
### April 2017 
        First version released.
### July 2018
        Code has been refactored, and error handling has been improved. On the "File system" page,
        the display of album art is added before the directory names.