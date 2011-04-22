#/usr/bin/env python

import sys, subprocess, urllib, urllib2, os
from time import strftime
import json
import unicodedata

''' 
	REPLACE FOLLOWING WITH VALID EDITOR AND VIEWER URLS 
'''
MINUS_URL = 'http://min.us/'
# MINUS_URL = 'http://192.168.0.190:8001/'
READER_ID = "veTYOJ"
EDITOR_ID = "cXLjZ5CjJr5J"


def upload_file(filename, editor_id, key):
	basename = os.path.basename(filename)
	url ='%sapi/UploadItem?editor_id=%s&key=%s&filename=%s' % (MINUS_URL, editor_id, key, basename)
	file_arg = 'file=@%s' % filename
	p = subprocess.Popen(['curl', '-s', '-S', '-F', file_arg, url], stdout=subprocess.PIPE, stderr=subprocess.PIPE)
	output,error = p.communicate()
	if p.returncode == 0:
		dd = parseDict(output)
		print 'Uploaded %s (%s)' % (basename, dd['filesize'])
	else:
		print 'Failed %s : %s' % (basename, error)

def parseDict(dictStr):
	dictStr = dictStr.translate(None,'\"{}')
	items = [s for s in dictStr.split(', ') if s]
	dd = {}
	for item in items:
		key,value = item.split(': ')
		dd[key] = value

	return dd	

def create_gallery():
	f = urllib2.urlopen('%sapi/CreateGallery' % MINUS_URL)
	result = f.read()
	dd = parseDict(result)

	return (dd['editor_id'], "", dd['reader_id'])

def saveGallery(name, editor_id, items):
	name = "test"
	params = { "name" : name, "id" : editor_id, "key" : "OK", "items" : json.dumps(items) }
	params = urllib.urlencode(params)
	try:
		f = urllib2.urlopen('%sapi/SaveGallery' % MINUS_URL, params)
	except urllib2.HTTPError, e:
		print "\n", e.code
		print "SaveGallery Failed:\n", "params: ", params

def generateImageList(reader_id):

	formattedList = []
	f = urllib2.urlopen('%sapi/GetItems/m%s' % (MINUS_URL, reader_id))
	jsonData = json.loads( f.read())
	imagesList = jsonData[u'ITEMS_GALLERY']
	for image in imagesList:
		image = unicodedata.normalize('NFKD', image).encode('ascii','ignore')
		image = image[17:]
		image = image.split(".")[0]
		formattedList.append(image)
	return formattedList
		

def main():
	#editor_id,key,reader_id = create_gallery()
	#args = sys.argv[1:]
	#for arg in args:
	#	upload_file(arg,editor_id,key)
	imageList = generateImageList(READER_ID)	
	saveGallery("Testing rename - %s" % strftime("%Y-%m-%d %H:%M"), EDITOR_ID, imageList)
	print 'Editor URL: http://min.us/m%s' % EDITOR_ID 
	print 'Viewer URL: http://min.us/m%s' % READER_ID

if __name__ == '__main__':
	main()