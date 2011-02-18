#!/usr/env python

# upload_to_minus.py - Nautilus Extension for uploading image or 
# batch of images to http://min.us.
# Copyright (C) 2010  Dejan Noveski <dr.mote@gmail.com>
#
# Permission is hereby granted, free of charge, to any person
# obtaining a copy of this software and associated documentation files
# (the "Software"), to deal in the Software without restriction,
# including without limitation the rights to use, copy, modify, merge,
# publish, distribute, sublicense, and/or sell copies of the Software,
# and to permit persons to whom the Software is furnished to do so,
# subject to the following conditions:
#
# The above copyright notice and this permission notice shall be
# included in all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
# EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
# MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
# NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
# BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
# ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
# CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.

"""
    Check README.rst
"""

import gconf
import nautilus
import urllib, urllib2
import mimetools, mimetypes
import os, stat
from StringIO import StringIO
from exceptions import ImportError
import simplejson as json
from minus_utils import *

# Any other mimetypes i forgot?
SUPPORTED_FORMATS = ('image/jpeg', 
                    'image/png', 
                    'image/gif',
                    'image/bmp',)

# Min.us urls
MINUS_URL = "http://min.us/"
API_URL = MINUS_URL + "api/"
GALLERY_URL = API_URL + "CreateGallery"
UPLOAD_URL = API_URL + "UploadItem"

class MinusUploaderExtension(nautilus.MenuProvider):

    """
        Minus Uploader provider class - Adds an item in the contex menu in
        nautilus for image mimetypes. 
    """
    def __init__(self):
        self.gconf = gconf.client_get_default()

    def menu_activate(self, menu, files):
        """ Callback for menu item activate """
        return self.upload_gallery(files)

    def get_file_items(self, window, files):
        """ Shows the menu item """
        if len(files) == 0:
            return

        for f in files:
            if not f.get_mime_type() in SUPPORTED_FORMATS:
                return 
            if f.get_uri_scheme() != 'file':
                return

        item = nautilus.MenuItem('Nautilus::upload_to_min_us',
                                 'Upload to min.us',
                                 'Upload to min.us',
                                 'up')
        # connect to callback
        item.connect('activate', self.menu_activate, files)
        return item,

    def upload_gallery(self, files):
        """ Uploads selected images to imgur """
        # create a gallery - minus works like that
        gallery = self.create_gallery()
        if gallery:
            editor_id = gallery["editor_id"]
            reader_id = gallery["reader_id"]
            
            for f in files:
                # use the created gallery and add the images there
                self.upload_image(f.get_uri(), editor_id, reader_id)
        
            try:
                # Open the default browser and show the gallery
                import webbrowser
                webbrowser.open(MINUS_URL + "m" + editor_id)
            except ImportError, e:
                # No browser? Show the url in notification.
                notify(MINUS_URL + "m" + editor_id)

    def upload_image(self, image, editor_id, reader_id):

        try:
            image_path = path_from_uri(image) 
            params = {
                    "file": open(image_path, "rb")}
            notify("Uploading %s to min.us"%os.path.basename(image_path))
            urlopener = urllib2.build_opener(MultipartPostHandler)
            response = urlopener.open(
                    "%s?key=%s&editor_id=%s&filename=%s" % 
                    (UPLOAD_URL, "nautilus-uploader-"+editor_id, 
                        editor_id, os.path.basename(image_path),),
                    params)
        except URLError, e:
            notify(e.reason)

    def create_gallery(self):
        request = urllib2.Request(GALLERY_URL)
        try:
            response = urllib2.urlopen(request)
        except urllib2.URLError, e:
            notify(e.reason)
            return None
        
        return json.loads(response.read()) 

