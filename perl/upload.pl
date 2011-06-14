#!/usr/bin/env perl

use strict;
use warnings;
use JSON;
use HTTP::Request;
use HTTP::Request::Common;
use HTTP::Cookies;
use LWP::UserAgent;
use File::Basename;

my $user = "fill_here";    #Username
my $pass = "fill_here";    #Password

sub usage {
print " -u <editor id> <filename>: upload item\n -i <id>: get item by id\n -g: list galleries\n";
}

@ARGV || die &usage ;

my $browser = LWP::UserAgent->new;
$browser->timeout(15);

my $cookie_jar = HTTP::Cookies->new(
  file => 'cookies.txt',
  autosave => 1,
);
$cookie_jar->save('cookies.txt');

$browser->cookie_jar($cookie_jar);


#SignIn
my $req_login = HTTP::Request->new(POST => "http://min.us/api/SignIn");
$req_login->content_type("application/x-www-form-urlencoded");
$req_login->content("username=" . $user . "&password1=" . $pass);


sub GetItem {

$browser->request($req_login)->as_string; #login

chomp(my $id = $ARGV[1] );

my $req_getitem = HTTP::Request->new(GET=> "http://min.us/api/GetItems/m$id");	#adding an m here
$req_getitem->content_type("application/x-www-form-urlencoded");

if ($browser->request($req_getitem)->is_success) {

    my $json = $browser->request($req_getitem)->content;
    my $json_coder = JSON::XS->new->utf8;
    my %json_perl = %{ $json_coder->decode($json) };

    foreach my $key ( sort keys %json_perl )  {
        print "$key\n";
		if (ref ($json_perl{$key}) eq "ARRAY" ) {
			foreach ( @{ $json_perl{$key} } ) {		#dereferencing array
				print $_ . "\n";
			}
		} else {
			print $json_perl{$key}."\n";
		}

    }


} else {
print "Request not successful\n";
print $browser->request($req_getitem)->status_line, "\n";
}

}

sub MyGalleries {

$browser->request($req_login)->as_string; #login

my $req_mygalleries = HTTP::Request->new(GET=> "http://min.us/api/MyGalleries.json");
$req_mygalleries->content_type("text/html");

if ($browser->request($req_mygalleries)->is_success) {

    my $json = $browser->request($req_mygalleries)->content;
    my $json_coder = JSON::XS->new->utf8->pretty->allow_nonref;
    my %json_perl = %{ $json_coder->decode($json) };

    foreach my $key ( sort keys %json_perl )  {
        print "$key\n\n";

        if (ref ($json_perl{$key}) eq "ARRAY" ) {
			foreach ( @{ $json_perl{$key} } ) {		#dereferencing array of hashes
                print "\n";
				foreach ( %{ $_ } ) {				#dereferencing hashes
					print $_ . "\n" ;
				} 
            }
		} else {
            print $json_perl{$key}."\n";
        }

    }


} else {
print "Request not successful\n";
print $browser->request($req_mygalleries)->status_line, "\n";
}

}


sub UploadItem {

#Note that the editor_id has the leading m removed

chomp( my $editor_id = $ARGV[1] );
chomp( my $filename = $ARGV[2] );

unless (-e $filename) { die "$filename: $!"; }

my $basename = basename($filename);
my $length = -s $filename;
   
my $url = "http://204.236.229.205/api/UploadItem?". "editor_id=" . $editor_id . "&key=OK&filename=" . $basename;

$HTTP::Request::Common::DYNAMIC_FILE_UPLOAD = 1;

my $req_upitem = POST(
    $url,
    Content_Type => 'multipart/form-data',
    Content      => [ file => [$filename] ],
);

my $res = $browser->request($req_upitem);

if ($res->status_line =~ m/^200/ ) {
	print "Successfully uploaded\n\n";
	my $json = $res->content;
    my $json_coder = JSON::XS->new->utf8->pretty->allow_nonref;
    my %json_perl = %{ $json_coder->decode($json) };

        foreach my $key ( sort keys %json_perl )  {
            print "$key\n";
        print $json_perl{$key}."\n";
        }

} else { 
   print "Request not successful\n";
   print $res->status_line, "\n"; 
}

}


if ($ARGV[0] eq "-u") {
	if (@ARGV == 3) { &UploadItem } else { &usage } 
} elsif ($ARGV[0] eq "-i") {
    if (@ARGV == 2) { &GetItem } else { &usage }
} elsif ($ARGV[0] eq "-g") {
    &MyGalleries
} else {
    &usage
}


exit;
