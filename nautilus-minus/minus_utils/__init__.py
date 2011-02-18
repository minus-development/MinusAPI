# multipart is an LGPL library. I didn't want to clutter the code here
# so I put it in an outside script. File uploads with python's stdlib
# sucks big time. This lib defines a MultiPart handler.
from multipart import *

def notify(message):
    """
        Notify On Screen for possible errors and actions.
        Tries to use libnotify, and falls back to zenity.
    """
    try:
        import pynotify
        pynotify.Notification(message).show()
        return
    except ImportError, e:
        pass
    os.popen("zenity --info --text='%s'"%message)

def path_from_uri(file_uri):
    """ 
        Nautilus returns 'file://[path]' uri format.
        We don't need the file:// handle.
    """
    return file_uri[7:]

