import sys, subprocess, urllib2, os

def upload_file(filename, editor_id, key):
	basename = os.path.basename(filename)
	url='http://min.us/api/UploadItem' + '?editor_id=' + editor_id + '&key=' + key + '&filename=' + basename
	file_arg = 'file=@'+filename
	p = subprocess.Popen(['curl', '-s', '-S', '-F', file_arg, url], stdout=subprocess.PIPE, stderr=subprocess.PIPE)
	output,error = p.communicate()
	if p.returncode == 0:
		dd = parseDict(output)
		print 'Uploaded ' + basename + ' (' + dd['filesize'] + ')'
	else:
		print 'Failed ' + basename + ' : ' + error
	
def parseDict(dictStr):
	dictStr = dictStr.translate(None,'\"{}')
	items = [s for s in dictStr.split(', ') if s]
	dd = {}
	for item in items:
		key,value = item.split(': ')
		dd[key] = value
	
	return dd	

def create_gallery():
	f = urllib2.urlopen('http://min.us/api/CreateGallery')
	result = f.read()
	dd = parseDict(result)
	
	return (dd['editor_id'], dd['key'], dd['reader_id'])

def main():
	editor_id,key,reader_id = create_gallery()
	args = sys.argv[1:]
	for arg in args:
		upload_file(arg,editor_id,key)
	print 'Editor URL: http://min.us/m' + editor_id 
	print 'Viewer URL: http://min.us/m' + reader_id
	
main()