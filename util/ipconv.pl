#!/usr/bin/env perl

use strict;
use warnings;
use Encode;
use Encode::Guess;
use File::Find::Rule;

# Default output encoding.
my %files_and_encodings = ();
$files_and_encodings{ '*.cs' } = 'utf-8-with-signature-dos';
$files_and_encodings{ '*.amf' } = 'utf-8-dos';
$files_and_encodings{ '*.txt' } = 'utf-8-dos';

my $failed = 0;

while(my($file_pattern, $output_encoding) = each %files_and_encodings) {

    my %newline_table = ( "-dos", "\r\n", "-mac", "\r", "-unix", "\n" );

    $output_encoding =~ /(.*?)(-with-signature)?(-dos|-mac|-unix)?$/;
    my $encoder = find_encoding($1);
    die "Unknown encoding: $1\n" unless ref $encoder;
    my $with_signature = $2 ? 1 : 0;
    my $newline = $3 ? $newline_table{$3} : undef;

    my $canonical_encoding = $encoder->name;
    $with_signature = $with_signature && ($canonical_encoding =~ /^utf-?8/);

    use Cwd qw();
    my $path = Cwd::cwd();
    print "$path\n";

    my @files = File::Find::Rule->file()
	->name ( $file_pattern )
	->in ($path);

    foreach my $file (@files) {
	print "Processing $file\n";
	my $fh;
	unless (open $fh, "<", $file) {
	    warn "Opening $file: $!\n";
	    $failed++;
	    next;
	}
	binmode $fh;
	read $fh, my $data, (-s $file);
	close $fh;
	next unless length($data) > 0;
	my $utf8 = decode("Guess", $data);
	$utf8 =~ s/\x{feff}//g;
	$utf8 = "\x{feff}".$utf8 if $with_signature;
	if (defined $newline) {
	    my @lines = split /\r\n?|\n/, $utf8;
	    map { $_ .= $newline; } @lines;
	    $utf8 = join "", @lines;
	}
	$data = $encoder->encode($utf8);
# no backups
#    unless (rename $file, "$file$backup") {
#	warn "Renaming to $file$backup: $!\n";
#	$failed++;
#	next;
#    }
	unless (open $fh, ">", $file) {
	    warn "Opening $file: $!\n";
	    $failed++;
	    next;
	}
	binmode $fh;
	print $fh $data;
	close $fh;
    }
}

exit $failed > 0 ? 1 : 0;
