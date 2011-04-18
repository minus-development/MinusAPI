use LWP;
 
my $browser = LWP::UserAgent->new;
  
#Note that the editor_id has the leading m removed
my $editor_id = "eSJGrgpEiE9O";
my $filename = "test.txt";
# my $basename = basename($filename);
my $basename = $filename;
my $length = -s $filename;
   
my $url = "http://204.236.229.205/api/UploadItem?". "editor_id=" . $editor_id . "&key=OK&filename=" . $basename;

my $ua = LWP::UserAgent->new;
$HTTP::Request::Common::DYNAMIC_FILE_UPLOAD = 1;

use HTTP::Request::Common;
my $req = POST(
    $url,
    Content_Type => 'multipart/form-data',
    Content      => [ file => [$filename] ],
);

my $res = $ua->request($req);

print $res->status_line, "\n";
if ($res->status_line == 200) {
	print $res->content, "\n";
}