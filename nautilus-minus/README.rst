ABOUT
#####

upload_to_minus.py:  part of nautilus-minus package - Upload image or more
images at once to http://min.us. Each batch upload makes it's own gallery. Only
images supported. Adds context menu item that allows uploading. It uses pynotify
- libnotify if availible, and if not, tries to inform the user of possible errors,
updates, with zenity. Try to have at least 1 of these packages installed. Ubuntu
and Ubuntu forks have libnotify by default.

INSTALLATION
############

1. Use the deb package. Should be in github download section. 

        dpkg -i package_name.deb

    Remember to restart nautilus:

        nautilus -q or
        killall nautilus

    This installs all dependencies, so you don't have to worry about anything else.

2. Copy all files from the package into $HOME/.nautilus/python-extensions/

    Install python-nautilus package (http://projects.gnome.org/nautilus-python/):
    
        sudo apt-get install python-nautilus - for Ubuntu/Ubuntu forks

    Restart nautilus:

        nautilus -q


USAGE
#####

Right click on an image or selection of images -> Upload to min.us.
Wait for it...

