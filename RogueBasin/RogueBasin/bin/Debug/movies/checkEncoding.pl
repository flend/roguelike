#!/usr/bin/perl -w

use strict;
use Encode::Guess;

if(@ARGV != 1)
{
    print "Usage: perl $0 <file to analyze>\n";
    exit;
}

my @files = glob($ARGV[0]);

foreach my $file (@files)
{
    open(FILE,$file);
    binmode(FILE);
    if(read(FILE,my $filestart, 500)) {
	my $enc = guess_encoding($filestart);
	if(ref($enc)) {
	    print "$file:\t",$enc->name,"\n";
	}
	else {
	    print "Encoding of file $file can't be guessed\n";
	}
    }
    else {
	print "Cannot read from file $file\n";
    }
}

close(FILE);
